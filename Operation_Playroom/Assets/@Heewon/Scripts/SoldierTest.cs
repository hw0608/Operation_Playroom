using System;
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
    Transform targetItem;
    public bool isHoldingItem;
    
    public NetworkVariable<State> CurrentState
    {
        get { return currentState; }
        set {
            currentState = value;
        }
    }

    public override void Attack()
    {

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"{agent.remainingDistance} , {agent.stoppingDistance} , {agent.destination}"); 
        }

        if (king != null)
        {
            float distanceToKing = Vector3.SqrMagnitude(transform.position - king.position);
            bool isNearKing = distanceToKing <= sqrStoppingDistance * 1.2f;

            if (currentState.Value != State.Idle)
            {
                // 바라보는 각도 계산
                Vector2 forward = new Vector2(transform.position.z, transform.position.x);
                Vector2 steeringTarget = new Vector2(agent.steeringTarget.z, agent.steeringTarget.x);
                Vector2 dir = steeringTarget - forward;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                transform.eulerAngles = Vector3.up * angle;
            }

            if (CurrentState.Value == State.Following && isNearKing && agent.velocity.sqrMagnitude < 0.01f)
            {
                CurrentState.Value = State.Idle;
                agent.velocity = Vector3.zero;
            }
            else if (CurrentState.Value == State.Following || (CurrentState.Value == State.Idle && distanceToKing > sqrStoppingDistance * 1.5f))
            {
                CurrentState.Value = State.Following;
                agent.SetDestination(king.position);
            }
            else if (CurrentState.Value == State.MoveToward)
            {
                if (targetItem == null)
                {
                    currentState.Value = State.Following;
                    agent.SetDestination(king.position);
                }

                if (!isHoldingItem && agent.remainingDistance <= agent.stoppingDistance + float.Epsilon)
                {
                    PickupItem();
                }

                else if (isHoldingItem)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance + float.Epsilon)
                    {
                        GiveItem();
                    }
                    else
                    {
                        if (networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) return;
                        agent.SetDestination(king.position);
                    }
                }
            }
        }
    }

    public void TryPickupItem(GameObject item)
    {
        Debug.Log("TryPickupItem");
        currentState.Value = State.MoveToward;
        targetItem = item.transform;
        agent.stoppingDistance = 0.1f;
        agent.SetDestination(targetItem.position);
    }

    void PickupItem()
    {
        isHoldingItem = true;
        targetItem.SetParent(itemContainer);
        targetItem.transform.localPosition = Vector3.zero;
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

    public void ResetState()
    {
        if (isHoldingItem) { return; }
        agent.stoppingDistance = 0.5f;
        currentState.Value = State.Following;
    }
}