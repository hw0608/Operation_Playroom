using UnityEngine;

public interface ICommand
{
    void Execute(SoldierTest soldier, GameObject target);
    bool CanExecute(SoldierTest soldier, GameObject target);
}
