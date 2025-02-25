using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : NetworkBehaviour
{
    private float scaleFactor = 0.1f; // 왕 병사 스케일
    [SerializeField] float followDistance = 0.15f; // 왕과의 거리 

    private NavMeshAgent navAgent;
    public Transform king; // 왕 유닛 참조
    public int formationIndex; // 병사 삼각 대형 위치 인덱스
    private Vector3 lastTargetPosition; // 마지막 목표 위치 

    bool isCarryingItem = false; // 아이템을 옮기고 있는가?

    private Soldier soldier;
    [SerializeField] private float itemCollectRange = 1.5f;  // 자원 수집 범위
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        soldier = GetComponent<Soldier>();
    }
    // 병사 대형 초기화
    public void SoldieFormationInitialize(Transform king, int formationIndex)
    {
        this.king = king;
        this.formationIndex = formationIndex;
    }
    // 왕 따라가기
    public void FollowKing()
    {
        if (king == null || navAgent == null || !navAgent.enabled) return;

        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > followDistance * 10) // 너무 멀어지면 강제이동
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
        }
        else
        {
            if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.5f) // 일정 거리 차이나면 이동
            {
                navAgent.SetDestination(targetPosition);
                lastTargetPosition = targetPosition;
            }
        }
        if (IsMoving())
        {
            soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        }
        else
        {
            soldier.GetComponent<SoldierAnim>().SoldierIdleAnim();
        }
    }
    public bool IsMoving() 
    {
        return navAgent.velocity.sqrMagnitude > 0.01f;
    }
    // 병사 위치
    private Vector3 GetTrianglePosition(int index)
    {
        //float spacing = 1.5f * scaleFactor; // 병사간 간격
        //Vector3 kingPosition = king.position;

        //Vector3 forwardOffset = king.forward * -spacing; // 왕 뒤쪽
        //Vector3 sideOffset = king.right * spacing; // 왕 좌우 

        //switch (index)
        //{
        //    case 0:
        //        return kingPosition - sideOffset; // 왼쪽
        //    case 1:
        //        return kingPosition + sideOffset; // 오른쪽
        //    case 2:
        //        return kingPosition + forwardOffset; // 뒤쪽 
        //    default:
        //        return kingPosition; // 기본 위치
        //}
        float spacing = 1.5f * scaleFactor;
        Vector3 kingPosition = king.position;
        int row = (index / 2) + 1; // 행
        int col = index % 2 == 0 ? -1 : 1; // 열(짝수면 왼쪽, 홀수면 오른쪽)

        Vector3 forwardOffset = king.forward * -spacing * row; // 왕 뒤
        Vector3 sideOffset = king.right * spacing * col * row; // 왕 좌우

        return kingPosition + forwardOffset + sideOffset;
    }
    [ServerRpc]
    // 자원 옮기기
    public void MoveToItemServerRpc(Vector3 itemPosition)
    {
        if (navAgent == null)
        {
            return;
        }
        navAgent.SetDestination(itemPosition);
        if (Vector3.Distance(transform.position, itemPosition) < itemCollectRange)
        {
            isCarryingItem = true;
            soldier.SetState(3); 
        }
    }
    [ServerRpc]
    public void ReturnToKingServerRpc(Vector3 kingPosition)
    {
        if (king == null) return;
        navAgent.SetDestination(kingPosition);

        if (Vector3.Distance(transform.position, kingPosition) < followDistance)
        {
            isCarryingItem = false; 
        }
    }
}
