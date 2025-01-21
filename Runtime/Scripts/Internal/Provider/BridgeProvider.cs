using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Core.Bridge
{
    internal class BridgeProvider
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MessageCallback(string eventName, string jsonData);

        private const char INTERFACE_SEPARATOR = '_';

        private readonly INativeBridge nativeBridge;

        private static ConcurrentQueue<Action> messageQueue;
        private static Dictionary<string, MonoBehaviour> eventKeyAction;

        public BridgeProvider()
        {
#if UNITY_EDITOR
            nativeBridge = new EditorBridge();
#elif UNITY_STANDALONE_WIN
            nativeBridge = new WindowsBridge();
#elif UNITY_ANDROID
            nativeBridge = new AndroidBridge();
#elif UNITY_IOS
            nativeBridge = new iOSBridge();
#endif
            if (nativeBridge == null)
            {
                throw new Exception("[NativeBridge] Not supported on this platform.");
            }

            nativeBridge.Initialize(OnReceiveMessage);

            messageQueue = new ConcurrentQueue<Action>();
            eventKeyAction = new Dictionary<string, MonoBehaviour>();
        }

        public void AddListener(MonoBehaviour mono)
        {
            string monoName = mono.GetType().Name;
            if (eventKeyAction.ContainsKey(monoName) == false)
            {
                eventKeyAction.Add(monoName, mono);
            }
        }

        public void RemoveListener(MonoBehaviour mono)
        {
            string monoName = mono.GetType().Name;
            if (eventKeyAction.ContainsKey(monoName) == true)
            {
                eventKeyAction.Remove(monoName);
            }
        }

        /// <summary>
        /// Unity MainThread에서 처리하기 위해 Update Loop에서 message를 dequeue하여 처리합니다.
        /// </summary>
        public void Update()
        {
            while (messageQueue.Count > 0)
            {
                if (messageQueue.TryDequeue(out var action))
                {
                    action.Invoke();
                }
            }
        }

        public void SendNative(string eventName, string jsonData)
        {
            Debug.Log($"[NativeBridge] {eventName}\n{jsonData}");
            nativeBridge.CallNative(eventName, jsonData);
        }

        [AOT.MonoPInvokeCallback(typeof(MessageCallback))]
        private static void OnReceiveMessage([MarshalAs(UnmanagedType.HString)] string eventName, [MarshalAs(UnmanagedType.HString)] string jsonData)
        {
            // parse message.
            string[] events = eventName.Split(INTERFACE_SEPARATOR);
            string className = events[2];
            string methodName = events[3];

            // enqueue.
            messageQueue.Enqueue(() => { OnProcessEvent(eventName, className, methodName, jsonData); });
        }

        private static void OnProcessEvent (string eventName, string className, string methodName, string jsonData)
        {
            Debug.Log($"[NativeBridge] {eventName}\n{jsonData}");

            if (eventKeyAction.ContainsKey(className))
            {
                eventKeyAction[className].SendMessage(methodName, jsonData);
            }
            else
            {
                Debug.LogError($"[NativeBridge] Not found class. {className}");
            }
        }
    }
}