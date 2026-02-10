using GameJam26;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace GameJam2026.GamePlay
{
   
    public class ExistingDoor : MonoBehaviour,IInteractable
    {
        [Header("Door Access Requirement")]
        [SerializeField] private int _coinToOpen=666;

        [Header("Door Barrier Setting")]
        [SerializeField] private BoxCollider2D _doorBar;
        [SerializeField] private GameObject _doorBarrierSprite;
        private bool _isOpen=false;
        public void Interact()
        {
            if (_isOpen||PlayerController.instance.GetPlayerCoins()<_coinToOpen) { return; }
            _isOpen = true;
            _doorBar.enabled= false;
            _doorBarrierSprite.SetActive(false);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("PlayerFeet")) { PlayerController.instance.OnPlayerEscaped(); }
        }
    }
}
