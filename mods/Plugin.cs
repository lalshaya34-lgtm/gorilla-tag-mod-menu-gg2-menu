
using BepInEx;
using UnityEngine;
using GorillaTagMod.Menu;
using GorillaTagMod.Mods;

namespace GorillaTagMod
{
    [BepInPlugin(
        "com.example.gorillatag.modmenu",
        "Gorilla Tag Mod Menu",
        "1.0.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Gorilla Tag Mod Menu loaded.");

            gameObject.AddComponent<WristMenu>();
            gameObject.AddComponent<FeatureMenu>();
        }

        private void OnDestroy()
        {
            FeatureRegistry.DisableAll();
        }
    }
}
