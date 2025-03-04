using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    [SerializeField] GameObject soldierPrefab;

    public void SpawnSoldiers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSoldierServerRpc(transform.position, NetworkManager.Singleton.LocalClientId);
        }
    }

    public void DespawnSoldier(ulong soldierId)
    {
        DespawnSoldierServerRpc(soldierId);
    }

    [ServerRpc]
    void SpawnSoldierServerRpc(Vector3 position, ulong clientId)
    {
        GameObject soldier = Instantiate(soldierPrefab, position + Vector3.back, Quaternion.identity);
        NetworkObject netObj = soldier.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId, true);

        AddSoldierClientRpc(netObj.NetworkObjectId, clientId);
    }

    [ClientRpc]
    void AddSoldierClientRpc(ulong soldierId, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        NetworkObject soldierObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[soldierId];
        SoldierTest soldier = soldierObj.GetComponent<SoldierTest>();
        
        NetworkObject kingObj = GetComponent<NetworkObject>();
        KingTest king = NetworkManager.Singleton.SpawnManager.SpawnedObjects[kingObj.NetworkObjectId].GetComponent<KingTest>();

        king.soldiers.Add(soldierObj.GetComponent<SoldierTest>());
        soldier.SetKing(king.transform);
    }

    [ServerRpc]
    void DespawnSoldierServerRpc(ulong soldierId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(soldierId, out NetworkObject soldier))
        {
            soldier.Despawn(true);
        }
    }
}
