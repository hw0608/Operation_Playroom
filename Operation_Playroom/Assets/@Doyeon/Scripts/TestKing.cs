using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


public class TestKing : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private Rigidbody rb;
    public LayerMask groundLayer;
    public float groundCheck = 1.1f; // 바닥 감지 거리
    private bool isGrounded;

    public float gravity = 9.8f; //중력 값
    public float jumpForce = 5f; // 점프 힘 

    private Vector3 moveDirection; // 이동방향 저장

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
        }

        // 점프 처리 
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }


    }

    private void FixedUpdate()
    {
        // 중력 적용 (땅에 있지 않을 때만 아래로 힘 가하기)
        if (!isGrounded)
        {
            rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }

        // 땅에 있을 때만 이동 적용
        if (isGrounded)
        {
            //rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
    }

    
}
