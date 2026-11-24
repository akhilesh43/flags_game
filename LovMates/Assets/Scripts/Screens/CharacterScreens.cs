using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LovMates
{
    // 6 + 7. Boy / Girl selection
    public class CharacterSelectScreen : ScreenBase
    {
        public int gender;
        int model;
        AvatarView preview;
        Text nameLbl;

        protected override void Build()
        {
            Title(gender == 0 ? "Boy Selection" : "Girl Selection");
            if (gender == 1) BackBtn(null, () => G.Go(ScreenId.BoySelect, false));
            preview = AvatarView.Create(R, "Preview", 0.4f, 0.55f, 520, 900);
            nameLbl = UI.Label(R, "", 0.1f, 0.22f, 0.7f, 0.28f, 48, UI.Gold);
            for (int i = 0; i < 4; i++)
            {
                int m = i;
                UI.Btn(R, (i + 1).ToString(), 0.8f, 0.72f - i * 0.12f, 0.96f, 0.80f - i * 0.12f, () => { model = m; Refresh(); }, 50, CharacterArt.Outfit[i]);
            }
            UI.Btn(R, "Select", 0.15f, 0.08f, 0.85f, 0.15f, Confirm, 56);
        }

        CharacterLook Look { get { return gender == 0 ? D.boy : D.girl; } }

        public override void OnShow() { model = Look.model; Refresh(); }

        void Refresh()
        {
            Look.model = model; Look.outfit = model;
            preview.Set(gender, Look);
            nameLbl.text = (gender == 0 ? "Boy " : "Girl ") + (model + 1);
        }

        void Confirm()
        {
            G.Go(gender == 0 ? ScreenId.GirlSelect : ScreenId.Customize, false);
        }
    }

    // 8. Character customization
    public class CustomizeScreen : ScreenBase
    {
        static readonly string[] Opts = { "Hair", "Outfit", "Shoes", "Accessory", "Skin" };
        AvatarView boyV, girlV;
        int editing;
        Text info;
        Button editBtn, meBtn;

        protected override void Build()
        {
            Title("Customize");
            BackBtn(null, () => G.Go(ScreenId.GirlSelect, false));
            boyV = AvatarView.Create(R, "Boy", 0.2f, 0.60f, 340, 620);
            girlV = AvatarView.Create(R, "Girl", 0.52f, 0.60f, 340, 620);
            info = UI.Label(R, "", 0.04f, 0.33f, 0.96f, 0.39f, 36, UI.Gold);
            for (int i = 0; i < Opts.Length; i++)
            {
                int k = i;
                UI.Btn(R, Opts[i], 0.76f, 0.78f - i * 0.09f, 0.97f, 0.85f - i * 0.09f, () => Cycle(k), 32);
            }
            editBtn = UI.Btn(R, "Edit: Boy", 0.05f, 0.24f, 0.48f, 0.31f, () => { editing = 1 - editing; Refresh(); }, 38, UI.DarkGreen);
            meBtn = UI.Btn(R, "I play: Boy", 0.52f, 0.24f, 0.95f, 0.31f, () => { D.myGender = 1 - D.myGender; Refresh(); }, 38, UI.DarkGreen);
            UI.Btn(R, "Done", 0.15f, 0.08f, 0.85f, 0.15f, Done, 56);
        }

        CharacterLook Cur { get { return editing == 0 ? D.boy : D.girl; } }

        public override void OnShow() { Refresh(); }

        void Cycle(int i)
        {
            var l = Cur;
            switch (i)
            {
                case 0: l.hair = (l.hair + 1) % 6; break;
                case 1: l.outfit = (l.outfit + 1) % 6; break;
                case 2: l.shoes = (l.shoes + 1) % 6; break;
                case 3: l.accessory = (l.accessory + 1) % 6; break;
                case 4: l.skin = (l.skin + 1) % 6; break;
            }
            Refresh();
        }

        void Refresh()
        {
            boyV.Set(0, D.boy); girlV.Set(1, D.girl);
            var l = Cur;
            info.text = "Hair " + (l.hair + 1) + "  Outfit " + (l.outfit + 1) + "  Shoes " + (l.shoes + 1) + "  Acc " + (l.accessory + 1) + "  Skin " + (l.skin + 1);
            UI.SetLabel(editBtn, "Edit: " + (editing == 0 ? "Boy" : "Girl"));
            UI.SetLabel(meBtn, "I play: " + (D.myGender == 0 ? "Boy" : "Girl"));
        }

        void Done()
        {
            D.characterCreated = true;
            G.Save();
            G.Go(ScreenId.CoupleConnect, false);
        }
    }

    // 9. Couple connection
    public class CoupleConnectScreen : ScreenBase
    {
        InputField code;
        Text myCode;

        protected override void Build()
        {
            Title("Couple Connection");
            UI.Label(R, "Connect with your partner", 0.1f, 0.74f, 0.9f, 0.80f, 46, UI.Gold);
            myCode = UI.Label(R, "", 0.1f, 0.62f, 0.9f, 0.72f, 64, UI.White);
            code = UI.Input(R, "Enter Partner Code", 0.1f, 0.50f, 0.9f, 0.58f);
            UI.Btn(R, "Connect", 0.1f, 0.40f, 0.9f, 0.48f, Connect, 54);
            UI.Btn(R, "Share Code", 0.1f, 0.30f, 0.9f, 0.38f, Share, 54, UI.Gold);
            UI.Btn(R, "Skip for now", 0.25f, 0.15f, 0.75f, 0.21f, () => G.Go(ScreenId.Loading, false), 38, UI.DarkGreen);
        }

        public override void OnShow()
        {
            if (string.IsNullOrEmpty(D.myCode))
            {
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < 6; i++) sb.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
                D.myCode = sb.ToString();
                G.Save();
            }
            myCode.text = "Your code: " + D.myCode;
        }

        void Connect()
        {
            string pn, err;
            string c = code.text.Trim().ToUpperInvariant();
            if (Services.Partner.Connect(c, D.myCode, out pn, out err))
            {
                D.connected = true; D.partnerCode = c; D.partnerName = pn;
                G.Toast("Connected with " + pn + "!");
                G.Go(ScreenId.Loading, false);
            }
            else G.Toast(err);
        }

        void Share()
        {
            GUIUtility.systemCopyBuffer = "Join me on LovMates! My code: " + D.myCode;
            G.Toast("Code copied - send it to your partner");
        }
    }

    // 10. Loading
    public class LoadingScreen : ScreenBase
    {
        Bar bar;
        protected override void Build()
        {
            UI.Label(R, "LovMates", 0.05f, 0.50f, 0.95f, 0.62f, 100, UI.White);
            UI.Label(R, "Loading...\nPlease Wait", 0.2f, 0.22f, 0.8f, 0.32f, 48);
            bar = UI.MakeBar(R, 0.2f, 0.16f, 0.8f, 0.19f);
        }
        public override void OnShow() { StartCoroutine(Run()); }
        IEnumerator Run()
        {
            float t = 0;
            while (t < 1.6f) { t += Time.deltaTime; bar.Set(t / 1.6f); yield return null; }
            G.GoHome();
        }
    }
}
