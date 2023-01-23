using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using ShiftPlanner.Domain;
using ShiftPlanner.Services.Contracts;

namespace ShiftPlanner.Services.Services.Google;

public class GoogleSheetsScheduleRepository : GoogleSheetsService, IGoogleSheetsScheduleRepository 
{
    public GoogleSheetsScheduleRepository(IAwsSecretsManagerClient secretsManagerClient) : base(secretsManagerClient)
    {
    }

    public async Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result)
    {
        var service = await GetGoogleSheetsService();

        service.Spreadsheets.Values.BatchClear(
            new BatchClearValuesRequest() {Ranges = new[] {"Vysledky!A2:G40"}}, SpreadsheetId).Execute();
        service.Spreadsheets.Values.BatchClear(
            new BatchClearValuesRequest() {Ranges = new[] {"Vysledky!B1:G1"}}, SpreadsheetId).Execute();
        
        var workplaces = Workplace.Workplaces;
        var schedule = result.PlannedSchedule;

        await WriteListToSheet(service, "A2", month.Days.Select(d => d.Date.Day), "COLUMNS");
        await WriteListToSheet(service, "B1", workplaces.Select(w => w.Name));

        int rowIndex = 2;
        foreach (var day in month.Days)
        {
            await WriteListToSheet(service, $"B{rowIndex}", workplaces.Select(w => schedule.GetAssignedEmployee(new Shift(day, w)).Name));
            rowIndex++;
        }
    }

    public Task StoreSchedule(ScheduledMonth month, Schedule schedule)
    {
        throw new NotImplementedException();
    }

    public Task<Schedule> LoadSchedule(ScheduledMonth month)
    {
        throw new NotImplementedException();
    }

    private static async Task WriteListToSheet<T>(
        SheetsService service,
        string rangeStart,
        IEnumerable<T> values,
        string dimension = "ROWS")
    {
        await service.Spreadsheets.Values.BatchUpdate(
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
            }, SpreadsheetId).ExecuteAsync(); }
}
