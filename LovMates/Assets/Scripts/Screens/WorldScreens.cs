using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LovMates
{
    // 11. Main game home: walk with joystick, doors show a live "Enter" button
    public class HomeScreen : ScreenBase
    {
        Text nameLbl, loveLbl, coinsLbl, gemsLbl;
        WorldView world;
        RectTransform places, menu;

        protected override void Build()
        {
            // HUD
            var av = UI.Pos(R, "Avatar", 0.085f, 0.955f, 110, 110);
            var ai = av.gameObject.AddComponent<Image>();
            ai.sprite = UI.Circle; ai.color = UI.Gold;
            var ab = av.gameObject.AddComponent<Button>();
            ab.targetGraphic = ai;
            ab.onClick.AddListener(() => G.Go(ScreenId.Settings));
            UI.Label(av, "ME", 0, 0, 1, 1, 36, Color.black);
            nameLbl = UI.Label(R, "", 0.16f, 0.95f, 0.47f, 0.985f, 36, UI.White, TextAnchor.MiddleLeft);
            loveLbl = UI.Label(R, "", 0.16f, 0.918f, 0.47f, 0.952f, 28, UI.Pink, TextAnchor.MiddleLeft);
            UI.Box(R, "GemsBg", 0.48f, 0.94f, 0.69f, 0.985f, UI.Glass);
            gemsLbl = UI.Label(R, "", 0.50f, 0.94f, 0.68f, 0.985f, 32);
            UI.Box(R, "CoinsBg", 0.70f, 0.94f, 0.89f, 0.985f, UI.Glass);
            coinsLbl = UI.Label(R, "", 0.71f, 0.94f, 0.88f, 0.985f, 32, UI.Gold);
            UI.Btn(R, "II", 0.90f, 0.935f, 0.98f, 0.985f, () => G.Go(ScreenId.Pause), 40, UI.DarkGreen);

            // World with live doors -> straight inside the location
            world = WorldView.Create(R, 0.03f, 0.38f, 0.97f, 0.90f);
            world.AddDoor("Our Home", new Vector2(-290, 260), () => GoInside(ScreenId.OurHome));
            world.AddDoor("Garden", new Vector2(290, 260), () => GoInside(ScreenId.Garden));
            world.AddDoor("Love City", new Vector2(-290, 0), () => GoInside(ScreenId.LoveCity));
            world.AddDoor("Beach", new Vector2(290, 0), () => GoInside(ScreenId.Beach));
            world.AddDoor("Love Park", new Vector2(-290, -260), () => GoInside(ScreenId.LovePark));
            world.AddDoor("Cafe", new Vector2(290, -260), () => GoInside(ScreenId.Cafe));

            // Quick actions (right of joystick)
            UI.Grid(R, new List<(string label, UnityAction act)> {
                ("Interact", () => G.Go(ScreenId.Interaction)),
                ("Hug & Kiss", () => G.Go(ScreenId.HugKiss)),
                ("Dance", () => G.Go(ScreenId.Dance)),
                ("Photo", () => StartCoroutine(G.CapturePhoto())) }, 2, 0.42f, 0.10f, 0.97f, 0.28f, 34);

            // Popups
            places = MakePopup("Places", new List<(string label, ScreenId id)> {
                ("Our Home", ScreenId.OurHome), ("Bedroom", ScreenId.Bedroom), ("Living Room", ScreenId.LivingRoom),
                ("Kitchen", ScreenId.Kitchen), ("Garden", ScreenId.Garden), ("Love Park", ScreenId.LovePark),
                ("Beach", ScreenId.Beach), ("Love City", ScreenId.LoveCity), ("Cafe", ScreenId.Cafe) });
            menu = MakePopup("Menu", new List<(string label, ScreenId id)> {
                ("Wardrobe", ScreenId.Wardrobe), ("Decor", ScreenId.Decor), ("Diary", ScreenId.Diary),
                ("Gallery", ScreenId.Gallery), ("Letters", ScreenId.Letters), ("Mood", ScreenId.Mood),
                ("Goals", ScreenId.Goals), ("Special Days", ScreenId.SpecialDays), ("Missions", ScreenId.Missions),
                ("Event", ScreenId.Event), ("Settings", ScreenId.Settings), ("Date", ScreenId.RomanticDate) });

            // Bottom nav (built after popups' creation order doesn't matter; popups are siblings added later)
            string[] nav = { "Places", "Pet", "Shop", "Chat", "Menu" };
            UnityAction[] acts = {
                () => places.gameObject.SetActive(true), () => G.Go(ScreenId.Pet), () => G.Go(ScreenId.GiftShop),
                () => G.Go(ScreenId.Chat), () => menu.gameObject.SetActive(true) };
            for (int i = 0; i < nav.Length; i++)
                UI.Btn(R, nav[i], 0.02f + i * 0.196f, 0.012f, 0.02f + i * 0.196f + 0.18f, 0.085f, acts[i], 36);
            places.SetAsLastSibling(); menu.SetAsLastSibling();
        }

        RectTransform MakePopup(string title, List<(string label, ScreenId id)> items)
        {
            var card = UI.Box(R, "Popup_" + title, 0.04f, 0.18f, 0.96f, 0.86f, new Color(0.02f, 0.2f, 0.05f, 0.97f)).rectTransform;
            UI.Label(card, title, 0.1f, 0.9f, 0.9f, 0.98f, 56, UI.Gold);
            var list = new List<(string label, UnityAction act)>();
            foreach (var it in items)
            {
                var id = it.id;
                list.Add((it.label, () => { card.gameObject.SetActive(false); G.Go(id); }));
            }
            UI.Grid(card, list, 3, 0.04f, 0.16f, 0.96f, 0.9f, 36);
            UI.Btn(card, "Close", 0.3f, 0.03f, 0.7f, 0.13f, () => card.gameObject.SetActive(false), 44, UI.Pink);
            card.gameObject.SetActive(false);
            return card;
        }

        void GoInside(ScreenId id)
        {
            G.Go(id);
            G.Get<LocationScreen>(id).EnterInside();
        }

        public override void OnShow()
        {
            Missions.EnsureToday();
            places.gameObject.SetActive(false); menu.gameObject.SetActive(false);
            world.Refresh();
            G.OnCurrencyChanged += RefreshHud;
            RefreshHud();
        }

        public override void OnHide() { G.OnCurrencyChanged -= RefreshHud; }

        void RefreshHud()
        {
            nameLbl.text = D.playerName + "  Lv." + D.level;
            loveLbl.text = "Love " + D.affection;
            coinsLbl.text = D.coins.ToString("N0");
            gemsLbl.text = D.gems.ToString("N0");
        }
    }

    // 12-20. Location screen: preview card with a LIVE Enter button -> walkable interior with actions
    public class LocationScreen : ScreenBase
    {
        string title = "", tagline = "";
        Color tint = new Color(0.16f, 0.55f, 0.14f);
        (string label, Action act)[] extras = new (string label, Action act)[0];
        RectTransform preview, inside;
        WorldView world;
        bool isInside;
        float timer;

        protected override Color TopColor { get { return tint; } }

        public void Setup(string t, string tag, Color tintColor, params (string label, Action act)[] ex)
        {
            title = t; tagline = tag; tint = tintColor; extras = ex;
        }

        protected override void Build()
        {
            // Preview card
            preview = UI.Rect(R, "Preview", 0, 0, 1, 1);
            BackBtn(preview);
            Title(title, preview);
            UI.Label(preview, tagline, 0.1f, 0.5f, 0.9f, 0.58f, 46, UI.Gold);
            UI.Btn(preview, "Enter", 0.2f, 0.10f, 0.8f, 0.19f, EnterInside, 64);

            // Interior
            inside = UI.Rect(R, "Inside", 0, 0, 1, 1);
            BackBtn(inside, () => { isInside = false; G.Back(); });
            Title(title, inside);
            world = WorldView.Create(inside, 0.03f, 0.38f, 0.97f, 0.90f);
            var items = new List<(string label, UnityAction act)> {
                ("Interact", () => G.Go(ScreenId.Interaction)),
                ("Hug & Kiss", () => G.Go(ScreenId.HugKiss)),
                ("Dance", () => G.Go(ScreenId.Dance)) };
            foreach (var e in extras) { var a = e.act; items.Add((e.label, () => a())); }
            UI.Grid(inside, items, 3, 0.42f, 0.03f, 0.99f, 0.29f, 30);
            SetInside(false);
        }

        void SetInside(bool v)
        {
            isInside = v;
            inside.gameObject.SetActive(v);
            preview.gameObject.SetActive(!v);
        }

        public override void OnShow()
        {
            if (!G.Returning && isInside) SetInside(false);
            if (isInside) world.Refresh();
        }

        public void EnterInside()
        {
            SetInside(true);
            world.Refresh();
            string key = Id.ToString();
            if (!D.visited.Contains(key))
            {
                D.visited.Add(key);
                G.AddCoins(50); G.AddXp(20);
                G.Toast("First visit! +50 coins");
            }
            Missions.Progress("visit");
        }

        void Update()
        {
            if (!isInside) return;
            timer += Time.deltaTime;
            if (timer >= 10f) { timer = 0; G.AddCoins(5); G.AddXp(1); } // time-together trickle
        }
    }
}
