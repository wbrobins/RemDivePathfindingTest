using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Toggle toggleSprintCheckbox;

    private void Awake()
    {
        toggleSprintCheckbox.onValueChanged.AddListener(v => GameSettings.ToggleSprint = v);
    }

    private void OnEnable()
    {
        toggleSprintCheckbox.SetIsOnWithoutNotify(GameSettings.ToggleSprint);
    }
}
