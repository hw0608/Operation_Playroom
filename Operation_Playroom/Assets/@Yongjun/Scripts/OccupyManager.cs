using System.Collections.Generic;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    // 점령지 프리팹
    [SerializeField] GameObject occupyPrefab;

    // 점령지로 지정할 위치 오브젝트
    [SerializeField] List<GameObject> occupyPoints;

    void Start()
    {
        GenerateOccupy();
    }

    /// <summary>
    /// 점령지 생성 함수
    /// </summary>
    void GenerateOccupy()
    {
        foreach (GameObject points in occupyPoints)
        {
            Instantiate(occupyPrefab, points.transform.position, Quaternion.identity);
        }
    }
}