using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using static ReturningState;

public interface IFormable
{
    void SoldierInitialize(Transform king, int formationIndex);
}

public class Soldier : NetworkBehaviour, IFormable 
{
    private Character character;
    public ISoldierState currentState; // 현재 상태
    // 병사 행동 관련 컴포넌트
    private SoldierFormation soldierFormation;
    private SoldierAttack soldierAttack;
    private SoldierAnim soldierAnim;

    // 상태 및 HP 관리
    public NetworkVariable<int> state = new NetworkVariable<int>();  
    public NetworkVariable<float> maxHp = new NetworkVariable<float>(80f);
    public NetworkVariable<float> currentHp = new NetworkVariable<float>(80f);

    // 아이템 타겟, 팀 정보
    public NetworkVariable<NetworkObjectReference> itemTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<NetworkObjectReference> enemyTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<int> team = new NetworkVariable<int>();

    public Transform king;
    public bool isSoldierDie = false;

    // 병사 초기화
    public void SoldierInitialize(Transform king, int formationIndex)
    {
        soldierFormation = GetComponent<SoldierFormation>(); // 참조
        soldierAttack = GetComponent<SoldierAttack>();
        soldierAnim = GetComponent<SoldierAnim>();

        soldierFormation.SoldierFormationInitialize(king, formationIndex); // 초기화
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            // 아이템 타겟 확인 후 동기화
            NetworkObject itemNetObj;
            if (itemTarget.Value.TryGet(out itemNetObj) && !object.ReferenceEquals(itemNetObj, null))
            {
                itemTarget.Value = new NetworkObjectReference(itemNetObj);
            }
            // 팀 배정 요청
            AssignTeamServerRpc(); 
        }
    }

    private void Start()
    {
        character = GetComponent<Character>();

        if (IsOwner)
        {
            SetState(0);
        }
    }

    private void Update()
    {
        if (isSoldierDie) return;

        if (IsOwner && currentState != null)
        {
            Transform enemyTransform = FindEnemies();
            if (enemyTransform != null)
            {
                SetState(1, enemyTransform.GetComponent<NetworkObject>().NetworkObjectId);
            }
            else
            {
                currentState.Execute(this);
            }
        }
        else if (currentState == null)
        {
            currentState = NSoldierState.GetStateFromInt(state.Value, king, itemTarget.Value, team.Value);
            currentState.EnterState(this);
        }
    }
    // 적 인식 및 상태 전환
    private Transform FindEnemies()
    {
        float detectionRange = 10f; // 적 탐지 거리

        var allEnemies = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IEnemyTarget>();

        foreach (IEnemyTarget potentialEnemy in allEnemies)
        {
            // 자신이 아니고 상대 팀이며, 탐지 범위 내
            if (potentialEnemy.GetTransform() != this.transform && potentialEnemy.GetTeam() != team.Value)
            {
                float distance = Vector3.Distance(transform.position, potentialEnemy.GetTransform().position);

                if (distance <= detectionRange)
                {
                    return potentialEnemy.GetTransform();
                }
            } 
        }
        return null;
    }

    // 상태전환 및 동기화
    public void SetState(int stateCode, ulong targetId)
    {
        if (IsServer)
        {
            if (stateCode == 1) // 공격
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject enemyObj))
                {
                    // 네트워크 변수에 저장
                    enemyTarget.Value = enemyObj;
                    // SoldierState도 AttackingState로
                    currentState = new AttackingState(enemyObj.transform);
                    currentState.EnterState(this);

                    // 동기화
                    UpdateStateClientRpc(stateCode, team.Value);
                }
            }
            else if (stateCode == 2) // 자원
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject itemObj))
                {
                    itemTarget.Value = itemObj;
                    currentState = new CollectingState(itemObj.transform);
                    currentState.EnterState(this);

                    // 동기화
                    UpdateStateClientRpc(stateCode, team.Value);
                }
            }
        }
        else
        {
            UpdateStateServerRpc(stateCode); // 서버에게 요청
        }
    }
    public void SetState(int stateCode)
    {
        if (IsServer)
        {
            if (stateCode == 0) // 왕 따라가기
            {
                currentState = new FollowingState();
                currentState.EnterState(this);

                // 클라이언트 동기화
                UpdateStateClientRpc(stateCode, team.Value);
            }
        }
        else
        {
            // 클라이언트에서 서버로 상태 전환 요청
            UpdateStateServerRpc(stateCode);
        }
    }


    [ServerRpc(RequireOwnership = false)] // Owner가 아니라도 호출 가능
    private void UpdateStateServerRpc(int newState)
    {
        state.Value = newState; // 서버에서 state.Value를 업데이트 

        currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
        currentState.EnterState(this);

        UpdateStateClientRpc(newState, team.Value); // 모든 클라이언트에서 상태값을 받고 상태를 재설정 
    }
    [ServerRpc]
    private void AssignTeamServerRpc()
    {
        if (NetworkManager.Singleton.ConnectedClients.Count % 2 == 0)
        {
            team.Value = 0; // 블루팀
        }
        else
        {
            team.Value = 1; // 빨간팀
        }
    }

    [ClientRpc]
    private void UpdateStateClientRpc(int newState, int myTeam)
    {
        currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
        currentState.EnterState(this);
    }

    public void Attack()
    {
        if (IsServer)
        {
            soldierAnim.SoldierAttackAnim();
        }
    }

    public void SetHP()
    {
        maxHp.Value = 80;
        currentHp.Value = maxHp.Value;
    }

    public void CollectItem(Transform item)
    {
        NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
        itemTarget.Value = new NetworkObjectReference(itemNetworkObject);

        SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId);
    }

    public void ReturnToKing()
    {
        NetworkObject itemNetworkObject;
        if (itemTarget.Value.TryGet(out itemNetworkObject) && itemTarget.Value.TryGet(out itemNetworkObject))
        {
            SetState(3);
        }
    }

    public void SoldierDie()
    {
        if (isSoldierDie) return;

        SetState(4);
    }
}