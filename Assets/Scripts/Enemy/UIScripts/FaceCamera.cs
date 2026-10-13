using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = cam.transform.rotation;
    }
}
