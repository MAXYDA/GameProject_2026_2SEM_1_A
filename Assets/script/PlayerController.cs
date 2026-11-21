using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    Pickup,
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [Header("바닥 설정")]
    [SerializeField] private float gravity = -20;
    private CharacterController controller;
    private float verticalVeolcity;
    private PlayerState currentState= PlayerState.Normal;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void Update()
    {
        //1 wasd 입력
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }
        //상타와 관계없이 중력은 계속 적용
        ApplyGravity();
            //Nomal 상태가 아니면 이동 입력을 받지 않는다
            if (currentState != PlayerState.Normal) return;
        HandleMovement(keyboard);    
    }
    private void HandleMovement(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;
        if (keyboard.aKey.isPressed)
            input.x -= 1f;
        if (keyboard.dKey.isPressed)
            input.x += 1f;
        if (keyboard.sKey.isPressed)
            input.y -= 1f;
        if (keyboard.wKey.isPressed)
            input.y += 1f;
        input = Vector2.ClampMagnitude(input, 1f);
        //2 카메라 앞쪽과 오른쪽 방향
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        //카메라 위아래 기울기는 이동에 사용 하지 않는다

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        //3카메라 기준 이동 방향
        Vector3 moveDirction = cameraForward * input.y + cameraRight * input.y;
        moveDirction = Vector3.ClampMagnitude(moveDirction, 1f);
        //4Shift 달리기
        bool isRunning = keyboard.leftShiftKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        //5수평 이동
        controller.Move(moveDirction * currentSpeed * Time.deltaTime);
        //6이동 방향으로 회전
        if (moveDirction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        //8 idle,walk,run 에니메이션
        float animationSpeed = 0f;
        if (moveDirction.sqrMagnitude > 0.001f)
        {
            animationSpeed = isRunning ? 1f : 0.5f;
        }
        animator.SetFloat("speed", animationSpeed, 0.1f, Time.deltaTime);
    }
    public void ChangeState(PlayerState newState)
    {
        currentState = newState;
        if (currentState != PlayerState.Normal)
        {
            animator.SetFloat("speed",0);
        }
        Debug.Log("현재 상태:" + currentState);
    }
    private void ApplyGravity()
    {
        //7기본 중력 설정
        if (controller.isGrounded && verticalVeolcity < 0f)
        {
            verticalVeolcity = -2f;
        }
        else
        {
            verticalVeolcity += gravity * Time.deltaTime;
        }
        controller.Move(Vector3.up * verticalVeolcity * Time.deltaTime); //컨트롤러에 이동 방향과 속도를 준다
    }
}
