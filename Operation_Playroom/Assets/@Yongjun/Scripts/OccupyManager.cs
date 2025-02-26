using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    [SerializeField] GameObject occupyPrefab; // ������ ������
    [SerializeField] GameObject occupyPoints; // ������ ��ġ
    [SerializeField] GameObject occupyPool; // ������ Ǯ

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
            occupyInstance.transform.SetParent(occupyPool.transform);
        }
    }
}