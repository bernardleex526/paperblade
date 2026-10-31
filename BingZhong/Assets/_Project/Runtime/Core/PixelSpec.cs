namespace BingZhong.Core
{
    /// <summary>
    /// 像素画面规格（决策 #8）。这里是全项目唯一的定义，相机、导入管线和测试都从这里取值。
    /// </summary>
    public static class PixelSpec
    {
        /// <summary>内部参考分辨率宽度（像素）。</summary>
        public const int ReferenceWidth = 480;

        /// <summary>内部参考分辨率高度（像素）。</summary>
        public const int ReferenceHeight = 270;

        /// <summary>每个 Unity 单位对应的像素数（瓦片 16×16）。</summary>
        public const int PixelsPerUnit = 16;

        /// <summary>正交相机 size = 参考高度 / 2 / PPU。</summary>
        public const float OrthographicSize = ReferenceHeight / 2f / PixelsPerUnit;

        /// <summary>给定输出分辨率下的整数放大倍数（不足 1 时返回 1）。</summary>
        public static int IntegerScaleFor(int screenWidth, int screenHeight)
        {
            int sx = screenWidth / ReferenceWidth;
            int sy = screenHeight / ReferenceHeight;
            int s = sx < sy ? sx : sy;
            return s < 1 ? 1 : s;
        }
    }
}
