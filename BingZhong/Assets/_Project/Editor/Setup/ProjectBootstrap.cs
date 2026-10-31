using System;
using System.IO;
using BingZhong.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BingZhong.EditorTools
{
    /// <summary>
    /// T02 项目初始化。可以重复执行（幂等）：
    /// Unity.exe -batchmode -projectPath BingZhong -executeMethod BingZhong.EditorTools.ProjectBootstrap.Apply -quit -logFile -
    /// </summary>
    public static class ProjectBootstrap
    {
        public const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
        public const string TestPatternPath = "Assets/_Project/Art/Debug/pixel_test_pattern.png";
        const string UnlitSpriteMaterialPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // 决策 #9 配色（Gemini 风格圣经建议值）
        static readonly Color32 Indigo = new Color32(0x14, 0x1B, 0x33, 0xFF);
        static readonly Color32 Rust = new Color32(0x7A, 0x4A, 0x30, 0xFF);
        static readonly Color32 Lava = new Color32(0xFF, 0x6A, 0x1A, 0xFF);
        static readonly Color32 Bronze = new Color32(0x2F, 0x8F, 0x83, 0xFF);
        static readonly Color32 SoulFire = new Color32(0x8F, 0xE3, 0xFF, 0xFF);
        static readonly Color32 Bone = new Color32(0xE8, 0xDC, 0xC4, 0xFF);

        [MenuItem("BingZhong/Setup/Apply Project Bootstrap")]
        public static void Apply()
        {
            ApplyPlayerSettings();
            ApplyEditorSettings();
            Sprite pattern = EnsureTestPattern();
            CreateBootScene(pattern);
            AssetDatabase.SaveAssets();
            Debug.Log("[Bootstrap] OK");
        }

        static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "兵冢";
            PlayerSettings.companyName = "bernard";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.runInBackground = false;
        }

        static void ApplyEditorSettings()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.lineEndingsForNewScripts = LineEndingsMode.Unix;
            // 保留域重载：避免静态状态在多次进入 Play 模式（包括 PlayMode 测试）之间残留
            EditorSettings.enterPlayModeOptionsEnabled = false;
        }

        static Sprite EnsureTestPattern()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(TestPatternPath));
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color32 c;
                    if (x < 16 && y < 16) c = ((x + y) & 1) == 0 ? Bone : Indigo;      // 1 像素棋盘
                    else if (x >= 16 && y < 16) c = (x & 1) == 0 ? Lava : Rust;          // 1 像素竖条
                    else if (x < 16) c = (y & 1) == 0 ? Bronze : Indigo;                 // 1 像素横条
                    else c = (x == 16 || x == 31 || y == 16 || y == 31) ? SoulFire : Rust; // 1 像素边框
                    px[y * size + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            File.WriteAllBytes(TestPatternPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(TestPatternPath, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(TestPatternPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelSpec.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(TestPatternPath);
        }

        static void CreateBootScene(Sprite pattern)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BootScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = PixelSpec.OrthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Indigo;
            cam.allowMSAA = false;
            cam.GetUniversalAdditionalCameraData();

            var ppc = camGo.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = PixelSpec.PixelsPerUnit;
            ppc.refResolutionX = PixelSpec.ReferenceWidth;
            ppc.refResolutionY = PixelSpec.ReferenceHeight;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;
            var so = new SerializedObject(ppc);
            so.FindProperty("m_FilterMode").enumValueIndex = (int)PixelPerfectCamera.PixelPerfectFilterMode.Point;
            so.ApplyModifiedPropertiesWithoutUndo();

            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            var patternGo = new GameObject("PixelTestPattern");
            var sr = patternGo.AddComponent<SpriteRenderer>();
            sr.sprite = pattern;
            var unlit = AssetDatabase.LoadAssetAtPath<Material>(UnlitSpriteMaterialPath);
            if (unlit != null) sr.sharedMaterial = unlit;

            EditorSceneManager.SaveScene(scene, BootScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BootScenePath, true) };
        }

        /// <summary>
        /// 在 batchmode 下把 Boot 场景渲染成 PNG：
        /// -executeMethod BingZhong.EditorTools.ProjectBootstrap.CaptureBoot -captureOut &lt;png&gt; [-captureWidth 1920 -captureHeight 1080]
        /// </summary>
        public static void CaptureBoot()
        {
            string outPath = GetArg("-captureOut") ?? "../build/screenshots/boot.png";
            int w = int.Parse(GetArg("-captureWidth") ?? "1920");
            int h = int.Parse(GetArg("-captureHeight") ?? "1080");

            EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null) throw new InvalidOperationException("Boot 场景里没有 MainCamera");

            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point };
            rt.Create();
            cam.targetTexture = rt;
            cam.Render();
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            cam.targetTexture = null;

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath)));
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            Debug.Log($"[Bootstrap] Captured {w}x{h} -> {Path.GetFullPath(outPath)}");
        }

        static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }
    }
}
