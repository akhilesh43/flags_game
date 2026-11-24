using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LovMates
{
    public class Item
    {
        public string id, name; public int price;
        public Item(string i, string n, int p) { id = i; name = n; price = p; }
    }

    public static class Items
    {
        public static readonly Item[] Gifts = {
            new Item("rose", "Red Rose", 20), new Item("choco", "Chocolate Box", 40), new Item("flowers", "Flower Bouquet", 60),
            new Item("cake", "Love Cake", 80), new Item("teddy", "Teddy Bear", 120), new Item("giftbox", "Surprise Gift Box", 150),
            new Item("ring", "Promise Ring", 500) };
        // index in this array == CharacterLook.outfit
        public static readonly Item[] Clothes = {
            new Item("outfit0", "Green Hoodie Set", 0), new Item("outfit1", "Blue Denim Look", 100), new Item("outfit2", "Pink Party Wear", 150),
            new Item("outfit3", "Orange Street Style", 120), new Item("outfit4", "Purple Evening Look", 200), new Item("outfit5", "White Classic", 80) };
        public static readonly Item[] Decor = {
            new Item("lamp", "Warm Lamp", 60), new Item("plant", "Indoor Plant", 40), new Item("rug", "Cozy Rug", 90),
            new Item("painting", "Wall Painting", 110), new Item("sofa", "Love Sofa", 150), new Item("tv", "Smart TV", 200), new Item("bed", "Royal Bed", 300) };
    }

    public enum CatalogMode { Shop, Wardrobe, Decor }

    // 26 Gift Shop (Buy/Gift), 27 Wardrobe (Try/Wear), 28 Room Decoration (Decorate/Save)
    public class CatalogScreen : ScreenBase
    {
        CatalogMode mode; string title; Item[] items;
        int sel = -1;
        ScrollRect list; Text info; AvatarView preview;

        public void Setup(CatalogMode m, string t, Item[] it) { mode = m; title = t; items = it; }

        protected override void Build()
        {
            Title(title); BackBtn();
            float top = 0.88f;
            if (mode == CatalogMode.Wardrobe)
            {
                preview = AvatarView.Create(R, "Preview", 0.5f, 0.75f, 240, 430);
                top = 0.58f;
            }
            list = UI.Scroll(R, 0.05f, 0.23f, 0.95f, top);
            info = UI.Label(R, "", 0.05f, 0.165f, 0.95f, 0.22f, 36, UI.Gold);
            string a = mode == CatalogMode.Shop ? "Buy" : mode == CatalogMode.Wardrobe ? "Try" : "Decorate";
            string b = mode == CatalogMode.Shop ? "Gift" : mode == CatalogMode.Wardrobe ? "Wear" : "Save";
            UI.Btn(R, a, 0.05f, 0.07f, 0.48f, 0.15f, ActionA, 52);
            UI.Btn(R, b, 0.52f, 0.07f, 0.95f, 0.15f, ActionB, 52, UI.Gold);
        }

        public override void OnShow() { sel = -1; Refresh(); }

        int Count(string id) { int n = 0; foreach (var s in D.inventory) if (s == id) n++; return n; }
        bool Owned(Item it) { return it.price == 0 || Count(it.id) > 0; }

        void Refresh()
        {
            UI.Clear(list.content);
            for (int i = 0; i < items.Length; i++)
            {
                int idx = i; var it = items[i];
                string tag;
                if (mode == CatalogMode.Shop) tag = it.price + " coins" + (Count(it.id) > 0 ? "   (x" + Count(it.id) + ")" : "");
                else if (mode == CatalogMode.Wardrobe) tag = Owned(it) ? "owned" : it.price + " coins";
                else tag = it.price + " coins";
                UI.Row(list.content, it.name + "   -   " + tag, 100,
                    sel == i ? new Color(0.8f, 0.6f, 0.1f, 0.95f) : (Color?)null, TextAnchor.MiddleLeft, () => { sel = idx; Refresh(); });
            }
            if (mode == CatalogMode.Shop) info.text = "Coins: " + D.coins.ToString("N0");
            else if (mode == CatalogMode.Wardrobe) { info.text = "Wearing: " + Items.Clothes[G.MyLook.outfit % Items.Clothes.Length].name; preview.Set(G.MyGender, G.MyLook); }
            else info.text = "Placed: " + D.placedDecor.Count + "   Saved: " + D.savedDecor.Count;
        }

        bool Picked() { if (sel < 0) { G.Toast("Select an item first"); return false; } return true; }

        void ActionA()
        {
            if (!Picked()) return;
            var it = items[sel];
            if (mode == CatalogMode.Shop)
            {
                if (!G.SpendCoins(it.price)) return;
                D.inventory.Add(it.id); G.Toast("Bought " + it.name); Refresh();
            }
            else if (mode == CatalogMode.Wardrobe)
            {
                var l = G.MyLook;
                var tmp = new CharacterLook { model = l.model, hair = l.hair, outfit = sel, shoes = l.shoes, accessory = l.accessory, skin = l.skin };
                preview.Set(G.MyGender, tmp);
            }
            else
            {
                if (!G.SpendCoins(it.price)) return;
                D.placedDecor.Add(it.id); G.AddXp(3); G.Toast(it.name + " placed"); Refresh();
            }
        }

        void ActionB()
        {
            if (mode == CatalogMode.Decor)
            {
                D.savedDecor = new List<string>(D.placedDecor); G.Save(); G.Toast("Room saved"); Refresh(); return;
            }
            if (!Picked()) return;
            var it = items[sel];
            if (mode == CatalogMode.Shop)
            {
                if (!D.inventory.Remove(it.id)) { G.Toast("Buy it first"); return; }
                int love = it.price / 10 + 5;
                G.AddAffection(love); G.AddXp(5);
                UI.Message("Gift Sent", D.partnerName + " loved the " + it.name + "!\n+" + love + " love");
                Refresh();
            }
            else
            {
                if (!Owned(it)) { if (!G.SpendCoins(it.price)) return; D.inventory.Add(it.id); }
                G.MyLook.outfit = sel; G.Save(); G.Toast("Now wearing " + it.name); Refresh();
            }
        }
    }
}
