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
    private float scaleFactor = 0.1f; // �� ���� ������
    public Transform king; // �� ���� ����
    [SerializeField] float followDistance = 0.15f; // �հ��� �Ÿ� 
    private NavMeshAgent navAgent;

    public int formationIndex; // ���� �ﰢ ���� ��ġ �ε���
    private Vector3 lastTargetPosition; // ������ ��ǥ ��ġ 

    [SerializeField] float attackRange = 0.15f; // ���� ����
    [SerializeField] int damage = 10; // ���ݷ�
    [SerializeField] float attackCooldown = 1.5f; // ���� ��Ÿ��
    private float lastAttackTime = 0f; // ������ ���� �ð�

    [SerializeField] Transform holdPoint; // ������ ��ġ

    private NetworkVariable<State> networkState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    State state;
    Animator animator;
    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // �׺�޽� �Ӽ� ����
        navAgent.radius *= scaleFactor;
        navAgent.speed *= scaleFactor;
        navAgent.acceleration *= scaleFactor;
        navAgent.stoppingDistance = followDistance *0.05f;

        // ���� ���纸�� NavMesh �浹 �켱������ ������ ����
        navAgent.avoidancePriority = 60; // 50���� ������ �켱������ �� ����

        if (king != null)
        {
            lastTargetPosition = GetTrianglePosition(formationIndex);
            navAgent.SetDestination(lastTargetPosition);
        }
        // �� �κ� ����
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
            // NetworkObject�� ����ȭ�� �Ŀ� ���� �Ҵ�
            king = GameObject.FindFirstObjectByType<TestController>().transform;
        }
    }
    private void Update()
    {
        if (!IsServer) return;

        if (king == null || navAgent == null || !navAgent.enabled) return;
        
        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        // Idle �ִϸ��̼�
        if (navAgent.velocity.magnitude < 0.01f)
        {
            UpdateSoldierAnim(State.Idle);
        }
        // Move �ִϸ��̼�
        else if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.05f)
        {
            UpdateSoldierAnim(State.Walk);
            navAgent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
        }
        if (distanceToKing > followDistance * 2) // �ʹ� �־����� �����̵�
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
        }
        else
        {
            if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.05f) // ���� �Ÿ� ���̳��� �̵�
            {
                navAgent.SetDestination(targetPosition);
                lastTargetPosition = targetPosition;
            }
        }
        
        // ���� ���� �� �� ã�� 
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0 && Time.time -lastAttackTime > attackCooldown)
        {
            
            Attack(enemies[0].gameObject);
            lastAttackTime = Time.time;
        }
        // ���� ������ ����������
        if (IsServer)
        {
            networkState.Value = state;
        }
    }
    private Vector3 GetTrianglePosition(int index)
    {
        float spacing = 1.5f * scaleFactor; // ���簣 ����
        Vector3 kingPosition = king.position;
        Vector3 forwardOffset = king.forward * -spacing * 1f; // �� ����
        Vector3 sideOffset = king.right * spacing; // �� �¿� 

        switch (index)
        {
            case 0:
                return kingPosition - sideOffset; // ����
            case 1:
                return kingPosition + sideOffset; // ������
            case 2:
                return kingPosition + forwardOffset ; // ���� 
            default:
                return kingPosition; // �⺻ ��ġ
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
