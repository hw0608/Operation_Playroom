using Unity.Cinemachine;
using UnityEngine;

public class Archer : Character
{
    bool isAiming;

    // ���� �޼���
    public override void Attack()
    {
        // ȭ�� �߻�
        if (isAiming)
        {
            Debug.Log("Archer Attack");
        }
    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ���� ����
        if (Input.GetButtonDown("Aim"))
        {
            isAiming = true;
        }
        // ���� ���
        if (Input.GetButtonUp("Aim"))
        {
            isAiming = false;
        }
        // �߻�
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
        Debug.Log("Interaction");
    }

    // ü�� ���� �޼���
    public override void SetHP()
    {
        maxHp = 80;
        currentHp = maxHp;
    }
}

