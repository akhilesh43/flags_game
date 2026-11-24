using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LovMates
{
    /// <summary>
    /// Entry point. Boots itself in ANY scene (RuntimeInitializeOnLoadMethod), so just press Play.
    /// Owns save data, screen navigation, currency, rewards, toast.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }
        public SaveData Data;
        public RectTransform Root;
        public ScreenId Current { get; private set; }
        public bool Returning { get; private set; }
        public event Action OnCurrencyChanged;

        readonly Dictionary<ScreenId, ScreenBase> screens = new Dictionary<ScreenId, ScreenBase>();
        readonly Stack<ScreenId> history = new Stack<ScreenId>();
        readonly Dictionary<string, float> cooldowns = new Dictionary<string, float>();
        static readonly HashSet<ScreenId> NoHistory = new HashSet<ScreenId> {
            ScreenId.Splash, ScreenId.Loading, ScreenId.Welcome, ScreenId.Login, ScreenId.Register, ScreenId.Forgot,
            ScreenId.BoySelect, ScreenId.GirlSelect, ScreenId.Customize, ScreenId.CoupleConnect, ScreenId.End };

        Image toastBox; Text toastText; Coroutine toastCo; AudioSource sfx, bgm; string bgmName; bool bgmPaused; float lastCoinSfx;

        public string Today { get { return DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); } }
        public CharacterLook MyLook { get { return Data.myGender == 0 ? Data.boy : Data.girl; } }
        public CharacterLook PartnerLook { get { return Data.myGender == 0 ? Data.girl : Data.boy; } }
        public int MyGender { get { return Data.myGender; } }
        public int PartnerGender { get { return 1 - Data.myGender; } }
        public string GalleryDir { get { return Path.Combine(Application.persistentDataPath, "Gallery"); } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (I != null) return;
            var go = new GameObject("LovMates");
            DontDestroyOnLoad(go);
            go.AddComponent<GameManager>();
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            Data = SaveSystem.Load();

            EnsureCamera();
            EnsureEventSystem();
            BuildCanvas();
            BuildToast();
            sfx = gameObject.AddComponent<AudioSource>();
            bgm = gameObject.AddComponent<AudioSource>(); bgm.loop = true; bgm.volume = 0.35f; bgm.playOnAwake = false;
            Missions.EnsureToday();
            ApplySettings();
            BuildScreens();
            InvokeRepeating("Save", 30f, 30f);
            Go(ScreenId.Splash, false);
        }

        // ---------------- bootstrap helpers ----------------
        void EnsureCamera()
        {
            if (Camera.allCamerasCount > 0) return;
            var c = new GameObject("Camera").AddComponent<Camera>();
            c.transform.SetParent(transform);
            c.clearFlags = CameraClearFlags.SolidColor;
            c.backgroundColor = new Color(0.03f, 0.25f, 0.06f);
            c.gameObject.AddComponent<AudioListener>();
        }

        void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
            es.transform.SetParent(transform);
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var m = es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            m.AssignDefaultActions();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        void BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080, 1920);
            sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            sc.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            Root = (RectTransform)go.transform;
        }

        void BuildToast()
        {
            toastBox = UI.Box(Root, "Toast", 0.08f, 0.86f, 0.92f, 0.92f, new Color(0f, 0.1f, 0.02f, 0.92f));
            toastBox.raycastTarget = false;
            toastText = UI.Label(toastBox.transform, "", 0.03f, 0f, 0.97f, 1f, 38, UI.White);
            toastBox.gameObject.SetActive(false);
        }

        void Add<T>(ScreenId id, Action<T> cfg = null) where T : ScreenBase
        {
            var go = new GameObject(id.ToString(), typeof(RectTransform));
            go.transform.SetParent(Root, false);
            var s = go.AddComponent<T>();
            s.Id = id;
            if (cfg != null) cfg(s);
            s.Init();
            go.SetActive(false);
            screens[id] = s;
        }

        void BuildScreens()
        {
            Add<SplashScreen>(ScreenId.Splash);
            Add<WelcomeScreen>(ScreenId.Welcome);
            Add<LoginScreen>(ScreenId.Login);
            Add<RegisterScreen>(ScreenId.Register);
            Add<ForgotScreen>(ScreenId.Forgot);
            Add<CharacterSelectScreen>(ScreenId.BoySelect, s => s.gender = 0);
            Add<CharacterSelectScreen>(ScreenId.GirlSelect, s => s.gender = 1);
            Add<CustomizeScreen>(ScreenId.Customize);
            Add<CoupleConnectScreen>(ScreenId.CoupleConnect);
            Add<LoadingScreen>(ScreenId.Loading);
            Add<HomeScreen>(ScreenId.Home);

            // ---- Locations (screens 12-20): each has a LIVE "Enter" button ----
            Add<LocationScreen>(ScreenId.OurHome, s => s.Setup("Our Home", "Your cozy love nest", new Color(0.2f, 0.55f, 0.35f),
                ("Bedroom", () => Go(ScreenId.Bedroom)), ("Living Room", () => Go(ScreenId.LivingRoom)), ("Kitchen", () => Go(ScreenId.Kitchen))));
            Add<LocationScreen>(ScreenId.Bedroom, s => s.Setup("Bedroom", "Rest and dream together", new Color(0.55f, 0.35f, 0.3f),
                ("Decorate", () => Go(ScreenId.Decor)), ("Love Diary", () => Go(ScreenId.Diary))));
            Add<LocationScreen>(ScreenId.LivingRoom, s => s.Setup("Living Room", "Movie nights and cuddles", new Color(0.35f, 0.4f, 0.55f),
                ("Decorate", () => Go(ScreenId.Decor)), ("Watch Movie", () => TryReward("movie", 30, 30, 10, 10, "Movie night! +30 coins"))));
            Add<LocationScreen>(ScreenId.Kitchen, s => s.Setup("Kitchen", "Cook something sweet", new Color(0.6f, 0.5f, 0.3f),
                ("Cook Together", () => TryReward("cook", 20, 25, 8, 10, "Cooked together! +25 coins"))));
            Add<LocationScreen>(ScreenId.Garden, s => s.Setup("Garden", "Flowers, gazebo and sunshine", new Color(0.2f, 0.6f, 0.2f),
                ("Plant Flowers", () => TryReward("plant", 20, 20, 8, 8, "Flowers planted! +20 coins"))));
            Add<LocationScreen>(ScreenId.LovePark, s => s.Setup("Love Park", "Ferris wheel and fun", new Color(0.3f, 0.6f, 0.3f),
                ("Ferris Wheel", () => TryReward("ferris", 40, 40, 12, 12, "What a view! +40 coins"))));
            Add<LocationScreen>(ScreenId.Beach, s => s.Setup("Beach", "Sun, sand and sea", new Color(0.2f, 0.55f, 0.65f),
                ("Take Photo", () => StartCoroutine(CapturePhoto())), ("Swim", () => TryReward("swim", 30, 30, 10, 10, "Splash! +30 coins"))));
            Add<LocationScreen>(ScreenId.LoveCity, s => s.Setup("Love City", "Lights, shops and nightlife", new Color(0.4f, 0.3f, 0.6f),
                ("Shopping", () => Go(ScreenId.GiftShop))));
            Add<LocationScreen>(ScreenId.Cafe, s => s.Setup("Cafe & Restaurant", "Coffee, dinner and dates", new Color(0.55f, 0.4f, 0.25f),
                ("Romantic Date", () => Go(ScreenId.RomanticDate))));

            Add<RomanticDateScreen>(ScreenId.RomanticDate);
            Add<InteractionScreen>(ScreenId.Interaction);
            Add<HugKissScreen>(ScreenId.HugKiss);
            Add<DanceScreen>(ScreenId.Dance);
            Add<PetScreen>(ScreenId.Pet);
            Add<CatalogScreen>(ScreenId.GiftShop, s => s.Setup(CatalogMode.Shop, "Gift Shop", Items.Gifts));
            Add<CatalogScreen>(ScreenId.Wardrobe, s => s.Setup(CatalogMode.Wardrobe, "Wardrobe", Items.Clothes));
            Add<CatalogScreen>(ScreenId.Decor, s => s.Setup(CatalogMode.Decor, "Room Decoration", Items.Decor));
            Add<DiaryScreen>(ScreenId.Diary);
            Add<GalleryScreen>(ScreenId.Gallery);
            Add<ChatScreen>(ScreenId.Chat);
            Add<LettersScreen>(ScreenId.Letters);
            Add<MoodScreen>(ScreenId.Mood);
            Add<GoalsScreen>(ScreenId.Goals);
            Add<SpecialDaysScreen>(ScreenId.SpecialDays);
            Add<MissionsScreen>(ScreenId.Missions);
            Add<SettingsScreen>(ScreenId.Settings);
            Add<PauseScreen>(ScreenId.Pause);
            Add<EventScreen>(ScreenId.Event);
            Add<EndScreen>(ScreenId.End);
        }

        public T Get<T>(ScreenId id) where T : ScreenBase { return (T)screens[id]; }

        // ---------------- navigation ----------------
        public void Go(ScreenId id, bool push = true) { Navigate(id, push, false); }

        public void Back()
        {
            if (history.Count > 0) Navigate(history.Pop(), false, true);
            else Navigate(ScreenId.Home, false, true);
        }

        public void ClearHistory() { history.Clear(); }
        public void GoHome() { history.Clear(); Go(ScreenId.Home, false); }

        void Navigate(ScreenId id, bool push, bool returning)
        {
            if (Current != ScreenId.None)
            {
                var c = screens[Current];
                c.OnHide();
                c.gameObject.SetActive(false);
                if (push && !NoHistory.Contains(Current)) history.Push(Current);
            }
            Returning = returning;
            Current = id;
            var s = screens[id];
            s.gameObject.SetActive(true);
            s.transform.SetAsLastSibling();
            if (toastBox != null) toastBox.transform.SetAsLastSibling();
            s.OnShow();
            if (id != ScreenId.Splash) Sfx("whoosh");
            UpdateMusic(id);
        }

        // ---------------- flow ----------------
        public void StartGameFlow()
        {
            if (string.IsNullOrEmpty(Data.currentEmail)) Go(ScreenId.Login, false);
            else AfterAuth();
        }

        public void AfterAuth()
        {
            Save();
            if (!Data.characterCreated) Go(ScreenId.BoySelect, false);
            else Go(ScreenId.Loading, false);
        }

        // ---------------- economy ----------------
        void Changed() { if (OnCurrencyChanged != null) OnCurrencyChanged(); }
        public void AddCoins(int n) { Data.coins = Mathf.Max(0, Data.coins + n); if (n > 0 && Time.unscaledTime - lastCoinSfx > 0.4f) { lastCoinSfx = Time.unscaledTime; Sfx("coin"); } Changed(); }
        public void AddGems(int n) { Data.gems = Mathf.Max(0, Data.gems + n); Changed(); }
        public void AddAffection(int n) { Data.affection += n; Changed(); }

        public bool SpendCoins(int n)
        {
            if (Data.coins < n) { Toast("Not enough coins"); return false; }
            Data.coins -= n; Changed(); return true;
        }

        public void AddXp(int n)
        {
            Data.xp += n;
            while (Data.xp >= Data.level * 100)
            {
                Data.xp -= Data.level * 100;
                Data.level++;
                Data.coins += 100;
                UI.Message("Level Up!", "You reached level " + Data.level + "\n+100 coins");
            }
            Changed();
        }

        public bool TryReward(string key, float cooldown, int coins, int aff, int xp, string msg)
        {
            float now = Time.realtimeSinceStartup, until;
            if (cooldowns.TryGetValue(key, out until) && now < until)
            {
                Toast("Wait " + Mathf.CeilToInt(until - now) + "s");
                return false;
            }
            cooldowns[key] = now + cooldown;
            AddCoins(coins); AddAffection(aff); AddXp(xp);
            Toast(msg);
            return true;
        }

        // ---------------- photo ----------------
        public IEnumerator CapturePhoto()
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            string id = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            Directory.CreateDirectory(GalleryDir);
            File.WriteAllBytes(Path.Combine(GalleryDir, id + ".png"), tex.EncodeToPNG());
            int tw = 270, th = 480;
            var thumb = new Texture2D(tw, th, TextureFormat.RGB24, false);
            for (int y = 0; y < th; y++)
                for (int x = 0; x < tw; x++)
                    thumb.SetPixel(x, y, tex.GetPixelBilinear(x / (float)tw, y / (float)th));
            thumb.Apply();
            File.WriteAllBytes(Path.Combine(GalleryDir, "th_" + id + ".png"), thumb.EncodeToPNG());
            Destroy(tex); Destroy(thumb);
            Data.gallery.Add(id);
            Missions.Progress("photo");
            AddXp(5);
            Toast("Photo saved to gallery");
            Save();
        }

        // ---------------- misc ----------------
        public void Toast(string msg)
        {
            if (toastBox == null) return;
            toastText.text = msg;
            toastBox.gameObject.SetActive(true);
            toastBox.transform.SetAsLastSibling();
            if (toastCo != null) StopCoroutine(toastCo);
            toastCo = StartCoroutine(HideToast());
        }

        IEnumerator HideToast()
        {
            yield return new WaitForSeconds(2.2f);
            toastBox.gameObject.SetActive(false);
        }

        public void Click()
        {
            if (!Data.sound) return;
            var c = Art.Clip("click");
            if (c != null) sfx.PlayOneShot(c);
        }

        /// <summary>Play a one-shot sound effect from Resources/Audio (coin, success, error, heart, kiss, message, whoosh...).</summary>
        public void Sfx(string name, float vol = 1f)
        {
            if (!Data.sound || sfx == null) return;
            var c = Art.Clip(name);
            if (c != null) sfx.PlayOneShot(c, vol);
        }

        public void PauseMusic(bool pause) { bgmPaused = pause; if (bgm == null) return; if (pause) bgm.Pause(); else bgm.UnPause(); }

        static readonly ScreenId[] RomanticScreens = { ScreenId.RomanticDate, ScreenId.Diary, ScreenId.Letters, ScreenId.SpecialDays, ScreenId.Event, ScreenId.End, ScreenId.CoupleConnect };

        void UpdateMusic(ScreenId id)
        {
            if (bgm == null) return;
            string want = System.Array.IndexOf(RomanticScreens, id) >= 0 ? "romantic" : "bgm";
            if (id == ScreenId.Dance) { return; }          // dance screen plays its own track
            if (bgmName == want && bgm.isPlaying) return;
            var c = Art.Clip(want);
            if (c == null) return;
            bgmName = want; bgm.clip = c; bgm.loop = true;
            if (!bgmPaused) bgm.Play();
        }

        public void ApplySettings()
        {
            AudioListener.volume = Data.sound ? 1f : 0f;
            QualitySettings.SetQualityLevel(Mathf.Clamp(Data.quality, 0, QualitySettings.names.Length - 1), true);
        }

        public void Save() { SaveSystem.Save(Data); }
        void OnApplicationPause(bool paused) { if (paused) Save(); }
        void OnApplicationQuit() { Save(); }

        public void Quit()
        {
            Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public static class Missions
    {
        static MissionState M(string key, string title, int reward)
        {
            return new MissionState { key = key, title = title, target = 1, reward = reward };
        }

        public static void EnsureToday()
        {
            var g = GameManager.I;
            var d = g.Data;
            if (d.missionsDay == g.Today && d.missions.Count > 0) return;
            d.missionsDay = g.Today;
            d.missions = new List<MissionState> {
                M("talk", "Talk to your partner", 20), M("photo", "Take a photo", 20), M("date", "Complete a date", 30),
                M("pet", "Play with your pet", 15), M("visit", "Visit a place", 15) };
        }

        public static void Progress(string key, int n = 1)
        {
            EnsureToday();
            foreach (var m in GameManager.I.Data.missions)
                if (m.key == key && m.progress < m.target)
                {
                    m.progress = Mathf.Min(m.target, m.progress + n);
                    if (m.progress >= m.target) GameManager.I.Toast("Mission complete: " + m.title);
                }
        }
    }
}
