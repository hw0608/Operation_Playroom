using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponDamage : NetworkBehaviour
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
        // 본인이 아닐 경우 리턴
        if (!IsOwner) return;

        // 본인을 타격했을경우 리턴
        if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
        {
            if (ownerClientId == obj.OwnerClientId)
            {
                return;
            }
        }
        if (other.TryGetComponent<Character>(out Character targetCharacter))
        {
            // 같은 팀이면 타격하지 않음
            //if (ownerTeam == targetCharacter.team.Value)
            //{
            //    Debug.Log("Same team, no damage applied.");
            //    return;
            //}
        }
        if (!isCollision)
        {
            isCollision = true;

            if (other.TryGetComponent<Health>(out Health health))
            {
                if (health.IsServer)
                {
                    Debug.Log("IsServer Damage");
                    health.TakeDamage(damage, ownerClientId);
                }
                else
                {
                    Debug.Log("else Damage");
                    health.TakeDamageServerRpc(damage, ownerClientId);
                }
                StartCoroutine(ResetCollisionRoutine());
            }
        }
    }

    IEnumerator ResetCollisionRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isCollision = false;
    }
}
