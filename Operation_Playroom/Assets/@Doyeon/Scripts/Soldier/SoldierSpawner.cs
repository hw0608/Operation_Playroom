using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ���� ���� �� �ʱ�ȭ 
public class SoldierSpawner : NetworkBehaviour
{
    [SerializeField] GameObject[] soldierPrefabs;
    [SerializeField] Transform[] soldierSpawnPoints;
    [SerializeField] Transform kingTransform;
    [SerializeField] int initialSoldierCount = 3; // �ʱ� ���� ��
    [SerializeField] int maxSoldierCount = 10; // �ִ� ���� �� 
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
            // Soldier Prefab�� Spawn Point ��ġ�� ����
            GameObject soldier = Instantiate(
                soldierPrefabs[i % soldierPrefabs.Length],
                soldierSpawnPoints[i % soldierSpawnPoints.Length].position,
                Quaternion.identity);

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // ��� Ŭ���̾�Ʈ�� ����ȭ
            }

            IFormable formableSoldier = soldier.GetComponent<IFormable>(); // ���� �ʱ�ȭ
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
            Debug.Log("���� �ִ�ġ");
            return;
        }

        // Soldier �ϳ��� �߰� ����
        GameObject soldier = Instantiate(
            soldierPrefabs[count % soldierPrefabs.Length],
            soldierSpawnPoints[count % soldierSpawnPoints.Length].position,
            Quaternion.identity
        );

        NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsSpawned)
        {
            networkObject.Spawn(); // ��� Ŭ���̾�Ʈ�� ����ȭ
        }

        IFormable formableSoldier = soldier.GetComponent<IFormable>(); // ���� �ʱ�ȭ
        if (formableSoldier != null)
        {
            formableSoldier.SoldierInitialize(kingTransform, spawnSoldier.Count);
        }

        // ������ ���縦 ����Ʈ�� �߰�
        spawnSoldier.Add(soldier);

        currentSoldierCount = spawnSoldier.Count;
    }
}