using Unity.Netcode;
using UnityEngine;

public class TestController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheck = 1.1f; // �ٴ� ���� �Ÿ�
    [SerializeField] float jumpForce = 5f; // ���� �� 
    private bool isGrounded;
    private Vector3 moveDirection;
    private Vector3 velocity; // �߷� ����

    private CharacterController characterController;
    //
    [SerializeField] GameObject[] soldierPrefabs;
    [SerializeField] Transform[] soldierSpawnPoints;
    //
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnSoldiers();
        }
    }
    //
    private void Start()
    {
        characterController = GetComponent<CharacterController>();

    }

    private void Update()
    {
        if (!IsOwner) return;
       
        // �ٴ� ����
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheck, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // �ٴڿ� �پ��ֵ���
        }

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

        // �̵�
        Vector3 move = transform.forward * moveDirection.magnitude * moveSpeed;
        characterController.Move(move * Time.deltaTime);

        // ����
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        // �߷� ����
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void SpawnSoldiers()
    {
        for (int i = 0; i < soldierSpawnPoints.Length; i++)
        {
            // �ε����� �´� ���� �������� ����
            GameObject soldier = Instantiate(soldierPrefabs[i], soldierSpawnPoints[i].position, Quaternion.identity);

            // Soldier ��ũ��Ʈ�� formationIndex ����
            Soldier soldierScript = soldier.GetComponent<Soldier>();
            soldierScript.formationIndex = i; // 0, 1, 2�� ����

            soldierScript.king = this.transform;

            soldier.GetComponent<NetworkObject>().Spawn();
        }
    }
    //

    [ServerRpc]
    private void SyncPositionServerRpc(Vector3 newPosition)
    {
        SyncPositionClientRpc(newPosition);
    }
    [ClientRpc]
    private void SyncPositionClientRpc(Vector3 newPosition)
    {
        if (!IsOwner)
        {
            transform.position = newPosition;
        }
    }
}
