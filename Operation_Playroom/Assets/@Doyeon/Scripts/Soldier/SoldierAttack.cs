using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SoldierAttack : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private SoldierAnim soldierAnim;
    private Soldier soldier;
    [SerializeField] private float attackRange = 2.0f; // 공격 범위
    [SerializeField] private float attackCooldown = 1.5f; // 공격 쿨타임
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

        // 네트워크 변수(enemyTarget)가 유효한지 확인
        if (!soldier.enemyTarget.Value.TryGet(out enemyNetObj) || enemyNetObj == null)
        {
            // 적이 없으면 상태 복귀
            //soldier.SetState(0);
            return;
        }


        // 적 Transform 에 접근
        Transform enemyTransform = enemyNetObj.transform;

        // 공격
        navAgent.SetDestination(enemyTransform.position);

        if (Vector3.Distance(transform.position, enemyTransform.position) < attackRange)
        {
            navAgent.ResetPath(); // 공격 중에는 멈추기

            if (Time.time > lastAttackTime + attackCooldown)
            {
                soldierAnim.SoldierAttackAnim();
                lastAttackTime = Time.time;

                // 적의 체력 감소
                TestEnemyHealth enemyHealth = enemyNetObj.GetComponent<TestEnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(5); // 데미지 
                }
            }
        }
    }

}
