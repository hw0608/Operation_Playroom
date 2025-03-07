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
        SetFloatAnimationserverRpc("Move", speed);

        if (newValue == State.Attack && previousValue != State.Attack)
        {
            SetTriggerAnimationserverRpc("SpearAttack");
        }
        if (newValue == State.Idle)
        {
            agent.avoidancePriority = 50;
        }
        else if (newValue == State.Following)
        {
            agent.avoidancePriority = 49;
        }
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
        bool isNearKing = distanceToKing <= sqrDistance * 1.2f;
        bool isFarKing = distanceToKing > sqrDistance * 1.5f;
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
            case State.Attack:
                AttackState(hasArrived);
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
    void AttackState(bool hasArrived)
    {
        if (target == null || !target.activeInHierarchy)
        {
            ResetState();
            return;
        }

        if (hasArrived && attackAble)
        {
            Attack();
        }
        else if (!hasArrived)
        {
            agent.SetDestination(target.transform.position);
        }
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
            //currentState.Value = State.Following;
            //agent.SetDestination(king.position);
            ResetState();
            return;
        }

        if (target.CompareTag("Item"))
        {
            HandleItemPickup(hasArrived);
        }
        else if (target.CompareTag("Occupy") && isHoldingItem)
        {
            HandleItemDelivery(hasArrived);
        }
        else
        {
            //HandleEnemyAttack(hasArrived);
            AttackState(hasArrived);
        }
    }
    
    void HandleItemPickup(bool hasArrived)
    {
        if (!isHoldingItem && hasArrived)
        {
            PickupItem();
        }
        else if (isHoldingItem && !target.CompareTag("Occupy")) 
        {
            if
            (networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName("Holding") &&
               networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            { return; }
            agent.SetDestination(king.position);
        }
    }
    
    void HandleItemDelivery(bool hasArrived)
    {
        if (hasArrived)
        {
            DeliverItemToOccupy();
        }
        else
        {
            agent.SetDestination(target.transform.position);
        }
    }
    // 점령지로 이동
    public void TryDeliverItemToOccupy(GameObject occupy)
    {
        if (!isHoldingItem) return;

        currentState.Value = State.MoveToward;
        target = occupy;
        agent.stoppingDistance = 0.3f;
        agent.SetDestination(occupy.transform.position);
    }
    // 점령지에 놓기
    void DeliverItemToOccupy()
    {
        if (target == null || !target.CompareTag("Occupy"))
        {
            return;
        }

        Transform item = itemContainer.GetChild(0);
        item.SetParent(null);
        item.position = target.transform.position;
        item.gameObject.SetActive(false);

        target = null;
        isHoldingItem = false;
        SetAvatarLayerWeightserverRpc(0);
        ResetState();
    }

    void HandleEnemyAttack(bool hasArrived)
    {
        //if (hasArrived)
        //{
        //    Attack();
        //}
        if (target == null)
        {
            ResetState();
            return;
        }
        if (target.CompareTag("Resource"))
        {
            HandleItemPickup(hasArrived);
        }
        else
        {
            AttackState(hasArrived);
            //agent.SetDestination(target.transform.position);
        }
    }

    public void TryPickupItem(GameObject item)
    {
        Debug.Log("TryPickupItem");
        currentState.Value = State.MoveToward;
        target = item;
        agent.stoppingDistance = 0.3f;
        agent.SetDestination(target.transform.position);
    }

    void PickupItem()
    {
        currentState.Value = State.Following;
        isHoldingItem = true;
        target.transform.SetParent(itemContainer);
        target.transform.localPosition = Vector3.zero;
        SetAvatarLayerWeightserverRpc(1);
        SetTriggerAnimationserverRpc("Holding");
        agent.stoppingDistance = 0.3f;
        agent.SetDestination(king.position);
    }

    public void TryAttack(GameObject enemy)
    {
        Debug.Log("TryAttack");
        currentState.Value = State.MoveToward;
        target = enemy;
        agent.stoppingDistance = 0.2f;
        agent.SetDestination(target.transform.position);
    }

    public void ResetState()
    {
        if (isHoldingItem) { return; }
        agent.stoppingDistance = 0.3f;
        currentState.Value = State.Following;
    }
}