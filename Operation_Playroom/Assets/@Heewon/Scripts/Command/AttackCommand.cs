using UnityEngine;

public class AttackCommand : ICommand
{
    public bool CanExecute(SoldierTest soldier, GameObject target)
    {
        return soldier != null && soldier.CanReceiveCommand && target != null && target.CompareTag("Enemy");
    }

    public void Execute(SoldierTest soldier, GameObject target)
    {
        soldier.TryAttack(target);
    }
}
