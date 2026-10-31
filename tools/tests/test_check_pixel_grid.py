import sys
import unittest
from pathlib import Path

import numpy as np

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "art"))
from check_pixel_grid import check_grid  # noqa: E402

BG = (0x14, 0x1B, 0x33)
A = (0xFF, 0x6A, 0x1A)
B = (0xE8, 0xDC, 0xC4)


def make_canvas(w=64, h=48):
    img = np.zeros((h, w, 3), dtype=np.uint8)
    img[:, :] = BG
    return img


def upscale(small: np.ndarray, s: int) -> np.ndarray:
    return np.repeat(np.repeat(small, s, axis=0), s, axis=1)


class CheckPixelGridTests(unittest.TestCase):
    def setUp(self):
        small = np.zeros((4, 4, 3), dtype=np.uint8)
        for y in range(4):
            for x in range(4):
                small[y, x] = A if (x + y) % 2 == 0 else B
        self.sprite = upscale(small, 4)  # 16x16

    def test_clean_integer_upscale_passes(self):
        img = make_canvas()
        img[8:24, 12:28] = self.sprite
        rep = check_grid(img, 4, BG, [A, B])
        self.assertTrue(rep.ok, rep.errors)
        self.assertEqual(rep.total_blocks, 16)
        self.assertEqual(rep.bad_blocks, 0)
        self.assertEqual(rep.bbox, (12, 8, 28, 24))

    def test_blurred_pixel_detected(self):
        img = make_canvas()
        img[8:24, 12:28] = self.sprite
        img[10, 13] = ((np.array(A, dtype=np.int32) + np.array(B, dtype=np.int32)) // 2).astype(np.uint8)
        rep = check_grid(img, 4, BG, [A, B])
        self.assertFalse(rep.ok)
        self.assertEqual(rep.bad_blocks, 1)
        self.assertEqual(len(rep.off_palette_colors), 1)

    def test_misaligned_size_detected(self):
        img = make_canvas()
        img[8:24, 12:27] = self.sprite[:, :15]
        rep = check_grid(img, 4, BG)
        self.assertFalse(rep.ok)
        self.assertIn("整数倍", rep.errors[0])

    def test_empty_image_fails(self):
        rep = check_grid(make_canvas(), 4, BG)
        self.assertFalse(rep.ok)


if __name__ == "__main__":
    unittest.main()
