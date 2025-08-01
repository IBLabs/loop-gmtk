
using UnityEngine;

public class BarrelMovement : MonoBehaviour
{
  [Header("Rotation Configuration")]
  public float heightOffset = 2f;
  public float radius = 3f;
  public float rotationSpeed = 45f;

  private Vector3 originalPosition;
  private float currentAngle = 0f;

  public float TotalRotationDelta => currentAngle;

  void Start()
  {
    originalPosition = transform.position;

    UpdatePosition();
  }

  void Update()
  {
    currentAngle += rotationSpeed * Time.deltaTime;

    UpdatePosition();
  }

  private void UpdatePosition()
  {
    float angleInRadians = currentAngle * Mathf.Deg2Rad;

    Vector3 offset = new Vector3(
      0f,
      heightOffset + Mathf.Cos(angleInRadians) * radius,
      Mathf.Sin(angleInRadians) * radius
    );

    transform.position = originalPosition + offset;

    Vector3 upDirection = new Vector3(0f, Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    Vector3 forwardDirection = Vector3.right;

    transform.rotation = Quaternion.LookRotation(forwardDirection, upDirection);
  }

  void OnDrawGizmosSelected()
  {
    Vector3 center = Application.isPlaying ? originalPosition : transform.position;

    Gizmos.color = Color.yellow;

    for (int i = 0; i < 36; i++)
    {
      float angle1 = i * 10f * Mathf.Deg2Rad;
      float angle2 = (i + 1) * 10f * Mathf.Deg2Rad;

      Vector3 pos1 = center + new Vector3(0, heightOffset + Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius);
      Vector3 pos2 = center + new Vector3(0, heightOffset + Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius);

      Gizmos.DrawLine(pos1, pos2);
    }

    Gizmos.color = Color.red;
    Gizmos.DrawSphere(center, 0.1f);

    Gizmos.color = Color.green;
    Gizmos.DrawLine(center, center + Vector3.up * heightOffset);

    Gizmos.color = Color.blue;
    Gizmos.DrawRay(center, Vector3.right * radius);
    Gizmos.DrawRay(center, Vector3.left * radius);
  }
}