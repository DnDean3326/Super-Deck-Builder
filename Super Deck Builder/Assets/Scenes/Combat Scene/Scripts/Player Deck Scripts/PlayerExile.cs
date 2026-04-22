using System.Collections.Generic;
using UnityEngine;

public class PlayerExile : MonoBehaviour
{
    private CombatEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInExile = new List<CombatCard>();
    
    public void SetMyPlayer(CombatEntity player)
    {
        myPlayer = player;
    }
    
    public int GetExileCount()
    {
        return cardsInExile.Count;
    }
}
