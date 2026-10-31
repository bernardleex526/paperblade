"""汇总 Unity 测试结果，并给出可信的退出码。

支持两种输入：
  * NUnit XML（`unity test` / Unity.exe -runTests 的输出）
  * Pipeline JSON（`unity command test run/status --result-only` 的输出）

判定规则（任一不满足即失败）：
  * 文件存在且能解析
  * Pipeline JSON 的 success 不为 false
  * 测试总数 > 0（防止"没发现测试"被当成通过）
  * 失败数 = 0；Inconclusive 数 = 0
  * --min-total N：总数至少为 N

用法：python tools/ci/test_results.py <结果文件> [--min-total N]
退出码：0 通过 / 1 测试失败或数量不足 / 2 文件缺失或无法解析
"""
from __future__ import annotations

import argparse
import json
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from pathlib import Path


@dataclass
class Summary:
    source: str
    total: int = 0
    passed: int = 0
    failed: int = 0
    skipped: int = 0
    inconclusive: int = 0
    failures: list[tuple[str, str]] = field(default_factory=list)
    error: str | None = None


def _int(v) -> int:
    try:
        return int(v)
    except (TypeError, ValueError):
        return 0


def parse_nunit_xml(text: str) -> Summary:
    root = ET.fromstring(text)
    run = root if root.tag == "test-run" else root.find(".//test-run")
    if run is None:
        raise ValueError("XML 中没有 test-run 节点")
    s = Summary(source="nunit-xml",
                total=_int(run.get("total")), passed=_int(run.get("passed")),
                failed=_int(run.get("failed")), skipped=_int(run.get("skipped")),
                inconclusive=_int(run.get("inconclusive")))
    for case in run.iter("test-case"):
        if case.get("result") == "Failed":
            msg_el = case.find("./failure/message")
            msg = (msg_el.text or "").strip() if msg_el is not None else ""
            s.failures.append((case.get("fullname", case.get("name", "?")), msg))
    return s


def _get(d: dict, key: str):
    """Pipeline 同步接口用 PascalCase（Summary/Total），异步 status 接口用小写（summary/total）。"""
    if key in d:
        return d[key]
    return d.get(key[:1].lower() + key[1:])


def parse_pipeline_json(text: str) -> Summary:
    d = json.loads(text)
    s = Summary(source="pipeline-json")
    if d.get("success") is False:
        s.error = d.get("error") or d.get("message") or "Pipeline 返回 success=false"
    status = d.get("status")
    if status is not None and status != "completed":
        s.error = f"异步测试状态为 {status}，不是 completed"
    summ = _get(d, "Summary") or {}
    s.total = _int(_get(summ, "Total"))
    s.passed = _int(_get(summ, "Passed"))
    s.failed = _int(_get(summ, "Failed"))
    s.skipped = _int(_get(summ, "Skipped"))
    s.inconclusive = _int(_get(summ, "Inconclusive"))
    for r in _get(d, "Results") or []:
        if r.get("Status") == "Failed":
            s.failures.append((r.get("FullName", "?"), (r.get("Message") or "").strip()))
    return s


def load(path: Path) -> Summary:
    text = path.read_text(encoding="utf-8-sig")
    stripped = text.lstrip()
    if stripped.startswith("<"):
        return parse_nunit_xml(text)
    if stripped.startswith("{"):
        return parse_pipeline_json(text)
    raise ValueError("无法识别的结果格式")


def evaluate(s: Summary, min_total: int = 1) -> list[str]:
    problems = []
    if s.error:
        problems.append(f"执行错误：{s.error}")
    if s.total < max(1, min_total):
        problems.append(f"测试总数 {s.total} 少于要求的 {max(1, min_total)}")
    if s.failed:
        problems.append(f"{s.failed} 个测试失败")
    if s.inconclusive:
        problems.append(f"{s.inconclusive} 个测试结果不确定")
    return problems


def main(argv: list[str] | None = None) -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("result")
    ap.add_argument("--min-total", type=int, default=1)
    args = ap.parse_args(argv)
    path = Path(args.result)
    if not path.is_file():
        print(f"ERROR: 结果文件不存在：{path}")
        return 2
    try:
        s = load(path)
    except (ValueError, ET.ParseError, json.JSONDecodeError) as e:
        print(f"ERROR: 无法解析 {path}：{e}")
        return 2
    print(f"[{s.source}] {path.name}: total={s.total} passed={s.passed} failed={s.failed} "
          f"skipped={s.skipped} inconclusive={s.inconclusive}")
    for name, msg in s.failures:
        print(f"  FAILED {name}\n         {msg[:500]}")
    problems = evaluate(s, args.min_total)
    for p in problems:
        print(f"ERROR: {p}")
    print("RESULT: PASS" if not problems else "RESULT: FAIL")
    return 0 if not problems else 1


if __name__ == "__main__":
    sys.exit(main())
