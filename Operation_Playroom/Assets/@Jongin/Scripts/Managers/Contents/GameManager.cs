using Unity.Netcode;
using UnityEngine;
using static Define;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    
    EGameState gameState;

    void Start()
    {
        gameState = EGameState.Ready;
        remainTime.Value = 600f;
    }


    void Update()
    {
        if (gameState != EGameState.Play) return;

        if (IsServer)
        {
            remainTime.Value -= Time.deltaTime;
        }
    }
}
