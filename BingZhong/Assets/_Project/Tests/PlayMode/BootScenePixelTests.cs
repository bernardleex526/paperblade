using System.Collections;
using BingZhong.Core;
using BingZhong.Tests.PlayMode.Support;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BingZhong.Tests.PlayMode
{
    public class BootScenePixelTests
    {
        static readonly Color32 Background = new Color32(0x14, 0x1B, 0x33, 0xFF);

        [UnitySetUp]
        public IEnumerator LoadBoot()
        {
            yield return SceneManager.LoadSceneAsync("Boot", LoadSceneMode.Single);
            yield return null;
        }

        [Test]
        public void BootScene_HasPixelPerfectCameraFromPixelSpec()
        {
            var cam = Camera.main;
            Assert.IsNotNull(cam);
            var ppc = cam.GetComponent<PixelPerfectCamera>();
            Assert.IsNotNull(ppc);
            Assert.AreEqual(PixelSpec.PixelsPerUnit, ppc.assetsPPU);
            Assert.AreEqual(PixelSpec.ReferenceWidth, ppc.refResolutionX);
            Assert.AreEqual(PixelSpec.ReferenceHeight, ppc.refResolutionY);
        }

        [TestCase(1920, 1080, 4)]
        [TestCase(3840, 2160, 8)]
        public void BootScene_RendersExactIntegerUpscale(int width, int height, int scale)
        {
            var cam = Camera.main;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var prev = RenderTexture.active;
            try
            {
                rt.Create();
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
            }
            finally
            {
                RenderTexture.active = prev;
                cam.targetTexture = null;
                rt.Release();
                Object.Destroy(rt);
            }

            var result = PixelGrid.Analyze(tex.GetPixels32(), width, height, scale, Background);
            Object.Destroy(tex);

            Assert.IsTrue(result.HasContent, "渲染结果里没有测试图案");
            Assert.IsTrue(result.Aligned, $"内容区域 {result.Bounds} 不是 {scale} 的整数倍");
            // 测试图案是 32x32 像素，放大后应为 (32*scale)^2
            Assert.AreEqual(32 * scale, result.Bounds.width);
            Assert.AreEqual(32 * scale, result.Bounds.height);
            Assert.AreEqual(32 * 32, result.TotalBlocks);
            Assert.AreEqual(0, result.BadBlocks, $"{result.BadBlocks} 个 {scale}x{scale} 块颜色不一致（模糊）");
        }
    }

    /// <summary>
    /// 帧率矩阵样板：同一段模拟在 30、60、144 帧下得到相同的时间推进。T12 起的手感测试照此写法。
    /// </summary>
    public class FrameRateMatrixSampleTests
    {
        [TearDown]
        public void TearDown() => SimulatedFrameRate.Reset();

        [UnityTest]
        public IEnumerator SimulatedOneSecond_AdvancesSameGameTime([ValueSource(typeof(SimulatedFrameRate), nameof(SimulatedFrameRate.Matrix))] int fps)
        {
            SimulatedFrameRate.Apply(fps);
            yield return null; // 让新的步长生效
            float start = Time.time;
            int frames = 0;
            while (Time.time - start < 1f - 0.5f / fps)
            {
                yield return null;
                frames++;
            }
            Assert.AreEqual(fps, frames, "模拟 1 秒应当恰好推进 fps 帧");
            Assert.AreEqual(1f, Time.time - start, 1f / fps, "游戏时间推进应当约为 1 秒");
            Assert.AreEqual(1f / fps, Time.deltaTime, 1e-5f);
        }

        [UnityTest]
        public IEnumerator UnscaledClock_KeepsRunningWhenTimeScaleIsZero([ValueSource(typeof(SimulatedFrameRate), nameof(SimulatedFrameRate.Matrix))] int fps)
        {
            // 打击停顿与慢动作期间，输入缓冲和无敌帧必须用不受 timeScale 影响的时钟（Pre-Plan 6.4）
            SimulatedFrameRate.Apply(fps);
            yield return null;
            Time.timeScale = 0f;
            float scaledStart = Time.time;
            float unscaledStart = Time.unscaledTime;
            for (int i = 0; i < fps / 2; i++) yield return null;
            Assert.AreEqual(scaledStart, Time.time, 1e-6f, "timeScale=0 时游戏时间不应推进");
            Assert.Greater(Time.unscaledTime - unscaledStart, 0f, "unscaledTime 必须继续推进");
        }
    }
}
