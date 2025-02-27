using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 병사 생성 및 초기화 
public class SoldierSpawner : NetworkBehaviour
{
    [SerializeField] GameObject[] soldierPrefabs;
    [SerializeField] Transform[] soldierSpawnPoints;
    [SerializeField] Transform kingTransform;
    [SerializeField] int initialSoldierCount = 3; // 초기 병사 수
    [SerializeField] int maxSoldierCount = 10; // 최대 병사 수 
    private List<GameObject> spawnSoldier = new List<GameObject>();

    public int currentSoldierCount;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentSoldierCount = initialSoldierCount;
            SpawnSoldiersServerRpc();
        }
    }
    

    [ServerRpc(RequireOwnership = false)]
    public void SpawnSoldiersServerRpc()
    {
        if (!IsServer) return;

        int formationIndex = 0;


        for (int i = 0; i < soldierSpawnPoints.Length; i++)
        {
            // Soldier Prefab을 Spawn Point 위치에 생성
            GameObject soldier = Instantiate(
                soldierPrefabs[i % soldierPrefabs.Length],
                soldierSpawnPoints[i % soldierSpawnPoints.Length].position,
                Quaternion.identity);

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // 모든 클라이언트에 동기화
            }

            IFormable formableSoldier = soldier.GetComponent<IFormable>(); // 병사 초기화
            if (formableSoldier != null)
            {
                formableSoldier.SoldierInitialize(kingTransform, formationIndex);
                formationIndex++;
            }
            spawnSoldier.Add(soldier);
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void AddSoldierServerRpc(int count)
    {
        if (!IsServer) return;

        if (count > maxSoldierCount)
        {
            Debug.Log("병사 최대치");
            return;
        }

        // Soldier 하나만 추가 스폰
        GameObject soldier = Instantiate(
            soldierPrefabs[count % soldierPrefabs.Length],
            soldierSpawnPoints[count % soldierSpawnPoints.Length].position,
            Quaternion.identity
        );

        NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsSpawned)
        {
            networkObject.Spawn(); // 모든 클라이언트에 동기화
        }

        IFormable formableSoldier = soldier.GetComponent<IFormable>(); // 병사 초기화
        if (formableSoldier != null)
        {
            formableSoldier.SoldierInitialize(kingTransform, spawnSoldier.Count);
        }

        // 생성된 병사를 리스트에 추가
        spawnSoldier.Add(soldier);

        currentSoldierCount = spawnSoldier.Count;
    }
}