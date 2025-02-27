using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Swordman : Character
{
    public float attackCooldown = 2f;

    bool attackAble = true;
    IEnumerator attackRoutine;

    // 공격 메서드
    public override void Attack()
    {
        // 검 휘두르며 공격
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

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 공격
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
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
        // 줍기
        Debug.Log("Interaction");
    }

    // 칼 공격 코루틴
    IEnumerator SwordAttack()
    {
        SetAvatarLayerWeightserverRpc(1); // 상체 움직임으로 설정
        attackAble = false; // 재공격 비활성화
        SetTriggerAnimationserverRpc("SwordAttack"); // 공격 모션 실행

        // 1.15초 쿨타임
        yield return new WaitForSeconds(attackCooldown);

        attackAble = true; // 공격 가능
        SetAvatarLayerWeightserverRpc(0); // 상체 움직임 해제
    }
}
