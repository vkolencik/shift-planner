// Class to demonstrate the use of Sheets list values API

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

static class Program
{
    /* Global instance of the scopes required by this quickstart.
     If modifying these scopes, delete your previously saved token.json/ folder. */
    static string[] Scopes = {SheetsService.Scope.SpreadsheetsReadonly};
    static string ApplicationName = "Google Sheets API .NET Quickstart";

    static void Main(string[] args)
    {
        try
        {
            var credential = GoogleCredential.FromFile("credentials/krtekpoc-credentials.json").CreateScoped(Scopes);            // Load client secrets.
            // using (var stream =
            //        new FileStream("credentials/krtekpoc-credentials.json", FileMode.Open, FileAccess.Read))
            // {
            //     /* The file token.json stors the user's access and refresh tokens, and is created
            //      automatically when the authorization flow completes for the first time. */
            //     string credPath = "token.json";
            //     credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //         GoogleClientSecrets.FromStream(stream).Secrets,
            //         Scopes,
            //         "user",
            //         CancellationToken.None,
            //         new FileDataStore(credPath, true)).Result;
            //     Console.WriteLine("Credential file saved to: " + credPath);
            // }

            // Create Google Sheets API service.
            var service = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                });

            // Define request parameters.
            String spreadsheetId = "1iLE1TDFKrUbnzTVX8PmjP5AvvccD2dsFjomD5c5yiJs";
            String range = "Kose!A3:H38";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values == null || values.Count == 0)
            {
                Console.WriteLine("No data found.");
                return;
            }

            var competenceGroups = new[]
            {
                new CompetenceGroup(
                    new Workplace[]
                    {
                        new Workplace("A1"),
                        new Workplace("A2"),
                        new Workplace("A3"),
                    }),
                new CompetenceGroup(
                    new Workplace[]
                    {
                        new Workplace("RES"),
                        new Workplace("KPR"),
                        new Workplace("URG"),
                    })
            };
            var workplaces = competenceGroups.SelectMany(cg => cg.Workplaces).ToArray();
            var employees = new List<Employee>();
            
            foreach (var row in values)
            {
                var name = (string) row[0];
                var competenceLevel = (CompetenceLevel) (int.Parse((string) row[1]));

                var capabilities = workplaces
                    .Select(
                        (w, i) =>
                            string.IsNullOrWhiteSpace((string) row[i + 2])
                                ? new WorkplaceCapability.Red(w) as WorkplaceCapability
                                : new WorkplaceCapability.Green(w, uint.Parse((string) row[i + 2])))
                    .ToArray();

                employees.Add(new Employee(name, competenceLevel, new Capabilities(capabilities)));
                // Print columns A and E, which correspond to indices 0 and 4.
                Console.WriteLine(string.Join(", ", row));
            }
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
