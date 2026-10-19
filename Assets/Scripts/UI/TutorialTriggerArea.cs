using UnityEngine;

public class TutorialTriggerArea : MonoBehaviour
{
    private TutorialHandler tutorialHandler;
    [SerializeField] private string tText;

    void Start()
    {
        tutorialHandler = GameObject.Find("TutorialHandler").GetComponent<TutorialHandler>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            tutorialHandler.goToNextScene = true;
            tutorialHandler.DisplayTutorial(tText);
        }
    }
}
