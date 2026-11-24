using System;
using System.Security.Cryptography;
using System.Text;

namespace LovMates
{
    // Swap these implementations with Firebase / PlayFab / your own server later.
    public interface IAuthBackend
    {
        bool Register(string name, string email, string pass, out string err);
        bool Login(string idOrEmail, string pass, out string err);
        bool Reset(string email, out string msg);
        bool ChangePassword(string oldPass, string newPass, out string err);
        void Guest();
    }

    public interface IPartnerBackend
    {
        bool Connect(string code, string myCode, out string partnerName, out string err);
    }

    public class LocalAuth : IAuthBackend
    {
        SaveData D { get { return GameManager.I.Data; } }

        static string Hash(string email, string pass)
        {
            using (var s = SHA256.Create())
                return Convert.ToBase64String(s.ComputeHash(Encoding.UTF8.GetBytes(email.ToLowerInvariant() + "|" + pass + "|lovmates")));
        }

        public static bool ValidEmail(string e)
        {
            if (string.IsNullOrEmpty(e)) return false;
            int at = e.IndexOf('@');
            return at > 0 && e.IndexOf('.', at) > at + 1 && !e.EndsWith(".");
        }

        UserAccount Find(string idOrEmail)
        {
            idOrEmail = (idOrEmail ?? "").Trim().ToLowerInvariant();
            foreach (var a in D.accounts)
                if (a.email == idOrEmail || a.name.ToLowerInvariant() == idOrEmail) return a;
            return null;
        }

        public bool Register(string name, string email, string pass, out string err)
        {
            err = null;
            name = (name ?? "").Trim();
            email = (email ?? "").Trim().ToLowerInvariant();
            if (name.Length < 2) { err = "Enter your name"; return false; }
            if (!ValidEmail(email)) { err = "Enter a valid email"; return false; }
            if ((pass ?? "").Length < 6) { err = "Password must be 6+ characters"; return false; }
            if (Find(email) != null) { err = "Account already exists"; return false; }
            D.accounts.Add(new UserAccount { name = name, email = email, passHash = Hash(email, pass) });
            D.currentEmail = email;
            D.playerName = name;
            return true;
        }

        public bool Login(string idOrEmail, string pass, out string err)
        {
            err = null;
            var a = Find(idOrEmail);
            if (a == null || a.passHash != Hash(a.email, pass ?? "")) { err = "Wrong email/username or password"; return false; }
            D.currentEmail = a.email;
            D.playerName = a.name;
            return true;
        }

        public bool Reset(string email, out string msg)
        {
            var a = Find(email);
            if (a == null) { msg = "No account found with that email"; return false; }
            msg = "Reset link sent (demo). Connect a real backend to send emails.";
            return true;
        }

        public bool ChangePassword(string oldPass, string newPass, out string err)
        {
            err = null;
            var a = Find(D.currentEmail);
            if (a == null) { err = "Not logged in"; return false; }
            if (a.passHash != Hash(a.email, oldPass ?? "")) { err = "Old password is wrong"; return false; }
            if ((newPass ?? "").Length < 6) { err = "New password must be 6+ characters"; return false; }
            a.passHash = Hash(a.email, newPass);
            return true;
        }

        public void Guest()
        {
            const string email = "guest@lovmates.local";
            if (Find(email) == null)
                D.accounts.Add(new UserAccount { name = "Guest", email = email, passHash = Hash(email, Guid.NewGuid().ToString()) });
            D.currentEmail = email;
            D.playerName = "Guest";
        }
    }

    public class LocalPartner : IPartnerBackend
    {
        public bool Connect(string code, string myCode, out string partnerName, out string err)
        {
            partnerName = "Partner"; err = null;
            if (string.IsNullOrEmpty(code) || code.Length != 6) { err = "Code must be 6 characters"; return false; }
            if (code == myCode) { err = "That's your own code"; return false; }
            return true; // demo: any valid 6-char code connects. Replace with server lookup.
        }
    }

    public static class Services
    {
        public static IAuthBackend Auth = new LocalAuth();
        public static IPartnerBackend Partner = new LocalPartner();
    }
}
