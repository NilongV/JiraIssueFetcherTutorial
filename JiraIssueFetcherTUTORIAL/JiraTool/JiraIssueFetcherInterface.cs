using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JiraIssueTool.JiraTool
{
    public enum ProjectFetchingOptions
    {
        KEYS = 0,
        NAMES = 1,
    }

    public enum IssueFetchingOptions
    {
        BASIC = 0,
        ADVANCED = 1,
    }


    public class JiraIssueData
    {
        public JiraIssueData(string summary, string description, string issueType, string priority = null, string assignee = null, string status = null)
        {
            Summary = summary;
            Description = description;
            IssueType = issueType;
            Priority = priority;
            Assignee = assignee;
            Status = status;
        }
        

        public string IssueType { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string Assignee { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Interface can fetch issues, projects and issue keys from Jira
    /// </summary>
    
    public interface IJiraIssueFetcher
    {
        
        Task<List<string>> GetAllProjectsAsync(ProjectFetchingOptions options);
        Task<List<string>> GetIssueKeysAsync(string projectKey);
        Task<JiraIssueData> FetchIssueDataAsync(string issueKey, IssueFetchingOptions options);
    }

    
    





    
    
    

    
    


}
