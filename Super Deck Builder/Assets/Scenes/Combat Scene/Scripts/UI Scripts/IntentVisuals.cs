using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntentVisuals : MonoBehaviour
{
    [SerializeField] private Sprite attackSprite;
    [SerializeField] private Sprite defendSprite;
    [SerializeField] private Sprite buffSprite;
    [SerializeField] private Sprite debuffSprite;
    
    [SerializeField] private GameObject intentObject;
    private Image intentImage;
    private TextMeshProUGUI intentText;

    private void Awake()
    {
        intentImage = intentObject.GetComponentInChildren<Image>();
        intentText = intentObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        intentObject.SetActive(false);
    }

    public void DisplayIntent(EnemyIntents intent, int value)
    {
        intentObject.SetActive(true);
        Sprite intentSprite;
        switch (intent) {
            case EnemyIntents.Attack:
                intentSprite = attackSprite;
                break;
            case EnemyIntents.Defend:
                intentSprite = defendSprite;
                break;
            case EnemyIntents.Buff:
                intentSprite = buffSprite;
                break;
            case EnemyIntents.Debuff:
                intentSprite = debuffSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
        }
        
        intentImage.sprite = intentSprite;

        if (value != 0) {
            intentText.gameObject.SetActive(true);
            intentText.text = value.ToString();
        } else {
            intentText.gameObject.SetActive(false);
        }
    }
    
    public void DisplayIntent(EnemyIntents intent, int minValue, int maxValue)
    {
        intentObject.SetActive(true);
        Sprite intentSprite;
        switch (intent) {
            case EnemyIntents.Attack:
                intentSprite = attackSprite;
                break;
            case EnemyIntents.Defend:
                intentSprite = defendSprite;
                break;
            case EnemyIntents.Buff:
                intentSprite = buffSprite;
                break;
            case EnemyIntents.Debuff:
                intentSprite = debuffSprite;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(intent), intent, null);
        }
        
        intentImage.sprite = intentSprite;

        if (minValue != 0 && maxValue != 0) {
            intentText.gameObject.SetActive(true);
            intentText.text = minValue + "-" + maxValue;
        } else {
            intentText.gameObject.SetActive(false);
        }
        
    }
    
    public void HideIntent()
    {
        intentObject.SetActive(false);
    }
}