using Unity.Netcode;
using UnityEngine;

public class GameSceneUI : MonoBehaviour
{
    public void OnDisconnectButtonPressed()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
