using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private Transform cardParent;
    private CombatEntity myPlayer;
    [SerializeField] private List<CombatCard> cardsInHand = new List<CombatCard>();

    private void Awake()
    {
        var canvas = FindFirstObjectByType<Canvas>().gameObject;
        cardParent = canvas.transform.Find("Hand Visual");
    }
    
    public void SetMyPlayer(CombatEntity player)
    {
        myPlayer = player;
    }

    public int GetHandCount()
    {
        return cardsInHand.Count;
    }
    
    public void AddCardToHand(CombatCard card)
    {
        cardsInHand.Add(card);
        Instantiate(card.cardVisuals, cardParent);
    }
}
