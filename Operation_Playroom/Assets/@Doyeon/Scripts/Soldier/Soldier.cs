using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    public Transform king; // 왕 유닛 참조
    public float followDistance = 2.0f; // 왕과의 거리 유지 
    private NavMeshAgent navAgent;
    private Queue<Vector3> kingPositions = new Queue<Vector3>(); // 왕의 이동 경로를 저장할 큐
    public int followDelay = 10; // 병사가 따라오는 지연 프레임 

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        // 왕 위치 큐에 저장
        if (kingPositions.Count > followDelay)
        {
            navAgent.SetDestination(kingPositions.Dequeue()); // 병사 과거 위치 따라감
        }
        kingPositions.Enqueue(transform.position); // 현재 왕 위치 저장
    }
}
