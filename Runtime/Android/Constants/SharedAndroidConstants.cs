using UnityEngine;

namespace Chartboost.Constants
{
    public class SharedAndroidConstants
    {
        public static AndroidJavaObject UnityPlayerCurrentActivity()
        {
            using var unityPlayer = new AndroidJavaClass(ClassUnityPlayer);
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>(PropertyCurrentActivity);
            return currentActivity;
        }

        public const string ClassUnityPlayer = "com.unity3d.player.UnityPlayer";
        public const string ClassString = "java.lang.String";
        public const string ClassBoolean = "java.lang.Boolean";
        public const string ClassInteger = "java.lang.Integer";
        public const string ClassHashMap = "java.util.HashMap";
        public const string ClassHashSet = "java.util.HashSet";
        public const string ClassArrayList = "java.util.ArrayList";
        
        public const string PropertyCurrentActivity = "currentActivity";
        
        public const string FunctionGetSharedPreferences = "getSharedPreferences";

        public const string FunctionSize = "size";
        public const string FunctionToString = "toString";
        public const string FunctionGetValue = "getValue";
        public const string FunctionNext = "next";
        public const string FunctionHasNext = "hasNext";
        public const string FunctionIterator = "iterator";
        public const string FunctionEntrySet = "entrySet";
        public const string FunctionGetKey = "getKey";
        public const string FunctionBooleanValue = "booleanValue";
        public const string FunctionDoubleValue = "doubleValue";
        public const string FunctionFloatValue = "floatValue";
        public const string FunctionIntValue = "intValue";
        public const string FunctionGetInt = "getInt";
        public const string FunctionPutInt = "putInt";
        public const string FunctionGetString = "getString";
        public const string FunctionPutString = "putString";
        public const string FunctionEdit = "edit";
        public const string FunctionApply = "apply";
        public const string FunctionHashCode = "hashCode";
        public const string FunctionStart = "start";
        public const string FunctionSetPreInitializationConfiguration = "setPreinitializationConfiguration";
        public const string FunctionGet = "get";
        public const string FunctionSet = "set";
        public const string FunctionPut = "put";
        public const string FunctionAdd = "add";
        
        public const string FunctionGetLogLevel = "getLogLevel";
        public const string FunctionSetLogLevel = "setLogLevel";  
        public const string FunctionGetTestMode = "getTestMode";
        public const string FunctionSetTestMode = "setTestMode";
        public const string FunctionGetAdapterVersion = "getAdapterVersion";
        public const string FunctionGetPartnerSdkVersion = "getPartnerSdkVersion";
        public const string FunctionGetPartnerId = "getPartnerId";
        public const string FunctionGetPartnerDisplayName = "getPartnerDisplayName";
        public const string FunctionGetVerboseLoggingEnabled = "getVerboseLoggingEnabled";
        public const string FunctionSetVerboseLoggingEnabled = "setVerboseLoggingEnabled";
        public const string FunctionGetTestDeviceIds = "getTestDeviceIds";
        public const string FunctionSetTestDeviceIds = "setTestDeviceIds";
    }
}
