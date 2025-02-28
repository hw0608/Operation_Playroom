using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    [SerializeField] GameObject occupyPrefab; // 점령지 프리팹
<<<<<<< HEAD
    [SerializeField] GameObject occupyPoints; // 점령지 위치
    [SerializeField] GameObject occupyPool; // 점령지 풀
=======
    [SerializeField] Transform occupyPoints; // 점령지 위치들
    [SerializeField] Transform occupyPool; // 점령지 위치들
>>>>>>> yj

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