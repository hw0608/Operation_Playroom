using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -5); // 카메라 위치 오프셋
    public float smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desirePosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desirePosition, smoothSpeed * Time.deltaTime);

            transform.LookAt(target);
        }
    }
}
