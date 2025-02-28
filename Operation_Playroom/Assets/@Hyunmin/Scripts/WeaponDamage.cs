using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [SerializeField] int damage = 10;

    ulong ownerClientId;
    bool isCollision;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
        {
            if (ownerClientId == obj.OwnerClientId)
            {
                Debug.Log("Network? ==");
                return;
            }
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            if (!isCollision)
            {
                StartCoroutine(SwordDamageRoutine(health));
            }
        }
    }

    IEnumerator SwordDamageRoutine(Health health)
    {
        isCollision = true;
        if (health.IsServer)
        {
            health.TakeDamage(damage, ownerClientId);
        }
        else
        {
            health.TakeDamageServerRpc(damage, ownerClientId);
        }

        yield return new WaitForSeconds(0.5f);

        isCollision = false;
    }
}
