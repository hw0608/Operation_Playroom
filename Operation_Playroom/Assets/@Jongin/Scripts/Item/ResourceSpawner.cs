using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] GameObject resourceParent;
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
            {
                SpawnResourceRandomPos(initSpawnCount);
            }
        }
    }
    Collider[] itemBuffer = new Collider[1];
    public void SpawnResourceRandomPos(int count)
    {
        int j = 0;
        int attempts = 0; // �õ� Ƚ���� �����ϴ� ����

        while (j < count)
        {
            // ������ ��ġ�� �����մϴ�. (�� �������� ���� ��ǥ�� ������ �°� ����)
            Vector3 randomPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));

            // NavMesh���� �ش� ��ġ�� ��ȿ���� Ȯ���ϰ�, ��ȿ�ϸ� �ش� ��ġ�� ��ȯ�մϴ�.
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                int numColliders = Physics.OverlapSphereNonAlloc(hit.position, 0.5f, itemBuffer, layerMask);
                if (numColliders == 0)
                {
                    int randInt = Random.Range(0, 3);
                    //GameObject go = Instantiate(resourcePrefab);
                    GameObject go = Managers.Resource.Instantiate("ResourcePrefab",null, true);
                    go.GetComponent<NetworkObject>().TrySetParent(resourceParent,true);

                    //go.GetComponent<NetworkObject>().Spawn(true);
                    go.transform.position = new Vector3(hit.position.x, 0, hit.position.z);
                    for (int i = 0; i < go.transform.childCount; i++)
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

    public void RespawnResource()
    {
        SpawnResourceRandomPos(1);
    }
}
