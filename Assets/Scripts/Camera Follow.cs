using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;         // 따라갈 캐릭터
    public float smoothSpeed = 0.2f;  // 부드러운 이동 속도
    public Vector3 offset;           // 캐릭터와 카메라 간 거리

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 = 캐릭터 위치 + offset
        Vector3 desiredPosition = target.position + offset;

        // 부드럽게 이동
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
