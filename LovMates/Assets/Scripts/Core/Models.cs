using System;
using System.Collections.Generic;

namespace LovMates
{
    // Numbers match the 40 screens in the design sheet.
    public enum ScreenId
    {
        None = 0, Splash = 1, Welcome, Login, Register, Forgot, BoySelect, GirlSelect, Customize,
        CoupleConnect, Loading, Home, OurHome, Bedroom, LivingRoom, Kitchen, Garden, LovePark, Beach,
        LoveCity, Cafe, RomanticDate, Interaction, HugKiss, Dance, Pet, GiftShop, Wardrobe, Decor,
        Diary, Gallery, Chat, Letters, Mood, Goals, SpecialDays, Missions, Settings, Pause, Event, End
    }

    [Serializable] public class UserAccount { public string name; public string email; public string passHash; }
    [Serializable] public class CharacterLook { public int model; public int hair; public int outfit; public int shoes; public int accessory; public int skin; }
    [Serializable] public class ChatMsg { public bool mine; public string text; public string time; }
    [Serializable] public class DiaryEntry { public string date; public string text; }
    [Serializable] public class MoodEntry { public string day; public int mood; }
    [Serializable] public class GoalItem { public string title; public bool done; public bool rewarded; }
    [Serializable] public class MissionState { public string key; public string title; public int progress; public int target; public int reward; public bool claimed; }

    [Serializable]
    public class SaveData
    {
        public List<UserAccount> accounts = new List<UserAccount>();
        public string currentEmail = "";
        public string playerName = "Player";

        public int level = 1, xp = 0, coins = 1250, gems = 100, affection = 0;

        public bool characterCreated, connected;
        public int myGender = 0; // 0 = boy, 1 = girl
        public CharacterLook boy = new CharacterLook();
        public CharacterLook girl = new CharacterLook();

        public string myCode = "", partnerCode = "", partnerName = "Partner";
        public string anniversary = "";

        public List<string> inventory = new List<string>();
        public List<string> placedDecor = new List<string>();
        public List<string> savedDecor = new List<string>();
        public List<string> visited = new List<string>();
        public List<string> gallery = new List<string>();

        public string petName = "Bubbles";
        public int petHunger = 80, petHappy = 80;
        public string petTick = "";

        public List<ChatMsg> chat = new List<ChatMsg>();
        public List<DiaryEntry> diary = new List<DiaryEntry>();
        public List<MoodEntry> moods = new List<MoodEntry>();
        public List<GoalItem> goals = new List<GoalItem>();
        public List<MissionState> missions = new List<MissionState>();
        public string missionsDay = "";

        public bool sound = true;
        public int quality = 2;
        public int language = 0;

        public string lastLetterDay = "";
        public int lettersOpened = 0;
        public string lastEventDay = "";
    }
}
