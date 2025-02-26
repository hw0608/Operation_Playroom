using Unity.Netcode;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] int damage = 10;

    ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == null)
        {
            return;
        }

        if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
        {
            if (ownerClientId == obj.OwnerClientId)
            {
                return;
            }
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage, ownerClientId);
        }
    }
}
