#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Buffers;

namespace TSKT
{
    public class InputActionUIManager : MonoBehaviour
    {
        [SerializeField]
        bool controlNavigation = false;

        readonly List<GameObject?> selectedGameObjects = new();
        readonly ArrayBufferWriter<InputActionUI> sortedItems = new();

        Selectable?[]? pool;

        void BuildNavigation(in ReadOnlySpan<InputActionUI> sortedItems)
        {
            if (pool == null || pool.Length < Selectable.allSelectableCount)
            {
                pool = new Selectable[Selectable.allSelectableCount];
            }
            var count = Selectable.AllSelectablesNoAlloc(pool);
            for (int i = count; i < pool.Length; i++)
            {
                pool[i] = null;
            }
            foreach (var it in pool.AsSpan(0, count))
            {
                var navigation = it!.navigation;
                navigation.mode = Navigation.Mode.None;
                it.navigation = navigation;
            }

            GameObject? topSelectableGameObject = null;
            int? latestLog = null;
            foreach (var it in sortedItems)
            {
                if (it.Navigation.mode != Navigation.Mode.None)
                {
                    var selectable = it.Selectable;
                    if (selectable && selectable!.enabled)
                    {
                        selectable.navigation = it.Navigation;
                        topSelectableGameObject = it.gameObject;
                        if (!latestLog.HasValue || latestLog.Value > 0)
                        {
                            var index = selectedGameObjects.IndexOf(it.gameObject);
                            if (index >= 0)
                            {
                                if (!latestLog.HasValue || latestLog.Value > index)
                                {
                                    latestLog = index;
                                }
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
                EventSystem.current.SetSelectedGameObject(topSelectableGameObject);
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
                    var i = selectedGameObjects.IndexOf(currentSelectedGameObject);
                    if (i > 0)
                    {
                        selectedGameObjects.RemoveAt(i);
                    }
                    selectedGameObjects.Insert(0, currentSelectedGameObject);

                    if (currentSelectedGameObject.TryGetComponent<Selectable>(out var selectable))
                    {
                        if (!selectable.enabled)
                        {
                            InputActionUI.ShouldBuildNavigation = true;
                        }
                    }
                }
            }

            sortedItems.Clear();
            InputActionUI.BuildSortedItemsToActivate(sortedItems);
            if (InputActionUI.ShouldBuildNavigation)
            {
                InputActionUI.ShouldBuildNavigation = false;

                if (controlNavigation)
                {
                    BuildNavigation(sortedItems.WrittenSpan);
                }

                foreach (var it in sortedItems.WrittenSpan)
                {
                    it.Activate();
                }
            }
            else
            {
                foreach (var item in sortedItems.WrittenSpan)
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