using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;

/*
*   Controls the camera movement
*/

public class CameraController : Singleton<CameraController>
{
    [System.Serializable]
    public class ColourMixerProfile
    {
        public Color redMixer = Color.red, greenMixer = Color.green, blueMixer = Color.blue;
    }

    [System.Serializable]
    public class DepthOfFieldSettings
    {
        public float focusDistance;
        public float focalLength;
        public float aperture;
    }

    [Header("Transforms")]
    [SerializeField] private new Camera camera;
    [SerializeField] private Transform cameraContainer;

    [Header("Renderer")]
    [SerializeField] private Volume postProcessingVolume;
    [SerializeField] private ScriptableRendererFeature ssaoRenderFeature;

    [Header("Colourblindness")]
    [SerializeField] private ColourMixerProfile[] colourblindProfiles;

    [Header("Parameters")]
    [SerializeField] private Vector2 cameraBoundsMin;
    [SerializeField] private Vector2 cameraBoundsMax;
    [SerializeField] private float panSpeed;
    [SerializeField] private float rotationSpeedMouse;
    [SerializeField] private float rotationSpeedKeys;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minCameraDist, maxCameraDist;
    [SerializeField] private bool panSmoothing;
    [SerializeField] private float panSmoothingValue;
    [SerializeField] private bool lockCameraMovement;
    [SerializeField] private DepthOfFieldSettings dofSettingsOrtho;
    [SerializeField] private DepthOfFieldSettings dofSettingsPersp;
    [SerializeField] private float edgeScrollingBorderSize;

    // TargetPosition is separate from Target.position to allow for interpolated camera movement
    private Vector3 targetPosition;

    private float cameraDist;
    private Vector3 worldSpaceGrab, worldSpaceGrabLast;

    private List<Camera> allCameras;

    // A plane that a ray cast from a mouse position can collide with
    private Plane raycastPlane;
 
    [HideInInspector] public Transform objectToFollow;

    public Camera Camera { get { return camera; } }

    void InputUpdate()
    {
        if (InputHandler.Instance.IsPressed("Camera Movement - Keys")) MoveCamera();
        if (InputHandler.Instance.IsPressed("Camera Rotation - Mouse") && !InputHandler.Instance.IsPressed("Camera Movement - Mouse")) RotateCameraMouse();
        if (InputHandler.Instance.IsPressed("Camera Rotation - Keys")) RotateCamera();

        if (InputHandler.Instance.WasPressedThisFrame("Camera Movement - Mouse")) Grab();
        if (InputHandler.Instance.IsPressed("Camera Movement - Mouse")) Pan();
    }

    protected override void Awake() 
    {
        base.Awake();

        // localCameraOffset = camera.transform.localPosition;
        cameraDist = camera.transform.localPosition.z;
        targetPosition = transform.position;

        raycastPlane = new Plane(Vector3.up, Vector3.zero);
        allCameras = new() { camera };
    }

