using UnityEngine;
using UnityEngine.UI;

namespace Slice25Image.Runtime
{
    internal static class SlicedImageUtility
    {
        internal static Vector4 GetAdjustedBorders(Image image, Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = image.rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                if (!Mathf.Approximately(originalRect.size[axis], 0f))
                {
                    float borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && !Mathf.Approximately(combinedBorders, 0f))
                {
                    float borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }

            return border;
        }

        internal static void GenerateSimpleSprite(Image image, Sprite activeSprite, VertexHelper vertexHelper)
        {
            Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(activeSprite);
            Vector2 size = activeSprite.rect.size;
            Rect rect = image.GetPixelAdjustedRect();

            Vector4 dimensions = new Vector4(
                rect.x + rect.width * padding.x / size.x,
                rect.y + rect.height * padding.y / size.y,
                rect.x + rect.width * (size.x - padding.z) / size.x,
                rect.y + rect.height * (size.y - padding.w) / size.y);
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(activeSprite);
            Color32 color = image.color;

            vertexHelper.Clear();
            vertexHelper.AddVert(new Vector3(dimensions.x, dimensions.y), color, new Vector2(uv.x, uv.y));
            vertexHelper.AddVert(new Vector3(dimensions.x, dimensions.w), color, new Vector2(uv.x, uv.w));
            vertexHelper.AddVert(new Vector3(dimensions.z, dimensions.w), color, new Vector2(uv.z, uv.w));
            vertexHelper.AddVert(new Vector3(dimensions.z, dimensions.y), color, new Vector2(uv.z, uv.y));
            vertexHelper.AddTriangle(0, 1, 2);
            vertexHelper.AddTriangle(2, 3, 0);
        }

        internal static void AddQuad(
            VertexHelper vertexHelper,
            Vector2 positionMin,
            Vector2 positionMax,
            Color32 color,
            Vector2 uvMin,
            Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(positionMin.x, positionMin.y), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(positionMin.x, positionMax.y), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(positionMax.x, positionMax.y), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(positionMax.x, positionMin.y), color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        internal static bool HasPositiveArea(float[] xPositions, int x, float[] yPositions, int y)
        {
            return xPositions[x + 1] > xPositions[x] && yPositions[y + 1] > yPositions[y];
        }
    }
}
