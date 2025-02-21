using Unity.Netcode;
using UnityEngine;

public class TestController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheck = 1.1f; // 바닥 감지 거리
    [SerializeField] float jumpForce = 5f; // 점프 힘 
    private bool isGrounded;
    private Vector3 moveDirection;
    private Vector3 velocity; // 중력 적용

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
       
        // 바닥 감지
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheck, groundLayer);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 바닥에 붙어있도록
        }

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

        // 이동
        Vector3 move = transform.forward * moveDirection.magnitude * moveSpeed;
        characterController.Move(move * Time.deltaTime);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }

        // 중력 적용
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void SpawnSoldiers()
    {
        for (int i = 0; i < soldierSpawnPoints.Length; i++)
        {
            // 인덱스에 맞는 병사 프리팹을 생성
            GameObject soldier = Instantiate(soldierPrefabs[i], soldierSpawnPoints[i].position, Quaternion.identity);

            // Soldier 스크립트의 formationIndex 설정
            Soldier soldierScript = soldier.GetComponent<Soldier>();
            soldierScript.formationIndex = i; // 0, 1, 2로 설정

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
