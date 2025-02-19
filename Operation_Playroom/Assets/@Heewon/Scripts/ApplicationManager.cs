using System.Threading.Tasks;
using Unity.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
    ApplicationData appData;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server);
    }

    async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            appData = new ApplicationData();

            ServerSingleton.Instance.Init();
            await ServerSingleton.Instance.CreateServer();
            await ServerSingleton.Instance.serverManager.StartGameServerAsync();
        }
        else
        {
            bool authenticated = await ClientSingleton.Instance.InitAsync();

            HostSingleton hostSingleton = HostSingleton.Instance;

            if (authenticated)
            {
                GotoMenu();
            }

        }
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
