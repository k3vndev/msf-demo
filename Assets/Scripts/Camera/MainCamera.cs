using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;

/// <summary>Responsible of the main camera's behavior, including the overlay cameras stack.</summary>
[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour
{
    Camera _camera;
    UniversalAdditionalCameraData _cameraData;
    readonly HashSet<OverlayCamera> _overlayCameras = new();

    public enum OverlayCameraPriority
    {
        Default,
        Transitions,
        LoadingIcon,
    }

    void Awake()
    {
        _camera = GetComponent<Camera>();
        _cameraData = _camera.GetUniversalAdditionalCameraData();
    }

    /// <summary>Adds an overlay camera to the stack.</summary>
    /// <returns>Whether the camera was added to the stack.</returns>
    public bool AddOverlayCamera(Camera camera, OverlayCameraPriority priority)
    {
        // If the camera is already in the stack, return false
        if (_overlayCameras.Any(ov => ov.Camera == camera))
            return false;

        // If the camera is not in the stack, add it
        OverlayCamera overlayCamera = new(camera, (int)priority);
        _overlayCameras.Add(overlayCamera);

        // Refresh the stack to add the new camera at the correct position
        RefreshStack();
        return true;
    }

    /// <summary>Removes an overlay camera from the stack.</summary>
    /// <returns>Whether the camera was removed from the stack.</returns>
    public bool RemoveOverlayCamera(Camera camera)
    {
        // Remove the camera from the stack and refresh the stack
        if (_overlayCameras.RemoveWhere(ov => ov.Camera == camera) > 0)
        {
            RefreshStack();
            return true;
        }
        return false;
    }

    /// <summary>Sorts the overlay cameras by priority and refreshes the stack.</summary>
    public void RefreshStack()
    {
        // Clear the stack
        _cameraData.cameraStack.Clear();

        // Add the overlay cameras to the stack in order of priority
        foreach (var overlayCamera in _overlayCameras.OrderBy(ov => ov.Priority))
            _cameraData.cameraStack.Add(overlayCamera.Camera);
    }

    class OverlayCamera
    {
        public readonly Camera Camera;
        public readonly int Priority;

        public OverlayCamera(Camera camera, int priority)
        {
            Camera = camera;
            Priority = priority;
        }
    }
}