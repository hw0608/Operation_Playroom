using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    private float scaleFactor = 0.1f; // �� ���� ������
    [SerializeField] Transform king; // �� ���� ����
    [SerializeField] float followDistance = 0.15f; // �հ��� �Ÿ� 
    private NavMeshAgent navAgent;

    [SerializeField] int formationIndex; // ���� �ﰢ ���� ��ġ �ε���
    private Vector3 lastTargetPosition; // ������ ��ǥ ��ġ 

    [SerializeField] float attackRange = 0.15f; // ���� ����
    [SerializeField] int damage = 10; // ���ݷ�
    [SerializeField] float attackCooldown = 1.5f; // ���� ��Ÿ��
    private float lastAttackTime = 0f; // ������ ���� �ð�

    [SerializeField] Transform holdPoint; // ������ ��ġ
    
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
        
        if(distanceToKing > followDistance * 2) // �ʹ� �־����� �����̵�
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
    }
}
