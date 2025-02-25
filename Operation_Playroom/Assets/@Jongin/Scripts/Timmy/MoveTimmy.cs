using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static Define;
public class MoveTimmy : NetworkBehaviour
{
    public List<Transform> path = new List<Transform>();
    ETimmyState timmyState = ETimmyState.Sleep;

    int pathIndex = 0;

    NavMeshAgent agent;
    Animator animator;
    bool isMove = false;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsServer) return;
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            MoveToPath(pathIndex);
            isMove = true;
            animator.SetTrigger("Walk");
        }

        if (isMove)
        {
            if (HasReachedDestination())
            {
                animator.SetTrigger("Lifting");
                //lifting animation 

                isMove = false;
            }
        }
    }

    void MoveToPath(int index)
    {
        GetComponent<NavMeshAgent>().SetDestination(path[index].position);
        pathIndex++;
    }

    bool HasReachedDestination()
    {
        // ������Ʈ�� ��θ� ������ �ְ�, ���� �Ÿ��� ���� �Ÿ����� ������ ������ ������ �Ǵ�
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
        }
        return false;
    }
}
