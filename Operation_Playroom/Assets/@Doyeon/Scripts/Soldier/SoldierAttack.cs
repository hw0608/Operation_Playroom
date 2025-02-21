using UnityEngine;

public class SoldierAttack : MonoBehaviour
{
    [SerializeField] float attackRange = 0.15f; // 공격 범위
    [SerializeField] int damage = 10; // 공격력
    [SerializeField] float attackCooldown = 1.5f; // 공격 쿨타임
    private float lastAttackTime = 0f; // 마지막 공격 시간

    private void Update()
    {
        // 공격 범위 내 적 찾기 
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0 && Time.time - lastAttackTime > attackCooldown)
        {

            Attack(enemies[0].gameObject);
            lastAttackTime = Time.time;
        }
    }
    void Attack(GameObject enemy)
    {
        Debug.Log("Attack :" + enemy.name);

        //if (IsServer)
        //{
        //    networkState.Value = State.Attack;
        //    if (enemy.TryGetComponent(out Health enemyHealth))
        //    {
        //        enemyHealth.TakeDamage(damage);
        //    }
        //}
    }
}
