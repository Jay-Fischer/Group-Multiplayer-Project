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


    private readonly NetworkVariable<Quaternion> netGunRotation = new NetworkVariable<Quaternion>(
        writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone);

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += OnAttackButtonPressed;

        startPosition = gunSpawnPoint.transform.localPosition;
        startRotation = gunSpawnPoint.transform.localRotation;
    }

    private void OnAttackButtonPressed(InputAction.CallbackContext ctx)
    {
        if (IsLocalPlayer && isSemiAuto)
            ShootRpc();
    }

    private void Update()
    {
        if (IsLocalPlayer)
        {
            Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

            if (!isSemiAuto && shootAction.IsPressed())
                ShootRpc();

            if (Physics.Raycast(ray, out RaycastHit rayCastHit, 999f, aimColllisionMask))
            {
                gunSpawnPoint.LookAt(rayCastHit.point);
                netGunRotation.Value = gunSpawnPoint.localRotation;
            }
        }
        else
        {
            gunSpawnPoint.localRotation = netGunRotation.Value;
        }
        //RecoilRpc();
        HandleRecoilReturn();
    }



    [Rpc(SendTo.Server)]
    private void ShootRpc()
    {        
        if (Time.time < nextShootTime) return;
            nextShootTime = Time.time + shootDelay;

        gunSpawnPoint.localRotation = netGunRotation.Value;

        GameObject newBullet = Instantiate(BulletPrefab, firePoint.position, firePoint.rotation);
        NetworkObject networkObj = newBullet.GetComponent<NetworkObject>();
        networkObj.Spawn();


        if(newBullet.TryGetComponent(out Rigidbody rb))
            rb.AddForce(shootForce * firePoint.forward, ForceMode.Impulse);


        PlayRecoilVisualsClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void PlayRecoilVisualsClientRpc()
    {
        gunSpawnPoint.transform.localPosition -= new Vector3(0, 0, RecoilKickBack);
        gunSpawnPoint.transform.localRotation *= Quaternion.Euler(-RecoilKickUp, 0, 0);
    }

    private void HandleRecoilReturn()
    {
        gunSpawnPoint.transform.localPosition = Vector3.Lerp(gunSpawnPoint.transform.localPosition, startPosition, RecoilReturnSpeed * Time.deltaTime);
        gunSpawnPoint.transform.localRotation = Quaternion.Slerp(gunSpawnPoint.transform.localRotation, startRotation, RecoilReturnSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        shootAction.started -= OnAttackButtonPressed;
    }
}