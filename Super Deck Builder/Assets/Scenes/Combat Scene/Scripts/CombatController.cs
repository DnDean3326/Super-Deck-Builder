using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombatController : MonoBehaviour
{
    private enum CombatState
    {
        Start,
        Combat,
        PlayerTurn,
        NpcTurn,
        Targeting, // Unsure if this one will be needed with the simpler targeting system
        TurnEnd,
        Victory,
        Defeat
    }
    
    [Header("Battle System")]
    [SerializeField] private CombatState state;

    [Header("Player Decks")]
    [SerializeField] private List<PlayerDeck> playerDecks = new List<PlayerDeck>();

    [Header("Combatants")] 
    [SerializeField] private List<CombatEntity> allCombatants = new List<CombatEntity>();
    [SerializeField] private List<PlayerEntity> playerCombatants = new List<PlayerEntity>();
    [SerializeField] private List<CombatEntity> allyCombatants = new List<CombatEntity>();
    [SerializeField] private List<NpcEntity> enemyCombatants = new List<NpcEntity>();
    [SerializeField] private List<CombatEntity> turnOrder = new  List<CombatEntity>();
    [SerializeField] private List<CombatEntity> targetList = new List<CombatEntity>();
    [SerializeField] private CombatEntity activeEntity;
    
    [Header("Tokens")]
    [SerializeField] private List<CombatToken> allTokens = new List<CombatToken>();
    
    [Header("UI Objects")]
    //
    
    // Turn pause consts
    private const float TURN_ACTION_DELAY = 0.8f;
    private const float END_DELAY = 2.5f;
    
    // Consts
    private const int BASE_CARD_DRAW = 5; 

    // Script Variables
    private PartyManager partyManager;
    private EnemyManager enemyManager;
    private TokenManager tokenManager;
    private EnergyDisplayUI energyDisplayUI;
    
    // Character Specific Logic
    //private ImmuneBattleLogic immuneLogic;
    //private RicochetBattleLogic ricochetLogic;

    // Game Tracking Vars
    private bool firstRound = true;
    private bool canSelectCards = false;
    private bool cardSelected = false;
    private bool targetSelected = false;

    private void Awake()
    {
        partyManager = FindFirstObjectByType<PartyManager>();
        enemyManager = FindFirstObjectByType<EnemyManager>();
        tokenManager = FindFirstObjectByType<TokenManager>();
        energyDisplayUI = FindAnyObjectByType<EnergyDisplayUI>();
        
        // Temp code for one-scene testing TODO remove when greater framework is created
        partyManager.GenerateTestParty();
        enemyManager.GenerateTestEncounter();
        
        InitializeBattleTokens();
        CreatePartyEntities();
        CreateEnemyEntities();
        CreatePlayerDecks();
        SetTurnOrder();
    }
    
    private void Start()
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
            PlayerEntity tempEntity = new PlayerEntity(character, BASE_CARD_DRAW);
            
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
            NpcEntity tempEntity = new NpcEntity(enemy);
            
            //TODO Spawn character visuals
            
            //TODO check if other enemies share a name in the combat and append a number to the end of their name
            
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
    
    private PlayerDraw CreatePlayerDraw(PlayerEntity player, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerDraw>();
        PlayerDraw drawScript = deckHolder.GetComponent<PlayerDraw>();
        drawScript.SetMyPlayer(player);
        
        drawScript.InitializeDeck(player.myCards);
        
        return drawScript;
    }

    private PlayerHand CreatePlayerHand(PlayerEntity player, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerHand>();
        PlayerHand handScript = deckHolder.GetComponent<PlayerHand>();
        handScript.SetMyPlayer(player);
        
        return handScript;
    }

    private PlayerDiscard CreatePlayerDiscard(PlayerEntity player, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerDiscard>();
        PlayerDiscard discardScript = deckHolder.GetComponent<PlayerDiscard>();
        discardScript.SetMyPlayer(player);

        return discardScript;
    }

    private PlayerExile CreatePlayerExile(PlayerEntity player, GameObject deckHolder)
    {
        deckHolder.AddComponent<PlayerExile>();
        PlayerExile exileScript = deckHolder.GetComponent<PlayerExile>();
        exileScript.SetMyPlayer(player);
        
        return exileScript;
    }

    private void SetTurnOrder()
    {
        foreach (var player in playerCombatants) {
            turnOrder.Add(allCombatants.Find(t => t.name == player.name));
        }
        foreach (var ally in allyCombatants) {
            if (ally.isPlayer) { continue; }
            turnOrder.Add(ally);
        }
        foreach (var enemy in enemyCombatants) {
            turnOrder.Add(allCombatants.Find(t => t.name == enemy.name));
        }
    }

    private IEnumerator StartRoutine()
    {
        print("Start Routine");
        
        if (state == CombatState.Start) {
            // Eventually combatant initialization info will go here
            state = CombatState.Combat;
            StartCoroutine(CombatRoutine());
            yield return new WaitForSeconds(1f);
        } else {
            print("Start Routine called but the combat system is in the " + state + " state.");
        }
    }

    private IEnumerator CombatRoutine()
    {
        print("Combat Routine");
        
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
                print("active Entity is not null");
                int activePosition = turnOrder.IndexOf(activeEntity) + 1;
                if (activePosition >= turnOrder.Count) {
                    activePosition = 0;
                }
                activeEntity = turnOrder[activePosition];
            }
            print(activeEntity.name);
            yield return new WaitForSeconds(1f);

            // Calls the appropriate turn routine
            if (allyCombatants.Any(t => t.name == activeEntity.name)) {
                if (playerCombatants.Any(t => t.name == activeEntity.name)) {
                    state = CombatState.PlayerTurn;
                    StartCoroutine(PlayerTurnRoutine());
                    yield break;
                } else {
                    state = CombatState.NpcTurn;
                    StartCoroutine(NpcTurnRoutine()); //TODO add ally functionality
                    yield break;
                }
            } else {
                state = CombatState.NpcTurn;
                print("Enemy turn reached");
                StartCoroutine(NpcTurnRoutine());
                yield break;
            }
        } else {
            print("Combat Routine called but the battle combat is in the " + state + " state.");
        }
    }

    private IEnumerator PlayerTurnRoutine()
    {
        print("Player Turn Routine");
        
        if (state == CombatState.PlayerTurn) {
            PlayerEntity activePlayer = playerCombatants.Find(t => t.name == activeEntity.name);
            activePlayer.DisplayCharacterEnergy(energyDisplayUI);
            
            activePlayer.DrawHand();
            cardSelected = false;
            
            while (activePlayer.currentEnergy > 0) {
                canSelectCards = true;
                yield return new WaitUntil(() => cardSelected);
                canSelectCards = false;

                state = CombatState.Targeting;
                //TODO eventually make this a variable so the active routine can be canceled for the purposes of changing selection
                yield return StartCoroutine(TargetingRoutine());
                cardSelected = false;
            }
            activePlayer.DiscardHand();
            activePlayer.HideEnergyUI(energyDisplayUI);
            state = CombatState.Combat;
            StartCoroutine(CombatRoutine());
            yield return new WaitForSeconds(1f);
        } else {
            print("Player Turn Routine called but the combat system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator TargetingRoutine()
    {
        print("Target Routine");
        
        if (state == CombatState.Targeting) {
            PlayerEntity activePlayer = playerCombatants.Find(t => t.name == activeEntity.name);
            CombatEntity targetEntity = null;
            CombatCard selectedCard = activePlayer.selectedCard;
            
            TargetClicked(0, selectedCard.cardType == CardTypes.Attack); //TODO remove this code below and add proper targeting
            yield return new WaitUntil(() => targetSelected);
            
            targetEntity = activePlayer.selectedTarget;
            activePlayer.UpdateEnergyUI(energyDisplayUI, selectedCard.energyCost);
            activePlayer.myDeck.hand.RemoveCardFromHand(selectedCard, false);

            switch (selectedCard.cardType) {
                case CardTypes.Attack:
                    yield return StartCoroutine(AttackCardRoutine(activePlayer, targetEntity));
                    
                    break;
                case CardTypes.Skill:
                    yield return StartCoroutine(SkillCardRoutine(activePlayer, targetEntity));
                    
                    break;
                case CardTypes.Power:
                    //TODO Add Power routine
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            targetSelected = false;
            state = CombatState.PlayerTurn;
            yield return new WaitForSeconds(1f);
        } else {
            print("Targeting Routine called but the combat system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator NpcTurnRoutine()
    {
        print("NPC Turn Routine");

        if (state == CombatState.NpcTurn) {
            NpcEntity activeEnemy = enemyCombatants.Find(t => t.name == activeEntity.name);
            NpcAbility activeAbility = activeEnemy.RetrieveAbility();
            CombatEntity target;

            if (activeAbility.targetGroup == NpcBrain.TargetFaction.Party) {
                target = allyCombatants[Random.Range(0, allyCombatants.Count)];
            } else {
                target = enemyCombatants[Random.Range(0, enemyCombatants.Count)];
            }

            if (activeAbility.relevantIntents.Any(t => t == EnemyIntents.Attack)) {
                yield return StartCoroutine(NpcAttackRoutine(activeAbility, activeEnemy, target));
            } else if (activeAbility.relevantIntents.Any(t => t == EnemyIntents.Defend)) {
                yield return StartCoroutine(NpcDefendRoutine(activeAbility, activeEnemy, target));
            } else {
                //yield return StartCoroutine();
            }

            state = CombatState.Combat;
            StartCoroutine(CombatRoutine());
            yield return new WaitForSeconds(1f);
        } else {
            print("NPC Turn Routine called but the combat system is in the " + state + " state.");
            yield break;
        }
    }

    private IEnumerator NpcAttackRoutine(NpcAbility activeAbility, NpcEntity user, CombatEntity target)
    {
        print(user.name + " used " + activeAbility.abilityName + " against " + target.name);
        
        // Declare damage values
        int damageModifier = 0;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        bool isLethal = false;
        
        // Set Min and Max damage values 
        for (int i = 0; i < activeAbility.abilityValues.Count; i++) {
            if (activeAbility.abilityValues[i].cardValue == CardValue.Damage) {
                minDamageRange = activeAbility.abilityValues[i].minValue + damageModifier;
                maxDamageRange = activeAbility.abilityValues[i].maxValue + damageModifier;
            }
        }

        // Determine damage amount
        int damage = Random.Range(minDamageRange, maxDamageRange+1);
        
        // Declare defense values
        int defenseModifier = 0;
        int minDefenseRange = 0;
        int maxDefenseRange = 0;
        
        // Set Min and Max defense values 
        for (int i = 0; i < activeAbility.abilityValues.Count; i++) {
            if (activeAbility.abilityValues[i].cardValue == CardValue.Defense) {
                minDefenseRange = activeAbility.abilityValues[i].minValue + defenseModifier;
                maxDefenseRange = activeAbility.abilityValues[i].maxValue + defenseModifier;
            }
        }

        // Determine defense amount
        int defense = Random.Range(minDefenseRange, maxDefenseRange+1);

        // Declare heal values
        int healModifier = 0;
        int minHealRange = 0;
        int maxHealRange = 0;
        
        // Set Min and Max heal values 
        for (int i = 0; i < activeAbility.abilityValues.Count; i++) {
            if (activeAbility.abilityValues[i].cardValue == CardValue.Heal) {
                minHealRange = activeAbility.abilityValues[i].minValue + healModifier;
                maxHealRange = activeAbility.abilityValues[i].maxValue + healModifier;
            }
        }

        // Determine heal amount
        int heal = Random.Range(minHealRange, maxHealRange+1);
        
        // Play combat animations // TODO call animations
        
        // Apply Block and Healing
        foreach (var abilityValue in activeAbility.abilityValues) {
            if (abilityValue.cardValue == CardValue.Defense) {
                user.currentDefense += defense;
                if (user.currentDefense > user.maxDefense) {
                    user.currentDefense = user.maxDefense;
                }
            }
            if (abilityValue.cardValue == CardValue.Heal) {
                user.currentHealth += heal;
                if (user.currentHealth > user.maxHealth) {
                    user.currentHealth = user.maxHealth;
                }
            }
        }
        
        // Deal the damage to defense, or if the target has none, to HP
        if (target.currentDefense > 0) {
            // If the damage dealt is greater than the target's defense, deal the rest to their HP
            if (damage > target.currentDefense) {
                int overflowDamage = damage - target.currentDefense;
                target.currentDefense = 0;
                target.currentHealth -= overflowDamage;
            } else {
                target.currentDefense -= damage;
            }
        } else {
            target.currentHealth -= damage;
        }
        print("Target HP: " + target.currentHealth);
        if (target.currentHealth <= 0) {
            isLethal = true;
        }
        
        SaveResources();
        yield break;
    }

    private IEnumerator NpcDefendRoutine(NpcAbility activeAbility, NpcEntity user, CombatEntity target)
    {
        print(user.name + " used " + activeAbility.abilityName + " against " + target.name);
        
        // Declare defense values
        int defenseModifier = 0;
        int minDefenseRange = 0;
        int maxDefenseRange = 0;
        
        // Set Min and Max defense values 
        for (int i = 0; i < activeAbility.abilityValues.Count; i++) {
            if (activeAbility.abilityValues[i].cardValue == CardValue.Defense) {
                minDefenseRange = activeAbility.abilityValues[i].minValue + defenseModifier;
                maxDefenseRange = activeAbility.abilityValues[i].maxValue + defenseModifier;
            }
        }

        // Determine defense amount
        int defense = Random.Range(minDefenseRange, maxDefenseRange+1);

        // Declare heal values
        int healModifier = 0;
        int minHealRange = 0;
        int maxHealRange = 0;
        
        // Set Min and Max heal values 
        for (int i = 0; i < activeAbility.abilityValues.Count; i++) {
            if (activeAbility.abilityValues[i].cardValue == CardValue.Heal) {
                minHealRange = activeAbility.abilityValues[i].minValue + healModifier;
                maxHealRange = activeAbility.abilityValues[i].maxValue + healModifier;
            }
        }

        // Determine heal amount
        int heal = Random.Range(minHealRange, maxHealRange+1);
        
        // Apply Block and Healing
        foreach (var abilityValue in activeAbility.abilityValues) {
            if (abilityValue.cardValue == CardValue.Defense) {
                target.currentDefense += defense;
                if (target.currentDefense > target.maxDefense) {
                    target.currentDefense = target.maxDefense;
                }
            }
            if (abilityValue.cardValue == CardValue.Heal) {
                target.currentHealth += heal;
                if (target.currentHealth > target.maxHealth) {
                    target.currentHealth = target.maxHealth;
                }
            }
        }
        
        SaveResources();
        yield break;
    }

    private IEnumerator AttackCardRoutine(PlayerEntity user, CombatEntity target)
    {
        print(user.name + " used " + user.selectedCard.cardName + " against " + target.name);

        CombatCard activeCard = user.selectedCard;
        
        // Declare damage values
        int damageModifier = 0;
        float acc = 100;
        int minDamageRange = 0;
        int maxDamageRange = 0;
        bool isLethal = false;
        
        // Set Min and Max damage values 
        for (int i = 0; i < activeCard.cardValues.Count; i++) {
            if (activeCard.cardValues[i].cardValue == CardValue.Damage) {
                minDamageRange = activeCard.cardValues[i].minValue + damageModifier;
                maxDamageRange = activeCard.cardValues[i].maxValue + damageModifier;
            }
        }

        // Determine damage amount
        int damage = Random.Range(minDamageRange, maxDamageRange+1);

        // Play combat animations // TODO call animations
        
        // Deal the damage to defense, or if the target has none, to HP
        if (target.currentDefense > 0) {
            // If the damage dealt is greater than the target's defense, deal the rest to their HP
            if (damage > target.currentDefense) {
                int overflowDamage = damage - target.currentDefense;
                target.currentDefense = 0;
                target.currentHealth -= overflowDamage;
            } else {
                target.currentDefense -= damage;
            }
        } else {
            target.currentHealth -= damage;
        }
        print("Target HP: " + target.currentHealth);
        if (target.currentHealth <= 0) {
            isLethal = true;
        }
        
        SaveResources();
        yield break;
    }

    private IEnumerator SkillCardRoutine(PlayerEntity user, CombatEntity target)
    {
        print(user.name + " used " + user.selectedCard.cardName + " against " + target.name);
        
        CombatCard activeCard = user.selectedCard;
        
        // Declare defense values
        int defenseModifier = 0;
        int minDefenseRange = 0;
        int maxDefenseRange = 0;
        
        // Set Min and Max defense values 
        for (int i = 0; i < activeCard.cardValues.Count; i++) {
            if (activeCard.cardValues[i].cardValue == CardValue.Defense) {
                minDefenseRange = activeCard.cardValues[i].minValue + defenseModifier;
                maxDefenseRange = activeCard.cardValues[i].maxValue + defenseModifier;
            }
        }

        // Determine defense amount
        int defense = Random.Range(minDefenseRange, maxDefenseRange+1);

        // Declare heal values
        int healModifier = 0;
        int minHealRange = 0;
        int maxHealRange = 0;
        
        // Set Min and Max heal values 
        for (int i = 0; i < activeCard.cardValues.Count; i++) {
            if (activeCard.cardValues[i].cardValue == CardValue.Heal) {
                minHealRange = activeCard.cardValues[i].minValue + healModifier;
                maxHealRange = activeCard.cardValues[i].maxValue + healModifier;
            }
        }

        // Determine heal amount
        int heal = Random.Range(minHealRange, maxHealRange+1);

        // Play combat animations // TODO call animations
        
        // Apply Block and Healing
        foreach (var cardValue in activeCard.cardValues) {
            if (cardValue.cardValue == CardValue.Defense) {
                target.currentDefense += defense;
                if (target.currentDefense > target.maxDefense) {
                    target.currentDefense = target.maxDefense;
                }
            }
            if (cardValue.cardValue == CardValue.Heal) {
                target.currentHealth += heal;
                if (target.currentHealth > target.maxHealth) {
                    target.currentHealth = target.maxHealth;
                }
            }
        }
        
        SaveResources();
        yield break;
    }

    private void SaveResources()
    {
        for (var i = 0; i < allyCombatants.Count; i++) {
            var ally = allyCombatants[i];
            partyManager.SaveHealth(i, ally.currentHealth);
        }
    }

    private void RemoveDeadCombatants()
    {
        for (int i = 0; allCombatants.Count > i; i++) {
            CombatEntity deadCombatant = null;
            if (allCombatants[i].currentHealth <= 0) {
                deadCombatant = allCombatants[i];
                if (!deadCombatant.isPlayer) {
                    int enemyIndex = enemyCombatants.FindIndex(t => t.name == deadCombatant.name);
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

    public void CardClicked(int index)
    {
        if (!canSelectCards) { return; }
        PlayerEntity activePlayer = playerCombatants.Find(t => t.name == activeEntity.name);
        List<CombatCard> cardsInHand = activePlayer.myDeck.hand.GetCardsInHand();
        CombatCard card = cardsInHand[index];
        activePlayer.selectedCard = card;
        cardSelected = true;
    }

    public void TargetClicked(int index, bool isEnemy)
    {
        CombatEntity target = isEnemy ? enemyCombatants[index] : allyCombatants[index];
        activeEntity.selectedTarget = target;
        targetSelected = true;
    }
}

[Serializable]
public class CombatEntity //TODO extend this class to PlayerEntity so I can separate player only values from enemies
{
    public string name;
    public Sprite portrait;
    public bool isPlayer;
    public bool myFirstTurn = true;
    public bool wasDamagedLastTurn = false;
    public int damagedBy;
    
    public string activeAbilityType;
    public int target;
    
    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    public int maxDefense = 99999;
    public int currentDefense;
    
    public CombatEntity selectedTarget;
    
    public List<CombatToken> activeTokens = new List<CombatToken>();

    internal CombatEntity() {}
}

[Serializable]
public class PlayerEntity : CombatEntity
{
    public int maxEnergy = 3;
    public int currentEnergy = 3;
    public int turnDraw;
    
    public CombatCard selectedCard;
    
    public PlayerDeck myDeck;
    public List<CombatCard> myCards = new List<CombatCard>();
    
    internal PlayerEntity(ActiveCharacter playerCharacter, int draw)
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
        
        turnDraw = draw;
    }

    internal void DisplayCharacterEnergy(EnergyDisplayUI energyUI)
    {
        currentEnergy = maxEnergy;
        energyUI.DisplayEnergy(currentEnergy, maxEnergy);
    }

    internal void UpdateEnergyUI(EnergyDisplayUI energyUI, int energyConsumed)
    {
        currentEnergy -= energyConsumed;
        if (currentEnergy < 0 ) { currentEnergy = 0; }
        energyUI.ChangeEnergy(currentEnergy, maxEnergy);
    }

    internal void HideEnergyUI(EnergyDisplayUI energyUI)
    {
        energyUI.HideEnergy();
    }
    
    internal void LinkDeck(PlayerDeck deck)
    {
        myDeck = deck;
    }

    internal void DrawHand()
    {
        myDeck.DrawCards(turnDraw);
    }

    internal void DiscardHand()
    {
        myDeck.DiscardHand();
    }
}

[Serializable]
public class NpcEntity : CombatEntity
{
    private int takenTurns = 0;
    private NpcBrain myBrain;
    
    internal NpcEntity(Enemy enemy)
    {
        isPlayer = false;
        
        name = enemy.enemyName;
        maxHealth = enemy.maxHealth;
        currentHealth = enemy.currentHealth;
        maxArmor = enemy.maxHealth;
        currentArmor = enemy.currentArmor;
        myBrain = enemy.myBrain;
    }

    internal void DisplayIntent()
    {
        
    }

    internal NpcAbility RetrieveAbility()
    {
        NpcAbility turnAbility = myBrain.npcAbilities[takenTurns % myBrain.npcAbilities.Count];
        takenTurns++;
        return turnAbility;
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

    public void DiscardHand()
    {
        while (hand.GetHandCount() > 0) {
            hand.RemoveCardFromHand(hand.GetCardsInHand()[hand.GetHandCount()-1], false);
        }
    }
}
