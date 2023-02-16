/*using Newtonsoft.Json.Linq;
using System.Reflection;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using GoogleTwitchParser.Models;
using GoogleTwitchParser.Exceptions;

namespace GoogleTwitchParser.Services;

public class TwitchChatService
{

    public TwitchClient? TwitchClient { get; set; }

    public TwitchChatService()
    {
        var appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (appDirectory is not null)
        {
            string jsonString = string.Join('\n', File.ReadAllLines(Path.Combine(appDirectory, "settings.json")));
            dynamic json = JObject.Parse(jsonString);
            JsonConfig.Ip = json.ip;
            JsonConfig.Username = json.username;
            JsonConfig.AccessToken = json.accessToken;
            JsonConfig.Port = (int)json.port;
            JsonConfig.Channel = json.channel;
        }
        else
            throw new PathToFileException();
    }

    public void Start()
    {
        Authorize();
    }

    private void BindEvents()
    {
        if (TwitchClient is not null)
        {
            TwitchClient.OnLog += Client_OnLog;
            TwitchClient.OnJoinedChannel += Client_OnJoinedChannel;
            TwitchClient.OnConnected += Client_OnConnected;
        }
    }

    private void Authorize()
    {
        try
        {
            ConnectionCredentials credentials = new ConnectionCredentials(JsonConfig.Username, JsonConfig.AccessToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            TwitchClient = new TwitchClient(customClient);
            BindEvents();
            TwitchClient.Initialize(credentials, JsonConfig.Channel);
            TwitchClient.Connect();
        }
        catch (Exception e)
        {
            throw new TwitchApiException($"Unable to authorize to twitch: {e}");
        }
    }

    private void Client_OnLog(object sender, OnLogArgs e)
    {
        Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
    }

    private void Client_OnConnected(object sender, OnConnectedArgs e)
    {
        Console.WriteLine($"Connected to {e.AutoJoinChannel}");
    }

    private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
    {
        Console.WriteLine($"Joined to {e.Channel}");
        TwitchClient.SendMessage(e.Channel, "Я РУССКИЙ!");
    }
}
*/