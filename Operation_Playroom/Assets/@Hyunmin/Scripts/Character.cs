using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;

public abstract class Character : NetworkBehaviour, ICharacter
{
    public CinemachineFreeLookModifier cam;

    bool isGrounded;

    CharacterController controller;
    Vector3 velocity;

    protected Animator animator;
    protected NetworkAnimator networkAnimator;
    protected float maxHp = 100;
    protected float currentHp;
    protected float moveSpeed = 5;
    protected Quaternion currentRotation;




    public virtual void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();


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
        HandleAnimationserverRpc("Move", speed, 0.1f, Time.deltaTime);

        // ���� �������� �������� ȸ���� ����
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // ȸ�� ���� (ȸ�� ���� ��� ������)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    [ServerRpc]
    void HandleAnimationserverRpc(string name, float value, float dampTime, float deltaTime)
    {
        networkAnimator.Animator.SetFloat(name, value, dampTime, deltaTime);
    }

    // �ǰ� �޼���
    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp < 0)
        {
            Die();
        }
    }

    // ��� �޼���
    public void Die()
    {
        Debug.Log("Die");
    }
}
