using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class CommanderKing : King
{
    private SoldierSpawner soldierSpawner;
    private bool isCommand = false; // EŰ�� ���ȴ��� Ȯ��
    private bool isRetreat = false; // QŰ�� ���ȴ��� Ȯ��

    public override void OnNetworkSpawn()
    {
        base.Start(); // King Start() ȣ��
        soldierSpawner = GetComponent<SoldierSpawner>();
        
        if (IsOwner)
        {
            if (soldierSpawner != null)
            {
                soldierSpawner.SpawnSoldiers(); // ������� �ʱ�ȭ�ϰ� ��ġ
            }
            StartCoroutine(CommandCheckCoroutine());
        }
    }
    void Update()
    {
        if (!IsOwner) return;
        HandleInput();
    }

    public override void HandleInput() // Ű�� ���������� üũ
    {
        base.HandleInput();

        if (Input.GetKeyDown(KeyCode.E)) //E 
        {
            isCommand = true;
        }

        if (Input.GetKeyDown(KeyCode.Q)) //Q
        {
            isRetreat = true;
        }
    }

    private IEnumerator CommandCheckCoroutine() // ��� ȣ��
    {
        while (true)
        {
            
            if (isCommand)
            {
                CommandSoldiersServerRpc();
                isCommand = false;
            }

            if (isRetreat)
            {
                RetreatSoldiersServerRpc();
                isRetreat = false;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
    [ServerRpc]
    private void CommandSoldiersServerRpc()
    {
        // �ֺ� ������� ã�Ƽ� �ൿ ����
        GameObject[] soldiers = GameObject.FindGameObjectsWithTag("Soldier");
        foreach (var soldierObj in soldiers)
        {
            Soldier soldier = soldierObj.GetComponent<Soldier>(); // Ž���� �ݶ��̴� �� soldier ������Ʈ�� ���� ������Ʈ�� ������ �� 
            if (soldier != null && soldier.currentState is FollowingState) // ���簡 ����� ���� �� �ִ� ���¶�� 
            {
                float distance = Vector3.Distance(transform.position, soldier.transform.position);
                if (distance <= 5)
                {
                    // �ֺ� �� Ȯ��
                    GameObject enemy = FindNearestEnemy(); // �ֺ� �� ã�� �Լ� ȣ��
                    if (enemy != null)
                    {
                        soldier.SetState(1, enemy.GetComponent<NetworkObject>().NetworkObjectId); // ���� ���� ��� AttackingState�� ����
                    }
                    else
                    {
                        // �ֺ� �ڿ� Ȯ��
                        GameObject item = FindNearestItem();
                        if (item != null)
                        {
                            soldier.SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId); // �ڿ��� �ִٸ�..
                        }
                    }
                }
            }
        }
    }
    [ServerRpc]
    private void RetreatSoldiersServerRpc()
    {
        // ���� ���� ����鿡�� ���� ���
        GameObject[] attackingSoldiers = GameObject.FindGameObjectsWithTag("Soldier"); 
        foreach (var attackingSoldierObj in attackingSoldiers) 
        {
            Soldier soldier = attackingSoldierObj.GetComponent<Soldier>();
            if (soldier != null && soldier.currentState is AttackingState) // ���� ���� ���� �� 
            {
                soldier.SetState(0,0); // �������
            }
        }
    }

    // ���� ã�� �޼��� (���� �� ���� ����� ��)
    private GameObject FindNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Enemy")); // ���߿� �ݶ��̴��� ����
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
        Collider[] item = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Item")); // (���� ��ġ�� �߽�����, ~)
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

