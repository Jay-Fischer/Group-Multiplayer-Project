using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] bool isSemiAuto = false;

    [Header("Recoil Stats")]
    [SerializeField] private float RecoilKickBack = 0.88f;
    [SerializeField] private float RecoilKickUp = 6.0f;
    [SerializeField] private float RecoilReturnSpeed = 8.0f;

    private Vector3 startPosition;
    private Quaternion startRotation;



    [SerializeField] private float shootDelay = 0.3f;
    private float nextShootTime;

    private InputAction shootAction;

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += OnAttackButtonPressed;

        startPosition = transform.localPosition;
        startRotation = transform.localRotation;
    }

    private void OnAttackButtonPressed(InputAction.CallbackContext ctx)
    {
        if (isSemiAuto)
            Shoot();
    }

    private void Update()
    {
        if (!isSemiAuto && shootAction.IsPressed())
            Shoot();

        transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, RecoilReturnSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, startRotation, RecoilReturnSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        if (Time.time < nextShootTime) return;
        nextShootTime = Time.time + shootDelay;

        GameObject newBullet = Instantiate(BulletPrefab, firePoint.position, firePoint.rotation);
        if(newBullet.TryGetComponent(out Rigidbody rb))
        rb.AddForce(shootForce * firePoint.forward * Time.fixedDeltaTime, ForceMode.Impulse);

        transform.localPosition -= new Vector3(0, 0, RecoilKickBack);
        transform.localRotation *= Quaternion.Euler(-RecoilKickUp, 0, 0);
    }

    private void OnDestroy()
    {
        shootAction.started -= OnAttackButtonPressed;
    }
}