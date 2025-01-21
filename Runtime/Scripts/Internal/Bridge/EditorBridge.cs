namespace Core.Bridge
{
    internal class EditorBridge : INativeBridge
    {
        public static BridgeProvider.MessageCallback recvCallback;
        public static BridgeProvider.MessageCallback sendCallback;

        void INativeBridge.Initialize(BridgeProvider.MessageCallback callback)
        {
            recvCallback = callback;
        }

        void INativeBridge.CallNative(string eventName, string jsonData)
        {
            sendCallback?.Invoke(eventName, jsonData);
        }
    }
}