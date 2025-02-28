using UnityEngine;
using Unity.Netcode;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab; // ������ ������
    [SerializeField] Transform occupyPoints; // ������ ��ġ��
    [SerializeField] Transform occupyPool; // ������ ��ġ��

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
            occupyInstance.transform.SetParent(occupyPool, false);

            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(true);
                networkObject.TrySetParent(occupyPool.GetComponent<NetworkObject>());
            }
        }
    }
}