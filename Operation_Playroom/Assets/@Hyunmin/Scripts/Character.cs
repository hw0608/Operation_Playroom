using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class Character : NetworkBehaviour, ICharacter
{
    [HideInInspector] public CinemachineFreeLookModifier cam;

    [SerializeField] GameObject targetItem;
    [SerializeField] GameObject weaponObject;
    [SerializeField] Material[] teamMaterials;
    [SerializeField] Renderer[] playerRenderers;

    public NetworkVariable<int> team = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool isGrounded;
    Vector3 velocity;
    float detectItemRange = 0.2f;
    NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected bool attackAble;
    protected bool isHoldingItem;
    protected float maxHp = 100;
    protected float currentHp;
    protected float moveSpeed = 5;

    protected Animator animator;
    protected NetworkAnimator networkAnimator;
    protected Quaternion currentRotation;

    Coroutine damageRoutine;

    public virtual void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();

        attackAble = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        transform.position = new Vector3(0, 0.5f, 0);

        if (IsOwner) 
        {
            team.Value = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
        }

        foreach (var renderer in playerRenderers)
        {
            renderer.material = new Material(teamMaterials[team.Value]);
        }

        // 색상 변경 이벤트 구독
        playerColor.OnValueChanged += OnPlayerColorChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SetTeamMaterial();
        }
    }

    public override void OnDestroy()
    {   
        playerColor.OnValueChanged -= OnPlayerColorChanged;
    }

    public abstract void Attack(); // 공격 구현
    public abstract void Interaction(); // 상호작용 구현
    public abstract void HandleInput(); // 키 입력 구현

    // 플레이어 피격 시 색상 변경 메서드
    private void OnPlayerColorChanged(Color previousValue, Color newValue)
    {
        if(playerRenderers != null)
        {
            foreach (var renderer in playerRenderers)
            {
                renderer.sharedMaterial.color = newValue;
            }
        }
    }

    // 이동 메서드
    public virtual void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // 카메라 방향에 따른 이동
        Vector3 moveDirection = cam.gameObject.transform.right * moveX + cam.gameObject.transform.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // 애니메이션 적용
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        // 일정 움직임이 있을때만 회전값 변경
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // 회전 적용 (회전 값은 계속 유지됨)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    // 피격 메서드
    public virtual void TakeDamage()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }
        damageRoutine = StartCoroutine(DamageRoutine());
    }

    // 데미지 루틴
    IEnumerator DamageRoutine()
    {
        SetAvatarLayerWeightClientRpc(1);
        SetTriggerAnimationClientRpc("Damage");

        if (IsServer)
        {
            playerColor.Value = Color.red;
        }
        attackAble = false;

        yield return new WaitForSeconds(0.5f);

        if (IsServer)
        {
            playerColor.Value = Color.white;
        }

        SetAvatarLayerWeight(0);

        attackAble = true;
        damageRoutine = null;
    }

    protected void SetAvatarLayerWeight(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

    protected void SetTriggerAnimation(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    protected void SetFloatAnimation(string name, float value)
    {
        networkAnimator.Animator.SetFloat(name, value);
    }

    [ClientRpc]
    protected void SetAvatarLayerWeightClientRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

    [ClientRpc]
    protected void SetTriggerAnimationClientRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // 사망 메서드
    public void Die()
    {
        SetTriggerAnimation("Die");
    }

    // 아이템 줍기 메서드
    public void PickUp()
    {
        targetItem = FindNearestItem();
        if (targetItem != null)
        {
            PickupItem();
        }
    }

    // 아이템 내려놓기 메서드
    public void Drop()
    {
        if (targetItem == null) return;

        DropItem();
    }

    // 자원 찾기 (범위 내 가장 가까운 자원)
    GameObject FindNearestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectItemRange);
        Collider[] item = colliders.Where(col => col.CompareTag("Item")).ToArray();

        GameObject nearestItem = null; // 가까운 자원을 저장할 오브젝트
        float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장, 초기값은 무한대 

        foreach (Collider resource in item) // 탐색된 모든 자원 순회하며
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position); // 왕과 각 자원간 거리 계산
            if (distance < minDistance) // 현재 계산된거리가 최소 거리보다 작으면
            {
                minDistance = distance; // 최소거리에 현재 계산 거리 업데이트
                nearestItem = resource.gameObject; // 오브젝트에 자원 저장
            }
        }
        return nearestItem; // 최소거리자원 오브젝트 반환
    }

    // 아이템을 줍는 메서드
    void PickupItem()
    {
        // 무기 감추기 및 들고있는 상태
        weaponObject.SetActive(false);
        isHoldingItem = true;

        // 아이템 오브젝트 위치시킴
        //targetItem.transform.SetParent(gameObject.transform);
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId,true);
        //targetItem.transform.localPosition = new Vector3(0, 2f, 0);

        // 줍는 애니메이션
        SetAvatarLayerWeight(1);
        SetTriggerAnimation("Holding");
    }

    // 아이템을 내려놓는 메서드
    void DropItem()
    {
        // 무기 보이기 및 들고 있지 않은 상태
        weaponObject.SetActive(true);
        isHoldingItem = false;

        // 아이템 오브젝트 내려놓기
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, false);
        //targetItem.transform.position = transform.position + transform.up * 0.08f + transform.forward * 0.25f; // 앞에 내려놓기
        targetItem = null;

        // 애니메이션 해제
        SetAvatarLayerWeight(0);
        SetTriggerAnimation("Idle");

    }

    void SetTeamMaterial()
    {
        Debug.Log("Set Team Material");

        Material teamMaterial = new Material(teamMaterials[team.Value]);
        foreach(var renderer in playerRenderers)
        {
            renderer.material = teamMaterial;
        }

    }

    // 애니메이션 Trigger 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetTriggerAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // 애니메이션 Float 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetFloatAnimationserverRpc(string name, float value)
    {
        networkAnimator.Animator.SetFloat(name, value);
    }

    // 애니메이션 상체 웨이트 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

}
