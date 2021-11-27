using UnityEngine;
using CommandTerminal;
using UnityEngine.SceneManagement;

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



        /// <summary>
        /// Reloads the currently active scene
        /// </summary>
        [RegisterCommand(Help = "Reloads the currently active scene")]
        public static void Reload()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex, LoadSceneMode.Single);
        }

        private static void FrontCommandReload(CommandArg[] args)
        {
            if (Terminal.IssuedError) return;
            Reload();
        }
    }
}
