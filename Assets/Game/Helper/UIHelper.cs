using UnityEngine;
using UnityEngine.UI;
using UniverseEngine;

namespace USquad
{
    public static class UIHelper
    {
        public static void SetActive<T>(T transform, bool active, bool forceShow = false) where T : Component
        {
            if (transform == null)
            {
                return;
            }

            if (forceShow)
            {
                transform.gameObject.SetActive(active);
                return;
            }
            
            if (transform.gameObject.activeInHierarchy == active)
            {
                return;
            }

            transform.gameObject.SetActive(active);
        }

        public static void SetSprite(Image image, Sprite sprite)
        {
            if (image != null)
            {
                image.sprite = sprite;
            }
        }

        public static void OpenWidget(UIID id, Run<UIElement> onOpen = null, Args args = default(Args))
        {
            UISystem.OpenWidget((int)id, onOpen, args);
        }

        public static void CloseWidget(UIID id, bool destroy = false, Run onClose = null)
        {
            UISystem.Close((int)id, destroy, onClose);
        }

        public static bool IsOpened(UIID id)
        {
            return UISystem.IsOpened((int)id);
        }
    }
}
