using System.Collections.Generic;
using UnityEngine;

public class PlayerDiscard : MonoBehaviour
{
    private CombatEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInDiscard = new List<CombatCard>();
    
    public void SetMyPlayer(CombatEntity player)
    {
        myPlayer = player;
    }

    public int GetDiscardCount()
    {
        return cardsInDiscard.Count;
    }
    
    public List<CombatCard> RetrieveCardsInDiscard()
    {
        List<CombatCard> returnCards = new List<CombatCard>();
        returnCards.AddRange(cardsInDiscard);
        cardsInDiscard.Clear();
        return returnCards;
    }
}
