using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using UnityEngine;

namespace HorseRace
{
    public sealed class HorseyAtlas
    {
        private readonly Texture2D texture;
        private readonly Dictionary<string, AtlasEntry> entries = new Dictionary<string, AtlasEntry>();
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private HorseyAtlas(Texture2D texture, TextAsset xml)
        {
            this.texture = texture;
            this.texture.filterMode = FilterMode.Point;
            this.texture.wrapMode = TextureWrapMode.Clamp;
            ParseXml(xml);
        }

        public static HorseyAtlas Load(string resourceName)
        {
            Texture2D texture = Resources.Load<Texture2D>("HorseyGame/" + resourceName);
            TextAsset xml = Resources.Load<TextAsset>("HorseyGame/" + resourceName);

            if (texture == null || xml == null)
            {
                Debug.LogWarning($"Horsey atlas '{resourceName}' is missing a texture or XML asset.");
                return null;
            }

            return new HorseyAtlas(texture, xml);
        }

        public Sprite Get(string spriteName)
        {
            if (sprites.TryGetValue(spriteName, out Sprite cached))
            {
                return cached;
            }

            if (!entries.TryGetValue(spriteName, out AtlasEntry entry))
            {
                Debug.LogWarning($"Sprite '{spriteName}' was not found in Horsey atlas '{texture.name}'.");
                return null;
            }

            Rect rect = new Rect(entry.X, texture.height - entry.Y - entry.Height, entry.Width, entry.Height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(texture, rect, pivot, 16f, 0, SpriteMeshType.FullRect);
            sprite.name = spriteName;
            sprites[spriteName] = sprite;
            return sprite;
        }

        private void ParseXml(TextAsset xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml.text);

            XmlNodeList nodes = document.SelectNodes("//sprite");
            if (nodes == null)
            {
                return;
            }

            foreach (XmlNode node in nodes)
            {
                string name = Attribute(node, "n");
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                entries[name] = new AtlasEntry
                {
                    X = Number(node, "x"),
                    Y = Number(node, "y"),
                    Width = Number(node, "w"),
                    Height = Number(node, "h")
                };
            }
        }

        private static string Attribute(XmlNode node, string name)
        {
            return node.Attributes?[name]?.Value;
        }

        private static float Number(XmlNode node, string name)
        {
            string value = Attribute(node, name);
            if (string.IsNullOrEmpty(value))
            {
                return 0f;
            }

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        private struct AtlasEntry
        {
            public float X;
            public float Y;
            public float Width;
            public float Height;
        }
    }
}
