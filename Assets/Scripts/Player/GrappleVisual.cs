using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrappleVisual : MonoBehaviour
{
    public Transform targetTransform;
    [SerializeField] private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer.positionCount = 2;

        if(targetTransform == null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (targetTransform != null)
        {
            lineRenderer.enabled = true;
            
            //point 0 follows this object, Point 1 follows the target
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, targetTransform.position);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public void ConnectTo(Transform newTarget)
    {
        targetTransform = newTarget;
    }

    public void Disconnect()
    {
        targetTransform = null;
        lineRenderer.enabled = false;
    }
}
