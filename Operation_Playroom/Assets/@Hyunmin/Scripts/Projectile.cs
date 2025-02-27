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

    // ȭ�� �߻� �޼���
    public void Launch(Vector3 shootPoint, Vector3 direction, TrailRenderer trailPrefab = null)
    {
        transform.position = shootPoint;
        transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);

        // ���� ����
        if(trailPrefab != null)
        {
            trail = Instantiate(trailPrefab, transform);
            trail.Clear();

        }

        // �߻� ��ƾ ����
        arrowCoroutine = StartCoroutine(ArrowParabolaRoutine(direction));
    }

    // ȭ�� �߻� ��ƾ
    IEnumerator ArrowParabolaRoutine(Vector3 direction)
    {
        Vector3 velocity = direction * speed;
        float time = 0;

        while (time < flightTime && !Iscollision)
        {
            time += Time.deltaTime;

            // �̵�
            transform.position += velocity * Time.deltaTime;

            // �߷� ����
            velocity.y -= gravity * Time.deltaTime;

            yield return null;
        }
        Destroy(trail);
        Managers.Pool.Push(gameObject);
    }

    // ��, �������� �浹 �� ȣ��
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Projectile>()) return; // ���� ȭ�� �ǰ� ��
        if (other.GetComponent<PlayerController>()) return; // �ٸ� �÷��̾� �ǰ� ��

        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
            Managers.Pool.Push(gameObject);
        }
    }
}
