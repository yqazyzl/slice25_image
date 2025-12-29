using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Slice25Image.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/HalfSlice15Image", 11)]
    public class HalfSlice15Image : Image
    {
        [SerializeField] private int m_CenterWidth; //Top

        private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }
        [NonSerialized]
        private Sprite m_OverrideSprite;
        private Vector2 pivot;
        private RectTransform _target;
        private RectTransform target
        {
            get
            {
                if (_target == null)
                {
                    _target = transform as RectTransform;
                }
                return _target;
            }

        }

        /// <summary>
        /// Generate vertices for a 9-sliced Image.
        /// </summary>
        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }

            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = UnityEngine.Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();

            Vector4 adjustedBorders = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect);
            padding = padding / multipliedPixelsPerUnit;

            float w = ((rect.width - m_CenterWidth) * 0.5f) - activeSprite.rect.width;
            float[] vertXs = new float[6];
            vertXs[0] = padding.x;
            vertXs[1] = vertXs[0] + w;
            vertXs[2] = vertXs[1] + activeSprite.rect.width;
            vertXs[3] = vertXs[2] + m_CenterWidth;
            vertXs[4] = vertXs[3] + activeSprite.rect.width;
            vertXs[5] = rect.width - padding.z;

            float[] vertYs = {
                padding.y,
                adjustedBorders.y,
                rect.height - adjustedBorders.w,
                rect.height - padding.w,
            };

            for (int i = 0; i < vertXs.Length; i++)
            {
                vertXs[i] += rect.x;
            }

            for (int i = 0; i < vertYs.Length; i++)
            {
                vertYs[i] += rect.y;
            }

            float[] uvXs = {
                outer.x,
                outer.x,
                outer.z,
                outer.z,
                outer.x,
                outer.x,
            };

            float[] uvYs = {
                outer.y,
                inner.y,
                inner.w,
                outer.w,
            };

            toFill.Clear();

            for (int x = 0; x < 5; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {

                    int y2 = y + 1;

                    // Check for zero or negative dimensions to prevent invalid quads (UUM-71372)
                    //if ((vertXs[x2] - vertXs[x] <= 0) || (vertXs[y2] - vertXs[y] <= 0))
                    //    continue;

                    AddQuad(toFill,
                        new Vector2(vertXs[x], vertYs[y]),
                        new Vector2(vertXs[x2], vertYs[y2]),
                        color,
                        new Vector2(uvXs[x], uvYs[y]),
                        new Vector2(uvXs[x2], uvYs[y2]));
                }
            }
        }
        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

        /// <summary>
        /// Generate vertices for a simple Image.
        /// </summary>
        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            var uv = (activeSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;

            var color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x, v.y), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var padding = activeSprite == null ? Vector4.zero : UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
            var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref r, size);
            }

            v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }

        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
        {
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = rect.height;
                rect.height = rect.width * (1.0f / spriteRatio);
                rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = rect.width;
                rect.width = rect.height * spriteRatio;
                rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (activeSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            GenerateSlicedSprite(toFill);
        }

        //#if UNITY_EDITOR
        //    override protected void OnValidate()
        //    {
        //        base.OnValidate();
        //        if (target.pivot != pivot) {
        //            target.pivot = pivot;
        //        }
        //    }
        //#endif
    }
}


