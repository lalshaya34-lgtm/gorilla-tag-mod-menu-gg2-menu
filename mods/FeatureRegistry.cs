
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaTagMod.Mods
{
    public sealed class ModFeature
    {
        public string Name { get; }
        public string Category { get; }
        public string Description { get; }
        public bool Enabled { get; private set; }

        private readonly Action<bool> _toggle;

        public ModFeature(
            string name,
            string category,
            string description,
            Action<bool> toggle = null)
        {
            Name = name;
            Category = category;
            Description = description;
            _toggle = toggle;
        }

        public void SetEnabled(bool enabled)
        {
            if (Enabled == enabled)
                return;

            _toggle?.Invoke(enabled);
            Enabled = enabled;
        }
    }

    public static class FeatureRegistry
    {
        private static readonly List<ModFeature> Features =
            new List<ModFeature>();

        public static IReadOnlyList<ModFeature> All => Features;

        public static void Register(ModFeature feature)
        {
            if (feature == null)
                throw new ArgumentNullException(nameof(feature));

            if (Features.Any(f =>
                string.Equals(
                    f.Name,
                    feature.Name,
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Duplicate feature: " + feature.Name);
            }

            Features.Add(feature);
        }

        public static IEnumerable<ModFeature> Search(
            string query,
            string category = null)
        {
            IEnumerable<ModFeature> result = Features;

            if (!string.IsNullOrWhiteSpace(category))
                result = result.Where(f => f.Category == category);

            if (!string.IsNullOrWhiteSpace(query))
                result = result.Where(f =>
                    f.Name.IndexOf(
                        query,
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.Description.IndexOf(
                        query,
                        StringComparison.OrdinalIgnoreCase) >= 0);

            return result;
        }

        public static IReadOnlyList<ModFeature> GetPage(
            string query,
            string category,
            int page,
            int pageSize = 10)
        {
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return Search(query, category)
                .Skip(Math.Max(0, page) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public static void DisableAll()
        {
            foreach (ModFeature feature in Features.ToArray())
            {
                try
                {
                    feature.SetEnabled(false);
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"Could not disable {feature.Name}: {ex}");
                }
            }
        }
    }
}
