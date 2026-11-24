using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace LovMates
{
    // 33. Mood tracker
    public class MoodScreen : ScreenBase
    {
        static readonly string[] Names = { "Love", "Happy", "Okay", "Sad", "Angry" };
        static readonly Color[] Cols = {
            new Color(0.9f, 0.3f, 0.5f), new Color(0.95f, 0.8f, 0.2f), new Color(0.4f, 0.6f, 0.9f),
            new Color(0.4f, 0.4f, 0.7f), new Color(0.85f, 0.25f, 0.2f) };
        int sel = -1;
        Button[] btns = new Button[5];
        Text streak; ScrollRect list;

        protected override void Build()
        {
            Title("Mood Tracker"); BackBtn();
            UI.Label(R, "How are you today?", 0.1f, 0.78f, 0.9f, 0.85f, 54);
            for (int i = 0; i < 5; i++)
            {
                int k = i;
                btns[i] = UI.Btn(R, Names[i], 0.03f + i * 0.19f, 0.66f, 0.03f + i * 0.19f + 0.17f, 0.76f, () => { sel = k; Refresh(); }, 30, Cols[i]);
            }
            UI.Btn(R, "Save", 0.2f, 0.56f, 0.8f, 0.63f, Save, 52);
            streak = UI.Label(R, "", 0.1f, 0.50f, 0.9f, 0.55f, 38, UI.Gold);
            list = UI.Scroll(R, 0.05f, 0.08f, 0.95f, 0.48f);
        }

        public override void OnShow()
        {
            var e = D.moods.Find(m => m.day == G.Today);
            sel = e != null ? e.mood : -1;
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < 5; i++) btns[i].transform.localScale = Vector3.one * (i == sel ? 1.1f : 1f);
            int s = 0; var d = DateTime.Today;
            while (D.moods.Exists(m => m.day == d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))) { s++; d = d.AddDays(-1); }
            streak.text = "Streak: " + s + " day" + (s == 1 ? "" : "s");
            UI.Clear(list.content);
            for (int i = D.moods.Count - 1; i >= 0 && i >= D.moods.Count - 14; i--)
                UI.Row(list.content, D.moods[i].day + "   -   " + Names[Mathf.Clamp(D.moods[i].mood, 0, 4)], 80);
        }

        void Save()
        {
            if (sel < 0) { G.Toast("Pick a mood first"); return; }
            var e = D.moods.Find(m => m.day == G.Today);
            if (e == null) { e = new MoodEntry { day = G.Today }; D.moods.Add(e); G.AddCoins(10); G.AddXp(5); }
            e.mood = sel; G.Save(); Refresh(); G.Toast("Mood saved");
        }
    }

    // 34. Couple goals
    public class GoalsScreen : ScreenBase
    {
        ScrollRect list; Text progress; Bar bar; InputField input; Button viewBtn; bool showAll;

        protected override void Build()
        {
            Title("Couple Goals"); BackBtn();
            progress = UI.Label(R, "", 0.1f, 0.835f, 0.9f, 0.885f, 44, UI.Gold);
            bar = UI.MakeBar(R, 0.1f, 0.805f, 0.9f, 0.83f);
            list = UI.Scroll(R, 0.05f, 0.30f, 0.95f, 0.79f);
            input = UI.Input(R, "New goal...", 0.05f, 0.20f, 0.68f, 0.27f);
            UI.Btn(R, "Add", 0.70f, 0.20f, 0.95f, 0.27f, AddGoal, 40, UI.Gold);
            viewBtn = UI.Btn(R, "View All", 0.2f, 0.08f, 0.8f, 0.16f, () => { showAll = !showAll; Refresh(); }, 52);
        }

        public override void OnShow()
        {
            if (D.goals.Count == 0)
                foreach (var t in new[] { "Travel Together", "Grow Together", "Be Happy Forever" }) D.goals.Add(new GoalItem { title = t });
            showAll = false; Refresh();
        }

        void Refresh()
        {
            UI.Clear(list.content);
            int done = 0;
            foreach (var g in D.goals) if (g.done) done++;
            int n = showAll ? D.goals.Count : Mathf.Min(3, D.goals.Count);
            for (int i = 0; i < n; i++)
            {
                var g = D.goals[i];
                UI.Row(list.content, (g.done ? "[x]  " : "[  ]  ") + g.title, 100,
                    g.done ? new Color(0.2f, 0.6f, 0.15f, 0.95f) : (Color?)null, TextAnchor.MiddleLeft, () => Toggle(g));
            }
            progress.text = done + " / " + D.goals.Count + " completed";
            bar.Set(D.goals.Count == 0 ? 0 : done / (float)D.goals.Count);
            UI.SetLabel(viewBtn, showAll ? "Show Less" : "View All");
        }

        void Toggle(GoalItem g)
        {
            g.done = !g.done;
            if (g.done && !g.rewarded) { g.rewarded = true; G.AddCoins(50); G.AddXp(10); G.Toast("Goal complete! +50 coins"); }
            G.Save(); Refresh();
        }

        void AddGoal()
        {
            string t = input.text.Trim();
            if (t.Length == 0) return;
            D.goals.Add(new GoalItem { title = t }); input.text = ""; showAll = true; G.Save(); Refresh();
        }
    }

    // 35. Special days
    public class SpecialDaysScreen : ScreenBase
    {
        Text date, left, others; InputField input;

        protected override void Build()
        {
            Title("Special Days"); BackBtn();
            UI.Label(R, "Our Anniversary", 0.1f, 0.78f, 0.9f, 0.85f, 60, UI.Gold);
            date = UI.Label(R, "", 0.1f, 0.72f, 0.9f, 0.78f, 44);
            left = UI.Label(R, "", 0.1f, 0.62f, 0.9f, 0.71f, 70, UI.Pink);
            UI.Btn(R, "Celebrate", 0.2f, 0.52f, 0.8f, 0.60f, Celebrate, 54);
            input = UI.Input(R, "Anniversary date (YYYY-MM-DD)", 0.05f, 0.42f, 0.68f, 0.49f);
            UI.Btn(R, "Set", 0.70f, 0.42f, 0.95f, 0.49f, SetDate, 44, UI.Gold);
            others = UI.Label(R, "", 0.08f, 0.14f, 0.92f, 0.38f, 40, UI.White, TextAnchor.UpperLeft, FontStyle.Normal);
        }

        public override void OnShow() { Refresh(); }

        static int DaysLeft(int month, int day)
        {
            var today = DateTime.Today;
            var d = new DateTime(today.Year, month, Mathf.Min(day, DateTime.DaysInMonth(today.Year, month)));
            if (d < today) d = new DateTime(today.Year + 1, month, Mathf.Min(day, DateTime.DaysInMonth(today.Year + 1, month)));
            return (int)(d - today).TotalDays;
        }

        bool Anniv(out DateTime dt)
        {
            return DateTime.TryParseExact(D.anniversary, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
        }

        int AnnivLeft() { DateTime dt; return Anniv(out dt) ? DaysLeft(dt.Month, dt.Day) : -1; }

        void Refresh()
        {
            int dl = AnnivLeft();
            date.text = string.IsNullOrEmpty(D.anniversary) ? "Not set yet" : D.anniversary;
            left.text = dl < 0 ? "Set your date" : dl == 0 ? "Today!" : dl + " Days Left";
            others.text = "Valentine's Day: " + DaysLeft(2, 14) + " days\nNew Year: " + DaysLeft(1, 1) + " days\nChristmas: " + DaysLeft(12, 25) + " days";
        }

        void SetDate()
        {
            DateTime dt;
            if (!DateTime.TryParseExact(input.text.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) { G.Toast("Use format YYYY-MM-DD"); return; }
            D.anniversary = dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); G.Save(); Refresh(); G.Toast("Anniversary saved");
        }

        void Celebrate()
        {
            if (AnnivLeft() == 0)
            {
                if (G.TryReward("anniv", 72000, 500, 100, 100, "Happy Anniversary!")) UI.Message("Happy Anniversary!", "Another beautiful year together.\n+500 coins   +100 love");
            }
            else G.TryReward("celebrate", 600, 20, 5, 5, "Little celebration! +20 coins");
        }
    }

    // 36. Missions & rewards
    public class MissionsScreen : ScreenBase
    {
        ScrollRect list; Text rewards;

        protected override void Build()
        {
            Title("Missions & Rewards"); BackBtn();
            UI.Label(R, "Daily Missions", 0.1f, 0.84f, 0.9f, 0.90f, 50, UI.Gold);
            list = UI.Scroll(R, 0.05f, 0.40f, 0.95f, 0.83f);
            rewards = UI.Label(R, "", 0.1f, 0.30f, 0.9f, 0.37f, 50, UI.White);
            UI.Btn(R, "Claim", 0.2f, 0.14f, 0.8f, 0.24f, Claim, 60);
        }

        public override void OnShow() { Refresh(); }

        void Refresh()
        {
            Missions.EnsureToday();
            UI.Clear(list.content);
            int avail = 0;
            foreach (var m in D.missions)
            {
                bool ready = m.progress >= m.target && !m.claimed;
                if (ready) avail += m.reward;
                UI.Row(list.content, (m.claimed ? "[done]  " : "") + m.title + "      " + m.progress + "/" + m.target + "      +" + m.reward + " gems", 110,
                    m.claimed ? new Color(0.2f, 0.4f, 0.2f, 0.7f) : ready ? new Color(0.8f, 0.6f, 0.1f, 0.95f) : (Color?)null);
            }
            rewards.text = "Rewards: Gems x" + avail;
        }

        void Claim()
        {
            int sum = 0;
            foreach (var m in D.missions)
                if (m.progress >= m.target && !m.claimed) { m.claimed = true; sum += m.reward; }
            if (sum == 0) { G.Sfx("error"); G.Toast("Nothing to claim yet"); return; }
            G.AddGems(sum); G.Sfx("success"); G.Save(); Refresh(); G.Toast("Claimed " + sum + " gems!");
        }
    }

    // 37. Settings
    public class SettingsScreen : ScreenBase
    {
        static readonly string[] Quality = { "Low", "Medium", "High" };
        static readonly string[] Langs = { "English", "Hindi", "Spanish" };
        Text profile; Button sound, gfx, lang;

        protected override void Build()
        {
            Title("Settings"); BackBtn();
            profile = UI.Label(R, "", 0.05f, 0.82f, 0.95f, 0.90f, 50, UI.Gold);
            sound = UI.Btn(R, "", 0.05f, 0.72f, 0.95f, 0.79f, () => { D.sound = !D.sound; G.ApplySettings(); Refresh(); }, 40);
            gfx = UI.Btn(R, "", 0.05f, 0.63f, 0.95f, 0.70f, () => { D.quality = (D.quality + 1) % 3; G.ApplySettings(); Refresh(); }, 40);
            lang = UI.Btn(R, "", 0.05f, 0.54f, 0.95f, 0.61f, () => { D.language = (D.language + 1) % Langs.Length; Refresh(); }, 40);
            UI.Btn(R, "Account Security", 0.05f, 0.45f, 0.95f, 0.52f, ChangePassword, 40);
            UI.Btn(R, "Logout", 0.05f, 0.36f, 0.95f, 0.43f, Logout, 40, UI.Pink);
            UI.Btn(R, "About", 0.05f, 0.20f, 0.48f, 0.28f, () => UI.Message("LovMates", "Our Little World\nVersion 1.0"), 44);
            UI.Btn(R, "Support", 0.52f, 0.20f, 0.95f, 0.28f, () => UI.Message("Support", "Contact: support@yourdomain.com\n(edit this in SettingsScreen.cs)"), 44);
        }

        public override void OnShow() { Refresh(); }

        void Refresh()
        {
            profile.text = D.playerName + "   Lv." + D.level;
            UI.SetLabel(sound, "Sound & Music:  " + (D.sound ? "ON" : "OFF"));
            UI.SetLabel(gfx, "Graphics Quality:  " + Quality[Mathf.Clamp(D.quality, 0, 2)]);
            UI.SetLabel(lang, "Language:  " + Langs[D.language % Langs.Length]);
            G.Save();
        }

        void ChangePassword()
        {
            var card = UI.Overlay(0.06f, 0.3f, 0.94f, 0.72f);
            UI.Label(card, "Change Password", 0.05f, 0.82f, 0.95f, 0.97f, 50, UI.Gold);
            var o = UI.Input(card, "Old password", 0.06f, 0.60f, 0.94f, 0.78f, true);
            var n = UI.Input(card, "New password", 0.06f, 0.38f, 0.94f, 0.56f, true);
            UI.Btn(card, "Save", 0.52f, 0.06f, 0.94f, 0.26f, () =>
            {
                string err;
                if (Services.Auth.ChangePassword(o.text, n.text, out err)) { G.Save(); UI.Close(card); G.Toast("Password updated"); }
                else G.Toast(err);
            }, 44);
            UI.Btn(card, "Cancel", 0.06f, 0.06f, 0.48f, 0.26f, () => UI.Close(card), 44, UI.Pink);
        }

        void Logout()
        {
            D.currentEmail = ""; G.Save(); G.ClearHistory(); G.Go(ScreenId.Login, false);
        }
    }

    // 38. Pause menu
    public class PauseScreen : ScreenBase
    {
        protected override void Build()
        {
            UI.Label(R, "Paused", 0.1f, 0.72f, 0.9f, 0.82f, 90, UI.White);
            UI.Btn(R, "Resume", 0.15f, 0.58f, 0.85f, 0.66f, () => G.Back(), 54);
            UI.Btn(R, "Settings", 0.15f, 0.47f, 0.85f, 0.55f, () => G.Go(ScreenId.Settings), 54);
            UI.Btn(R, "Home", 0.15f, 0.36f, 0.85f, 0.44f, () => G.GoHome(), 54);
            UI.Btn(R, "Exit Game", 0.15f, 0.25f, 0.85f, 0.33f, () => G.Go(ScreenId.End, false), 54, UI.Pink);
        }
    }

    // 39. Event
    public class EventScreen : ScreenBase
    {
        Text ends;

        protected override void Build()
        {
            Title("Special Event"); BackBtn();
            UI.Label(R, "Valentine's Day\nSpecial Event", 0.05f, 0.62f, 0.95f, 0.82f, 80, UI.Pink);
            ends = UI.Label(R, "", 0.1f, 0.36f, 0.9f, 0.44f, 52, UI.Gold);
            UI.Label(R, "Join daily for +200 coins and +25 gems", 0.1f, 0.47f, 0.9f, 0.55f, 38);
            UI.Btn(R, "Join Event", 0.15f, 0.14f, 0.85f, 0.24f, Join, 60, UI.Pink);
        }

        // Event window ends every Feb 14. Change month/day here for other seasonal events.
        static DateTime EventEnd()
        {
            var n = DateTime.Now;
            var e = new DateTime(n.Year, 2, 14, 23, 59, 59);
            return n > e ? e.AddYears(1) : e;
        }

        public override void OnShow() { UpdateText(); }
        void Update() { if (ends != null && gameObject.activeInHierarchy) UpdateText(); }

        void UpdateText()
        {
            var r = EventEnd() - DateTime.Now;
            ends.text = "Ends in: " + (int)r.TotalDays + "d " + r.Hours + "h";
        }

        void Join()
        {
            if (D.lastEventDay == G.Today) { G.Toast("Already joined today - come back tomorrow"); return; }
            D.lastEventDay = G.Today;
            G.AddCoins(200); G.AddGems(25); G.AddAffection(30); G.AddXp(40); G.Save();
            UI.Message("Event Reward", "+200 coins\n+25 gems\n+30 love");
        }
    }

    // 40. End screen
    public class EndScreen : ScreenBase
    {
        protected override Color TopColor { get { return new Color(0.5f, 0.3f, 0.15f); } }
        protected override void Build()
        {
            UI.Label(R, "Love Forever", 0.05f, 0.55f, 0.95f, 0.70f, 110, UI.Pink);
            UI.Btn(R, "Play Again", 0.15f, 0.33f, 0.85f, 0.41f, () => G.GoHome(), 54);
            UI.Btn(R, "Share", 0.15f, 0.22f, 0.85f, 0.30f, Share, 54, UI.Gold);
            UI.Btn(R, "Quit", 0.3f, 0.10f, 0.7f, 0.16f, () => G.Quit(), 40, UI.DarkGreen);
        }
        void Share()
        {
            GUIUtility.systemCopyBuffer = "I'm playing LovMates - Our Little World with " + D.partnerName + "!";
            G.Toast("Copied - paste it anywhere to share");
        }
    }
}
