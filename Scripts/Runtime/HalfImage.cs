using UnityEngine;
using UnityEngine.UI;

namespace Slice25Image.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/HalfImage", 11)]
    public class HalfImage : Image
    {
        private static readonly float[] VertexXs = new float[4];
        private static readonly float[] VertexYs = new float[3];
        private static readonly float[] UvXs = new float[4];
        private static readonly float[] UvYs = new float[3];

        [Min(0)]
        public int TopY;

        private Sprite ActiveSprite => overrideSprite;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            Sprite activeSprite = ActiveSprite;
            if (activeSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            if (!hasBorder)
            {
                SlicedImageUtility.GenerateSimpleSprite(this, activeSprite, toFill);
                return;
            }

            GenerateMirroredSprite(activeSprite, toFill);
        }

        private void GenerateMirroredSprite(Sprite activeSprite, VertexHelper toFill)
        {
            Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite) / multipliedPixelsPerUnit;
            Rect rect = GetPixelAdjustedRect();
            float contentWidth = Mathf.Max(0f, rect.width - padding.x - padding.z);
            float contentHeight = Mathf.Max(0f, rect.height - padding.y - padding.w);
            float spriteWidth = activeSprite.rect.width / multipliedPixelsPerUnit;
            float spriteHeight = activeSprite.rect.height / multipliedPixelsPerUnit;
            float halfWidth = Mathf.Min(spriteWidth, contentWidth * 0.5f);
            float topRatio = Mathf.Clamp01(TopY / activeSprite.rect.height);
            float topHeight = contentHeight >= spriteHeight
                ? Mathf.Min(TopY / multipliedPixelsPerUnit, contentHeight)
                : contentHeight * topRatio;

            VertexXs[0] = rect.x + padding.x;
            VertexXs[1] = VertexXs[0] + halfWidth;
            VertexXs[3] = rect.xMax - padding.z;
            VertexXs[2] = VertexXs[3] - halfWidth;

            VertexYs[0] = rect.y + padding.y;
            VertexYs[1] = VertexYs[0] + topHeight;
            VertexYs[2] = rect.yMax - padding.w;

            UvXs[0] = outer.x;
            UvXs[1] = outer.z;
            UvXs[2] = outer.z;
            UvXs[3] = outer.x;

            UvYs[0] = outer.y;
            UvYs[1] = Mathf.Lerp(outer.y, outer.w, topRatio);
            UvYs[2] = outer.w;

            toFill.Clear();
            for (int x = 0; x < VertexXs.Length - 1; x++)
            {
                for (int y = 0; y < VertexYs.Length - 1; y++)
                {
                    if (!SlicedImageUtility.HasPositiveArea(VertexXs, x, VertexYs, y))
                    {
                        continue;
                    }

                    SlicedImageUtility.AddQuad(
                        toFill,
                        new Vector2(VertexXs[x], VertexYs[y]),
                        new Vector2(VertexXs[x + 1], VertexYs[y + 1]),
                        color,
                        new Vector2(UvXs[x], UvYs[y]),
                        new Vector2(UvXs[x + 1], UvYs[y + 1]));
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            TopY = Mathf.Max(0, TopY);
            base.OnValidate();
        }
#endif
    }
}
