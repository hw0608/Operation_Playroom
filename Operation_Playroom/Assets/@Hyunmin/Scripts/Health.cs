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

    Character character;

    void Start()
    {
        character = GetComponent<Character>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        currentHealth.Value = maxHealth;
        //currentHealth.OnValueChanged += OnHealthChanged;
    }

    public void TakeDamage(int damage, ulong clientId)
    {
        ModifyHealth(-damage, clientId);
        character.TakeDamage();
    }

    public void RestoreHealth(int heal, ulong clientId)
    {
        ModifyHealth(heal, clientId);
    }

    void OnHealthChanged(int previousValue, int newValue)
    {
        //throw new NotImplementedException();
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
