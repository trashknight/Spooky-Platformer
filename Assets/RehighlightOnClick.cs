using UnityEngine;
using UnityEngine.EventSystems;

public class RehighlightOnClick : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public void OnPointerExit(PointerEventData eventData)
    {
        // Force re-enter if pointer is still over this object after exit
        StartCoroutine(ForceReenter());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight should reactivate normally after forced reenter
    }

    private System.Collections.IEnumerator ForceReenter()
    {
        yield return null; // wait 1 frame
        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
    }
}