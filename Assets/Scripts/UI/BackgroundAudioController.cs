using System.Collections;
using UnityEngine;

public class BackgroundAudioController : MonoBehaviour
{
    public static BackgroundAudioController Instance { get; private set; }

    public enum AudioState { Normal, Chasing }

    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip chaseClip;

    [SerializeField] private AudioSource audioSourceA; // ambient
    [SerializeField] private AudioSource audioSourceB; // chase

    [SerializeField] private float fadeSeconds = 1.5f;
    [SerializeField] private float ambientVolume = 1f;
    [SerializeField] private float chaseMaxVolume = 1f;

    public AudioState currentState = AudioState.Normal;

    private AudioSource ambient;
    private AudioSource chase;
    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ambient = audioSourceA;
        chase = audioSourceB;

        ambient.clip = ambientClip;
        ambient.loop = true;
        ambient.spatialBlend = 0f;
        ambient.playOnAwake = false;

        chase.clip = chaseClip;
        chase.loop = true;
        chase.spatialBlend = 0f;
        chase.playOnAwake = false;
    }

    void Start()
    {
        ambient.volume = ambientVolume;
        ambient.Play();

        chase.volume = 0f;
        ApplyStateImmediate(currentState);
    }

    public void SetState(AudioState state)
    {
        if (state == currentState) return;
        currentState = state;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        float target = (state == AudioState.Chasing) ? chaseMaxVolume : 0f;
        fadeRoutine = StartCoroutine(FadeChase(target, fadeSeconds));
    }

    //for buttons to work
    public void SetDefaultState()
    {
        SetState(AudioState.Normal);
    }

    public void SetChasingState()
    {
        SetState(AudioState.Chasing);
    }
    void ApplyStateImmediate(AudioState state)
    {
        ambient.volume = ambientVolume;
        if (!ambient.isPlaying) ambient.Play();

        if (state == AudioState.Chasing)
        {
            if (!chase.isPlaying) chase.Play();
            chase.volume = chaseMaxVolume;
        }
        else
        {
            chase.volume = 0f;
        }
    }

    IEnumerator FadeChase(float target, float seconds)
    {
        if (target > 0f && !chase.isPlaying) chase.Play();

        float start = chase.volume;
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / seconds);
            chase.volume = Mathf.Lerp(start, target, a);
            yield return null;
        }

        chase.volume = target;

        if (Mathf.Approximately(target, 0f))
            chase.Stop();

        fadeRoutine = null;
    }
}
