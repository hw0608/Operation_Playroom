using System.Linq;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using static ReturningState;

// ��Ʈ��ũ�� ������ȯ�� ����, �����ڵ� <-> ���°�ü ��ȯ
public static class NSoldierState
{
    // ���� ��ü�� ���� �ڵ�� ��ȯ
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
    // ���� �ڵ带 ���� ��ü�� ��ȯ
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

        // �������� ������ ��
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

        return new IdleState(); // �⺻ ��
    }
}
