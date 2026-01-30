using UnityEngine;

/// <summary>
/// 스프라이트가 항상 카메라를 바라보도록 하는 컴포넌트 (Billboard 효과)
/// </summary>
public class BillboardSprite : MonoBehaviour
{
    private Camera targetCamera;
    
    void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera != null)
        {
            // 카메라를 바라보도록 회전
            transform.LookAt(targetCamera.transform);
            transform.Rotate(0, 180, 0); // 뒤집기
        }
    }
}








