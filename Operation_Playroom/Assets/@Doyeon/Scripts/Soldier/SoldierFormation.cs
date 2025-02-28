using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

// 병사의 위치 및 이동 
[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : NetworkBehaviour
{
    [SerializeField] float followDistance = 0.2f; // 왕과의 거리 
    [SerializeField] float warpDistance = 5.0f; // 순간 이동 거리

    private NavMeshAgent navAgent;
    private Transform king; // 왕 유닛 참조
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
    private void Update()
    {
        //if (IsServer)
        //{ }
            //Debug.Log($"왕 따라가는 병사 = {name}{transform.position}, 왕 위치 = {king.position}");
            FollowKing();
        
        
    }
    // 병사 대형 초기화
    public void SoldierFormationInitialize(Transform king, int formationIndex)
    {
        this.king = king;
        this.formationIndex = formationIndex;
    }
    // 왕 따라가기
    public void FollowKing()
    {// 네브메쉬 서버에서만 이동하도록 설정
        if (king == null || navAgent == null || !navAgent.enabled) return;

        // 왕 좌표 체크
        //Debug.Log($"왕 따라가는 병사 = {name}, 왕 위치 ={king.position}");

        Vector3 directionToKing = (king.position - transform.position).normalized;
        Vector3 targetPosition = king.position -(directionToKing * followDistance);
       

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > warpDistance) // 너무 멀어지면 강제이동
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"병사 Warp : {targetPosition}");
        }
        
     
        if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.1f) // 일정 거리 차이나면 이동
        {
            navAgent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"병사 Destination : {targetPosition}");
        }
       
        if (IsMoving())
        {
            Debug.Log("병사 걷는 중");
            soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        }
        else
        {
            Debug.Log("병사 Idle 중");
            soldier.GetComponent<SoldierAnim>().SoldierIdleAnim();
        }
    }
    public bool IsMoving() 
    {
        Debug.Log("움직임");
        //return navAgent.velocity.magnitude > 0.01f;
        return !navAgent.isStopped;
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
