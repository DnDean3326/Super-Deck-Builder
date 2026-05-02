using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CardInfo", menuName = "Scriptable Objects/CardInfo")]
public class CardInfo : ScriptableObject
{
    public string cardName;
    public Sprite cardArt;
    public CardTypes cardType;
    public CardCharacter cardCharacter;
    public int energyCost;
    public int repeatUses;
    public CardTargeting cardTargeting;
    
    public List<SCardValueInfo> cardValueInfos = new List<SCardValueInfo>();
    public List<SExtraCostInfo> extraCostInfos = new List<SExtraCostInfo>();
    public List<STokenAppliedInfo> selfTokenAppliedInfos = new List<STokenAppliedInfo>();
    public List<STokenAppliedInfo> targetTokenAppliedInfos = new List<STokenAppliedInfo>();
    
    [TextArea(15,20)]
    public string cardDescription;
}

public class SCardValueInfo
{
    public CardValue cardValue;
    public int minValue;
    public int maxValue;
}

public class SExtraCostInfo
{
    public List<ExtraCosts> extraCostType;
    public List<int> extraCost;
}

public class STokenAppliedInfo
{
    public Token token;
    public int tokenCount;
}

public struct CardValueInfo
{
    public CardValue cardValue;
    public int minValue;
    public int maxValue;

    public CardValueInfo(SCardValueInfo info)
    {
        cardValue = info.cardValue;
        minValue = info.minValue;
        maxValue = info.maxValue;
    }
}

public struct ExtraCostInfo
{
    public List<ExtraCosts> extraCostType;
    public List<int> extraCost;

    public ExtraCostInfo(SExtraCostInfo info)
    {
        extraCostType = info.extraCostType;
        extraCost = info.extraCost;
    }
}

public struct TokenAppliedInfo
{
    public Token token;
    public int tokenCount;

    public TokenAppliedInfo(STokenAppliedInfo info)
    {
        token = info.token;
        tokenCount = info.tokenCount;
    }
}
