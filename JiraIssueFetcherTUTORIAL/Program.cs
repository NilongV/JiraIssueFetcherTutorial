
// Muista importata JiraTool/JiraFetcheri.
using JiraIssueTool.JiraTool;

// Esimerkki muuttujat Jira-sivuston osoitteelle, sähköpostille ja API-tokenille.
string jiraSiteUrl = "jira-entteri.atlassian.net";
string email;
string apiToken;

Console.Write("Syötä sähköpostiosoite: ");
email = Console.ReadLine().ToLower();

// API-tokenin tulisi mieluiten kysyä käyttäjältä syötteenä, eikä hardcodeta koodiin.
// Sama koskee myös sähköpostia ja Jira-sivuston osoitetta - riippuen sovelluksen käyttötarkoituksesta.
Console.Write("Syötä API-token: ");
apiToken = Console.ReadLine();

//-------------------------------------------------------------------------------------------\\


// ESIMERKKI KUINKA JIRAISSUEFETCHERIÄ KÄYTETÄÄN \\


// Ensimmäiseksi täytyy luoda JiraIssueFetcher-olio.
// Tarvitset Jira-sivuston osoitteen, sähköpostiosoitteen ja API-tokenin.
IJiraIssueFetcher fetcher = new JiraIssueFetcher(jiraSiteUrl, email, apiToken);

// JiraIssueFetcher sisältää 3 metodia, joita voidaan käyttää.

// Aloitetaan FetchIssueDataAsync-metodista.
// Tämä metodi hakee tietoja annetusta issuesta REST API:n kautta.
// Tarvitset issuen avaimen ja issuen hakutavan (IssueFetchingOptions).
// Jos haluat hakea perustiedot, käytä IssueFetchingOptions.BASIC. Jos haluat hakea enemmän tietoja, käytä IssueFetchingOptions.ADVANCED.
// Huomaa, että tällä hetkellä advanced-palauttaa vain perustiedot sekä issuen prioriteetin, statuksen ja assignerin.
JiraIssueData data = await fetcher.FetchIssueDataAsync("JIT-1", IssueFetchingOptions.BASIC);

// Jos et halua mennä Jiraan hakemaan issue-avainta, voit käyttää metodia GetIssueKeysAsync.
//Tämä metodi palauttaa listan kaikista projektin issue avaimista. Tarvitset projektin avaimen, joka yleensä on issue-avain ilman viivaa ja numeroa.
List<string> listOfIssueKeys = await fetcher.GetIssueKeysAsync("JIT");

// Seuraava metodi tuo sinulle listana kaikki projektit, joihin sinulla on pääsy.
// Metodi tarvitsee vain sen, haluatko projektien avaimet vai niiden nimet.
List<string> listOfProjects = await fetcher.GetAllProjectsAsync(ProjectFetchingOptions.KEYS);

// Esimerkki printtauksista issue datan, issue-avainten ja projektien osalta.
Console.WriteLine($"ISSUEN TIEDOT: Summary: {data.Summary}, Kuvaus: {data.Description}, Issuen tyyppi: {data.IssueType}");

foreach (string key in listOfIssueKeys)
{
    Console.WriteLine($"ISSUEN AVAIN: {key}:");
}

foreach (string pro in listOfProjects)
{
    Console.WriteLine($"PROJEKTIN AVAIN: {pro}");
}