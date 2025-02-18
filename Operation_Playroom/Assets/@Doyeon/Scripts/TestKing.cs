using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;


public class TestKing : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    private Rigidbody rb;
    public LayerMask groundLayer;
    public float groundCheck = 1.1f; // �ٴ� ���� �Ÿ�
    private bool isGrounded;

    public float gravity = 9.8f; //�߷� ��
    public float jumpForce = 5f; // ���� �� 

    private Vector3 moveDirection; // �̵����� ����

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    private void Update()
    {
        // �ٴ� ����
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheck, groundLayer);
        // �Է¹ޱ�
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        // �̵� ���� ����
        moveDirection = new Vector3(horizontal, 0, vertical).normalized; 

        // �̵������� ������ ȸ��
        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // ���� ó�� 
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }


    }

    private void FixedUpdate()
    {
        // �߷� ���� (���� ���� ���� ���� �Ʒ��� �� ���ϱ�)
        if (!isGrounded)
        {
            rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }

        // ���� ���� ���� �̵� ����
        if (isGrounded)
        {
            //rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
    }

    
}
