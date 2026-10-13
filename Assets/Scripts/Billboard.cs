using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform followTarget;       
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.5f, 0);

    private Camera cam;

    void Start() => cam = Camera.main;

    void LateUpdate()
    {
        if (followTarget) transform.position = followTarget.position + worldOffset;
        if (cam) transform.forward = cam.transform.forward;
    }
}