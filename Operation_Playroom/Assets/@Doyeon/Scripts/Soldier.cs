using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    public Transform king; // �� ���� ����
    public float followDistance = 2.0f; // �հ��� �Ÿ� ���� 
    private NavMeshAgent navAgent;
    private Queue<Vector3> kingPositions = new Queue<Vector3>();
    public int followDelay = 10; // ���簡 ������� ���� ������ 

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        // ���� ��ġ ����
        if (kingPositions.Count > followDelay)
        {
            navAgent.SetDestination(kingPositions.Dequeue());
        }
        kingPositions.Enqueue(transform.position);
    }
}
