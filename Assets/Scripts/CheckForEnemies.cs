using UnityEngine;
using UnityEngine.Events; 

public class CheckForEnemies : MonoBehaviour
{
    public UnityEvent onAllEnemiesDefeated;

    [Tooltip("Tick if a SpawnManager will call SetSpawningFinished(). Untick for scenes with only pre-placed enemies.")]
    [SerializeField] bool waitForSpawner = true;
    [SerializeField, TextArea(3,10)] private string tText;
    private TutorialHandler tutorialHandler;

    bool spawningFinished;
    bool fired;
    private PlayerController player;

    void OnEnable()  => EnemyBehavior.OnEnemyRemoved += HandleEnemyRemoved;
    void OnDisable() => EnemyBehavior.OnEnemyRemoved -= HandleEnemyRemoved;

    void Start()
    {
        tutorialHandler = GameObject.Find("TutorialHandler").GetComponent<TutorialHandler>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        //start runs after every OnEnable in the scene, so Active is already populated
        spawningFinished = !waitForSpawner;
        Check();
        player.UpdateAlertText("Enemies remaining: " + EnemyBehavior.Active.Count.ToString());
    }

    public void SetSpawningFinished()
    {
        spawningFinished = true;
        Check();
    }

    void HandleEnemyRemoved(EnemyBehavior enemy)
    {
        Check();
        player.UpdateAlertText("Enemies remaining: " + EnemyBehavior.Active.Count.ToString());
    }

    void Check()
    {
        if (fired || !spawningFinished) return;
        if (EnemyBehavior.Active.Count > 0) return;

        fired = true;
        Debug.Log("All enemies defeated!");
        onAllEnemiesDefeated.Invoke();
    }

    public void DisplayFinalText()
    {
        tutorialHandler.goToLink = true;
        tutorialHandler.DisplayTutorial(tText);
    }
}


