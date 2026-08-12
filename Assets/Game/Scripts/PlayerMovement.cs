using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;


public class PlayerMovement : MonoBehaviour
{   
    [Header("Movement Settings")]
  public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public Rigidbody2D rg;
    public float targetSpeed = 0f;

    private Vector2 moveInput;
    private bool isRunning = false;
    private Vector2 lastFacingDirection = new Vector2(0f, -1f); 
    private UnityEngine.Animator animator;
    [Header("Flashlight")]
    [SerializeField] private FlashlightController flashlight;

    void Start()
    {
       animator = GetComponent<UnityEngine.Animator>();
        if (rg == null) rg = GetComponent<Rigidbody2D>();
        
        // Cấu hình Rigidbody2D để player không đi trên tường
        rg.gravityScale = 0f; 
        rg.constraints = RigidbodyConstraints2D.FreezeRotation; // Không xoay

        // Đảm bảo có reference đến flashlight (fix cho persistence)
        if (flashlight == null)
        {
            flashlight = GetComponentInChildren<FlashlightController>();
            if (flashlight == null)
            {
                flashlight = FindAnyObjectByType<FlashlightController>();
            }
            if (flashlight != null)
            {
                Debug.Log("[PlayerMovement] Flashlight reference restored after scene reload");
            }
        }
    }

     void Update()
    {
      // 1. KIỂM TRA INPUT LUÔN PHẢI ĐẶT TRONG UPDATE
        if (Keyboard.current != null)
        {
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) + (Keyboard.current.aKey.isPressed ? -1f : 0f);
            float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) + (Keyboard.current.sKey.isPressed ? -1f : 0f);
            moveInput = new Vector2(x, y);

            // Kiểm tra phím Shift an toàn tại đây
            isRunning = Keyboard.current.leftShiftKey.isPressed;
        }
        else
        {
            moveInput = Vector2.zero;
            isRunning = false;
        }

        bool isMoving = moveInput.magnitude > 0.01f;

        // 2. Tính toán tốc độ và hướng
        if (isMoving)
        {
            lastFacingDirection = moveInput.normalized; 
            targetSpeed = isRunning ? runSpeed : walkSpeed;
            if (flashlight != null)
            {
                flashlight.SetFacingDirection(lastFacingDirection);
            }
        }
        else
        {
            targetSpeed = 0f; 
        }

        // 3. Gửi tham số sang Animator
        if (animator != null)
        {
            animator.SetFloat("MoveX", lastFacingDirection.x);
            animator.SetFloat("MoveY", lastFacingDirection.y);
            animator.SetFloat("speed", targetSpeed);
        }
        // Trong HandleInput(), sau khi tính input:

       
    }

    void FixedUpdate()
    {   
        // 4. FixedUpdate CHỈ dùng để áp dụng lực vật lý
        rg.MovePosition(rg.position + moveInput * targetSpeed * Time.fixedDeltaTime);
    }
}
