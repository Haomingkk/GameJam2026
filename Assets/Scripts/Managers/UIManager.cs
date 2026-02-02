using GameJam2026.GamePlay;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam2026.UI {
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance { get; private set; }

        [Header("Player Status")]
        [SerializeField] private Image _energyBarMask;
        [SerializeField] private List<Image> _healthImages = new List<Image>();
        [SerializeField] private List<Image> _maskImages = new List<Image>();
        [SerializeField] private List<Image> _coinNumbers = new List<Image>();
        [SerializeField] private List<Sprite> _digitSprite = new List<Sprite>();

        [Header("UI Effect")]
        [SerializeField] Image _scareFaceUI;
        [SerializeField] private float _scareFaceTime = 0.5f; 

        private float _maxBarWidth;
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            _maxBarWidth = _energyBarMask.rectTransform.rect.width;

        }
        private void Start()
        {
            if (PlayerController.instance != null)
            {
                PlayerController.instance.OnEnergyUpdate += _UpDateUIEnegryBar;
                PlayerController.instance.OnHealthUpdate += _UpdateUIHealth;
                PlayerController.instance.OnMaskStateUpdate += _UpdateMask;
                PlayerController.instance.OnCoinUpdate += _UpdateUICoin;
            }
           
        }
        
        private void OnEnable()
        {
            EventHandler.InstantiateMonsterFace += _ShowScareFace;
        }
        private void OnDisable()
        {
            if (PlayerController.instance != null)
            {
                PlayerController.instance.OnEnergyUpdate -= _UpDateUIEnegryBar;
                PlayerController.instance.OnHealthUpdate -= _UpdateUIHealth;
                PlayerController.instance.OnMaskStateUpdate -= _UpdateMask;
                PlayerController.instance.OnCoinUpdate -= _UpdateUICoin;
                EventHandler.InstantiateMonsterFace -= _ShowScareFace;
            }
        }
        private void _ShowScareFace(Sprite mask) {
            StartCoroutine(_ScareFaceRoutine(mask));
        
        }
        private void _UpdateMask( ) {
            MaskState m = PlayerController.instance.GetCurrentMaskState();
            for (int i = 0; i < _maskImages.Count; i++) {
                _maskImages[i].color = new Color(0.5f,0.5f,0.5f);
            }
            if (m == MaskState.MaskA || m == MaskState.InvalidMaskA) {
                _maskImages[0].color = Color.white;
            }
            if (m == MaskState.MaskB) {
                _maskImages[1].color = Color.white;
            }
            if (m == MaskState.MaskD) {
                _maskImages[2].color = Color.white;
            }
        
        }
        
  
        private void _UpDateUIEnegryBar(float energyRatio)
        {

            RectTransform rt = _energyBarMask.rectTransform;

            float width = energyRatio * _maxBarWidth;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
        private void _UpdateUIHealth(int health) {

            for (int i = 0; i < _healthImages.Count - health; i++) {
                _healthImages[i].enabled = true;
            }
            for (int i = health; i < _healthImages.Count; i++) {
                _healthImages[i].enabled = false;
            }

        }
        private void _UpdateUICoin(int coins) {
            coins = Mathf.Clamp(coins, 0, 9999);

            int thousands = coins / 1000;
            int hundreds = (coins / 100) % 10;
            int tens = (coins / 10) % 10;
            int ones = coins % 10;

            int[] d = { thousands, hundreds, tens, ones };

            for (int i = 0; i < 4; i++)
            {
                _coinNumbers[i].sprite = _digitSprite[d[i]];
                _coinNumbers[i].enabled = true;
            }
            }
        private IEnumerator _ScareFaceRoutine(Sprite mask) {
            _scareFaceUI.sprite = mask;
            _scareFaceUI.color = Color.white;
            yield return new WaitForSeconds(_scareFaceTime);
            _scareFaceUI.sprite = null;
            _scareFaceUI.color = Color.clear;
        }

    }
    }
