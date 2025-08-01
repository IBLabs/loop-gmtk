using UnityEngine;

public class SquashAndStretchController : MonoBehaviour
{
  [Header("Landing Squash Settings")]
  [SerializeField] private Vector3 landingSquash = new Vector3(1.4f, 0.6f, 1f);
  [SerializeField] private float squashDuration = 0.1f;
  [SerializeField] private float returnDuration = 0.3f;
  [SerializeField] private AnimationCurve squashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
  [SerializeField]
  private AnimationCurve returnCurve = new AnimationCurve(
    new Keyframe(0f, 0f, 0f, 0f),
    new Keyframe(0.3f, 1.2f, 0f, 0f),
    new Keyframe(0.6f, 0.9f, 0f, 0f),
    new Keyframe(1f, 1f, 0f, 0f)
  );

  private Vector3 originalScale;
  private Coroutine currentAnimation;

  private void Start()
  {
    originalScale = transform.localScale;
  }

  private void OnEnable()
  {
    PlayerController.OnJumpLand += HandleJumpLand;
  }

  private void OnDisable()
  {
    PlayerController.OnJumpLand -= HandleJumpLand;
    if (currentAnimation != null)
    {
      StopCoroutine(currentAnimation);
    }
  }

  private void HandleJumpLand()
  {
    PlayLandingSquash();
  }

  private void PlayLandingSquash()
  {
    if (currentAnimation != null)
    {
      StopCoroutine(currentAnimation);
    }

    currentAnimation = StartCoroutine(LandingSquashCoroutine());
  }

  private System.Collections.IEnumerator LandingSquashCoroutine()
  {
    Vector3 targetSquash = new Vector3(
      originalScale.x * landingSquash.x,
      originalScale.y * landingSquash.y,
      originalScale.z * landingSquash.z
    );

    // Squash phase
    float t = 0f;
    while (t <= 1f)
    {
      t += Time.deltaTime / squashDuration;
      float curveValue = squashCurve.Evaluate(t);
      transform.localScale = Vector3.Lerp(originalScale, targetSquash, curveValue);
      yield return null;
    }

    // Return phase with spring-like bounce
    t = 0f;
    while (t <= 1f)
    {
      t += Time.deltaTime / returnDuration;
      float curveValue = returnCurve.Evaluate(t);
      transform.localScale = Vector3.Lerp(targetSquash, originalScale, curveValue);
      yield return null;
    }

    // Ensure we end at exactly the original scale
    transform.localScale = originalScale;
    currentAnimation = null;
  }
}