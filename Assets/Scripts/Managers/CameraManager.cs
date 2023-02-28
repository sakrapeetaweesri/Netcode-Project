using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera vcam;

    public static CameraManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Assigns the given transform for the v-cam to follow.
    /// </summary>
    /// <param name="objectToFollow">The transform for the v-cam to follow.</param>
    public void AssignCameraTracker(Transform objectToFollow)
    {
        vcam.m_Follow = objectToFollow;
    }
}