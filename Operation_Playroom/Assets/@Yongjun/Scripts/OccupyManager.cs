using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    [SerializeField] GameObject occupyPrefab; // 점령지 프리팹
    [SerializeField] GameObject occupyPoints; // 점령지 위치
    [SerializeField] GameObject occupyPool; // 점령지 풀

    void Start()
    {
        GenerateOccupy();
        Destroy(occupyPoints);
    }

    void GenerateOccupy() // 점령지 위치에 프리팹 생성
    {
        foreach (Transform child in occupyPoints.transform)
        {
            GameObject occupyInstance = Instantiate(occupyPrefab, child.position, Quaternion.identity);
            occupyInstance.transform.SetParent(occupyPool.transform);
        }
    }
}