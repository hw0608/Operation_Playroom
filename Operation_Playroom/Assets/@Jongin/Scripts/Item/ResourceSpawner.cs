using System.Collections;
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
                InitSpawnResource(initSpawnCount);
                StartCoroutine(SpawnResourceRoutine());
            }
        }
    }
    Collider[] itemBuffer = new Collider[1];
    public void InitSpawnResource(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnResource();
        }
    }

    IEnumerator SpawnResourceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (currentSpawnCount < initSpawnCount)
            {
                SpawnResource();
            }
        }
    }

    Vector3 GetRandomSpawnPos()
    {
        int attempts = 0; // 시도 횟수를 추적하는 변수

        while (100 > attempts)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                int numColliders = Physics.OverlapSphereNonAlloc(hit.position, 0.5f, itemBuffer, layerMask);
                if (numColliders == 0)
                {
                    return hit.position;
                }
                else
                {
                    attempts++;
                }
            }
        }

        return Vector3.zero;
    }

    [ClientRpc]
    private void NotifyResourceSpawnedClientRpc(ulong networkObjectId, int randInt)
    {
        GameObject resourceObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
        resourceObject.SetActive(true);
        StartCoroutine(DelayActiveChild(resourceObject, randInt));
    }
    public void SpawnResource()
    {
        Vector3 randomPos = GetRandomSpawnPos();
        if (randomPos == Vector3.zero) return;

        int randInt = Random.Range(0, 3);
        GameObject go = Managers.Resource.Instantiate("ResourcePrefab", null, true);
        if (go.transform.parent != resourceParent)
            go.GetComponent<NetworkObject>().TrySetParent(resourceParent, true);

        go.transform.position = new Vector3(randomPos.x, 0, randomPos.z);
        //for (int j = 0; j < go.transform.childCount; j++) 
        //{
        //    go.transform.GetChild(j).gameObject.SetActive(false);
        //}
        //go.transform.GetChild(randInt).gameObject.SetActive(true);
        NotifyResourceSpawnedClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId, randInt);
        currentSpawnCount++;
    }

    IEnumerator DelayActiveChild(GameObject go, int activeChildIndex)
    {
        yield return new WaitForSeconds(0.5f);
        // 자식 객체들 중 지정된 인덱스만 활성화
        for (int i = 0; i < go.transform.childCount; i++)
        {
            go.transform.GetChild(i).gameObject.SetActive(i == activeChildIndex);
        }
        go.GetComponent<ResourceData>().resourceCollider.enabled = true;
    }
}
