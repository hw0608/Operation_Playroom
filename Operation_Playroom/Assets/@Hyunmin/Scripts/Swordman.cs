using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Swordman : Character
{
    [SerializeField] GameObject swordHitbox;
    public float attackCooldown = 1f;

    IEnumerator attackRoutine;

    // 공격 메서드
    public override void Attack()
    {
        // 검 휘두르며 공격
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
        attackRoutine = SwordAttack();
        StartCoroutine(SwordAttack());
    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 공격
        if (Input.GetButtonDown("Attack"))
        {
            // 검 휘두르며 공격
            if (attackAble)
            {
                Attack();
            }
        }
        // 줍기
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // 상호작용 메서드
    public override void Interaction()
    {
        // 아이템 내려놓기
        if (isHoldingItem)
        {
            Drop();
        }
        // 아이템 들기
        else
        {
            PickUp();
        }
    }

    // 칼 공격 코루틴
    IEnumerator SwordAttack()
    {
        swordHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId, team.Value);

        SetAvatarLayerWeight(1); // 상체 움직임으로 설정

        attackAble = false; // 재공격 비활성화
        holdItemAble = false;

        SetTriggerAnimation("SwordAttack"); // 공격 모션 실행

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);

        yield return new WaitForSeconds(0.5f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true; // 공격 가능
        holdItemAble = true;

        SetAvatarLayerWeight(0); // 상체 움직임 해제
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
