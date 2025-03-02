using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SoldierAttack : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private SoldierAnim soldierAnim;
    private Soldier soldier;
    [SerializeField] private float attackRange = 2.0f; // ���� ����
    [SerializeField] private float attackCooldown = 1.5f; // ���� ��Ÿ��
    private float lastAttackTime;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        soldierAnim = GetComponent<SoldierAnim>();
        soldier = GetComponent<Soldier>();
    }

    public void AttackTarget()
    {
        NetworkObject enemyNetObj;

        // ��Ʈ��ũ ����(enemyTarget)�� ��ȿ���� Ȯ��
        if (!soldier.enemyTarget.Value.TryGet(out enemyNetObj) || enemyNetObj == null)
        {
            // ���� ������ ���� ����
            //soldier.SetState(0);
            return;
        }


        // �� Transform �� ����
        Transform enemyTransform = enemyNetObj.transform;

        // ����
        navAgent.SetDestination(enemyTransform.position);

        if (Vector3.Distance(transform.position, enemyTransform.position) < attackRange)
        {
            navAgent.ResetPath(); // ���� �߿��� ���߱�

            if (Time.time > lastAttackTime + attackCooldown)
            {
                soldierAnim.SoldierAttackAnim();
                lastAttackTime = Time.time;

                // ���� ü�� ����
                TestEnemyHealth enemyHealth = enemyNetObj.GetComponent<TestEnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(5); // ������ 
                }
            }
        }
    }

}
