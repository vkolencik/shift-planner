using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ShiftPlanner.Domain;
using InvalidOperationException = System.InvalidOperationException;

namespace ShiftPlanner.Services.Services.Google;

internal class GoogleSheetsEmployeeInfoService : GoogleSheetsService, IEmployeeInfoService
{
    private static class RowIndices
    {
        public const int StatusFlag = 0;
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
            var name = (string) row[RowIndices.Name];
            if (string.IsNullOrEmpty(name))
            {
                break;
            }

            var statusString = (string?) row[RowIndices.StatusFlag];
            var employeeStatus = ParseEmployeeStatus(statusString);

            var email = (string) row[RowIndices.Email];
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException($"Missing email for employee name {name}");
            }

            if (row.Count <= RowIndices.Capabilities)
            {
                // Disabled employee without full set of data
                if (employeeStatus == EmployeeStatus.Active)
                {
                    throw new InvalidOperationException($"Active employee {name} doesn't have full data set");
                }
                
                employees.Add(CreateEmptyEmployee(employeeStatus, name, email));
                continue;
            }

            var competenceLevel = ParseCompetenceLevel((string) row[RowIndices.CompetenceLevel]);

            var workload = ParseWorkload((string?) row[RowIndices.WorkloadMin], (string?) row[RowIndices.WorkloadMax]);

            var capabilitiesArray = row.Skip(RowIndices.Capabilities).Cast<string?>().ToArray();
            Capabilities capabilities = ParseCapabilities(capabilitiesArray, workplaces);

            employees.Add(new EmployeeInfo(name, email, competenceLevel, capabilities, workload, employeeStatus));
        }

        return employees;
    }

    private Capabilities ParseCapabilities(string?[] capabilitiesArray, IList<Workplace> workplaces)
    {
        if (capabilitiesArray.All(string.IsNullOrWhiteSpace))
        {
            return Capabilities.Empty;
        }
        
        var paddedCapabilitiesArray = capabilitiesArray.Concat(Enumerable.Repeat("", workplaces.Count - capabilitiesArray.Length)).ToArray();
        var capabilities = paddedCapabilitiesArray
            .Select(
                (cellValue, i) =>
                    string.IsNullOrWhiteSpace(cellValue)
                        ? new WorkplaceCapability.Red(workplaces[i]) as WorkplaceCapability
                        : new WorkplaceCapability.Green(workplaces[i], uint.Parse((string) cellValue)))
            .ToArray();
        
        return new Capabilities(capabilities);
    }

    private static Workload ParseWorkload(string? min, string? max)
        => string.IsNullOrWhiteSpace(min) || string.IsNullOrWhiteSpace(max) ? Workload.None : new Workload(uint.Parse(min), uint.Parse(max));

    private static CompetenceLevel ParseCompetenceLevel(string? competenceLevelString)
        => string.IsNullOrWhiteSpace(competenceLevelString) ? CompetenceLevel.None : (CompetenceLevel) int.Parse(competenceLevelString);

    private EmployeeInfo CreateEmptyEmployee(EmployeeStatus status, string name, string email)
    {
        if (status == EmployeeStatus.Active)
        {
            throw new InvalidOperationException($"Cannot create empty active employee (name {name})");
        }

        return new EmployeeInfo(name, email, CompetenceLevel.None, Capabilities.Empty, Workload.None, status);
    }

    private static EmployeeStatus ParseEmployeeStatus(string? statusString)
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
