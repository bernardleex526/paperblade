#!/usr/bin/env bash
# 运行 tools/ 下 Python 工具的单元测试。
# 用法：bash tools/ci/run_python_tests.sh
set -uo pipefail
export PYTHONUTF8=1 PYTHONIOENCODING=utf-8
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT" || exit 2
"${PYTHON:-python}" -m unittest discover -s tools/tests -v
rc=$?
echo "== run_python_tests.sh 结束：exit=$rc"
exit $rc
