// Class to demonstrate the use of Sheets list values API

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine;

namespace ShiftPlanner.GoogleSheet;

[Obsolete]
static class Program
{
    private const string SpreadsheetId = "1iLE1TDFKrUbnzTVX8PmjP5AvvccD2dsFjomD5c5yiJs";

    static string[] Scopes = {SheetsService.Scope.Spreadsheets};
    static string ApplicationName = "Krtek";

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
        var scheduledMonth = new ScheduledMonth(2022, 6);
        var schedulingParams = new MonthScheduleParameters(
            scheduledMonth,
            employees.Select(e => new EmployeeMonth(e)).ToArray(),
            competenceGroups,
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ShiftAssignment>(),
            Array.Empty<ISet<Workplace>>());

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

        // WriteScheduleToGoogleSheet(scheduledMonth, workplaces, successResult.Schedule);
    }

    private static void WriteScheduleToGoogleSheet(ScheduledMonth month, IReadOnlyCollection<Workplace> workplaces, Schedule schedule)
    {
        var service = GetGoogleSheetsService();

        service.Spreadsheets.Values.BatchClear(
            new BatchClearValuesRequest() {Ranges = new[] {"Vysledky!A2:G40"}}, SpreadsheetId).Execute();
        service.Spreadsheets.Values.BatchClear(
            new BatchClearValuesRequest() {Ranges = new[] {"Vysledky!B1:G1"}}, SpreadsheetId).Execute();

        WriteListToSheet(service, "A2", month.Days.Select(d => d.Date.Day), "COLUMNS");
        WriteListToSheet(service, "B1", workplaces.Select(w => w.Name));

        int rowIndex = 2;
        foreach (var day in month.Days)
        {
            WriteListToSheet(service, $"B{rowIndex}", workplaces.Select(w => schedule.GetAssignedEmployee(new Shift(day, w)).Name));
            rowIndex++;
        }
    }

    private static void WriteListToSheet<T>(
        SheetsService service,
        string rangeStart,
        IEnumerable<T> values,
        string dimension = "ROWS")
    {
        service.Spreadsheets.Values.BatchUpdate(
            new BatchUpdateValuesRequest()
            {
                Data = new List<ValueRange>
                {
                    new ValueRange()
                    {
                        Range = $"Vysledky!{rangeStart}",
                        Values = new List<IList<object>> {values.Cast<object>().ToList()},
                        MajorDimension = dimension
                    },
                },
                ValueInputOption = "RAW"
            }, SpreadsheetId).Execute();
    }

    private static IReadOnlyCollection<Employee> ReadInputFromGoogleSheet(Workplace[] workplaces)
    {
        var service = GetGoogleSheetsService();

        // Define request parameters.
        String range = "Kose!B4:J49";
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(SpreadsheetId, range);

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
            if (row.Count <= 2 || string.IsNullOrEmpty((string) row[2]))
            {
                continue;
            }

            var competenceLevel = (CompetenceLevel) (int.Parse((string) row[2]));

            var capabilities = workplaces
                .Select(
                    (w, i) =>
                        row.Count <= i + 3 || string.IsNullOrWhiteSpace((string) row[i + 3])
                            ? new WorkplaceCapability.Red(w) as WorkplaceCapability
                            : new WorkplaceCapability.Green(w, uint.Parse((string) row[i + 3])))
                .ToArray();

            employees.Add(new Employee(name, competenceLevel, new Capabilities(capabilities)));
        }

        return employees;
    }

    private static SheetsService GetGoogleSheetsService()
    {
        var credential = GoogleCredential.FromFile("credentials/krtekpoc-credentials.json").CreateScoped(Scopes);

        // Create Google Sheets API service.
        var service = new SheetsService(
            new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        return service;
    }
}
