using Unity.Netcode;
using UnityEngine;

public interface ISoldierState 
{
    void EnterState(Soldier soldier); 
    void Execute(Soldier soldier); 
}
// 왕 따라가기
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
// 공격
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
// 자원 수집
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
// 왕에게 돌아가기
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

            // 왕에게 도착했는지 확인
            if (Vector3.Distance(soldier.transform.position, king.position) < 1.5f)
            {
                // 자원 전달 및 상태 전환
                DeliverResource(soldier);
                soldier.SetState(0);
            }
        }
    }

    private void DeliverResource(Soldier soldier)
    {
        // 임시
        TestItemCollection.Instance.AddResource("Wood", 1);
        soldier.itemTarget = null; // 자원 초기화
    }
// 병사 죽음
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

