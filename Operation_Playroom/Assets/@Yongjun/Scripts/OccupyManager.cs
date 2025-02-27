using UnityEngine;
using Unity.Netcode;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab; // ������ ������
    [SerializeField] Transform occupyPoints; // ������ ��ġ��
    [SerializeField] Transform occupyPool;   // ������ Ǯ

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
