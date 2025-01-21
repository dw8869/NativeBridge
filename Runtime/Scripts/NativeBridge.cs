using System;
using UnityEngine;

namespace Core.Bridge
{
    public class NativeBridge : MonoBehaviour
    {
        public static NativeBridge Instance { get; private set; }

        private BridgeProvider provider;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            Initialize();
        }

        private void Initialize()
        {
            provider = new BridgeProvider();

            AddListener(this);
        }

        private void OnDestroy()
        {
            RemoveListener(this);
        }

        public void AddListener(MonoBehaviour mono)
        {
            provider.AddListener(mono);
        }

        public void RemoveListener(MonoBehaviour mono)
        {
            provider.RemoveListener(mono);
        }

        private void Update()
        {
            provider.Update();
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// (전달할 데이터가 없는 경우 사용)
        /// </summary>
        /// <param name="evtName"></param>
        public void REQToNative(NativeBridgeEvent evtName)
        {
            provider.SendNative(evtName.ToString(), string.Empty);
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evtName"></param>
        /// <param name="data"></param>
        public void REQToNative<T>(NativeBridgeEvent evtName, T data)
        {
            provider.SendNative(evtName.ToString(), JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// (전달할 데이터가 없는 경우 사용)
        /// </summary>
        /// <param name="evtName"></param>
        public void ACKToNative(NativeBridgeEvent evtName)
        {
            provider.SendNative(evtName.ToString(), NativeBridgeResponse.CreateEmpty());
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evtName"></param>
        /// <param name="data"></param>
        public void ACKToNative<T>(NativeBridgeEvent evtName, T data)
        {
            provider.SendNative(evtName.ToString(), NativeBridgeResponse.Create(data));
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// (전달할 데이터가 없는 경우 사용)
        /// </summary>
        /// <param name="evtName"></param>
        public void NTYToNative(NativeBridgeEvent evtName)
        {
            provider.SendNative(evtName.ToString(), string.Empty);
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evtName"></param>
        /// <param name="data"></param>
        public void NTYToNative<T>(NativeBridgeEvent evtName, T data)
        {
            provider.SendNative(evtName.ToString(), JsonUtility.ToJson(data));
        }

        /// <summary>
        /// Native로 인터페이스를 전송합니다.
        /// (데이터가 JSON String 일 경우 사용)
        /// </summary>
        /// <param name="evtName"></param>
        /// <param name="jsonData"></param>
        public void NTYToNative(NativeBridgeEvent evtName, string jsonData)
        {
            provider.SendNative(evtName.ToString(), jsonData);
        }

        /// <summary>
        /// Native로 실패 응답을 전송합니다.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="scene"></param>
        /// <param name="e"></param>
        public void ErrorACKToNative(NativeBridgeEvent eventName, Enum e)
        {
            try
            {
                Type enumType = e.GetType();
                string enumString = e.ToString();

                string error = ((int)Enum.Parse(enumType, enumString)).ToString("D2");

                string errorCode = $"{error}";
                string errorMsg = $"{enumType.Name}.{enumString}";

                provider.SendNative(eventName.ToString(), NativeBridgeResponse.Error(errorCode, errorMsg));
            }
            catch (Exception ex)
            {
                string eMsg = (e != null) ? e.ToString() : "";
                Debug.LogError($"{eventName} / {eMsg}");
                Debug.LogException(ex);
            }
        }
    }
}