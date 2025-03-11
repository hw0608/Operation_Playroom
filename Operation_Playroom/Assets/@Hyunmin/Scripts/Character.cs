using System;
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
    [SerializeField] Material[] damageMaterials;
    [SerializeField] Renderer[] playerRenderers;

    public NetworkVariable<int> team = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    float detectItemRange = 0.2f;

    protected bool attackAble;
    protected bool holdItemAble;
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
        holdItemAble = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            team.Value = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
        }

        team.OnValueChanged += (oldValue, newValue) => OnTeamValueChanged(newValue);

        if (IsClient)
        {
            SyncMaterialsOnSpawn();
        }

        OnTeamValueChanged(team.Value);
    }

    public override void OnDestroy()
    {
        team.OnValueChanged -= (oldValue, newValue) => OnTeamValueChanged(newValue);
    }

    public abstract void Attack(); // ���� ����
    public abstract void Interaction(); // ��ȣ�ۿ� ����
    public abstract void HandleInput(); // Ű �Է� ����


    // �̵� �޼���
    public virtual void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // ī�޶� ���⿡ ���� �̵�
        Vector3 moveDirection = cam.gameObject.transform.right * moveX + cam.gameObject.transform.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // �ִϸ��̼� ����
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        // ���� �������� �������� ȸ���� ����
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // ȸ�� ���� (ȸ�� ���� ��� ������)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    // �ǰ� �޼���
    public virtual void TakeDamage()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }
        damageRoutine = StartCoroutine(DamageRoutine());
    }

    // ������ ��ƾ
    IEnumerator DamageRoutine()
    {
        SetAvatarLayerWeightClientRpc(1);
        SetTriggerAnimationClientRpc("Damage");

        ApplyDamageMaterialClientRpc(team.Value);
        attackAble = false;

        yield return new WaitForSeconds(0.5f);

        ApplyMaterialClientRpc(team.Value);

        SetAvatarLayerWeight(0);

        attackAble = true;
        damageRoutine = null;
    }

    void SyncMaterialsOnSpawn()
    {
        Character[] players = FindObjectsByType<Character>(FindObjectsSortMode.None);

        foreach (Character player in players)
        {
            if (player != this)
            {
                player.ApplyMaterial(player.team.Value);
            }
        }
    }

    void OnTeamValueChanged(int teamValue)
    {
        if (teamMaterials == null || teamMaterials.Length == 0)
        {
            return;
        }

        if (teamValue < 0 || teamValue >= teamMaterials.Length)
        {
            teamValue = 0;
        }

        UpdateTeamMaterialClientRpc(teamValue);
        SyncMaterialsOnSpawn();
    }

    void ApplyMaterial(int teamIndex)
    {
        if (teamIndex < 0) return;
        Material targetMaterial = teamMaterials[teamIndex];
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = targetMaterial;
            }
        }
    }

    [ClientRpc]
    void ApplyMaterialClientRpc(int teamIndex)
    {
        if (teamIndex < 0) return;
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = teamMaterials[teamIndex];
            }
        }
    }

    [ClientRpc]
    void ApplyDamageMaterialClientRpc(int teamIndex)
    {
        if (teamIndex < 0) return;
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = damageMaterials[teamIndex];
            }
        }
    }

    [ClientRpc]
    void UpdateTeamMaterialClientRpc(int teamIndex)
    {
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogError("Null");
            return;
        }

        Material targetMaterial = teamMaterials[teamIndex];

        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = new Material(teamMaterials[teamIndex]);
            }
        }
    }

    // ��� �޼���
    public void Die()
    {
        SetTriggerAnimation("Die");
    }

    // ������ �ݱ� �޼���
    public void PickUp()
    {
        targetItem = FindNearestItem();
        if (targetItem != null && holdItemAble)
        {
            attackAble = false;
            PickupItem();
        }
    }

    // ������ �������� �޼���
    public void Drop()
    {
        if (targetItem == null) return;

        attackAble = true;
        DropItem();
    }

    // �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    GameObject FindNearestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectItemRange);
        Collider[] item = colliders.Where(col => col.CompareTag("Item")).ToArray();

        GameObject nearestItem = null; // ����� �ڿ��� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����, �ʱⰪ�� ���Ѵ� 

        foreach (Collider resource in item) // Ž���� ��� �ڿ� ��ȸ�ϸ�
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position); // �հ� �� �ڿ��� �Ÿ� ���
            if (distance < minDistance) // ���� ���ȰŸ��� �ּ� �Ÿ����� ������
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ� ������Ʈ
                nearestItem = resource.gameObject; // ������Ʈ�� �ڿ� ����
            }
        }
        return nearestItem; // �ּҰŸ��ڿ� ������Ʈ ��ȯ
    }

    // �������� �ݴ� �޼���
    void PickupItem()
    {
        // ���� ���߱� �� ����ִ� ����
        weaponObject.SetActive(false);
        isHoldingItem = true;
        holdItemAble = false;

        // ������ ������Ʈ ��ġ��Ŵ
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, true, team.Value);

        // �ݴ� �ִϸ��̼�
        SetAvatarLayerWeight(1);
        SetTriggerAnimation("Holding");
    }

    // �������� �������� �޼���
    void DropItem()
    {
        // ���� ���̱� �� ��� ���� ���� ����
        weaponObject.SetActive(true);
        isHoldingItem = false;
        holdItemAble = true;

        // ������ ������Ʈ ��������
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, false, team.Value);
        targetItem = null;

        // �ִϸ��̼� ����
        SetAvatarLayerWeight(0);
        SetTriggerAnimation("Idle");

    }

    // �ִϸ��̼� Trigger �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetTriggerAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // �ִϸ��̼� ��ü ����Ʈ �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
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

}
