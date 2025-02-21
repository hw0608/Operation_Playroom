using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : NetworkBehaviour
{
    enum State
    {
        Idle,
        Walk,
        Attack,
        hold
    }
    private float scaleFactor = 0.1f; // 왕 병사 스케일
    public Transform king; // 왕 유닛 참조
    [SerializeField] float followDistance = 0.15f; // 왕과의 거리 
    private NavMeshAgent navAgent;

    public int formationIndex; // 병사 삼각 대형 위치 인덱스
    private Vector3 lastTargetPosition; // 마지막 목표 위치 

    [SerializeField] float attackRange = 0.15f; // 공격 범위
    [SerializeField] int damage = 10; // 공격력
    [SerializeField] float attackCooldown = 1.5f; // 공격 쿨타임
    private float lastAttackTime = 0f; // 마지막 공격 시간

    [SerializeField] Transform holdPoint; // 아이템 위치

    private NetworkVariable<State> networkState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    State state;
    Animator animator;
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
        // 이 부분 수정
        if (king.TryGetComponent(out NavMeshAgent kingAgent))
        {
            navAgent.speed = kingAgent.speed * 1.1f;
        }

        state = State.Idle;
        animator = GetComponent<Animator>();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkState.Value = State.Idle;
        }
        base.OnNetworkSpawn();
        if (IsServer && king == null)
        {
            // NetworkObject가 동기화된 후에 왕을 할당
            king = GameObject.FindFirstObjectByType<TestController>().transform;
        }
    }
    private void Update()
    {
        if (!IsServer) return;

        if (king == null || navAgent == null || !navAgent.enabled) return;
        
        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        // Idle 애니메이션
        if (navAgent.velocity.magnitude < 0.01f)
        {
            UpdateSoldierAnim(State.Idle);
        }
        // Move 애니메이션
        else if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.05f)
        {
            UpdateSoldierAnim(State.Walk);
            navAgent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
        }
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
        
        // 공격 범위 내 적 찾기 
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0 && Time.time -lastAttackTime > attackCooldown)
        {
            
            Attack(enemies[0].gameObject);
            lastAttackTime = Time.time;
        }
        // 상태 변경을 서버에서만
        if (IsServer)
        {
            networkState.Value = state;
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

        //if (IsServer)
        //{
        //    networkState.Value = State.Attack;
        //    if (enemy.TryGetComponent(out Health enemyHealth))
        //    {
        //        enemyHealth.TakeDamage(damage);
        //    }
        //}
    }
    public void AttackEnded()
    {
        SetState(State.Idle);
    }
    
    void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        if (IsServer)
        {
            networkState.Value = state;
        }
    }
    void UpdateSoldierAnim(State currentState)
    {
        switch (currentState)
        {
            case State.Idle:
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
                break;
            case State.Walk:
                animator.SetBool("Idle", false);
                animator.SetBool("Walk", true);
                break;
            case State.Attack:
                animator.SetTrigger("Attack");
                break;
        }
    }
}
