using UnityEngine;
using UnityEngine.UI;

namespace HorseRace
{
    [AddComponentMenu("UI/Vertical Gradient Graphic")]
    public sealed class VerticalGradientGraphic : MaskableGraphic
    {
        [SerializeField] private Color[] gradientColors =
        {
            new Color32(53, 42, 145, 255),
            new Color32(67, 76, 175, 255),
            new Color32(71, 81, 188, 255),
            new Color32(53, 42, 145, 255),
            new Color32(53, 42, 145, 255)
        };

        [SerializeField] private float[] gradientStops =
        {
            0f,
            0.33235f,
            0.62524f,
            0.90344f,
            1f
        };

        public void Configure(Color[] colors, float[] stops)
        {
            if (colors == null || stops == null || colors.Length < 2 || colors.Length != stops.Length)
            {
                return;
            }

            gradientColors = colors;
            gradientStops = stops;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();
            if (gradientColors == null || gradientStops == null ||
                gradientColors.Length < 2 || gradientColors.Length != gradientStops.Length)
            {
                return;
            }

            Rect rect = rectTransform.rect;
            for (int i = 0; i < gradientColors.Length; i++)
            {
                float stop = Mathf.Clamp01(gradientStops[i]);
                float y = Mathf.Lerp(rect.yMax, rect.yMin, stop);
                AddVertex(vertexHelper, new Vector2(rect.xMin, y), gradientColors[i]);
                AddVertex(vertexHelper, new Vector2(rect.xMax, y), gradientColors[i]);

                if (i == 0)
                {
                    continue;
                }

                int current = i * 2;
                int previous = current - 2;
                vertexHelper.AddTriangle(previous, current, current + 1);
                vertexHelper.AddTriangle(previous, current + 1, previous + 1);
            }
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color color)
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.position = position;
            vertex.color = color;
            vertexHelper.AddVert(vertex);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
