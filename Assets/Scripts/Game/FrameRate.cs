using UnityEngine;
using System.Collections;

public class FrameRate : MonoBehaviour
{
    public int targetFrameRate = 30;

    void Awake()
    {
        ForceFrameRate(targetFrameRate);
    }

    void ForceFrameRate(int frameRate)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
    }
}
