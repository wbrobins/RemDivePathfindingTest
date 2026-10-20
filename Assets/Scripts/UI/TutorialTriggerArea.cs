using UnityEngine;

public class TutorialTriggerArea : MonoBehaviour
{
    private TutorialHandler tutorialHandler;
    [SerializeField, TextArea(3,10)] private string tText;

    void Start()
    {
        tutorialHandler = GameObject.Find("TutorialHandler").GetComponent<TutorialHandler>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && other.GetComponent<PlayerController>() != null)
        {
            tutorialHandler.goToNextScene = true;
            tutorialHandler.DisplayTutorial(tText);
        }
    }
}
