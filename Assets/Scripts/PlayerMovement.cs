using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lookDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D missing!");
    }

    void Update()
    {
        // --- 1. 이동 입력 받기 ---
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // --- 2. 바라보는 방향 ---
        if (moveInput != Vector2.zero)
        {
            lookDirection = moveInput.normalized;
        }

        // --- 3. X 키 눌렀을 때 레이캐스트 ---
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("X키 눌림");

            RaycastHit2D hit = Physics2D.Raycast(rb.position, lookDirection, 1.5f, LayerMask.GetMask("NPC"));

            Debug.DrawRay(rb.position, lookDirection * 1.5f, Color.red, 5f);

            if (hit.collider != null)
            {
                Debug.Log("Raycast가 NPC를 맞췄습니다: " + hit.collider.gameObject.name);

                // --- NPC 대화창 열기 ---
                NonPlayerCharacter npc = hit.collider.GetComponent<NonPlayerCharacter>();
                if (npc != null)
                {
                    npc.DisplayDialog();
                }
                else
                {
                    Debug.LogWarning("NPC 스크립트 없음: " + hit.collider.gameObject.name);
                }
            }
            else
            {
                Debug.Log("Raycast가 아무것도 맞추지 못했습니다.");
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
