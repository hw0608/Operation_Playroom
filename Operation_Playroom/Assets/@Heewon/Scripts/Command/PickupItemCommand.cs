using UnityEngine;

public class PickupItemCommand : ICommand
{
    public bool CanExecute(SoldierTest soldier, GameObject target)
    {
        return soldier != null && soldier.CanReceiveCommand && target != null && target.CompareTag("Item");
    }

    public void Execute(SoldierTest soldier, GameObject target)
    {
        soldier.TryPickupItem(target);
    }
}
