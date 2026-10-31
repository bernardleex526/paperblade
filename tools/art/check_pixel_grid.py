"""检查截图里的像素画面是否严格按整数倍放大（没有模糊、没有混色）。

用法：
    python tools/art/check_pixel_grid.py <截图.png> --scale 4 [--background 141B33] [--palette 141B33,7A4A30,...]

规则：
  1. 找出所有非背景像素的包围盒（内容区域）。
  2. 包围盒的宽高必须是 scale 的整数倍，且内容按 scale×scale 的块对齐：每个块内所有像素颜色完全相同。
  3. 如果给了 --palette，内容区域里出现的每种颜色都必须在调色板里（出现调色板外的颜色说明有插值/混色）。
退出码：0 表示通过，1 表示发现问题，2 表示参数或文件错误。
"""
from __future__ import annotations

import argparse
import sys
from dataclasses import dataclass, field

import numpy as np
from PIL import Image


@dataclass
class GridReport:
    width: int
    height: int
    bbox: tuple[int, int, int, int] | None  # x0, y0, x1(不含), y1(不含)
    scale: int
    bad_blocks: int = 0
    total_blocks: int = 0
    off_palette_colors: list[str] = field(default_factory=list)
    errors: list[str] = field(default_factory=list)

    @property
    def ok(self) -> bool:
        return not self.errors


def parse_hex(c: str) -> tuple[int, int, int]:
    c = c.strip().lstrip("#")
    if len(c) != 6:
        raise ValueError(f"颜色必须是 6 位十六进制：{c}")
    return int(c[0:2], 16), int(c[2:4], 16), int(c[4:6], 16)


def check_grid(rgb: np.ndarray, scale: int, background: tuple[int, int, int],
               palette: list[tuple[int, int, int]] | None = None) -> GridReport:
    h, w, _ = rgb.shape
    rep = GridReport(width=w, height=h, bbox=None, scale=scale)
    if scale < 1:
        rep.errors.append("scale 必须 ≥ 1")
        return rep

    bg = np.array(background, dtype=rgb.dtype)
    content = np.any(rgb != bg, axis=2)
    ys, xs = np.nonzero(content)
    if len(xs) == 0:
        rep.errors.append("截图里没有非背景像素")
        return rep
    x0, x1 = int(xs.min()), int(xs.max()) + 1
    y0, y1 = int(ys.min()), int(ys.max()) + 1
    rep.bbox = (x0, y0, x1, y1)

    bw, bh = x1 - x0, y1 - y0
    if bw % scale or bh % scale:
        rep.errors.append(f"内容区域 {bw}x{bh} 不是 {scale} 的整数倍")
        return rep

    region = rgb[y0:y1, x0:x1].astype(np.int32)
    blocks = region.reshape(bh // scale, scale, bw // scale, scale, 3)
    first = blocks[:, :1, :, :1, :]
    uniform = np.all(blocks == first, axis=(1, 3, 4))
    rep.total_blocks = int(uniform.size)
    rep.bad_blocks = int((~uniform).sum())
    if rep.bad_blocks:
        rep.errors.append(f"{rep.bad_blocks}/{rep.total_blocks} 个 {scale}x{scale} 块内颜色不一致（模糊或未对齐）")

    if palette:
        allowed = {tuple(p) for p in palette} | {tuple(background)}
        colors = {tuple(int(v) for v in c) for c in np.unique(region.reshape(-1, 3), axis=0)}
        off = sorted(colors - allowed)
        rep.off_palette_colors = ["%02X%02X%02X" % c for c in off]
        if off:
            rep.errors.append(f"出现 {len(off)} 种调色板外的颜色（插值或混色）：{', '.join(rep.off_palette_colors[:8])}")
    return rep


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("image")
    ap.add_argument("--scale", type=int, required=True)
    ap.add_argument("--background", default="141B33")
    ap.add_argument("--palette", default="")
    args = ap.parse_args(argv)
    try:
        img = Image.open(args.image).convert("RGB")
        bg = parse_hex(args.background)
        pal = [parse_hex(c) for c in args.palette.split(",") if c.strip()] or None
    except (OSError, ValueError) as e:
        print(f"ERROR: {e}", file=sys.stderr)
        return 2
    rep = check_grid(np.asarray(img), args.scale, bg, pal)
    print(f"image={args.image} size={rep.width}x{rep.height} scale={rep.scale} bbox={rep.bbox} "
          f"blocks={rep.total_blocks} bad_blocks={rep.bad_blocks}")
    for e in rep.errors:
        print(f"ERROR: {e}")
    print("RESULT: PASS" if rep.ok else "RESULT: FAIL")
    return 0 if rep.ok else 1


if __name__ == "__main__":
    sys.exit(main())
