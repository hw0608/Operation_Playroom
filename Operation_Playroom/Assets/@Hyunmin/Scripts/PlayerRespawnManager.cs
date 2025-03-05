using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawnManager : NetworkBehaviour
{
    [SerializeField] NetworkObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            HandlePlayerSpawn(player);
        }

        PlayerController.OnPlayerSpawn += HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn += HandlePlayerDespawn;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }

        PlayerController.OnPlayerSpawn -= HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn -= HandlePlayerDespawn;
    }

    private void HandlePlayerSpawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie += HandlePlayerDie;
    }

    private void HandlePlayerDespawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie -= HandlePlayerDie;
    }

    private void HandlePlayerDie(Health sender)
    {
        PlayerController player = sender.GetComponent<PlayerController>();
        StartCoroutine(RespawnPlayerRoutine(player.OwnerClientId));

        player.GetComponent<NetworkObject>().Despawn();
        Destroy(player.gameObject);

    }

    IEnumerator RespawnPlayerRoutine(ulong ownerClientId)
    {
        yield return null;

        //NetworkObject playerObj = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPoint(), Quaternion.identity);
        //playerObj.SpawnAsPlayerObject(ownerClientId);
    }
}
