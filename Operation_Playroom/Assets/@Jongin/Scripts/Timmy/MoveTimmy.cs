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
    public NetworkVariable<bool> timmyActive = new NetworkVariable<bool>(true);
    public GameObject[] BuildingDummy;

    ETimmyState timmyState = ETimmyState.Sleep;

    int pathIndex = 0;

    NavMeshAgent agent;
    Animator animator;
    bool isMove = false;

    Vector3 startPos;
    Quaternion startRot;
    private void Start()
    {
        timmyActive.OnValueChanged += OnSetActiveSelf;
        gameObject.SetActive(false);
    }
    public void OnSetActiveSelf(bool oldValue, bool newValue)
    {
        gameObject.SetActive(newValue);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        timmyActive.Value = false;
        startPos = transform.position;
        startRot = transform.rotation;

    }

    public void ResetTimmy()
    {
        transform.position = startPos;
        transform.rotation = startRot;
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
                    yield return new WaitForSeconds(1.5f);
                    SetBuildingDummyClientRpc(pathIndex);
                    yield return new WaitForSeconds(4.5f);
                    isMove = false;
                    pathIndex++;
                }
            }
            yield return null;
        }
        ResetBuildingDummyClientRpc();
        callback?.Invoke();
    }

    [ClientRpc]
    void SetBuildingDummyClientRpc(int pathIndex)
    {
        BuildingDummy[pathIndex].SetActive(true);
    } 

    [ClientRpc]
    void ResetBuildingDummyClientRpc()
    {
        for(int i = 0; i< BuildingDummy.Length; i++)
        {
            BuildingDummy[i].SetActive(false);
        }
    }

    void MoveToPath(int index)
    {
        agent.SetDestination(path[index].position);
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
