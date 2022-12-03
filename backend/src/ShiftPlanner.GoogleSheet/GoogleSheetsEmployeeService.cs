using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ShiftPlanner.Domain;

namespace ShiftPlanner.GoogleSheet;

public class GoogleSheetsEmployeeService : IEmployeeService
{
    private static readonly string[] Scopes = {SheetsService.Scope.Spreadsheets};
    private static readonly string ApplicationName = "Krtek";
    private static readonly string SpreadsheetId = "1iLE1TDFKrUbnzTVX8PmjP5AvvccD2dsFjomD5c5yiJs";

    public async Task<IReadOnlyCollection<Employee>> GetEmployees(IList<Workplace> workplaces)
    {
        using var service = GetGoogleSheetsService();

        // Define request parameters.
        String range = "Kose!A3:J100";
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(SpreadsheetId, range);

        ValueRange response = await request.ExecuteAsync();

        IList<IList<object>> values = response.Values;

        if (values == null || values.Count == 0)
        {
            throw new Exception("No data found.");
        }

        var employees = new List<Employee>();

        foreach (var row in values)
        {
            if (row.Count < 4)
            {
                continue;
            }
            
            var name = (string) row[1];
            if (string.IsNullOrEmpty(name))
            {
                break;
            }

            var competenceLevelString = (string) row[3];
            if (string.IsNullOrWhiteSpace(competenceLevelString))
            {
                continue;
            }

            var disableFlagString = (string) row[0];
            var disabled = disableFlagString == "X";
            var email = (string) row[2];
            var competenceLevel = (CompetenceLevel) int.Parse(competenceLevelString);

            var capabilitiesArray = row.Skip(4).ToArray();
            var paddedCapabilitiesArray = capabilitiesArray.Concat(Enumerable.Repeat("", workplaces.Count - capabilitiesArray.Length)).ToArray();
            var capabilities = paddedCapabilitiesArray
                .Select(
                    (cellValue, i) =>
                        string.IsNullOrWhiteSpace((string) cellValue)
                            ? new WorkplaceCapability.Red(workplaces[i]) as WorkplaceCapability
                            : new WorkplaceCapability.Green(workplaces[i], uint.Parse((string) cellValue)))
                .ToArray();

            employees.Add(new Employee(name, competenceLevel, new Capabilities(capabilities), Workload.Full, disabled));
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
