
using GameJam26;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace GameJam2026.GamePlay
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance { get; private set; }

        public event Action <float> OnEnergyUpdate;
        public event Action <int> OnCoinUpdate;
        public event Action <int> OnHealthUpdate;
        public event Action OnMaskStateUpdate;

        public int watchingPlayerNum;

        [Header("Player Movement")]
        [SerializeField]private float _walkSpeed = 5.0f;
        [SerializeField] private float _interactiveFreezeTime = 2.0f;
        [SerializeField] private float _damagedFreezeTime = 1.0f;
        [SerializeField] private float _dieAnimationLength = 2.0f;
        [SerializeField] private float _watchingModifier = 0.8f;

        [Header("Player Status")]
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _maxEnergy = 10f;
        [SerializeField] private int _maxCoin = 9999;
        [SerializeField] private float _invincibleTime = 0.5f;
        private bool _isInvicible;
        private int _health;
        private PlayerState _playerState;
        private int _coin;
        private float _energy;

        [Header("Knockback")]
        [SerializeField] private float _knockbackForce = 6f;
        [SerializeField] private float _knockbackTime = 0.12f;

        [Header("Player Sight")]
        [SerializeField] GameObject _normalSight;
        [SerializeField] GameObject _maskDSight;
        [SerializeField] GameObject _energyCollectSightRange;

        [Header("Interactive Range")]
        [SerializeField] InteractiveRange _interactiveRange;
        private bool _calculatingNearInteractable = true;

        [Header("Mask Status")]
        [SerializeField] private float _energyComsumeSpeed=0.1f;
        [SerializeField] private float _energyGatheringSpeed = 0.2f;
        [SerializeField] private float _coinMaskDGatherTime = 1.0f;
        [SerializeField] private SpriteRenderer _playerMask;
        [SerializeField] private Sprite[] _maskSpriteImage = new Sprite [3];
        private float _coinMaskDGatherTimer;
        private int _isGartheringEnergy;

        [Header("Player Audio")]
        [SerializeField] private PlayerAudioController _audioController;
        public MaskState maskState { get; private set; }


        private bool _isPlayerinControl=true;// change with game mode, may change while we have game mode script
        private bool _isAllowToMove=true;

        private Rigidbody2D _rb2D;
        private Animator _animator;

        private Vector2 _moveInput;

        private Coroutine _lootCoroutine;
        private Coroutine _damagedCoroutine;
        private Coroutine _knockbackRoutine;

        #region Unity Lifecycle
        private void Awake()
        {
            
            if (instance != null)
                Destroy(gameObject);
            else
                instance = this;

            _rb2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
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
            if (_knockbackRoutine!=null) return;
            _PlayerMovement();
            if (_moveInput != Vector2.zero)
               _UpdateViewDirection(_moveInput);
               _UpdateFacing(_moveInput);
        }
        #endregion
       
        public void OnMove(InputValue value)
        {
            //Debug.Log($"Getting input context {value.Get<Vector2>()}, the player in control is {_isPlayerinControl}, the player {_isAllowToMove} to allow move");
            Vector2 input = value.Get<Vector2>();

            if (_isPlayerinControl && _isAllowToMove)
            {     _moveInput = input;
                if (input.x == 0 && input.y == 0)
                {  _playerState = PlayerState.Idle;
                    _animator.SetBool("isWalking", false);
                }
                else
                {
                    _playerState = PlayerState.Move;
                    _animator.SetBool("isWalking",true);
                    
                } 
            }
            else
            { _moveInput = Vector2.zero; }

            
        }
        public void OnPlayerDamaged(Vector2 monsterdirection) {
            if (!_isInvicible) {
                
                _UpdateHealth(-1);
                Vector2 knockDir = monsterdirection.normalized;
                _StartKnockback(knockDir);
                _OnInvencible();
            }
        }
        public void OnCoinCollected(int amount) {
            _UpdateCoin(amount);
        }

        public MaskState GetCurrentMaskState() {
            return maskState;
        }
        public bool GetCalculatingNearInteractable() { 
          return _calculatingNearInteractable;
        }
        public void UpdateGatheringEnergy(int amount) {
            
            _isGartheringEnergy += amount;
            _isGartheringEnergy = Math.Max(0, _isGartheringEnergy);
            Debug.Log($"Player gathering energy from{_isGartheringEnergy} enemy");
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
                if (maskState != MaskState.MaskB && watchingPlayerNum != 0)
                {
                    _rb2D.linearVelocity = _moveInput.normalized * _walkSpeed * _watchingModifier; 
                }
                else
                {
                    _rb2D.linearVelocity = _moveInput.normalized * _walkSpeed;
                }
                if (_moveInput.x != 0 || _moveInput.y != 0) { _audioController.PlayerWalk(); }

                //Debug.Log($"Player is moving in {_rb2D.linearVelocity} speed");
            }
            
        }
        private void _UpdateFacing(Vector2 move)
        {
            if (move.x > 0.01f)
                _animator.SetBool("isFacingLeft", false);
            else if (move.x < -0.01f)
                _animator.SetBool("isFacingLeft", true);
        }
      
        private void _StartKnockback(Vector2 direction)
        {
            if (_knockbackRoutine != null)
                StopCoroutine(_knockbackRoutine);
            else
            {
                Debug.Log("Start Knock Back!");
                _InterruptLootCoroutine();
                _knockbackRoutine = StartCoroutine(_KnockbackRoutine(direction));
            }
        }
        private void _HandlePlayerInput()
        {
            if (!_isPlayerinControl || !_isAllowToMove) { return; }
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
            if (kb.pKey.wasPressedThisFrame) { OnPlayerDamaged(Vector2.left); }
        }
        private void _ToggleMask(MaskState target)
        {
            if (maskState == target)
            {
                if (maskState == MaskState.MaskB) { EventHandler.CallNotifyDeactiveMaskB(transform); }
                maskState = MaskState.None; _playerMask.sprite = null; 
                
            }
            else if ((target == MaskState.InvalidMaskA && maskState == MaskState.MaskA) || (target == MaskState.MaskA && maskState == MaskState.InvalidMaskA))
            {
                maskState = MaskState.None;
                _playerMask.sprite = null;
            }
            else if (_energy > 0 || target == MaskState.MaskD) 
            { 
                maskState = target;
                if (maskState == MaskState.MaskA || maskState == MaskState.InvalidMaskA) { _playerMask.sprite = _maskSpriteImage[0]; }
                else if (maskState == MaskState.MaskB) { _playerMask.sprite = _maskSpriteImage[1];EventHandler.CallNotifyActiveMaskB(transform); }
                else if (maskState == MaskState.MaskD) 
                {
                    _playerMask.sprite = _maskSpriteImage[2];
                    _SwitchToMaskDSight();
                    OnMaskStateUpdate?.Invoke();
                    return;
                }
                    }
            //Debug.Log($"Player Switch to mask {maskState} now!");
            OnMaskStateUpdate?.Invoke();
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
            _energyCollectSightRange.transform.rotation = Quaternion.Euler(0f, 0f, angle+180);
        }
        private void _UpdateMaskEnergy() {
            if (maskState == MaskState.None) { return; }
            if (maskState != MaskState.MaskD)
            {
                _energy -= _energyComsumeSpeed * Time.deltaTime;
                if (_energy <= 0) { _energy = 0;_ToggleMask(maskState); }
                OnEnergyUpdate?.Invoke(_energy/_maxEnergy);
            }
            else  {
                if (_isGartheringEnergy!=0)
                {
                    _energy = Math.Min(_energy += _energyGatheringSpeed * Time.deltaTime, _maxEnergy);
                    _coinMaskDGatherTimer += Time.deltaTime;
                    if (_coinMaskDGatherTimer >= _coinMaskDGatherTime) {
                        _coinMaskDGatherTimer = 0;
                        _UpdateCoin(1);
                    }
                    OnEnergyUpdate?.Invoke(_energy/_maxEnergy);
                }
            }
            //Debug.Log($"Energy is Updating . Player has {_energy} Energy now!");
        }
        private void _UpdateCoin(int amount) {
            if (amount > 0) {
                _coin = Math.Min(_coin + amount, _maxCoin);
                //Debug.Log($"Coin {amount}");
                OnCoinUpdate?.Invoke(_coin);
            }
        }
        private void _UpdateHealth(int amount) {
            if (amount < 0) {
                _health += amount;
                OnHealthUpdate?.Invoke(_health);
               
                if (_health <= 0)
                {
                    _animator.SetBool("isDied",true);
                    _animator.SetTrigger("Damaged");
                    StartCoroutine(_DieRoutine());
                }
                else {
                    if (_lootCoroutine != null) { StopCoroutine(_lootCoroutine); }
                    if (_damagedCoroutine != null) { StopCoroutine(_damagedCoroutine); }
                    _animator.SetTrigger("Damaged");
                    //StartCoroutine(_OnDamaged());
                }
               
            }
            
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
           if(_interactiveRange.GetInteractable()==null) return;
           var comp = (_interactiveRange.GetInteractable() as Component);
           if (comp == null) return;
            //if treasure box, startloot
            if (comp.TryGetComponent<Chest>(out var chest))
            {
                _lootCoroutine=StartCoroutine(_OnLootRountine());
                _animator.SetTrigger("Interactive");
            }
            else {
                _interactiveRange.GetInteractable()?.Interact();
                _animator.SetTrigger("Interactive");
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
            _lootCoroutine = null;
            _interactiveRange.GetInteractable()?.Interact();
        }
        private void _InterruptLootCoroutine() {
            if (_lootCoroutine == null) return;
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
        private IEnumerator _DieRoutine() {
            
            _isPlayerinControl = false;
            _isAllowToMove = false;
            _playerState = PlayerState.Die;
            _audioController.PlayerFail();
            yield return new WaitForSeconds(_dieAnimationLength);
            SceneManager.LoadScene(0);
        }
        private IEnumerator _KnockbackRoutine(Vector2 direction)
        {
            _isAllowToMove = false;      
            _rb2D.linearVelocity = Vector2.zero;

            _playerState = PlayerState.TakeDamage;
            _rb2D.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);

            Debug.Log($"Player Knock Back!{direction*_knockbackForce}");
            yield return new WaitForSeconds(_knockbackTime);

            _rb2D.linearVelocity= Vector2.zero;
            _isAllowToMove = true;
            _playerState = PlayerState.Idle;
            _knockbackRoutine = null;
        }
    }
    

    public enum MaskState { MaskA,InvalidMaskA,MaskB,MaskD,None}
    public enum PlayerState { Idle,Move,Interact,TakeDamage,Die}
}
