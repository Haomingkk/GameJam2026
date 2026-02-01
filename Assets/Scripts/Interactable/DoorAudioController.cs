using UnityEngine;

namespace GameJam26
{
    public class DoorAudioController : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] private AudioClip openDoorClip;
    [SerializeField] private AudioClip closeDoorClip;

    public void PlayOpenDoorOneShot()
    {
        if (audioSource == null || openDoorClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(openDoorClip);
    }

    public void PlayCloseDoorOneShot()
    {
        if (audioSource == null || closeDoorClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(closeDoorClip);
    }
}
}

