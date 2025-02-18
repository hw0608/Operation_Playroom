using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    public Transform king; // 왕 유닛 참조
    public float followDistance = 2.0f; // 왕과의 거리 유지 
    private NavMeshAgent navAgent;
    private Queue<Vector3> kingPositions = new Queue<Vector3>();
    public int followDelay = 10; // 병사가 따라오는 지연 프레임 

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        // 왕의 위치 저장
        if (kingPositions.Count > followDelay)
        {
            navAgent.SetDestination(kingPositions.Dequeue());
        }
        kingPositions.Enqueue(transform.position);
    }
}
