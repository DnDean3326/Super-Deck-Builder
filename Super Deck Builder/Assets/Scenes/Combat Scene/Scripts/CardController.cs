using UnityEngine;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CombatController combatController;
    private Canvas myCanvas;
    
    private Vector3 startPos;
    private const int CANVAS_ORDER = 1;

    private void Awake()
    {
        combatController = FindFirstObjectByType<CombatController>();
        myCanvas = GetComponent<Canvas>();
        myCanvas.overrideSorting = true;
        myCanvas.sortingOrder = CANVAS_ORDER;
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        int index = gameObject.transform.GetSiblingIndex();
        combatController.CardClicked(index);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        myCanvas.sortingOrder = 3;
        
        startPos = gameObject.transform.position;
        var pos = gameObject.transform.position;
        pos.y += 300;
        gameObject.transform.position = pos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myCanvas.sortingOrder = CANVAS_ORDER;
        
        gameObject.transform.position = startPos;
    }
}
