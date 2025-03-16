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
            // ������ �ƴ� ��� ����
            if (!IsOwner) return;

            // ������ Ÿ��������� ����
            if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
            {
                if (ownerClientId == obj.OwnerClientId)
                {
                    return;
                }
            }

            // ���� �� Ÿ�� �� ����
            if (other.TryGetComponent<Character>(out Character character))
            {
                if (ownerTeam == character.team.Value)
                {
                    Debug.Log("Team Kill");
                    return;
                }
            }

            // �ǹ� Ÿ�� �� ������
            if (other.TryGetComponent<Building>(out Building building))
            {
                // ���� �� �ǹ� Ÿ�� ��
                Owner myTeam = ownerTeam == 0 ? Owner.Blue : Owner.Red;

                if (myTeam != building.buildingOwner)
                {
                    isCollision = true;

                    building.TakeDamageServerRpc(damage);
                    if (building.isDestruction)
                    {
                        Debug.Log("destroy building!");
                        GameManager.Instance.myPlayData.destroy++;
                    }
                    NoiseCheckManager noise = FindFirstObjectByType<NoiseCheckManager>();
                    noise.AddNoiseGage(2);

                    StartCoroutine(ResetCollisionRoutine());
                }
            }

            // ��� �� Ÿ�� �� ������
            if (other.TryGetComponent<Health>(out Health health))
            {
                isCollision = true;

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
                if (health.isDead)
                {
                    Debug.Log("kill Enemy!");
                    GameManager.Instance.myPlayData.kill++;
                }
                NoiseCheckManager noise = FindFirstObjectByType<NoiseCheckManager>();
                noise.AddNoiseGage(2);

                StartCoroutine(ResetCollisionRoutine());
            }
        }
    }

    // ������ �ݶ��̴��� ��Ÿ���� �����ϴ� ��ƾ
    IEnumerator ResetCollisionRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isCollision = false;
    }
}
