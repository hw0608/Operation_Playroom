using UnityEngine;
using UnityEngine.AI;

public class SoldierFormation : MonoBehaviour
{
    public Transform King;
    public Vector3 formationOffset; // ���纰 ��ġ ������
    private NavMeshAgent navAgent;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        Vector3 targetPosition = King.position + King.transform.TransformDirection(formationOffset);
        navAgent.SetDestination(targetPosition);
    }
}
