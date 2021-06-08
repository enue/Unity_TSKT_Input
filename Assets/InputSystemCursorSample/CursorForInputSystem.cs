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
        Image image = default!;

        RectTransform? rectTransform;
        RectTransform RectTransform => rectTransform ? rectTransform! : rectTransform = GetComponent<RectTransform>();

        readonly Vector3[] worldCorners = new Vector3[4];

        void Update()
        {
            var selected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (!selected)
            {
                image.enabled = false;
                return;
            }

            image.enabled = true;
            var selectedRectTransform = selected.GetComponent<RectTransform>();
            selectedRectTransform.GetWorldCorners(worldCorners);
            var size = worldCorners[2] - worldCorners[0];
            var worldPosition = Vector3.Lerp(worldCorners[2], worldCorners[0], 0.5f);
            RectTransform.position = worldPosition;
            var scale =  RectTransform.lossyScale;
            RectTransform.sizeDelta = new Vector2(size.x / scale.x, size.y / scale.y);
        }
    }
}
