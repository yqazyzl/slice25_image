using UnityEngine;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Slice25Image.Runtime
{
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("UI/Slice25Image", 11)]
    public class Slice25Image : Image
    {

        static readonly Vector2[] s_VertScratch = new Vector2[6];
        static readonly Vector2[] s_UVScratch = new Vector2[6];
        private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }
        [NonSerialized]
        private Sprite m_OverrideSprite;

        [Range(0f, 1f)]
        public float horizontalRatio = 1f; // horizontal Ratio, default fill width;

        [Range(0f, 1f)]
        public float verticalRatio = 1f;   // vertical Ratio, default fill height;
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
        /// 
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

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[5] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[4].x = rect.width - adjustedBorders.z;
            s_VertScratch[4].y = rect.height - adjustedBorders.w;

            var w3 = sprite.rect.width - adjustedBorders.z - adjustedBorders.x;
            var h3 = sprite.rect.height - adjustedBorders.w - adjustedBorders.y;
            var w2 = (s_VertScratch[4].x - s_VertScratch[1].x - w3) * horizontalRatio;
            var h2 = (s_VertScratch[4].y - s_VertScratch[1].y - h3) * verticalRatio;
            s_VertScratch[2].x = s_VertScratch[1].x + w2;
            s_VertScratch[2].y = s_VertScratch[1].y + h2;
            s_VertScratch[3].x = s_VertScratch[2].x + w3;
            s_VertScratch[3].y = s_VertScratch[2].y + h3;

            pivot = new((s_VertScratch[2].x + w3 * 0.5f) / rect.width, (s_VertScratch[2].y + h3 * 0.5f) / rect.height);


            for (int i = 0; i < 6; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.x, inner.y);
            s_UVScratch[3] = new Vector2(inner.z, inner.w);
            s_UVScratch[4] = new Vector2(inner.z, inner.w);
            s_UVScratch[5] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 5; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 5; ++y)
                {
                    int y2 = y + 1;

                    // Check for zero or negative dimensions to prevent invalid quads (UUM-71372)
                    if ((s_VertScratch[x2].x - s_VertScratch[x].x <= 0) || (s_VertScratch[y2].y - s_VertScratch[y].y <= 0))
                        continue;

                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
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


