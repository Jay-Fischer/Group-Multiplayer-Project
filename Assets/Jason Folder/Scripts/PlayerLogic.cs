using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLogic : MonoBehaviour
{
    IA_Player _inputs;
    [SerializeField] CharacterController cc;
    [SerializeField] private Transform camPosition;

    [Header("Movement Variables")]
    [SerializeField] Vector2 moveInput;
    private Vector3 moveDirection = Vector3.zero;
    private float currentMoveSpeed = 0;
    public float moveSpeed;
    public float runSpeed;

    [Header("Jumping Variables")]
    public float jumpForce = 1.5f;
    public float quickJumpMultiplier = 0.5f;
    public int numberOfJumps;
    [SerializeField] private int _jumps;
    [SerializeField] private float jumpReset;


    [Header("coyote and buffer variables")]
    public float coyoteTimer = 0.5f;
    public float jumpBufferTimer = 0.5f;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBuffer;


    [Header("Player Physics Variables")]
    public float gravityForce = -8;
    [SerializeField] private Vector3 playerVelocity;
    public Transform groundCheck;
    public LayerMask groundLayer;
    [SerializeField] Collider[] groundCollider;

    [Header("Boolean Variables")]
    public bool isGrounded;
    public bool hurt;

    private void Awake()
    {
        _inputs = new IA_Player();
        cc = GetComponent<CharacterController>();
        camPosition = Camera.main.transform;

        currentMoveSpeed = moveSpeed;
    }
    private void OnEnable()
    {
        _inputs.Enable();
    }

    private void OnDisable()
    {
        _inputs.Disable();
    }

    private void Update()
    {
        HandlePhysics();
        HandleInput();
        HandleMovement();

        if (isGrounded) coyoteTime = coyoteTimer;
        else if (coyoteTime > 0) coyoteTime -= Time.deltaTime;
        if (coyoteTime == coyoteTimer) _jumps = numberOfJumps;

        jumpBuffer -= Time.deltaTime;
        if (jumpBuffer < 0) jumpBuffer = 0;
        if (jumpReset > 0) jumpReset -= Time.deltaTime;
    }

    private void HandleInput()
    {
        moveInput = _inputs.Player.Move.ReadValue<Vector2>().normalized;
        if (_inputs.Player.Jump.triggered)
        {
            jumpBuffer = jumpBufferTimer;
            jumpReset = 0.5f;
        }
        //Player Jump
        if (jumpBuffer > 0)
        {
            if (coyoteTime > 0)
            {                
                jumpBuffer = 0;
                coyoteTime = 0;
                playerVelocity.y = Mathf.Sqrt(jumpForce * -3f * gravityForce);
            }
            else if (_jumps > 0)
            {
                playerVelocity.y = 0;                
                _jumps--;
                jumpBuffer = 0;
                coyoteTime = 0;
                playerVelocity.y = Mathf.Sqrt(jumpForce * -3f * gravityForce);
            }

        }
        //Quick Jump
        if (_inputs.Player.Jump.WasReleasedThisFrame() && playerVelocity.y > 0)
        {
            playerVelocity.y *= quickJumpMultiplier;
        }
    }

    private void HandlePhysics()
    {
        groundCollider = Physics.OverlapSphere(groundCheck.position, 0.2f, groundLayer);
        if (groundCollider.Length > 0 && jumpReset <= 0)
            isGrounded = true;
        else
            isGrounded = false;

        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -0.5f;
        else
            playerVelocity.y += gravityForce * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float oldY = moveDirection.y;

        Vector2 newSpeed = new Vector2(moveInput.y * currentMoveSpeed, moveInput.x * currentMoveSpeed);

        moveDirection = (forward * newSpeed.x) + (right * newSpeed.y);
        moveDirection.y = oldY;

        cc.Move(moveDirection * Time.deltaTime);
        cc.Move(playerVelocity * Time.deltaTime);
    }
}
