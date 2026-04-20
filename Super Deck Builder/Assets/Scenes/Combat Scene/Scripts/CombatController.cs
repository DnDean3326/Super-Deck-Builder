using System;
using System.Collections.Generic;
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
    private EnemyManager enemyManager;
    private TokenManager tokenManager;
    
    // Character Specific Logic
    //private ImmuneBattleLogic immuneLogic;
    //private RicochetBattleLogic ricochetLogic;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();
        tokenManager = FindFirstObjectByType<TokenManager>();
        
        // Temp code for one-scene testing
        partyManager.GenerateTestParty();
        enemyManager.GenerateTestEncounter();
        
        InitializeBattleTokens();
        CreatePartyEntities();
        CreateEnemyEntities();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void InitializeBattleTokens()
    {
        List<Token> tokens = new List<Token>();
        tokens.AddRange(tokenManager.GetTokenInfo());

        foreach (Token token in tokens) {
            CombatToken tempToken = new CombatToken(token);
            
            allTokens.Add(tempToken);
        }
    }

    private void CreatePartyEntities()
    {
        List<ActiveCharacter> activeCharacters = new List<ActiveCharacter>();
        activeCharacters.AddRange(partyManager.GetCurrentParty());

        foreach (ActiveCharacter character in activeCharacters) {
            CombatEntity tempEntity = new CombatEntity(character);
            
            //TODO make this way less janky
            if (tempEntity.isPlayer) {
                foreach (ActiveCard card in character.currentDeck) {
                    CombatCard tempCard = new CombatCard(card);
                    playerDeck.Add(tempCard);
                }
            }
            
            allCombatants.Add(tempEntity);
            allyCombatants.Add(tempEntity);
        }
    }

    private void CreateEnemyEntities()
    {
        List<Enemy> enemies = new List<Enemy>();
        enemies.AddRange(enemyManager.GetCurrentEnemies());

        foreach (Enemy enemy in enemies) {
            CombatEntity tempEntity = new CombatEntity(enemy);
            
            allCombatants.Add(tempEntity);
            enemyCombatants.Add(tempEntity);
        }
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

    internal CombatEntity(ActiveCharacter playerCharacter)
    {
        isPlayer = true;

        name = playerCharacter.activeAlias;
        maxHealth = playerCharacter.maxHealth;
        currentHealth = playerCharacter.currentHealth;
        maxArmor = playerCharacter.maxArmor;
        currentArmor = playerCharacter.currentArmor;
        
        activeTokens = new List<CombatToken>();
    }
    
    internal CombatEntity(Enemy enemy)
    {
        isPlayer = false;
        
        name = enemy.enemyName;
        maxHealth = enemy.maxHealth;
        currentHealth = enemy.currentHealth;
        maxArmor = enemy.maxHealth;
        currentArmor = enemy.currentArmor;
        
        activeTokens = new List<CombatToken>();
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

    internal CombatToken(Token token)
    {
        tokenName = token.tokenName;
        displayName = token.displayName;
        tokenIcon = token.tokenIcon;
        tokenType = token.tokenType;
        tokenValue = token.tokenValue;
        tokenCap = token.tokenCap;
        tokenCount = 0;
        tokenInverses.AddRange(tokenInverses);
        
        tokenDescription = token.tokenDescription;
    }
}

[Serializable]
public class CombatCard
{
    public string cardName;
    public Sprite cardArt;
    public CardTypes cardType;
    public int energyCost;
    public int repeatUses;

    public List<CardValueInfo> cardValues = new List<CardValueInfo>();
    public List<ExtraCostInfo> extraCosts = new List<ExtraCostInfo>();
    public List<TokenAppliedInfo> selfTokensApplied = new List<TokenAppliedInfo>();
    public List<TokenAppliedInfo> targetTokensApplied = new List<TokenAppliedInfo>();
    
    public string cardDescription;

    internal CombatCard(ActiveCard activeCard)
    {
        cardName = activeCard.cardName;
        cardArt = activeCard.cardArt;
        cardType = activeCard.cardType;
        energyCost = activeCard.energyCost;
        repeatUses = activeCard.repeatUses;
        cardDescription = activeCard.cardDescription;
        
        if (activeCard.cardValues != null) {
            cardValues.AddRange(activeCard.cardValues);
        }
        if (activeCard.extraCosts != null) {
            extraCosts.AddRange(activeCard.extraCosts);
        }
        if (activeCard.selfTokensApplied != null) {
            selfTokensApplied.AddRange(activeCard.selfTokensApplied);
        }
        if (activeCard.targetTokensApplied != null) {
            targetTokensApplied.AddRange(activeCard.targetTokensApplied);
        }
    }
}
