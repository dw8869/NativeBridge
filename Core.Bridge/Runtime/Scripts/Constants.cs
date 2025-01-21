using System;
using UnityEngine;

namespace Core.Bridge
{
    public class NativeBridgeResponse
    {
        [Serializable]
        private class Response<T>
        {
            public string errorCode;
            public string errorMsg;
            public T data;
        }

        [Serializable]
        private class Empty
        {

        }

        public static string Create<T>(T data)
        {
            Response<T> response = new Response<T>();
            response.errorCode = string.Empty;
            response.errorMsg = string.Empty;
            response.data = data;

            return JsonUtility.ToJson(response);
        }

        public static string CreateEmpty()
        {
            Response<Empty> response = new Response<Empty>();
            response.errorCode = string.Empty;
            response.errorMsg = string.Empty;
            response.data = null;

            return JsonUtility.ToJson(response);
        }

        public static string Error(string code, string msg)
        {
            Response<Empty> response = new Response<Empty>();
            response.errorCode = code;
            response.errorMsg = msg;
            response.data = null;

            return JsonUtility.ToJson(response);
        }
    }

    public enum NativeBridgeEvent
    {
        NONE,

        // Native -> Unity로 앱 시작 요청.
        N2U_REQ_ApplicationController_StartApplication,
        // Unity -> Native로 앱 시작 응답.
        U2N_ACK_ApplicationController_StartApplication,

        // Unity -> Native로 유저 데이터 요청.
        U2N_REQ_ApplicationController_UserData,
        // Native -> Unity로 유저 데이터 응답.
        N2U_ACK_ApplicationController_UserData,
    }
}