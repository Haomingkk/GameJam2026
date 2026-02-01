using UnityEngine;

public class MonsterAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float range = 10f;
    [SerializeField] private float playInterval = 2f;

    private Transform playerTransform;
    private float playTimer;

    void Start()
    {
        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (!playerTransform || audioClips.Length == 0)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= range)
        {
            playTimer += Time.deltaTime;
            if (playTimer >= playInterval)
            {
                playTimer = 0f;
                PlayRandomAudio();
            }
        }
        else
        {
            playTimer = 0f;
        }
    }

    private void PlayRandomAudio()
    {
        if (!audioSource || audioClips.Length == 0)
        {
            return;
        }

        AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];
        audioSource.PlayOneShot(randomClip);
        Debug.Log("Playing monster audio: " + randomClip.name);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
