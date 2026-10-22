using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class FPSCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    private CharacterController characterController;
    private IA_Player _input;

    [Header("Mouse Look")]
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float topLookLimit = -90f;
    [SerializeField] private float bottomLookLimit = 90f;
    private float lookAngle = 0.0f;
    private Vector2 lookInput;


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        _input = new IA_Player();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        _input.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnDisable()
    {
        _input.Player.Disable();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        handleLook();
    }


    private void handleLook()
    {
        Vector2 mouseDelta = new Vector2(Mouse.current.delta.x.ReadValue(), Mouse.current.delta.y.ReadValue());

        lookAngle += -mouseDelta.y * sensitivity;
        lookAngle = Mathf.Clamp(lookAngle, topLookLimit, bottomLookLimit);

        cameraTransform.transform.localRotation = Quaternion.Euler(lookAngle, 0, 0);
        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * sensitivity, 0);
    }

}
