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

    // 체력 적용 메서드
    public override void SetHP()
    {
        maxHp = 150;
        currentHp = maxHp;
    }

    // 칼 공격 코루틴
    IEnumerator SwordAttack()
    {
        HandleLayerWeightserverRpc(1);
        attackAble = false; // 재공격 비활성화
        HandleAnimationserverRpc("Attack"); // 공격 모션 실행

        // 0.2초 후 0.2초 동안 콜라이더 활성화
        yield return new WaitForSeconds(0.2f);
        swordHitbox.GetComponent<Collider>().enabled = true;

        yield return new WaitForSeconds(0.2f);
        swordHitbox.GetComponent<Collider>().enabled = false;

        // 0.5초 쿨타임
        yield return new WaitForSeconds(attackCooldown);
        attackAble = true; // 공격 가능
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
