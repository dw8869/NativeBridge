#if UNITY_IOS
using System.Runtime.InteropServices;

namespace Core.Bridge
{
    internal class iOSBridge : INativeBridge
    {
        private const string LIB_NAME = "__Internal";

        [DllImport(LIB_NAME)]
        private static extern void RegisterUnityMethod([MarshalAs(UnmanagedType.FunctionPtr)] BridgeProvider.MessageCallback callBack);

        [DllImport(LIB_NAME)]
        private static extern void CallNative(string eventName, string jsonData);

        void INativeBridge.Initialize(BridgeProvider.MessageCallback callback)
        {
            RegisterUnityMethod(callback);
        }

        void INativeBridge.CallNative(string eventName, string jsonData)
        {
            CallNative(eventName, jsonData);
        }
    }
}
#endif