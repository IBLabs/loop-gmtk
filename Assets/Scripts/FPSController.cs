using UnityEngine;

public class FPSController : MonoBehaviour
{
  void Start()
  {
    Application.targetFrameRate = -1;
    QualitySettings.vSyncCount = 0;
  }
}