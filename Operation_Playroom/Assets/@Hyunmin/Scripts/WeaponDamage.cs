using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponDamage : NetworkBehaviour
{
    [SerializeField] int damage;

    ulong ownerClientId;
    int ownerTeam;

    bool isCollision;
    NoiseCheckManager noise;
    public void SetOwner(ulong ownerClientId, int ownerTeam, int damage)
    {
        this.ownerClientId = ownerClientId;
        this.ownerTeam = ownerTeam;
        this.damage = damage;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollision) 
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
                if (character.team.Value == -1)
                {
                    Debug.Log("Unknown Team");
                    return;
                }
                if (ownerTeam == character.team.Value)
                {
                    Debug.Log("Team Kill");
                    return;
                }
            }

            // 건물 타격 시 데미지
            if (other.TryGetComponent<Building>(out Building building))
            {
                // 같은 팀 건물 타격 시
                Owner myTeam = ownerTeam == 0 ? Owner.Blue : Owner.Red;

                if (myTeam != building.buildingOwner)
                {
                    isCollision = true;

                    building.TakeDamageServerRpc(damage, ownerClientId);
                    GetComponentInParent<Character>().SwordSound();

                    if(noise ==null)
                        noise = FindFirstObjectByType<NoiseCheckManager>();
                    noise.AddNoiseGage(1.5f);

                    StartCoroutine(ResetCollisionRoutine());
                }
            }

            // 상대 팀 타격 시 데미지
            if (other.TryGetComponent<Health>(out Health health))
            {
                isCollision = true;
                GetComponentInParent<Character>().SwordSound();

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
                if (noise == null)
                    noise = FindFirstObjectByType<NoiseCheckManager>();
                noise.AddNoiseGage(1.5f);

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
