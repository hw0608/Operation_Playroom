using Unity.Netcode;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] int damage = 10;

    ulong ownerClientId;
    int ownerTeam;

    public void SetOwner(ulong ownerClientId, int ownerTeam)
    {
        this.ownerClientId = ownerClientId;
        this.ownerTeam = ownerTeam;
    }

    void OnTriggerEnter(Collider other)
    {
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
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage, ownerClientId);

            Managers.Pool.Push(gameObject);
        }

    }
}
