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
        Debug.Log("죽었당");
        Destroy(gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soldier"))
        {
            Soldier soldier = other.GetComponent<Soldier>();
            //soldier.TakeDamage(10);
            // Soldier에게 10 데미지
        }
    }
}


