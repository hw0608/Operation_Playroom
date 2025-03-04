using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Swordman : Character
{
    [SerializeField] GameObject swordHitbox;
    public float attackCooldown = 1f;

    IEnumerator attackRoutine;

    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
        attackRoutine = SwordAttack();
        StartCoroutine(SwordAttack());
    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ����
        if (Input.GetButtonDown("Attack"))
        {
            // �� �ֵθ��� ����
            if (attackAble)
            {
                Attack();
            }
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
        // �������� ��� ������
        if (isHoldingItem)
        {
            Drop();
            attackAble = true;
        }
        else
        {
            PickUp();
            attackAble = false;
        }
    }

    // Į ���� �ڷ�ƾ
    IEnumerator SwordAttack()
    {
        Debug.Log("sword");
        swordHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId);
        SetAvatarLayerWeightserverRpc(1); // ��ü ���������� ����
        attackAble = false; // ����� ��Ȱ��ȭ
        SetTriggerAnimationserverRpc("SwordAttack"); // ���� ��� ����

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);

        yield return new WaitForSeconds(0.2f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true; // ���� ����
        SetAvatarLayerWeightserverRpc(0); // ��ü ������ ����
    }

    [ServerRpc]
    void EnableHitboxServerRpc(bool state)
    {
        EnableHitboxClientRpc(state);
    }

    [ClientRpc]
    void EnableHitboxClientRpc(bool state)
    {
        swordHitbox.GetComponent<Collider>().enabled = state;
    }
}
