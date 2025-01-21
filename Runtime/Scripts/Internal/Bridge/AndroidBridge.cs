using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Core.Bridge
{
    internal class AndroidBridge : INativeBridge
    {
        private const string LIB_NAME = "unity_native_msg";

        private readonly object lockObject = new object();

        private Thread sendThread;
        private IntPtr jClass = IntPtr.Zero;
        private IntPtr jMethodID = IntPtr.Zero;

        [DllImport(LIB_NAME)]
        private static extern void RegisterUnityMethod([MarshalAs(UnmanagedType.FunctionPtr)] BridgeProvider.MessageCallback callBack);

        void INativeBridge.Initialize(BridgeProvider.MessageCallback callback)
        {
            RegisterUnityMethod(callback);
        }

        void INativeBridge.CallNative(string eventName, string jsonData)
        {
            sendThread = new Thread(() => Internal_CallNative(eventName, jsonData));
            sendThread.Start();
            sendThread.Join();
        }

        private void Internal_CallNative(string eventName, string jsonData)
        {
            try
            {
                lock (lockObject)
                {
                    if (AndroidJNI.AttachCurrentThread() == 0)
                    {
                        if (jClass == IntPtr.Zero)
                            jClass = AndroidJNI.NewGlobalRef(AndroidJNI.FindClass("com/unity/unitymsg/UnityMsg"));
                        if (jMethodID == IntPtr.Zero)
                            jMethodID = AndroidJNI.GetStaticMethodID(jClass, "CallNative", "(Ljava/lang/String;Ljava/lang/String;)V");

                        if (jClass == IntPtr.Zero || jMethodID == IntPtr.Zero)
                        {
                            Debug.LogWarning("CallNative : jClass or jMethodID is null");
                        }
                        else
                        {
                            jvalue[] jvalues = new jvalue[2];
                            jvalues[0].l = AndroidJNI.NewStringUTF(eventName);
                            jvalues[1].l = AndroidJNI.NewStringUTF(jsonData);
                            AndroidJNI.CallStaticVoidMethod(jClass, jMethodID, jvalues);
                        }

                        AndroidJNI.DetachCurrentThread();
                    }
                    else
                    {
                        Debug.LogWarning("CallNative : AndroidJNI Can't AttachCurrentThread");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Thread.CurrentThread.Abort();
                sendThread = null;
            }
        }
    }
}