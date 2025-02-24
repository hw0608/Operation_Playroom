using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Archer : Character
{
    [SerializeField] CinemachineCamera aimCamera;

    bool isAiming;
    float xRotation = 0;
    float mouseSensitivity = 100;


    // 공격 메서드
    public override void Attack()
    {
        // 화살 발사
        if (isAiming)
        {
            Debug.Log("Archer Attack");
        }
    }

    // 이동 메서드
    public override void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // 카메라 방향에 따른 이동
        Transform referenceCam = isAiming ? aimCamera.transform : cam.transform; // 기준으로 할 카메라 선택
        Vector3 moveDirection = referenceCam.right * moveX + referenceCam.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // 애니메이션 적용
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        HandleAnimationserverRpc("Move", speed, 0.1f, Time.deltaTime);

        // 조준중이 아닐때
        if (!isAiming)
        {
            // 일정 움직임이 있을때만 회전값 변경
            if (moveDirection.magnitude > 0.1f)
            {
                currentRotation = Quaternion.LookRotation(moveDirection);
            }

            // 회전 적용 (회전 값은 계속 유지됨)
            rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
        }
        // 조준중일때
        else
        {
            RotateView(); // 1인칭 회전 적용
        }
      
    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 조준 시작
        if (Input.GetButtonDown("Aim"))
        {
            aimCamera.transform.position = transform.position + transform.forward * 0.05f + transform.up * 0.11f;
            transform.rotation = cam.transform.rotation;
            aimCamera.transform.rotation = cam.transform.rotation;
            aimCamera.transform.localRotation = Quaternion.Euler(0, 0f, 0f);

            aimCamera.Priority = 10;

            isAiming = true;
        }
        // 조준 취소
        if (Input.GetButtonUp("Aim"))
        {
            aimCamera.Priority = -10;
            isAiming = false;
        }
        // 조준중이면
        if (isAiming)
        {
            // 발사
            if (Input.GetButtonDown("Attack"))
            {
                Attack();
            }
        }
        // 줍기
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // 상호작용 메서드
    public override void Interaction()
    {
        // 줍기
        Debug.Log("Interaction");
    }

    // 체력 적용 메서드
    public override void SetHP()
    {
        maxHp = 80;
        currentHp = maxHp;
    }

    // 1인칭 회전 메서드
    void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 위아래 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 40f); // 시야 제한

        // 캐릭터와 카메라 회전
        aimCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + mouseX, 0f);
    }

    [ServerRpc]
    void HandleAnimationserverRpc(string name, float value, float dampTime, float deltaTime)
    {
        networkAnimator.Animator.SetFloat(name, value, dampTime, deltaTime);
    }
}

