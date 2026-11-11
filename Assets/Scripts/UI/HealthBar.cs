using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image fill;
    [SerializeField] bool hideWhenFull = true;

    public void SetHealth(float current, float max)
    {
        float pct = Mathf.Clamp01(current/max);
        fill.fillAmount = pct;
        gameObject.SetActive(!(hideWhenFull && pct >= 1f));
    }
}
