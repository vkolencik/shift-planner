using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Services.Services.Google;

internal class GoogleSheetsEmployeeInfoService : GoogleSheetsService, IEmployeeInfoService
{
    private static class RowIndices
    {
        public const int DisabledFlag = 0;
        public const int Name = 1;
        public const int Email = 2;
        public const int CompetenceLevel = 3;
        public const int WorkloadMin = 4;
        public const int WorkloadMax = 5;
        public const int Capabilities = 6;
    }

    public GoogleSheetsEmployeeInfoService(IAwsSecretsManagerClient secretsManagerClient) : base(secretsManagerClient)
    {
    }

    public async Task<IReadOnlyCollection<EmployeeInfo>> GetEmployees(IList<Workplace> workplaces)
    {
        using var service = await GetGoogleSheetsService();

        // Define request parameters.
        String range = "Kose!A3:L100";
        SpreadsheetsResource.ValuesResource.GetRequest request =
            service.Spreadsheets.Values.Get(SpreadsheetId, range);

        ValueRange response = await request.ExecuteAsync();

        IList<IList<object>> values = response.Values;

        if (values == null || values.Count == 0)
        {
            throw new Exception("No data found.");
        }

        var employees = new List<EmployeeInfo>();

        foreach (var row in values)
        {
            if (row.Count < 4)
            {
                continue;
            }
            
            var name = (string) row[RowIndices.Name];
            if (string.IsNullOrEmpty(name))
            {
                break;
            }

            var competenceLevelString = (string) row[RowIndices.CompetenceLevel];
            if (string.IsNullOrWhiteSpace(competenceLevelString))
            {
                continue;
            }

            // var workloadString = (string) row[RowIndices.Workload];
            // if (string.IsNullOrWhiteSpace(workloadString))
            // {
            //     continue;
            // }
            var workload = new Workload(0, 0);

            var statusString = (string) row[RowIndices.DisabledFlag];
            var employeeStatus = ParseEmployeeStatus(statusString);
            var email = (string) row[RowIndices.Email];
            var competenceLevel = (CompetenceLevel) int.Parse(competenceLevelString);

            var capabilitiesArray = row.Skip(RowIndices.Capabilities).ToArray();
            var paddedCapabilitiesArray = capabilitiesArray.Concat(Enumerable.Repeat("", workplaces.Count - capabilitiesArray.Length)).ToArray();
            var capabilities = paddedCapabilitiesArray
                .Select(
                    (cellValue, i) =>
                        string.IsNullOrWhiteSpace((string) cellValue)
                            ? new WorkplaceCapability.Red(workplaces[i]) as WorkplaceCapability
                            : new WorkplaceCapability.Green(workplaces[i], uint.Parse((string) cellValue)))
                .ToArray();

            employees.Add(new EmployeeInfo(name, email, competenceLevel, new Capabilities(capabilities), workload, employeeStatus));
        }

        return employees;
    }

    private static EmployeeStatus ParseEmployeeStatus(string statusString)
    {
        return statusString switch
        {
            null or "" => EmployeeStatus.Active,
            "X" => EmployeeStatus.Disabled,
            "N" => EmployeeStatus.Inactive,
            _ => throw new ArgumentOutOfRangeException(nameof(statusString), statusString)
        };
    }
}
