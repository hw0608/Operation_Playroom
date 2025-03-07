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
        int attempts = 0; // 시도 횟수를 추적하는 변수

        while (j < count)
        {
            // 임의의 위치를 선택합니다. (이 예에서는 월드 좌표의 범위에 맞게 설정)
            Vector3 randomPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));

            // NavMesh에서 해당 위치가 유효한지 확인하고, 유효하면 해당 위치를 반환합니다.
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
                    attempts = 0; // 자원 배치 성공 시 시도 횟수 초기화
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

        // 자식 객체들 중 지정된 인덱스만 활성화
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
