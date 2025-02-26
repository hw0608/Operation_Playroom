using System.Threading;
using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public bool isDead;

    public Action<Health> OnDie;
    public static Action<ulong, int> OnScored;

    Character character;

    public void Start()
    {
        character = GetComponent<Character>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        currentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damage, ulong clientId)
    {
        ModifyHealth(-damage, clientId);
        character.TakeDamage(damage, clientId);
    }

    public void RestoreHealth(int heal, ulong clientId)
    {
        ModifyHealth(heal, clientId);
    }

    void ModifyHealth(int value, ulong clientId)
    {
        if (isDead) { return; }
        int newHealth = currentHealth.Value + value;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value == 0)
        {
            isDead = true;
            character.Die();

            if (GetComponent<PlayerController>() != null)
            {
            }
            else if (GetComponent<Character>() != null)
            {
            }

            OnDie?.Invoke(this);

        }
    }
}
