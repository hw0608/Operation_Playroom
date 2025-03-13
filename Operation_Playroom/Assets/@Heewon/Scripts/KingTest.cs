using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KingTest : Character
{
    [SerializeField] int initialSoldiersCount;
    [SerializeField] float itemDetectRange;
    [SerializeField] float occupyDetectRange; // ������ ���� ����
    [SerializeField] float enemyDetectRange;
    [SerializeField] float soldierSpacing = 0.2f;
    [SerializeField] int maxSoldierCount = 10;

    Spawner soldierSpawner;
    public List<SoldierTest> soldiers = new List<SoldierTest>();
    public List<Vector3> soldierOffsets = new List<Vector3>();

    public SoldierTest debugSoldier;

    OccupyManager occupyManager;

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

        occupyManager = FindFirstObjectByType<OccupyManager>();

        if (team.Value == 0)
        {
            occupyManager.blueTeamOccupyCount.OnValueChanged -= HandleSoldierSpawn;
            occupyManager.blueTeamOccupyCount.OnValueChanged += HandleSoldierSpawn;
        }
        else
        {
            occupyManager.redTeamOccupyCount.OnValueChanged -= HandleSoldierSpawn;
            occupyManager.redTeamOccupyCount.OnValueChanged += HandleSoldierSpawn;
        }

        soldierSpawner = GetComponent<Spawner>();
        soldierSpawner.SpawnSoldiers(initialSoldiersCount);

        StartCoroutine(WaitForSpawnSoldiers());
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        if (team.Value == 0)
        {
            occupyManager.blueTeamOccupyCount.OnValueChanged -= HandleSoldierSpawn;
        }
        else
        {
            occupyManager.redTeamOccupyCount.OnValueChanged -= HandleSoldierSpawn;
        }
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

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // E ��ư ������
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

    // ��ó ������ ã��
    GameObject FindNearestOccupy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, occupyDetectRange, LayerMask.GetMask("Occupy"));
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


    // ���� ã�� �޼��� (���� �� ���� ����� ��)
    private GameObject FindNearestEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.4f, enemyDetectRange, LayerMask.GetMask("Enemy")); // ���߿� �ݶ��̴��� ����
        GameObject nearestEnemy = null; // ���� ����� ���� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����. �ʱ� ���� ���Ѵ�� ����

        Owner myTeam = team.Value == 0 ? Owner.Blue : Owner.Red;

        var enemies = colliders
            .SelectMany(col =>
                {
                    var character = col.GetComponent<Character>();
                    if (character != null && !character.GetComponent<Health>().isDead && character.team.Value != team.Value)
                        return new[] { character.gameObject };

                    var building = col.GetComponent<Building>();

                    if (building != null && building.buildingOwner != myTeam)
                        return new[] { building.gameObject };

                    return Enumerable.Empty<GameObject>();
                }).ToArray();

        foreach (GameObject enemy in enemies) // Ž���� ��� �� ��ȸ
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position); // ��, �� ���� �Ÿ��� ���
            if (distance < minDistance) // ���� ���� �Ÿ��� �ּ� �Ÿ����� ������ 
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ��� ������Ʈ
                nearestEnemy = enemy; // ����� �� ������Ʈ�� ���� ���� ������Ʈ�� ���� 
            }
        }
        return nearestEnemy; // ����� �� ������Ʈ�� ��ȯ
    }

    // �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    private GameObject FindNearestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, itemDetectRange, LayerMask.GetMask("Item")); // (���� ��ġ�� �߽�����, ~)
        GameObject nearestItem = null; // ����� �ڿ��� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����, �ʱⰪ�� ���Ѵ� 

        var items = colliders
            .Select(col => col.GetComponent<ResourceData>())
            .Where(r => r != null && !r.isMarked && !r.isHolding.Value)
            .ToArray();

        foreach (ResourceData resource in items) // Ž���� ��� �ڿ� ��ȸ�ϸ�
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position); // �հ� �� �ڿ��� �Ÿ� ���
            if (distance < minDistance) // ���� ���ȰŸ��� �ּ� �Ÿ����� ������
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ� ������Ʈ
                nearestItem = resource.gameObject; // ������Ʈ�� �ڿ� ����
            }
        }
        return nearestItem; // �ּҰŸ��ڿ� ������Ʈ ��ȯ
    }

    #endregion

    #region Command

    // �������� �ڿ� �ֱ�
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
            // ����� ���� �� ���� ����
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

    void HandleSoldierSpawn(int previousValue, int newValue)
    {
        Debug.Log("HandleSoldierSpawn");
        if (newValue > previousValue)
        {
            Debug.Log("SpawnSoldier.");
            soldierSpawner.SpawnSoldiers(1);
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

    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        Debug.Log("Sword Attack");
    }
    // ��ȣ�ۿ� �޼���
    public override void Interaction()
    {
        // �ݱ�
        Debug.Log("King Interaction");
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.5f, itemDetectRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.5f, occupyDetectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 0.4f, enemyDetectRange);
    }
}
