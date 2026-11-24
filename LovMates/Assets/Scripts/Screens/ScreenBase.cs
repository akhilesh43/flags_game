using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LovMates
{
    public abstract class ScreenBase : MonoBehaviour
    {
        [HideInInspector] public ScreenId Id;
        protected RectTransform R;
        protected GameManager G { get { return GameManager.I; } }
        protected SaveData D { get { return GameManager.I.Data; } }

        protected virtual Color TopColor { get { return new Color(0.16f, 0.55f, 0.14f); } }
        protected virtual Color BottomColor { get { return new Color(0.02f, 0.2f, 0.05f); } }

        public void Init()
        {
            R = (RectTransform)transform;
            R.anchorMin = Vector2.zero; R.anchorMax = Vector2.one;
            R.offsetMin = R.offsetMax = Vector2.zero;
            var img = gameObject.AddComponent<Image>();
            // Put your clean background art at Resources/Backgrounds/NN.png (NN = screen number, e.g. 11.png)
            var bg = Art.Sprite("Backgrounds/" + ((int)Id).ToString("00"));
            if (bg != null) { img.sprite = bg; img.color = Color.white; }
            else img.sprite = UI.Gradient(TopColor, BottomColor);
            Build();
        }

        protected abstract void Build();
        public virtual void OnShow() { }
        public virtual void OnHide() { }

        protected Text Title(string t, Transform p = null)
        {
            return UI.Label(p ?? R, t, 0.18f, 0.925f, 0.82f, 0.985f, 60, UI.White);
        }

        protected Button BackBtn(Transform p = null, UnityAction act = null)
        {
            UnityAction a = act ?? new UnityAction(() => G.Back());
            return UI.Btn(p ?? R, "<", 0.03f, 0.93f, 0.15f, 0.985f, a, 52, UI.DarkGreen);
        }
    }
}
