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
                return;
            }
        }
        if (!isCollision)
        {
            isCollision = true;

            // 충돌한 객체가 Health를 가지고 있으면 데미지 처리
            if (other.TryGetComponent<Health>(out Health health))
            {
                // 서버에서 데미지 처리
                if (health.IsServer) return;
                StartCoroutine(SwordDamageRoutine(health));
            }
        }
    }

    IEnumerator SwordDamageRoutine(Health health)
    {
        isCollision = true;
        if (health.IsServer)
        {
            Debug.Log("Client Attack");
            health.TakeDamage(damage, ownerClientId);
        }
        else if(health.IsClient)
        {
            Debug.Log("Server Attack");
            health.TakeDamageServerRpc(damage, ownerClientId);
        }

        yield return new WaitForSeconds(0.5f);

        isCollision = false;
    }
}
