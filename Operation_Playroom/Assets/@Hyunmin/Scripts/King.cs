using UnityEngine;

public class King : Character
{
    // 공격 메서드
    public override void Attack()
    {
        // 검 휘두르며 공격
        Debug.Log("Sword Attack");
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
        Debug.Log("King Interaction");
    }

    // 체력 적용 메서드
    public override void SetHP()
    {
        maxHp = 150;
        currentHp = maxHp;
    }
}
