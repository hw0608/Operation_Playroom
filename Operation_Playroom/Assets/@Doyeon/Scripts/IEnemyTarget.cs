using UnityEngine;

public interface IEnemyTarget
{
    Transform GetTransform();  // ���� Transform ��������
    int GetTeam();             // ���� �� ���� ��������
}
