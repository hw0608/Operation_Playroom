using System.Collections.Generic;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    // ������ ������
    [SerializeField] GameObject occupyPrefab;

    // �������� ������ ��ġ ������Ʈ
    [SerializeField] List<GameObject> occupyPoints;

    void Start()
    {
        GenerateOccupy();
    }

    /// <summary>
    /// ������ ���� �Լ�
    /// </summary>
    void GenerateOccupy()
    {
        foreach (GameObject points in occupyPoints)
        {
            Instantiate(occupyPrefab, points.transform.position, Quaternion.identity);
        }
    }
}