using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LovMates
{
    public static class CharacterArt
    {
        public static readonly Color[] Outfit = {
            new Color(.15f,.7f,.15f), new Color(.2f,.45f,.95f), new Color(.95f,.4f,.65f),
            new Color(.98f,.6f,.15f), new Color(.6f,.35f,.9f), new Color(.95f,.95f,.95f) };
        public static readonly Color[] Hair = {
            new Color(.1f,.07f,.05f), new Color(.4f,.22f,.1f), new Color(.9f,.75f,.3f),
            new Color(.7f,.2f,.1f), new Color(.1f,.6f,.6f), new Color(.85f,.85f,.85f) };
        public static readonly Color[] Skin = {
            new Color(1f,.87f,.75f), new Color(.95f,.77f,.6f), new Color(.85f,.65f,.45f),
            new Color(.7f,.5f,.35f), new Color(.5f,.35f,.25f), new Color(.98f,.8f,.8f) };
        public static readonly Color[] Shoes = {
            Color.white, new Color(.1f,.1f,.1f), new Color(.4f,.25f,.12f),
            new Color(.8f,.15f,.15f), new Color(.2f,.3f,.8f), new Color(.1f,.5f,.1f) };
        public static readonly Color[] Acc = {
            new Color(0,0,0,0), UI.Gold, new Color(.8f,.8f,.85f),
            UI.Pink, new Color(.2f,.9f,.9f), new Color(.9f,.1f,.1f) };

        public static Color P(Color[] a, int i) { return a[Mathf.Abs(i) % a.Length]; }

        /// <summary>Drop sprites at Resources/Characters/boy_0.png, girl_0.png ... to replace placeholders.</summary>
        public static Sprite Get(int gender, int model)
        {
            return Art.Sprite("Characters/" + (gender == 0 ? "boy" : "girl") + "_" + model);
        }
    }

    public class AvatarView : MonoBehaviour
    {
        public RectTransform rt;
        Image art, body, shoes, head, hair, acc;
        RectTransform parts;
        Text tag;

        public static AvatarView Create(Transform parent, string name, float ax, float ay, float w, float h)
        {
            var r = UI.Pos(parent, name, ax, ay, w, h);
            var v = r.gameObject.AddComponent<AvatarView>();
            v.rt = r;
            v.art = UI.Box(r, "Art", 0, 0, 1, 1, Color.white, false);
            v.art.preserveAspect = true;
            v.parts = UI.Rect(r, "Parts", 0, 0, 1, 1);
            v.body = UI.Box(v.parts, "Body", 0.12f, 0.00f, 0.88f, 0.66f, UI.Green);
            v.shoes = UI.Box(v.parts, "Shoes", 0.12f, 0.00f, 0.88f, 0.08f, Color.white);
            v.head = UI.Box(v.parts, "Head", 0.22f, 0.64f, 0.78f, 0.93f, Color.white);
            v.hair = UI.Box(v.parts, "Hair", 0.19f, 0.84f, 0.81f, 0.98f, Color.black);
            v.acc = UI.Box(v.parts, "Acc", 0.38f, 0.50f, 0.62f, 0.58f, Color.clear);
            v.tag = UI.Label(v.parts, "", 0f, 0.2f, 1f, 0.45f, 40, UI.White);
            foreach (var g in r.GetComponentsInChildren<Graphic>()) g.raycastTarget = false;
            return v;
        }

        public void Set(int gender, CharacterLook look)
        {
            var sp = CharacterArt.Get(gender, look.model);
            art.gameObject.SetActive(sp != null);
            parts.gameObject.SetActive(sp == null);
            if (sp != null) { art.sprite = sp; return; }
            body.color = CharacterArt.P(CharacterArt.Outfit, look.outfit);
            shoes.color = CharacterArt.P(CharacterArt.Shoes, look.shoes);
            head.color = CharacterArt.P(CharacterArt.Skin, look.skin);
            hair.color = CharacterArt.P(CharacterArt.Hair, look.hair);
            acc.color = CharacterArt.P(CharacterArt.Acc, look.accessory);
            tag.text = (gender == 0 ? "BOY " : "GIRL ") + (look.model + 1);
            tag.color = look.outfit == 5 ? Color.black : Color.white;
        }
    }

    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform knob;
        public Vector2 Value;
        RectTransform rt;

        void Awake() { rt = (RectTransform)transform; }
        public void OnPointerDown(PointerEventData e) { OnDrag(e); }
        public void OnDrag(PointerEventData e)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out lp);
            float r = rt.rect.width * 0.5f;
            Value = Vector2.ClampMagnitude(lp / r, 1f);
            if (knob != null) knob.anchoredPosition = Value * r * 0.6f;
        }
        public void OnPointerUp(PointerEventData e)
        {
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>Walkable 2D stage: joystick moves the player, partner follows, doors show a live "Enter" button.</summary>
    public class WorldView : MonoBehaviour
    {
        class Door { public string name; public RectTransform rt; public Action go; }

        RectTransform stage;
        AvatarView me, partner;
        VirtualJoystick joy;
        Button enter;
        Text enterLabel;
        readonly List<Door> doors = new List<Door>();
        Action cur;
        float bob;
        public float speed = 650f;

        public static WorldView Create(RectTransform parent, float x0, float y0, float x1, float y1)
        {
            var rt = UI.Rect(parent, "World", x0, y0, x1, y1);
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = new Color(0f, 0.15f, 0.03f, 0.35f); bg.raycastTarget = false;
            rt.gameObject.AddComponent<RectMask2D>();
            var v = rt.gameObject.AddComponent<WorldView>();
            v.stage = rt;
            v.partner = AvatarView.Create(rt, "Partner", 0.5f, 0.5f, 200, 360);
            v.me = AvatarView.Create(rt, "Me", 0.5f, 0.5f, 200, 360);
            v.partner.rt.anchoredPosition = new Vector2(-150, 0);

            var jr = UI.Pos(parent, "Joystick", 0.2f, 0.2f, 320, 320);
            var jb = jr.gameObject.AddComponent<Image>();
            jb.sprite = UI.Circle; jb.color = new Color(1, 1, 1, 0.2f);
            var knob = UI.Pos(jr, "Knob", 0.5f, 0.5f, 130, 130);
            var kb = knob.gameObject.AddComponent<Image>();
            kb.sprite = UI.Circle; kb.color = new Color(0.3f, 0.9f, 0.2f, 0.9f); kb.raycastTarget = false;
            v.joy = jr.gameObject.AddComponent<VirtualJoystick>();
            v.joy.knob = knob;

            v.enter = UI.Btn(parent, "Enter", 0.3f, 0.295f, 0.7f, 0.365f, () => { if (v.cur != null) v.cur(); }, 52, UI.Gold);
            v.enterLabel = v.enter.GetComponentInChildren<Text>();
            v.enter.gameObject.SetActive(false);
            return v;
        }

        public void Refresh()
        {
            var g = GameManager.I;
            me.Set(g.MyGender, g.MyLook);
            partner.Set(g.PartnerGender, g.PartnerLook);
        }

        public void AddDoor(string label, Vector2 pos, Action go)
        {
            var r = UI.Pos(stage, "Door_" + label, 0.5f, 0.5f, 270, 130);
            r.anchoredPosition = pos;
            var im = r.gameObject.AddComponent<Image>();
            im.sprite = UI.Round; im.type = Image.Type.Sliced; im.color = new Color(1f, 0.85f, 0.25f, 0.35f);
            im.raycastTarget = false;
            UI.Label(r, label, 0, 0, 1, 1, 36, UI.White);
            r.SetAsFirstSibling();
            doors.Add(new Door { name = label, rt = r, go = go });
        }

        void Update()
        {
            Vector2 v = joy.Value;
            if (v.sqrMagnitude > 0.01f)
            {
                Vector2 half = stage.rect.size * 0.5f - new Vector2(100, 180);
                Vector2 p = me.rt.anchoredPosition + v * speed * Time.deltaTime;
                p.x = Mathf.Clamp(p.x, -half.x, half.x);
                p.y = Mathf.Clamp(p.y, -half.y, half.y);
                me.rt.anchoredPosition = p;
                bob += Time.deltaTime * 14f;
                me.rt.localEulerAngles = new Vector3(0, 0, Mathf.Sin(bob) * 4f);
            }
            else me.rt.localEulerAngles = Vector3.zero;

            Vector2 target = me.rt.anchoredPosition + new Vector2(-170, 0);
            partner.rt.anchoredPosition = Vector2.Lerp(partner.rt.anchoredPosition, target, Time.deltaTime * 3f);

            int bi = -1; float bd = 240f;
            for (int i = 0; i < doors.Count; i++)
            {
                float d = Vector2.Distance(me.rt.anchoredPosition, doors[i].rt.anchoredPosition);
                if (d < bd) { bd = d; bi = i; }
            }
            if (bi >= 0) { enterLabel.text = "Enter " + doors[bi].name; cur = doors[bi].go; }
            else cur = null;
            if (enter.gameObject.activeSelf != (bi >= 0)) enter.gameObject.SetActive(bi >= 0);
        }
    }
}
