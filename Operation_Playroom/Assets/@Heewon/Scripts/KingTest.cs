using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class KingTest : Character
{
    [SerializeField] int initialSoldiersCount;
    [SerializeField] float detectItemRange;

    Spawner soldierSpawner;
    public List<SoldierTest> soldiers = new List<SoldierTest>();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        base.Start(); 
        soldierSpawner = GetComponent<Spawner>();

        soldierSpawner.SpawnSoldiers(initialSoldiersCount);
    }
    void Update()
    {
        if (!IsOwner) return;
        //HandleInput();
    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 공격
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
        // E 버튼 누르면
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (FindNearestItem() == null)
            {
                CommandSoldierToAdvance();
            }
            else
            {
                CommandSoldierToPickupItem();
            }
            Debug.Log("King E눌림");
        }
        // Q 버튼 누르면
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CommandSoldierToReturn();
            Debug.Log("Q눌림");
        }

        /*
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (IsOwner)
            {
                if (soldierSpawner != null)
                {
                    soldierSpawner.AddSoldierServerRpc(1);
                }
            }
            Debug.Log($"F키 눌림 by {OwnerClientId}");
        }
        */
    }

    void CommandSoldierToPickupItem()
    {
        foreach(SoldierTest soldier in soldiers)
        {
            if (soldier.isHoldingItem || soldier.CurrentState.Value != State.Idle && soldier.CurrentState.Value != State.Following)
            {
                continue;
            }

            GameObject item = FindNearestItem();

            if (item != null)
            {
                soldier.TryPickupItem(item);
                item.gameObject.layer = 0;
            }
        }
    }

    void CommandSoldierToReturn()
    {
        foreach(SoldierTest soldier in soldiers)
        {
            soldier.ResetState();
        }
    }

    void CommandSoldierToAdvance()
    {
        foreach(SoldierTest soldier in soldiers)
        {
            if (soldier.isHoldingItem || soldier.CurrentState.Value != State.Idle && soldier.CurrentState.Value != State.Following)
            {
                continue;
            }

            soldier.Attack();
        }
    }

    // 공격 메서드
    public override void Attack()
    {
        // 검 휘두르며 공격
        Debug.Log("Sword Attack");
    }
    // 상호작용 메서드
    public override void Interaction()
    {
        // 줍기
        Debug.Log("King Interaction");
    }

    // 적을 찾는 메서드 (범위 내 가장 가까운 적)
    private GameObject FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Enemy")); // 나중에 콜라이더로 수정
        GameObject nearestEnemy = null; // 가장 가까운 적을 저장할 오브젝트
        float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장. 초기 값은 무한대로 설정

        foreach (Collider enemy in enemies) // 탐색된 모든 적 순회
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position); // 왕, 각 적의 거리를 계산
            if (distance < minDistance) // 현재 계산된 거리가 최소 거리보다 작으면 
            {
                minDistance = distance; // 최소거리를 현재 계산 거리로 업데이트
                nearestEnemy = enemy.gameObject; // 가까운 적 오브젝트에 현재 적의 오브젝트로 저장 
            }
        }
        return nearestEnemy; // 가까운 적 오브젝트를 반환
    }

    // 자원 찾기 (범위 내 가장 가까운 자원)
    private GameObject FindNearestItem()
    {
        Collider[] item = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, detectItemRange, LayerMask.GetMask("Item")); // (왕의 위치를 중심으로, ~)
        GameObject nearestItem = null; // 가까운 자원을 저장할 오브젝트
        float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장, 초기값은 무한대 

        foreach (Collider resource in item) // 탐색된 모든 자원 순회하며
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position); // 왕과 각 자원간 거리 계산
            if (distance < minDistance) // 현재 계산된거리가 최소 거리보다 작으면
            {
                minDistance = distance; // 최소거리에 현재 계산 거리 업데이트
                nearestItem = resource.gameObject; // 오브젝트에 자원 저장
            }
        }
        return nearestItem; // 최소거리자원 오브젝트 반환
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.5f, detectItemRange);
    }
}


