using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ���� ���� �� �ʱ�ȭ 
public class SoldierSpawner : NetworkBehaviour
{
    private static SoldierSpawner _instance;
    public static SoldierSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SoldierSpawner");
                _instance = go.AddComponent<SoldierSpawner>();
                DontDestroyOnLoad(go);

                if (_instance == null)
                    Debug.LogError("����Ǯ�� �������� �ʽ��ϴ�");
            }
            return _instance;
        }
    }
    // Ǯ����
    [SerializeField] private GameObject soldierPrefab;
    [SerializeField] private int poolSize = 10;
    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    // ���� ����
    [SerializeField] Transform kingTransform;
    [SerializeField] int initialSoldierCount = 3; // �ʱ� ���� ��
    [SerializeField] int maxSoldierCount = 10; // �ִ� ���� �� 

    private List<GameObject> spawnSoldier = new List<GameObject>();
    private int currentSoldierCount;
    private float scaleFactor = 0.1f; // �� ���� ������

    public override void OnNetworkSpawn() // ��Ʈ��ũ ������Ʈ �����Ǹ� ȣ��
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (IsServer) // Host ������ �Ʒ� �۾��� ���� 
        {
            Debug.Log("�������� ���� ����");
            InitializePool(); // ���� Ǯ �ʱ�ȭ
            currentSoldierCount = initialSoldierCount; // �ʱ� ���� �� ����
            SpawnSoldiersServerRpc(); // �ʱ� ���� ����
        }
    }
    // ���� Ǯ �ʱ�ȭ
    private void InitializePool()
    {
        //if (!IsServer) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject soldier = Instantiate(soldierPrefab); // ���� Ǯ ������ ��ŭ �̸� ����
            soldier.SetActive(false);

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn(); // ��Ʈ��ũ�� ����ȭ
                networkObject.Despawn(); // ��Ʈ��ũ�� ��Ȱ��ȭ
            }

            poolQueue.Enqueue(soldier); // Ǯ�� ����
        }
    }
    private GameObject GetFromPool()
    {
        //if (!IsServer) return null;

        if (poolQueue.Count > 0)
        {
            GameObject soldier = poolQueue.Dequeue(); // ���� ������ 
            soldier.SetActive(true); // ���� Ȱ��ȭ

            NetworkObject netObj = soldier.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned) 
            {
                netObj.Spawn(); // ��Ʈ��ũ�� ����ȭ
            }

            return soldier;
        }
        else // ���� Ǯ�� ���簡 ���ٸ�
        {
            GameObject soldier = Instantiate(soldierPrefab); // �� ���� ����
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

    // �ʱ� ���� ����
    [ServerRpc(RequireOwnership = false)]
    public void SpawnSoldiersServerRpc()
    {
        int formationIndex = 0;

        for (int i = 0; i < initialSoldierCount; i++)
        {
            GameObject soldier = SoldierSpawner.Instance.GetFromPool(); // Ǯ���� ���� ��������
            soldier.transform.position = GetTrianglePosition(formationIndex);  // ���� ��ġ ���
            soldier.transform.rotation = Quaternion.identity; // ���� ȸ�� 0

            Debug.Log($"[�ʱ� ���� ����] ���� {i} ��ġ: {soldier.transform.position}");

            NetworkObject networkObject = soldier.GetComponent<NetworkObject>(); 
            if (networkObject != null && !networkObject.IsSpawned)
            {
                networkObject.Spawn(); // ��Ʈ��ũ ����ȭ
            }
            // ���� �����̼� �ʱ�ȭ
            SoldierFormation formation = soldier.GetComponent<SoldierFormation>(); 
            if (formation != null)
            {
                formation.SoldierFormationInitialize(kingTransform, formationIndex);
            }
            // ���� �ʱ�ȭ
            IFormable formableSoldier = soldier.GetComponent<IFormable>(); 
            if (formableSoldier != null)
            {
                formableSoldier.SoldierInitialize(kingTransform, formationIndex);
                formationIndex++;
            }
            spawnSoldier.Add(soldier);
            // �ʱ� ���� �߰� 
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void AddSoldierServerRpc(int count)
    {
        if (!IsServer) return;

        if (count >= maxSoldierCount)
        {
            Debug.Log("���� �ִ�ġ");
            return;
        }

        GameObject soldier = SoldierSpawner.Instance.GetFromPool();
        soldier.transform.position = GetTrianglePosition(spawnSoldier.Count);
        soldier.transform.rotation = Quaternion.identity;

        NetworkObject networkObject = soldier.GetComponent<NetworkObject>();
        if (networkObject != null && !networkObject.IsSpawned)
        {
            networkObject.Spawn(); // ��� Ŭ���̾�Ʈ�� ����ȭ
        }

        SoldierFormation formation = soldier.GetComponent<SoldierFormation>();
        if (formation != null)
        {
            formation.SoldierFormationInitialize(kingTransform, spawnSoldier.Count);
        }

        IFormable formableSoldier = soldier.GetComponent<IFormable>(); // ���� �ʱ�ȭ
        if (formableSoldier != null)
        {
            formableSoldier.SoldierInitialize(kingTransform, spawnSoldier.Count);
        }

        // ������ ���� �߰�
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
    // ���� ��ġ
    public Vector3 GetTrianglePosition(int index)
    {
        float spacing = 1.5f * scaleFactor;
        Vector3 kingPosition = kingTransform.position;
        int row = (index / 2) + 1; // ��
        int col = index % 2 == 0 ? -1 : 1; // ��(¦���� ����, Ȧ���� ������)

        Vector3 forwardOffset = kingTransform.forward * -spacing * row; // �� ��
        Vector3 sideOffset = kingTransform.right * spacing * col * row; // �� �¿�

        return kingPosition + forwardOffset + sideOffset;
    }
    // Ư�� ���� ���� 
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