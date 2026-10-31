using BingZhong.Core;
using BingZhong.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BingZhong.Tests.EditMode
{
    public class PixelSpecTests
    {
        [TestCase(1920, 1080, 4)]
        [TestCase(3840, 2160, 8)]
        [TestCase(2560, 1440, 5)]
        [TestCase(1366, 768, 2)]
        [TestCase(480, 270, 1)]
        [TestCase(320, 200, 1)]
        public void IntegerScaleFor_ReturnsLargestIntegerFit(int w, int h, int expected)
        {
            Assert.AreEqual(expected, PixelSpec.IntegerScaleFor(w, h));
        }

        [Test]
        public void OrthographicSize_MatchesReferenceHeightAndPpu()
        {
            Assert.AreEqual(8.4375f, PixelSpec.OrthographicSize, 1e-6f);
            Assert.AreEqual(PixelSpec.ReferenceHeight, PixelSpec.OrthographicSize * 2f * PixelSpec.PixelsPerUnit, 1e-4f);
        }
    }

    public class BootSceneSetupTests
    {
        [Test]
        public void BootScene_IsFirstEnabledBuildScene()
        {
            var scenes = EditorBuildSettings.scenes;
            Assert.IsNotEmpty(scenes);
            Assert.AreEqual(ProjectBootstrap.BootScenePath, scenes[0].path);
            Assert.IsTrue(scenes[0].enabled);
        }

        [Test]
        public void BootScene_PixelPerfectCamera_UsesPixelSpec()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EditorSceneManager.OpenScene(ProjectBootstrap.BootScenePath, OpenSceneMode.Single);
                var cam = Camera.main;
                Assert.IsNotNull(cam, "Boot 场景缺少 MainCamera");
                Assert.IsTrue(cam.orthographic);
                var ppc = cam.GetComponent<PixelPerfectCamera>();
                Assert.IsNotNull(ppc, "MainCamera 缺少 PixelPerfectCamera");
                Assert.AreEqual(PixelSpec.PixelsPerUnit, ppc.assetsPPU);
                Assert.AreEqual(PixelSpec.ReferenceWidth, ppc.refResolutionX);
                Assert.AreEqual(PixelSpec.ReferenceHeight, ppc.refResolutionY);
                Assert.AreEqual(PixelPerfectCamera.GridSnapping.UpscaleRenderTexture, ppc.gridSnapping);
                var filter = new SerializedObject(ppc).FindProperty("m_FilterMode").enumValueIndex;
                Assert.AreEqual((int)PixelPerfectCamera.PixelPerfectFilterMode.Point, filter, "放大滤波必须是 Point");
            }
            finally
            {
                if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        [Test]
        public void TestPattern_ImportedAsPointFilteredUncompressedSprite()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(ProjectBootstrap.TestPatternPath);
            Assert.IsNotNull(importer);
            Assert.AreEqual(TextureImporterType.Sprite, importer.textureType);
            Assert.AreEqual(FilterMode.Point, importer.filterMode);
            Assert.AreEqual(TextureImporterCompression.Uncompressed, importer.textureCompression);
            Assert.AreEqual(PixelSpec.PixelsPerUnit, (int)importer.spritePixelsPerUnit);
            Assert.IsFalse(importer.mipmapEnabled);
        }
    }
}
