using UnityEngine;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CombatController combatController;
    [SerializeField] private Canvas myCanvas;
    
    private Vector3 startPos;
    [SerializeField] private int handLayer;

    private void Awake()
    {
        combatController = FindFirstObjectByType<CombatController>();
        myCanvas.overrideSorting = true;
    }

    public void SetHandLayering(int index)
    {
        handLayer = index;
        myCanvas.sortingOrder = handLayer;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        int index = gameObject.transform.GetSiblingIndex();
        combatController.CardClicked(index);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        myCanvas.sortingOrder = 20;
        
        startPos = gameObject.transform.position;
        var pos = gameObject.transform.position;
        pos.y += 300;
        gameObject.transform.position = pos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myCanvas.sortingOrder = handLayer;
        
        gameObject.transform.position = startPos;
    }
}
