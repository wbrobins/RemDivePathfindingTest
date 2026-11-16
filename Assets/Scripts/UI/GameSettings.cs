using System;
using UnityEngine;

public static class GameSettings
{
    private const string ToggleSprintKey = "ToggleSprint";
    private const string FovKey = "Fov";

    public const float MinFov = 50f;
    public const float MaxFov = 90f;
    public const float DefaultFov = 60f;

    public static event Action<float> OnFovChanged;

    public static bool ToggleSprint
    {
        get => PlayerPrefs.GetInt(ToggleSprintKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(ToggleSprintKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static float Fov
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(FovKey, DefaultFov), MinFov, MaxFov);
        set
        {
            value = Mathf.Clamp(value, MinFov, MaxFov);
            PlayerPrefs.SetFloat(FovKey, value);
            PlayerPrefs.Save();
            OnFovChanged?.Invoke(value);
        }
    }
}
