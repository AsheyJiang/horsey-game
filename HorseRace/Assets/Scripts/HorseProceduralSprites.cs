using System;
using UnityEngine;

namespace HorseRace
{
    internal static class HorseProceduralSprites
    {
        private const int SpriteSize = 64;

        public static Sprite Body(PhysicalHorseController.HorseGenes genes)
        {
            float bellyDepth = Mathf.Clamp01(genes.BodyBellyCurveDepth);
            float skinny = Mathf.Clamp01(genes.BodySkinny);
            float top = Mathf.Lerp(0.88f, 0.80f, skinny);
            float sideBottom = genes.BodyHasCurvedBelly ? Mathf.Lerp(0.16f, 0.22f, skinny) : 0.02f;
            return Shape("HorseBody_" + genes.VisualSeed, delegate(float x, float y)
            {
                if (x < 0.03f || x > 0.97f || y > top)
                {
                    return false;
                }

                float bottom = sideBottom;
                if (genes.BodyHasCurvedBelly)
                {
                    float bodyX = Mathf.Clamp01((x - 0.08f) / 0.84f);
                    float arc = Mathf.Sin(Mathf.PI * bodyX);
                    bottom -= Mathf.Lerp(0.12f, 0.24f, bellyDepth) * arc;
                }

                return y >= Mathf.Max(0.02f, bottom);
            });
        }
        public static Sprite BodyBelly(PhysicalHorseController.HorseGenes genes)
        {
            float depth = Mathf.Clamp01(genes.BodyBellyCurveDepth);
            return Shape("HorseBodyBelly_" + genes.VisualSeed, delegate(float x, float y)
            {
                float arc = Mathf.Sin(Mathf.PI * Mathf.Clamp01(x));
                float top = Mathf.Lerp(0.22f, 0.96f, arc) * Mathf.Lerp(0.65f, 1f, depth);
                return y <= top;
            });
        }

        public static Sprite BodyBackPatch(PhysicalHorseController.HorseGenes genes)
        {
            return Polygon("HorseBodyBackPatch_" + genes.VisualSeed, new[]
            {
                new Vector2(0.00f, 0.70f),
                new Vector2(0.20f, 0.92f),
                new Vector2(0.74f, 0.86f),
                new Vector2(1.00f, 0.66f),
                new Vector2(0.78f, 0.52f),
                new Vector2(0.18f, 0.48f)
            });
        }

        public static Sprite BodyBlockPatch(PhysicalHorseController.HorseGenes genes)
        {
            return Shape("HorseBodyBlockPatch_" + genes.VisualSeed, delegate(float x, float y)
            {
                if (genes.BodyPatternStyle == 0)
                {
                    return false;
                }

                bool shoulder = x > 0.62f && x < 0.92f && y > 0.18f && y < 0.80f;
                bool hip = x > 0.05f && x < 0.24f && y > 0.22f && y < 0.78f;
                bool stripe = x > 0.28f && x < 0.46f && y > 0.48f && y < 0.88f;
                return genes.BodyPatternStyle == 1 ? shoulder : shoulder || hip || stripe;
            });
        }

        public static Sprite Neck(PhysicalHorseController.HorseGenes genes)
        {
            float slope = Mathf.Clamp01(genes.NeckGiraffe);
            return Polygon("HorseNeck_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.18f),
                new Vector2(0.98f, 0.28f + slope * 0.08f),
                new Vector2(0.98f, 0.76f + slope * 0.06f),
                new Vector2(0.02f, 0.84f)
            });
        }
        public static Sprite NeckMane(PhysicalHorseController.HorseGenes genes)
        {
            return Polygon("HorseNeckMane_" + genes.VisualSeed, new[]
            {
                new Vector2(0.00f, 0.72f),
                new Vector2(0.96f, 0.82f),
                new Vector2(0.96f, 1.00f),
                new Vector2(0.10f, 0.92f)
            });
        }

