using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

// STUB so it compiles — real class comes from the game at runtime
namespace GorillaLocomotion
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;
        public Transform leftHandTransform, rightHandTransform;
        public float maxJumpSpeed = 6.5f, jumpMultiplier = 1.1f, slideMultiplier = 1f;
        public CapsuleCollider bodyCollider;
        public Rigidbody bodyRigidbody;
    }
}

namespace GG2Menu
{
    [BepInPlugin("com.gg2.menu", "gg2 menu", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        internal static ManualLogSource Log;
        internal static ConfigEntry<KeyboardShortcut> MenuKey;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            MenuKey = Config.Bind("General", "Menu Key", new KeyboardShortcut(KeyCode.Y), "Toggle gg2 menu");
            gameObject.AddComponent<ModManager>();
            gameObject.AddComponent<MenuUI>();
            Log.LogInfo($"gg2 menu loaded with {ModManager.Instance.Mods.Count} mods!");
        }
    }

    public abstract class Mod
    {
        public string Name, Category;
        public bool Enabled;
        protected Mod(string name, string cat) { Name = name; Category = cat; }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnUpdate() { }
        public virtual void OnClick() { }
    }

    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance;
        public List<Mod> Mods = new List<Mod>();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            // ===== MOVEMENT =====
            float[] speeds = { 1.2f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f, 15f, 20f, 30f, 50f, 100f, 200f, 500f };
            foreach (var s in speeds) Mods.Add(new ParamMod(ModType.Speed, s, $"Speed x{s}", "Movement"));
            float[] jumps = { 1.2f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 7f, 10f, 15f, 20f, 30f, 50f };
            foreach (var j in jumps) Mods.Add(new ParamMod(ModType.Jump, j, $"Jump x{j}", "Movement"));
            float[] gravs = { 0.05f, 0.1f, 0.2f, 0.3f, 0.5f, 0.75f, 1.5f, 2f, 3f, 5f, 10f, 20f };
            foreach (var g in gravs) Mods.Add(new ParamMod(ModType.Gravity, g, $"Gravity x{g}", "Movement"));
            float[] slides = { 0.25f, 0.5f, 1.5f, 2f, 3f, 5f, 8f, 10f, 20f, 50f };
            foreach (var s in slides) Mods.Add(new ParamMod(ModType.Slide, s, $"Slide x{s}", "Movement"));
            foreach (var f in new float[] { 2f, 5f, 10f, 20f, 50f, 100f }) Mods.Add(new FlyMod(f));
            foreach (var d in new float[] { 10f, 20f, 35f, 50f, 100f }) Mods.Add(new DashMod(d));

            // ===== BODY =====
            foreach (var f in new float[] { 70f, 80f, 90f, 100f, 110f, 120f, 130f, 150f, 179f })
                Mods.Add(new ParamMod(ModType.FOV, f, $"FOV {f}", "Body"));
            foreach (var a in new float[] { 0.2f, 0.5f, 1.5f, 2f, 3f, 5f, 10f })
                Mods.Add(new ParamMod(ModType.HeadScale, a, $"Head Scale x{a}", "Body"));

            // ===== VISUAL =====
            foreach (var t in new[] { "Red", "Orange", "Yellow", "Lime", "Green", "Teal", "Cyan", "Blue", "Indigo", "Purple", "Magenta", "Pink", "White", "Gray", "Black", "Sepia", "Sunset", "Ocean", "Forest", "Toxic", "Blood", "Gold", "Neon Blue", "Neon Pink", "Neon Green", "Sky", "Mint", "Lavender", "Coral", "Crimson" })
                Mods.Add(new TintMod(t));

