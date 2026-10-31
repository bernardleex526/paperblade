#!/usr/bin/env bash
# 运行 Unity 测试，并给出可信的退出码。
#
# 用法：bash tools/ci/run_tests.sh [EditMode|PlayMode|all] [--filter <名称模式>] [--min-total N]
#
# 后端自动选择：
#   - 编辑器没有打开本项目：`unity test`（batchmode），NUnit XML 写到 build/test-results/<Mode>.xml
#   - 编辑器已经打开本项目：通过 Pipeline 在编辑器里运行，结果 JSON 写到 build/test-results/<Mode>.json
#     （PlayMode 会触发域重载，只能异步启动后轮询状态）
#   - 设置 BZ_TEST_BACKEND=batch 或 editor 可以强制指定（batch 需要先关闭编辑器）
#
# 判定交给 tools/ci/test_results.py：测试总数为 0、有失败、Pipeline 返回 success=false 都算失败。
# 退出码：0 全部通过；1 测试失败或没有测试；2 环境或参数错误
set -uo pipefail
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT="$ROOT/BingZhong"
PROJECT_W="$(cygpath -w "$PROJECT")"
OUT="$ROOT/build/test-results"
LOGS="$ROOT/build/logs"
PY="${PYTHON:-python}"
POLL_TIMEOUT="${BZ_TEST_TIMEOUT:-1800}"
mkdir -p "$OUT" "$LOGS"

MODE="${1:-all}"
[ $# -gt 0 ] && shift
FILTER=""
MIN_TOTAL=1
while [ $# -gt 0 ]; do
  case "$1" in
    --filter) FILTER="${2:-}"; shift 2 ;;
    --min-total) MIN_TOTAL="${2:-1}"; shift 2 ;;
    *) echo "ERROR: 未知参数 $1" >&2; exit 2 ;;
  esac
done

case "$MODE" in
  EditMode|PlayMode) MODES=("$MODE") ;;
  all) MODES=(EditMode PlayMode) ;;
  *) echo "ERROR: 模式只能是 EditMode、PlayMode 或 all" >&2; exit 2 ;;
esac

editor_open() {
  unity --no-banner status 2>/dev/null | grep -Fqi "$PROJECT_W"
}

json_field() { # json_field <file> <python 表达式，d 为解析后的对象>
  "$PY" -c "import json,sys
try:
    d=json.load(open(sys.argv[1],encoding='utf-8-sig'))
    print($2)
except Exception:
    print('')" "$1"
}

run_batch() {
  local m="$1" xml="$OUT/$1.xml"
  rm -f "$xml"
  local args=(--no-banner --non-interactive test "$PROJECT_W" --mode "$m" --output "$(cygpath -w "$xml")")
  [ -n "$FILTER" ] && args+=(--filter "$FILTER")
  echo "== [$m] backend=batch: unity ${args[*]}"
  unity "${args[@]}" > "$LOGS/tests-$m.log" 2>&1
  local rc=$?
  tail -n 5 "$LOGS/tests-$m.log"
  "$PY" "$ROOT/tools/ci/test_results.py" "$xml" --min-total "$MIN_TOTAL"
  local prc=$?
  if [ $prc -eq 0 ] && [ $rc -ne 0 ]; then
    echo "ERROR: 结果文件显示通过，但 unity test 退出码为 $rc（见 $LOGS/tests-$m.log）"
    return 1
  fi
  return $prc
}

run_editor() {
  local m="$1" json="$OUT/$1.json"
  rm -f "$json"
  local fargs=(--mode "$m")
  [ -n "$FILTER" ] && fargs+=(--filter "$FILTER")
  echo "== [$m] backend=editor (Pipeline)"
  if [ "$m" = "EditMode" ]; then
    unity --no-banner command test run --timeout "$POLL_TIMEOUT" --result-only -- "${fargs[@]}" > "$json" 2> "$LOGS/tests-$m.err"
  else
    # 删除上一次的状态文件，避免读到旧的 completed 结果
    rm -f "$PROJECT/Temp/pipeline_test_status.json"
    local start="$OUT/$m.start.json"
    unity --no-banner command test run --timeout 60 --result-only -- "${fargs[@]}" --async_tests true > "$start" 2> "$LOGS/tests-$m.err"
    if [ "$(json_field "$start" "d.get('success')")" != "True" ]; then
      echo "ERROR: PlayMode 测试没有启动："; cat "$start"; return 2
    fi
    local waited=0 tmp="$OUT/$m.status.json" st=""
    while [ $waited -lt "$POLL_TIMEOUT" ]; do
      sleep 3; waited=$((waited + 3))
      unity --no-banner command test status --timeout 30 --result-only > "$tmp" 2>/dev/null || continue
      st="$(json_field "$tmp" "d.get('status','')")"
      if [ "$st" = "completed" ]; then mv "$tmp" "$json"; break; fi
    done
    if [ "$st" != "completed" ]; then
      echo "ERROR: 等待 ${POLL_TIMEOUT} 秒后 PlayMode 测试仍未完成（最后状态：${st:-无}）"; return 1
    fi
  fi
  "$PY" "$ROOT/tools/ci/test_results.py" "$json" --min-total "$MIN_TOTAL"
}

BACKEND="${BZ_TEST_BACKEND:-auto}"
if [ "$BACKEND" = "auto" ]; then
  if editor_open; then BACKEND=editor; else BACKEND=batch; fi
fi

overall=0
for m in "${MODES[@]}"; do
  if [ "$BACKEND" = "editor" ]; then run_editor "$m"; else run_batch "$m"; fi
  rc=$?
  [ $rc -gt $overall ] && overall=$rc
done
echo "== run_tests.sh 结束：backend=$BACKEND modes=${MODES[*]} exit=$overall"
exit $overall
