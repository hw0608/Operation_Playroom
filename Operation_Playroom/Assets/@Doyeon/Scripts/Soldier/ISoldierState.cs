using Unity.Netcode;
using UnityEngine;

public interface ISoldierState 
{
    void EnterState(Soldier soldier); 
    void Execute(Soldier soldier); 
}
// �� ���󰡱�
public class FollowingState : ISoldierState
{
    public void EnterState(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        }
    }
    public void Execute(Soldier soldier)
    {   
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierFormation>().FollowKing();
        }
    }
}
// ����
public class AttackingState : ISoldierState
{
    private Transform target;
    public AttackingState(Transform target) { this.target = target; }

    public void EnterState(Soldier soldier)
    {
        NetworkObject enemyNetworkObject;
        if (soldier.enemyTarget.Value.TryGet(out enemyNetworkObject))
        {
            Transform enemyTransform = enemyNetworkObject.transform;
            target = enemyTransform;
            soldier.GetComponent<SoldierAnim>().SoldierAttackAnim();
        }
    }

    public void Execute(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierAttack>().AttackTarget();
        }
    }
}
// �ڿ� ����
public class CollectingState : ISoldierState
{
    private Transform item;
    public CollectingState(Transform itme) { this.item = itme; }

    public void EnterState(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            NetworkObject itemNetworkObject = item.GetComponent<NetworkObject>();
            soldier.itemTarget.Value = itemNetworkObject;
        }
    }

    public void Execute(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierFormation>().MoveToItemServerRpc(item.position);
        }
    }
}
// �տ��� ���ư���
public class ReturningState : ISoldierState
{
    private Transform king; 
    private Transform collectedItem; 

    public ReturningState(Transform king, Transform collectedItem)
    {
        this.king = king;
        this.collectedItem = collectedItem;
    }

    public void EnterState(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierAnim>().SoldierWalkAnim();
        }
    }

    public void Execute(Soldier soldier)
    {
        if (soldier.IsServer)
        {
            soldier.GetComponent<SoldierFormation>().ReturnToKingServerRpc(king.position);

            // �տ��� �����ߴ��� Ȯ��
            if (Vector3.Distance(soldier.transform.position, king.position) < 1.5f)
            {
                // �ڿ� ���� �� ���� ��ȯ
                DeliverResource(soldier);
                soldier.SetState(0);
            }
        }
    }

    private void DeliverResource(Soldier soldier)
    {
        // �ӽ�
        TestItemCollection.Instance.AddResource("Wood", 1);
        soldier.itemTarget = null; // �ڿ� �ʱ�ȭ
    }
// ���� ����
public class SoldierDieState : ISoldierState
{
        public void EnterState(Soldier soldier)
        {
            if (soldier.IsServer)
            {
                soldier.GetComponent<SoldierAnim>().SoldierDieAnim();
                soldier.isSoldierDie = true;
                soldier.enabled = false;
                soldier.GetComponent<Collider>().enabled = false;
                soldier.GetComponent<Rigidbody>().isKinematic = true;

                soldier.GetComponent<NetworkObject>().Despawn();
            }
        }
        public void Execute(Soldier soldier)
        {

        }
    }
}

