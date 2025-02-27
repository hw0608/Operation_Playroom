using System;
using System.Collections;
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
            CallTimmy(null);
        }

        //if (isMove)
        //{
        //    if (HasReachedDestination())
        //    {
        //        animator.SetTrigger("Lifting");
        //        //lifting animation 

        //        isMove = false;
        //    }
        //}
    }

    public void CallTimmy(Action callback)
    {
        StartCoroutine(MoveTimmyToPath(callback));
    }

    IEnumerator MoveTimmyToPath(Action callback)
    {
        pathIndex = 0;
        while (pathIndex < path.Count)
        {
            if (!isMove)
            {
                MoveToPath(pathIndex);
                isMove = true;
                animator.SetTrigger("Walk");
            }
            else
            {
                if (HasReachedDestination())
                {
                    animator.SetTrigger("Lifting");
                    yield return new WaitForSeconds(6);
                    isMove = false;
                    pathIndex++;
                }
            }
            yield return null;
        }
        callback?.Invoke();
    }

    void MoveToPath(int index)
    {
        GetComponent<NavMeshAgent>().SetDestination(path[index].position);
    }

    bool HasReachedDestination()
    {
        // 에이전트가 경로를 가지고 있고, 남은 거리가 정지 거리보다 작으면 도착한 것으로 판단
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
        }
        return false;
    }
}
