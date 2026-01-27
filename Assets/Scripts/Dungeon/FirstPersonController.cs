using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;
    
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float verticalLookLimit = 80f;
    
    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool isRunning = false;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // 카메라 찾기 또는 생성
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            GameObject cameraObj = new GameObject("PlayerCamera");
            cameraObj.transform.SetParent(transform);
            cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0); // 눈 높이
            cameraObj.transform.localRotation = Quaternion.identity;
            playerCamera = cameraObj.AddComponent<Camera>();
            playerCamera.fieldOfView = 75f;
            playerCamera.nearClipPlane = 0.1f;
            playerCamera.farClipPlane = 50f;
            playerCamera.clearFlags = CameraClearFlags.SolidColor;
            playerCamera.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            playerCamera.tag = "MainCamera";
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
            
            // 2D 스타일을 위한 설정
            // Orthographic은 1인칭에 부적합하므로 Perspective 유지
            // 대신 텍스처 필터와 해상도 조정
            
            Debug.Log("[FirstPersonController] Player camera created");
        }
        else
        {
            // 기존 카메라 활성화 확인
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
            playerCamera.tag = "MainCamera";
            Debug.Log("[FirstPersonController] Using existing camera");
        }
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // 마우스 회전
        HandleMouseLook();
        
        // 이동 입력
        HandleMovement();
        
        // ESC로 커서 잠금 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 수평 회전 (Y축)
        transform.Rotate(0, mouseX, 0);
        
        // 수직 회전 (X축) - 제한
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -verticalLookLimit, verticalLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
    
    void HandleMovement()
    {
        // 달리기 체크
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        
        // 이동 입력
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // 지면에 닿아있는지 확인
        if (characterController.isGrounded)
        {
            // 이동 방향 계산 (로컬 공간)
            moveDirection = transform.right * horizontal + transform.forward * vertical;
            moveDirection *= currentSpeed;
            
            // 점프
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }
        
        // 중력 적용
        moveDirection.y -= gravity * Time.deltaTime;
        
        // 이동 실행
        characterController.Move(moveDirection * Time.deltaTime);
    }
}



