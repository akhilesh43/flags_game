using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace LovMates
{
    // 29. Love diary
    public class DiaryScreen : ScreenBase
    {
        ScrollRect list; InputField input;
        protected override void Build()
        {
            Title("Love Diary"); BackBtn();
            list = UI.Scroll(R, 0.05f, 0.35f, 0.95f, 0.88f);
            input = UI.Input(R, "Dear diary...", 0.05f, 0.24f, 0.95f, 0.32f);
            UI.Btn(R, "Write", 0.05f, 0.10f, 0.48f, 0.19f, () => input.ActivateInputField(), 52);
            UI.Btn(R, "Save", 0.52f, 0.10f, 0.95f, 0.19f, Save, 52, UI.Gold);
        }
        public override void OnShow() { Refresh(); }
        void Refresh()
        {
            UI.Clear(list.content);
            for (int i = D.diary.Count - 1; i >= 0; i--)
                UI.Row(list.content, D.diary[i].date + "\n" + D.diary[i].text, 150, null, TextAnchor.MiddleLeft, null, 34);
            if (D.diary.Count == 0) UI.Row(list.content, "Your story starts here. Write your first memory!", 120);
        }
        void Save()
        {
            string t = input.text.Trim();
            if (t.Length == 0) { G.Toast("Write something first"); return; }
            D.diary.Add(new DiaryEntry { date = DateTime.Now.ToString("dd MMM yyyy, HH:mm"), text = t });
            input.text = "";
            G.AddCoins(10); G.AddXp(3); G.Save(); Refresh(); G.Toast("Saved to diary");
        }
    }

    // 30. Gallery
    public class GalleryScreen : ScreenBase
    {
        ScrollRect grid;
        readonly List<UnityEngine.Object> temps = new List<UnityEngine.Object>();

        protected override void Build()
        {
            Title("Our Gallery"); BackBtn();
            grid = UI.ScrollGrid(R, 0.04f, 0.24f, 0.96f, 0.88f, new Vector2(290, 440), 3);
            UI.Btn(R, "View", 0.05f, 0.10f, 0.48f, 0.19f, () => { if (D.gallery.Count > 0) ViewPhoto(D.gallery[D.gallery.Count - 1]); else G.Toast("No photos yet"); }, 52);
            UI.Btn(R, "Add", 0.52f, 0.10f, 0.95f, 0.19f, () => G.StartCoroutine(AddPhoto()), 52, UI.Gold);
        }

        public override void OnShow() { Refresh(); }
        public override void OnHide() { FreeTextures(); }

        void FreeTextures() { foreach (var o in temps) if (o != null) Destroy(o); temps.Clear(); }

        Sprite Load(string file)
        {
            string p = Path.Combine(G.GalleryDir, file);
            if (!File.Exists(p)) return null;
            var tex = new Texture2D(2, 2);
            tex.LoadImage(File.ReadAllBytes(p));
            var sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            temps.Add(tex); temps.Add(sp);
            return sp;
        }

        void Refresh()
        {
            FreeTextures();
            UI.Clear(grid.content);
            for (int i = D.gallery.Count - 1; i >= 0 && i >= D.gallery.Count - 30; i--)
            {
                string id = D.gallery[i];
                var sp = Load("th_" + id + ".png");
                if (sp == null) continue;
                var go = new GameObject("Photo", typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(grid.content, false);
                var im = go.GetComponent<Image>(); im.sprite = sp; im.preserveAspect = true;
                go.GetComponent<Button>().onClick.AddListener(() => ViewPhoto(id));
            }
        }

        void ViewPhoto(string id)
        {
            var sp = Load(id + ".png");
            if (sp == null) { G.Toast("Photo missing"); return; }
            var card = UI.Overlay(0.03f, 0.08f, 0.97f, 0.94f);
            var im = UI.Box(card, "Photo", 0.03f, 0.12f, 0.97f, 0.98f, Color.white, false);
            im.sprite = sp; im.preserveAspect = true; im.color = Color.white;
            UI.Btn(card, "Close", 0.3f, 0.02f, 0.7f, 0.10f, () => UI.Close(card), 44);
        }

        // Pure-UI game: a "photo" is a screenshot of the world. Swap for NativeGallery picker if you want device photos.
        IEnumerator AddPhoto()
        {
            G.Go(ScreenId.Home);
            yield return new WaitForSeconds(0.7f);
            yield return G.CapturePhoto();
            G.Back();
        }
    }

    // 31. Love chat
    public class ChatScreen : ScreenBase
    {
        static readonly string[] Replies = {
            "I miss you too!", "You made my day.", "Can't wait to see you tonight.", "Love you to the moon and back.",
            "Let's go on a date soon!", "Thinking about you right now.", "You're my favorite person." };
        ScrollRect list; InputField input; Text header;

        protected override void Build()
        {
            header = Title("Love Chat"); BackBtn();
            list = UI.Scroll(R, 0.04f, 0.2f, 0.96f, 0.88f);
            input = UI.Input(R, "Type a message...", 0.04f, 0.08f, 0.76f, 0.16f);
            UI.Btn(R, "Send", 0.78f, 0.08f, 0.96f, 0.16f, Send, 40, UI.Gold);
        }

        public override void OnShow() { header.text = D.partnerName; Refresh(); }

        void Refresh()
        {
            UI.Clear(list.content);
            foreach (var m in D.chat)
                UI.Row(list.content, m.text + "\n" + m.time, 100,
                    m.mine ? new Color(0.15f, 0.55f, 0.15f, 0.95f) : new Color(0.1f, 0.3f, 0.35f, 0.95f),
                    m.mine ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft, null, 34);
            StartCoroutine(ToBottom());
        }

        IEnumerator ToBottom() { yield return null; Canvas.ForceUpdateCanvases(); list.verticalNormalizedPosition = 0f; }

        void Add(bool mine, string text)
        {
            D.chat.Add(new ChatMsg { mine = mine, text = text, time = DateTime.Now.ToString("HH:mm") });
            if (D.chat.Count > 100) D.chat.RemoveAt(0);
        }

        void Send()
        {
            string t = input.text.Trim();
            if (t.Length == 0) return;
            Add(true, t); input.text = ""; G.Sfx("message");
            Missions.Progress("talk"); G.AddXp(1);
            Refresh();
            StartCoroutine(Reply());
        }

        // Demo auto-reply. Replace with real-time backend (Firebase / Photon / WebSocket) for true couple chat.
        IEnumerator Reply()
        {
            yield return new WaitForSeconds(1.2f);
            Add(false, Replies[UnityEngine.Random.Range(0, Replies.Length)]);
            Refresh();
        }
    }

    // 32. Love letters
    public class LettersScreen : ScreenBase
    {
        static readonly string[] Letters = {
            "Every day with you feels like the best chapter of my story.",
            "You are my favorite hello and my hardest goodbye.",
            "Thank you for being my calm in every storm.",
            "Whatever tomorrow brings, I want to face it with you.",
            "Your smile is my favorite place to rest.",
            "I fall for you a little more every single day.",
            "Home is wherever you are.",
            "You make ordinary moments feel magical." };
        Text count;

        protected override void Build()
        {
            Title("Love Letters"); BackBtn();
            var env = UI.Box(R, "Envelope", 0.15f, 0.45f, 0.85f, 0.75f, new Color(0.95f, 0.9f, 0.8f));
            UI.Label(env.transform, "A letter is waiting for you", 0.05f, 0.1f, 0.95f, 0.9f, 52, new Color(0.5f, 0.1f, 0.2f));
            count = UI.Label(R, "", 0.1f, 0.33f, 0.9f, 0.39f, 38, UI.Gold);
            UI.Btn(R, "Open Letter", 0.2f, 0.12f, 0.8f, 0.21f, Open, 56);
        }

        public override void OnShow() { count.text = "Letters opened: " + D.lettersOpened; }

        void Open()
        {
            if (D.lastLetterDay == G.Today) { UI.Message("Come back tomorrow", "Your next love letter arrives tomorrow."); return; }
            D.lastLetterDay = G.Today;
            string t = Letters[D.lettersOpened % Letters.Length];
            D.lettersOpened++;
            G.AddCoins(20); G.AddAffection(5);
            count.text = "Letters opened: " + D.lettersOpened;
            UI.Message("A letter from " + D.partnerName, t + "\n\n+20 coins");
        }
    }
}
