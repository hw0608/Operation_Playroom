using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KingTest : Character
{
    [SerializeField] int initialSoldiersCount;
    [SerializeField] float itemDetectRange;
    [SerializeField] float occupyDetectRange; // 점령지 감지 범위
    [SerializeField] float soldierSpacing = 0.2f;
    [SerializeField] int maxSoldierCount = 10;

    Spawner soldierSpawner;
    public List<SoldierTest> soldiers = new List<SoldierTest>();
    public List<Vector3> soldierOffsets = new List<Vector3>();

    public SoldierTest debugSoldier;

    Vector3 GetFormationOffset(int index, int[] colOffsets)
    {
        float verticalSpacing = soldierSpacing * 1.2f;
        int row = index / 5;
        int col = index % 5;

        float offsetX = colOffsets[col];

        return new Vector3(offsetX * soldierSpacing, 0, -row * verticalSpacing - soldierSpacing);
    }

    #region Network
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        int[] colOffsets = { 0, -1, 1, -2, 2 };
        if (!IsOwner) return;
        base.Start();
        for (int i = 0; i < maxSoldierCount; i++)
        {
            soldierOffsets.Add(GetFormationOffset(i, colOffsets));
        }

        soldierSpawner = GetComponent<Spawner>();
        soldierSpawner.SpawnSoldiers(initialSoldiersCount);

        StartCoroutine(WaitForSpawnSoldiers());
    }

    IEnumerator WaitForSpawnSoldiers()
    {
        while (soldiers.Count < initialSoldiersCount)
        {
            yield return null;
        }

        for (int i = 0; i < initialSoldiersCount; i++)
        {
            soldiers[i].Offset = soldierOffsets[i];
        }
    }

    #endregion

    // 키 입력 메서드
    public override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            debugSoldier.HandleOnDie(debugSoldier.GetComponent<Health>());
            debugSoldier.gameObject.SetActive(false);
        }

        // E 버튼 누르면
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (FindNearestEnemy() != null)
            {
                Debug.Log("CommandSoldierToAdvance");
                CommandSoldierToAdvance();
            }
            else if (FindNearestOccupy() != null && HasSoldierWithItem())
            {
                Debug.Log("CommandSoldierToDeliverItem");
                CommandSoldierToDeliverItem();
            }
            else if (FindNearestItem() != null)
            {
                Debug.Log("CommandSoldierToPickupItem");
                CommandSoldierToPickupItem();
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CommandSoldierToReturn();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            int idx = GetAvailableSoldierIndex();
            soldierSpawner.index = idx == -1 ? soldiers.Count : idx;
            soldierSpawner.SpawnSoldiers(1);
        }
    }

    #region Find

    // 근처 점령지 찾기
    GameObject FindNearestOccupy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, occupyDetectRange, LayerMask.GetMask("Occupy"));
        GameObject nearestOccupy = null;
        float minDistance = Mathf.Infinity;

        var results = colliders
            .Select(col => col.GetComponent<OccupySystem>())
            .Where(o => o != null && !o.hasBuilding.Value)
            .ToArray();

        foreach (OccupySystem occupy in results)
        {
            if (occupy != null)
            {
                float distance = Vector3.Distance(transform.position, occupy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestOccupy = occupy.gameObject;
                }
            }
        }
        return nearestOccupy;
    }


    // 적을 찾는 메서드 (범위 내 가장 가까운 적)
    private GameObject FindNearestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 1f, LayerMask.GetMask("Enemy")); // 나중에 콜라이더로 수정
        GameObject nearestEnemy = null; // 가장 가까운 적을 저장할 오브젝트
        float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장. 초기 값은 무한대로 설정

        var enemies = colliders
            .Select(col => col.GetComponent<Character>())
            .Where(c => c != null && c.team.Value != team.Value)
            .ToArray();

        foreach (Character enemy in enemies) // 탐색된 모든 적 순회
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
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, itemDetectRange, LayerMask.GetMask("Item")); // (왕의 위치를 중심으로, ~)
        GameObject nearestItem = null; // 가까운 자원을 저장할 오브젝트
        float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장, 초기값은 무한대 

        var items = colliders
            .Select(col => col.GetComponent<ResourceData>())
            .Where(r => r != null && !r.isMarked && !r.isHolding.Value)
            .ToArray();

        foreach (ResourceData resource in items) // 탐색된 모든 자원 순회하며
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

    #endregion

    #region Command

    // 점령지로 자원 넣기
    void CommandSoldierToDeliverItem()
    {
        foreach (SoldierTest soldier in soldiers)
        {
            if (soldier == null) continue;
            GameObject occupy = FindNearestOccupy();
            if (soldier.HasItem)
            {
                soldier.TryDeliverItemToOccupy(occupy);
            }
        }
    }

    void CommandSoldierToPickupItem()
    {
        foreach (SoldierTest soldier in soldiers)
        {
            if (soldier == null) continue;
            // 명령을 받을 수 없는 상태
            if (!soldier.CanReceiveCommand)
            {
                continue;
            }

            GameObject item = FindNearestItem();

            if (item != null)
            {
                soldier.TryPickupItem(item);
            }
        }
    }

    void CommandSoldierToReturn()
    {
        foreach (SoldierTest soldier in soldiers)
        {
            if (soldier == null) continue;
            soldier.TryResetState();
        }
    }

    public void CommandSoldierToAdvance()
    {
        foreach (SoldierTest soldier in soldiers)
        {
            if (soldier == null) continue;
            if (!soldier.CanReceiveCommand)
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

    #endregion

    public void HandleSoldierDie(SoldierTest deadSoldier)
    {
        int index = soldiers.IndexOf(deadSoldier);

        if (soldiers[index + 5] != null)
        {
            soldiers[index] = soldiers[index + 5];
            soldiers[index + 5] = null;
            soldiers[index].Offset = soldierOffsets[index];
        }
        else
        {
            soldiers[index] = null;
        }
    }

    int GetAvailableSoldierIndex()
    {
        int availableSoldierIndex = -1;

        for (int i = 0; i < soldiers.Count; i++)
        {
            if (soldiers[i] == null)
            {
                availableSoldierIndex = availableSoldierIndex == -1 ? i : Mathf.Min(availableSoldierIndex, i);
            }
        }

        return availableSoldierIndex;
    }

    bool HasSoldierWithItem()
    {
        return soldiers.Any(soldier => soldier.HasItem);
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

}
