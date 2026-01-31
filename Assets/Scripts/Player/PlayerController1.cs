using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [Header("Player Status")]
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _maxEnergy = 10f;
        [SerializeField] private int _maxCoin = 99999;
        private int _health;
        private PlayerState _playerState;
        private int _coin;

        [Header("Mask Status")]
        [SerializeField] private float _energyComsumeSpeed=0.1f;
        [SerializeField] private float _energyGatheringSpeed = 0.2f;
        [SerializeField] private float _coinMaskDGatherTime = 1.0f;
        private float _coinMaskDGatherTimer;
        private bool _isGartheringEnergy;
        public MaskState maskState { get; private set; }
       

        private float _energy;
        private bool _isPlayerinControl=true;// change with game mode, may change while we have game mode script
        private bool _isAllowToMove=true;

        private Vector2 _moveInput;

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
            _HandleMaskInput();
            _HandleMaskAStates();
            _UpdateMaskEnergy();
        }
        private void FixedUpdate()
        {
            _PlayerMovement();
        }
        #endregion
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
       
        private void _InitPlayerStatus() {
            _health = _maxHealth;
            _energy = _maxEnergy;
            _playerState = PlayerState.Idle;
            maskState = MaskState.None;
            
            _isPlayerinControl = true;
            _isAllowToMove = true;

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
        private void _HandleMaskInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskA);

            if (kb.digit2Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskB);

            if (kb.digit3Key.wasPressedThisFrame)
                _ToggleMask(MaskState.MaskD);
        }
        private void _ToggleMask(MaskState target)
        {
            if (maskState == target)
            { maskState = MaskState.None; }
            else if ((target == MaskState.InvalidMaskA && maskState == MaskState.MaskA) || (target == MaskState.MaskA && maskState == MaskState.InvalidMaskA))
            {
                maskState = MaskState.None;
            }
            else if(_energy>0)
            { maskState = target; }
            Debug.Log($"Player Switch to mask {maskState} now!");
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
        private void _UpdateMaskEnergy() {
            if (maskState != MaskState.None && maskState != MaskState.MaskD)
            {
                _energy -= _energyComsumeSpeed * Time.deltaTime;
                if (_energy <= 0) { _energy = 0;_ToggleMask(maskState); }
                OnEnergyUpdate?.Invoke();
            }
            else if (maskState == MaskState.MaskD) {
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
        }
        private void _UpdateCoin(int amount) {
            if (amount > 0) {
                _coin = Math.Min(_coin + amount, _maxCoin);
                OnCoinUpdate?.Invoke();
            }
        }
    }
    

    public enum MaskState { MaskA,InvalidMaskA,MaskB,MaskD,None}
    public enum PlayerState { Idle,Move,Interact,TakeDamage,Die}
}