    void LateUpdate() 
    {
        if(PlayerController.Instance && !PlayerController.Instance.IsPaused && !lockCameraMovement)
        {
            InputUpdate();

            // Only zoom if the mouse isn't over a UI element to avoid zooming when scrolling
            if (EventSystem.current && !EventSystem.current.IsPointerOverGameObject())
            {
                float scrollDelta = -InputHandler.Instance.FindAction("Mouse Scroll Delta").ReadValue<Vector2>().y;
                zoom(scrollDelta * zoomSpeed * Time.unscaledDeltaTime);
            }

            // Lerp towards camera zoom dist
            if(camera.orthographic)
            {
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, -cameraDist / 2f, Time.unscaledDeltaTime * 10);
                camera.nearClipPlane = cameraDist;
            }

            camera.transform.localPosition = new Vector3(0, 0, Mathf.Lerp(camera.transform.localPosition.z, cameraDist, Time.unscaledDeltaTime * 10));


            objectToFollow = null;
            if (InputHandler.Instance.IsPressed("Actor Camera Follow") && PlayerController.Instance.SelectedCharacter)
            {
                objectToFollow = PlayerController.Instance.SelectedCharacter.transform;
            }

            if (objectToFollow != null) targetPosition = objectToFollow.transform.position;
            transform.position = targetPosition;

            // cameraContainer.position = panSmoothing ? Vector3.Lerp(cameraContainer.position, targetPosition, Time.deltaTime * panSmoothingValue) : targetPosition;
            // camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, localCameraOffset, Time.deltaTime * 10);

            if(OptionsManager.Instance.edgeScrolling.BoolValue && Screen.fullScreen)
            {
                float speed = panSpeed * (OptionsManager.Instance.cameraPanSpeed.Value / 10.0f);

                if (Input.mousePosition.x < edgeScrollingBorderSize)
                {
                    move(-transform.right * speed * Time.unscaledDeltaTime);
                }
                if (Input.mousePosition.x > Screen.width - edgeScrollingBorderSize)
                {
                    move(transform.right * speed * Time.unscaledDeltaTime);
                }
                if (Input.mousePosition.y < edgeScrollingBorderSize)
                {
                    move(-transform.forward * speed * Time.unscaledDeltaTime);
                }
                if (Input.mousePosition.y > Screen.height - edgeScrollingBorderSize)
                {
                    move(transform.forward * speed * Time.unscaledDeltaTime);
                }
            }
        }
    }

    void MoveCamera()
    {
        if (InputHandler.Instance.IsPressed("Camera Rotation - Mouse")) return;
        Vector2 MovementAxis = InputHandler.Instance.FindAction("Camera Movement - Keys").ReadValue<Vector2>();
        float speed = panSpeed * (OptionsManager.Instance.cameraPanSpeed.Value / 10.0f);
        move(transform.forward * MovementAxis.y * speed * Time.unscaledDeltaTime);
        move(transform.right * MovementAxis.x * speed * Time.unscaledDeltaTime);
    }

    void RotateCameraMouse()
    {
        float speed = rotationSpeedMouse * (OptionsManager.Instance.cameraRotSpeed.Value / 10.0f);
        float rot = InputHandler.Instance.FindAction("Mouse Delta").ReadValue<Vector2>().x * speed;
        transform.Rotate(Vector3.up, rot);
    }

    public void RotateCamera()
    {
        float speed = rotationSpeedKeys * (OptionsManager.Instance.cameraRotSpeed.Value / 10.0f);
        transform.Rotate(Vector3.up, speed * InputHandler.Instance.FindAction("Camera Rotation - Keys").ReadValue<float>() * Time.unscaledDeltaTime);
    }

    public void SetPositionImmediate(Vector3 pos) 
    {
        targetPosition = pos;
        transform.position = pos;
        worldSpaceGrab = Vector3.zero;
        worldSpaceGrabLast = Vector3.zero;
    }

    public void ResetCameraDist() 
    {
        camera.transform.localPosition = Vector3.forward * cameraDist;
    }

    // Gets the ground position where the mouse right clicks
    public void Grab() 
    {
        if (GetMousePointOnGround(out Vector3 point)) 
        {
            worldSpaceGrab = point;
            worldSpaceGrabLast = worldSpaceGrab;
        }
    }

    // Called when the mouse moves while grabbing - find the new position and translate the camera in the opposite direction
    public void Pan() 
    {
        if(GetMousePointOnGround(out Vector3 point)) 
        {
            worldSpaceGrab = point;
            Vector3 delta = worldSpaceGrab - worldSpaceGrabLast;
            move(new Vector3(-delta.x,0, -delta.z)) ;
        }
    }

    public float GetCameraDistAbsolute()
    {
        return Mathf.Abs(camera.transform.localPosition.z);
    }

    private void move(Vector3 delta) 
    {
        targetPosition += delta;
        objectToFollow = null;

        // Limit camera position to bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, cameraBoundsMin.x, cameraBoundsMax.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, cameraBoundsMin.y, cameraBoundsMax.y);
    }

    private void zoom(float dist) 
    {
        // zoom faster the further out the camera is
        dist *= (cameraDist / 4);

        if(cameraDist + dist > minCameraDist && cameraDist + dist < maxCameraDist) 
        {
            cameraDist += dist;
        }
    }

    // Gets the point at which the mouse position ray hits a plane at y = 0
    // Returns raycast success
    public bool GetMousePointOnGround(out Vector3 point) 
    {
        Ray ray = camera.ScreenPointToRay(InputHandler.Instance.FindAction("Mouse Position").ReadValue<Vector2>());
        bool hit = raycastPlane.Raycast(ray, out float distance);
        point = ray.GetPoint(distance);
        return hit;
    }

    public void RegisterAdditionalCamera(Camera camera)
    {
        if(!allCameras.Contains(camera))
        {
            allCameras.Add(camera);
        }
    }

    public void SwitchToCamera(Camera camera)
    {
        // Remove destroyed cameras
        allCameras.RemoveAll(o => o == null);

        foreach(Camera cam in allCameras)
        {
            cam.gameObject.SetActive(cam == camera);
        }
    }

    public void SwitchToDefaultCamera()
    {
        SwitchToCamera(camera);
    }

    public void SetOrthographicEnabled(bool enabled)
    {
        camera.orthographic = enabled;
        if(enabled)
        {
            camera.orthographicSize = -cameraDist / 2f;
            //camera.transform.localPosition = new Vector3(0, 0, -20);
        }
        else
        {
            //camera.transform.localPosition = new Vector3(0, 0, cameraDist);
        }

        if (postProcessingVolume.profile.TryGet(out DepthOfField settings))
        {
            DepthOfFieldSettings dofSettings = enabled ? dofSettingsOrtho : dofSettingsPersp;
            settings.focusDistance.value = dofSettings.focusDistance;
            settings.focalLength.value = dofSettings.focalLength;
            settings.aperture.value = dofSettings.aperture;
        }
    }

    public void SetAntialiasingEnabled(bool enabled)
    {
        camera.GetUniversalAdditionalCameraData().antialiasing = enabled ? AntialiasingMode.FastApproximateAntialiasing : AntialiasingMode.None;
    }

    public void SetAmbientOcclusionEnabled(bool enabled)
    {
        ssaoRenderFeature.SetActive(enabled);
    }

    public void SetColourblindMode(int mode)
    {
        if(mode >= colourblindProfiles.Length)
        {
            Debug.LogWarning("Trying to apply a colourblind mode that doesn't have a profile!");
        }
        postProcessingVolume.profile.TryGet(out ChannelMixer settings);

        if(settings != null)
        {
            ColourMixerProfile colourblindProfile = colourblindProfiles[mode];

            settings.redOutRedIn.value = colourblindProfile.redMixer.r * 100;
            settings.redOutGreenIn.value = colourblindProfile.redMixer.g * 100;
            settings.redOutBlueIn.value = colourblindProfile.redMixer.b * 100;

            settings.greenOutRedIn.value = colourblindProfile.greenMixer.r * 100;
            settings.greenOutGreenIn.value = colourblindProfile.greenMixer.g * 100;
            settings.greenOutBlueIn.value = colourblindProfile.greenMixer.b * 100;

            settings.blueOutRedIn.value = colourblindProfile.blueMixer.r * 100;
            settings.blueOutGreenIn.value = colourblindProfile.blueMixer.g * 100;
            settings.blueOutBlueIn.value = colourblindProfile.blueMixer.b * 100;
        }
    }

    public void SetPostProcessingComponentActive<T>(bool active) where T : VolumeComponent
    {
        if(postProcessingVolume.profile.TryGet<T>(out T settings))
        {
            settings.active = active;
        }
    }

    private void OnDestroy()
    {
        // When exiting the game reenable this so it's active in the editor again
        ssaoRenderFeature.SetActive(true);
    }
}
