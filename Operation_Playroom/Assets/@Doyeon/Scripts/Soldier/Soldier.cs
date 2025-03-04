using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;


public enum SoldierState
{
    Idle, 
    Following,
    Attacking,
    Collecting,
    Returning,
    Dying
}

public class Soldier : Character
{
    NetworkVariable<SoldierState> currentState = new NetworkVariable<SoldierState>(SoldierState.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<SoldierState> CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
        }
    }

    public NetworkVariable<int> formationIndex = new NetworkVariable<int>(
    0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
);

    // 병사 행동 관련 컴포넌트
    private SoldierFormation soldierFormation;
    private SoldierAttack soldierAttack;
    private SoldierAnim soldierAnim;

    // 상태 및 HP 관리
    public NetworkVariable<int> state = new NetworkVariable<int>();
    public new NetworkVariable<float> maxHp = new NetworkVariable<float>(80f);
    public new NetworkVariable<float> currentHp = new NetworkVariable<float>(80f);

    // 아이템 타겟, 팀 정보
    public NetworkVariable<NetworkObjectReference> itemTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<NetworkObjectReference> enemyTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<int> team = new NetworkVariable<int>();

    [SerializeField] float followDistance = 0.2f; // 왕과의 거리 
    [SerializeField] float warpDistance = 5.0f; // 순간 이동 거리
    
    private Vector3 lastTargetPosition; // 마지막 목표 위치 

    private NavMeshAgent navAgent;
    public Transform king;
    public bool isSoldierDie = false;
    

    
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[Soldier] 네트워크 스폰 완료! ID: {NetworkObjectId}");
        if (!IsOwner) return;
        navAgent = GetComponent<NavMeshAgent>();
        SoldierInitialize(king, formationIndex.Value);
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[Soldier] NavMeshAgent가 없음!");
            return;
        }
        //// 아이템 타겟 확인 후 동기화
        //NetworkObject itemNetObj;
        //if (itemTarget.Value.TryGet(out itemNetObj) && !object.ReferenceEquals(itemNetObj, null))
        //{
        //    itemTarget.Value = new NetworkObjectReference(itemNetObj);
        //}
        //// 팀 배정 요청
        //AssignTeamServerRpc();
        navAgent.enabled = true;
        Debug.Log("[Soldier] NavMeshAgent 활성화 완료!");
    }

    private void Update()
    {
        
        FollowKing();
        Debug.Log($"[Soldier] Update 실행 중... King 위치: {king?.position}");

        //if (IsOwner && currentState != null)
        //{
        //    Transform enemyTransform = FindEnemies();
        //    if (enemyTransform != null && !(currentState is AttackingState))
        //    {
        //        ChangeState(new AttackingState(enemyTransform));
        //    }
        //    else if (soldierFormation.IsMoving() && !(currentState is FollowingState))
        //    {
        //        ChangeState(new FollowingState(king));
        //    }
        //    else if (!soldierFormation.IsMoving() && !(currentState is IdleState))
        //    {
        //        ChangeState(new IdleState());
        //    }

        //    currentState.Execute(this);
        //}

        //else if (currentState == null)
        //{
        //    currentState = NSoldierState.GetStateFromInt(state.Value, king, itemTarget.Value, team.Value);
        //    currentState.EnterState(this);
        //}
        if (isSoldierDie) return;
    }
    public void FollowKing()
    {// 네브메쉬 서버에서만 이동하도록 설정
        if (king == null || navAgent == null || !navAgent.enabled) return;

        Vector3 directionToKing = (king.position - transform.position).normalized;
        Vector3 targetPosition = king.position - (directionToKing * followDistance);


        float distanceToKing = Vector3.Distance(transform.position, king.position);

        if (distanceToKing > warpDistance) // 너무 멀어지면 강제이동
        {
            navAgent.Warp(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"병사 Warp : {targetPosition}");
        }


        if (Vector3.Distance(lastTargetPosition, targetPosition) > 0.1f) // 일정 거리 차이나면 이동
        {
            navAgent.SetDestination(targetPosition);
            lastTargetPosition = targetPosition;
            Debug.Log($"병사 Destination : {targetPosition}");
            GetComponent<SoldierAnim>().SoldierWalkAnim();
        }

        //if (IsMoving())
        //{
        //    Debug.Log("병사 걷는 중");
        //    soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        //}
        //else
        //{
        //    Debug.Log("병사 Idle 중");
        //    soldier.GetComponent<SoldierAnim>().SoldierIdleAnim();
        //}
    }
    public bool IsMoving()
    {
        Debug.Log("움직임");
        //return navAgent.velocity.magnitude > 0.01f;
        //return !navAgent.isStopped;
        return navAgent.remainingDistance > navAgent.stoppingDistance && !navAgent.pathPending;
    }
    //public void SoldierFormationInitialize(Transform king, int formationIndex)
    //{
    //    this.king = king;
    //    this.formationIndex = formationIndex;
    //}
    public void SoldierInitialize(Transform king, int formationIndx)
    {
        if (!IsOwner) return;
        formationIndex.Value = formationIndx;
        this.king = king;
        if (king == null)
        {
            Debug.LogError("[Soldier] King이 설정되지 않음! 병사가 따라갈 대상이 없음.");
        }
        else
        {
            Debug.Log($"[Soldier] King 설정 완료! King 위치: {king.position}");
        }
        //SoldierFormationInitialize(king, formationIndx); // 초기화

        currentState.OnValueChanged += SoldierAnimation;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        currentState.OnValueChanged -= SoldierAnimation;
    }

    void SoldierAnimation(SoldierState oldValue, SoldierState newValue)
    {
        float speed = newValue == SoldierState.Following || newValue == SoldierState.Collecting ? 1f : 0f;
        SetFloatAnimationserverRpc("Move", speed, 0f, Time.fixedDeltaTime);
    }
    //private void ChangeState(SoldierState newState)
    //{
    //    currentState = newState;
    //    currentState.EnterState(this);
    //}
    // 적 인식 및 상태 전환
    //private Transform FindEnemies()
    //{
    //    float detectionRange = 10f; // 적 탐지 거리

    //    var allEnemies = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IEnemyTarget>();

    //    foreach (IEnemyTarget potentialEnemy in allEnemies)
    //    {
    //        // 자신이 아니고 상대 팀이며, 탐지 범위 내
    //        if (potentialEnemy.GetTransform() != this.transform && potentialEnemy.GetTeam() != team.Value)
    //        {
    //            float distance = Vector3.Distance(transform.position, potentialEnemy.GetTransform().position);

    //            if (distance <= detectionRange)
    //            {
    //                return potentialEnemy.GetTransform();
    //            }
    //        } 
    //    }
    //    return null;
    //}

    //[ServerRpc(RequireOwnership = false)] // Owner가 아니라도 호출 가능
    //private void UpdateStateServerRpc(int newState)
    //{
    //    state.Value = newState; // 서버에서 state.Value를 업데이트 

    //    //currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
    //    currentState.EnterState(this);

    //    UpdateStateClientRpc(newState, team.Value); // 모든 클라이언트에서 상태값을 받고 상태를 재설정 
    //}
    //[ServerRpc]
    //private void AssignTeamServerRpc()
    //{
    //    if (NetworkManager.Singleton.ConnectedClients.Count % 2 == 0)
    //    {
    //        team.Value = 0; // 블루팀
    //    }
    //    else
    //    {
    //        team.Value = 1; // 빨간팀
    //    }
    //}

    //[ClientRpc]
    //private void UpdateStateClientRpc(int newState, int myTeam)
    //{
    //    //currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
    //    currentState.EnterState(this);
    //}

    

    public void SetHP()
    {
        maxHp.Value = 80;
        currentHp.Value = maxHp.Value;
    }

    public void CollectItem(Transform item)
    {
        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
        itemTarget.Value = new NetworkObjectReference(itemNetworkObject);

        //SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId);
    }

    public void ReturnToKing()
    {
        NetworkObject itemNetworkObject;
        //if (itemTarget.Value.TryGet(out itemNetworkObject) && itemTarget.Value.TryGet(out itemNetworkObject))
        //{
        //    SetState(3);
        //}
    }

    public void SoldierDie()
    {
        if (isSoldierDie) return;

        

        // NetworkObject 비활성화 및 제거
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
        isSoldierDie = true;
    }

    public override void Attack()
    {
        throw new System.NotImplementedException();
    }

    public override void Interaction()
    {
        throw new System.NotImplementedException();
    }

    public override void HandleInput()
    {
        throw new System.NotImplementedException();
    }
}