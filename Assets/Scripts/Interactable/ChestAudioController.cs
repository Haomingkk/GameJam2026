using UnityEngine;

namespace GameJam26
{
    public class ChestAudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openChest;
        [SerializeField] private AudioClip bonus;
        [SerializeField] private float bonusDelaySeconds = 1f;

        void Awake()
        {
            if (!audioSource)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void PlayOpenThenBonus()
        {
            StopAllCoroutines();
            StartCoroutine(PlayOpenThenBonusRoutine());
        }

        private System.Collections.IEnumerator PlayOpenThenBonusRoutine()
        {
            if (openChest != null)
            {
                audioSource.PlayOneShot(openChest);
            }

            if (bonusDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(bonusDelaySeconds);
            }

            if (bonus != null)
            {
                audioSource.PlayOneShot(bonus);
            }
        }
    }
}
