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
    [SerializeField] private List<PlayerDeck> playerDecks = new List<PlayerDeck>();

    [Header("Combatants")] 
    [SerializeField] private List<CombatEntity> allCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> allyCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> enemyCombatants = new List<CombatEntity>();
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

    private const int BASE_CARD_DRAW = 5; 

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
        CreatePlayerDecks();
    }
    
    void Start()
    {
        FirstDraw();
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
            
            //TODO Spawn character visuals
            
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
            
            //TODO Spawn character visuals
            
            allCombatants.Add(tempEntity);
            enemyCombatants.Add(tempEntity);
        }
    }

    private void CreatePlayerDecks()
    {
        for (int i = 0; i < allyCombatants.Count; i++) {
            var ally = allyCombatants[i];
            
            GameObject tempDeckHolder = new GameObject { name = "Player " + (i + 1) + " Deck" };
            tempDeckHolder.transform.SetParent(this.transform);

            var tempDraw = CreatePlayerDraw(ally, tempDeckHolder);
            var tempHand = CreatePlayerHand(ally, tempDeckHolder);
            var tempDiscard = CreatePlayerDiscard(ally, tempDeckHolder);
            var tempExile = CreatePlayerExile(ally, tempDeckHolder);

            PlayerDeck tempDeck = new PlayerDeck(tempDraw, tempHand, tempDiscard, tempExile);
            tempDeck.ReshuffleDeck();
            ally.LinkDeck(tempDeck);
            playerDecks.Add(tempDeck);
        }
    }
    
    private PlayerDraw CreatePlayerDraw(CombatEntity ally, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerDraw>();
        PlayerDraw drawScript = deckHolder.GetComponent<PlayerDraw>();
        drawScript.SetMyPlayer(ally);
        
        drawScript.InitializeDeck(ally.myCards);
        
        return drawScript;
    }

    private PlayerHand CreatePlayerHand(CombatEntity ally, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerHand>();
        PlayerHand handScript = deckHolder.GetComponent<PlayerHand>();
        handScript.SetMyPlayer(ally);
        
        return handScript;
    }

    private PlayerDiscard CreatePlayerDiscard(CombatEntity ally, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerDiscard>();
        PlayerDiscard discardScript = deckHolder.GetComponent<PlayerDiscard>();
        discardScript.SetMyPlayer(ally);

        return discardScript;
    }

    private PlayerExile CreatePlayerExile(CombatEntity ally, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerExile>();
        PlayerExile exileScript = deckHolder.GetComponent<PlayerExile>();
        exileScript.SetMyPlayer(ally);
        
        return exileScript;
    }

    private void FirstDraw()
    {
        foreach (var ally in allyCombatants) {
            if (ally.isPlayer) {
                ally.myDeck.DrawCards(BASE_CARD_DRAW);
            }
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

    public PlayerDeck myDeck;
    
    public List<CombatToken> activeTokens = new List<CombatToken>();
    public List<CombatCard> myCards = new List<CombatCard>();

    internal CombatEntity(ActiveCharacter playerCharacter)
    {
        isPlayer = true;

        name = playerCharacter.activeAlias;
        maxHealth = playerCharacter.maxHealth;
        currentHealth = playerCharacter.currentHealth;
        maxArmor = playerCharacter.maxArmor;
        currentArmor = playerCharacter.currentArmor;

        foreach (var card in playerCharacter.currentDeck) {
            CombatCard tempCard = new CombatCard(card);
            myCards.Add(tempCard);
        }
    }
    
    internal CombatEntity(Enemy enemy)
    {
        isPlayer = false;
        
        name = enemy.enemyName;
        maxHealth = enemy.maxHealth;
        currentHealth = enemy.currentHealth;
        maxArmor = enemy.maxHealth;
        currentArmor = enemy.currentArmor;
    }

    internal void LinkDeck(PlayerDeck deck)
    {
        myDeck = deck;
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
    
    [FormerlySerializedAs("cardPrefab")] public GameObject cardVisuals;

    internal CombatCard(ActiveCard activeCard)
    {
        cardName = activeCard.cardName;
        cardArt = activeCard.cardArt;
        cardType = activeCard.cardType;
        energyCost = activeCard.energyCost;
        repeatUses = activeCard.repeatUses;
        cardDescription = activeCard.cardDescription;
        cardVisuals = activeCard.cardPrefab;
        
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

[Serializable]
public class PlayerDeck
{
    public PlayerDraw draw;
    public PlayerHand hand;
    public PlayerDiscard discard;
    public PlayerExile exile;

    internal PlayerDeck(PlayerDraw playerDraw, PlayerHand playerHand, PlayerDiscard playerDiscard, PlayerExile playerExile)
    {
        draw = playerDraw;
        hand = playerHand;
        discard = playerDiscard;
        exile = playerExile;
    }

    public void ReshuffleDeck()
    {
        var discardedCards = discard.RetrieveCardsInDiscard();
        draw.AddCardsToDraw(discardedCards);
        draw.ShuffleDeck();
    }

    public void DrawCards(int drawCount)
    {
        for (int i = 0; i < drawCount; i++) {
            if (draw.GetDrawCount() == 0) {
                ReshuffleDeck();
            }
            CombatCard drawnCard = draw.DrawTopCard();
            hand.AddCardToHand(drawnCard);
        }
    }
}
