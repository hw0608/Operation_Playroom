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
        // 서버에서 실제 발사체 생성
        GameObject arrow = Managers.Pool.Pop(projectile);
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId);

        // 클라이언트에 동기화
        FireClientRpc(spawnPoint, direction);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction, trailprefab);
    }

    [ClientRpc]
    void FireClientRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // 클라이언트에서 더미 화살 생성
        if (IsOwner) return;  // 소유자 클라이언트에서는 더미를 만들지 않음

        GameObject arrow = Managers.Pool.Pop(projectile);
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction, trailprefab);
    }

}
