using UnityEngine;
using UnityEngine.UI;

namespace Slice25Image.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/HalfSlice15Image", 11)]
    public class HalfSlice15Image : Image
    {
        private static readonly float[] VertexXs = new float[8];
        private static readonly float[] VertexYs = new float[4];
        private static readonly float[] UvXs = new float[8];
        private static readonly float[] UvYs = new float[4];

        [SerializeField, Min(0)]
        private int m_CenterWidth;

        public int centerWidth
        {
            get => m_CenterWidth;
            set
            {
                int clampedValue = Mathf.Max(0, value);
                if (m_CenterWidth == clampedValue)
                {
                    return;
                }

                m_CenterWidth = clampedValue;
                SetVerticesDirty();
            }
        }

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

            GenerateMirroredSlicedSprite(activeSprite, toFill);
        }

        private void GenerateMirroredSlicedSprite(Sprite activeSprite, VertexHelper toFill)
        {
            Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
            Vector4 inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite) / multipliedPixelsPerUnit;
            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = SlicedImageUtility.GetAdjustedBorders(
                this,
                activeSprite.border / multipliedPixelsPerUnit,
                rect);

            float contentStart = rect.x + padding.x;
            float contentEnd = rect.xMax - padding.z;
            float contentWidth = Mathf.Max(0f, contentEnd - contentStart);
            float center = Mathf.Min(m_CenterWidth, contentWidth);
            float sideWidth = (contentWidth - center) * 0.5f;
            float naturalSideWidth = activeSprite.rect.width / multipliedPixelsPerUnit;
            float sampledSideWidth = Mathf.Min(naturalSideWidth, sideWidth);
            float borderWidth = Mathf.Min(adjustedBorders.x, sampledSideWidth);
            float stretchWidth = sideWidth - sampledSideWidth;

            VertexXs[0] = contentStart;
            VertexXs[1] = VertexXs[0] + borderWidth;
            VertexXs[2] = VertexXs[1] + stretchWidth;
            VertexXs[3] = VertexXs[0] + sideWidth;
            VertexXs[4] = VertexXs[3] + center;
            VertexXs[5] = VertexXs[4] + sampledSideWidth - borderWidth;
            VertexXs[6] = VertexXs[5] + stretchWidth;
            VertexXs[7] = contentEnd;

            VertexYs[0] = rect.y + padding.y;
            VertexYs[1] = rect.y + adjustedBorders.y;
            VertexYs[2] = rect.yMax - adjustedBorders.w;
            VertexYs[3] = rect.yMax - padding.w;

            UvXs[0] = outer.x;
            UvXs[1] = inner.x;
            UvXs[2] = inner.x;
            UvXs[3] = outer.z;
            UvXs[4] = outer.z;
            UvXs[5] = inner.x;
            UvXs[6] = inner.x;
            UvXs[7] = outer.x;

            UvYs[0] = outer.y;
            UvYs[1] = inner.y;
            UvYs[2] = inner.w;
            UvYs[3] = outer.w;

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
            m_CenterWidth = Mathf.Max(0, m_CenterWidth);
            base.OnValidate();
        }
#endif
    }
}
