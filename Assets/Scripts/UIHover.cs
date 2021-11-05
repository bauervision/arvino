using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{

    public static bool overUI = false;


    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        overUI = true;
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        overUI = false;
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        overUI = true;
        print("Pressed over UI");
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        overUI = false;
        print("Released over UI");
    }
}