            // ===== EMOTES =====
            var emotes = new (string, KeyCode)[] {
                ("T-Pose", KeyCode.Alpha1), ("Backflip", KeyCode.Alpha2), ("Handstand", KeyCode.Alpha3),
                ("Sit", KeyCode.Alpha4), ("Dance Spin", KeyCode.Alpha5), ("Wave", KeyCode.Alpha6),
                ("Plank", KeyCode.Alpha7), ("Superman", KeyCode.Alpha8), ("Nae Nae", KeyCode.Alpha9),
                ("Floss", KeyCode.Alpha0), ("Dab", KeyCode.F1), ("Cartwheel", KeyCode.F2),
                ("Moonwalk", KeyCode.F3), ("Helicopter", KeyCode.F4), ("Breakdance", KeyCode.F5),
                ("Float", KeyCode.F6), ("Crawl", KeyCode.F7), ("Ragdoll", KeyCode.F8)
            };
            foreach (var (n, k) in emotes) Mods.Add(new EmoteMod(n, k));

            // ===== SOUNDBOARD =====
            var sounds = new (string, KeyCode, float)[] {
                ("Beep", KeyCode.Keypad0, 800f), ("Boop", KeyCode.Keypad1, 600f), ("Bloop", KeyCode.Keypad2, 400f),
                ("High Pitch", KeyCode.Keypad3, 1500f), ("Low Pitch", KeyCode.Keypad4, 200f),
                ("Siren", KeyCode.Keypad5, 1000f), ("Alarm", KeyCode.Keypad6, 1200f),
                ("Ding", KeyCode.Keypad7, 900f), ("Dong", KeyCode.Keypad8, 500f), ("Chime", KeyCode.Keypad9, 700f)
            };
            foreach (var (n, k, f) in sounds) Mods.Add(new SoundMod(n, k, f));
            for (int i = 1; i <= 20; i++) Mods.Add(new SoundMod($"Tone {i}", KeyCode.None, 150f + i * 60f));

            // ===== WORLD =====
            foreach (var w in new[] { "Fog Dense", "Fog Light", "No Fog", "Blue Fog", "Red Fog", "Green Fog", "Dark Fog", "White Fog", "Sunrise", "Sunset", "Deep Ocean", "Alien World", "Lava World", "Ice World", "Desert", "Blood Moon", "Dream", "Void", "Neon" })
                Mods.Add(new WorldMod(w));

