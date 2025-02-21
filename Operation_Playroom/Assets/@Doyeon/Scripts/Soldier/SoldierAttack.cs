using UnityEngine;

public class SoldierAttack : MonoBehaviour
{
    [SerializeField] float attackRange = 0.15f; // ���� ����
    [SerializeField] int damage = 10; // ���ݷ�
    [SerializeField] float attackCooldown = 1.5f; // ���� ��Ÿ��
    private float lastAttackTime = 0f; // ������ ���� �ð�

    private void Update()
    {
        // ���� ���� �� �� ã�� 
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
