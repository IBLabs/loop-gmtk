using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
  [SerializeField] private GameObject bulletPrefab;
  [SerializeField] private float fireRate = 0.5f; // Time between shots in seconds
  [SerializeField] private InputActionProperty fireAction;

  private float nextFireTime = 0f;

  void OnEnable()
  {
    fireAction.action?.Enable();
  }

  void OnDisable()
  {
    fireAction.action?.Disable();
  }

  void Update()
  {
    if (fireAction.action.IsPressed() && Time.time >= nextFireTime)
    {
      Shoot();
      nextFireTime = Time.time + fireRate;
    }
  }

  void Shoot()
  {
    if (bulletPrefab != null)
    {
      Vector3 spawnPosition = new Vector3(transform.position.x, 0f, 0f);
      Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
    }
  }
}