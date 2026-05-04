using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatVisuals : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private GameObject myVisuals;
    [SerializeField] private GameObject myAura;
    [SerializeField] private GameObject myUI;
    
    [Header("Value Info")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthText;
    [SerializeField] private Image defenseIcon;
    [SerializeField] private Image defenseFill;
    [SerializeField] private GameObject defenseText;
    [SerializeField] private TextMeshProUGUI damageText;
    
    private Animator myAnimator;
    private CombatEntity me;
    private SpriteRenderer visualsSprite;
    private SpriteRenderer auraSprite;
    private Canvas uiCanvas;
    private TextMeshProUGUI healthTMP;
    private TextMeshProUGUI defenseTMP;
    
    // Animation Consts
    private const string IS_HIT_PARAM = "HitTrigger";
    private const string IS_DEFENDED_PARAM = "DefendTrigger";
    private const string IS_HEALED_PARAM = "HealedTrigger";

    private void Awake()
    {
        // Components for animation
        myAnimator = GetComponent<Animator>();
        
        // Components for layering
        visualsSprite = myVisuals.GetComponent<SpriteRenderer>();
        //auraSprite = myAura.GetComponent<SpriteRenderer>();
        uiCanvas = myUI.GetComponent<Canvas>();
        
        // Components for HP/Defense text display
        healthTMP = healthText.GetComponent<TextMeshProUGUI>();
        defenseTMP = defenseText.GetComponent<TextMeshProUGUI>();
    }
    
    public void SetStartingValues(CombatEntity entity)
    {
        me = entity;

        healthTMP.text = me.currentHealth + " / " + me.maxHealth;
        defenseTMP.text = me.currentDefense.ToString();
        
        UpdateHealthBar();
        UpdateDefense();
    }
    
    public void UpdateHealthBar()
    {
        float hpPercent = ((float)me.currentHealth / me.maxHealth);
        
        healthTMP.text = me.currentHealth + " / " + me.maxHealth;
        
        healthBar.fillAmount = hpPercent;
        
        if (me.currentHealth <= 0) {
            //PlayDeathAnimation(); TODO add death animation
            Destroy(gameObject, 1f);
        }
    }
    
    public void UpdateDefense()
    {
        defenseTMP.text = me.currentDefense.ToString();
        defenseFill.enabled = me.currentDefense > 0;
    }
    
    public void PlayHitAnimation(int damageDealt/*, bool isCrit*/)
    {
        myAnimator.SetTrigger(IS_HIT_PARAM);
        /*if (isCrit) {
            damageText.text = "CRIT!\n" + damageDealt;
        } else {
            damageText.text = damageDealt.ToString();
        }*/
        damageText.text = damageDealt.ToString();
    }
    
    public void PlayDefendAnimation(int defenseApplied/*, bool isCrit*/)
    {
        myAnimator.SetTrigger(IS_DEFENDED_PARAM);
        /*if (isCrit) {
            damageText.text = "CRIT!\n" + restoreApplied;
        } else {
            damageText.text = restoreApplied.ToString();
        }*/
        damageText.text = defenseApplied.ToString();
    }
    
    public void PlayHealAnimation(int restoreApplied/*, bool isCrit*/)
    {
        myAnimator.SetTrigger(IS_HEALED_PARAM);
        /*if (isCrit) {
            damageText.text = "CRIT!\n" + restoreApplied;
        } else {
            damageText.text = restoreApplied.ToString();
        }*/
        damageText.text = restoreApplied.ToString();
    }
}
