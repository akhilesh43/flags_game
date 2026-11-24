using System.Collections.Generic;
using UnityEngine;

namespace LovMates
{
    /// <summary>
    /// Loads art/audio from Resources. Works even if the PNG/JPG was imported as a plain Texture2D
    /// (no manual "Sprite" import setting needed) - it builds the Sprite at runtime and caches it.
    /// </summary>
    public static class Art
    {
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        public static Sprite Sprite(string path)
        {
            Sprite s;
            if (sprites.TryGetValue(path, out s)) return s;
            s = Resources.Load<Sprite>(path);
            if (s == null)
            {
                var tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    s = UnityEngine.Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            sprites[path] = s;
            return s;
        }

        public static AudioClip Clip(string name)
        {
            AudioClip c;
            if (clips.TryGetValue(name, out c)) return c;
            c = Resources.Load<AudioClip>("Audio/" + name);
            clips[name] = c;
            return c;
        }
    }
}
