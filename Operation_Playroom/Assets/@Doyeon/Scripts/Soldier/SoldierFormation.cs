using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : NetworkBehaviour
{
    private float scaleFactor = 0.1f; // �� ���� ������
    [SerializeField] float followDistance = 0.15f; // �հ��� �Ÿ� 

    private NavMeshAgent navAgent;
    public Transform king; // �� ���� ����
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
    // ���� ���� �ʱ�ȭ
    public void SoldieFormationInitialize(Transform king, int formationIndex)
    {
        this.king = king;
        this.formationIndex = formationIndex;
    }
    // �� ���󰡱�
    public void FollowKing()
    {
        if (king == null || navAgent == null || !navAgent.enabled) return;

        Vector3 targetPosition = GetTrianglePosition(formationIndex);

        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > followDistance * 10) // �ʹ� �־����� �����̵�
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
        }
        else
        {
            if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.5f) // ���� �Ÿ� ���̳��� �̵�
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
    // ���� ��ġ
    private Vector3 GetTrianglePosition(int index)
    {
        //float spacing = 1.5f * scaleFactor; // ���簣 ����
        //Vector3 kingPosition = king.position;

        //Vector3 forwardOffset = king.forward * -spacing; // �� ����
        //Vector3 sideOffset = king.right * spacing; // �� �¿� 

        //switch (index)
        //{
        //    case 0:
        //        return kingPosition - sideOffset; // ����
        //    case 1:
        //        return kingPosition + sideOffset; // ������
        //    case 2:
        //        return kingPosition + forwardOffset; // ���� 
        //    default:
        //        return kingPosition; // �⺻ ��ġ
        //}
        float spacing = 1.5f * scaleFactor;
        Vector3 kingPosition = king.position;
        int row = (index / 2) + 1; // ��
        int col = index % 2 == 0 ? -1 : 1; // ��(¦���� ����, Ȧ���� ������)

        Vector3 forwardOffset = king.forward * -spacing * row; // �� ��
        Vector3 sideOffset = king.right * spacing * col * row; // �� �¿�

        return kingPosition + forwardOffset + sideOffset;
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
