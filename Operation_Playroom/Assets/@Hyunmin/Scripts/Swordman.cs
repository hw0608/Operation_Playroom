using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Swordman : Character
{
    public float attackCooldown = 0.5f;

    [SerializeField] GameObject swordHitbox;
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

    // ü�� ���� �޼���
    public override void SetHP()
    {
        maxHp = 150;
        currentHp = maxHp;
    }

    // Į ���� �ڷ�ƾ
    IEnumerator SwordAttack()
    {
        HandleLayerWeightserverRpc(1);
        attackAble = false; // ����� ��Ȱ��ȭ
        HandleAnimationserverRpc("Attack"); // ���� ��� ����

        // 0.2�� �� 0.2�� ���� �ݶ��̴� Ȱ��ȭ
        yield return new WaitForSeconds(0.2f);
        swordHitbox.GetComponent<Collider>().enabled = true;

        yield return new WaitForSeconds(0.2f);
        swordHitbox.GetComponent<Collider>().enabled = false;

        // 0.5�� ��Ÿ��
        yield return new WaitForSeconds(attackCooldown);
        attackAble = true; // ���� ����
        HandleLayerWeightserverRpc(0);
    }

    [ServerRpc]
    void HandleAnimationserverRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    [ServerRpc]
    void HandleLayerWeightserverRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

}
