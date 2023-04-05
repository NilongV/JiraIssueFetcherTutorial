// Muista importata!
using JiraFetcher;

// Esimerkki muuttujat sivustolle, emailille ja api tokenille.
string atlassianSite = "jira-entteri.atlassian.net";

Console.Write("Syötä email: ");
string email = Console.ReadLine().ToLower();

// Api tokeni kannattaa mieluiten kysyä jonkinlaisena inputtina tms, eikä hardcodata.
// Sama emailin ja sivuston kanssa, riippuu myös sovelluksen käyttötarkoituksesta.
Console.Write("Syötä api token: ");
string api_token = Console.ReadLine();



// ESIMERKKI MITEN JIRAISSUEFETCHERIÄ KÄYTETÄÄN\\

// Ensin täytyy luoda fetcheri.
// Tarvitset Atlassian sivuston, sähköpostin, api tokenin, että käyttö on mahdollista. 
IJiraIssueFetcher fetcher = new JiraIssueFetcher(atlassianSite, email, api_token);


// Fetcher sisältää 3 metodia, jota voidaan käyttää.


// Aloitetaan GetAllProjectsAsyncista.

//Seuraava metodi tuo sinulle listana kaikki projektit, joihon sinulla on pääsy.
//Metodi tarvitsee vain sen, että haluatko projektien avaimet vai niiden nimet.
List<string> listOfProjects = await fetcher.GetAllProjectsAsync(ProjectFetchingOptions.KEYS);


//Valitaan jokin satunnainen projekti avain.
//Jos jostain syystä projekti avain on virheellinen, kokeile ajaa ohjelma uusiksi.
Random random = new Random();
string randomProject = listOfProjects[random.Next(listOfProjects.Count)];

/* Voi olla, että ei jaksa mennä jiraan ja etsiä issue avainta, no ei hätää, koska siihenkin on metodi.
GetIssueKeysAsync metodilla saat kaikki projektin issue avaimet listana, tarvitset toki projekti avaimen,
joka yleensä on issue avain ilman viivaa ja numeroa. */
List<string> listOfIssueKeys = await fetcher.GetIssueKeysAsync(randomProject);

//Tehdään sama avaimille, eli otetaan listasta jokin satunnainen issue projektista
//Jos jokin virhe tulee, niin käynnistä ohjelma uusiksi.
Random random2 = new Random();
string randomKey = listOfIssueKeys[random.Next(listOfIssueKeys.Count)];


// Metodi yksinkertaisesti, hakee tietoa annetusta issuesta, rest apia käyttäen.
// Tarviset issuen avaimen, issuen hakemis tavan, eli haentaanko perustiedot vai haetaanko hieman enemmän tietoa
// Tähän pitää käyttää IssueFetchingOptions.ADVANCED.
// Tällä hetkellä kylläkin advanced palauttaa vain basic tiedot sekä issue prioriteetin, sen statuksen ja sen assignerin
JiraIssueData data = await fetcher.FetchIssueDataAsync(randomKey, IssueFetchingOptions.BASIC);
    





// Esimerkki printtaus issue datasta
Console.WriteLine($"ISSUEN DATAA: Summary: {data.Summary}, Desc: {data.Description}, Issue tyyppi: {data.IssueType}");

// Esimerkki printtaus issue avaimista
foreach (string key in listOfIssueKeys)
{
    Console.WriteLine($"ISSUE AVAIN: {key}:");
}

// Esimerkki printtaus projekteista (tässä tapauksessa avaimista)
foreach (string pro in listOfProjects)
{
    Console.WriteLine($"PROJEKTI AVAIN: {pro}");

}
