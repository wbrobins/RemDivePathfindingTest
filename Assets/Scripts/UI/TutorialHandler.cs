using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject tutorialObjPrefab;
    [SerializeField, TextArea(3,10)] private string text = "Yo";
    private TutorialText currObj;
    [SerializeField] private string nextScene;
    [SerializeField] private string surveyLink;
    public bool goToNextScene = false;
    public bool goToLink = false;

    void Start()
    {
        DisplayTutorial(text);
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
        else if (goToLink)
        {
            currObj.SetLink(surveyLink);
            currObj.SetButtonToLink();
        }
    }
}
