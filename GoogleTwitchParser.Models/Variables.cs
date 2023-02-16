using Newtonsoft.Json;

namespace GoogleTwitchParser.Models;
public class Variables
{
    [JsonProperty]
    public static int Timer { get; set; } = 10000;

    // google
    public static bool IsGoogleParserEnabled { get; set; }

    public static string SpreadSheetId { get; set; }

    public static string SpreadSheetTitle { get; set; }

    public static string SheetTitle { get; set; }

    public static string GameColumn { get; set; }

    // twitch
    public static bool IsTwitchParserEnabled { get; set; }

    public static string TwitchModeName { get; set; }

    public static string MainChannelNickname { get; set; }

    public static string SpyChannelNickname { get; set; }

    [JsonProperty]
    public static string ClientId { get; set; }

    [JsonProperty]
    public static string OAuthToken { get; set; }
}
