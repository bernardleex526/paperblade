#!/usr/bin/env bash
# 编译检查：编辑器错误数为 0 才算通过。
# 用法：bash tools/ci/compile_check.sh
#   - 编辑器已打开本项目：`unity recompile`（返回编译错误）
#   - 编辑器没打开：Unity.exe batchmode 打开项目后退出，统计日志中的 `error CS`
# 退出码：0 通过 / 1 有编译错误 / 2 环境错误
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT="$ROOT/BingZhong"
PROJECT_W="$(cygpath -w "$PROJECT")"
LOGS="$ROOT/build/logs"
UNITY_EXE="${UNITY_EXE:-E:/Unity/6000.6.6f1/Editor/Unity.exe}"
mkdir -p "$LOGS"

if unity --no-banner status 2>/dev/null | grep -Fqi "$PROJECT_W"; then
  echo "== compile_check: editor (unity recompile)"
  unity --no-banner recompile --project-path "$PROJECT_W" --timeout 300
  rc=$?
  echo "== compile_check 结束：exit=$rc"
  exit $rc
fi

if [ ! -x "$UNITY_EXE" ] && [ ! -f "$UNITY_EXE" ]; then
  echo "ERROR: 找不到 Unity：$UNITY_EXE（可用 UNITY_EXE 环境变量指定）"; exit 2
fi
LOG="$LOGS/compile_check.log"
echo "== compile_check: batchmode ($UNITY_EXE)"
"$UNITY_EXE" -batchmode -projectPath "$PROJECT_W" -quit -logFile "$(cygpath -w "$LOG")"
rc=$?
errors=$(grep -cE 'error CS[0-9]+' "$LOG" || true)
echo "unity_exit=$rc error_CS=$errors log=$LOG"
grep -E 'error CS[0-9]+' "$LOG" | sort -u | head -20
if [ "$errors" -gt 0 ]; then exit 1; fi
if [ $rc -ne 0 ]; then echo "ERROR: Unity 退出码 $rc"; exit 2; fi
echo "== compile_check 结束：exit=0"
exit 0
