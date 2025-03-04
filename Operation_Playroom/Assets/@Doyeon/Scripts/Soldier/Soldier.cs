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

    // ���� �ൿ ���� ������Ʈ
    private SoldierFormation soldierFormation;
    private SoldierAttack soldierAttack;
    private SoldierAnim soldierAnim;

    // ���� �� HP ����
    public NetworkVariable<int> state = new NetworkVariable<int>();
    public new NetworkVariable<float> maxHp = new NetworkVariable<float>(80f);
    public new NetworkVariable<float> currentHp = new NetworkVariable<float>(80f);

    // ������ Ÿ��, �� ����
    public NetworkVariable<NetworkObjectReference> itemTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<NetworkObjectReference> enemyTarget = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<int> team = new NetworkVariable<int>();

    [SerializeField] float followDistance = 0.2f; // �հ��� �Ÿ� 
    [SerializeField] float warpDistance = 5.0f; // ���� �̵� �Ÿ�
    
    private Vector3 lastTargetPosition; // ������ ��ǥ ��ġ 

    private NavMeshAgent navAgent;
    public Transform king;
    public bool isSoldierDie = false;
    

    
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[Soldier] ��Ʈ��ũ ���� �Ϸ�! ID: {NetworkObjectId}");
        if (!IsOwner) return;
        navAgent = GetComponent<NavMeshAgent>();
        SoldierInitialize(king, formationIndex.Value);
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            Debug.LogError("[Soldier] NavMeshAgent�� ����!");
            return;
        }
        //// ������ Ÿ�� Ȯ�� �� ����ȭ
        //NetworkObject itemNetObj;
        //if (itemTarget.Value.TryGet(out itemNetObj) && !object.ReferenceEquals(itemNetObj, null))
        //{
        //    itemTarget.Value = new NetworkObjectReference(itemNetObj);
        //}
        //// �� ���� ��û
        //AssignTeamServerRpc();
        navAgent.enabled = true;
        Debug.Log("[Soldier] NavMeshAgent Ȱ��ȭ �Ϸ�!");
    }

    private void Update()
    {
        
        FollowKing();
        Debug.Log($"[Soldier] Update ���� ��... King ��ġ: {king?.position}");

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
    {// �׺�޽� ���������� �̵��ϵ��� ����
        if (king == null || navAgent == null || !navAgent.enabled) return;

        Vector3 directionToKing = (king.position - transform.position).normalized;
        Vector3 targetPosition = king.position - (directionToKing * followDistance);


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
            GetComponent<SoldierAnim>().SoldierWalkAnim();
        }

        //if (IsMoving())
        //{
        //    Debug.Log("���� �ȴ� ��");
        //    soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        //}
        //else
        //{
        //    Debug.Log("���� Idle ��");
        //    soldier.GetComponent<SoldierAnim>().SoldierIdleAnim();
        //}
    }
    public bool IsMoving()
    {
        Debug.Log("������");
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
            Debug.LogError("[Soldier] King�� �������� ����! ���簡 ���� ����� ����.");
        }
        else
        {
            Debug.Log($"[Soldier] King ���� �Ϸ�! King ��ġ: {king.position}");
        }
        //SoldierFormationInitialize(king, formationIndx); // �ʱ�ȭ

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
    // �� �ν� �� ���� ��ȯ
    //private Transform FindEnemies()
    //{
    //    float detectionRange = 10f; // �� Ž�� �Ÿ�

    //    var allEnemies = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IEnemyTarget>();

    //    foreach (IEnemyTarget potentialEnemy in allEnemies)
    //    {
    //        // �ڽ��� �ƴϰ� ��� ���̸�, Ž�� ���� ��
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

    //[ServerRpc(RequireOwnership = false)] // Owner�� �ƴ϶� ȣ�� ����
    //private void UpdateStateServerRpc(int newState)
    //{
    //    state.Value = newState; // �������� state.Value�� ������Ʈ 

    //    //currentState = NSoldierState.GetStateFromInt(newState, king, itemTarget.Value, team.Value);
    //    currentState.EnterState(this);

    //    UpdateStateClientRpc(newState, team.Value); // ��� Ŭ���̾�Ʈ���� ���°��� �ް� ���¸� �缳�� 
    //}
    //[ServerRpc]
    //private void AssignTeamServerRpc()
    //{
    //    if (NetworkManager.Singleton.ConnectedClients.Count % 2 == 0)
    //    {
    //        team.Value = 0; // �����
    //    }
    //    else
    //    {
    //        team.Value = 1; // ������
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

        

        // NetworkObject ��Ȱ��ȭ �� ����
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