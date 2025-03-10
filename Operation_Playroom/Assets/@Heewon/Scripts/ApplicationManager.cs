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
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        Managers.Resource.LoadAllAsync<GameObject>("default", (name, loadCount, totalCount) =>
        {
            if (loadCount >= totalCount)
            {
                tcs.SetResult(true);
            }
        });

        await tcs.Task;

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
            else
            {
                //TODO: 로그인 실패했을 경우 재시도
            }
        }
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene("MainScene");
    }
}
