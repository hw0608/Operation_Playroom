using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    [SerializeField] GameObject occupyPrefab; // ������ ������
<<<<<<< HEAD
    [SerializeField] GameObject occupyPoints; // ������ ��ġ
    [SerializeField] GameObject occupyPool; // ������ Ǯ
=======
    [SerializeField] Transform occupyPoints; // ������ ��ġ��
    [SerializeField] Transform occupyPool; // ������ ��ġ��
>>>>>>> yj

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
<<<<<<< HEAD
            occupyInstance.transform.SetParent(occupyPool.transform);
=======
            occupyInstance.transform.SetParent(occupyPool, false);

            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn(true);
                networkObject.TrySetParent(occupyPool.GetComponent<NetworkObject>());
            }
>>>>>>> yj
        }
    }
}