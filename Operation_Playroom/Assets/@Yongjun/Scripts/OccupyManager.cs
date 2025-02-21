using System.Collections.Generic;
using UnityEngine;

public class OccupyManager : MonoBehaviour
{
    // ������ ������
    [SerializeField] GameObject occupyPrefab;

    // �������� ������ ��ġ ������Ʈ
    [SerializeField] List<Transform> occupyPoints;

    void Start()
    {
        GenerateOccupy();
    }

    /// <summary>
    /// ������ ���� �Լ�
    /// </summary>
    void GenerateOccupy()
    {
        foreach (Transform points in occupyPoints)
        {
            Instantiate(occupyPrefab, points.position, Quaternion.identity);
        }
    }
}