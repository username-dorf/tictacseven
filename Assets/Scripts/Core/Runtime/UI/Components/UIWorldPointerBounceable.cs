using UnityEngine.EventSystems;

namespace Core.UI.Components
{
   
    public abstract class UIWorldPointerBounceable: UIWorldBounceable, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            ExecutePress();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            ExecuteRelease();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            ExecuteExit();
        }
    }
}