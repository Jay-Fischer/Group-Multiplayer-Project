using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : NetworkBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] bool isSemiAuto = false;
    [SerializeField] private float shootDelay = 0.3f;
    private float nextShootTime;
    private InputAction shootAction;
    [SerializeField] private Transform gunSpawnPoint;
    

    [Header("Recoil Stats")]
    [SerializeField] private float RecoilKickBack = 0.88f;
    [SerializeField] private float RecoilKickUp = 6.0f;
    [SerializeField] private float RecoilReturnSpeed = 8.0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    [Header("Layer Mask")]
    public LayerMask aimColllisionMask = new LayerMask();

    
    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += OnAttackButtonPressed;

        startPosition = gunSpawnPoint.transform.localPosition;
        startRotation = gunSpawnPoint.transform.localRotation;
    }

    private void OnAttackButtonPressed(InputAction.CallbackContext ctx)
    {
        if (isSemiAuto)
            ShootRpc();
    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);

            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimColllisionMask))
            {
                gunSpawnPoint.LookAt(rayCastHit.point);
            }

            if (!isSemiAuto && shootAction.IsPressed())
                ShootRpc();

            gunSpawnPoint.transform.localPosition = Vector3.Lerp(gunSpawnPoint.transform.localPosition, startPosition, RecoilReturnSpeed * Time.deltaTime);
            gunSpawnPoint.transform.localRotation = Quaternion.Slerp(gunSpawnPoint.transform.localRotation, startRotation, RecoilReturnSpeed * Time.deltaTime);
        }
    }
    [Rpc(SendTo.Server)]
    private void ShootRpc()
    {        
        if (Time.time < nextShootTime) return;
        nextShootTime = Time.time + shootDelay;

        GameObject newBullet = Instantiate(BulletPrefab, firePoint.position, firePoint.rotation);
        NetworkObject networkObj = newBullet.GetComponent<NetworkObject>();
        networkObj.Spawn();
        if(newBullet.TryGetComponent(out Rigidbody rb))
        

        gunSpawnPoint.transform.localPosition -= new Vector3(0, 0, RecoilKickBack);
        gunSpawnPoint.transform.localRotation *= Quaternion.Euler(-RecoilKickUp, 0, 0);


        rb.AddForce(shootForce * firePoint.forward * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void OnDestroy()
    {
        shootAction.started -= OnAttackButtonPressed;
    }
}