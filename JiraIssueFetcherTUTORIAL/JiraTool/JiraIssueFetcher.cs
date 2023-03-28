
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace JiraIssueTool.JiraTool
{
    public class JiraIssueFetcher : IJiraIssueFetcher
    {
        string site;
        string domain;
        bool Access;
        string authorization;
        
        /// <summary>
        /// JiraIssueFetcher contructor needs your atlassian site and authorization.
        /// Site as "your-domain.atlassian.net" and email as "john.doe@work.com". Constructor also needs an api token.
        /// Check here how to make one: https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/ 
        /// </summary>
        /// <param name="atlassianSite"></param>
        /// <param name="authorization"></param>
        public JiraIssueFetcher(string atlassianDomain, string email, string api_token)
        {
            //this constructor takes the url, so we can access the issue information.
            this.site = atlassianDomain;
            this.authorization = $"{email}:{api_token}";
            if (site != null)
            {
                if (IsAtlassianDomain())
                {
                    TryLoggingIn();
                    Access = true;
                }
            }
        }

        /// <summary>
        /// Takes jira issue key and data fetching option as parameters. the options for data fetching are basic or advanced.
        /// basic retrieves Issue's description, summary and type and advanced option retrieves basic info 
        /// and also issue's priority EX. medium, it's assignee and finally it's status EX: TO DO
        /// </summary>
        /// <param name="issueKey"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<JiraIssueData> FetchIssueDataAsync(string issueKey, IssueFetchingOptions options)
        {    
            switch (options)
            {
                case IssueFetchingOptions.BASIC:
                    return await GetBasicIssueDataAsync(issueKey);
                case IssueFetchingOptions.ADVANCED:
                    var basicData = await GetBasicIssueDataAsync(issueKey);
                    var priority = await GetIssueInfo(issueKey, "priority");
                    var assignee = await GetIssueInfo(issueKey, "assignee");
                    var status = await GetIssueInfo(issueKey, "status");
                    return new JiraIssueData(basicData.Summary, basicData.Description, basicData.IssueType, priority, assignee, status);
                default:
                    return null;
            }
        }

        async Task<JiraIssueData> GetBasicIssueDataAsync(string issueKey)
        {
            var summary = await GetIssueInfo(issueKey, "summary");
            var description = await GetIssueInfo(issueKey, "description");
            var issueType = await GetIssueInfo(issueKey, "type");
            return new JiraIssueData(summary, description, issueType);
        }

        /// <summary>
        /// Method retrieves every project from user's atlassian account and returns names of the keys of the projects
        /// Needs option as parameter.
        /// </summary>
        /// <returns>List of project names or project keys that user has access of</returns>
        public async Task<List<string>> GetAllProjectsAsync(ProjectFetchingOptions options)
        {
            if (Access)
            {
                try
                {

                    List<string> projectKeys = new List<string>();
                    List<string> projectNames = new List<string>();
                    
                    using var httpClient = new HttpClient();
                    //Adding auth header for the client so we can log in and get authorized.
                    //We would use RestSharp library but basic auth doesn't work with that library at the moment/also Atlassian has disabled it.
                    //We are using basic because oAuth tokens are *little bit* complicated, but oAuth should work better and oAuth is more secure than basic, atleast atlassian says so..\\
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Encoder());
                    HttpResponseMessage response = await httpClient.GetAsync(site + "/rest/api/3/project");

                    //Here we read the response as a string                   
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var dict = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseContent);
                    foreach (var item in dict)
                    {
                        foreach (KeyValuePair<string, object> entry in item)
                        {
                            string key = entry.Key;
                            string value = entry.Value.ToString();
                            if (key == "key")
                            {
                                projectKeys.Add(value);
                            }
                            if (key == "name")
                            {
                                projectNames.Add(value);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    await Task.Delay(1000);

                    switch (options)
                    {
                        case ProjectFetchingOptions.KEYS:
                            return projectKeys;
                        case ProjectFetchingOptions.NAMES:
                            return projectNames;
                    }


                }



                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            await Task.Delay(1000);

            // Return a list of numbers
            return null;

        }


        /// <summary>
        /// Retrieves all issue keys from specific project. *Needs jira project key as parameter*.
        /// INFO: Project key usually is displayed in an issue key, for example we have issue "JIT-1" the project key would then be "JIT"
        /// </summary>
        /// <param name="projectKey"></param>
        /// <returns>List of project's issue keys</returns>
        public async Task<List<string>> GetIssueKeysAsync(string projectKey)
        {
            List<string> issueKeys = new List<string>();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Encoder());

            HttpResponseMessage response = await httpClient.GetAsync($"{site}/rest/api/3/search?jql=project={projectKey}");
            string responseContent = await response.Content.ReadAsStringAsync();
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

            // Get the "issues" property from the JSON response and cast it to a JArray
            JArray issuesArray = (JArray)dict["issues"];

            // Convert the JArray to a List<object>
            List<object> issues = issuesArray.Select(x => (object)x).ToList();
                    
            
                    
            foreach (object issue in issues)
            {
                // Cast the issue to a JObject
                JObject issueData = (JObject)issue;

                // Extract the key of the issue as a string
                string key = issueData["key"].ToString();
                if(key != null)
                    issueKeys.Add(key);
                if(key == null)
                    Console.WriteLine("Issue key not found");
                
                    
            }
            await Task.Delay(1000);

            if (issues.Count > 0)
                return issueKeys;
            else
                return null;
                    
        
        }



        /// <summary>
        /// Method retrieves wanted info from given jira issue
        /// </summary>
        /// <param name="issueKey"></param>
        /// <param name="wantedInfo"></param>
        /// <returns>string of wanted info</returns>
        private async Task<string> GetIssueInfo(string issueKey, string wantedInfo)
        {
            string info = "EMPTY";
            
            // Create an instance of HttpClient to make the request to the JIRA REST API
            HttpClient client = new HttpClient();
            // Add authorization header to the request, using the Basic authentication scheme
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Encoder());
            // Send the request to the JIRA API endpoint that corresponds to the specified issue key
            var response = await client.GetAsync($"{site}/rest/api/latest/issue/{issueKey}");
            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();
            // Parse the response content as a JObject
            JObject obj = JObject.Parse(responseContent);
            // Declare a JToken variable to hold the specific issue information we want to retrieve
            JToken issueToken = null;

            // Create a new dictionary to hold the issue information we will retrieve
            Dictionary<string, string> infoDict = new Dictionary<string, string>();

            // Check which type of information we want to retrieve, and call the corresponding method
            switch (wantedInfo)
            {
                case "summary":
                    issueToken = obj.SelectToken("fields.summary"); info = "issue summary";
                    break;
                case "description":
                    issueToken = obj.SelectToken("fields.description"); info = "issue description";
                    break;
                case "type":
                    issueToken = obj.SelectToken("fields.issuetype"); info = "issuetype";
                    break;
                case "status":
                    issueToken = obj.SelectToken("fields.status"); info = "issue status";
                    break;
                case "priority":
                    issueToken = obj.SelectToken("fields.priority"); info = "issue priority";
                    break;
                case "assignee":
                    issueToken = obj.SelectToken("fields.assignee"); info = "issue assignee";
                    break;
            }


            if (CheckIfNull(issueToken, info) == true)
            {
                // If the selected issue information is null, display a message box indicating that it won't be
                if (issueToken.Type == JTokenType.Null)
                    Console.WriteLine($"{info} value is null, so it will not be added");
                else
                {
                    if (wantedInfo == "type"  || wantedInfo == "priority" || wantedInfo == "status")
                    {
                        JObject issueField = (JObject)issueToken;
                        issueToken = (string)issueField["name"];
                    }
                    Console.WriteLine(issueToken.ToString());
                    return issueToken.ToString();

                }
                          
            }
            return "null error";

            bool CheckIfNull(JToken token, string field)
            {

                switch (token)
                {
                    case null:
                        Console.WriteLine($"Key {field} does not exist in the JSON object.");
                        break;
                    default:
                        return true;
                        
                }
                return false;
            }

            
            

         
            

        }
        private bool IsAtlassianDomain()
        {
            // Parse the URL to get the domain name
            if (!site.Contains("https://") || !site.Contains("http://"))
            {
                this.site = "https://" + site;
            }
            Uri uri = new Uri(site);
            string domainName = uri.Host;

            // Compare the domain name to the Jira server domain name
            bool isAtlassian = domainName.EndsWith(".atlassian.net");


            if (isAtlassian)
            {
                return true;
            }
            Console.WriteLine("Invalid Atlassian url!");
            return false;
        }
        
            


        

        private async void TryLoggingIn()
        {
            try
            {
                if (authorization != null & site != null)
                {

                    using var httpClient = new HttpClient();

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Encoder());

                    HttpResponseMessage response = await httpClient.GetAsync($"https://nippetest.atlassian.net");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        private string Encoder()
        {
            // Encode the concatenated value using Base64
            if (authorization != null)
            {
                byte[] encodedBytes = Encoding.UTF8.GetBytes(authorization);
                return Convert.ToBase64String(encodedBytes);

            }
            return "not authorized!";

        }

    }

}