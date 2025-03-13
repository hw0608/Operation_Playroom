using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : NetworkBehaviour
{
    public int maxHealth = 100;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public bool isDead;

    public Action<Health> OnDie;

    [SerializeField] Image hpBar;
    Character character;

    void Start()
    {
        character = GetComponent<Character>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        if (IsClient && IsOwner)
        {
            StartCoroutine(FindHpbar());
        }

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public void InitializeHealth()
    {
        Debug.Log("Restore");
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
        isDead = false;
    }

    public void TakeDamage(int damage, ulong clientId)
    {
        ModifyHealth(-damage, clientId);
        character.TakeDamage();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage, ulong clientId)
    {
        TakeDamage(damage, clientId);
    }

    public void RestoreHealth(int heal, ulong clientId)
    {
        ModifyHealth(heal, clientId);
    }

    void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsClient && IsOwner)
        {
            UpdateHpbar();
        }

        if (newValue == 0)
        {
            isDead = true;

            if(GetComponent<PlayerController>() != null)
            {
                GetComponent<PlayerController>().isPlayable = false;
            }
        }
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
                GetComponent<PlayerController>().isPlayable = false;
            }
            else if (GetComponent<Character>() != null)
            {
            }

            OnDie?.Invoke(this);
        }
    }
    IEnumerator FindHpbar()
    {
        yield return new WaitUntil(() => GameObject.FindWithTag("HPBar"));

        hpBar = GameObject.FindWithTag("HPBar").GetComponent<Image>();
    }

    void UpdateHpbar()
    {
        hpBar.fillAmount = (float)currentHealth.Value / maxHealth;
        Debug.Log(hpBar.fillAmount);
    }
}
