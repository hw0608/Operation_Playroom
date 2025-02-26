using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public abstract class Character : NetworkBehaviour, ICharacter
{
    [HideInInspector] public CinemachineFreeLookModifier cam;

    bool isGrounded;
    Vector3 velocity;

    protected bool attackable;
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

        attackable = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    public abstract void Attack(); // ���� ����
    public abstract void Interaction(); // ��ȣ�ۿ� ����
    public abstract void HandleInput(); // Ű �Է� ����
    public abstract void SetHP(); // ���� �� ü������

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
        SetFloatAnimationserverRpc("Move", speed, 0.1f, Time.deltaTime);

        // ���� �������� �������� ȸ���� ����
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // ȸ�� ���� (ȸ�� ���� ��� ������)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    // �ǰ� �޼���
    public virtual void TakeDamage(float damage, ulong clientId)
    {
        Debug.Log("Damage");
        StartCoroutine(DamageRoutine());
    }

    IEnumerator DamageRoutine()
    {
        SetAvatarLayerWeightserverRpc(1);
        SetTriggerAnimationserverRpc("Damage");
        attackable = false;

        yield return new WaitForSeconds(0.5f);

        SetAvatarLayerWeightserverRpc(0);
        attackable = true;

    }

    // ��� �޼���
    public void Die()
    {
        Debug.Log("Die");
        SetTriggerAnimationserverRpc("Die");
    }

    // �ִϸ��̼� Trigger �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetTriggerAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // �ִϸ��̼� Float �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetFloatAnimationserverRpc(string name, float value, float dampTime, float deltaTime)
    {
        networkAnimator.Animator.SetFloat(name, value, dampTime, deltaTime);
    }

    // �ִϸ��̼� ��ü ����Ʈ �޼���
    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

}