        public static Sprite HeadBase(PhysicalHorseController.HorseGenes genes)
        {
            float square = Mathf.Clamp01(genes.HeadSquare);
            float jaw = Mathf.Lerp(0.22f, 0.16f, square);
            float crown = Mathf.Lerp(0.72f, 0.82f, square);
            return Polygon("HorseHeadBase_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.34f),
                new Vector2(0.30f, jaw),
                new Vector2(0.72f, 0.18f),
                new Vector2(0.98f, 0.34f),
                new Vector2(0.88f, 0.50f),
                new Vector2(0.98f, 0.62f),
                new Vector2(0.62f, crown),
                new Vector2(0.52f, 0.98f),
                new Vector2(0.42f, crown),
                new Vector2(0.08f, 0.62f)
            });
        }
        public static Sprite HeadBack(PhysicalHorseController.HorseGenes genes)
        {
            return Polygon("HorseHeadBack_" + genes.VisualSeed, new[]
            {
                new Vector2(0.05f, 0.20f),
                new Vector2(0.38f, 0.25f),
                new Vector2(0.40f, 0.75f),
                new Vector2(0.04f, 0.86f)
            });
        }

        public static Sprite Muzzle(PhysicalHorseController.HorseGenes genes)
        {
            float jaw = Mathf.Clamp(genes.Jaw, -0.35f, 0.35f);
            return Polygon("HorseMuzzle_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.30f),
                new Vector2(0.82f, 0.18f + jaw * 0.08f),
                new Vector2(0.98f, 0.38f),
                new Vector2(0.78f, 0.62f),
                new Vector2(0.06f, 0.58f)
            });
        }

        public static Sprite Jaw(PhysicalHorseController.HorseGenes genes)
        {
            float jaw = Mathf.Clamp(genes.Jaw, -0.35f, 0.35f);
            return Polygon("HorseJaw_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.36f + jaw * 0.12f),
                new Vector2(0.86f, 0.28f),
                new Vector2(0.98f, 0.48f),
                new Vector2(0.16f, 0.70f)
            });
        }

        public static Sprite Ear(PhysicalHorseController.HorseGenes genes)
        {
            if (genes.EarShape == 1)
            {
                return Polygon("HorseEarRound_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.18f, 0.08f),
                    new Vector2(0.48f, 0.98f),
                    new Vector2(0.82f, 0.10f),
                    new Vector2(0.56f, 0.00f)
                });
            }

            if (genes.EarShape == 4)
            {
                return Polygon("HorseEarWide_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.12f, 0.08f),
                    new Vector2(0.70f, 0.96f),
                    new Vector2(0.92f, 0.02f)
                });
            }

            return Polygon("HorseEarPoint_" + genes.VisualSeed, new[]
            {
                new Vector2(0.18f, 0.04f),
                new Vector2(0.52f, 0.98f),
                new Vector2(0.82f, 0.06f)
            });
        }

        public static Sprite Eye(PhysicalHorseController.HorseGenes genes)
        {
            if (genes.EyeStyle == 2)
            {
                return Polygon("HorseEyeDiamond_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.50f, 0.02f),
                    new Vector2(0.98f, 0.50f),
                    new Vector2(0.50f, 0.98f),
                    new Vector2(0.02f, 0.50f)
                });
            }

            if (genes.EyeStyle == 0)
            {
                return Shape("HorseEyeDot_" + genes.VisualSeed, delegate(float x, float y)
                {
                    return Circle(x, y, 0.5f, 0.5f, 0.34f, 0.34f);
                });
            }

            return Shape("HorseEyeOval_" + genes.VisualSeed, delegate(float x, float y)
            {
                return Circle(x, y, 0.5f, 0.5f, 0.48f, 0.34f);
            });
        }

        public static Sprite Brow(PhysicalHorseController.HorseGenes genes)
        {
            return Polygon("HorseBrow_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.58f),
                new Vector2(0.94f, 0.34f),
                new Vector2(0.98f, 0.50f),
                new Vector2(0.08f, 0.74f)
            });
        }

        public static Sprite Nose(PhysicalHorseController.HorseGenes genes)
        {
            if (genes.NoseStyle == 2)
            {
                return Polygon("HorseNoseSlit_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.28f, 0.06f),
                    new Vector2(0.80f, 0.22f),
                    new Vector2(0.70f, 0.92f),
                    new Vector2(0.18f, 0.74f)
                });
            }

            if (genes.NoseStyle == 3)
            {
                return Polygon("HorseNoseTri_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.08f, 0.20f),
                    new Vector2(0.88f, 0.50f),
                    new Vector2(0.08f, 0.82f)
                });
            }

            return Shape("HorseNoseDot_" + genes.VisualSeed, delegate(float x, float y)
            {
                return Circle(x, y, 0.5f, 0.5f, 0.36f, 0.42f);
            });
        }

        public static Sprite Mouth()
        {
            return Shape("HorseMouth", delegate(float x, float y)
            {
                return y > 0.44f && y < 0.56f && x > 0.02f && x < 0.98f;
            });
        }

        public static Sprite Teeth(PhysicalHorseController.HorseGenes genes)
        {
            if (genes.TeethShape == 2)
            {
                return Shape("HorseTeethBars_" + genes.VisualSeed, delegate(float x, float y)
                {
                    return y < 0.92f && (Mathf.FloorToInt(x * 5f) % 2 == 0);
                });
            }

            return Polygon("HorseTeeth_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.10f),
                new Vector2(0.96f, 0.20f),
                new Vector2(0.82f, 0.92f),
                new Vector2(0.08f, 0.82f)
            });
        }

        public static Sprite Tongue(PhysicalHorseController.HorseGenes genes)
        {
            return Shape("HorseTongue_" + genes.VisualSeed, delegate(float x, float y)
            {
                return Circle(x, y, 0.45f, 0.58f, 0.50f, 0.28f) && x > 0.08f;
            });
        }

        public static Sprite Tail(PhysicalHorseController.HorseGenes genes)
        {
            float half = Mathf.Clamp(genes.TailBaseWidth * 0.5f, 0.10f, 0.48f);
            if (genes.TailShape == 1)
            {
                return Polygon("HorseTailCrooked_" + genes.VisualSeed, new[]
                {
                    new Vector2(0.98f, 0.50f),
                    new Vector2(0.40f, 0.42f),
                    new Vector2(0.06f, 0.50f - half),
                    new Vector2(0.24f, 0.50f + half),
                    new Vector2(0.48f, 0.58f)
                });
            }

            return Polygon("HorseTailTri_" + genes.VisualSeed, new[]
            {
                new Vector2(0.98f, 0.50f),
                new Vector2(0.02f, 0.50f - half),
                new Vector2(0.02f, 0.50f + half)
            });
        }

        public static Sprite Leg(PhysicalHorseController.HorseGenes genes, bool lower)
        {
            float taper = lower ? 0.08f : 0.04f;
            return Polygon("HorseLeg_" + lower + "_" + genes.VisualSeed, new[]
            {
                new Vector2(0.22f + taper, 0.00f),
                new Vector2(0.78f - taper, 0.00f),
                new Vector2(0.70f, 1.00f),
                new Vector2(0.30f, 1.00f)
            });
        }
        public static Sprite Knee()
        {
            return Polygon("HorseKnee", new[]
            {
                new Vector2(0.10f, 0.36f),
                new Vector2(0.90f, 0.30f),
                new Vector2(0.92f, 0.66f),
                new Vector2(0.12f, 0.72f)
            });
        }

        public static Sprite Foot(PhysicalHorseController.HorseGenes genes)
        {
            float slant = Mathf.Clamp01(genes.FootToeSlant);
            return Polygon("HorseFoot_" + genes.VisualSeed, new[]
            {
                new Vector2(0.02f, 0.28f),
                new Vector2(0.74f, 0.18f),
                new Vector2(0.98f, 0.18f + slant * 0.44f),
                new Vector2(0.88f, 0.66f),
                new Vector2(0.10f, 0.76f)
            });
        }

        private static Sprite Polygon(string name, Vector2[] points)
        {
            return Shape(name, delegate(float x, float y)
            {
                return IsPointInPolygon(new Vector2(x, y), points);
            });
        }

        private static Sprite Shape(string name, Func<float, float, bool> isInside)
        {
            Texture2D texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false);
            texture.name = name;
            texture.filterMode = FilterMode.Point;

            Color32 clear = new Color32(255, 255, 255, 0);
            Color32 fill = new Color32(255, 255, 255, 255);
            for (int y = 0; y < SpriteSize; y++)
            {
                for (int x = 0; x < SpriteSize; x++)
                {
                    float u = (x + 0.5f) / SpriteSize;
                    float v = (y + 0.5f) / SpriteSize;
                    texture.SetPixel(x, y, isInside(u, v) ? fill : clear);
                }
            }

            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0f, 0f, SpriteSize, SpriteSize), new Vector2(0.5f, 0.5f), SpriteSize);
        }

        private static bool Circle(float x, float y, float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / Mathf.Max(0.001f, rx);
            float dy = (y - cy) / Mathf.Max(0.001f, ry);
            return dx * dx + dy * dy <= 1f;
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[j];
                bool crosses = (a.y > point.y) != (b.y > point.y);
                if (crosses)
                {
                    float x = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                    if (point.x < x)
                    {
                        inside = !inside;
                    }
                }
            }

            return inside;
        }
    }
}


