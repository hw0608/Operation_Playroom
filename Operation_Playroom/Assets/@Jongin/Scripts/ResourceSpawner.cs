using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] GameObject resourcePrefab;
    [SerializeField] int initSpawnCount;
    [SerializeField] LayerMask layerMask;

    public int currentSpawnCount;
    public override void OnNetworkSpawn()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (IsServer)
                SpawnResourceRandomPos();
        }
    }
    Collider[] itemBuffer = new Collider[1];
    public void SpawnResourceRandomPos()
    {
        int j = 0;
        int attempts = 0; // �õ� Ƚ���� �����ϴ� ����

        while (j < initSpawnCount)
        {
            // ������ ��ġ�� �����մϴ�. (�� �������� ���� ��ǥ�� ������ �°� ����)
            Vector3 randomPosition = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));

            // NavMesh���� �ش� ��ġ�� ��ȿ���� Ȯ���ϰ�, ��ȿ�ϸ� �ش� ��ġ�� ��ȯ�մϴ�.
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                int numColliders = Physics.OverlapSphereNonAlloc(hit.position, 0.5f, itemBuffer, layerMask);
                if (numColliders == 0)
                {
                    int randInt = Random.Range(0, 3);
                    GameObject go = Instantiate(resourcePrefab, new Vector3(hit.position.x, 1, hit.position.z), Quaternion.identity);
                    go.GetComponent<NetworkObject>().Spawn(true);
                    for (int i = 0; i < resourcePrefab.transform.childCount; i++)
                    {
                        go.transform.GetChild(i).gameObject.SetActive(false);
                    }
                    go.transform.GetChild(randInt).gameObject.SetActive(true);

                    NotifyResourceSpawnedClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, randInt);
                    j++; 
                    attempts = 0; // �ڿ� ��ġ ���� �� �õ� Ƚ�� �ʱ�ȭ
                }
                else
                {
                    attempts++;
                }
            }

            // 10�� �̻� �õ��ص� ��ȿ�� ��ġ�� ã�� ���ϸ� ������ ����
            if (attempts >= 100)
            {
                break;
            }
        }
    }
    [ClientRpc]
    private void NotifyResourceSpawnedClientRpc(ulong networkObjectId, int activeChildIndex)
    {
        GameObject resourceObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;

        // �ڽ� ��ü�� �� ������ �ε����� Ȱ��ȭ
        for (int i = 0; i < resourceObject.transform.childCount; i++)
        {
            resourceObject.transform.GetChild(i).gameObject.SetActive(i == activeChildIndex);
        }
    }
}
