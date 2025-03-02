using System.Linq;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using static ReturningState;

// 네트워크상 상태전환을 관리, 상태코드 <-> 상태객체 변환
public static class NSoldierState
{
    // 상태 객체를 정수 코드로 변환
    public static int GetStateInt(ISoldierState state)
    {
        if (state is IdleState) return 0;
        if (state is FollowingState) return 1;
        if (state is AttackingState) return 2;
        if (state is CollectingState) return 3;
        if (state is ReturningState) return 4;
        if (state is SoldierDieState) return 5;
        return -1;
    }
    // 정수 코드를 상태 객체로 변환
    public static ISoldierState GetStateFromInt(int state, Transform king, NetworkObjectReference itemTarget, int myTeam)
    {
        if (state == 2)
        {
            IEnemyTarget[] allEnemies = (IEnemyTarget[])Object.FindObjectsOfType<MonoBehaviour>().OfType<IEnemyTarget>();
            foreach (IEnemyTarget potentialEnemy in allEnemies)
            {
                if (potentialEnemy.GetTeam() != myTeam)
                {
                    return new AttackingState(potentialEnemy.GetTransform());
                }
            }
        }

        NetworkObject itemNetworkObject;

        // 아이템이 존재할 때
        if (itemTarget.TryGet(out itemNetworkObject) && itemNetworkObject != null)
        {
            Transform itemTransform = itemNetworkObject.transform;

            switch (state)
            {
                case 3: return new CollectingState(itemTransform);
                case 4: return new ReturningState(king, itemTransform);
            }
        }

        if (state == 1) return new FollowingState(king);
        if (state == 5) return new SoldierDieState();

        return new IdleState(); // 기본 값
    }
}
