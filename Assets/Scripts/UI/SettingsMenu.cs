using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Toggle toggleSprintCheckbox;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovLabel; 

    private void Awake()
    {
        toggleSprintCheckbox.onValueChanged.AddListener(v => GameSettings.ToggleSprint = v);

        fovSlider.minValue = GameSettings.MinFov;
        fovSlider.maxValue = GameSettings.MaxFov;
        fovSlider.wholeNumbers = true;
        fovSlider.onValueChanged.AddListener(v =>
        {
            GameSettings.Fov = v;
            UpdateFovLabel(v);
        });
    }

    private void OnEnable()
    {
        toggleSprintCheckbox.SetIsOnWithoutNotify(GameSettings.ToggleSprint);
        fovSlider.SetValueWithoutNotify(GameSettings.Fov);
        UpdateFovLabel(GameSettings.Fov);
    }

    private void UpdateFovLabel(float v)
    {
        if (fovLabel != null) fovLabel.text = Mathf.RoundToInt(v).ToString();
    }
}
