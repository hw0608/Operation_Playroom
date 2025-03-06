using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class KingTest : Character
{
    [SerializeField] int initialSoldiersCount;
    [SerializeField] float detectItemRange;
    [SerializeField] float occupyDetectRange; // 점령지 감지 범위 

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
        // E 버튼 누르면
        if (Input.GetKeyDown(KeyCode.E))
        {
            //GameObject nearestOccupy = FindNearestOccupy();
            //if (nearestOccupy != null && HasSoldierWithItem())
            //{
            //    CommandSoldierToDeliverItem(nearestOccupy);
            //}
            if (FindNearestItem() == null)
            {
                CommandSoldierToAdvance();
            }
            else
            {
                CommandSoldierToPickupItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CommandSoldierToReturn();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            soldierSpawner.SpawnSoldiers(1);
        }
    }
    // 점령지로 자원 넣기
    void CommandSoldierToDeliverItem(GameObject occupy)
    {
        foreach (SoldierTest soldier in soldiers)
        {
            if (soldier.isHoldingItem)
            {
                //soldier.TryDeliverItemToOccupy(occupy);
                break;
            }
        }
    }
    bool HasSoldierWithItem()
    {
        return soldiers.Any(soldier => soldier.isHoldingItem);
    }
    // 근처 점령지 찾기
    GameObject FindNearestOccupy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, occupyDetectRange, LayerMask.GetMask("Occupy"));
        GameObject nearestOccupy = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            GameObject occupy = col.gameObject;
            if (occupy != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestOccupy = occupy;
                }
            }
        }
        return nearestOccupy;
    }
    void CommandSoldierToPickupItem()
    {
        foreach(SoldierTest soldier in soldiers)
        {
            // 아이템을 가져오라는 명령을 받을 수 없는 상태
            if (soldier.isHoldingItem || soldier.CurrentState.Value != State.Idle && soldier.CurrentState.Value != State.Following)
            {
                continue;
            }

            GameObject item = FindNearestItem();

            if (item != null)
            {
                // TODO: 수정
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

    public void CommandSoldierToAdvance()
    {
        foreach(SoldierTest soldier in soldiers)
        {
            if (soldier.isHoldingItem || soldier.CurrentState.Value != State.Idle && soldier.CurrentState.Value != State.Following)
            {
                continue;
            }

            GameObject enemy = FindNearestEnemy();

            if (enemy != null)
            {
                soldier.TryAttack(enemy);
            }
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
        Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 1f, LayerMask.GetMask("Enemy")); // 나중에 콜라이더로 수정
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
}


