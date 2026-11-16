using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Toggle toggleSprintCheckbox;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovLabel; 
    [SerializeField] private Slider volumeSlider;

    private void Awake()
    {
        //sprint toggle
        toggleSprintCheckbox.onValueChanged.AddListener(v => GameSettings.ToggleSprint = v);

        //fov slider
        fovSlider.minValue = GameSettings.MinFov;
        fovSlider.maxValue = GameSettings.MaxFov;
        fovSlider.wholeNumbers = true;
        fovSlider.onValueChanged.AddListener(v =>
        {
            GameSettings.Fov = v;
            UpdateFovLabel(v);
        });

        //master volume slider
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.onValueChanged.AddListener(v => GameSettings.MasterVolume = v);
    }

    private void OnEnable()
    {
        //sprint toggle
        toggleSprintCheckbox.SetIsOnWithoutNotify(GameSettings.ToggleSprint);

        //fov slider
        fovSlider.SetValueWithoutNotify(GameSettings.Fov);
        UpdateFovLabel(GameSettings.Fov);

        //master volume slider
        volumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
    }

    private void UpdateFovLabel(float v)
    {
        if (fovLabel != null) fovLabel.text = Mathf.RoundToInt(v).ToString();
    }
}
