using GoogleTwitchParser.Models;
using GoogleTwitchParser.Services;
using System;
namespace GoogleTwitchParser;

public class App
{
    private readonly static GoogleSheetsService _googleSheetsService = new();
    private readonly static TwitchApiService _twitchApiService = new(Variables.ClientId,Variables.OAuthToken);

    public App()
    {
    }

    public async Task StartMainMenu()
    {
        Console.Clear();
        try
        {
            Helper.LoadDataFromJsonFile();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        var twitchMode = Variables.IsTwitchParserEnabled ? $": {Variables.TwitchModeName}" : string.Empty;
        Console.WriteLine($"1. Старт");
        Console.WriteLine($"2. [{BoolToString(Variables.IsGoogleParserEnabled)}] Google Spreadsheets");
        Console.WriteLine($"3. [{BoolToString(Variables.IsTwitchParserEnabled)}] Twitch{twitchMode}");
        Console.WriteLine($"4. Время обновления: {Variables.Timer}мс");
        Console.WriteLine($"5. Обновить OAuth Token");
        Console.WriteLine($"6. Обновить ClientId");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.D1:
                Console.Clear();
                await StartCycleAsync();
                return;
            case ConsoleKey.D2:
                await GoogleSettingsMenu();
                return;
            case ConsoleKey.D3:
                await TwitchSettingsMenu();
                return;
            case ConsoleKey.D4:
                Variables.Timer = TimerSettingsMenu();
                return;
            case ConsoleKey.D5:
                await RefreshOAuthAsync();
                return;
            case ConsoleKey.D6:
                await RefreshClientIdAsync();
                return;
            default:
                return;
        }
    }
    private async Task RefreshOAuthAsync()
    {
        Console.Clear();
        Console.WriteLine("Введите OAuth Token:");
        var token = Console.ReadLine();
        Variables.OAuthToken = token;
        await Helper.SaveDataToJsonFileAsync(new Variables());
    }
    private async Task RefreshClientIdAsync()
    {
        Console.Clear();
        Console.WriteLine("Введите ClientId:");
        var token = Console.ReadLine();
        Variables.ClientId = token;
        await Helper.SaveDataToJsonFileAsync(new Variables());
    }

