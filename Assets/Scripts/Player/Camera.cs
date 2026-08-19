using System;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject player;
    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        player = GameObject.Find("Player");
    }

    void Update()
    {
        rotation.x += Input.GetAxis(xAxis) * 2;
        rotation.y += Input.GetAxis(yAxis) * 2;
        rotation.y = Math.Clamp(rotation.y, -90f, 90f);
        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);

        transform.localRotation = xQuat * yQuat;
        //player.transform.localRotation = xQuat;
    }
}
