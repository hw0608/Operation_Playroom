using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponDamage : NetworkBehaviour
{
    [SerializeField] int damage = 10;

    ulong ownerClientId;
    int ownerTeam;

    bool isCollision;

    public void SetOwner(ulong ownerClientId, int ownerTeam)
    {
        this.ownerClientId = ownerClientId;
        this.ownerTeam = ownerTeam;
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

        // 같은 팀 타격 시 리턴
        if (other.TryGetComponent<Character>(out Character character))
        {
            if (ownerTeam == character.team.Value)
            {
                Debug.Log("Team Kill");
                return;
            }
        }

        // 상대 팀 타격 시 데미지
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

    // 데미지 콜라이더에 쿨타임을 적용하는 루틴
    IEnumerator ResetCollisionRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isCollision = false;
    }
}
