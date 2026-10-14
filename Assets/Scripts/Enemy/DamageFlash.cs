using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    private SkinnedMeshRenderer skinnedMeshRenderer;
    
    [Header("Materials")]
    public Material normalMaterial;
    public Material redMaterial;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        //switch to the red material
        skinnedMeshRenderer.material = redMaterial;

        //wait for a fraction of a second
        yield return new WaitForSeconds(0.15f);

        //revert back to the normal material
        skinnedMeshRenderer.material = normalMaterial;
    }
}