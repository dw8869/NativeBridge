namespace Core.Bridge
{
    internal interface INativeBridge
    {
        void Initialize(BridgeProvider.MessageCallback callback);
        void CallNative(string eventName, string jsonData);
    }
}