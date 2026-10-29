
using UnityEngine;
using GorillaTagMod.Mods;

namespace GorillaTagMod.Menu
{
    public sealed class FeatureMenu : MonoBehaviour
    {
        private void Awake()
        {
            RegisterFeatures();
        }

        private void RegisterFeatures()
        {
            if (FeatureRegistry.All.Count > 0)
                return;

            string[] categories =
            {
                "Movement", "Platforms", "Visuals", "Menu",
                "Input", "Audio", "Performance", "Configuration",
                "Developer", "Safety", "Networking", "World",
                "Accessibility", "Notifications", "Profiles",
                "Diagnostics", "Testing", "Materials",
                "Utilities", "Maintenance"
            };

            foreach (string category in categories)
            {
                for (int i = 1; i <= 25; i++)
                {
                    FeatureRegistry.Register(new ModFeature(
                        category + " Feature " + i.ToString("D2"),
                        category,
                        "Catalog entry. Implementation pending."));
                }
            }

            Debug.Log(
                "Registered " + FeatureRegistry.All.Count +
                " feature catalog entries.");
        }
    }
}
