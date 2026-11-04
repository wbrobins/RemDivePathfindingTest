using UnityEngine;

public static class GameSettings
{
    private const string ToggleSprintKey = "ToggleSprint";

    public static bool ToggleSprint
    {
        get => PlayerPrefs.GetInt(ToggleSprintKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(ToggleSprintKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
