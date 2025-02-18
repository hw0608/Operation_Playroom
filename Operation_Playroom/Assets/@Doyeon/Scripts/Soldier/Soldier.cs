using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    public Transform king; // �� ���� ����
    public float followDistance = 2.0f; // �հ��� �Ÿ� ���� 
    private NavMeshAgent navAgent;
    private Queue<Vector3> kingPositions = new Queue<Vector3>(); // ���� �̵� ��θ� ������ ť
    public int followDelay = 10; // ���簡 ������� ���� ������ 

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        // �� ��ġ ť�� ����
        if (kingPositions.Count > followDelay)
        {
            navAgent.SetDestination(kingPositions.Dequeue()); // ���� ���� ��ġ ����
        }
        kingPositions.Enqueue(transform.position); // ���� �� ��ġ ����
    }
}
