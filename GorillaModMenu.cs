using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

// ============================================================
// STUBS — fake classes so GitHub Actions compiles.
// Real classes load from the game at runtime.
// ============================================================
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

// ============================================================
// PLUGIN
// ============================================================
namespace GorillaModMenu
{
    [BepInPlugin("com.yourname.modmenu", "Gorilla Tag Mod Menu", "3.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        internal static ManualLogSource Log;
        internal static ConfigEntry<KeyboardShortcut> MenuKey;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            MenuKey = Config.Bind("General", "Menu Key",
                new KeyboardShortcut(KeyCode.Y), "Toggle menu");
            gameObject.AddComponent<ModManager>();
            gameObject.AddComponent<ModMenuUI>();
            gameObject.AddComponent<FPSOverlay>();
            Logger.LogInfo($"Mod Menu loaded with {ModManager.Instance.Mods.Count} mods!");
        }
    }

    // ============================================================
    // MOD BASE
    // ============================================================
    public abstract class Mod
    {
        public string Name;
        public string Category;
        public bool Enabled;
        protected Mod(string name, string category) { Name = name; Category = category; }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnUpdate() { }
        public virtual void OnClick() { } // for menu-button actions (soundboard etc.)
    }

    // ============================================================
    // MANAGER
    // ============================================================
    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance;
        public List<Mod> Mods = new List<Mod>();

        private void Awake()
        {
            Instance = this;
            BuildModList();
        }

        private void BuildModList()
        {
            // ================= MOVEMENT (140) =================
            float[] speeds = { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.75f, 1.9f, 2f, 2.2f, 2.4f, 2.6f, 2.8f, 3f, 3.25f, 3.5f, 3.75f, 4f, 4.5f, 5f, 5.5f, 6f, 7f, 8f, 9f, 10f, 12f, 14f, 16f, 18f, 20f, 25f, 30f, 40f, 50f, 75f, 100f, 150f, 200f, 500f };
            foreach (var s in speeds) Mods.Add(new ParamMod(ModType.Speed, s, $"Speed x{s}", "Movement"));

            float[] jumps = { 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.6f, 1.75f, 2f, 2.25f, 2.5f, 3f, 3.5f, 4f, 5f, 6f, 7f, 8f, 10f, 12f, 15f, 20f, 25f, 30f, 50f };
            foreach (var j in jumps) Mods.Add(new ParamMod(ModType.Jump, j, $"Jump x{j}", "Movement"));

            float[] gravs = { 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.25f, 0.2f, 0.15f, 0.1f, 0.075f, 0.05f, 0.025f, 0.01f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 7f, 10f, 15f, 20f, 30f, 50f };
            foreach (var g in gravs) Mods.Add(new ParamMod(ModType.Gravity, g, $"Gravity x{g}", "Movement"));

            float[] slides = { 1.25f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 6f, 8f, 10f, 15f, 20f, 30f, 50f, 0.75f, 0.5f, 0.25f, 0.1f };
            foreach (var s in slides) Mods.Add(new ParamMod(ModType.Slide, s, $"Slide x{s}", "Movement"));

            float[] flyspeeds = { 2f, 4f, 6f, 8f, 10f, 15f, 20f, 30f, 50f, 100f };
            foreach (var f in flyspeeds) Mods.Add(new FlyMod(f));

            float[] dashpowers = { 10f, 15f, 20f, 25f, 30f, 40f, 50f, 75f, 100f };
            foreach (var d in dashpowers) Mods.Add(new DashMod(d));

            string[] moveExtras = { "No Clip", "Wall Run", "Bouncy Floors", "Ice Skating", "Slow Fall", "Double Jump", "Grapple Swing", "Moon Jump", "Bounce Pads", "Freeze Self", "Teleport: Spawn", "Teleport: Forward 10m", "Teleport: Up 10m", "Teleport: Down 10m", "Teleport: Back 10m", "Save Position", "Load Position", "Time Stop", "Speed Ramp", "Air Control" };
            foreach (var n in moveExtras) Mods.Add(new SimpleMod(n, "Movement"));

            // ================= BODY (60) =================
            float[] fovs = { 60f, 65f, 70f, 75f, 80f, 85f, 90f, 95f, 100f, 105f, 110f, 115f, 120f, 125f, 130f, 135f, 140f, 145f, 150f, 160f, 170f, 179f };
            foreach (var f in fovs) Mods.Add(new ParamMod(ModType.FOV, f, $"FOV {f}", "Body"));

            float[] arms = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.75f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f, 5f, 7f, 10f, 15f };
            foreach (var a in arms) Mods.Add(new ParamMod(ModType.ArmLength, a, $"Arm Length {a}", "Body"));

