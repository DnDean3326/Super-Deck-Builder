using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyDisplayUI : MonoBehaviour
{
    private Image myImage;
    private TextMeshProUGUI myText;

    private string baseText = "/";
    private string maxEnergyText;
    
    private void Awake()
    {
        myImage = GetComponent<Image>();
        myText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        maxEnergyText = baseText;
        gameObject.SetActive(false);
    }

    public void DisplayEnergy(int currentEnergy, int maxEnergy)
    {
        gameObject.SetActive(true);
        maxEnergyText = baseText + maxEnergy;
        myText.text = currentEnergy + maxEnergyText;
    }

    public void ChangeEnergy(int currentEnergy)
    {
        myText.text = currentEnergy + maxEnergyText;
    }

    public void ChangeEnergy(int currentEnergy, int maxEnergy)
    {
        maxEnergyText = baseText + maxEnergy;
        myText.text = currentEnergy + maxEnergyText;
    }

    public void HideEnergy()
    {
        gameObject.SetActive(false);
    }
}
