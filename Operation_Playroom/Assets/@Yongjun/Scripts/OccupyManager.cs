using Unity.Netcode;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    [SerializeField] GameObject occupyPrefab; // ������ ������

    [SerializeField] Transform occupyPoints; // ������ ��ġ��
    [SerializeField] Transform occupyPool; // ������ ��ġ��

    void Start()
    {
        GenerateOccupy();
        Destroy(occupyPoints);
    }

    void GenerateOccupy() // ������ ��ġ�� ������ ����
    {
        foreach (Transform child in occupyPoints.transform)
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