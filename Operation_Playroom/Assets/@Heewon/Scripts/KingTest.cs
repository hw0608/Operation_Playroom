using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class KingTest : Character
{
    [SerializeField] int initialSoldiersCount;
    [SerializeField] float detectItemRange;
    [SerializeField] float occupyDetectRange; // ������ ���� ���� 

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

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // E ��ư ������
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
    // �������� �ڿ� �ֱ�
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
    // ��ó ������ ã��
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
            // �������� ��������� ����� ���� �� ���� ����
            if (soldier.isHoldingItem || soldier.CurrentState.Value != State.Idle && soldier.CurrentState.Value != State.Following)
            {
                continue;
            }

            GameObject item = FindNearestItem();

            if (item != null)
            {
                // TODO: ����
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

    // ���� ã�� �޼��� (���� �� ���� ����� ��)
    private GameObject FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 1f, LayerMask.GetMask("Enemy")); // ���߿� �ݶ��̴��� ����
        GameObject nearestEnemy = null; // ���� ����� ���� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����. �ʱ� ���� ���Ѵ�� ����

        foreach (Collider enemy in enemies) // Ž���� ��� �� ��ȸ
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position); // ��, �� ���� �Ÿ��� ���
            if (distance < minDistance) // ���� ���� �Ÿ��� �ּ� �Ÿ����� ������ 
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ��� ������Ʈ
                nearestEnemy = enemy.gameObject; // ����� �� ������Ʈ�� ���� ���� ������Ʈ�� ���� 
            }
        }
        return nearestEnemy; // ����� �� ������Ʈ�� ��ȯ
    }

    // �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    private GameObject FindNearestItem()
    {
        Collider[] item = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, detectItemRange, LayerMask.GetMask("Item")); // (���� ��ġ�� �߽�����, ~)
        GameObject nearestItem = null; // ����� �ڿ��� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����, �ʱⰪ�� ���Ѵ� 

        foreach (Collider resource in item) // Ž���� ��� �ڿ� ��ȸ�ϸ�
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
}


