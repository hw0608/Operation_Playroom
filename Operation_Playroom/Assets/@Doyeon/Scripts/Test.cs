using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheck = 1.1f; // 바닥 감지 거리
    [SerializeField] float jumpForce = 5f; // 점프 힘 
    private bool isGrounded;

    private NavMeshAgent navAgent;
    private Vector3 moveDirection;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.acceleration = 8f;
        navAgent.stoppingDistance = 0.1f; // 멈추는 거리 조정
        navAgent.updateRotation = false; // 자동회전 비활성화
    }

    private void Update()
    {
        // 바닥 감지
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheck, groundLayer);

        // 입력받기
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 이동 방향 설정
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;

        // 이동방향이 있으면 회전
        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            navAgent.SetDestination(transform.position + moveDirection * 2f); // 앞으로 이동
        }

    }
}