            float[] heads = { 0.1f, 0.15f, 0.2f, 0.3f, 0.4f, 0.5f, 0.75f, 1.25f, 1.5f, 2f, 2.5f, 3f, 4f, 5f, 7f, 10f };
            foreach (var h in heads) Mods.Add(new ParamMod(ModType.HeadScale, h, $"Head Scale x{h}", "Body"));

            string[] bodyExtras = { "Small Player", "Big Player", "Tall Player", "Short Player", "Fast Hands", "Strong Grip", "Sticky Hands", "Player Glow", "Invisible Body", "Ghost Mode" };
            foreach (var n in bodyExtras) Mods.Add(new SimpleMod(n, "Body"));

            // ================= VISUAL (60) =================
            string[] tints = { "Red", "Orange", "Yellow", "Lime", "Green", "Teal", "Cyan", "Blue", "Indigo", "Purple", "Magenta", "Pink", "White", "Gray", "Black", "Sepia", "Sunset", "Sunrise", "Ocean", "Forest", "Candy", "Toxic", "Blood", "Gold", "Silver", "Neon Blue", "Neon Pink", "Neon Green", "Deep Purple", "Warm White", "Cool White", "Slate", "Rose", "Sky", "Mint", "Lavender", "Peach", "Coral", "Olive", "Maroon", "Navy", "Turquoise", "Beige", "Chocolate", "Crimson", "Salmon", "Khaki", "Plum", "Orchid", "Tomato", "Steel", "Tan", "Violet", "Azure", "Chartreuse", "Denim", "Fuchsia", "Jade", "Ruby", "Sapphire" };
            foreach (var t in tints) Mods.Add(new TintMod(t));

            // ================= EMOTES (40) =================
            var emotes = new (string, KeyCode)[] {
                ("T-Pose", KeyCode.Alpha1), ("Backflip", KeyCode.Alpha2), ("Handstand", KeyCode.Alpha3),
                ("Sit", KeyCode.Alpha4), ("Dance Spin", KeyCode.Alpha5), ("Wave", KeyCode.Alpha6),
                ("Plank", KeyCode.Alpha7), ("Superman", KeyCode.Alpha8), ("Nae Nae", KeyCode.Alpha9),
                ("Floss", KeyCode.Alpha0), ("Dab", KeyCode.F1), ("Cartwheel", KeyCode.F2),
                ("Moonwalk", KeyCode.F3), ("Helicopter", KeyCode.F4), ("Breakdance", KeyCode.F5),
                ("Float", KeyCode.F6), ("Crouch Walk", KeyCode.F7), ("Sprint", KeyCode.F8),
                ("Crawl", KeyCode.F9), ("Ragdoll", KeyCode.F10)
            };
            foreach (var (n, k) in emotes) Mods.Add(new EmoteMod(n, k));
            foreach (var (n, _) in emotes) Mods.Add(new SimpleMod($"Emote: {n} (auto-loop)", "Emotes"));

            // ================= SOUNDBOARD (50) =================
            var sounds = new (string, KeyCode, float)[] {
                ("Beep", KeyCode.Keypad0, 800f), ("Boop", KeyCode.Keypad1, 600f), ("Bloop", KeyCode.Keypad2, 400f),
                ("High Pitch", KeyCode.Keypad3, 1500f), ("Low Pitch", KeyCode.Keypad4, 200f),
                ("Siren", KeyCode.Keypad5, 1000f), ("Alarm", KeyCode.Keypad6, 1200f),
                ("Ding", KeyCode.Keypad7, 900f), ("Dong", KeyCode.Keypad8, 500f),
                ("Chime", KeyCode.Keypad9, 700f)
            };
            foreach (var (n, k, f) in sounds) Mods.Add(new SoundMod(n, k, f));
            for (int i = 1; i <= 40; i++)
                Mods.Add(new SoundMod($"Tone {i}", KeyCode.None, 150f + i * 40f));

