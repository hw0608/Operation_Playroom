using UnityEngine;

public class DeliverItemCommand : ICommand
{
    public bool CanExecute(SoldierTest soldier, GameObject target)
    {
        return soldier != null && soldier.HasItem && target != null && target.CompareTag("Occupy");
    }

    public void Execute(SoldierTest soldier, GameObject target)
    {
        soldier.TryDeliverItemToOccupy(target);
    }
}
