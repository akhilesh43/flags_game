using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LovMates
{
    public abstract class PairScreen : ScreenBase
    {
        protected AvatarView me, partner;

        protected void BuildPair(float ay = 0.6f, float w = 340, float h = 620)
        {
            partner = AvatarView.Create(R, "Partner", 0.5f, ay, w, h);
            me = AvatarView.Create(R, "Me", 0.5f, ay, w, h);
            me.rt.anchoredPosition = new Vector2(-170, 0);
            partner.rt.anchoredPosition = new Vector2(170, 0);
        }

        public override void OnShow()
        {
            me.Set(G.MyGender, G.MyLook);
            partner.Set(G.PartnerGender, G.PartnerLook);
        }

        protected void Float(string text, Color c)
        {
            float x = UnityEngine.Random.Range(0.3f, 0.7f);
            var t = UI.Label(R, text, x - 0.12f, 0.5f, x + 0.12f, 0.58f, 70, c);
            StartCoroutine(Rise(t));
        }

        IEnumerator Rise(Text t)
        {
            float life = 1.3f, a = 0;
            while (a < life && t != null)
            {
                a += Time.deltaTime;
                t.rectTransform.anchoredPosition += Vector2.up * 220f * Time.deltaTime;
                var c = t.color; c.a = 1f - a / life; t.color = c;
                yield return null;
            }
            if (t != null) Destroy(t.gameObject);
        }
    }

    // 21. Romantic date
    public class RomanticDateScreen : PairScreen
    {
        Bar bar; Text status; Button start; bool running;
        protected override void Build()
        {
            Title("Romantic Date"); BackBtn();
            BuildPair(0.62f);
            status = UI.Label(R, "Candlelit dinner for two", 0.1f, 0.26f, 0.9f, 0.32f, 44, UI.Gold);
            bar = UI.MakeBar(R, 0.15f, 0.21f, 0.85f, 0.24f);
            start = UI.Btn(R, "Start Date", 0.2f, 0.08f, 0.8f, 0.16f, () => { if (!running) StartCoroutine(Run()); }, 56);
        }
        IEnumerator Run()
        {
            running = true; start.interactable = false;
            string[] steps = { "Pouring wine...", "Sharing dessert...", "Slow dance...", "Watching the stars..." };
            for (int i = 0; i < steps.Length; i++)
            {
                status.text = steps[i];
                float t = 0;
                while (t < 1.5f) { t += Time.deltaTime; bar.Set((i + t / 1.5f) / steps.Length); yield return null; }
                Float("Love", UI.Pink);
            }
            bar.Set(1f);
            G.AddCoins(150); G.AddAffection(25); G.AddXp(30);
            Missions.Progress("date");
            UI.Message("Date Complete", "You two had a wonderful evening!\n+150 coins   +25 love");
            status.text = "Candlelit dinner for two"; bar.Set(0);
            running = false; start.interactable = true;
        }
        public override void OnHide() { running = false; if (start != null) start.interactable = true; }
    }

    // 22. Couple interaction
    public class InteractionScreen : PairScreen
    {
        static readonly string[] Reacts = { "Heart", "Wow", "Kiss", "Smile", "Hug", "Gift" };
        protected override void Build()
        {
            Title("Couple Interaction"); BackBtn();
            BuildPair(0.62f);
            var items = new List<(string label, UnityAction act)>();
            foreach (var r in Reacts) { var s = r; items.Add((s, () => React(s))); }
            UI.Grid(R, items, 3, 0.04f, 0.17f, 0.96f, 0.38f, 38);
            UI.Btn(R, "Interact", 0.2f, 0.06f, 0.8f, 0.14f, () => React(Reacts[UnityEngine.Random.Range(0, Reacts.Length)]), 56);
        }
        void React(string r)
        {
            Float(r + "!", UI.Pink); G.Sfx(r == "Kiss" ? "kiss" : "heart");
            G.AddAffection(2); G.AddXp(1);
            if (UnityEngine.Random.value < 0.3f) G.Toast(D.partnerName + " loves it!");
        }
    }

    // 23. Hug & Kiss
    public class HugKissScreen : PairScreen
    {
        bool busy;
        protected override void Build()
        {
            Title("Hug & Kiss"); BackBtn();
            BuildPair(0.6f, 360, 660);
            UI.Btn(R, "Love", 0.25f, 0.10f, 0.75f, 0.19f, () => { if (!busy) StartCoroutine(Hug()); }, 64, UI.Pink);
        }
        IEnumerator Hug()
        {
            busy = true;
            Vector2 a0 = me.rt.anchoredPosition, b0 = partner.rt.anchoredPosition;
            Vector2 a1 = new Vector2(-70, 0), b1 = new Vector2(70, 0);
            float t = 0;
            while (t < 0.8f) { t += Time.deltaTime; float k = t / 0.8f; me.rt.anchoredPosition = Vector2.Lerp(a0, a1, k); partner.rt.anchoredPosition = Vector2.Lerp(b0, b1, k); yield return null; }
            Float("Love", UI.Pink); G.Sfx("kiss"); G.AddAffection(5); G.AddXp(2);
            yield return new WaitForSeconds(0.8f);
            t = 0;
            while (t < 0.5f) { t += Time.deltaTime; float k = t / 0.5f; me.rt.anchoredPosition = Vector2.Lerp(a1, a0, k); partner.rt.anchoredPosition = Vector2.Lerp(b1, b0, k); yield return null; }
            busy = false;
        }
        public override void OnHide()
        {
            busy = false;
            if (me != null) { me.rt.anchoredPosition = new Vector2(-170, 0); partner.rt.anchoredPosition = new Vector2(170, 0); }
        }
    }

    // 24. Couple dance
    public class DanceScreen : PairScreen
    {
        bool dancing; float danceTime; Button btn; AudioSource music;
        protected override void Build()
        {
            Title("Couple Dance"); BackBtn();
            BuildPair(0.6f);
            btn = UI.Btn(R, "Dance", 0.25f, 0.10f, 0.75f, 0.19f, Toggle, 64);
        }
        void Toggle()
        {
            dancing = !dancing; danceTime = 0;
            UI.SetLabel(btn, dancing ? "Stop" : "Dance");
            var clip = Art.Clip("dance"); // optional music
            if (clip != null)
            {
                if (music == null) music = gameObject.AddComponent<AudioSource>();
                if (dancing) { music.clip = clip; music.loop = true; music.Play(); G.PauseMusic(true); } else { music.Stop(); G.PauseMusic(false); }
            }
        }
        void OnDisable() { if (dancing) { dancing = false; if (music != null) music.Stop(); G.PauseMusic(false); } }
        void Update()
        {
            if (me == null) return;
            if (!dancing) { me.rt.localEulerAngles = Vector3.zero; partner.rt.localEulerAngles = Vector3.zero; return; }
            danceTime += Time.deltaTime;
            me.rt.localEulerAngles = new Vector3(0, 0, Mathf.Sin(danceTime * 6f) * 9f);
            partner.rt.localEulerAngles = new Vector3(0, 0, -Mathf.Sin(danceTime * 6f) * 9f);
            me.rt.anchoredPosition = new Vector2(-170, Mathf.Abs(Mathf.Sin(danceTime * 6f)) * 40f);
            partner.rt.anchoredPosition = new Vector2(170, Mathf.Abs(Mathf.Cos(danceTime * 6f)) * 40f);
            if (danceTime >= 8f)
            {
                G.AddCoins(60); G.AddAffection(10); G.AddXp(15);
                G.Toast("Great dance! +60 coins");
                Float("Wow", UI.Gold);
                Toggle();
            }
        }
        public override void OnHide()
        {
            if (dancing) Toggle();
            if (me != null) { me.rt.anchoredPosition = new Vector2(-170, 0); partner.rt.anchoredPosition = new Vector2(170, 0); }
        }
    }

    // 25. Virtual pet
    public class PetScreen : ScreenBase
    {
        RectTransform pet; Text mood, nameLbl; Bar hunger, happy;
        protected override void Build()
        {
            Title("Virtual Pet"); BackBtn();
            pet = UI.Pos(R, "Pet", 0.5f, 0.58f, 460, 460);
            var im = pet.gameObject.AddComponent<Image>();
            var sp = Art.Sprite("Characters/pet"); // optional pet art
            if (sp != null) { im.sprite = sp; im.preserveAspect = true; }
            else { im.sprite = UI.Circle; im.color = new Color(1f, 0.95f, 0.85f); UI.Label(pet, "PET", 0, 0, 1, 1, 90, new Color(0.3f, 0.2f, 0.1f)); }
            im.raycastTarget = false;
            nameLbl = UI.Label(R, "", 0.1f, 0.34f, 0.9f, 0.40f, 52, UI.Gold);
            mood = UI.Label(R, "", 0.1f, 0.30f, 0.9f, 0.34f, 36);
            UI.Label(R, "Hunger", 0.05f, 0.255f, 0.28f, 0.285f, 30, UI.White, TextAnchor.MiddleLeft);
            hunger = UI.MakeBar(R, 0.3f, 0.26f, 0.95f, 0.28f, new Color(1f, 0.6f, 0.2f));
            UI.Label(R, "Happy", 0.05f, 0.215f, 0.28f, 0.245f, 30, UI.White, TextAnchor.MiddleLeft);
            happy = UI.MakeBar(R, 0.3f, 0.22f, 0.95f, 0.24f, UI.Pink);
            UI.Btn(R, "Play", 0.06f, 0.08f, 0.48f, 0.17f, Play, 52);
            UI.Btn(R, "Feed", 0.52f, 0.08f, 0.94f, 0.17f, Feed, 52, UI.Gold);
        }
        public override void OnShow()
        {
            long ticks;
            if (long.TryParse(D.petTick, out ticks))
            {
                double mins = (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalMinutes;
                int drop = (int)(mins / 10) * 3;
                if (drop > 0)
                {
                    D.petHunger = Mathf.Max(0, D.petHunger - drop);
                    D.petHappy = Mathf.Max(10, D.petHappy - drop / 2);
                    D.petTick = DateTime.UtcNow.Ticks.ToString();
                }
            }
            else D.petTick = DateTime.UtcNow.Ticks.ToString();
            Refresh();
        }
        void Refresh()
        {
            nameLbl.text = D.petName;
            hunger.Set(D.petHunger / 100f); happy.Set(D.petHappy / 100f);
            mood.text = D.petHunger < 30 ? "Hungry!" : D.petHappy > 70 ? "Very happy" : "Doing okay";
        }
        void Feed()
        {
            D.petHunger = Mathf.Min(100, D.petHunger + 30); D.petTick = DateTime.UtcNow.Ticks.ToString();
            G.AddXp(2); Refresh(); G.Toast(D.petName + " enjoyed the meal!");
        }
        void Play()
        {
            if (D.petHunger < 30) { G.Toast(D.petName + " is hungry - feed first"); return; }
            D.petHappy = Mathf.Min(100, D.petHappy + 15); D.petHunger = Mathf.Max(0, D.petHunger - 5);
            G.AddCoins(5); G.AddXp(5); Missions.Progress("pet"); Refresh(); G.Toast("Fun time! +5 coins");
        }
        void Update() { if (pet != null) pet.anchoredPosition = new Vector2(0, Mathf.Sin(Time.time * 3f) * 14f); }
    }
}
