using UnityEngine;

public class FPSController : MonoBehaviour
{
  void Start()
  {
    Application.targetFrameRate = 60;
    QualitySettings.vSyncCount = 0;
  }
}