using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField] GameObject serverProjectile;
    [SerializeField] GameObject clientProjectile;
    [SerializeField] GameObject trailprefab;

    float speed = 3f;
    float gravity = 0.75f;
    public float flightTime = 3f;


    public void ShootArrow(Transform shootPoint)
    {
        if (!IsOwner) return;

        FireServerRpc(shootPoint.position, shootPoint.forward);
    }

    [ServerRpc]
    void FireServerRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // �������� ���� �߻�ü ����
        GameObject arrow = Managers.Pool.Pop(serverProjectile);
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction);

        // Ŭ���̾�Ʈ�� ����ȭ
        FireClientRpc(spawnPoint, direction); 
    }

    [ClientRpc]
    void FireClientRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // Ŭ���̾�Ʈ���� ���� ȭ�� ����

        GameObject arrow = Managers.Pool.Pop(clientProjectile);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction, trailprefab);
    }

}
