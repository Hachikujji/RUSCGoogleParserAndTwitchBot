using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Channels.ModifyChannelInformation;
using TwitchLib.Api.Helix.Models.Streams.CreateStreamMarker;

namespace GoogleTwitchParser.Services;

public class TwitchApiService
{
    private TwitchAPI? API;
    public bool IsApiConnected { get; set; } = false;

    private string previousMarkerDescription = string.Empty;

    public TwitchApiService(string clientId, string accessToken)
    {
        ConfigApi(clientId, accessToken);
    }

    private void ConfigApi(string clientId, string accessToken)
    {
        API = new TwitchAPI();
        API.Settings.ClientId = clientId;
        API.Settings.AccessToken = accessToken;
    }

    public async Task TestApiAsync()
    {
        try
        {
            await API.Helix.Games.GetTopGamesAsync();
            IsApiConnected = true;
        }
        catch (Exception)
        {
            throw new Exception("Unable to get response from the twitch server. Please verify OAuth token.");
        }
    }

    public async Task<string?> ChangeGameAsync(string accessToken, string channelName, string gameName)
    {
        try
        {
            //Returns a stream object if online, if channel is offline it will be null/empty.
            var channelStreamResponse = (await API.Helix.Streams.GetStreamsAsync(userLogins: new List<string>() { channelName }));
            if (channelStreamResponse is null)
                throw new Exception("Unable to get channel response from the twitch servers.");
            var channelStream = channelStreamResponse.Streams.FirstOrDefault();
            if (channelStream is null)
                throw new Exception("Your channel is not online or unavailable");

            var gameResponse = (await API.Helix.Games.GetGamesAsync(gameNames: new List<string>() { gameName }));
            if (gameResponse is null)
                throw new Exception("Unable to get game response from the twitch servers.");
            var game = gameResponse.Games.FirstOrDefault();
            if (game is null)
                throw new Exception($"Game {gameName} not found.");

            if (channelStream.GameId == game.Id)
                return null;

            //Update Channel Title/Game/Language/Delay - Only require 1 option here.
            var request = new ModifyChannelInformationRequest() { GameId = game.Id};
            await API.Helix.Channels.ModifyChannelInformationAsync(channelStream.UserId, request, accessToken);
            return game.Name;
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public async Task<string?> ChangeGameToTheSpiedChannelAsync(string accessToken, string spyChannelName, string channelName)
    {
        try
        {
            var channelStreamResponse = (await API.Helix.Streams.GetStreamsAsync(userLogins: new List<string>() { channelName }));
            var spyChannelStreamResponse = (await API.Helix.Streams.GetStreamsAsync(userLogins: new List<string>() { spyChannelName }));
            var channelStream = channelStreamResponse.Streams.FirstOrDefault();
            var spyChannelStream = spyChannelStreamResponse.Streams.FirstOrDefault();
            if (channelStream is null)
                throw new Exception("Your channel is not online or unavailable");
            if (spyChannelStream is null)
                throw new Exception("Spy channel is not online or unavailable");
            if (channelStream.GameId == spyChannelStream.GameId)
                return null;
            var request = new ModifyChannelInformationRequest() { GameId = spyChannelStream.GameId };
            await API.Helix.Channels.ModifyChannelInformationAsync(channelStream.UserId, request, accessToken);
            await SetMarkerAsync(accessToken, channelName, spyChannelStream.GameName);
            return spyChannelStream.GameName;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to get information about stream: {e.Message}");
        }
    }

    public async Task SetMarkerAsync(string accessToken, string channelName, string description)
    {
        try
        {
            if (description == previousMarkerDescription)
                return;
            var channelStreamResponse = (await API.Helix.Streams.GetStreamsAsync(userLogins: new List<string>() { channelName }));
            var channelStream = channelStreamResponse.Streams.FirstOrDefault();
            if (channelStream is null)
                throw new Exception("Your channel is not online or unavailable");
            var request = new CreateStreamMarkerRequest() { Description = description, UserId = channelStream.UserId};
            await API.Helix.Streams.CreateStreamMarkerAsync(request, accessToken);
            previousMarkerDescription = description;
        }
        catch (Exception e)
        {
            throw new Exception($"Unable to set marker: {e.Message}");
        }
    }

    public async Task SendShoutoutAsync(string shoutouterLogin, string shoutoutedLogin)
    {
        var shotouterUserResponce = await API.Helix.Users.GetUsersAsync(logins: new List<string>() { $"{shoutouterLogin}" });
        var shotouterUser = shotouterUserResponce.Users.FirstOrDefault();

    }
}
