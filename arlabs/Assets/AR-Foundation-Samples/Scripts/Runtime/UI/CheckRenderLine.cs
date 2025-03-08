using UnityEngine;
using UnityEngine.XR;

public class CheckRenderLine : MonoBehaviour
{
    [SerializeField]
    Camera m_CameraAR;

    public Camera cameraAR
    {
        get => m_CameraAR;
        set => m_CameraAR = value;
    }

    void Start()
    {
        if(!m_CameraAR.stereoEnabled)
        {
            enabled = false;
        }

    }

    void Update()
    {
        HandleLineRender();
    }

    void HandleLineRender()
    {
        
    }
}
