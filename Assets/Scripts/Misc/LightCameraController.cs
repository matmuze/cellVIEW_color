﻿using UnityEngine;

[ExecuteInEditMode]
public class LightCameraController : MonoBehaviour
{
    public Camera LightCamera;
    public Camera MainCamera;
    
    public bool EnableLight;
    public bool FollowMainCamera;

    [Range(0, 150)]
    public float CameraOffsetZ;

    [Range(0, 15)]
    public float LightSize;  // 2.0

    [Range(-5, 5)]
    public float ShadowOffset;  // 0.25 

    [Range(-5, 5)]
    public float ShadowFactor; // 2

    [Range(-5, 5)]
    public float ShadowBias;

    void Update()
    {
        LightCamera.gameObject.SetActive(EnableLight);

        if (FollowMainCamera)
        {
            LightCamera.transform.rotation = MainCamera.transform.rotation;
            LightCamera.transform.position = MainCamera.transform.position + MainCamera.transform.forward * -CameraOffsetZ;
        }
    }
}