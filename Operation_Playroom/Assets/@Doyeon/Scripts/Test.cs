using UnityEngine;
using UnityEngine.AI;

public class Test : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheck = 1.1f; // �ٴ� ���� �Ÿ�
    [SerializeField] float jumpForce = 5f; // ���� �� 
    private bool isGrounded;

    private NavMeshAgent navAgent;
    private Vector3 moveDirection;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.acceleration = 8f;
        navAgent.stoppingDistance = 0.1f; // ���ߴ� �Ÿ� ����
        navAgent.updateRotation = false; // �ڵ�ȸ�� ��Ȱ��ȭ
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
            navAgent.SetDestination(transform.position + moveDirection * 2f); // ������ �̵�
        }

    }
}
