using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private Transform cardParent;
    private PlayerEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInHand = new List<CombatCard>();

    private void Awake()
    {
        var canvas = GameObject.FindGameObjectWithTag("Main Canvas");
        cardParent = canvas.transform.Find("Hand Visual");
    }
    
    public void SetMyPlayer(PlayerEntity player)
    {
        myPlayer = player;
    }

    public int GetHandCount()
    {
        return cardsInHand.Count;
    }

    public List<CombatCard> GetCardsInHand()
    {
        return cardsInHand;
    }
    
    public void AddCardToHand(ref CombatCard card)
    {
        cardsInHand.Add(card);
        var cardObject = Instantiate(card.cardVisuals, cardParent);
        card.myCardController = cardObject.GetComponent<CardController>();
        cardObject.name = card.cardName;
    }

    public void RemoveCardFromHand(CombatCard card, bool toExile)
    {
        if (toExile) {
            myPlayer.myDeck.exile.AddToExile(card);
        } else {
            myPlayer.myDeck.discard.AddToDiscard(card);
        }
        
        int index = cardsInHand.IndexOf(card);
        Destroy(cardParent.GetChild(index).gameObject);
        cardsInHand.Remove(card);
    }
}
