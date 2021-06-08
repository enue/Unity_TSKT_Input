#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TSKT
{
    public class InputActionUIManager : MonoBehaviour
    {
        [SerializeField]
        bool controlNavigation = false;

        void BuildNavigation(List<InputActionUI> sortedItems)
        {
            foreach (var it in Selectable.allSelectablesArray)
            {
                var navigation = it!.navigation;
                navigation.mode = Navigation.Mode.None;
                it.navigation = navigation;
            }
            GameObject? selectdGameObject = null;
            foreach (var it in sortedItems)
            {
                if (it.Selectable)
                {
                    if (it.TryGetComponent<Selectable>(out var selectable))
                    {
                        var navigation = selectable.navigation;
                        navigation.mode = Navigation.Mode.Automatic;
                        selectdGameObject = it.gameObject;
                        selectable.navigation = navigation;
                    }
                }
            }
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectdGameObject);
        }

        void Update()
        {
            using (InputActionUI.BuildSortedItemsToActivate(out var sortedItems))
            {
                if (InputActionUI.Modified)
                {
                    InputActionUI.Modified = false;

                    if (controlNavigation)
                    {
                        BuildNavigation(sortedItems);
                    }

                    foreach (var it in sortedItems)
                    {
                        it.Activate();
                    }
                }

                foreach (var item in sortedItems)
                {
                    item.Invoke(out var exclusive);
                    if (exclusive)
                    {
                        break;
                    }
                }
            }
        }
    }
}