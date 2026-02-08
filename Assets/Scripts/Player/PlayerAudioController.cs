using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioSource sourceB; // for player walk and die
    [SerializeField] AudioClip heartbeatClip;
    [SerializeField] AudioClip attackClip;
    [SerializeField] AudioClip[] walkClip;
    [SerializeField] AudioClip dieClip;
    [SerializeField] float radius = 25f;
    [SerializeField] float minInterval = 1f;   // very close
    [SerializeField] float maxInterval = 5.0f;    // near edge
    [SerializeField] float distanceCheckEvery = 0.1f;

    GameObject[] monsters;
    float distTimer;
    float playTimer;
    float lastDist = Mathf.Infinity;

    void Start()
    {
        if (!source) source = GetComponent<AudioSource>();
        if (!sourceB) sourceB = GetComponent<AudioSource>(); // not ideal
        monsters = GameObject.FindGameObjectsWithTag("Monster");
    }

    void Update()
    {
        distTimer += Time.deltaTime;
        if (distTimer >= distanceCheckEvery)
        {
            distTimer = 0f;
            lastDist = DistanceToNearestMonster();
        }

        float interval = Mathf.Lerp(minInterval, maxInterval, lastDist / radius);

        playTimer += Time.deltaTime;
        if (playTimer >= interval)
        {
            playTimer = 0f;
            source.PlayOneShot(heartbeatClip);
        }
    }
    
    float DistanceToNearestMonster()
    {
        if (monsters == null || monsters.Length == 0) return Mathf.Infinity;
    
        Vector3 p = transform.position;
        float best = Mathf.Infinity;

        for (int i = 0; i < monsters.Length; i++)
        {
            var m = monsters[i];
            if (!m) continue;

            // squared distance (cheaper than Vector3.Distance)
            float d2 = (m.transform.position - p).sqrMagnitude;
            if (d2 < best) best = d2;
        }

        return Mathf.Sqrt(best);
    }

    public void PlayerWalk()
    {
        if (!sourceB || walkClip.Length == 0)
        {
            return;
        }

        // Only play walk if no other audio is actively playing on sourceB
        if (sourceB.isPlaying)
        {
            return;
        }

        AudioClip ac = walkClip[Random.Range(0, walkClip.Length)];
        sourceB.PlayOneShot(ac);
    }

    public void PlayerFail()
    {
        if (!sourceB || dieClip == null)
        {
            return;
        }

        sourceB.PlayOneShot(dieClip);
    }

        public void PlayerAttach()
    {
        if (!sourceB || attackClip == null)
        {
            return;
        }

        sourceB.PlayOneShot(attackClip);
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
