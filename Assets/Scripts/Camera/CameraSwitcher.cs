using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera panoramaCam;

    private bool isPanoramaActive = false;

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();

        // On abonne la fonction Switch à l'action "SwitchCamera"
        // controls.Player.SetCallbacks(this);
        controls.Player.SwitchCamera.performed += ContextMenu => OnSwitchCamera();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void OnSwitchCamera()
    {
        isPanoramaActive = !isPanoramaActive;
        SwitchCamera();
    }
    
    private void SwitchCamera()
    {
        if (isPanoramaActive)
        {
            playerCam.Priority = 0;
            panoramaCam.Priority = 10;
        }
        else
        {
            playerCam.Priority = 10;
            panoramaCam.Priority = 0;
        }
    }
}
