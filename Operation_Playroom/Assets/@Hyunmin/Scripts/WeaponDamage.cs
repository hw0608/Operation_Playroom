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

        // ��� �� Ÿ�� �� ������
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

    // ������ �ݶ��̴��� ��Ÿ���� �����ϴ� ��ƾ
    IEnumerator ResetCollisionRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isCollision = false;
    }
}
