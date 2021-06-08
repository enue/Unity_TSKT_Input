#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TSKT
{
    public class InputActionUIManager : MonoBehaviour
    {
        [SerializeField]
        bool controlNavigation = false;

        readonly List<GameObject?> selectedGameObjects = new List<GameObject?>();

        void BuildNavigation(List<InputActionUI> sortedItems)
        {
            foreach (var it in Selectable.allSelectablesArray)
            {
                var navigation = it!.navigation;
                navigation.mode = Navigation.Mode.None;
                it.navigation = navigation;
            }
            GameObject? topSelectabeGameObject = null;
            int? latestLog = null;
            foreach (var it in sortedItems)
            {
                if (it.Selectable)
                {
                    if (it.TryGetComponent<Selectable>(out var selectable))
                    {
                        var navigation = selectable.navigation;
                        navigation.mode = Navigation.Mode.Automatic;
                        topSelectabeGameObject = it.gameObject;
                        selectable.navigation = navigation;

                        var index = selectedGameObjects.IndexOf(it.gameObject);
                        if (index > 0)
                        {
                            if (!latestLog.HasValue || latestLog.Value > index)
                            {
                                latestLog = index;
                            }
                        }
                    }
                }
            }
            if (latestLog.HasValue)
            {
                EventSystem.current.SetSelectedGameObject(selectedGameObjects[latestLog.Value]);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(topSelectabeGameObject);
            }

            selectedGameObjects.RemoveAll(_ => !_);
            selectedGameObjects.Insert(0, null);
        }

        void Update()
        {
            if (controlNavigation)
            {
                var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
                if (currentSelectedGameObject)
                {
                    selectedGameObjects[0] = currentSelectedGameObject;
                }
            }

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
                else
                {
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
}