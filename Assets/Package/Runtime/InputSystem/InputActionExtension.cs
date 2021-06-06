using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TSKT
{
    public static class InputActionExtension
    {
        static public System.IDisposable SubscribePerform(this InputAction action,
            System.Action<InputAction.CallbackContext> callback)
        {
            return new SubscribeInputActionPerform(action, callback);
        }
        static public System.IDisposable SubscribeStart(this InputAction action,
            System.Action<InputAction.CallbackContext> callback)
        {
            return new SubscribeInputActionStart(action, callback);
        }
        static public System.IDisposable SubscribeCancel(this InputAction action,
            System.Action<InputAction.CallbackContext> callback)
        {
            return new SubscribeInputActionCancel(action, callback);
        }
    }

    public class SubscribeInputActionPerform : System.IDisposable
    {
        readonly InputAction action;
        readonly System.Action<InputAction.CallbackContext> callback;

        public SubscribeInputActionPerform(InputAction action, System.Action<InputAction.CallbackContext> callback)
        {
            action.performed += callback;
            this.action = action;
            this.callback = callback;
        }

        public void Dispose()
        {
            action.performed -= callback;
        }
    }

    public class SubscribeInputActionStart : System.IDisposable
    {
        readonly InputAction action;
        readonly System.Action<InputAction.CallbackContext> callback;

        public SubscribeInputActionStart(InputAction action, System.Action<InputAction.CallbackContext> callback)
        {
            action.started += callback;
            this.action = action;
            this.callback = callback;
        }

        public void Dispose()
        {
            action.started -= callback;
        }
    }

    public class SubscribeInputActionCancel : System.IDisposable
    {
        readonly InputAction action;
        readonly System.Action<InputAction.CallbackContext> callback;

        public SubscribeInputActionCancel(InputAction action, System.Action<InputAction.CallbackContext> callback)
        {
            action.canceled += callback;
            this.action = action;
            this.callback = callback;
        }

        public void Dispose()
        {
            action.canceled -= callback;
        }
    }
}