using UnityEngine;

public class King : Character
{
    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        Debug.Log("Sword Attack");
    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ����
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
        // �ݱ�
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // ��ȣ�ۿ� �޼���
    public override void Interaction()
    {
        // �ݱ�
        Debug.Log("King Interaction");
    }

    // ü�� ���� �޼���
    public override void SetHP()
    {
        maxHp = 150;
        currentHp = maxHp;
    }
}
