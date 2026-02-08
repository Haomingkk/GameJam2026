using UnityEngine;
namespace GameJam2026.GamePlay
{
    public class Coins : MonoBehaviour
    {
        private int _value = 1;
        bool picked;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (picked) return;
            if (!other.CompareTag("CoinCollector")) return;

            picked = true;
            PlayerController.instance.OnCoinCollected(_value);
          
            Destroy(gameObject);
        }
    }
}
