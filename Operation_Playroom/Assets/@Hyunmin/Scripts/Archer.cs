using Unity.Cinemachine;
using UnityEngine;

public class Archer : Character
{
    bool isAiming;

    // 공격 메서드
    public override void Attack()
    {
        // 화살 발사
        if (isAiming)
        {
            Debug.Log("Archer Attack");
        }
    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 조준 시작
        if (Input.GetButtonDown("Aim"))
        {
            isAiming = true;
        }
        // 조준 취소
        if (Input.GetButtonUp("Aim"))
        {
            isAiming = false;
        }
        // 발사
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
        maxHp = 80;
        currentHp = maxHp;
    }
}

