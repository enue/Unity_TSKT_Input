#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TSKT.Tests
{
    /// <summary>
    /// 届いた通知を1文字ずつ記録するレシーバー。
    /// P: 押下 / F: 発火 / R: 解放 / C: 中断
    /// </summary>
    public class Recorder : IUIInputPressReceiver
    {
        readonly StringBuilder events = new();
        readonly Transform transform;

        public bool canHandle = true;

        public Recorder(Transform transform)
        {
            this.transform = transform;
        }

        public Transform Transform => transform;
        public bool CanHandle => canHandle;

        public void OnPressed(InputAction action) => events.Append('P');
        public void OnPerformed(InputAction action) => events.Append('F');
        public void OnReleased(InputAction action, bool canceled) => events.Append(canceled ? 'C' : 'R');

        /// <summary>
        /// 記録を取り出して空にする
        /// </summary>
        public string Take()
        {
            var result = events.ToString();
            events.Clear();
            return result;
        }
    }

    public class Counter
    {
        public int value;
    }

    /// <summary>
    /// EventSystem、UIInputDispatcher、Canvas、仮想キーボード、テスト用のアクションを用意する。
    /// 入力は仮想キーボードに状態を送って再現するので、ウィンドウのフォーカスには左右されない。
    /// </summary>
    public abstract class UIInputTestBase
    {
        protected Keyboard keyboard = default!;
        protected GameObject root = default!;
        protected Canvas canvas = default!;
        protected EventSystem eventSystem = default!;
        protected InputSystemUIInputModule module = default!;
        protected UIInputDispatcher dispatcher = default!;

        /// <summary>1キー</summary>
        protected InputAction action1 = default!;
        /// <summary>2キー</summary>
        protected InputAction action2 = default!;
        /// <summary>Hキーを0.3秒押し続けると発火する</summary>
        protected InputAction holdAction = default!;

        InputActionAsset asset = default!;
        readonly List<(InputAction action, IUIInputReceiver receiver)> registered = new();
        readonly List<System.IDisposable> handles = new();
        readonly List<Object> extraObjects = new();

        InputSettings.BackgroundBehavior backgroundBehavior;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode editorInputBehavior;
#endif

        [SetUp]
        public void SetUp()
        {
            var settings = InputSystem.settings;
            backgroundBehavior = settings.backgroundBehavior;
            settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorInputBehavior = settings.editorInputBehaviorInPlayMode;
            settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>("UIInputTestKeyboard");

            asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = asset.AddActionMap("Test");
            action1 = map.AddAction("Action1", InputActionType.Button, "<Keyboard>/1");
            action2 = map.AddAction("Action2", InputActionType.Button, "<Keyboard>/2");
            holdAction = map.AddAction("Hold", InputActionType.Button, "<Keyboard>/h", "hold(duration=0.3)");

            root = new GameObject("UIInputTestRoot");

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.transform.SetParent(root.transform, false);
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            module = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            dispatcher = eventSystemObject.AddComponent<UIInputDispatcher>();

            var canvasObject = new GameObject("Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(root.transform, false);
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var handle in handles)
            {
                handle.Dispose();
            }
            handles.Clear();

            foreach (var (action, receiver) in registered)
            {
                UIInputDispatcher.Unregister(action, receiver);
            }
            registered.Clear();

            foreach (var obj in extraObjects)
            {
                if (obj)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            extraObjects.Clear();

            Object.DestroyImmediate(root);
            asset.Disable();
            Object.DestroyImmediate(asset);
            InputSystem.RemoveDevice(keyboard);

            var settings = InputSystem.settings;
            settings.backgroundBehavior = backgroundBehavior;
#if UNITY_EDITOR
            settings.editorInputBehaviorInPlayMode = editorInputBehavior;
#endif
        }

        // ---------- 入力 ----------

        protected static IEnumerator Frames(int count = 3)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return null;
            }
        }

        protected IEnumerator Down(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return Frames();
        }

        protected IEnumerator Up()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return Frames();
        }

        protected IEnumerator Press(Key key)
        {
            yield return Down(key);
            yield return Up();
        }

        // ---------- 部品 ----------

        protected Recorder AddRecorder(InputAction action, Transform transform)
        {
            var recorder = new Recorder(transform);
            UIInputDispatcher.Register(action, recorder);
            registered.Add((action, recorder));
            return recorder;
        }

        /// <summary>
        /// テストが途中で失敗しても、TearDownで解除される
        /// </summary>
        protected System.IDisposable Block()
        {
            var handle = InputBlocker.Block();
            handles.Add(handle);
            return handle;
        }

        protected GameObject CreateChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        protected Button CreateButton(string name, Transform parent, Counter? counter = null)
        {
            var go = CreateChild(name, parent);
            var button = go.AddComponent<Button>();
            if (counter != null)
            {
                button.onClick.AddListener(() => ++counter.value);
            }
            return button;
        }

        /// <summary>
        /// UIInputModalScope付きのオブジェクトを、ボタンをひとつ持った状態で作る。
        /// </summary>
        protected UIInputModalScope CreateScope(string name, out Button button, bool selectFirst = true)
        {
            var go = CreateChild(name, canvas.transform);
            go.SetActive(false);
            go.AddComponent<CanvasGroup>();
            button = CreateButton("Button", go.transform);
            var scope = go.AddComponent<UIInputModalScope>();
            if (selectFirst)
            {
                SetPrivateField(scope, "firstSelected", button);
            }
            go.SetActive(true);
            return scope;
        }

        /// <summary>
        /// UIInputSubmit付きのボタンを作る。
        /// </summary>
        protected Button CreateSubmitButton(string name, Transform parent, InputAction action, Counter counter)
        {
            var go = CreateChild(name, parent);
            go.SetActive(false);
            var button = go.AddComponent<Button>();
            button.onClick.AddListener(() => ++counter.value);
            AddSubmit(go, action);
            go.SetActive(true);
            return button;
        }

        /// <summary>
        /// 非アクティブなオブジェクトに付けること(OnEnableでアクションを登録するため)。
        /// </summary>
        protected UIInputSubmit AddSubmit(GameObject go, InputAction action)
        {
            var reference = InputActionReference.Create(action);
            extraObjects.Add(reference);
            var submit = go.AddComponent<UIInputSubmit>();
            SetPrivateField(submit, "action", reference);
            return submit;
        }

        protected static void SetPrivateField(object target, string name, object? value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"field '{name}' not found on {target.GetType().Name}");
            field!.SetValue(target, value);
        }

        protected string SelectedName()
        {
            var selected = eventSystem.currentSelectedGameObject;
            return selected ? selected.transform.parent.name + "/" + selected.name : "null";
        }
    }
}
