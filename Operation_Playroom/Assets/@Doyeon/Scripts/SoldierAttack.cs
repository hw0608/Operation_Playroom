using UnityEngine;

public class SoldierAttack : MonoBehaviour
{
    public float attackRange = 2.0f;
    public int damage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    private void Update()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Enemy"));
        if (enemies.Length > 0 && Time.time - lastAttackTime > attackRange)
        {
            Attack(enemies[0].gameObject);
            lastAttackTime = Time.time;
        }
    }
    void Attack(GameObject enemy)
    {
        Debug.Log("╬Нец");
    }
}
