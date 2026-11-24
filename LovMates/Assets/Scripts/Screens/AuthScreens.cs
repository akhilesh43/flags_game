using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LovMates
{
    // 1. Splash
    public class SplashScreen : ScreenBase
    {
        Bar bar;
        protected override void Build()
        {
            UI.Label(R, "LovMates", 0.05f, 0.50f, 0.95f, 0.64f, 130, UI.White);
            UI.Label(R, "Our Little World", 0.1f, 0.44f, 0.9f, 0.51f, 56, UI.Gold);
            UI.Label(R, "Loading...", 0.2f, 0.16f, 0.8f, 0.21f, 44);
            bar = UI.MakeBar(R, 0.2f, 0.12f, 0.8f, 0.15f);
        }
        public override void OnShow() { StartCoroutine(Run()); }
        IEnumerator Run()
        {
            float t = 0;
            while (t < 2.2f) { t += Time.deltaTime; bar.Set(t / 2.2f); yield return null; }
            G.Go(ScreenId.Welcome, false);
        }
    }

    // 2. Welcome
    public class WelcomeScreen : ScreenBase
    {
        protected override void Build()
        {
            UI.Label(R, "LovMates", 0.05f, 0.42f, 0.95f, 0.56f, 130, UI.White);
            UI.Label(R, "Our Little World", 0.1f, 0.37f, 0.9f, 0.43f, 56, UI.Gold);
            UI.Label(R, "Together Forever", 0.1f, 0.2f, 0.9f, 0.25f, 44);
            UI.Btn(R, "Start Game", 0.15f, 0.09f, 0.85f, 0.17f, () => G.StartGameFlow(), 58);
        }
    }

    // 3. Login
    public class LoginScreen : ScreenBase
    {
        InputField id, pass;
        protected override void Build()
        {
            UI.Label(R, "LovMates", 0.05f, 0.76f, 0.95f, 0.88f, 110, UI.White);
            UI.Label(R, "Our Little World", 0.1f, 0.71f, 0.9f, 0.77f, 48, UI.Gold);
            id = UI.Input(R, "Email / Username", 0.1f, 0.60f, 0.9f, 0.67f);
            pass = UI.Input(R, "Password", 0.1f, 0.51f, 0.9f, 0.58f, true);
            UI.Btn(R, "Login", 0.1f, 0.41f, 0.9f, 0.49f, DoLogin, 54);
            UI.Label(R, "OR", 0.4f, 0.36f, 0.6f, 0.40f, 40);
            UI.Btn(R, "Google", 0.1f, 0.27f, 0.47f, 0.34f, SocialGuest, 40, new Color(0.25f, 0.25f, 0.3f));
            UI.Btn(R, "Apple", 0.53f, 0.27f, 0.9f, 0.34f, SocialGuest, 40, new Color(0.1f, 0.1f, 0.1f));
            UI.Btn(R, "Create Account", 0.1f, 0.17f, 0.9f, 0.24f, () => G.Go(ScreenId.Register, false), 46, UI.Gold);
            UI.Btn(R, "Forgot Password?", 0.2f, 0.09f, 0.8f, 0.15f, () => G.Go(ScreenId.Forgot, false), 36, UI.DarkGreen);
        }
        public override void OnShow() { pass.text = ""; }
        void DoLogin()
        {
            string err;
            if (Services.Auth.Login(id.text, pass.text, out err)) { pass.text = ""; G.Toast("Welcome back, " + D.playerName + "!"); G.AfterAuth(); }
            else G.Toast(err);
        }
        // Plug Google / Apple SDKs here. Until then both buttons sign in as a local guest.
        void SocialGuest()
        {
            Services.Auth.Guest();
            G.Toast("Social login SDK not connected - signed in as Guest");
            G.AfterAuth();
        }
    }

    // 4. Register
    public class RegisterScreen : ScreenBase
    {
        InputField name, email, pass, pass2;
        protected override void Build()
        {
            UI.Label(R, "Create Account", 0.05f, 0.80f, 0.95f, 0.88f, 80, UI.White);
            UI.Label(R, "Join LovMates Family", 0.1f, 0.75f, 0.9f, 0.80f, 44, UI.Gold);
            name = UI.Input(R, "Name", 0.1f, 0.66f, 0.9f, 0.73f);
            email = UI.Input(R, "Email Address", 0.1f, 0.57f, 0.9f, 0.64f);
            pass = UI.Input(R, "Password", 0.1f, 0.48f, 0.9f, 0.55f, true);
            pass2 = UI.Input(R, "Confirm Password", 0.1f, 0.39f, 0.9f, 0.46f, true);
            UI.Btn(R, "Register", 0.1f, 0.28f, 0.9f, 0.36f, DoRegister, 54);
            UI.Btn(R, "Already have account? Login", 0.1f, 0.19f, 0.9f, 0.25f, () => G.Go(ScreenId.Login, false), 36, UI.DarkGreen);
        }
        void DoRegister()
        {
            if (pass.text != pass2.text) { G.Toast("Passwords do not match"); return; }
            string err;
            if (Services.Auth.Register(name.text, email.text, pass.text, out err)) { G.Toast("Account created!"); G.AfterAuth(); }
            else G.Toast(err);
        }
    }

    // 5. Forgot password
    public class ForgotScreen : ScreenBase
    {
        InputField email;
        protected override void Build()
        {
            UI.Label(R, "Reset Password", 0.05f, 0.76f, 0.95f, 0.86f, 80, UI.White);
            UI.Label(R, "Enter the email you registered with", 0.1f, 0.67f, 0.9f, 0.73f, 40, UI.Gold);
            email = UI.Input(R, "Enter your email", 0.1f, 0.56f, 0.9f, 0.63f);
            UI.Btn(R, "Send Link", 0.1f, 0.45f, 0.9f, 0.53f, Send, 54);
            UI.Btn(R, "Back to Login", 0.2f, 0.35f, 0.8f, 0.41f, () => G.Go(ScreenId.Login, false), 40, UI.DarkGreen);
        }
        void Send()
        {
            string msg;
            Services.Auth.Reset(email.text, out msg);
            G.Toast(msg);
        }
    }
}
