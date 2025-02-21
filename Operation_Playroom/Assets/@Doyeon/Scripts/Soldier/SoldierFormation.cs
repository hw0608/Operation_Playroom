using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : MonoBehaviour
{
    private float scaleFactor = 0.1f; // 왕 병사 스케일
    public Transform king; // 왕 유닛 참조
    [SerializeField] float followDistance = 0.15f; // 왕과의 거리 
    private NavMeshAgent navAgent;

    public int formationIndex; // 병사 삼각 대형 위치 인덱스
    private Vector3 lastTargetPosition; // 마지막 목표 위치 

    public void SoldierInitialize(Transform king, int formationIndex)
    {
        this.king = king;
        this.formationIndex = formationIndex;

        navAgent.radius *= scaleFactor;
        navAgent.speed *= scaleFactor;
        navAgent.acceleration *= scaleFactor;
        navAgent.stoppingDistance = followDistance * 0.05f;

        if (king != null)
        {
            lastTargetPosition = GetTrianglePosition(formationIndex);
            navAgent.SetDestination(lastTargetPosition);
        }
    }
    public void FollowKing()
    {
        if (king == null || navAgent == null || !navAgent.enabled) return;

        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > followDistance * 2) // 너무 멀어지면 강제이동
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
        }
        else
        {
            if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.05f) // 일정 거리 차이나면 이동
            {
                navAgent.SetDestination(targetPosition);
                lastTargetPosition = targetPosition;
            }
        }
    }
    private Vector3 GetTrianglePosition(int index)
    {
        float spacing = 1.5f * scaleFactor; // 병사간 간격
        Vector3 kingPosition = king.position;
        Vector3 forwardOffset = king.forward * -spacing * 1f; // 왕 뒤쪽
        Vector3 sideOffset = king.right * spacing; // 왕 좌우 

        switch (index)
        {
            case 0:
                return kingPosition - sideOffset; // 왼쪽
            case 1:
                return kingPosition + sideOffset; // 오른쪽
            case 2:
                return kingPosition + forwardOffset; // 뒤쪽 
            default:
                return kingPosition; // 기본 위치
        }
    }
}
