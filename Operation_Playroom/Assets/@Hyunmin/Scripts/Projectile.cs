using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 3f;
    public float gravity = 0.75f;
    public float flightTime = 3f;

    bool Iscollision = false;

    Coroutine arrowCoroutine;
    GameObject trail;

    // 화살 발사 메서드
    public void Launch(Vector3 shootPoint, Vector3 direction, GameObject trailPrefab)
    {
        transform.position = shootPoint;
        transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);

        // 궤적 생성
        if (trail != null)
        {
            Destroy(trail);
        }
        trail = Instantiate(trailPrefab, transform);

        // 발사 루틴 시작
        arrowCoroutine = StartCoroutine(ArrowParabolaRoutine(direction));
    }

    // 화살 발사 루틴
    IEnumerator ArrowParabolaRoutine(Vector3 direction)
    {
        Vector3 velocity = direction * speed;
        float time = 0;

        while (time < flightTime && !Iscollision)
        {
            time += Time.deltaTime;

            // 이동
            transform.position += velocity * Time.deltaTime;

            // 중력 적용
            velocity.y -= gravity * Time.deltaTime;

            yield return null;
        }
        Destroy(trail);
        Managers.Pool.Push(gameObject);
    }

    // 벽, 구조물에 충돌 시 호출
    void OnTriggerEnter(Collider other)
    {
        if (Iscollision) return; // 이미 충돌상태 일때
        if (other.GetComponent<NetworkObject>()) return; // 플레이어 피격 시

        Iscollision = true;

        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
        }

        StartCoroutine(RemoveArrowRoutine());
    }

    // 화살 삭제 루틴
    IEnumerator RemoveArrowRoutine()
    {
        yield return new WaitForSeconds(3);

        Iscollision = false;

        Managers.Pool.Push(gameObject);
    }
}
