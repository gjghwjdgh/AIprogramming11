using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // 따라갈 대상 (Agent)
    public Vector3 offset = new Vector3(0f, 5f, -6f); // 카메라 위치 오프셋
    public float followSpeed = 5f;  // 따라가는 속도

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        transform.LookAt(target); // 대상 바라보도록 카메라 회전
    }
}
