using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance { get; private set; }

    [SerializeField] private Image crosshairImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private float hitDuration = 0.1f;

    private Coroutine routine;

    void Awake()
    {
        Instance = this;
        if (crosshairImage == null) crosshairImage = GetComponent<Image>();
        crosshairImage.sprite = normalSprite;
    }

    public void ShowHit()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        crosshairImage.sprite = hitSprite;
        yield return new WaitForSeconds(hitDuration);
        crosshairImage.sprite = normalSprite;
        routine = null;
    }
}
