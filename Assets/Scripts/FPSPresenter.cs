using UnityEngine;
using TMPro;

public class FPSPresenter : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI fpsText;
  [SerializeField] private float updateInterval = 0.5f;

  private float deltaTime = 0.0f;
  private float timer = 0.0f;

  void Update()
  {
    deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    timer += Time.unscaledDeltaTime;

    if (timer >= updateInterval)
    {
      float fps = 1.0f / deltaTime;
      fpsText.text = $"FPS: {fps:0}";
      timer = 0.0f;
    }
  }
}
