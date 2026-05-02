using System.Collections.Generic;
using UnityEngine;

public class PlayerDraw : MonoBehaviour
{
    private PlayerEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInDraw = new List<CombatCard>();
    
    public void SetMyPlayer(PlayerEntity player)
    {
        myPlayer = player;
    }

    public void InitializeDeck(List<CombatCard> playerDeck)
    {
        print("Adding cards to player deck");
        cardsInDraw.AddRange(playerDeck);
    }

    public int GetDrawCount()
    {
        return cardsInDraw.Count;
    }

    public CombatCard DrawTopCard()
    {
        var topCard = cardsInDraw[0];
        cardsInDraw.RemoveAt(0);
        return topCard;
    }

    public void AddCardsToDraw(List<CombatCard> cardsToAdd)
    {
        cardsInDraw.AddRange(cardsToAdd);
    }

    public void ShuffleDeck()
    {
        StaticMethods.Shuffle(cardsInDraw);
    }
}
