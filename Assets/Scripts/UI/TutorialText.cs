using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialText : MonoBehaviour
{
    private static int activeCount = 0;
    public static bool AnyShowing => activeCount > 0;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    private string sceneName;
    private string surveyLink;
    private bool showingTutorial = false;

    void Start()
    {   
        //show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        showingTutorial = true;
        //pause game
        Time.timeScale = 0;
    }

    public void SetButtonToNextScene()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(NextScene);
    }

    public void SetButtonToLink()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenLink);
    }

    public void SetNextScene(string pSceneName)
    {
        sceneName = pSceneName;
    }

    public void SetLink(string pLink)
    {
        surveyLink = pLink;
    }

    public void NextScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene not set");
            return;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }

    public void OpenLink()
    {
        Application.OpenURL(surveyLink);
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
        showingTutorial = false;
    }

    public bool ShowingTutorial()
    {
        return showingTutorial;
    }
    
    void OnEnable()  
    { 
        activeCount++; 
    }

    void OnDestroy() 
    { 
        activeCount--; 
    }
}
