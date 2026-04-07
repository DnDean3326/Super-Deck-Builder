using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Serialization;

public class CombatController : MonoBehaviour
{
    private enum CombatState
    {
        Start,
        Combat,
        PlayerTurn,
        EnemyTurn,
        Targeting, // Unsure if this one will be needed with the simpler targeting system
        TurnEnd,
        Victory,
        Defeat
    }
    
    [Header("Battle System")]
    [SerializeField] private CombatState state;

    [Header("Player Deck")]
    [SerializeField] private List<CombatCard> playerDeck = new List<CombatCard>();

    [Header("Combatants")] 
    [SerializeField] private List<CombatEntity> allCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> enemyCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> allyCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> turnOrder = new  List<CombatEntity>(); //Unsure if this will be needed
    [SerializeField] private List<CombatEntity> targetList = new List<CombatEntity>();
    
    [Header("Tokens")]
    [SerializeField] private List<CombatToken> allTokens = new List<CombatToken>();
    
    private PartyManager partyManager;
    //private EnemyManager enemyManager;
    //private TokenManager tokenManager;
    
    // Character Specific Logic
    //private ImmuneBattleLogic immuneLogic;
    //private RicochetBattleLogic ricochetLogic;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class CombatEntity
{
    public string name;
    public Sprite portrait;
    public bool isPlayer;
    public bool myFirstTurn = true;
    public bool wasDamagedLastTurn = false;
    public int damagedBy;
    
    public string activeAbilityType;
    public int target;
    //public Ability activeAbility;
    
    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    public int maxDefense = 99999;
    public int currentDefense;
    
    public List<CombatToken> activeTokens;

    CombatEntity(PlayerInfo playerInfo)
    {
        isPlayer = true;

        
    }
    
    CombatEntity(EnemyInfo enemyInfo)
    {
        isPlayer = false;
        
        name = enemyInfo.enemyName;
        maxHealth = enemyInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = enemyInfo.baseArmor;
        currentArmor = maxArmor;
        
        activeTokens = new List<CombatToken>();
    }
}

[Serializable]
public class CombatCard
{
    public string cardName;
    public Sprite cardArt;
    public CardTypes cardType;
    public int energyCost;
    public int cardValue;
    public int repeatUses;

    public bool hasExtraCost;
    public List<ExtraCostInfo> extraCostInfos = new List<ExtraCostInfo>();
    
    public string cardDescription;

    CombatCard(ActiveCard activeCard)
    {
        cardName = activeCard.cardName;
        cardArt = activeCard.cardArt;
        cardType = activeCard.cardType;
        energyCost = activeCard.energyCost;
        repeatUses = activeCard.repeatUses;
        cardDescription = activeCard.cardDescription;
        hasExtraCost = activeCard.hasExtraCost;
        if (hasExtraCost) {
            extraCostInfos.AddRange(activeCard.extraCostInfos);
        }
        cardDescription = activeCard.cardDescription;
    }
}

[Serializable]
public class CombatToken
{
    public string tokenName;
    public string displayName;
    public Sprite tokenIcon;
    public Token.TokenType tokenType;
    public float tokenValue;
    public int tokenCap;
    public int tokenCount;
    
    public List<String> tokenInverses = new List<String>();

    public string tokenDescription;

    CombatToken(string storedName, string storedDisplayName, Sprite storedIcon, Token.TokenType storedType,
        float storedValue, int storedCap, List<String> storedInverses, string storedDescription)
    {
        tokenName = storedName;
        displayName = storedDisplayName;
        tokenIcon = storedIcon;
        tokenType = storedType;
        tokenValue = storedValue;
        tokenCap = storedCap;
        tokenCount = 0;
        tokenInverses = storedInverses;

        tokenDescription = storedDescription;
    }
}