            // ===== FUN =====
            foreach (var t in new float[] { 0.1f, 0.25f, 0.5f, 0.75f, 1.25f, 1.5f, 2f, 3f, 5f })
                Mods.Add(new ParamMod(ModType.TimeScale, t, $"Time Scale x{t}", "Fun"));
        }

        public void Toggle(Mod mod)
        {
            mod.Enabled = !mod.Enabled;
            try { if (mod.Enabled) mod.OnEnable(); else mod.OnDisable(); }
            catch (Exception e) { Plugin.Log.LogError($"[{mod.Name}] {e.Message}"); mod.Enabled = false; }
        }

        public void DisableAll()
        {
            foreach (var m in Mods.Where(m => m.Enabled).ToList()) Toggle(m);
        }

        private void Update()
        {
            foreach (var m in Mods) if (m.Enabled) { try { m.OnUpdate(); } catch { } }
        }
    }

    public class MenuUI : MonoBehaviour
    {
        private bool open = true;
        private Rect win = new Rect(60, 40, 420, 500);
        private Vector2 scroll;
        private string search = "";
        private string category = "All";
        private GUIStyle btnStyle, searchStyle;

        private void Update()
        {
            if (Plugin.MenuKey.Value.IsDown()) open = !open;
        }

        private void OnGUI()
        {
            if (!open) return;
            if (btnStyle == null)
            {
                btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, alignment = TextAnchor.MiddleLeft };
                searchStyle = new GUIStyle(GUI.skin.textField) { fontSize = 14 };
            }
            win = GUI.Window(9999, win, Draw, "gg2 menu");
        }

        private void Draw(int id)
        {
            GUI.DragWindow(new Rect(0, 0, win.width, 30));
            search = GUI.TextField(new Rect(10, 32, win.width - 20, 26), search, searchStyle);

            GUILayout.Space(34);
            GUILayout.BeginHorizontal();
            foreach (var c in new[] { "All", "Movement", "Body", "Visual", "Emotes", "Soundboard", "World", "Fun" })
            {
                var old = GUI.backgroundColor;
                GUI.backgroundColor = category == c ? Color.cyan : Color.gray;
                if (GUILayout.Button(c, GUILayout.Height(24))) category = c;
                GUI.backgroundColor = old;
            }
            GUILayout.EndHorizontal();

            scroll = GUILayout.BeginScrollView(scroll);
            foreach (var mod in ModManager.Instance.Mods)
            {
                if (category != "All" && mod.Category != category) continue;
                if (search != "" && !mod.Name.ToLower().Contains(search.ToLower())) continue;

                var old = GUI.backgroundColor;
                GUI.backgroundColor = mod.Enabled ? Color.green : Color.gray;
                if (GUILayout.Button(mod.Name + (mod.Enabled ? " [ON]" : ""), btnStyle, GUILayout.Height(28)))
                {
                    if (mod.Category == "Soundboard") mod.OnClick();
                    else ModManager.Instance.Toggle(mod);
                }
                GUI.backgroundColor = old;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disable All", GUILayout.Height(28))) ModManager.Instance.DisableAll();
            if (GUILayout.Button("Close", GUILayout.Width(90), GUILayout.Height(28))) open = false;
            GUILayout.EndHorizontal();
        }
    }

    public enum ModType { Speed, Jump, Gravity, FOV, HeadScale, Slide, TimeScale }

    public class ParamMod : Mod
    {
        private ModType type;
        private float value, original;

        public ParamMod(ModType t, float v, string name, string cat) : base(name, cat) { type = t; value = v; }

        public override void OnEnable()
        {
            var p = GorillaLocomotion.Player.Instance;
            switch (type)
            {
                case ModType.Speed: if (p != null) { original = p.jumpMultiplier; p.jumpMultiplier = original * value; } break;
                case ModType.Jump: if (p != null) { original = p.maxJumpSpeed; p.maxJumpSpeed = original * value; } break;
                case ModType.Slide: if (p != null) { original = p.slideMultiplier; p.slideMultiplier = value; } break;
                case ModType.Gravity: Physics.gravity = Vector3.down * 9.81f * value; break;
                case ModType.FOV: if (Camera.main != null) { original = Camera.main.fieldOfView; Camera.main.fieldOfView = value; } break;
                case ModType.TimeScale: Time.timeScale = value; break;
                case ModType.HeadScale: if (p?.bodyCollider != null) { original = p.bodyCollider.radius; p.bodyCollider.radius = original * value; } break;
            }
        }

        public override void OnDisable()
        {
            var p = GorillaLocomotion.Player.Instance;
            switch (type)
            {
                case ModType.Speed: if (p != null) p.jumpMultiplier = original; break;
                case ModType.Jump: if (p != null) p.maxJumpSpeed = original; break;
                case ModType.Slide: if (p != null) p.slideMultiplier = original; break;
                case ModType.Gravity: Physics.gravity = Vector3.down * 9.81f; break;
                case ModType.FOV: if (Camera.main != null) Camera.main.fieldOfView = original; break;
                case ModType.TimeScale: Time.timeScale = 1f; break;
                case ModType.HeadScale: if (p?.bodyCollider != null) p.bodyCollider.radius = original; break;
            }
        }
    }

    public class FlyMod : Mod
    {
        private float speed;
        public FlyMod(float s) : base($"Fly {s}m/s (Space/Shift)", "Movement") { speed = s; }
        public override void OnUpdate()
        {
            var p = GorillaLocomotion.Player.Instance;
            if (p?.bodyRigidbody == null || Camera.main == null) return;
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.Space)) dir += Camera.main.transform.forward;
            if (Input.GetKey(KeyCode.LeftShift)) dir -= Camera.main.transform.forward;
            if (dir != Vector3.zero) p.bodyRigidbody.velocity = dir * speed;
        }
    }

    public class DashMod : Mod
    {
        private float power;
        public DashMod(float p) : base($"Dash {p} (press F)", "Movement") { power = p; }
        public override void OnUpdate()
        {
            var p = GorillaLocomotion.Player.Instance;
            if (p?.bodyRigidbody == null || Camera.main == null) return;
            if (Input.GetKeyDown(KeyCode.F)) p.bodyRigidbody.velocity = Camera.main.transform.forward * power;
        }
    }

    public class TintMod : Mod
    {
        private Color c;
        public TintMod(string name) : base($"Tint: {name}", "Visual")
        {
            c = name switch
            {
                "Red" => Color.red, "Orange" => new Color(1f, .5f, 0f), "Yellow" => Color.yellow,
                "Lime" => new Color(.7f, 1f, 0f), "Green" => Color.green, "Teal" => new Color(0f, .5f, .5f),
                "Cyan" => Color.cyan, "Blue" => Color.blue, "Indigo" => new Color(.3f, 0f, .5f),
                "Purple" => new Color(.5f, 0f, .5f), "Magenta" => Color.magenta, "Pink" => new Color(1f, .4f, .7f),
                "White" => Color.white, "Gray" => Color.gray, "Black" => Color.black,
                "Sepia" => new Color(.7f, .6f, .4f), "Sunset" => new Color(1f, .4f, .2f),
                "Ocean" => new Color(0f, .3f, .6f), "Forest" => new Color(.1f, .4f, .1f),
                "Toxic" => new Color(.4f, 1f, 0f), "Blood" => new Color(.5f, 0f, 0f), "Gold" => new Color(1f, .8f, 0f),
                "Neon Blue" => new Color(0f, .5f, 1f), "Neon Pink" => new Color(1f, 0f, .5f), "Neon Green" => new Color(0f, 1f, .3f),
                "Sky" => new Color(.5f, .8f, 1f), "Mint" => new Color(.6f, 1f, .8f), "Lavender" => new Color(.8f, .7f, 1f),
                "Coral" => new Color(1f, .6f, .5f), "Crimson" => new Color(.8f, .1f, .2f),
                _ => Color.gray
            };
        }
        public override void OnEnable() { RenderSettings.ambientLight = c; }
        public override void OnDisable() { RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f); }
    }

    public class EmoteMod : Mod
    {
        private KeyCode key;
        private string pose;
        public EmoteMod(string pose, KeyCode k) : base($"Emote: {pose} (hold {k})", "Emotes") { this.pose = pose; key = k; }
        public override void OnUpdate()
        {
            var p = GorillaLocomotion.Player.Instance;
            if (p?.bodyRigidbody == null || !Input.GetKey(key)) return;
            var rig = p.bodyRigidbody;
            switch (pose)
            {
                case "T-Pose": case "Sit": case "Wave": case "Plank": rig.velocity = Vector3.zero; break;
                case "Backflip": rig.velocity += Vector3.up * 6f * Time.deltaTime; break;
                case "Handstand": rig.velocity = Vector3.up * 2f; break;
                case "Dance Spin": rig.angularVelocity = Vector3.up * 5f; break;
                case "Superman": rig.velocity += Camera.main.transform.forward * 8f * Time.deltaTime; break;
                case "Nae Nae": rig.velocity = new Vector3(Mathf.Sin(Time.time * 8f) * 2f, 0f, 0f); break;
                case "Floss": rig.velocity = new Vector3(Mathf.Sin(Time.time * 6f) * 3f, Mathf.Sin(Time.time * 12f), 0f); break;
                case "Dab": rig.velocity += Vector3.right * 4f * Time.deltaTime; break;
                case "Cartwheel": rig.angularVelocity = new Vector3(0f, 0f, 4f); break;
                case "Moonwalk": rig.velocity += -Camera.main.transform.forward * 3f * Time.deltaTime; break;
                case "Helicopter": rig.angularVelocity = Vector3.up * 12f; break;
                case "Breakdance": rig.angularVelocity = new Vector3(3f, 5f, 0f); break;
                case "Float": rig.velocity = new Vector3(0f, Mathf.Sin(Time.time * 2f), 0f); break;
                case "Crawl": rig.velocity *= 0.3f; break;
                case "Ragdoll": rig.velocity += Vector3.down * 4f * Time.deltaTime; break;
            }
        }
    }

    public class SoundMod : Mod
    {
        private KeyCode key;
        private float freq;
        private string label;
        private AudioSource source;

        public SoundMod(string name, KeyCode k, float f) : base("♪ " + name + (k != KeyCode.None ? $" ({k})" : " (click)"), "Soundboard")
        { label = name; key = k; freq = f; }

        public void Play()
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            var clip = AudioClip.Create(label, 44100, 1, 44100, false);
            var data = new float[44100];
            for (int i = 0; i < 44100; i++)
                data[i] = Mathf.Sin(i * freq * Mathf.PI / 44100f) * 0.4f * Mathf.Lerp(1f, 0f, (float)i / 44100f);
            clip.SetData(data, 0);
            source.clip = clip;
            source.Play();
        }

        public override void OnClick() => Play();
        public override void OnUpdate() { if (key != KeyCode.None && Input.GetKeyDown(key)) Play(); }
    }

    public class WorldMod : Mod
    {
        private string effect;
        public WorldMod(string e) : base($"World: {e}", "World") { effect = e; }

        public override void OnEnable()
        {
            switch (effect)
            {
                case "Fog Dense": RenderSettings.fog = true; RenderSettings.fogDensity = 0.08f; break;
                case "Fog Light": RenderSettings.fog = true; RenderSettings.fogDensity = 0.01f; break;
                case "No Fog": RenderSettings.fog = false; break;
                case "Blue Fog": RenderSettings.fog = true; RenderSettings.fogColor = Color.blue; break;
                case "Red Fog": RenderSettings.fog = true; RenderSettings.fogColor = Color.red; break;
                case "Green Fog": RenderSettings.fog = true; RenderSettings.fogColor = Color.green; break;
                case "Dark Fog": RenderSettings.fog = true; RenderSettings.fogColor = Color.black; break;
                case "White Fog": RenderSettings.fog = true; RenderSettings.fogColor = Color.white; break;
                case "Sunrise": RenderSettings.ambientLight = new Color(1f, .6f, .3f); break;
                case "Sunset": RenderSettings.ambientLight = new Color(1f, .4f, .2f); break;
                case "Deep Ocean": RenderSettings.ambientLight = new Color(0f, .1f, .3f); break;
                case "Alien World": RenderSettings.ambientLight = new Color(.4f, 1f, .2f); break;
                case "Lava World": RenderSettings.ambientLight = new Color(1f, .2f, 0f); break;
                case "Ice World": RenderSettings.ambientLight = new Color(.7f, .9f, 1f); break;
                case "Desert": RenderSettings.ambientLight = new Color(1f, .9f, .6f); break;
                case "Blood Moon": RenderSettings.ambientLight = new Color(.6f, 0f, 0f); break;
                case "Dream": RenderSettings.ambientLight = new Color(.9f, .7f, 1f); break;
                case "Void": RenderSettings.ambientLight = new Color(.02f, .02f, .05f); break;
                case "Neon": RenderSettings.ambientLight = new Color(.2f, 1f, 1f); break;
            }
        }

        public override void OnDisable()
        {
            RenderSettings.fog = false;
            RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f);
            RenderSettings.fogColor = Color.gray;
            RenderSettings.fogDensity = 0.01f;
        }
    }
}
