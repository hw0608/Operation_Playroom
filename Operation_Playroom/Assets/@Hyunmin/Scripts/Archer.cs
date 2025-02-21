using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Archer : Character
{
    [SerializeField] CinemachineCamera aimCamera;

    bool isAiming;
    float xRotation = 0;
    float mouseSensitivity = 100;


    // ���� �޼���
    public override void Attack()
    {
        // ȭ�� �߻�
        if (isAiming)
        {
            Debug.Log("Archer Attack");
        }
    }

    private void Update()
    {
        if (isAiming)
        {
            RotateView();
        }
    }

    public override void Move(CinemachineCamera cam, Rigidbody rb)
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

        if (!isAiming)
        {
            // ���� �������� �������� ȸ���� ����
            if (moveDirection.magnitude > 0.1f)
            {
                currentRotation = Quaternion.LookRotation(moveDirection);
            }

            // ȸ�� ���� (ȸ�� ���� ��� ������)
            rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
        }
      
    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ���� ����
        if (Input.GetButtonDown("Aim"))
        {
            aimCamera.transform.position = transform.position + transform.forward * 0.05f + transform.up * 0.11f;
            transform.rotation = cam.transform.rotation;
            aimCamera.transform.rotation = cam.transform.rotation;

            aimCamera.Priority = 10;

            isAiming = true;
        }
        // ���� ���
        if (Input.GetButtonUp("Aim"))
        {
            aimCamera.Priority = -10;
            isAiming = false;
        }
        if (isAiming)
        {
            // �߻�
            if (Input.GetButtonDown("Attack"))
            {
                Attack();
            }
        }
        // �ݱ�
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // ��ȣ�ۿ� �޼���
    public override void Interaction()
    {
        // �ݱ�
        Debug.Log("Interaction");
    }

    // ü�� ���� �޼���
    public override void SetHP()
    {
        maxHp = 80;
        currentHp = maxHp;
    }

    void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���Ʒ� ȸ�� (Pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f); // �þ� ����

        // ĳ���Ϳ� ī�޶� ȸ��
        transform.Rotate(Vector3.up * mouseX); // �¿� ȸ�� (Yaw)
        aimCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // ���Ʒ� ȸ��
    }

    [ServerRpc]
    void HandleAnimationserverRpc(string name, float value, float dampTime, float deltaTime)
    {
        networkAnimator.Animator.SetFloat(name, value, dampTime, deltaTime);
    }
}

