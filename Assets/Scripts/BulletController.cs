using UnityEngine;

public class BulletController : MonoBehaviour
{
  [Header("Bullet Configuration")]
  public float damage = 10f;
  public float deathRotationDelta = 360f; // Degrees after which bullet is destroyed

  [Header("Collision Settings")]
  public LayerMask enemyLayer = -1;
  public LayerMask playerLayer = -1;

  private BarrelMovement barrelMovement;
  private float initialRotationDelta;

  void Start()
  {
    barrelMovement = GetComponent<BarrelMovement>();
    if (barrelMovement != null)
    {
      initialRotationDelta = barrelMovement.TotalRotationDelta;
    }
  }

  void Update()
  {
    CheckRotationDeath();
  }

  void CheckRotationDeath()
  {
    if (barrelMovement == null) return;

    float rotationTraveled = Mathf.Abs(barrelMovement.TotalRotationDelta - initialRotationDelta);
    if (rotationTraveled >= deathRotationDelta)
    {
      DestroyBullet();
    }
  }

  void OnTriggerEnter(Collider other)
  {
    HandleCollision(other);
  }

  void OnCollisionEnter(Collision collision)
  {
    HandleCollision(collision.collider);
  }

  void HandleCollision(Collider collider)
  {
    // Check if hit enemy
    if (IsInLayerMask(collider.gameObject, enemyLayer))
    {
      DealDamage(collider.gameObject, "Enemy");
      DestroyBullet();
    }
    // Check if hit player
    else if (IsInLayerMask(collider.gameObject, playerLayer))
    {
      DealDamage(collider.gameObject, "Player");
      DestroyBullet();
    }
  }

  void DealDamage(GameObject target, string targetType)
  {
    // Try to find health component
    var healthComponent = target.GetComponent<IHealth>();
    if (healthComponent != null)
    {
      healthComponent.TakeDamage(damage);
    }

    Debug.Log($"Bullet hit {targetType} for {damage} damage");
  }

  bool IsInLayerMask(GameObject obj, LayerMask layerMask)
  {
    return (layerMask.value & (1 << obj.layer)) > 0;
  }

  void DestroyBullet()
  {
    Destroy(gameObject);
  }
}

// Interface for health components
public interface IHealth
{
  void TakeDamage(float damage);
}