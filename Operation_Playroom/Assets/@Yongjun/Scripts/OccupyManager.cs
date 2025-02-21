using System.Collections.Generic;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    // 점령지 프리팹
    [SerializeField] GameObject occupyPrefab;

    // 점령지로 지정할 위치 오브젝트
    [SerializeField] List<Transform> occupyPoints;

    void Start()
    {
        GenerateOccupy();
    }

    /// <summary>
    /// 점령지 생성 함수
    /// </summary>
    void GenerateOccupy()
    {
        foreach (Transform points in occupyPoints)
        {
            Instantiate(occupyPrefab, points.position, Quaternion.identity);
        }
    }
}