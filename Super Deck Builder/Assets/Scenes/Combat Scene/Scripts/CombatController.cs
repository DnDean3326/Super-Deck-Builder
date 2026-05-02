using System;
using System.Collections;
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
        AllyTurn,
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
    [SerializeField] private List<CombatEntity> playerCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> allyCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> enemyCombatants = new List<CombatEntity>();
    [SerializeField] private List<CombatEntity> turnOrder = new  List<CombatEntity>();
    [SerializeField] private List<CombatEntity> targetList = new List<CombatEntity>();
    [SerializeField] private CombatEntity activeEntity = null;
    
    [Header("Tokens")]
    [SerializeField] private List<CombatToken> allTokens = new List<CombatToken>();
    
    // Turn pause consts
    private const float TURN_ACTION_DELAY = 0.8f;
    private const float END_DELAY = 2.5f;

    // Script Variables
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
        
        // Temp code for one-scene testing TODO remove when greater framework is created
        partyManager.GenerateTestParty();
        enemyManager.GenerateTestEncounter();
        
        InitializeBattleTokens();
        CreatePartyEntities();
        CreateEnemyEntities();
        CreatePlayerDecks();
        SetTurnOrder();
    }
    
    void Start()
    {
        StartCoroutine(StartRoutine());
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
            if (tempEntity.isPlayer) {
                playerCombatants.Add(tempEntity);
            }
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
        for (int i = 0; i < playerCombatants.Count; i++) {
            var player = playerCombatants[i];
            
            GameObject tempDeckHolder = new GameObject { name = "Player " + (i + 1) + " Deck" };
            tempDeckHolder.transform.SetParent(this.transform);

            var tempDraw = CreatePlayerDraw(player, tempDeckHolder);
            var tempHand = CreatePlayerHand(player, tempDeckHolder);
            var tempDiscard = CreatePlayerDiscard(player, tempDeckHolder);
            var tempExile = CreatePlayerExile(player, tempDeckHolder);

            PlayerDeck tempDeck = new PlayerDeck(tempDraw, tempHand, tempDiscard, tempExile);
            tempDeck.ReshuffleDeck();
            player.LinkDeck(tempDeck);
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

    private void SetTurnOrder()
    {
        turnOrder.AddRange(playerCombatants);
        foreach (var ally in allyCombatants) {
            if (turnOrder.Contains(ally)) { return; }
            turnOrder.Add(ally);
        }
        turnOrder.AddRange(enemyCombatants);
    }

    private void FirstDraw()
    {
        foreach (var player in playerCombatants) {
            player.myDeck.DrawCards(BASE_CARD_DRAW);
        }
    }

    private IEnumerator StartRoutine()
    {
        if (state == CombatState.Start) {
            // Eventually combatant initialization info will go here
            FirstDraw();
            
            state = CombatState.Combat;
            StartCoroutine(CombatRoutine());
            yield break;
        } else {
            print("Start Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator CombatRoutine()
    {
        if (state == CombatState.Combat) {
            // Remove any dead combatants from the combat
            FixResources();
            RemoveDeadCombatants();
            
            // If no party members remain -> battle is lost
            if (playerCombatants.Count <= 0) {
                state = CombatState.Defeat;
                yield return new WaitForSeconds(TURN_ACTION_DELAY);  // wait a few seconds
                print("GAME OVER! \n Go to game over screen.");
            }
            // If no enemies remain -> battle is won
            if (enemyCombatants.Count <= 0) {
                state = CombatState.Victory;
                yield return new WaitForSeconds(END_DELAY);  // wait a few seconds
                print("You win! \n Restart to play again.");
            }

            // Determine which entity acts next
            if (activeEntity == null) {
                activeEntity = turnOrder[0];
            } else {
                int activePosition = turnOrder.IndexOf(activeEntity) + 1;
                if (activePosition >= turnOrder.Count) {
                    activePosition = 0;
                }
                activeEntity = turnOrder[activePosition];
            }

            // Calls the appropriate turn routine
            if (allyCombatants.Contains(activeEntity)) {
                if (activeEntity.isPlayer) {
                    state = CombatState.PlayerTurn;
                    StartCoroutine(PlayerTurnRoutine());
                } else {
                    state = CombatState.AllyTurn;
                    //StartCoroutine(AllyTurnRoutine());
                }
            } else {
                state = CombatState.EnemyTurn;
                //StartCoroutine(EnemyTurnRoutine());
            }
        } else {
            print("Combat Routine called but the battle system is in the " + state + " state.");
        }
    }

    private IEnumerator PlayerTurnRoutine()
    {
        if (state == CombatState.PlayerTurn) {
            
        } else {
            print("Player Turn Routine called but the battle system is in the " + state + " state.");
            yield break;
        }
    }

    private void RemoveDeadCombatants()
    {
        for (int i = 0; allCombatants.Count > i; i++) {
            CombatEntity deadCombatant = null;
            if (allCombatants[i].currentHealth <= 0) {
                deadCombatant = allCombatants[i];
                if (!deadCombatant.isPlayer) {
                    int enemyIndex = enemyCombatants.IndexOf(deadCombatant);
                    //encounterFunds += enemyFunds[enemyIndex];
                }
                allCombatants.RemoveAt(i);
            }
            if (deadCombatant != null) {
                //yield return new WaitForSeconds(DEATH_DELAY); TODO Add some kind of delay here
            }
        }
    }

    private void FixResources()
    {
        foreach (var t in allCombatants) {
            if (t.currentHealth < 0) {
                t.currentHealth = 0;
            } else if (t.currentHealth > t.maxHealth) {
                t.currentHealth = t.maxHealth;
            }
            if (t.currentDefense < 0) {
                t.currentDefense = 0;
            } else if (t.currentDefense > t.maxDefense) {
                t.currentDefense = t.maxDefense;
            }
            if (t.currentArmor < 0) {
                t.currentArmor = 0;
            } else if (t.currentArmor > t.maxArmor) {
                t.currentArmor = t.maxArmor;
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
    
    public List<string> tokenInverses = new List<string>();

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
    public CardTargeting cardTargeting;

    public List<CardValueInfo> cardValues = new List<CardValueInfo>();
    public List<ExtraCostInfo> extraCosts = new List<ExtraCostInfo>();
    public List<TokenAppliedInfo> selfTokensApplied = new List<TokenAppliedInfo>();
    public List<TokenAppliedInfo> targetTokensApplied = new List<TokenAppliedInfo>();
    
    public string cardDescription;
    
    public GameObject cardVisuals;
    public CardController myCardController;

    internal CombatCard(ActiveCard activeCard)
    {
        cardName = activeCard.cardName;
        cardArt = activeCard.cardArt;
        cardType = activeCard.cardType;
        energyCost = activeCard.energyCost;
        repeatUses = activeCard.repeatUses;
        cardTargeting = activeCard.cardTargeting;
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
        
        myCardController = cardVisuals.GetComponent<CardController>();
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
