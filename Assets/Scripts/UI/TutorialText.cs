using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    [SerializeField] private string sceneName;

    void Start()
    {   
        //show cursor
        Cursor.lockState = CursorLockMode.None;
        //pause game
        Time.timeScale = 0;
    }

    public void SetButtonToNextScene()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(NextScene);
    }

    public void SetNextScene(string pSceneName)
    {
        sceneName = pSceneName;
    }

    public void NextScene()
    {
        if(sceneName != null)
        {
           SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("ERROR: Scene not set");
        }
        
    }

    public void DisplayText(string pText)
    {
        text.text = pText;
    }

    public void ExitUi()
    {
        Destroy(gameObject);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
    }
}
