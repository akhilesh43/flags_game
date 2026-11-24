using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LovMates
{
    public class Bar
    {
        public RectTransform fill;
        public void Set(float v) { fill.anchorMax = new Vector2(Mathf.Clamp01(v), 1f); }
    }

    /// <summary>Code-only UI factory. All rects use normalized (0..1) anchors so layouts scale to any screen.</summary>
    public static class UI
    {
        public static readonly Color Green = new Color(0.18f, 0.72f, 0.12f);
        public static readonly Color DarkGreen = new Color(0.03f, 0.25f, 0.06f, 0.92f);
        public static readonly Color Glass = new Color(0.02f, 0.2f, 0.05f, 0.7f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.25f);
        public static readonly Color Pink = new Color(1f, 0.35f, 0.55f);
        public static readonly Color White = Color.white;

        static Font font;
        static Sprite round, circle;

        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.Load<Font>("Fonts/Main"); // drop your own font at Resources/Fonts/Main
                    if (font == null)
                    {
#if UNITY_2022_2_OR_NEWER
                        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
                    }
                }
                return font;
            }
        }

        public static Sprite Round
        {
            get
            {
                if (round == null)
                {
                    int s = 64, r = 22;
                    var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float dx = Mathf.Max(r - x, x - (s - 1 - r), 0f);
                            float dy = Mathf.Max(r - y, y - (s - 1 - r), 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d + 0.5f)));
                        }
                    t.Apply();
                    round = Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
                }
                return round;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    int s = 128;
                    var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float dx = x - (s - 1) / 2f, dy = y - (s - 1) / 2f;
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(s / 2f - d)));
                        }
                    t.Apply();
                    circle = Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100);
                }
                return circle;
            }
        }

        public static Sprite Gradient(Color top, Color bottom)
        {
            var t = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 64; y++) t.SetPixel(0, y, Color.Lerp(bottom, top, y / 63f));
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 1, 64), new Vector2(0.5f, 0.5f), 100);
        }

        // ---------- layout primitives ----------
        public static RectTransform Rect(Transform p, string name, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(p, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(x0, y0);
            rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>Fixed pixel-size rect (reference 1080x1920) pinned to a normalized anchor point.</summary>
        public static RectTransform Pos(Transform p, string name, float ax, float ay, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(p, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = Vector2.zero;
            return rt;
        }

        public static Image Box(Transform p, string name, float x0, float y0, float x1, float y1, Color c, bool rounded = true)
        {
            var rt = Rect(p, name, x0, y0, x1, y1);
            var im = rt.gameObject.AddComponent<Image>();
            im.color = c;
            if (rounded) { im.sprite = Round; im.type = Image.Type.Sliced; }
            return im;
        }

        public static Text Label(Transform p, string text, float x0, float y0, float x1, float y1, int size = 40,
            Color? col = null, TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Bold)
        {
            var rt = Rect(p, "Label", x0, y0, x1, y1);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font; t.text = text; t.fontSize = size; t.color = col ?? White;
            t.alignment = anchor; t.fontStyle = style; t.raycastTarget = false; t.supportRichText = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = size;
            var sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0.12f, 0f, 0.8f); sh.effectDistance = new Vector2(2, -2);
            return t;
        }

        public static Button Btn(Transform p, string text, float x0, float y0, float x1, float y1,
            UnityAction onClick, int size = 44, Color? col = null)
        {
            var img = Box(p, "Btn_" + text, x0, y0, x1, y1, col ?? Green);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var cb = b.colors;
            cb.normalColor = Color.white; cb.highlightedColor = Color.white;
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f); cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            b.colors = cb;
            var ol = img.gameObject.AddComponent<Outline>();
            ol.effectColor = new Color(0.8f, 1f, 0.6f, 0.7f); ol.effectDistance = new Vector2(3, -3);
            if (onClick != null) b.onClick.AddListener(onClick);
            b.onClick.AddListener(() => { if (GameManager.I != null) GameManager.I.Click(); });
            Label(img.transform, text, 0.04f, 0f, 0.96f, 1f, size);
            return b;
        }

        public static void SetLabel(Button b, string s) { b.GetComponentInChildren<Text>().text = s; }
        public static void SetColor(Button b, Color c) { b.GetComponent<Image>().color = c; }

        public static InputField Input(Transform p, string placeholder, float x0, float y0, float x1, float y1, bool password = false)
        {
            var img = Box(p, "Input", x0, y0, x1, y1, new Color(0.02f, 0.18f, 0.04f, 0.92f));
            var f = img.gameObject.AddComponent<InputField>();
            var txt = Label(img.transform, "", 0.05f, 0f, 0.95f, 1f, 38, White, TextAnchor.MiddleLeft, FontStyle.Normal);
            var ph = Label(img.transform, placeholder, 0.05f, 0f, 0.95f, 1f, 38, new Color(1, 1, 1, 0.5f), TextAnchor.MiddleLeft, FontStyle.Italic);
            f.targetGraphic = img; f.textComponent = txt; f.placeholder = ph;
            if (password) f.contentType = InputField.ContentType.Password;
            return f;
        }

        public static Bar MakeBar(Transform p, float x0, float y0, float x1, float y1, Color? col = null)
        {
            var bg = Box(p, "Bar", x0, y0, x1, y1, new Color(0f, 0.12f, 0.03f, 0.9f));
            var fill = Box(bg.transform, "Fill", 0, 0, 1, 1, col ?? Green, false);
            var b = new Bar { fill = fill.rectTransform };
            b.Set(0);
            return b;
        }

        // ---------- scroll lists ----------
        public static ScrollRect Scroll(Transform p, float x0, float y0, float x1, float y1, float spacing = 12)
        {
            var bg = Box(p, "Scroll", x0, y0, x1, y1, Glass);
            var sr = bg.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Elastic;
            bg.gameObject.AddComponent<RectMask2D>();
            var rt = Rect(bg.transform, "Content", 0, 1, 1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.padding = new RectOffset(14, 14, 14, 14);
            v.childControlWidth = true; v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            rt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = rt; sr.viewport = bg.rectTransform;
            return sr;
        }

        public static ScrollRect ScrollGrid(Transform p, float x0, float y0, float x1, float y1, Vector2 cell, int cols, float spacing = 12)
        {
            var bg = Box(p, "ScrollGrid", x0, y0, x1, y1, Glass);
            var sr = bg.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Elastic;
            bg.gameObject.AddComponent<RectMask2D>();
            var rt = Rect(bg.transform, "Content", 0, 1, 1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = cell; g.spacing = new Vector2(spacing, spacing); g.padding = new RectOffset(14, 14, 14, 14);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = cols;
            rt.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = rt; sr.viewport = bg.rectTransform;
            return sr;
        }

        public static Text Row(RectTransform content, string text, float h = 110, Color? bg = null,
            TextAnchor a = TextAnchor.MiddleLeft, UnityAction click = null, int size = 38)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(content, false);
            var im = go.GetComponent<Image>();
            im.sprite = Round; im.type = Image.Type.Sliced; im.color = bg ?? new Color(0.05f, 0.4f, 0.1f, 0.9f);
            go.GetComponent<LayoutElement>().preferredHeight = h;
            if (click != null)
            {
                var b = go.AddComponent<Button>();
                b.targetGraphic = im;
                b.onClick.AddListener(click);
            }
            return Label(go.transform, text, 0.03f, 0f, 0.97f, 1f, size, White, a, FontStyle.Normal);
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject);
        }

        public static void Grid(Transform p, IList<(string label, UnityAction act)> items, int cols,
            float x0, float y0, float x1, float y1, int fs = 36)
        {
            int rows = Mathf.CeilToInt(items.Count / (float)cols);
            float cw = (x1 - x0) / cols, ch = (y1 - y0) / Mathf.Max(1, rows);
            for (int i = 0; i < items.Count; i++)
            {
                int c = i % cols, r = i / cols;
                float cx0 = x0 + c * cw, cy1 = y1 - r * ch;
                Btn(p, items[i].label, cx0 + cw * 0.05f, cy1 - ch * 0.95f, cx0 + cw * 0.95f, cy1 - ch * 0.05f, items[i].act, fs);
            }
        }

        // ---------- overlays ----------
        public static RectTransform Overlay(float x0 = 0.07f, float y0 = 0.3f, float x1 = 0.93f, float y1 = 0.7f)
        {
            var dim = Box(GameManager.I.Root, "Overlay", 0, 0, 1, 1, new Color(0, 0, 0, 0.7f), false);
            return Box(dim.transform, "Card", x0, y0, x1, y1, new Color(0.03f, 0.3f, 0.07f, 0.98f)).rectTransform;
        }

        public static void Close(RectTransform card) { Object.Destroy(card.parent.gameObject); }

        public static void Message(string title, string body)
        {
            var card = Overlay();
            Label(card, title, 0.05f, 0.76f, 0.95f, 0.95f, 54, Gold);
            Label(card, body, 0.06f, 0.28f, 0.94f, 0.76f, 40, White, TextAnchor.MiddleCenter, FontStyle.Normal);
            Btn(card, "OK", 0.3f, 0.06f, 0.7f, 0.22f, () => Close(card), 44);
        }
    }
}
