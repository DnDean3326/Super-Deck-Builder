using System.Collections.Generic;
using UnityEngine;

public class PlayerExile : MonoBehaviour
{
    private PlayerEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInExile = new List<CombatCard>();
    
    public void SetMyPlayer(PlayerEntity player)
    {
        myPlayer = player;
    }
    
    public void AddToExile(CombatCard card)
    {
        cardsInExile.Add(card);
    }
    
    public int GetExileCount()
    {
        return cardsInExile.Count;
    }
}