            // ================= WORLD (20) =================
            string[] worlds = { "Fog Dense", "Fog Light", "No Fog", "Blue Fog", "Red Fog", "Green Fog", "Dark Fog", "White Fog", "Fog Fade In", "Sunrise", "Sunset", "Deep Ocean", "Alien World", "Lava World", "Ice World", "Desert", "Blood Moon", "Dream", "Void", "Neon" };
            foreach (var w in worlds) Mods.Add(new WorldMod(w));

            // ================= FUN (60) =================
            float[] timescales = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f, 1.75f, 2f, 2.5f, 3f, 4f, 5f };
            foreach (var t in timescales) Mods.Add(new ParamMod(ModType.TimeScale, t, $"Time Scale x{t}", "Fun"));

            float[] lights = { 0.5f, 1f, 2f, 3f, 5f, 8f, 10f, 15f, 20f, 30f, 50f, 100f };
            foreach (var l in lights) Mods.Add(new ParamMod(ModType.LightRange, l, $"Light Range {l}", "Fun"));

            string[] funExtras = { "Rainbow Player", "Rainbow Room", "Disco Lights", "Hand Trails", "Flashlight", "Night Mode", "Full Bright", "Blackout", "Matrix Mode", "Spin Cam", "Big Body Tag", "Nyan Mode", "Chaos Mode", "Upside Down Cam", "Drunk Mode", "Tiny World", "Head Bob x2", "Fisheye", "Bulge Cam", "Party Mode" };
            foreach (var n in funExtras) Mods.Add(new SimpleMod(n, "Fun"));

            // ================= UTILITY (30) =================
            string[] utils = { "FPS Counter", "Full Brightness", "Zoom Hold", "Performance Boost", "Screenshot Mode", "Hide HUD", "Show HUD", "Fix Camera", "Reset Mods", "Anti Crash", "Ping Display", "Player Counter", "Room Info", "Clean Memory", "Rejoin Room", "Copy Room Code", "Auto Quality Low", "Auto Quality High", "Shadow Off", "Shadow On", "Anti Alias Off", "Anti Alias On", "VSync Off", "VSync On", "Limit 60 FPS", "Limit 90 FPS", "Limit 120 FPS", "Disable All On Close", "Debug Log", "Menu Pos Reset" };
            foreach (var u in utils) Mods.Add(new SimpleMod(u, "Utility"));

            // ================= SPOOKY (25) =================
            string[] spooky = { "Dark Fog", "Blood Moon", "Void World", "Whisper Sounds", "Flicker Lights", "Slow Heartbeat", "Red Tint Pulse", "Blackout Strobe", "Ghost Float", "Eerie Ambience", "Creaking Floor", "Fog Wall", "Dread Aura", "Shadow Mode", "Silent Room", "Cold Breath", "Static Noise", "Nightmare Fog", "Creepy Tint", "Slow Creep", "Heartbeat Fast", "Dark Corners", "Eyes in Dark", "Haunted Mode", "Spooky Noises" };
            foreach (var s in spooky) Mods.Add(new SimpleMod(s, "Spooky"));
        }

        public void Toggle(Mod mod)
        {
            mod.Enabled = !mod.Enabled;
            try
            {
                if (mod.Enabled) mod.OnEnable(); else mod.OnDisable();
            }
            catch (Exception e) { Plugin.Log.LogError($"[{mod.Name}] {e.Message}"); mod.Enabled = false; }
        }

        private void Update()
        {
            foreach (var m in Mods) if (m.Enabled) { try { m.OnUpdate(); } catch { } }
        }
    }

    // ============================================================
    // MENU UI — searchable + category tabs + soundboard click
    // ============================================================
    public class ModMenuUI : MonoBehaviour
    {
        private bool open;
        private Rect win = new Rect(50, 30, 420, 520);
        private Vector2 scroll;
        private string search = "";
        private string category = "All";
        private GUIStyle btnStyle, searchStyle;

        private void Awake() { DontDestroyOnLoad(this); }

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
            win = GUI.Window(9999, win, Draw, "Gorilla Mod Menu ~490 Mods");
        }

        private void Draw(int id)
        {
            GUI.DragWindow(new Rect(0, 0, win.width, 30));
            search = GUI.TextField(new Rect(10, 32, win.width - 20, 26), search, searchStyle);

            GUILayout.Space(34);
            GUILayout.BeginHorizontal();
            foreach (var c in new[] { "All", "Movement", "Body", "Visual", "Emotes", "Soundboard", "World", "Fun", "Spooky", "Utility" })
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
                    if (mod.Category == "Soundboard") mod.OnClick(); // click = play
                    else ModManager.Instance.Toggle(mod);
                }
                GUI.backgroundColor = old;
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disable All", GUILayout.Height(28)))
                foreach (var m in ModManager.Instance.Mods.Where(m => m.Enabled).ToList())
                    ModManager.Instance.Toggle(m);
            if (GUILayout.Button("Close", GUILayout.Width(90), GUILayout.Height(28))) open = false;
            GUILayout.EndHorizontal();
        }
    }

    // ============================================================
    // GENERIC PARAMETER MOD ENGINE
    // ============================================================
    public enum ModType { Speed, Jump, Gravity, FOV, ArmLength, HeadScale, Slide, TimeScale, LightRange }

    public class ParamMod : Mod
    {
        private ModType type;
        private float value;
        private float original;

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
                case ModType.LightRange: foreach (var l in UnityEngine.Object.FindObjectsOfType<Light>()) l.range = value; break;
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
                case ModType.ArmLength: break;
            }
        }
    }

    // ============================================================
    // FLY
    // ============================================================
    public class FlyMod : Mod
    {
        private float speed;
        public FlyMod(float s) : base($"Fly {s}m/s", "Movement") { speed = s; }
        public override void OnUpdate()
        {
            var p = GorillaLocomotion.Player.Instance;
            var head = Camera.main != null ? Camera.main.transform : null;
            if (p?.bodyRigidbody == null || head == null) return;
            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.Space)) dir += head.forward;
            if (Input.GetKey(KeyCode.LeftShift)) dir -= head.forward;
            p.bodyRigidbody.velocity = dir * speed;
        }
    }

    // ============================================================
    // DASH
    // ============================================================
    public class DashMod : Mod
    {
        private float power;
        public DashMod(float p) : base($"Dash {p} (press F)", "Movement") { power = p; }
        public override void OnUpdate()
        {
            var p = GorillaLocomotion.Player.Instance;
            var head = Camera.main != null ? Camera.main.transform : null;
            if (p?.bodyRigidbody == null || head == null) return;
            if (Input.GetKeyDown(KeyCode.F)) p.bodyRigidbody.velocity = head.forward * power;
        }
    }

    // ============================================================
    // ROOM TINTS (60 colors)
    // ============================================================
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
                "Sepia" => new Color(.7f, .6f, .4f), "Sunset" => new Color(1f, .4f, .2f), "Sunrise" => new Color(1f, .6f, .3f),
                "Ocean" => new Color(0f, .3f, .6f), "Forest" => new Color(.1f, .4f, .1f), "Candy" => new Color(1f, .6f, .8f),
                "Toxic" => new Color(.4f, 1f, 0f), "Blood" => new Color(.5f, 0f, 0f), "Gold" => new Color(1f, .8f, 0f),
                "Silver" => new Color(.8f, .8f, .9f), "Neon Blue" => new Color(0f, .5f, 1f), "Neon Pink" => new Color(1f, 0f, .5f),
                "Neon Green" => new Color(0f, 1f, .3f), "Deep Purple" => new Color(.3f, 0f, .4f), "Warm White" => new Color(1f, .95f, .85f),
                "Cool White" => new Color(.85f, .9f, 1f), "Slate" => new Color(.3f, .35f, .4f), "Rose" => new Color(1f, .5f, .6f),
                "Sky" => new Color(.5f, .8f, 1f), "Mint" => new Color(.6f, 1f, .8f), "Lavender" => new Color(.8f, .7f, 1f),
                "Peach" => new Color(1f, .8f, .7f), "Coral" => new Color(1f, .6f, .5f), "Olive" => new Color(.5f, .5f, 0f),
                "Maroon" => new Color(.5f, 0f, 0f), "Navy" => new Color(0f, 0f, .5f), "Turquoise" => new Color(.2f, .9f, .8f),
                "Beige" => new Color(.9f, .85f, .7f), "Chocolate" => new Color(.4f, .25f, .1f), "Crimson" => new Color(.8f, .1f, .2f),
                "Salmon" => new Color(1f, .7f, .6f), "Khaki" => new Color(.8f, .8f, .5f), "Plum" => new Color(.6f, .2f, .5f),
                "Orchid" => new Color(.9f, .5f, .9f), "Tomato" => new Color(1f, .3f, .2f), "Steel" => new Color(.5f, .6f, .7f),
                "Tan" => new Color(.85f, .7f, .5f), "Violet" => new Color(.6f, .3f, .9f), "Azure" => new Color(.2f, .7f, 1f),
                "Chartreuse" => new Color(.8f, 1f, 0f), "Denim" => new Color(.2f, .4f, .7f), "Fuchsia" => new Color(1f, 0f, .8f),
                "Jade" => new Color(0f, .7f, .5f), "Ruby" => new Color(.9f, 0f, .2f), "Sapphire" => new Color(0f, .3f, .9f),
                _ => Color.gray
            };
        }
        public override void OnEnable() { RenderSettings.ambientLight = c; }
        public override void OnDisable() { RenderSettings.ambientLight = new Color(0.6f, 0.6f, 0.6f); }
    }

    // ============================================================
    // EMOTES — hold key while enabled
    // ============================================================
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
                case "T-Pose": rig.velocity = Vector3.zero; rig.angularVelocity = Vector3.zero; break;
                case "Backflip": rig.velocity += Vector3.up * 6f * Time.deltaTime; break;
                case "Handstand": rig.velocity = Vector3.up * 2f; break;
                case "Sit": case "Wave": case "Plank": rig.velocity = Vector3.zero; break;
                case "Dance Spin": rig.angularVelocity = Vector3.up * 5f; break;
                case "Superman": if (Camera.main != null) rig.velocity += Camera.main.transform.forward * 8f * Time.deltaTime; break;
                case "Nae Nae": rig.velocity = new Vector3(Mathf.Sin(Time.time * 8f) * 2f, 0f, 0f); break;
                case "Floss": rig.velocity = new Vector3(Mathf.Sin(Time.time * 6f) * 3f, Mathf.Sin(Time.time * 12f) * 1f, 0f); break;
                case "Dab": rig.velocity += Vector3.right * 4f * Time.deltaTime; break;
                case "Cartwheel": rig.angularVelocity = new Vector3(0f, 0f, 4f); break;
                case "Moonwalk": if (Camera.main != null) rig.velocity += -Camera.main.transform.forward * 3f * Time.deltaTime; break;
                case "Helicopter": rig.angularVelocity = Vector3.up * 12f; break;
                case "Breakdance": rig.angularVelocity = new Vector3(3f, 5f, 0f); break;
                case "Float": rig.velocity = new Vector3(0f, Mathf.Sin(Time.time * 2f) * 1f, 0f); break;
                case "Crouch Walk": rig.velocity *= 0.5f; break;
                case "Sprint": rig.velocity *= 1.05f; break;
                case "Crawl": rig.velocity *= 0.3f; break;
                case "Ragdoll": rig.velocity += Vector3.down * 4f * Time.deltaTime; break;
            }
        }
    }

    // ============================================================
    // SOUNDBOARD — click in menu or press key
    // ============================================================
    public class SoundMod : Mod
    {
        private KeyCode key;
        private float freq;
        private string label;
        private AudioSource source;

        public SoundMod(string name, KeyCode k, float frequency) : base($"♪ {name}" + (k != KeyCode.None ? $" ({k})" : " (click)"), "Soundboard")
        { label = name; key = k; freq = frequency; }

        public override void OnEnable()
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            if (key != KeyCode.None) Enabled = true; // keyboard sounds stay "armed"
        }

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

        public override void OnUpdate()
        {
            if (key != KeyCode.None && Input.GetKeyDown(key)) Play();
        }
    }

    // ============================================================
    // WORLD
    // ============================================================
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
                case "Fog Fade In": RenderSettings.fog = true; RenderSettings.fogDensity = 0.001f; break;
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

    // ============================================================
    // SIMPLE MOD — safe placeholder that logs when enabled.
    // Wire real behavior into OnEnable/OnUpdate as you go.
    // ============================================================
    public class SimpleMod : Mod
    {
        public SimpleMod(string name, string cat) : base(name, cat) { }
        public override void OnEnable() { Plugin.Log.LogInfo($"[SimpleMod] {Name} enabled"); }
        public override void OnDisable() { }
    }

    // ============================================================
    // FPS OVERLAY
    // ============================================================
    public class FPSOverlay : MonoBehaviour
    {
        private float t; private int frames; private float fps;
        private void Update()
        {
            frames++; t += Time.unscaledDeltaTime;
            if (t >= 1f) { fps = frames / t; frames = 0; t = 0; }
        }
        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 120, 30), "FPS: " + (int)fps);
        }
    }
}
