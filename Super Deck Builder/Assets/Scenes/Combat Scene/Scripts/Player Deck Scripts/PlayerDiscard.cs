using System.Collections.Generic;
using UnityEngine;

public class PlayerDiscard : MonoBehaviour
{
    private PlayerEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInDiscard = new List<CombatCard>();
    
    public void SetMyPlayer(PlayerEntity player)
    {
        myPlayer = player;
    }

    public void AddToDiscard(CombatCard card)
    {
        cardsInDiscard.Add(card);
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
