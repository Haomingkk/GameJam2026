using GameJam2026.GamePlay;
using UnityEngine;

namespace GameJam26
{
    public class Chest : MonoBehaviour, IInteractable
    {
        public bool isOpen;
        public int coins;

        [SerializeField] private Sprite _chestOpenSprite;
        [SerializeField] private Vector2 _spawnRadius;
        [SerializeField] private float _spawnPosOffsite;
        [SerializeField] GameObject CoinPrefab;

        SpriteRenderer spriteRenderer;
        ChestAudioController chestAudioController;
        Animator animator;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            chestAudioController = GetComponent<ChestAudioController>();
            animator = GetComponent<Animator>();
        }

        public void SendPlayerCoin(int coinAmount)
        {
           PlayerController.instance.OnCoinCollected(coinAmount);

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


            spriteRenderer.sprite = _chestOpenSprite;
            //Romove this feature in update 2026.2
            /*if (spriteRenderer)
            {
                Color transparent = spriteRenderer.color;
                transparent.a = 0.3f;
                spriteRenderer.color = transparent;
            }*/
            // Give coins to player
            //SendPlayerCoin(coins);
            GenerateResourceInScene();

            //audio
            if(chestAudioController != null)
            {
                chestAudioController.PlayOpenThenBonus();
            }
        }
        public void GenerateResourceInScene()
        {
            

            for (int i = 0; i < coins; i++)
            {
                var spawnPos = new Vector3(transform.position.x + Random.Range(-_spawnRadius.x, _spawnRadius.x),
                    transform.position.y + _spawnPosOffsite + Random.Range(-_spawnRadius.y, _spawnRadius.y), 0);

                var item = Instantiate(
                CoinPrefab,
                spawnPos,
                Quaternion.identity
                );

                var direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), 0).normalized;


                //item.GetComponent<ItemBounce>().InitBounceItem(direction * _spawnForce);
            }
        }
    }
}

