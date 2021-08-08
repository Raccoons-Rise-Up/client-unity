using UnityEngine;

public class Game : MonoBehaviour
{
    private void Start()
    {
        QualitySettings.vSyncCount = 1; // Sync framerate to monitor refresh rate
        Application.targetFrameRate = 60; // FPS if VSync is turned off
    }
}
