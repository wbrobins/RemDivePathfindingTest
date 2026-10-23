using UnityEngine;
using UnityEngine.Events; 

public class CheckForEnemies : MonoBehaviour
{
    public UnityEvent onAllEnemiesDefeated;

    [Tooltip("Tick if a SpawnManager will call SetSpawningFinished(). Untick for scenes with only pre-placed enemies.")]
    [SerializeField] bool waitForSpawner = true;
    [SerializeField] private string tText;

    bool spawningFinished;
    bool fired;

    void OnEnable()  => EnemyBehavior.OnEnemyRemoved += HandleEnemyRemoved;
    void OnDisable() => EnemyBehavior.OnEnemyRemoved -= HandleEnemyRemoved;

    void Start()
    {
        //start runs after every OnEnable in the scene, so Active is already populated
        spawningFinished = !waitForSpawner;
        Check();
    }

    public void SetSpawningFinished()
    {
        spawningFinished = true;
        Check();
    }

    void HandleEnemyRemoved(EnemyBehavior enemy) => Check();

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
        GameObject.Find("TutorialHandler").GetComponent<TutorialHandler>().DisplayTutorial(tText);
    }
}


