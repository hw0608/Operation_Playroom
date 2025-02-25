using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField] GameObject projectile;
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
        GameObject arrow = Managers.Pool.Pop(projectile);
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId);

        // Ŭ���̾�Ʈ�� ����ȭ
        FireClientRpc(spawnPoint, direction);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction, trailprefab);
    }

    [ClientRpc]
    void FireClientRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // Ŭ���̾�Ʈ���� ���� ȭ�� ����
        if (IsOwner) return;  // ������ Ŭ���̾�Ʈ������ ���̸� ������ ����

        GameObject arrow = Managers.Pool.Pop(projectile);
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction, trailprefab);
    }

}
