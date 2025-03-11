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
        // ������ ��������
        if (isHoldingItem)
        {
            Drop();
        }
        // ������ ���
        else
        {
            PickUp();
        }
    }

    // Į ���� �ڷ�ƾ
    IEnumerator SwordAttack()
    {
        swordHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId, team.Value);

        SetAvatarLayerWeight(1); // ��ü ���������� ����

        attackAble = false; // ����� ��Ȱ��ȭ
        holdItemAble = false;

        SetTriggerAnimation("SwordAttack"); // ���� ��� ����

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);

        yield return new WaitForSeconds(0.5f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true; // ���� ����
        holdItemAble = true;

        SetAvatarLayerWeight(0); // ��ü ������ ����
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
