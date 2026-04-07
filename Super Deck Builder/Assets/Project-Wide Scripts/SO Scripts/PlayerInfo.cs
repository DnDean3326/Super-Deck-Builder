using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInfo", menuName = "Scriptable Objects/PlayerInfo")]
public class PlayerInfo : ScriptableObject
{
    public string characterName;
    public string characterAlias;
    
    public int baseHealth;
    public int baseArmor;

    public List<CardInfo> startingCards;
    public List<CardInfo> characterCardPool;
    
    [TextArea(15,20)]
    public string characterDescription;
}
