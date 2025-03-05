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

    public NetworkVariable<int> team = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool isGrounded;
    Vector3 velocity;
    float detectItemRange = 0.7f;


    protected bool attackAble;
    protected bool isHoldingItem;
    protected float maxHp = 100;
    protected float currentHp;
    protected float moveSpeed = 5;

    protected Animator animator;
    protected NetworkAnimator networkAnimator;
    protected Quaternion currentRotation;

    public virtual void Start()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();

        attackAble = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (IsOwner) {
            team.Value = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
        }
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
        SetFloatAnimationserverRpc("Move", speed);

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
        StartCoroutine(DamageRoutine());
    }

    // ������ ��ƾ
    IEnumerator DamageRoutine()
    {
        SetAvatarLayerWeightserverRpc(1);
        SetTriggerAnimationserverRpc("Damage");
        attackAble = false;

        yield return new WaitForSeconds(0.5f);

        SetAvatarLayerWeightserverRpc(0);
        attackAble = true;

    }

    // ��� �޼���
    public void Die()
    {
        SetTriggerAnimationserverRpc("Die");
    }

    // ������ �ݱ� �޼���
    public void PickUp()
    {
        targetItem = FindNearestItem();
        if (targetItem != null)
        {
            PickupItem();
        }
    }

    // ������ �������� �޼���
    public void Drop()
    {
        if (targetItem == null) return;

        DropItem();
    }

    // �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    GameObject FindNearestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, detectItemRange);
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

        // ������ ������Ʈ ��ġ��Ŵ
        targetItem.transform.SetParent(gameObject.transform);
        targetItem.transform.localPosition = new Vector3(0, 2f, 0);

        // �ݴ� �ִϸ��̼�
        SetAvatarLayerWeightserverRpc(1);
        SetTriggerAnimationserverRpc("Holding");
    }

    // �������� �������� �޼���
    void DropItem()
    {
        // ���� ���̱� �� ��� ���� ���� ����
        weaponObject.SetActive(true);
        isHoldingItem = false;

        // ������ ������Ʈ ��������
        targetItem.transform.SetParent(null);
        targetItem.transform.position = transform.position + transform.up * 0.08f + transform.forward * 0.25f; // �տ� ��������
        targetItem = null;

        // �ִϸ��̼� ����
        SetAvatarLayerWeightserverRpc(0);
        SetTriggerAnimationserverRpc("Idle");
    }

    // �ִϸ��̼� Trigger �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetTriggerAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // �ִϸ��̼� Float �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetFloatAnimationserverRpc(string name, float value)
    {
        networkAnimator.Animator.SetFloat(name, value);
    }

    // �ִϸ��̼� ��ü ����Ʈ �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

}
