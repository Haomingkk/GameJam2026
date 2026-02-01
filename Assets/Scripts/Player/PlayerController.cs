using GameJam26;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GameJam2026.GamePlay
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance { get; private set; }

        public Action OnEnergyUpdate;
        public Action OnCoinUpdate;
        public Action OnHealthUpdate;

        private Rigidbody2D _rb2D;

        [Header("Player Movement")]
        [SerializeField]private float _walkSpeed = 5.0f;
        [SerializeField] private float _interactiveFreezeTime = 2.0f;
        [SerializeField] private float _damagedFreezeTime = 1.0f;
        [SerializeField] private float _dieAnimationLength = 2.0f;

        [Header("Player Status")]
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _maxEnergy = 10f;
        [SerializeField] private int _maxCoin = 99999;
        [SerializeField] private float _invincibleTime = 0.5f;
        private bool _isInvicible;
        private int _health;
        private PlayerState _playerState;
        private int _coin;
        private float _energy;
        [Header("Player Sight")]
        [SerializeField] GameObject _normalSight;
        [SerializeField] GameObject _maskDSight;

        [Header("Mask Status")]
        [SerializeField] private float _energyComsumeSpeed=0.1f;
        [SerializeField] private float _energyGatheringSpeed = 0.2f;
        [SerializeField] private float _coinMaskDGatherTime = 1.0f;
        private float _coinMaskDGatherTimer;
        private bool _isGartheringEnergy;
        
        public MaskState maskState { get; private set; }

        private readonly List<IInteractable> _nearby = new();
        private IInteractable _currentInteractable;
        private bool _calculatingNearInteractable=true;

        private bool _isPlayerinControl=true;// change with game mode, may change while we have game mode script
        private bool _isAllowToMove=true;

        private Vector2 _moveInput;

        private Coroutine _lootCoroutine;
        private Coroutine _damagedCoroutine;
        #region Unity Lifecycle
        private void Awake()
        {
            _rb2D = GetComponent<Rigidbody2D>();
            if (instance != null)
                Destroy(gameObject);
            else
                instance = this;
        }
        private void OnEnable()
        {
            
        }
        private void OnDisable()
        {
            
        }
        private void Start()
        {
            _InitPlayerStatus();
        }
        private void Update()
        {
            _HandlePlayerInput();
            _HandleMaskAStates();
            _UpdateMaskEnergy();
        }
        private void FixedUpdate()
        {
            _PlayerMovement();
            if (_moveInput != Vector2.zero)
               _UpdateViewDirection(_moveInput);
        }
        #endregion
        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;

            if (!_nearby.Contains(interactable))
                _nearby.Add(interactable);

            _currentInteractable = _GetClosestInteractable();
        }
 
        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponentInParent<IInteractable>();
            if (interactable == null) return;

            _nearby.Remove(interactable);

            _currentInteractable = _GetClosestInteractable();
        }
        
        public void OnMove(InputValue value)
        {
            //Debug.Log($"Getting input context {value.Get<Vector2>()}, the player in control is {_isPlayerinControl}, the player {_isAllowToMove} to allow move");
            Vector2 input = value.Get<Vector2>();

            if (_isPlayerinControl && _isAllowToMove)
            { _moveInput = input;
                if (input.x == 0 && input.y == 0)
                {  _playerState = PlayerState.Idle; }
                else
                {
                    _playerState = PlayerState.Move;
                } 
            }
            else
            { _moveInput = Vector2.zero; }

            
        }
        public void OnPlayerDamaged(Vector2 monsterdirection) {
            if (!_isInvicible) {
                _UpdateHealth(-1);
            }
        }
        public void OnCoinCollected(int amount) {
            _UpdateCoin(amount);
        }
      
        private void _InitPlayerStatus() {
            _health = _maxHealth;
            _energy = _maxEnergy;
            _playerState = PlayerState.Idle;
            maskState = MaskState.None;
            
            _isPlayerinControl = true;
            _isAllowToMove = true;

            _calculatingNearInteractable = true;
        }
        /// <summary>
        /// The actual execution of the Player's movement is done through moveInput data
        /// </summary>
        private void _PlayerMovement() {
            if (_isAllowToMove== false)
            {
                _rb2D.linearVelocity = Vector2.zero;
            }
            else
            {
                _rb2D.linearVelocity = _moveInput.normalized * _walkSpeed;
                //Debug.Log($"Player is moving in {_rb2D.linearVelocity} speed");
            }
            
        }
        private IInteractable _GetClosestInteractable()
        {
            if (_calculatingNearInteractable == false) return null;
            Vector2 p = transform.position;

            float bestDistSq = float.PositiveInfinity;
            IInteractable best = null;


            for (int i = _nearby.Count - 1; i >= 0; i--)
            {
                var it = _nearby[i];
                if (it == null)
                {
                    _nearby.RemoveAt(i);
                    continue;
                }

                var comp = it as Component;
                if (comp == null) continue;

                float d = ((Vector2)comp.transform.position - p).sqrMagnitude;
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = it;
                }
            }

            return best;
        }
        private void _HandlePlayerInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskA);

            if (kb.digit2Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskB);

            if (kb.digit3Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskD);
            if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame) {
                _TryInteractive();
            }
        }
        private void _ToggleMask(MaskState target)
        {
            if (maskState == target)
            { maskState = MaskState.None; }
            else if ((target == MaskState.InvalidMaskA && maskState == MaskState.MaskA) || (target == MaskState.MaskA && maskState == MaskState.InvalidMaskA))
            {
                maskState = MaskState.None;
            }
            else if (_energy > 0 || maskState == MaskState.MaskD) 
            { 
                maskState = target;
                if (maskState == MaskState.MaskD) {
                    _SwitchToMaskDSight();
                    return;
                }
                    }
            Debug.Log($"Player Switch to mask {maskState} now!");
            _SwitchToNormalSight();
        }
        private void _HandleMaskAStates() {
            if (maskState != MaskState.MaskA && maskState != MaskState.InvalidMaskA) return;

            if (_playerState == PlayerState.Move || _playerState == PlayerState.TakeDamage || _playerState == PlayerState.Interact)
            {
                if (maskState == MaskState.MaskA) { maskState = MaskState.InvalidMaskA; }
            }
            else if (maskState == MaskState.InvalidMaskA) { maskState = MaskState.MaskA; }
            Debug.Log($"Player Switch to mask {maskState} now!");
        }
        private void _UpdateViewDirection(Vector2 move)
        {
            if (move.sqrMagnitude < 0.001f)
                return;

            move.Normalize();
            float angle = Mathf.Atan2(move.y, move.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45f) * 45f;
            _maskDSight.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        private void _UpdateMaskEnergy() {
            if (maskState == MaskState.None) { return; }
            if (maskState != MaskState.MaskD)
            {
                _energy -= _energyComsumeSpeed * Time.deltaTime;
                if (_energy <= 0) { _energy = 0;_ToggleMask(maskState); }
                OnEnergyUpdate?.Invoke();
            }
            else  {
                if (_isGartheringEnergy)
                {
                    _energy = Math.Min(_energy += _energyGatheringSpeed *= Time.deltaTime, _maxEnergy);
                    _coinMaskDGatherTimer += Time.deltaTime;
                    if (_coinMaskDGatherTimer >= _coinMaskDGatherTime) {
                        _coinMaskDGatherTimer = 0;
                        _UpdateCoin(1);
                    }
                    OnEnergyUpdate?.Invoke();
                }
            }
            Debug.Log($"Energy is Updating . Player has {_energy} Energy now!");
        }
        private void _UpdateCoin(int amount) {
            if (amount > 0) {
                _coin = Math.Min(_coin + amount, _maxCoin);
                Debug.Log($"Coin {amount}");
                OnCoinUpdate?.Invoke();
            }
        }
        private void _UpdateHealth(int amount) {
            if (amount < 0) {
                _health += amount;
                if (_health <= 0)
                {
                    StartCoroutine(_OnDie());
                }
                else {
                    if (_lootCoroutine != null) { StopCoroutine(_lootCoroutine); }
                    if (_damagedCoroutine != null) { StopCoroutine(_damagedCoroutine); }
                    StartCoroutine(_OnDamaged());
                }
               
            }
            OnHealthUpdate?.Invoke();
        }
        private void _SwitchToNormalSight() {
            _normalSight.SetActive(true);
            _maskDSight.SetActive(false);
        }
        private void _SwitchToMaskDSight() {
            _normalSight.SetActive(false);
            _maskDSight.SetActive(true);
        }

        private void _TryInteractive() { 
           if(_currentInteractable==null) return;
           var comp = (_currentInteractable as Component);
           if (comp == null) return;
            //if treasure box, startloot
            if (comp.TryGetComponent<Chest>(out var chest))
            {
                _lootCoroutine=StartCoroutine(_OnLootRountine());
            }
            else {
                _currentInteractable?.Interact();
            }
           
        }
        private IEnumerator _OnLootRountine() {
            _isAllowToMove = false;
            _calculatingNearInteractable = false;
            _playerState = PlayerState.Interact;
            yield return new WaitForSeconds(_interactiveFreezeTime);
            _isAllowToMove = true;
            _calculatingNearInteractable = true;
            _playerState = PlayerState.Idle;
            _currentInteractable?.Interact();
        }
        private void _InterruptLootCoroutine() {
            StopCoroutine(_lootCoroutine);
            _calculatingNearInteractable = true;
            _lootCoroutine = null;
        }
        private IEnumerator _OnDamaged() {
            _playerState = PlayerState.TakeDamage;
            _isAllowToMove = false;
            yield return new WaitForSeconds(_damagedFreezeTime);
            _playerState = PlayerState.Idle;
            _isAllowToMove = true;
        }
        private IEnumerator _OnInvencible() { 
           _isInvicible = true;
            yield return new WaitForSeconds(_invincibleTime);
            _isInvicible = false;
        
        }
        private IEnumerator _OnDie() {
            yield return new WaitForSeconds(_dieAnimationLength);
        }
    }
    

    public enum MaskState { MaskA,InvalidMaskA,MaskB,MaskD,None}
    public enum PlayerState { Idle,Move,Interact,TakeDamage,Die}
}