    private async Task GoogleSettingsMenu()
    {
        var menuSecondPoint = "2. Выбрать Таблицу";
        if (!string.IsNullOrWhiteSpace(Variables.SpreadSheetTitle))
            menuSecondPoint = $"2. Таблица [{Variables.SpreadSheetTitle}]";
        var menuThirdPoint = "3. Выбрать Лист";
        if (!string.IsNullOrWhiteSpace(Variables.SheetTitle))
            menuThirdPoint = $"3. Лист [{Variables.SheetTitle}]";

        Console.Clear();
        Console.WriteLine($"1. Статус: [{BoolToString(Variables.IsGoogleParserEnabled)}]");
        Console.WriteLine(menuSecondPoint);
        Console.WriteLine(menuThirdPoint);
        Console.WriteLine("4. Назад");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.D1:
                /*if ((Variables.TwitchModeName == TwitchModeEnum.GoogleGameColumn.ToString()) && Variables.IsTwitchParserEnabled)
                    Variables.IsTwitchParserEnabled = false;*/
                if (string.IsNullOrWhiteSpace(Variables.SheetTitle))
                    await ChooseSpreadSheetMenu();
                else
                    Variables.IsGoogleParserEnabled = !Variables.IsGoogleParserEnabled;
                return;
            case ConsoleKey.D2:
                await ChooseSpreadSheetMenu();
                return;
            case ConsoleKey.D3:
                Variables.SheetTitle = await ChooseSheetMenuAsync();
                return;
            default:
                return;
        }
    }

    private async Task TwitchSettingsMenu()
    {
        Console.Clear();
        Console.WriteLine($"1. Статус: [{BoolToString(Variables.IsTwitchParserEnabled)}]");
        Console.WriteLine("2. Пиздить игру с другого стрима");
        Console.WriteLine("3. Пиздить игру с таблицы");
        Console.WriteLine("4. Назад");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.D1:
                Variables.IsTwitchParserEnabled = !Variables.IsTwitchParserEnabled;

                return;
            case ConsoleKey.D2:
                Variables.MainChannelNickname = ChooseMainTwitchUsernameMenu();
                Variables.SpyChannelNickname = ChooseSpyTwitchUsernameMenu();
                Variables.TwitchModeName = TwitchModeEnum.TwitchLiveSpy.ToString();
                Variables.IsTwitchParserEnabled = true;
                return;
            case ConsoleKey.D3:
                Variables.MainChannelNickname = ChooseMainTwitchUsernameMenu();
                if (string.IsNullOrWhiteSpace(Variables.SheetTitle))
                    await ChooseSpreadSheetMenu();
                Variables.GameColumn = await ChooseGameColumnAsync();
                Variables.IsTwitchParserEnabled = true;
                Variables.TwitchModeName = TwitchModeEnum.GoogleGameColumn.ToString();
                return;
            default:
                return;
        }
    }
    private static async Task<string> ChooseGameColumnAsync()
    {

        var headers = await _googleSheetsService.GetHeadersAsync(Variables.SpreadSheetId, Variables.SheetTitle);
        int index = 0;
        do
        {
            Console.Clear();
            Console.WriteLine("Список колонок:");
            for (int i = 0; i < headers.Count(); i++)
                Console.WriteLine($"{i + 1}. {headers.ElementAt(i)}");
            Console.WriteLine("Выберите колонку с игрой:");
            int.TryParse(Console.ReadLine(), out index);
        }
        while (index == 0);
        return headers.ElementAt(index - 1).ToString();
    }

    private static string ChooseMainTwitchUsernameMenu()
    {
        Console.Clear();
        Console.WriteLine("Введите свой никнейм:");
        return Console.ReadLine();
    }

    private static string ChooseSpyTwitchUsernameMenu()
    {
        Console.Clear();
        Console.WriteLine("Введите никнейм канала с которого будем пиздить:");
        return Console.ReadLine();
    }

    private static int TimerSettingsMenu()
    {
        int index = 0;
        do
        {
            Console.Clear();
            Console.WriteLine("Введите время обновления в мс:");
            int.TryParse(Console.ReadLine(), out index);
        }
        while (index == 0);
        return index;
    }

    private async Task ChooseSpreadSheetMenu()
    {
        Console.Clear();
        Console.WriteLine("Таблица:");
        Console.WriteLine("1. RUSC");
        Console.WriteLine("2. Kioshides");
        Console.WriteLine("3. Мануальный ввод");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.D1:
                Variables.SpreadSheetTitle = "RUSC";
                Variables.SheetTitle = string.Empty;
                Variables.SpreadSheetId = "1K7M_bTNG_VX22Xljv_dyyOWLDbl_e5QFqzzqHOvEPvM";
                Variables.SheetTitle = await ChooseSheetMenuAsync();
                Variables.IsGoogleParserEnabled = true;
                return;
            case ConsoleKey.D2:
                Variables.SpreadSheetTitle = "Kioshides";
                Variables.SheetTitle = string.Empty;
                Variables.SpreadSheetId = "1o3ZuJc4ldGLBx6Z8nhaMRKfLWZ1o-dRcwpR4G2IqAlQ";
                Variables.SheetTitle = await ChooseSheetMenuAsync();
                Variables.IsGoogleParserEnabled = true;
                return;
            default:
            {
                Console.Clear();
                Console.WriteLine("Введите ID:");
                Variables.SpreadSheetId = Console.ReadLine();
                Variables.SpreadSheetTitle = "Мануальная";
                Variables.SheetTitle = string.Empty;
                Variables.SheetTitle = await ChooseSheetMenuAsync();
                Variables.IsGoogleParserEnabled = true;
                return;
            }
        }
    }

    private static async Task<string> ChooseSheetMenuAsync()
    {
        var titles = await _googleSheetsService.GetListOfTitlesAsync(Variables.SpreadSheetId);
        int index = 0;
        do
        {
            Console.Clear();
            Console.WriteLine("Список листов:");
            for (int i = 0; i < titles.Count(); i++)
                Console.WriteLine($"{i + 1}. {titles.ElementAt(i)}");
            Console.WriteLine("Выберите лист:");
            int.TryParse(Console.ReadLine(), out index);
        }
        while (index == 0);
        return titles.ElementAt(index-1);
    }

    private async Task StartCycleAsync()
    {
        Console.Clear();
        Console.WriteLine("Для выхода в меню зажмите Esc.");
        Console.WriteLine();
        do
        {
            while (!Console.KeyAvailable)
            {
                try
                {
                    TableData table = new();
                    if (Variables.IsGoogleParserEnabled)
                    {
                        table = await _googleSheetsService.UpdateFileAsync(Variables.SpreadSheetId, Variables.SheetTitle);
                        Console.WriteLine($"[{DateTimeOffset.Now.Hour}:{DateTimeOffset.Now.Minute}:{DateTimeOffset.Now.Second}] Текущая строка:");
                        for (int i = 0; i < table.Headers.Count - 1; i++)
                            Console.WriteLine($"{table.Headers[i].ToString().PadRight(table.HeaderMaxLength + 1)}: {table.Rows[table.CurrentRow][i]}");
                    }

                    if (Variables.IsTwitchParserEnabled)
                    {
                        switch (Variables.TwitchModeName)
                        {
                            case nameof(TwitchModeEnum.TwitchLiveSpy):
                                {
                                    var settedGameName = await _twitchApiService.ChangeGameToTheSpiedChannelAsync(Variables.OAuthToken, Variables.SpyChannelNickname, Variables.MainChannelNickname);
                                    if (settedGameName is not null)
                                        Console.WriteLine($"[{DateTimeOffset.Now.Hour}:{DateTimeOffset.Now.Minute}:{DateTimeOffset.Now.Second}] Смена игры на {settedGameName}");
                                    else
                                        Console.WriteLine($"[{DateTimeOffset.Now.Hour}:{DateTimeOffset.Now.Minute}:{DateTimeOffset.Now.Second}] Игра актуальная");
                                    break;
                                }
                            case nameof(TwitchModeEnum.GoogleGameColumn):
                                {
                                    if (table.Headers is null)
                                        table = await _googleSheetsService.UpdateFileAsync(Variables.SpreadSheetId, Variables.SheetTitle);
                                    var gameColumnIndex = table.Headers.FindIndex(header => header.Equals(Variables.GameColumn));
                                    var gameName = table.Rows[table.CurrentRow][gameColumnIndex].ToString();
                                    var settedGameName = await _twitchApiService.ChangeGameAsync(Variables.OAuthToken, Variables.MainChannelNickname, gameName);
                                    if (settedGameName is not null)
                                        Console.WriteLine($"[{DateTimeOffset.Now.Hour}:{DateTimeOffset.Now.Minute}:{DateTimeOffset.Now.Second}] Смена игры на {settedGameName}");
                                    else
                                        Console.WriteLine($"[{DateTimeOffset.Now.Hour}:{DateTimeOffset.Now.Minute}:{DateTimeOffset.Now.Second}] Игра актуальная");
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"WARNING: {e.Message}");
                }
                Console.WriteLine();
                Thread.Sleep(Variables.Timer);
            }
        } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
    }

    private static string BoolToString(bool var)
    {
        return var ? "✔️" : "❌";
    }

}
