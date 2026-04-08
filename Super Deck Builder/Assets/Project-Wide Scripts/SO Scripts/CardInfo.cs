using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "CardInfo", menuName = "Scriptable Objects/CardInfo")]
public class CardInfo : ScriptableObject
{
    public string cardName;
    public Sprite cardArt;
    public CardTypes cardType;
    public int energyCost;
    public int cardMinValue;
    public int cardMaxValue;
    public int repeatUses;
    
    public List<ExtraCostInfo> extraCostInfos;
    
    [TextArea(15,20)]
    public string cardDescription;
}

public struct ExtraCostInfo
{
    public List<ExtraCostInfo> extraCostType;
    public List<int> extraCost;
}
