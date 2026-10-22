using UnityEngine;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    private bool paused = false;

    void Update()
    {
        if (TutorialText.AnyShowing)
        {
            if (paused)
            {
                paused = false;
                pauseMenu.SetActive(false);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;
            pauseMenu.SetActive(paused);
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }
    }

    public bool Paused()
    {
        return paused;
    }
}
