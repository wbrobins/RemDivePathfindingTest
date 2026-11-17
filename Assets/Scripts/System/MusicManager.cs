using UnityEngine;

public class MusicManager : MonoBehaviour
{
   public static MusicManager Instance { get; private set;}

    [SerializeField] private AudioSource calmSource;
    [SerializeField] private AudioSource combatSource;
    [SerializeField] private AudioClip defaultTrack;
    [SerializeField] private AudioClip combatTrack;
    [SerializeField, Range(0f,1f)] private float volume = .5f;
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float combatLingerTime = 4f;

    private int combatants;
    private float lingerTimer;
    private float combatBlend;

   

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        calmSource.clip = defaultTrack;
        combatSource.clip = combatTrack;
        calmSource.loop = combatSource.loop = true;

        //schedule both on the same audio clock so they stay sample-aligned
        double startTime = AudioSettings.dspTime + 0.1;
        calmSource.PlayScheduled(startTime);
        combatSource.PlayScheduled(startTime);
    }

    void Update()
    {
        bool inCombat = combatants > 0;

        if (inCombat) lingerTimer = combatLingerTime;
        else lingerTimer -= Time.unscaledDeltaTime;

        float target = (inCombat || lingerTimer > 0f) ? 1f : 0f;
        combatBlend = Mathf.MoveTowards(combatBlend, target, fadeSpeed * Time.unscaledDeltaTime);

        calmSource.volume = (1f - combatBlend) * volume;
        combatSource.volume = combatBlend * volume;
        //Debug.Log($"combatants: {combatants}");
    }

    public void RegisterCombatant() => combatants++;
    public void UnregisterCombatant() => combatants = Mathf.Max(0, combatants - 1);

    public void SetVolume(float v) => volume = Mathf.Clamp01(v);
}
