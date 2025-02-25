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

    // ȭ�� �߻� �޼���
    public void Launch(Vector3 shootPoint, Vector3 direction, GameObject trailPrefab)
    {
        transform.position = shootPoint;
        transform.localRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);

        // ���� ����
        if (trail != null)
        {
            Destroy(trail);
        }
        trail = Instantiate(trailPrefab, transform);

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
        if (Iscollision) return; // �̹� �浹���� �϶�
        if (other.GetComponent<NetworkObject>()) return; // �÷��̾� �ǰ� ��

        Iscollision = true;

        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
        }

        StartCoroutine(RemoveArrowRoutine());
    }

    // ȭ�� ���� ��ƾ
    IEnumerator RemoveArrowRoutine()
    {
        yield return new WaitForSeconds(3);

        Iscollision = false;

        Managers.Pool.Push(gameObject);
    }
}
