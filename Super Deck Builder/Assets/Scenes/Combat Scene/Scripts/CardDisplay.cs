using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cardNameDisplay;
    [SerializeField] private TextMeshProUGUI cardEnergyDisplay;
    [SerializeField] private TextMeshProUGUI cardTypeDisplay;
    [SerializeField] private TextMeshProUGUI cardDescriptionDisplay;
    [SerializeField] private Image cardImageDisplay;

    public void SetCardDisplay(ActiveCard cardData)
    {
        cardNameDisplay.text = cardData.cardName;
        cardEnergyDisplay.text = cardData.energyCost.ToString();
        cardTypeDisplay.text = cardData.cardType.ToString();
        cardDescriptionDisplay.text = cardData.cardDescription;
        cardImageDisplay.sprite = cardData.cardArt;
    }
}
