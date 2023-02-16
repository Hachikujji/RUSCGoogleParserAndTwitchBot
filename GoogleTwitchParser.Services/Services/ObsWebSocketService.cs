/*using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTwitchParser.Services;

public class ObsWebSocketService
{
    private static readonly OBSWebsocket _obs = new OBSWebsocket();
    private static Task ConnectToObsAsync()
    {
        try
        {
            _obs.Connect($"ws://127.0.0.1:4444", "BEBRARUSC");
            if (_obs.IsConnected)
                Console.WriteLine("OBS работает намана");
        }
        catch (Exception)
        {
            Console.WriteLine("Чет нихуя к OBS не коннектит...");
        }
        return Task.CompletedTask;
    }

    private static async Task ChangeSceneAsync(string sceneText)
    {
        try
        {
            if (!_obs.IsConnected)
            {
                await ConnectToObsAsync();
            }

            if (_obs.IsConnected)
            {
                _obs.SetCurrentProgramScene(sceneText);
                Console.WriteLine($"Смена сцены на: {sceneText}");
            }
        }
        catch (ErrorResponseException)
        {
            Console.WriteLine("Блять чет ты обосрался с OBS сценой");
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine("Ты нахуя OBS закрыл?");
        }
    }
}
*/