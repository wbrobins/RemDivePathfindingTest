using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject player;
    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnEnable()
    {
        GameSettings.OnFovChanged += ApplyFov;
        ApplyFov(GameSettings.Fov);
    }

    void OnDisable()
    {
        GameSettings.OnFovChanged -= ApplyFov;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if(Cursor.lockState == CursorLockMode.Locked)
        {
          rotation.x += Input.GetAxis(xAxis) * 2;
            rotation.y += Input.GetAxis(yAxis) * 2;
            rotation.y = Math.Clamp(rotation.y, -90f, 90f);
            var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
            var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

            transform.localRotation = xQuat * yQuat;  
        }
    }

    void ApplyFov(float fov)
    {
        cam.fieldOfView = fov;
    }
}
