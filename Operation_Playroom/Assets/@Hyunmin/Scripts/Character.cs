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

    public abstract void Attack(); // 공격 구현
    public abstract void Interaction(); // 상호작용 구현
    public abstract void HandleInput(); // 키 입력 구현
    public abstract void SetHP(); // 직업 별 체력적용

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
        SetFloatAnimationserverRpc("Move", speed, 0.1f, Time.deltaTime);

        // 일정 움직임이 있을때만 회전값 변경
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // 회전 적용 (회전 값은 계속 유지됨)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    // 피격 메서드
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

    // 사망 메서드
    public void Die()
    {
        Debug.Log("Die");
        SetTriggerAnimationserverRpc("Die");
    }

    // 애니메이션 Trigger 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetTriggerAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    // 애니메이션 Float 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetFloatAnimationserverRpc(string name, float value, float dampTime, float deltaTime)
    {
        networkAnimator.Animator.SetFloat(name, value, dampTime, deltaTime);
    }

    // 애니메이션 상체 웨이트 메서드
    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

}
