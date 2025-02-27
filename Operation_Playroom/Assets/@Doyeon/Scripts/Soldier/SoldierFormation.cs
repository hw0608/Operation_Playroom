using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

// ������ ��ġ �� �̵� 
[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : NetworkBehaviour
{
    [SerializeField] float followDistance = 0.1f; // �հ��� �Ÿ� 
    [SerializeField] float warpDistance = 5.0f; // ���� �̵� �Ÿ�

    private NavMeshAgent navAgent;
    private Transform king; // �� ���� ����
    public int formationIndex; // ���� �ﰢ ���� ��ġ �ε���
    private Vector3 lastTargetPosition; // ������ ��ǥ ��ġ 

    bool isCarryingItem = false; // �������� �ű�� �ִ°�?

    private Soldier soldier;
    [SerializeField] private float itemCollectRange = 1.5f;  // �ڿ� ���� ����
    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        soldier = GetComponent<Soldier>();
    }
    private void Update()
    { 
        if (IsServer)
        {
            FollowKing();
        }
    }
    // ���� ���� �ʱ�ȭ
    public void SoldierFormationInitialize(Transform king, int formationIndex)
    {
        this.king = king;
        this.formationIndex = formationIndex;
    }
    // �� ���󰡱�
    public void FollowKing()
    {// �׺�޽� ���������� �̵��ϵ��� ����
        if (!IsServer || king == null || navAgent == null || !navAgent.enabled) return;

        // �� ��ǥ üũ
        Debug.Log($"�� ���󰡴� ���� = {name}, �� ��ġ ={king.position}");

        Vector3 directionToKing = (king.position - transform.position).normalized;
        Vector3 targetPosition = king.position -(directionToKing * followDistance);
       

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > warpDistance) // �ʹ� �־����� �����̵�
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"���� Warp : {targetPosition}");
        }
        
     
        if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.1f) // ���� �Ÿ� ���̳��� �̵�
        {
            navAgent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"���� Destination : {targetPosition}");
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
    
    [ServerRpc]
    // �ڿ� �ű��
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
