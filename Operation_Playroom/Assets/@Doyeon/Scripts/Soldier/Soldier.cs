using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : NetworkBehaviour
{
    enum State
    {
        Idle,
        Walk,
        Attack,
        hold
    }

    [SerializeField] private Transform king;
    [SerializeField] private int formationIndex;
    [SerializeField] Transform holdPoint; // 아이템 위치

    private NetworkVariable<State> networkState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private State state;
    private Animator animator;

    private SoldierFormation soldierFormation;
    private SoldierAttack soldierAttack;
    private SoldierA soldierA;

    private void Start()
    {
        soldierFormation = GetComponent<SoldierFormation>();
        soldierAttack = GetComponent<SoldierAttack>();
        soldierA = GetComponent<SoldierA>();
        
        soldierFormation.SoldierInitialize(king, formationIndex);
    }
    //public override void OnNetworkSpawn()
    //{
    //    if (IsServer)
    //    {
    //        networkState.Value = State.Idle;
    //    }
    //    base.OnNetworkSpawn();
    //    if (IsServer && king == null)
    //    {
    //        // NetworkObject가 동기화된 후에 왕을 할당
    //        king = GameObject.FindFirstObjectByType<TestController>().transform;
    //    }
    //}
    private void Update()
    {
        if (!IsServer) return;

        switch (state)
        {
            case State.Idle:
                soldierA.SodierIdleAnim();
                break;
            case State.Walk:
                soldierFormation.FollowKing();
                soldierA.SoldierWalkAnim();
                break;
            case State.Attack:
                soldierA.SoldierAttackAnim();
                break;
                
        }
       
        
        
        // 상태 변경을 서버에서만
        if (IsServer)
        {
            networkState.Value = state;
        }
    }
    
    
    public void AttackEnded()
    {
        SetState(State.Idle);
    }
    
    void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;
        if (IsServer)
        {
            networkState.Value = state;
        }
    }
    void UpdateSoldierAnim(State currentState)
    {
        switch (currentState)
        {
            case State.Idle:
                animator.SetBool("Idle", true);
                animator.SetBool("Walk", false);
                break;
            case State.Walk:
                animator.SetBool("Idle", false);
                animator.SetBool("Walk", true);
                break;
            case State.Attack:
                animator.SetTrigger("Attack");
                break;
        }
    }
}
