using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using GoogleTwitchParser.Models;
using static Google.Apis.Sheets.v4.SpreadsheetsResource;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GoogleTwitchParser.Services;


public class GoogleSheetsService
{
    private readonly static string[] _scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string _settingsFolderPath = AppDomain.CurrentDomain.BaseDirectory + "\\cfg";
    private static string _googleCredentialsFileName = "google-credentials.json";
    private static string _googleCredentialsPath = Path.Combine(_settingsFolderPath, _googleCredentialsFileName);
    private static string _currentRowFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CURRENT");

    private readonly SheetsService _sheetsService;

    public GoogleSheetsService()
    {
        _sheetsService = GetSheetsService();
    }

    public async Task ConnectToSheetAsync(string sheetId, string sheetTitle)
    {
        var serviceValues = _sheetsService.Spreadsheets.Values;
        await GetDataAsync(serviceValues, sheetId, sheetTitle);
    }

    public async Task<TableData> UpdateFileAsync(string sheetId, string sheetTitle)
    {
        var data = await GetDataAsync(_sheetsService.Spreadsheets.Values, sheetId, sheetTitle);
        await SaveAllRecordsToFileAsync(data);
        return data;
    }

    public async Task<IEnumerable<string>> GetListOfTitlesAsync(string sheetId)
    {
        try
        {
            var data = await new SpreadsheetsResource.GetRequest(_sheetsService, sheetId).ExecuteAsync();
            return (data.Sheets.Select(sheet => sheet.Properties.Title)).ToList();
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to receive list of sheet titles. Please re-check your spreadsheet id. {e.Message}");
        }
    }

    private SheetsService GetSheetsService()
    {
        Directory.CreateDirectory(_settingsFolderPath);
        if (!File.Exists(_googleCredentialsPath))
            throw new Exception("Google Credentials file is not provided. Please put it into cfg folder.");
        using var stream = new FileStream(_googleCredentialsPath, FileMode.Open, FileAccess.Read);
        var serviceInitializer = new BaseClientService.Initializer
        {
            HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(_scopes)
        };
        return new SheetsService(serviceInitializer);
    }

    private async Task SaveAllRecordsToFileAsync(TableData data, int take = 1 + 5)
    {
        if (data.CurrentRow == null)
            return;

        // fill list with possible rows, if out of index then saves null. Must be handled in future. 
        for (int i = data.CurrentRowIndex; i < data.CurrentRowIndex + take; i++)
        {
            var row = data.Rows.ElementAtOrDefault(i);
            await SaveToFileAsync(data.Headers, row, MakeNextPathString(i - data.CurrentRowIndex));
        }
    }

    private async Task SaveToFileAsync(List<object>? headers, List<object>? row, string path)
    {
        Directory.CreateDirectory(path);
        for (int i = 0; i < headers.Count - 1; i++)
        {
            await File.WriteAllTextAsync(Path.Combine(path, headers[i] + ".txt"), row is not null ? row[i].ToString() : string.Empty);
        }

    }

    private string MakeNextPathString(int position) => position == 0 ? _currentRowFolderPath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"NEXT[{position}]");

    private static async Task<TableData> GetDataAsync(ValuesResource valuesResource, string sheetId, string sheetTitle)
    {
        var data = new TableData();
        try
        {
            // call api request to google
            var response = await valuesResource.Get(sheetId, sheetTitle).ExecuteAsync();
            var values = response.Values;
            var headers = values.FirstOrDefault();
            if (headers is null)
                throw new Exception("Unable to receive sheet headers.");
            data.Headers = headers.ToList() ?? new();
            data.HeaderMaxLength = data.Headers.Select(c => ((string)c).Length).Max();
            data.Rows = values.Skip(1).Select(r => r.ToList()).ToList();
            data.Rows.RemoveAll(list => !list.Any());

            var currentRow = data.Rows.LastOrDefault(row => row.Any(cell => cell.ToString() == "TRUE"));

            if(currentRow != null)
            {
                data.CurrentRowIndex = data.Rows.IndexOf(currentRow);
                data.CurrentRow = currentRow;
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Data error: {e.Message}");
        }
        return data;
    }

    public async Task<List<object>> GetHeadersAsync(string sheetId, string sheetTitle)
    {
        try
        {
            var response = await _sheetsService.Spreadsheets.Values.Get(sheetId, sheetTitle).ExecuteAsync();
            var values = response.Values;
            var headers = values.FirstOrDefault();
            return headers is not null ? headers.ToList() : new();
        }
        catch (Exception e)
        {
            throw new Exception($"Header retrieving error: {e.Message}");
        }
    }

    // UNUSED FEATURE. Needs discussion
    /*public static async Task SetFlagOnGameChange(string sheetId, string sheetTitle, string gameName)
    {
        var response = await _sheetsService.Spreadsheets.Values.Get(sheetId, sheetTitle).ExecuteAsync();
        var row = response.Values.Where( row => row.Contains(gameName) && row.LastOrDefault() as string != "TRUE").FirstOrDefault();
        var index = response.Values.IndexOf(row);
        var range = $"{sheetTitle}!A{index+1}";
        var lists = new List<IList<object>?>();
        lists.Add(row);
        if (row != null)
        {
            var item = row.LastOrDefault();
            //row.Insert(0, "ASD");
            row.Remove(item);
            item = "TRUE";
            row.Add(item);
            var valueRange = new ValueRange()
            {
                Range = range,
                Values = lists,
            };
            var updateRequest = _sheetsService.Spreadsheets.Values.Update(valueRange, sheetId, range);
            updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
            updateRequest.Execute();
        }

    }*/
}
