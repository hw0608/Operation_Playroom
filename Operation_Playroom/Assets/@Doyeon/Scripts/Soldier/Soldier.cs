using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    private float scaleFactor = 0.1f; // 왕 병사 스케일
    [SerializeField] Transform king; // 왕 유닛 참조
    [SerializeField] float followDistance = 0.15f; // 왕과의 거리 
    private NavMeshAgent navAgent;

    [SerializeField] int formationIndex; // 병사 삼각 대형 위치 인덱스
    private Vector3 lastTargetPosition; // 마지막 목표 위치 

    [SerializeField] float attackRange = 0.15f; // 공격 범위
    [SerializeField] int damage = 10; // 공격력
    [SerializeField] float attackCooldown = 1.5f; // 공격 쿨타임
    private float lastAttackTime = 0f; // 마지막 공격 시간

    [SerializeField] Transform holdPoint; // 아이템 위치
    
    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // 네브메쉬 속성 조정
        navAgent.radius *= scaleFactor;
        navAgent.speed *= scaleFactor;
        navAgent.acceleration *= scaleFactor;
        navAgent.stoppingDistance = followDistance *0.05f;

        // 왕이 병사보다 NavMesh 충돌 우선순위가 높도록 설정
        navAgent.avoidancePriority = 60; // 50보다 높으면 우선순위가 더 높음

        if (king != null)
        {
            lastTargetPosition = GetTrianglePosition(formationIndex);
            navAgent.SetDestination(lastTargetPosition);
        }

        if (king.TryGetComponent(out NavMeshAgent kingAgent))
        {
            navAgent.speed = kingAgent.speed * 1.1f;
        }
    }
    private void Update()
    {
        if (king == null || navAgent == null || !navAgent.enabled) return;
        
        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);
        
        if(distanceToKing > followDistance * 2) // 너무 멀어지면 강제이동
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
        
        // 공격 범위 내 적 찾기 
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0 && Time.time -lastAttackTime > attackCooldown)
        {
            Attack(enemies[0].gameObject);
            lastAttackTime = Time.time;
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
                return kingPosition + forwardOffset ; // 뒤쪽 
            default:
                return kingPosition; // 기본 위치
        }
    }
    void Attack(GameObject enemy)
    {
        Debug.Log("Attack :" + enemy.name);
    }
}
