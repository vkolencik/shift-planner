// Class to demonstrate the use of Sheets list values API

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;

namespace ShiftPlanner.GoogleSheet;

static class Program
{
    /* Global instance of the scopes required by this quickstart.
     If modifying these scopes, delete your previously saved token.json/ folder. */
    static string[] Scopes = {SheetsService.Scope.Spreadsheets};
    static string ApplicationName = "Google Sheets API .NET Quickstart";

    static void Main(string[] args)
    {
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

        var employees = ReadInputFromGoogleSheet(workplaces);
        var schedulingParams = new MonthScheduleParameters(
            new ScheduledMonth(2022, 6),
            employees.Select(e => new EmployeeMonth(e)).ToArray(),
            competenceGroups);

        var scheduler = new ShiftScheduler();

        var result = scheduler.ComputeSchedule(schedulingParams);

        if (result is SolutionNotFound)
        {
            Console.Error.WriteLine($"Solution not found");
            Environment.ExitCode = 1;
            return;
        }

        var successResult = (SuccessfulSchedulingResult) result;
        Console.Out.WriteLine($"Computed schedule with flexibility {successResult.Flexibility}");

        Console.Out.WriteLine(successResult.Schedule.Shifts.First());
    }

    private static IReadOnlyCollection<Employee> ReadInputFromGoogleSheet(Workplace[] workplaces)
    {
        var credential = GoogleCredential.FromFile("credentials/krtekpoc-credentials.json").CreateScoped(Scopes);

        // Create Google Sheets API service.
        var service = new SheetsService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

        // Define request parameters.
        String spreadsheetId = "1iLE1TDFKrUbnzTVX8PmjP5AvvccD2dsFjomD5c5yiJs";
        String range = "Kose!A3:I38";
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(spreadsheetId, range);

        ValueRange response = request.Execute();
        IList<IList<Object>> values = response.Values;
        if (values == null || values.Count == 0)
        {
            throw new Exception("No data found.");
        }

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
        }

        return employees;
    }
}
