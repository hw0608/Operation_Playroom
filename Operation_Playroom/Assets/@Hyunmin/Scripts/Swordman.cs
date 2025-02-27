using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Swordman : Character
{
    public float attackCooldown = 2f;

    bool attackAble = true;
    IEnumerator attackRoutine;

    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        if (attackAble)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
            attackRoutine = SwordAttack();
            StartCoroutine(SwordAttack());
        }
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
        Debug.Log("Interaction");
    }

    // Į ���� �ڷ�ƾ
    IEnumerator SwordAttack()
    {
        SetAvatarLayerWeightserverRpc(1); // ��ü ���������� ����
        attackAble = false; // ����� ��Ȱ��ȭ
        SetTriggerAnimationserverRpc("SwordAttack"); // ���� ��� ����

        // 1.15�� ��Ÿ��
        yield return new WaitForSeconds(attackCooldown);

        attackAble = true; // ���� ����
        SetAvatarLayerWeightserverRpc(0); // ��ü ������ ����
    }
}
