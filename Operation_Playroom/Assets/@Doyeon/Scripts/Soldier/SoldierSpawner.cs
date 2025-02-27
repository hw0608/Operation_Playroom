using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// 병사 생성 및 초기화 
public class SoldierSpawner : NetworkBehaviour
{
    private static SoldierSpawner _instance;
    public static SoldierSpawner Instance
    {
        get
        {
            //if (_instance == null)
            //{
            //    GameObject go = new GameObject("SoldierPool");
            //    _instance = go.AddComponent<SoldierPool>();
            //    DontDestroyOnLoad(go);
                
            //    if (_instance == null)
            //        Debug.LogError("병사풀이 존재하지 않습니다");
            //}
            return _instance;
        }
    }
    // 풀관련
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private int poolSize = 10;
    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    // 스폰 관련
    [SerializeField] Transform kingTransform;
    [SerializeField] int initialSoldierCount = 3; // 초기 병사 수
    [SerializeField] int maxSoldierCount = 10; // 최대 병사 수 

    private List<GameObject> spawnSoldier = new List<GameObject>();
    private int currentSoldierCount;
    private float scaleFactor = 0.1f; // 왕 병사 스케일

    public override void OnNetworkSpawn()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (IsServer)
        {
            Debug.Log("서버에서 병사 스폰");
            InitializePool();
            currentSoldierCount = initialSoldierCount;
            SpawnSoldiersServerRpc();
        }
    }
    // 병사 풀 초기화
    private void InitializePool()
    {
        //if (!IsServer) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject soldier = Instantiate(soldierPrefab);
            //soldier.SetActive(false);

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
                networkObject.Despawn();
            }

            poolQueue.Enqueue(soldier);
        }
    }
    private GameObject GetFromPool()
    {
        //if (!IsServer) return null;

        if (poolQueue.Count > 0)
        {
            GameObject soldier = poolQueue.Dequeue();
            soldier.SetActive(true);

            NetworkObject netObj = soldier.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }

            return soldier;
        }
        else
        {
            GameObject soldier = Instantiate(soldierPrefab);
            NetworkObject netObj = soldier.GetComponent<NetworkObject>();

            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }

            soldier.SetActive(true);
            return soldier;
        }
    }
    private void ReturnToPool(GameObject soldier)
    {
        NetworkObject netObj = soldier.GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }

        soldier.SetActive(false);
        poolQueue.Enqueue(soldier);
    }


    [ServerRpc(RequireOwnership = false)]
    public void SpawnSoldiersServerRpc()
    {
        int formationIndex = 0;

        for (int i = 0; i < initialSoldierCount; i++)
        {
            GameObject soldier = SoldierSpawner.Instance.GetFromPool(); 
            soldier.transform.position = GetTrianglePosition(formationIndex); 
            soldier.transform.rotation = Quaternion.identity;

            Debug.Log($"[초기 병사 스폰] 병사 {i} 위치: {soldier.transform.position}");

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>(); 
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // 모든 클라이언트에 동기화
            }

            SoldierFormation formation = soldier.GetComponent<SoldierFormation>(); // 병사 포메이션 초기화
            if (formation != null)
            {
                formation.SoldierFormationInitialize(kingTransform, formationIndex);
            }

            IFormable formableSoldier = soldier.GetComponent<IFormable>(); // 병사 초기화
            if (formableSoldier != null)
            {
                formableSoldier.SoldierInitialize(kingTransform, formationIndex);
                formationIndex++;
            }
            spawnSoldier.Add(soldier); // 초기 병사 추가 
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void AddSoldierServerRpc(int count)
    {
        if (!IsServer) return;

        if (count >= maxSoldierCount)
        {
            Debug.Log("병사 최대치");
            return;
        }

        GameObject soldier = SoldierSpawner.Instance.GetFromPool();
        soldier.transform.position = GetTrianglePosition(spawnSoldier.Count);
        soldier.transform.rotation = Quaternion.identity;

        NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsSpawned)
        {
            networkObject.Spawn(); // 모든 클라이언트에 동기화
        }

        SoldierFormation formation = soldier.GetComponent<SoldierFormation>();
        if (formation != null)
        {
            formation.SoldierFormationInitialize(kingTransform, spawnSoldier.Count);
        }

        IFormable formableSoldier = soldier.GetComponent<IFormable>(); // 병사 초기화
        if (formableSoldier != null)
        {
            formableSoldier.SoldierInitialize(kingTransform, spawnSoldier.Count);
        }

        // 생성된 병사 추가
        spawnSoldier.Add(soldier);

        currentSoldierCount = spawnSoldier.Count;
    }
   
    [ClientRpc]
    private void InitializeFormationClientRpc(ulong networkObjectId, int formationIndex)
    {
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        SoldierFormation formation = netObj.GetComponent<SoldierFormation>();

        if (formation != null)
        {
            formation.SoldierFormationInitialize(kingTransform, formationIndex);
        }
    }
    // 병사 위치
    public Vector3 GetTrianglePosition(int index)
    {
        float spacing = 1.5f * scaleFactor;
        Vector3 kingPosition = kingTransform.position;
        int row = (index / 2) + 1; // 행
        int col = index % 2 == 0 ? -1 : 1; // 열(짝수면 왼쪽, 홀수면 오른쪽)

        Vector3 forwardOffset = kingTransform.forward * -spacing * row; // 왕 뒤
        Vector3 sideOffset = kingTransform.right * spacing * col * row; // 왕 좌우

        return kingPosition + forwardOffset + sideOffset;
    }
    // 특정 병사 제거 
    public void RemoveSoldier(GameObject soldier)
    {
        if (spawnSoldier.Contains(soldier))
        {
            spawnSoldier.Remove(soldier);
            ReturnToPool(soldier);
            currentSoldierCount = spawnSoldier.Count;
        }
    }
}