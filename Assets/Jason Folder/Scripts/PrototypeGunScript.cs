using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce;
    [SerializeField] private GameObject BulletPrefab;

    private InputAction shootAction;

    private void Start()
    {
        shootAction = InputSystem.actions.FindAction("Attack");
        shootAction.started += OnAttackButtonPressed;
    }

    private void OnAttackButtonPressed(InputAction.CallbackContext ctx)
    {
        Shoot();
    }

    private void Shoot()
    {
        GameObject newBullet = Instantiate(BulletPrefab, firePoint.position, firePoint.rotation);
        if(newBullet.TryGetComponent(out Rigidbody rb))
        rb.AddForce(shootForce * firePoint.forward * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    private void OnDestroy()
    {
        shootAction.started -= OnAttackButtonPressed;
    }
}