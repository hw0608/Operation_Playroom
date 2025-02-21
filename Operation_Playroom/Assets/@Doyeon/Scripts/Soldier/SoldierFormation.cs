using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SoldierFormation : MonoBehaviour
{
    private float scaleFactor = 0.1f; // �� ���� ������
    public Transform king; // �� ���� ����
    [SerializeField] float followDistance = 0.15f; // �հ��� �Ÿ� 
    private NavMeshAgent navAgent;

    public int formationIndex; // ���� �ﰢ ���� ��ġ �ε���
    private Vector3 lastTargetPosition; // ������ ��ǥ ��ġ 

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
                return kingPosition + forwardOffset; // ���� 
            default:
                return kingPosition; // �⺻ ��ġ
        }
    }
}
