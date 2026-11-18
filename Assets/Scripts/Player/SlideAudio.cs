using UnityEngine;

public class SlideAudio : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    [Header("Sources")]
    [SerializeField] private AudioSource introSource;
    [SerializeField] private AudioSource loopSource;

    [Header("Clips")]
    [SerializeField] private AudioClip slideIntro;
    [SerializeField] private AudioClip slideLoop;

    [Header("Tuning")]
    [SerializeField] private float maxVolume = 0.6f;
    [SerializeField] private float introVolume = 0.4f;
    [SerializeField] private float minSpeed = 8f;
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.85f, 1.1f);
    [Tooltip("Volume units per second when fading in")]
    [SerializeField] private float fadeInRate = 8f;
    [Tooltip("Volume units per second when fading out")]
    [SerializeField] private float fadeOutRate = 6f;

    private bool wasSliding;

    void Start()
    {
        loopSource.clip = slideLoop;
        loopSource.loop = true;
        loopSource.volume = 0f;
    }

    void Update()
    {
        bool sliding = player.CurrState == MovementState.Sliding;

        //slide just started
        if (sliding && !wasSliding)
        {
            introSource.volume = introVolume;
            introSource.PlayOneShot(slideIntro);
            if (!loopSource.isPlaying) loopSource.Play();
        }

        if (!sliding && wasSliding)
        {
            introSource.Stop();
        }
        wasSliding = sliding;

        //loop volume/pitch follow speed; silent if airborne or not sliding
        float targetVolume = 0f;
        if (sliding && player.Grounded)
        {
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, player.HorizontalSpeed);
            targetVolume = Mathf.Lerp(0.5f, 1f, t) * maxVolume;
            loopSource.pitch = Mathf.Lerp(pitchRange.x, pitchRange.y, t);
        }

        float rate = targetVolume > loopSource.volume ? fadeInRate : fadeOutRate;
        loopSource.volume = Mathf.MoveTowards(loopSource.volume, targetVolume, rate * Time.deltaTime);

        if (!sliding && loopSource.isPlaying && loopSource.volume <= 0.001f)
        {
            loopSource.Stop();
        }
    }
}