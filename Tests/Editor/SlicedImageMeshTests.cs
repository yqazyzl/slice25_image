using System.Reflection;
using NUnit.Framework;
using Slice25Image.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Slice25Image.Tests.Editor
{
    public class SlicedImageMeshTests
    {
        private const BindingFlags PopulateMeshFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [Test]
        public void Slice25Image_UsesOverrideSpriteUvs()
        {
            Texture2D texture = new Texture2D(64, 32);
            Sprite source = CreateSprite(texture, new Rect(0, 0, 32, 32));
            Sprite replacement = CreateSprite(texture, new Rect(32, 0, 32, 32));
            GameObject gameObject = CreateImageObject<Slice25Image>(new Vector2(100, 100));
            Slice25Image image = gameObject.GetComponent<Slice25Image>();
            image.sprite = source;
            image.overrideSprite = replacement;

            try
            {
                using (VertexHelper vertices = Populate(image))
                {
                    UIVertex vertex = GetVertex(vertices, 0);
                    Assert.That(vertex.uv0.x, Is.GreaterThanOrEqualTo(0.5f));
                }
            }
            finally
            {
                DestroyTestObjects(gameObject, source, replacement, texture);
            }
        }

        [Test]
        public void HalfImage_UsesAtlasOffsetForVerticalUvs()
        {
            Texture2D texture = new Texture2D(32, 64);
            Sprite source = CreateSprite(texture, new Rect(0, 0, 32, 32), new Vector4(2, 2, 2, 2));
            Sprite replacement = CreateSprite(texture, new Rect(0, 32, 32, 32), new Vector4(2, 2, 2, 2));
            GameObject gameObject = CreateImageObject<HalfImage>(new Vector2(96, 64));
            HalfImage image = gameObject.GetComponent<HalfImage>();
            image.sprite = source;
            image.overrideSprite = replacement;
            image.TopY = 16;

            try
            {
                using (VertexHelper vertices = Populate(image))
                {
                    Assert.That(vertices.currentVertCount, Is.GreaterThan(0));
                    for (int i = 0; i < vertices.currentVertCount; i++)
                    {
                        Assert.That(GetVertex(vertices, i).uv0.y, Is.GreaterThanOrEqualTo(0.5f));
                    }
                }
            }
            finally
            {
                DestroyTestObjects(gameObject, source, replacement, texture);
            }
        }

        [TestCase(typeof(Slice25Image))]
        [TestCase(typeof(HalfImage))]
        [TestCase(typeof(HalfSlice15Image))]
        public void SmallRect_DoesNotGenerateDegenerateOrReversedQuads(System.Type imageType)
        {
            Texture2D texture = new Texture2D(32, 32);
            Sprite sprite = CreateSprite(texture, new Rect(0, 0, 32, 32), new Vector4(8, 8, 8, 8));
            GameObject gameObject = new GameObject("Image Test", typeof(RectTransform), typeof(CanvasRenderer), imageType);
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(6, 6);
            Image image = gameObject.GetComponent<Image>();
            image.sprite = sprite;

            try
            {
                using (VertexHelper vertices = Populate(image))
                {
                    Assert.That(vertices.currentVertCount % 4, Is.EqualTo(0));
                    for (int i = 0; i < vertices.currentVertCount; i += 4)
                    {
                        UIVertex min = GetVertex(vertices, i);
                        UIVertex max = GetVertex(vertices, i + 2);
                        Assert.That(max.position.x, Is.GreaterThan(min.position.x));
                        Assert.That(max.position.y, Is.GreaterThan(min.position.y));
                    }
                }
            }
            finally
            {
                DestroyTestObjects(gameObject, sprite, texture);
            }
        }

        private static Sprite CreateSprite(Texture2D texture, Rect rect, Vector4 border = default)
        {
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static GameObject CreateImageObject<T>(Vector2 size) where T : Image
        {
            GameObject gameObject = new GameObject("Image Test", typeof(RectTransform), typeof(CanvasRenderer), typeof(T));
            gameObject.GetComponent<RectTransform>().sizeDelta = size;
            return gameObject;
        }

        private static VertexHelper Populate(Image image)
        {
            VertexHelper vertices = new VertexHelper();
            MethodInfo populateMesh = image.GetType().GetMethod("OnPopulateMesh", PopulateMeshFlags);
            Assert.That(populateMesh, Is.Not.Null);
            populateMesh.Invoke(image, new object[] { vertices });
            return vertices;
        }

        private static UIVertex GetVertex(VertexHelper vertices, int index)
        {
            UIVertex vertex = default;
            vertices.PopulateUIVertex(ref vertex, index);
            return vertex;
        }

        private static void DestroyTestObjects(params Object[] objects)
        {
            foreach (Object value in objects)
            {
                Object.DestroyImmediate(value);
            }
        }
    }
}
