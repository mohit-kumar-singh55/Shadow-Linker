using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Reference")]
    public AudioSource audioSource;
    public AudioClip[] bgm;

    private bool forceStopBGM = false;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        if (audioSource == null || bgm.Length == 0)
        {
            Debug.LogWarning("No Audio Source or BGM Provided!");
            enabled = false;
            return;
        }

        PlayBGM();
    }

    void LateUpdate()
    {
        if (!forceStopBGM && !audioSource.isPlaying) PlayBGM();
    }

    public void PlayBGM()
    {
        forceStopBGM = false;
        audioSource.PlayOneShot(bgm[Random.Range(0, bgm.Length)]);
    }

    public void StopBGM()
    {
        forceStopBGM = true;
        audioSource.Stop();
    }
}
