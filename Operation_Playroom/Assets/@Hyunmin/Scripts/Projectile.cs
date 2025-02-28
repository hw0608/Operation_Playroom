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
    TrailRenderer trail;

    // 화살 발사 메서드
    public void Launch(Vector3 shootPoint, Vector3 direction, TrailRenderer trailPrefab = null)
    {
        transform.position = shootPoint;
        transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);

        // 궤적 생성
        if(trailPrefab != null)
        {
            trail = Instantiate(trailPrefab, transform);
            trail.enabled = false;
            trail.Clear();
            trail.enabled = true;

        }

        // 발사 루틴 시작
        if(arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
            arrowCoroutine = null;
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<Projectile>() && !other.GetComponent<NetworkBehaviour>())
        {
            Managers.Pool.Push(gameObject);
        }
    }
}
