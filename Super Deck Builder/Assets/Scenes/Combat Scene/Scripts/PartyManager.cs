using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PartyManager : MonoBehaviour
{
    [SerializeField] private PlayerInfo[] allCharacters;
    [SerializeField] private List<ActiveCharacter> currentParty;

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
    
    public static int GetBaseLevel()
    {
        return BASE_LEVEL;
    }

    public void AddMemberToPartyByName(string memberName, int level)
    {
        for (int i = 0; i < currentParty.Count; i++) {
            if (allCharacters[i].characterAlias == memberName) {
                ActiveCharacter newMember = new ActiveCharacter(allCharacters[i], level);
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
}

[Serializable]
public class ActiveCharacter
{
    public string activeName;
    public int level;

    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    
    public List<ActiveCard> currentDeck = new List<ActiveCard>();

    public ActiveCharacter(PlayerInfo playerInfo)
    {
        level = PartyManager.GetBaseLevel();
        
        activeName = playerInfo.characterAlias;
        maxHealth = playerInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = playerInfo.baseArmor;
        currentArmor = maxArmor;
        
        AddStarterCards(playerInfo.startingCards);
    }

    public ActiveCharacter(PlayerInfo playerInfo, int level)
    {
        this.level = level;
        
        activeName = playerInfo.characterAlias;
        maxHealth = playerInfo.baseHealth;
        currentHealth = maxHealth;
        maxArmor = playerInfo.baseArmor;
        currentArmor = maxArmor;
        
        AddStarterCards(playerInfo.startingCards);
    }

    public void AddStarterCards(List<CardInfo> startingCards)
    {
        foreach (CardInfo cardInfo in startingCards) {
            currentDeck.Add(new ActiveCard(cardInfo));
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
    public int cardMinValue;
    public int cardMaxValue;
    public int repeatUses;

    public bool hasExtraCost = false;
    public List<ExtraCostInfo> extraCostInfos = new List<ExtraCostInfo>();
    
    public string cardDescription;

    public ActiveCard(CardInfo cardInfo)
    {
        cardName = cardInfo.cardName;
        cardArt = cardInfo.cardArt;
        cardType = cardInfo.cardType;
        energyCost = cardInfo.energyCost;
        cardMinValue = cardInfo.cardMinValue;
        cardMaxValue = cardInfo.cardMaxValue;
        repeatUses = cardInfo.repeatUses;
        if (cardInfo.extraCostInfos != null) {
            extraCostInfos.AddRange(cardInfo.extraCostInfos);
            hasExtraCost = true;
        }
        cardDescription = cardInfo.cardDescription;
    }

    public void ModifyEnergyCost(int valueChange)
    {
        energyCost += valueChange;
    }

    public void SetEnergyCost(int valueChange)
    {
        energyCost = valueChange;
    }
    
    public void ModifyValue(int valueChange)
    {
        cardMinValue += valueChange;
        cardMaxValue += valueChange;
    }
    
    public void SetValue(int valueChange)
    {
        cardMinValue = valueChange;
        cardMaxValue = valueChange;
    }

    public void ModifyMinValue(int valueChange)
    {
        cardMinValue += valueChange;
    }
    
    public void SetMinValue(int valueChange)
    {
        cardMinValue = valueChange;
    }
    
    public void SetMaxValue(int valueChange)
    {
        cardMaxValue = valueChange;
    }
    
    public void ModifyMaxValue(int valueChange)
    {
        cardMaxValue += valueChange;
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