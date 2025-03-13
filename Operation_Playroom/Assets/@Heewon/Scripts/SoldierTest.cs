using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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
    Vector3 offset;
    NavMeshAgent agent;
    Health health;
    NetworkVariable<State> currentState = new NetworkVariable<State>(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] GameObject spearHitbox;
    GameObject target;
    GameObject myItem;
    public bool isAttacking;
    bool isResetting;

    Coroutine attackRoutine;

    public NetworkVariable<State> CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
        }
    }

    public Vector3 Offset
    {
        set
        {
            offset = value;
        }
    }

    public bool CanReceiveCommand => !health.isDead && !isHoldingItem && (currentState.Value == State.Idle || currentState.Value == State.Following);
    public bool HasItem => isHoldingItem;


    #region Network
    public override void OnNetworkSpawn()
    {
        health = GetComponent<Health>();

        if (IsServer)
        {
            health.OnDie -= HandleOnDie;
            health.OnDie += HandleOnDie;
        }

        if (!IsOwner) { return; }

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        currentState.OnValueChanged -= HandleAnimation;
        currentState.OnValueChanged += HandleAnimation;

        
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        currentState.OnValueChanged -= HandleAnimation;
        health.OnDie -= HandleOnDie;
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

    #endregion

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        if (king == null) { return; }
        if (health.isDead) { return; }

        float sqrDistance = agent.stoppingDistance * agent.stoppingDistance;
        float distanceToKing = Vector3.SqrMagnitude(transform.position - GetFormationPosition());

        bool isNearKing = distanceToKing <= sqrDistance * 1.2f;
        bool isFarKing = distanceToKing > sqrDistance * 1.7f;
        bool hasArrived = false;

        if (target != null)
        {
            hasArrived = Vector3.SqrMagnitude(transform.position - target.transform.position) <= sqrDistance * 1.5f;
        }

        RotateToDestination(hasArrived);

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


    #region IDLE

    void IdleState(bool isFarKing)
    {
        if (isFarKing)
        {
            FollowKing();
        }
    }

    #endregion

    #region Following

    void FollowingState(bool isNearKing)
    {
        if (isNearKing)
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
        agent.SetDestination(GetFormationPosition());
    }

    #endregion

    #region MoveToward
    void MoveTowardState(bool hasArrived)
    {
        // TODO: ����
        if (target == null)
        {
            ResetState();
            return;
        }

        if (target.CompareTag("Item"))
        {
            HandleItemPickup(hasArrived);
        }
        else if (target.CompareTag("Enemy"))
        {
            CurrentState.Value = State.Attack;
        }
        else if (isHoldingItem)
        {
            HandleItemDelivery(hasArrived);
        }
    }

    void HandleItemPickup(bool hasArrived)
    {
        if (!isHoldingItem)
        {
            if (hasArrived)
            {
                PickupItem();
            }
            else if (!hasArrived && target.GetComponent<ResourceData>().isHolding.Value)
            {
                TryResetState();
            }
        }
        else if (isHoldingItem)
        {
            if
            (networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName("Holding") &&
               networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            { return; }
            agent.SetDestination(GetFormationPosition());
        }
    }

    void HandleItemDelivery(bool hasArrived)
    {
        if (hasArrived)
        {
            DeliverItemToOccupy();
        }
        //else
        //{
        //    agent.SetDestination(target.transform.position);
        //}
    }
    // �������� �̵�
    public void TryDeliverItemToOccupy(GameObject occupy)
    {
        if (!isHoldingItem) return;

        currentState.Value = State.MoveToward;
        target = occupy;
        agent.stoppingDistance = 0.3f;
        agent.SetDestination(occupy.transform.position);
    }
    // �������� ����
    void DeliverItemToOccupy()
    {
        if (target == null)
        {
            ResetState();
            return;
        }

        myItem.GetComponent<ResourceData>().isMarked = false;
        PutDownItem();
        ResetState();
    }

    public void TryPickupItem(GameObject item)
    {
        Debug.Log("TryPickupItem");
        currentState.Value = State.MoveToward;
        target = item;
        target.GetComponent<ResourceData>().isMarked = true;
        agent.stoppingDistance = 0.2f;
        agent.SetDestination(target.transform.position);
    }

    void PickupItem()
    {
        Debug.Log("PickupItem");
        currentState.Value = State.Following;
        isHoldingItem = true;
        target.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, true, team.Value);
        myItem = target;
        SetAvatarLayerWeight(1);
        SetTriggerAnimation("Holding");
        agent.stoppingDistance = 0.2f;
        agent.SetDestination(GetFormationPosition());
    }

    #endregion

    #region Attack

    void AttackState(bool hasArrived)
    {
        if (target.TryGetComponent(out Health health))
        {
            if (health.isDead)
            {
                GameObject enemy = FindNearestEnemy();

                if (enemy != null)
                {
                    TryAttack(enemy);
                }
                else
                {
                    ResetState();
                    return;
                }

            }
        }

        if (target.TryGetComponent(out Building building))
        {
            if (building.health.Value <= 0)
            {
                ResetState();
                return;
            }
        }

        if (hasArrived && attackAble)
        {
            Attack();
        }
        else if (!hasArrived)
        {
            if (!attackAble) { return; }
            agent.avoidancePriority = 49;
            SetFloatAnimation("Move", 1f);
            agent.SetDestination(target.transform.position);
        }
    }


    public void TryAttack(GameObject enemy)
    {
        Debug.Log("TryAttack");
        currentState.Value = State.MoveToward;
        target = enemy;
        agent.stoppingDistance = 0.1f;

        if (enemy.GetComponent<Building>() != null)
        {
            agent.stoppingDistance = 0.3f;
        }

        agent.SetDestination(target.transform.position);
    }

    public override void Attack()
    {
        if (attackAble)
        {
            agent.avoidancePriority = 50;
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
            attackRoutine = StartCoroutine(SpearAttack());
        }
    }
    IEnumerator SpearAttack()
    {
        StartCoroutine(RotateToTarget());
        spearHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId, team.Value);
        attackAble = false;
        SetTriggerAnimation("SpearAttack");

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);

        yield return new WaitForSeconds(0.2f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true;
    }


    IEnumerator RotateToTarget()
    {
        float timer = 2f;
        while (timer > 0)
        {
            if (target == null)
            {
                yield break;
            }
            timer -= Time.deltaTime;
            Vector2 forward = new Vector2(transform.position.z, transform.position.x);
            Vector2 targetPos = new Vector2(target.transform.position.z, target.transform.position.x);

            Vector2 dir = targetPos - forward;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * 5f);
            transform.eulerAngles = Vector3.up * smoothAngle;

            yield return null;
        }
    }


    #endregion

    #region Reset

    public void TryResetState()
    {
        if (isResetting) { return; }

        if (!isResetting
            && networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")
            && networkAnimator.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            StartCoroutine(WaitForAttckAnimationToEnd());
        }
        else
        {
            ResetState();
        }
    }

    void ResetState()
    {
        if (target != null && target.TryGetComponent(out ResourceData data))
        {
            Debug.Log("Mark ����");
            data.isMarked = false;
        }
        if (myItem != null)
        {
            PutDownItem();
        }
        target = null;
        agent.stoppingDistance = 0.2f;
        currentState.Value = State.Following;
    }

    IEnumerator WaitForAttckAnimationToEnd()
    {
        isResetting = true;

        while (!attackAble)
        {
            yield return null;
        }

        ResetState();
        isResetting = false;
    }

    #endregion

    #region Die

    public void HandleOnDie(Health health)
    {
        Debug.Log("HandleOnDie");

        StartCoroutine(RespawnSoldierRoutine());
    }

    IEnumerator RespawnSoldierRoutine()
    {
        float respawnTime = 10f;
        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1f);
            respawnTime -= 1f;
        }

        GameTeam gameTeam = (GameTeam)team.Value;
        Vector3 spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.None);

        Debug.Log($"Respawn Position: {spawnPosition}, Team: {gameTeam}");

        // �������� ��ġ ����
        transform.position = spawnPosition;

        health.InitializeHealth();

        // Ŭ���̾�Ʈ�� ��ġ ����ȭ
        UpdatePlayerStateClientRpc(NetworkObject, spawnPosition);
    }

    [ClientRpc]
    void UpdatePlayerStateClientRpc(NetworkObjectReference playerRef, Vector3 position)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            target = null;
            myItem = null;

            Character character = playerObj.GetComponent<Character>();
            Health health = playerObj.GetComponent<Health>();

            // Ŭ���̾�Ʈ���� ��ġ, ����, �ִϸ����� �ʱ�ȭ
            playerObj.transform.position = position;
            health.InitializeHealth();
            character.InitializeAnimator();

            Debug.Log($"Client {OwnerClientId} respawned at {position}");
        }
    }

    #endregion

    #region Animation
    void HandleAnimation(State previousValue, State newValue)
    {
        float speed = newValue == State.Following || newValue == State.MoveToward ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        if (newValue == State.Idle)
        {
            agent.avoidancePriority = 50;

        }
        else if (newValue == State.Following)
        {
            agent.avoidancePriority = 49;
        }
    }

    // Animation Event
    public void HadItemBefore()
    {
        if (myItem != null)
        {
            if (!health.isDead)
            {
                SetTriggerAnimation("Holding");
            }
            else
            {
                Debug.Log("PutDownItem");
                PutDownItem();
            }
        }
        else
        {
            SetAvatarLayerWeight(0);
        }
    }

    #endregion

    public void Init(Transform king, Vector3 offset)
    {
        this.king = king;
        this.offset = offset;
        team.Value = king.GetComponent<Character>().team.Value;
        agent.SetDestination(GetFormationPosition());
    }

    Vector3 GetFormationPosition()
    {
        Vector3 rotatedOffset = Quaternion.LookRotation(king.forward) * offset;

        return king.position + rotatedOffset;
    }

    // �ٶ󺸴� ���� ��� 
    void RotateToDestination(bool hasArrived)
    {
        if (CurrentState.Value == State.Idle
            || (hasArrived && (currentState.Value == State.MoveToward || currentState.Value == State.Attack))) { return; }

        Vector2 forward = new Vector2(transform.position.z, transform.position.x);
        Vector2 steeringTarget = new Vector2(agent.steeringTarget.z, agent.steeringTarget.x);
        Vector2 dir = steeringTarget - forward;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * 5f);
        transform.eulerAngles = Vector3.up * smoothAngle;
    }
    void PutDownItem()
    {
        if (myItem == null) return;

        myItem.GetComponent<ResourceData>().isMarked = false;
        myItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, false, team.Value);
        myItem = null;
        isHoldingItem = false;
        SetAvatarLayerWeight(0);
    }

    private GameObject FindNearestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.2f, LayerMask.GetMask("Enemy")); // ���߿� �ݶ��̴��� ����
        GameObject nearestEnemy = null; // ���� ����� ���� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����. �ʱ� ���� ���Ѵ�� ����

        Owner myTeam = team.Value == 0 ? Owner.Blue : Owner.Red;

        var enemies = colliders
            .SelectMany(col =>
            {
                var character = col.GetComponent<Character>();
                if (character != null && !character.GetComponent<Health>().isDead && character.team.Value != team.Value)
                    return new[] { character.gameObject };

                var building = col.GetComponent<Building>();

                if (building != null && building.buildingOwner != myTeam)
                    return new[] { building.gameObject };

                return Enumerable.Empty<GameObject>();
            }).ToArray();

        foreach (GameObject enemy in enemies) // Ž���� ��� �� ��ȸ
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position); // ��, �� ���� �Ÿ��� ���
            if (distance < minDistance) // ���� ���� �Ÿ��� �ּ� �Ÿ����� ������ 
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ��� ������Ʈ
                nearestEnemy = enemy; // ����� �� ������Ʈ�� ���� ���� ������Ʈ�� ���� 
            }
        }
        return nearestEnemy; // ����� �� ������Ʈ�� ��ȯ
    }

    public override void HandleInput()
    {
        
    }

    public override void Interaction()
    {
        throw new System.NotImplementedException();
    }
}