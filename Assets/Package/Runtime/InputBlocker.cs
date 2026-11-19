#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace TSKT
{
    /// <summary>
    /// ユーザーからの入力を止める。Block()の戻り値をDisposeすると解除される。
    /// 複数箇所から同時にBlockでき、すべてDisposeされた時点で入力が戻る。
    /// ブロック中は、EventSystemのInputSystemUIInputModuleを無効にする
    /// (クリック、決定、キャンセル、ナビゲーション、スクロール、ホバーがまとめて止まる)。
    /// UIInputDispatcherもブロック中は通知しない。
    /// ゲーム側でアクションを直接読んでいるコードは、IsBlockedを見て止めること。
    /// UIInputDispatcherがなくても単体で動く。
    /// </summary>
    public static class InputBlocker
    {
        sealed class Handle : System.IDisposable
        {
            readonly int generation;
            bool disposed;

            public Handle(int generation)
            {
                this.generation = generation;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;

                // 再生をまたいで残ったHandleは無視する
                if (generation != InputBlocker.generation)
                {
                    return;
                }

                --count;
                Apply();
            }
        }

        static int count;
        static int generation;
        static bool applied;

        // 自分で無効にしたモジュール。元から無効だったものは含めない
        static readonly List<InputSystemUIInputModule> disabledModules = new();
        static readonly List<InputSystemUIInputModule> buffer = new();

        public static bool IsBlocked => count > 0;

        public static System.IDisposable Block()
        {
            ++count;
            Apply();
            return new Handle(generation);
        }

        static void Apply()
        {
            var blocked = IsBlocked;
            if (applied == blocked)
            {
                return;
            }
            applied = blocked;
            if (blocked)
            {
                DisableModules();
            }
            else
            {
                EnableModules();
            }
        }

        static void DisableModules()
        {
            var eventSystem = EventSystem.current;
            if (!eventSystem)
            {
                return;
            }
            eventSystem.GetComponents(buffer);
            foreach (var module in buffer)
            {
                if (module.enabled)
                {
                    module.enabled = false;
                    disabledModules.Add(module);
                }
            }
            buffer.Clear();
        }

        static void EnableModules()
        {
            foreach (var module in disabledModules)
            {
                if (module)
                {
                    module.enabled = true;
                }
            }
            disabledModules.Clear();
        }

        // ブロック中に読み込まれたシーンのEventSystemも止める
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (applied)
            {
                DisableModules();
            }
        }

        // ReloadDomainなしで再生した場合に前回の状態を持ち越さない
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Initialize()
        {
            count = 0;
            ++generation;
            applied = false;
            disabledModules.Clear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
}
