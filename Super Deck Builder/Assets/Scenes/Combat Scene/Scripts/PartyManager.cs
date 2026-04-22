using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PlayerInfo testCharacter;
    
    [SerializeField] private PlayerInfo[] allCharacters;
    [SerializeField] private List<ActiveCharacter> currentParty;
    [SerializeField] private Transform cardStorage;

    private static GameObject _instance;

    private const int BASE_LEVEL = 1;
    
    private void Awake()
    {
        if (_instance != null) {
            Destroy(gameObject);
        } else {
            _instance = gameObject;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    public void GenerateTestParty()
    {
        AddMemberToPartyByName(testCharacter.characterAlias, BASE_LEVEL);
    }
    
    public static int GetBaseLevel()
    {
        return BASE_LEVEL;
    }

    public void AddMemberToPartyByName(string memberName, int level)
    {
        for (int i = 0; i < allCharacters.Length; i++) {
            if (allCharacters[i].characterAlias == memberName) {
                ActiveCharacter newMember = new ActiveCharacter(allCharacters[i], level);
                for (var p = 0; p < newMember.currentDeck.Count; p++) {
                    var card = newMember.currentDeck[p];
                    CreateCardDisplay(card);
                }

                currentParty.Add(newMember);
            }
        }
    }
    
    public List<PlayerInfo> GetAllAllies()
    {
        List<PlayerInfo> characterList = new List<PlayerInfo>();
        characterList.AddRange(allCharacters);
        return characterList;
    }
    
    public List<ActiveCharacter> GetCurrentParty()
    {
        List<ActiveCharacter> aliveParty = currentParty;
        for (int i = 0; i < aliveParty.Count; i++) {
            if (aliveParty[i].currentHealth <= 0) {
                aliveParty.RemoveAt(i);
            }
        }
        return aliveParty;
    }

    public void UpdatePartyStatus()
    {
        List<ActiveCharacter> deadMembers = new List<ActiveCharacter>();
        foreach (ActiveCharacter member in currentParty) {
            if (member.currentHealth <= 0) {
                deadMembers.Add(member);
            }
        }
        foreach (ActiveCharacter member in deadMembers) {
            currentParty.Remove(member);
        }
    }
    
    public void SaveHealth(int partyMember, int health)
    {
        currentParty[partyMember].currentHealth = health;
    }
    
    public void ClearCurrentParty()
    {
        currentParty.Clear();
    }
    
    public void CreateCardDisplay(ActiveCard card)
    {
        GameObject cardVisuals = Instantiate(card.cardPrefab, cardStorage);
        cardVisuals.GetComponent<CardDisplay>().SetCardDisplay(card);
        card.cardPrefab = cardVisuals;
    }
}

[Serializable]
public class ActiveCharacter
{
    public string activeName;
    public string activeAlias;
    public int level;

    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    
    public List<ActiveCard> currentDeck = new List<ActiveCard>();

    public ActiveCharacter(PlayerInfo playerInfo)
    {
        level = PartyManager.GetBaseLevel();
        
        activeName = playerInfo.characterName;
        activeAlias = playerInfo.characterAlias;
        maxHealth = playerInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = playerInfo.baseArmor;
        currentArmor = maxArmor;
        
        AddStarterCards(playerInfo.startingCards, playerInfo);
    }

    public ActiveCharacter(PlayerInfo playerInfo, int level)
    {
        this.level = level;
        
        activeName = playerInfo.characterName;
        activeAlias = playerInfo.characterAlias;
        maxHealth = playerInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = playerInfo.baseArmor;
        currentArmor = maxArmor;
        
        AddStarterCards(playerInfo.startingCards, playerInfo);
    }

    public void AddStarterCards(List<CardInfo> startingCards, PlayerInfo playerInfo)
    {
        foreach (CardInfo cardInfo in startingCards) {
            currentDeck.Add(new ActiveCard(cardInfo, playerInfo));
        }
    }
}

[Serializable]
public class ActiveCard
{
    public string cardName;
    public Sprite cardArt;
    public CardTypes cardType;
    public int energyCost;
    public int repeatUses;
    
    public List<ExtraCostInfo> extraCosts = new List<ExtraCostInfo>();
    public List<CardValueInfo> cardValues = new List<CardValueInfo>();
    public List<TokenAppliedInfo> selfTokensApplied = new List<TokenAppliedInfo>();
    public List<TokenAppliedInfo> targetTokensApplied = new List<TokenAppliedInfo>();
    
    public string cardDescription;
    
    public GameObject cardPrefab;

    public ActiveCard(CardInfo cardInfo, PlayerInfo playerInfo)
    {
        cardName = cardInfo.cardName;
        cardArt = cardInfo.cardArt;
        cardType = cardInfo.cardType;
        energyCost = cardInfo.energyCost;
        repeatUses = cardInfo.repeatUses;
        cardDescription = cardInfo.cardDescription;
        cardPrefab = playerInfo.characterCardPrefab;

        if (cardInfo.cardValueInfos != null) {
            foreach (var cardValue in cardInfo.cardValueInfos) {
                CardValueInfo tempValue = new CardValueInfo(cardValue);
                cardValues.Add(tempValue);
            }
        }
        if (cardInfo.extraCostInfos != null) {
            foreach (var extraCost in cardInfo.extraCostInfos) {
                ExtraCostInfo tempCost = new ExtraCostInfo(extraCost);
                extraCosts.Add(tempCost);
            }
        }
        if (cardInfo.selfTokenAppliedInfos != null) {
            foreach (var selfToken in cardInfo.selfTokenAppliedInfos) {
                TokenAppliedInfo tempToken = new TokenAppliedInfo(selfToken);
                selfTokensApplied.Add(tempToken);
            }
        }
        if (cardInfo.targetTokenAppliedInfos != null) {
            foreach (var selfToken in cardInfo.targetTokenAppliedInfos) {
                TokenAppliedInfo tempToken = new TokenAppliedInfo(selfToken);
                targetTokensApplied.Add(tempToken);
            }
        }
    }

    public void ModifyEnergyCost(int valueChange)
    {
        energyCost += valueChange;
    }

    public void SetEnergyCost(int valueChange)
    {
        energyCost = valueChange;
    }
    
    public void ModifyValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.minValue += valueChange;
            valueToChange.maxValue += valueChange;
        }
    }
    
    public void SetValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.minValue = valueChange;
            valueToChange.maxValue = valueChange;
        }
    }

    public void ModifyMinValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.minValue += valueChange;
        }
    }
    
    public void SetMinValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.minValue = valueChange;
        }
    }
    
    public void ModifyMaxValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.maxValue += valueChange;
        }
    }
    
    public void SetMaxValue(CardValue value, int valueChange)
    {
        if (cardValues.Any(t => t.cardValue == value)) {
            CardValueInfo valueToChange = cardValues.First(t => t.cardValue == value);
            valueToChange.maxValue = valueChange;
        }
    }

    public void ModifyRepeatUses(int valueChange)
    {
        repeatUses += valueChange;
    }

    public void SetRepeatUses(int valueChange)
    {
        repeatUses = valueChange;
    }
}