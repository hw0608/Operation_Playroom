using System;
using System.Collections;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum State
{
    Idle,
    Following,
    MoveToward,
    Attack
}

public class SoldierTest : Character
{
    Transform king;
    NavMeshAgent agent;
    NetworkVariable<State> currentState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    float sqrStoppingDistance;

    [SerializeField] Transform itemContainer;
    [SerializeField] GameObject spearHitbox;
    GameObject target;
    public bool isHoldingItem;
    public bool isAttacking;

    Coroutine attackRoutine;

    public NetworkVariable<State> CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
        }
    }

    public override void Attack()
    {
        if (attackAble)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
            attackRoutine = StartCoroutine(SpearAttack());
        }
    }

    public override void HandleInput()
    {

    }

    public override void Interaction()
    {
        throw new System.NotImplementedException();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        sqrStoppingDistance = agent.stoppingDistance * agent.stoppingDistance;

        currentState.OnValueChanged += HandleAnimation;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        currentState.OnValueChanged -= HandleAnimation;
    }

    IEnumerator SpearAttack()
    {
        RotateToDestination();
        spearHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId);
        SetAvatarLayerWeightserverRpc(1);
        attackAble = false;
        SetTriggerAnimationserverRpc("SpearAttack");

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);

        yield return new WaitForSeconds(0.2f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true;
        SetAvatarLayerWeightserverRpc(0);
    }

    [ServerRpc]
    void EnableHitboxServerRpc(bool state)
    {
        EnableHitboxClientRpc(state);
    }

    [ClientRpc]
    void EnableHitboxClientRpc(bool state)
    {
        spearHitbox.GetComponent<Collider>().enabled = state;
    }

    void HandleAnimation(State previousValue, State newValue)
    {
        float speed = newValue == State.Following || newValue == State.MoveToward ? 1f : 0f;
        SetFloatAnimationserverRpc("Move", speed, 0f, Time.fixedDeltaTime);
    }

    public void SetKing(Transform king)
    {
        this.king = king;
        agent.SetDestination(king.position);
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (king == null) { return; }

        float sqrDistance = agent.stoppingDistance * agent.stoppingDistance;
        float distanceToKing = Vector3.SqrMagnitude(transform.position - king.position);
        bool isNearKing = distanceToKing <= sqrStoppingDistance * 1.2f;
        bool isFarKing = distanceToKing > sqrStoppingDistance * 1.5f;
        bool hasArrived = false;
        
        if (target != null)
            hasArrived = Vector3.SqrMagnitude(transform.position - target.transform.position) <= sqrDistance + float.Epsilon;

        RotateToDestination();

        switch (CurrentState.Value)
        {
            case State.Idle:
                IdleState(isFarKing);
                break;
            case State.Following:
                FollowingState(isNearKing);
                break;
            case State.MoveToward:
                MoveTowardState(hasArrived);
                break;
        }
    }

    // 바라보는 각도 계산
    void RotateToDestination()
    {
        if (CurrentState.Value == State.Idle) { return; }

        Vector2 forward = new Vector2(transform.position.z, transform.position.x);
        Vector2 steeringTarget = new Vector2(agent.steeringTarget.z, agent.steeringTarget.x);
        Vector2 dir = steeringTarget - forward;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = Vector3.up * angle;
    }

    void IdleState(bool isFarKing)
    {
        if (isFarKing)
        {
            FollowKing();
        }
    }

    void FollowingState(bool isNearKing)
    {
        if (isNearKing && agent.velocity.sqrMagnitude < 0.01f)
        {
            currentState.Value = State.Idle;
            agent.velocity = Vector3.zero;
        }
        else
        {
            FollowKing();
        }
    }

    void FollowKing()
    {
        currentState.Value = State.Following;
        agent.SetDestination(king.position);
    }

    void MoveTowardState(bool hasArrived)
    {
        // TODO: 수정
        if (target == null)
        {
            currentState.Value = State.Following;
            agent.SetDestination(king.position);
        }

        if (target.CompareTag("Item"))
        {
            HandleItemPickup(hasArrived);
        }
        else
        {
            HandleEnemyAttack(hasArrived);
        }
    }

    void HandleItemPickup(bool hasArrived)
    {
        if (!isHoldingItem && hasArrived)
        {
            PickupItem();
        }
        else if (isHoldingItem)
        {
            if (hasArrived)
            {
                // TODO: 수정
                GiveItem();
            }
            else
            {
                if (networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName("Holding") && 
                    networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) return;
                agent.SetDestination(king.position);
            }
        }
    }

    void HandleEnemyAttack(bool hasArrived)
    {
        if (hasArrived)
        {
            Attack();
        }
        else
        {
            agent.SetDestination(target.transform.position);
        }
    }

    public void TryPickupItem(GameObject item)
    {
        Debug.Log("TryPickupItem");
        currentState.Value = State.MoveToward;
        target = item;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(target.transform.position);
    }

    void PickupItem()
    {
        isHoldingItem = true;
        target.transform.SetParent(itemContainer);
        target.transform.localPosition = Vector3.zero;
        SetAvatarLayerWeightserverRpc(1);
        SetTriggerAnimationserverRpc("Holding");
        agent.stoppingDistance = 0.3f;
        agent.SetDestination(king.position);

    }

    void GiveItem()
    {
        //SetAvatarLayerWeightserverRpc(0);
        currentState.Value = State.Idle;
        //isHoldingItem = false;
    }

    public void TryAttack(GameObject enemy)
    {
        Debug.Log("TryAttack");
        currentState.Value = State.MoveToward;
        target = enemy;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(target.transform.position);
    }

    public void ResetState()
    {
        if (isHoldingItem) { return; }
        agent.stoppingDistance = 0.5f;
        currentState.Value = State.Following;
    }
}