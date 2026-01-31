using UnityEngine;

namespace GameJam26
{
    public class Chest : MonoBehaviour, IInteractable
    {
        public bool isOpen;
        public int coins;
        SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SendPlayerCoin(int coinAmount)
        {
            // Find Player and add coin

        }

        public void Interact()
        {
            if (isOpen)
            {
                Debug.Log("Chest is already open!");
                return;
            }

            // Open the chest
            isOpen = true;
            Debug.Log($"Chest opened! Received {coins} coins.");

            if (spriteRenderer)
            {
                Color transparent = spriteRenderer.color;
                transparent.a = 0.3f;
                spriteRenderer.color = transparent;
            }
            // Give coins to player
            SendPlayerCoin(coins);
        }
    }
}

