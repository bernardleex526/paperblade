using UnityEngine;

namespace BingZhong.Tests.PlayMode.Support
{
    /// <summary>
    /// 帧率矩阵测试用：把每帧的 deltaTime 固定为 1/fps，让 30、60、144 帧下的模拟结果可以直接比较。
    /// 用法：SetUp 里 Apply(fps)，TearDown 里 Reset()。
    /// </summary>
    public static class SimulatedFrameRate
    {
        public static readonly int[] Matrix = { 30, 60, 144 };

        public static void Apply(int fps)
        {
            Time.captureDeltaTime = 1f / fps;
        }

        public static void Reset()
        {
            Time.captureDeltaTime = 0f;
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// 检查一张渲染结果是否严格按整数倍放大（与 tools/art/check_pixel_grid.py 的规则一致）。
    /// </summary>
    public static class PixelGrid
    {
        public struct Result
        {
            public RectInt Bounds;
            public int TotalBlocks;
            public int BadBlocks;
            public bool HasContent;
            public bool Aligned;
        }

        public static Result Analyze(Color32[] px, int width, int height, int scale, Color32 background)
        {
            var r = new Result();
            int x0 = width, y0 = height, x1 = -1, y1 = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var c = px[y * width + x];
                    if (c.r == background.r && c.g == background.g && c.b == background.b) continue;
                    if (x < x0) x0 = x;
                    if (y < y0) y0 = y;
                    if (x > x1) x1 = x;
                    if (y > y1) y1 = y;
                }
            }
            if (x1 < 0) return r;
            r.HasContent = true;
            r.Bounds = new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
            r.Aligned = r.Bounds.width % scale == 0 && r.Bounds.height % scale == 0;
            if (!r.Aligned) return r;

            for (int by = 0; by < r.Bounds.height / scale; by++)
            {
                for (int bx = 0; bx < r.Bounds.width / scale; bx++)
                {
                    r.TotalBlocks++;
                    int ox = x0 + bx * scale, oy = y0 + by * scale;
                    var first = px[oy * width + ox];
                    bool uniform = true;
                    for (int dy = 0; dy < scale && uniform; dy++)
                    {
                        for (int dx = 0; dx < scale; dx++)
                        {
                            var c = px[(oy + dy) * width + ox + dx];
                            if (c.r != first.r || c.g != first.g || c.b != first.b) { uniform = false; break; }
                        }
                    }
                    if (!uniform) r.BadBlocks++;
                }
            }
            return r;
        }
    }
}
