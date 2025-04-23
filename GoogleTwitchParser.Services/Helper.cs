using GoogleTwitchParser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GoogleTwitchParser.Services;

public static class Helper
{
    private static readonly string _settingsFileName = "settings.json";
    private static readonly string _settingsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cfg");
    private static readonly string _settingsFullPath = Path.Combine(_settingsFolderPath, _settingsFileName);

    public static void LoadDataFromJsonFile()
    {
        try
        {
            if (!File.Exists(_settingsFullPath))
                throw GetExceptionWithProperVariableName("OAuth Token and ClientId");
            string jsonString = string.Join('\n', File.ReadAllLines(_settingsFullPath));
            dynamic json = JObject.Parse(jsonString);

            Variables.OAuthToken = string.IsNullOrEmpty(json.OAuthToken.ToString()) ? throw GetExceptionWithProperVariableName(nameof(Variables.OAuthToken)) : json.OAuthToken;
            Variables.ClientId = string.IsNullOrEmpty(json.ClientId.ToString()) ? throw GetExceptionWithProperVariableName(nameof(Variables.ClientId)) : json.ClientId;
            Variables.Timer = json.Timer is null || json.Timer == 0 ? Variables.TimerDefault : json.Timer;
        }
        catch (Exception e)
        {
            throw new Exception($"Failure to load settings from file.. {e.Message}");
        }
    }

    public static async Task SaveDataToJsonFileAsync(Variables variables)
    {
        try
        {
            Directory.CreateDirectory(_settingsFolderPath);
            var json = JsonConvert.SerializeObject(variables);
            await File.WriteAllTextAsync(_settingsFullPath,json);
        }
        catch (Exception e)
        {
            throw new Exception($"Failure to save settings to file: {e.Message}");
        }
    }

    private static Exception GetExceptionWithProperVariableName(string variable)
    {
        return new Exception($"WARNING: File is not configurated. Please update {variable}.");
    }
}
