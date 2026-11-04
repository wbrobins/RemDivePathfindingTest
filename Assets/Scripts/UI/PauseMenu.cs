using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject controlsPanel;

    public static bool IsPaused { get; private set; }

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
                CloseSettings();
            else if (controlsPanel.activeSelf)
                CloseControls();
            else
                SetPaused(!IsPaused);
        }
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        pausePanel.SetActive(paused);
        settingsPanel.SetActive(false);

        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    public void Resume() => SetPaused(false);

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OpenControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void Quit()
    {
        Time.timeScale = 1f; // reset so the next scene isn't frozen
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
