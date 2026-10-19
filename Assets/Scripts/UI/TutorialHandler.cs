using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject tutorialObjPrefab;
    private TutorialText currObj;
    [SerializeField] private string nextScene;
    public bool goToNextScene = false;

    void Start()
    {
        DisplayTutorial("Yo");
    }

    public void DisplayTutorial(string pText)
    {
        if(currObj != null)
        {
            currObj = null;
        }

        currObj = Instantiate(tutorialObjPrefab).GetComponent<TutorialText>();
        currObj.DisplayText(pText);
        if (goToNextScene)
        {
           currObj.SetNextScene(nextScene); 
           currObj.SetButtonToNextScene();
        }
    }
}
