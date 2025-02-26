using UnityEngine;
using Unity.Netcode;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab; // 점령지 프리팹
    [SerializeField] Transform occupyPoints; // 점령지 위치들
    [SerializeField] Transform occupyPool;   // 점령지 풀

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateOccupy();
        }
    }

    private void GenerateOccupy()
    {
        foreach (Transform child in occupyPoints)
        {
            GameObject occupyInstance = Instantiate(occupyPrefab, child.position, Quaternion.identity);
            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(true);
            }

            occupyInstance.transform.SetParent(occupyPool);
        }
    }
}
