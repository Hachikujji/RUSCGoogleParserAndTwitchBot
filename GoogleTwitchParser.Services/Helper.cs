using GoogleTwitchParser.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace GoogleTwitchParser.Services;

public static class Helper
{
    private static readonly string _settingsFileName = "settings.json";
    private static readonly string _settingsFolderPath = AppDomain.CurrentDomain.BaseDirectory + "\\cfg";
    private static readonly string _fullPath = Path.Combine(_settingsFolderPath, _settingsFileName);

    public static void LoadDataFromJsonFile()
    {
        try
        {
            if (!File.Exists(_fullPath))
                throw GetExceptionWithProperVariableName("OAuth Token and ClientId");
            string jsonString = String.Join('\n', File.ReadAllLines(_fullPath));
            dynamic json = JObject.Parse(jsonString);

            Variables.OAuthToken = string.IsNullOrEmpty(json.OAuthToken.ToString()) ? throw GetExceptionWithProperVariableName(nameof(Variables.OAuthToken)) : json.OAuthToken;
            Variables.ClientId = string.IsNullOrEmpty(json.ClientId.ToString()) ? throw GetExceptionWithProperVariableName(nameof(Variables.ClientId)) : json.ClientId;
            Variables.Timer = json.Timer is null || json.Timer == 0 ? 10000 : json.Timer;
        }
        catch (Exception)
        {
            throw ;
        }
    }

    public static async Task SaveDataToJsonFileAsync(Variables variables)
    {
        try
        {
            Directory.CreateDirectory(_settingsFolderPath);
            var json = JsonConvert.SerializeObject(variables);
            await File.WriteAllTextAsync(_fullPath,json);
        }
        catch (Exception)
        {
            throw new Exception("Failure to save settings to file..");
        }
    }

    private static Exception GetExceptionWithProperVariableName(string variable)
    {
        return new Exception($"WARNING: File is not configurated. Please update {variable}.");
    }
}
