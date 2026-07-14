using UnityEngine;
using UnityEngine.UI;

namespace Slice25Image.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Slice25Image", 11)]
    public class Slice25Image : Image
    {
        private static readonly Vector2[] VertexScratch = new Vector2[6];
        private static readonly Vector2[] UvScratch = new Vector2[6];

        [Range(0f, 1f)]
        public float horizontalRatio = 1f;

        [Range(0f, 1f)]
        public float verticalRatio = 1f;

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

            GenerateSlicedSprite(activeSprite, toFill);
        }

        private void GenerateSlicedSprite(Sprite activeSprite, VertexHelper toFill)
        {
            Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
            Vector4 inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite) / multipliedPixelsPerUnit;
            Vector4 border = activeSprite.border;
            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = SlicedImageUtility.GetAdjustedBorders(
                this,
                border / multipliedPixelsPerUnit,
                rect);

            VertexScratch[0] = new Vector2(padding.x, padding.y);
            VertexScratch[1] = new Vector2(adjustedBorders.x, adjustedBorders.y);
            VertexScratch[4] = new Vector2(rect.width - adjustedBorders.z, rect.height - adjustedBorders.w);
            VertexScratch[5] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            float availableWidth = Mathf.Max(0f, VertexScratch[4].x - VertexScratch[1].x);
            float availableHeight = Mathf.Max(0f, VertexScratch[4].y - VertexScratch[1].y);
            float centerWidth = Mathf.Min(
                Mathf.Max(0f, activeSprite.rect.width - border.x - border.z) / multipliedPixelsPerUnit,
                availableWidth);
            float centerHeight = Mathf.Min(
                Mathf.Max(0f, activeSprite.rect.height - border.y - border.w) / multipliedPixelsPerUnit,
                availableHeight);
            float horizontalOffset = (availableWidth - centerWidth) * Mathf.Clamp01(horizontalRatio);
            float verticalOffset = (availableHeight - centerHeight) * Mathf.Clamp01(verticalRatio);

            VertexScratch[2] = new Vector2(
                VertexScratch[1].x + horizontalOffset,
                VertexScratch[1].y + verticalOffset);
            VertexScratch[3] = new Vector2(
                VertexScratch[2].x + centerWidth,
                VertexScratch[2].y + centerHeight);

            for (int i = 0; i < VertexScratch.Length; i++)
            {
                VertexScratch[i].x += rect.x;
                VertexScratch[i].y += rect.y;
            }

            UvScratch[0] = new Vector2(outer.x, outer.y);
            UvScratch[1] = new Vector2(inner.x, inner.y);
            UvScratch[2] = new Vector2(inner.x, inner.y);
            UvScratch[3] = new Vector2(inner.z, inner.w);
            UvScratch[4] = new Vector2(inner.z, inner.w);
            UvScratch[5] = new Vector2(outer.z, outer.w);

            toFill.Clear();
            for (int x = 0; x < VertexScratch.Length - 1; x++)
            {
                for (int y = 0; y < VertexScratch.Length - 1; y++)
                {
                    if (VertexScratch[x + 1].x <= VertexScratch[x].x ||
                        VertexScratch[y + 1].y <= VertexScratch[y].y)
                    {
                        continue;
                    }

                    SlicedImageUtility.AddQuad(
                        toFill,
                        new Vector2(VertexScratch[x].x, VertexScratch[y].y),
                        new Vector2(VertexScratch[x + 1].x, VertexScratch[y + 1].y),
                        color,
                        new Vector2(UvScratch[x].x, UvScratch[y].y),
                        new Vector2(UvScratch[x + 1].x, UvScratch[y + 1].y));
                }
            }
        }
    }
}
