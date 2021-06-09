#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

namespace TSKT
{
    public class CursorForInputSystem : MonoBehaviour
    {
        [SerializeField]
        InputActionReference point = default!;

        [SerializeField]
        InputActionReference navigate = default!;

        [SerializeField]
        Image image = default!;

        RectTransform? rectTransform;
        RectTransform RectTransform => rectTransform ? rectTransform! : rectTransform = GetComponent<RectTransform>();

        readonly Vector3[] worldCorners = new Vector3[4];

        bool usingKeyboard = true;

        void Start()
        {
            point.action.Enable();
            navigate.action.Enable();

            navigate.action.performed += _ =>
            {
                usingKeyboard = true;
            };

            point.action.performed += _ =>
            {
                usingKeyboard = false;
            };
        }

        void Update()
        {
            var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (!selected)
            {
                image.enabled = false;
                return;
            }

            image.enabled = usingKeyboard;
            var selectedRectTransform = selected.GetComponent<RectTransform>();
            var rect = GetWorldRect(selectedRectTransform);
            RectTransform.position = rect.center;
            var scale =  RectTransform.lossyScale;
            RectTransform.sizeDelta = new Vector2(rect.width / scale.x, rect.height / scale.y);

            if (image.enabled)
            {
                AdjustScrollPosition(selected, rect);
            }
        }

        void AdjustScrollPosition(GameObject obj, Rect rect)
        {
            var scrollRect = obj.GetComponentInParent<ScrollRect>();
            if (!scrollRect)
            {
                return;
            }
            if (!scrollRect.viewport)
            {
                return;
            }
            if (!obj.transform.IsChildOf(scrollRect.content))
            {
                return;
            }
            var contentRect = GetWorldRect(scrollRect.content);
            var viewportRect = GetWorldRect(scrollRect.viewport);
            if (scrollRect.vertical)
            {
                float move = 0f;
                if (rect.yMin < viewportRect.yMin)
                {
                    move = rect.yMin - viewportRect.yMin;
                }
                else if (rect.yMax > viewportRect.yMax)
                {
                    move = rect.yMax - viewportRect.yMax;
                }
                scrollRect.verticalNormalizedPosition += move / contentRect.height;
            }
            if (scrollRect.horizontal)
            {
                float move = 0f;
                if (rect.xMin < viewportRect.xMin)
                {
                    move = rect.xMin - viewportRect.xMin;
                }
                else if (rect.xMax > viewportRect.xMax)
                {
                    move = rect.xMax - viewportRect.xMax;
                }
                scrollRect.horizontalNormalizedPosition += move / contentRect.width;
            }
        }

        public Rect GetWorldRect(RectTransform rect)
        {
            rect.GetWorldCorners(worldCorners);
            return Rect.MinMaxRect(
                worldCorners[0].x, worldCorners[0].y,
                worldCorners[2].x, worldCorners[2].y);
        }
    }
}
