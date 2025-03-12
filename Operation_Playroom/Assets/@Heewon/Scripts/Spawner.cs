using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    public int index = 0;
    [SerializeField] GameObject soldierPrefab;

    public void SpawnSoldiers(int count)
    {
        KingTest king = GetComponent<KingTest>();
        int cnt = 0;

        for (int i = 0; i < king.soldiers.Count; i++)
        {
            if (king.soldiers[i] != null) cnt++;
        }

        if (cnt >= 10)
        {
            return;
        }

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

        if (king.soldiers.Count > index)
        {
            king.soldiers[index] = soldierObj.GetComponent<SoldierTest>();
        }
        else
        {
            king.soldiers.Add(soldierObj.GetComponent<SoldierTest>());
        }

        soldier.Init(king.transform, king.soldierOffsets[index]);
        index++;
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
