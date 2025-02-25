using UnityEngine;

public class TestEnemyHealth : MonoBehaviour
{
    public int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("�׾���");
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            //soldier.TakeDamage(10);
            // Soldier���� 10 ������
        }
    }
}


