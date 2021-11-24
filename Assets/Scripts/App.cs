using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Geo
{
    /// <summary>
    /// Class responsible for initializing the app at startup
    /// </summary>
    public static class App 
    {
        private static int TargetFrameRate = 50;

#if UNITY_EDITOR
        [UnityEditor.InitializeOnEnterPlayMode]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
        private static void Initialize()
        {
            Application.targetFrameRate = TargetFrameRate;
            Debug.Log("App initialized");
        }
    }
}
