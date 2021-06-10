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
            var rect = selectedRectTransform.GetWorldRect();
            RectTransform.position = rect.center;
            var scale = RectTransform.lossyScale;
            RectTransform.sizeDelta = new Vector2(rect.width / scale.x, rect.height / scale.y);

            if (image.enabled)
            {
                var scrollRect = selected.GetComponentInParent<ScrollRect>();
                if (scrollRect
                    && scrollRect.viewport
                    && selected.transform.IsChildOf(scrollRect.content))
                {
                    scrollRect.AdjustScrollPositionToCointainInViewport(selectedRectTransform, new Vector2(0f, 50f));
                    return;
                }
            }
        }

    }
}
