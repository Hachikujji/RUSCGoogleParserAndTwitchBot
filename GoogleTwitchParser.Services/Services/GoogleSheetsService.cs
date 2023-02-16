using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using GoogleTwitchParser.Models;
using System.Reflection;
using static Google.Apis.Sheets.v4.SpreadsheetsResource;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace GoogleTwitchParser.Services;


public class GoogleSheetsService
{
    private readonly static string[] _scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string _settingsFolderPath = AppDomain.CurrentDomain.BaseDirectory + "\\cfg";
    private static string _googleCredentialsFileName = "google-credentials.json";
    private static string _fullPath = Path.Combine(_settingsFolderPath, _googleCredentialsFileName);

    private static SheetsService? _sheetsService;

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
        await SaveToFileAsync(data);
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
        if (!File.Exists(_fullPath))
            throw new Exception("Google Credentials file is not provided. Please put it into cfg folder.");
        using (var stream = new FileStream(_fullPath, FileMode.Open, FileAccess.Read))
        {
            var serviceInitializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(_scopes)
            };
            return new SheetsService(serviceInitializer);
        }
    }

    private async Task SaveToFileAsync(TableData data)
    {
        var path = AppDomain.CurrentDomain.BaseDirectory + "\\CURRENT";
        var nextStatementPath = AppDomain.CurrentDomain.BaseDirectory + "\\NEXT";
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(nextStatementPath);
        var row = data.Rows.Where(x => x.Last().ToString() == "TRUE").ToList().LastOrDefault();
        var nextRow = data.Rows.ElementAtOrDefault(data.Rows.IndexOf(row) + 1);
        if (row == null)
            return;
        var sceneName = row.ElementAt(row.Count - 2).ToString();
        /////////////////////await ChangeSceneAsync(sceneName);
        for (int k = 0; k < row.Count - 1; k++)
        {
            await File.WriteAllTextAsync(Path.Combine(path, data.Headers[k].ToString() + ".txt"), row[k].ToString());
        }

        if (nextRow == null)
        {
            nextRow = new List<object>();
            foreach (var item in row)
            {
                nextRow.Add("");
            }
        }
        for (int k = 0; k < nextRow.Count - 1; k++)
        {
            await File.WriteAllTextAsync(Path.Combine(nextStatementPath, data.Headers[k].ToString() + ".txt"), nextRow[k].ToString());
        }
    }

    private static async Task<TableData> GetDataAsync(ValuesResource valuesResource, string sheetId, string sheetTitle)
    {
        var data = new TableData();
        try
        {
            var response = await valuesResource.Get(sheetId, sheetTitle).ExecuteAsync();
            var values = response.Values;
            data.Headers = values.FirstOrDefault().ToList();
            data.Rows = values.Skip(1).Select(r => r.ToList()).ToList();
            var currentRow = data.Rows.Where(x => x.Last().ToString() == "TRUE").ToList().LastOrDefault();
            if(currentRow != null)
                data.CurrentRow = data.Rows.IndexOf(currentRow);
            // max length
            data.HeaderMaxLength = data.Headers.Select(c => ((string)c).Length).Max();
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
            return values.FirstOrDefault().ToList();
        }
        catch (Exception e)
        {
            throw new Exception($"Header retrieving error: {e.Message}");
        }
    }

    public static async Task SetFlagOnGameChange(string sheetId, string sheetTitle, string gameName)
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

    }
}
