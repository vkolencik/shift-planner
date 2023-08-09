using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace ShiftPlanner.Services.Services.Google;

public abstract class GoogleSheetsService
{
    private static readonly string CertificateArn = "arn:aws:secretsmanager:eu-central-1:720500131615:secret:google-sheets-api-certificate-aY85SJ";
    private static readonly string[] Scopes = {SheetsService.Scope.Spreadsheets};
    private static readonly string ApplicationName = "Krtek";
    protected static readonly string SpreadsheetId = "1iLE1TDFKrUbnzTVX8PmjP5AvvccD2dsFjomD5c5yiJs";

    private IAwsSecretsManagerClient _secretsManagerClient;

    protected GoogleSheetsService(IAwsSecretsManagerClient secretsManagerClient)
    {
        _secretsManagerClient = secretsManagerClient;
    }

    protected async Task<SheetsService> GetGoogleSheetsService()
    {
        var secretJson = await _secretsManagerClient.FetchRawSecret(CertificateArn);
        var credential = GoogleCredential.FromJson(secretJson).CreateScoped(Scopes);

        Console.Out.WriteLine("Fetched Google credentials");
        
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
