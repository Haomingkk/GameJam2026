using GameJam2026.GamePlay;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Player Status")]
    [SerializeField] private Image _energyBarMask;
    [SerializeField] private List<Image> _healthImages = new List<Image>();
    [SerializeField] private List<Image> _maskImages = new List<Image>();

    private float _maxBarWidth;
    private void Awake()
    {

        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
        _maxBarWidth = _energyBarMask.rectTransform.rect.width;
    }
    private void Start()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnEnergyUpdate += _UpDateUIEnegryBar;
            PlayerController.instance.OnHealthUpdate += _UpdateUIHealth;
        }
    }
    private void OnEnable()
    {
        PlayerController.instance.OnEnergyUpdate += _UpDateUIEnegryBar;
        PlayerController.instance.OnHealthUpdate += _UpdateUIHealth;
    }
    private void OnDisable()
    {
        if (PlayerController.instance != null)
            PlayerController.instance.OnEnergyUpdate -= _UpDateUIEnegryBar;
            PlayerController.instance.OnHealthUpdate -= _UpdateUIHealth;
    }
    private void _UpDateUIEnegryBar(float energyRatio)
    {
  
        RectTransform rt = _energyBarMask.rectTransform;

        float width = energyRatio * _maxBarWidth;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }
    private void _UpdateUIHealth(int health) {
        
        for (int i = 0; i < _healthImages.Count-health; i++) {
            _healthImages[i].enabled = true;
        }
        for (int i = health; i < _healthImages.Count; i++) {
            _healthImages[i].enabled = false;
        }
    
    }

}
