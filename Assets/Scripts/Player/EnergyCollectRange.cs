using UnityEngine;
namespace GameJam2026.GamePlay
{

    public class EnergyCollectRange : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
           
            if (collision.CompareTag("Monster")) { Debug.Log("Find an enemy!"); PlayerController.instance.UpdateGatheringEnergy(1); }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Monster")) { Debug.Log("Leave an enemy!"); PlayerController.instance.UpdateGatheringEnergy(-1); }
        }
    }
}
