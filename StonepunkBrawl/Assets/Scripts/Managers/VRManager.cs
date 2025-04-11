using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using System.Collections.Generic;

namespace StonepunkBrawl.Managers
{
    public class VRManager : MonoBehaviour
    {
        public static VRManager Instance { get; private set; }
        
        [Header("Performance Settings")]
        public bool dynamicResolution = true;
        public float targetFrameRate = 72f; // Default for Quest
        public float renderScale = 1.0f;
        
        [Header("Comfort Settings")]
        public bool useComfortVignette = true;
        public bool snapTurning = true;
        public float snapTurnAngle = 30f;
        public bool enableHeadBob = false;
        
        [Header("Platform Specific")]
        public bool isOculusDevice;
        public bool isSteamVR;
        public bool isMobileVR;
        
        private XRDisplaySubsystem displaySubsystem;
        private List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeVR();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeVR()
        {
            // Get the display subsystem
            SubsystemManager.GetInstances(inputSubsystems);
            displaySubsystem = XRGeneralSettings.Instance?.Manager?.activeLoader?.GetLoadedSubsystem<XRDisplaySubsystem>();
            
            // Detect platform
            DetectVRPlatform();
            
            // Apply initial settings
            ApplyPlatformSpecificSettings();
            
            // Subscribe to events
            foreach (var inputSubsystem in inputSubsystems)
            {
                inputSubsystem.trackingOriginUpdated += OnTrackingOriginUpdated;
            }
        }
        
        private void DetectVRPlatform()
        {
            var deviceName = XRSettings.loadedDeviceName.ToLower();
            isOculusDevice = deviceName.Contains("oculus");
            isSteamVR = deviceName.Contains("openvr") || deviceName.Contains("steam");
            isMobileVR = Application.platform == RuntimePlatform.Android || 
                        Application.platform == RuntimePlatform.IPhonePlayer;
        }
        
        private void ApplyPlatformSpecificSettings()
        {
            if (isMobileVR)
            {
                // Mobile VR optimizations
                targetFrameRate = 72f;
                Application.targetFrameRate = (int)targetFrameRate;
                QualitySettings.vSyncCount = 0;
                
                if (dynamicResolution)
                {
                    EnableDynamicResolution();
                }
            }
            else
            {
                // PC VR settings
                targetFrameRate = 90f;
                Application.targetFrameRate = (int)targetFrameRate;
                QualitySettings.vSyncCount = 1;
            }
            
            // Apply render scale
            if (displaySubsystem != null)
            {
                displaySubsystem.scaleRenderViewport = renderScale;
            }
        }
        
        private void EnableDynamicResolution()
        {
            if (displaySubsystem != null)
            {
                displaySubsystem.automaticPerformanceLevel = true;
            }
        }
        
        private void OnTrackingOriginUpdated(XRInputSubsystem subsystem)
        {
            // Handle tracking origin changes
            Debug.Log($"Tracking origin updated for subsystem: {subsystem.SubsystemDescriptor.id}");
        }
        
        public void SetRenderScale(float scale)
        {
            renderScale = Mathf.Clamp(scale, 0.5f, 2f);
            if (displaySubsystem != null)
            {
                displaySubsystem.scaleRenderViewport = renderScale;
            }
        }
        
        public void ToggleComfortVignette(bool enable)
        {
            useComfortVignette = enable;
            // Implementation depends on your post-processing setup
        }
        
        public void SetSnapTurnAngle(float angle)
        {
            snapTurnAngle = Mathf.Clamp(angle, 15f, 90f);
        }
        
        public void OptimizeForBattery()
        {
            if (isMobileVR)
            {
                SetRenderScale(0.8f);
                targetFrameRate = 72f;
                Application.targetFrameRate = (int)targetFrameRate;
            }
        }
        
        public void OptimizeForPerformance()
        {
            if (isMobileVR)
            {
                SetRenderScale(1.0f);
                targetFrameRate = 90f;
                Application.targetFrameRate = (int)targetFrameRate;
            }
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                // Handle VR pause (e.g., headset removed)
                foreach (var inputSubsystem in inputSubsystems)
                {
                    inputSubsystem.TryRecenter();
                }
            }
        }
        
        private void OnDestroy()
        {
            foreach (var inputSubsystem in inputSubsystems)
            {
                inputSubsystem.trackingOriginUpdated -= OnTrackingOriginUpdated;
            }
        }
        
        // Helper method to check if VR is supported on the current platform
        public bool IsVRSupported()
        {
            return XRGeneralSettings.Instance?.Manager?.activeLoader != null;
        }
        
        // Helper method to get the current VR device name
        public string GetCurrentVRDeviceName()
        {
            return XRSettings.loadedDeviceName;
        }
        
        // Helper method to check if room-scale VR is supported
        public bool IsRoomScaleSupported()
        {
            foreach (var inputSubsystem in inputSubsystems)
            {
                if (inputSubsystem.GetTrackingOriginMode() == TrackingOriginModeFlags.Floor)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
