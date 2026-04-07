using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    
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
        level = 1;
        
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
    public int cardValue;
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
        cardValue = cardInfo.cardValue;
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
        cardValue += valueChange;
    }
    
    public void SetValue(int valueChange)
    {
        cardValue = valueChange;
    }

    public void ModifyRepeatUses(int valueChange)
    {
        repeatUses += valueChange;
    }

    public void SetRepeatUses(int valueChange)
    {
        repeatUses = valueChange;
    }

    public void ModifyExtraCost(int valueChange)
    {
        if (!hasExtraCost) {
            
        }
    }
}