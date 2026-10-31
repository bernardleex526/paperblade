# 兵冢（BingZhong）执行计划 · plan.md

> 依据：`兵冢 Pre-Plan — Grill+Plan 生成 f36ff7c2eb74434f93416505978645ad.md`（下称 Pre-Plan，T01 时复制为 `PRE_PLAN.md`），以及配套文档 `../gemini_work/兵冢 Pre-Plan · Gemini 美术参考图 …md`、`../gpt_work/兵冢 Pre-Plan · GPT 审查与验收 …md`。三者冲突时以 Pre-Plan 为准。
>
> 状态：阶段 A、B 于 2026-10-31 完成；**C1 已于 2026-10-31 批准**，进入阶段 C。执行进度见 `PLAN_PROGRESS.md`。
>
> 2026-10-31 你已确认：绑骨用 **Mixamo 自动绑骨 + Blender 原生骨骼和 IK**（替代 Auto-Rig Pro）；仓库根为 **`E:\MYGAME\opus_work`**；旧仓库备份到 **`E:\Backups\paperblade`**；Tripo 用**网页手动操作**。下文已按这些决定更新。
>
> 本文件是 Pre-Plan §14 阶段 B 的产出。Pre-Plan 第 2 节的决策、第 12 节的验收标准、第 11 节的检查点全部保留；与 Pre-Plan 的差异集中写在第 6 节。

---

## 1. 阶段 A：环境探测结果（2026-10-31，本机实测）

图例：✅ 符合 / ❌ 不符 / ❓ 无法在本机验证 / ⚠️ 部分符合或有附加风险

| # | 假设 | 结果 | 实际情况与证据 |
| --- | --- | --- | --- |
| E1 | Unity 6 LTS 或更新版本，支持 URP 2D | ✅ | `unity editors -i` → **6000.6.6f1**，路径 `E:\Unity\6000.6.6f1\Editor\Unity.exe`；本地模板 `com.unity.template.universal-2d 7.0.0` 状态 ready；Windows 构建支持只有 **Mono**（无 IL2CPP 模块，Demo 够用）；`unity license` → Unity Personal 已激活 |
| E2 | Blender 能在命令行运行 | ✅ | `blender --version` → **Blender 5.2.2 LTS**（`E:\Tools\blender-5.2.2-windows-x64\blender.exe`，已在 PATH）；`blender -b --python` 正常，内置 Python 3.13.13、numpy 2.3.4，可用引擎含 `BLENDER_EEVEE_NEXT`、`CYCLES`、`BLENDER_WORKBENCH` |
| E3 | 官方 Unity MCP 和 Pipeline 包能安装并连接 | ⚠️❓ | Unity CLI `unity` 1.0.0-beta.13 可用，自带 `unity mcp`（stdio MCP 服务器）、`unity pipeline install`、`unity command`/`unity list`（驱动已打开的编辑器）、`unity test`、`unity build`。**但 Claude Code 里目前没有注册任何 MCP 服务器**；Unity 官方插件 `unity@unity-agent-plugin 0.1.8-beta` 已安装但在全局设置里是禁用状态。项目还不存在，无法做只读查询 → 在 T02 验证 |
| E4 | Auto-Rig Pro 已购买并启用，可脚本调用 | ❌ | Blender 共 14 个插件，没有 ARP；已启用的只有内置插件和 `blender_mcp`；`'arp' in dir(bpy.ops)` → False。**影响决策 #16，见第 3 节；你已选方案 B（Mixamo 自动绑骨 + 原生 IK）** |
| E5 | ARP 能为非人形 Boss 建自定义骨骼 | ❓ | 依赖 E4，T26 小样验证 |
| E6 | 有可供脚本调用的 Tripo API Key | ❓→已定 | 环境变量里没有 `TRIPO*`；你选择网页手动操作 |
| E7 | Tripo credits 够 80–120 次生成 | ❓ | 需要你查看账户页 |
| E8 | Mixamo 账号可用，需手动下载 | ❓ | 需要你确认 |
| E9 | 电脑配置 | ✅⚠️ | Ryzen 5 5600（6C/12T）、32 GB 内存、RTX 4060 8 GB（驱动 610.88）、Windows 11 Pro 26200。**磁盘：C: 剩 62 GB，E: 剩 37 GB（已用 96%）**。开发机明显强于目标配置 GTX 1060，本机性能数据不能直接代表验收配置（见 T38） |
| E10 | URP 2D、Pixel Perfect、2D Animation、Tilemap Extras、Input System、Cinemachine、Test Framework、Timeline 能安装 | ❓ | 需要创建项目并连包注册表，属于修改操作 → T02 验证 |
| E11 | 人物/武器分层序列帧可行 | ❓ | T09/T10 小样 |
| E12 | Blender 能完成卡通着色、1px 描边、正交批量渲染、法线导出、调色板量化 | ❓ | T09 小样（Blender 本身具备条件：EEVEE Next、numpy 可用） |
| E13 | Git、Git LFS、LFS 额度 | ✅⚠️ | `git 2.54.0.windows.1`、`git-lfs 3.7.1`；**全局没有 `user.name`/`user.email`**（旧仓库提交用的是 `bernard <bernard@localhost>`）；LFS 额度需要登录 GitHub 查看 → ❓ |
| E14 | paperblade 没有阻止强推的保护，本机有推送权限 | ✅⚠️ | GitHub API：仓库**公开**，默认分支 `main`，`protected=false`，无规则集，无标签；远程只有 `refs/heads/main`（2299c8b8，2026-10-30「试玩版打包：Windows 第一章 Demo」）。推送权限要等 T04 的 `git push --dry-run` 才能确认。**全局 git 代理 `http(s).proxy=http://127.0.0.1:12000` 当前没有在监听**，走代理会失败，直连 GitHub 可以连通 |
| E15 | 项目目录外有备份目录且空间够 | ✅ | 你指定 `E:\Backups\paperblade`；旧仓库很小（API 显示 523 KB，另有 LFS 对象），E: 剩 37 GB 足够 |
| E16 | Gemini 能生成一致的四视图 | ⚠️ | `gemini_work/art/reference/` 已有 G00（正/侧）、G01 v3、G02 四视图、G03 细节图，manifest 全部是 `candidate`。G02 四视图头顶和脚底对齐，初步看来一致；问题见第 4 节，留到 C2 由你判断 |
| E17 | GPT 侧工具能读仓库、跑测试 | ⚠️ | 本机已装 `codex-cli 0.166.0`；能否在 Codex 里跑 Unity 测试需要你在 GPT 侧验证 |
| E18 | CC0 音效和 AI 音乐的商用授权 | ❓ | T37 前确认来源 |
| E19 | 有没装 Unity 的 Windows 电脑做打包测试 | ❓ | 需要你确认；没有就新建 Windows 本地用户测试 |
| E20 | itch.io 账号；butler | ❌❓ | `butler` 未安装；账号需要你确认。安装 butler 属于安装系统级软件，要先问你；默认 T41 由你手动上传 |
| E21 | 可商用中文像素字体 | ❓ | 建议用 Fusion Pixel Font 或 Ark Pixel Font（都是 SIL OFL 1.1），T36 下载时核对授权文件 |

**其他发现**：

1. 工作区布局：`E:\MYGAME\` 下有 `opus_work/`（本目录，只有 Pre-Plan）、`gemini_work/`（Gemini 文档和参考图）、`gpt_work/`（GPT 文档）三个目录，都不是 git 仓库。
2. Unity Hub 里登记了 `E:\MYGAME\PaperBlade`（旧项目），但磁盘上**已经没有这个目录**。旧项目现在只存在于 GitHub 上，所以 T01 的备份是旧项目唯一的副本。
3. 旧仓库的 `.gitattributes` 把 png/jpg/psd/fbx 等交给 **Git LFS**。`git bundle` 只包含 git 对象，不包含 LFS 文件内容，T01 必须另外执行 `git lfs fetch --all` 备份 LFS 对象。另外，强推覆盖后 GitHub 上旧的 LFS 对象**不会被删除**，仍然占用 LFS 存储额度（只有删除整个仓库才能释放）。
4. 系统 Python 3.13.14 已有 Pillow 12.3.0、numpy 2.5.1；没有 `py` 启动器；`uv 0.11.26`、Node 24.16 可用。`gh` CLI 未安装（用 GitHub REST API 和 git 命令代替）。

## 2. 基线

- 本目录不是 git 仓库，没有 Unity 项目、构建脚本或测试 → **基线 = 不存在**。
- 按 Pre-Plan §9，基线定义为"T03 完成后示例测试通过"。T01–T03 就是"搭建最小验证脚手架"，不再单独设 Task 0。
- 阶段 A 没有执行任何修改操作，也没有产生任何付费消耗（Tripo、生成图、AI 音乐均为 0）。

## 3. 与已确认决策冲突的事实及建议（需要你在 C1 决定）

### 3.1 Auto-Rig Pro 未安装（E4 ❌ → 决策 #16）

T07（主角绑骨）、T08/T19（动作接入和 IK 手调）、T24（敌人和 NPC）、T26（Boss）都依赖它。可选方案：

| 方案 | 做法 | 优点 | 缺点 |
| --- | --- | --- | --- |
| A. 购买 ARP（与教程一致） | 你购买并安装，我用 `bpy.ops.arp.*` 做绑骨、Mixamo 重定向和 IK | 和 Pre-Plan 完全一致；重定向和 IK 控制器现成 | 付费；ARP Smart 需要放置关节标记，无界面脚本化程度要在 T07 才能验证 |
| **B. Mixamo 自动绑骨 + Blender 原生骨骼和 IK（建议）** | 你把 Tripo 模型上传到 Mixamo 自动绑骨（放 5 个标记点），下载带骨骼的 FBX；之后所有 Mixamo 动作都用同一套骨骼，**不需要重定向**；武器插槽骨骼和手臂 IK 用 Blender 原生约束，由脚本添加 | 免费；纯 `bpy` 可以完全无界面执行；Mixamo 动作和骨骼一对一匹配 | 你需要在 Mixamo 网页多做一次上传（反正下载动作也需要你操作）；裙摆、背鳍类部件的蒙皮可能要手修权重 |
| C. Rigify | Blender 自带 | 免费 | 决策 #16 已否决（Agent 操作稳定性未经验证） |

非人形角色（熔浆炉灵、Boss）无论哪个方案都用 Blender 原生骨骼（Pre-Plan T26 的回退方案本来就是这样）。

**已决定（2026-10-31）：方案 B。** 决策 #16 中的"Blender + Auto-Rig Pro"改为"Blender + Mixamo 自动绑骨 + Blender 原生骨骼和 IK 约束"；Pre-Plan 里所有"用 ARP 的 IK 控制器调整"的步骤都改用 Blender 原生 IK 约束和动作层完成。如果 T07 发现 Mixamo 自动绑骨对战袍下摆、背部装饰的蒙皮无法修到合格，我会停下来，带着对比图再请你决定。

### 3.2 其余事项不冲突，但需要你确认或提供信息

见第 8 节"C1 待确认事项"。

## 4. 参考图预检（只读，正式校验在 T05）

这些问题会在 T05 的问题清单里列出，由你在 C2 决定，现在只是提前告知：

1. **画风方向变了**：`generation_queue.json` 的 `base_style` 是"国风水墨重彩 / 东方新水墨"，G02 主角是哥窑开片面具、饕餮宝镜甲、战袍。Gemini 风格圣经要求"风格化卡通、造型大块简洁"，决策 #9 要求深靛蓝/铁锈褐打底。不论参考图是什么画风，决策 #8 的像素管线（2–3 级卡通着色 + 调色板量化）都会把它变成卡通像素风，水墨质感不会保留。C2 时需要你确认能接受这一点。
2. **G01 v3 里的主角和 G02 不是同一个角色**（G01 是白色裂纹面具、紫色四肢的木偶；G02 是带发冠的面具、靛蓝战袍）。Gemini 的队列里已经排了 `G01_redraw`。
3. **G02 头身比约 1:6.5**，风格圣经要求 1:4.5–1:5；渲染到 48–56 像素高时，头部只有约 8 像素，面具细节会消失。
4. **G02 有战袍下摆和背部鳍状装饰**，绑骨和翻滚动作里容易穿模，也是 3.1 方案 B 需要手修权重的部位。
5. **G03 细节图只有 240–264 像素宽**，低于"长边 ≥ 1024 像素"的要求（细节图主要用于贴图参考，是否放宽由你决定）。
6. G02 正面、左侧和 G00 v2 的正面、侧面是同一文件（大小完全相同）；G04–G09 尚未生成。

## 5. 校准后的环境、目录和命令

### 5.1 工作区与仓库（建议，C1 确认）

- **仓库根 = `E:\MYGAME\opus_work`**，远程 `origin = https://github.com/bernardleex526/paperblade.git`，工作分支 `dev/demo`。
- Pre-Plan 原文件保留在原处，并写入 `.git/info/exclude`（只在本地忽略）；入库的是复制出来的 `PRE_PLAN.md`。
- Gemini 继续在 `gemini_work/art/reference/` 出图；T05 用 `tools/art/sync_reference.py` 单向复制到仓库的 `art/reference/`，并校验 SHA-256。
- GPT（Codex）直接在 `E:\MYGAME\opus_work` 里只读审查，只写 `docs/review/`。GPT 写入的报告由我在下一次会话开始时以 `[REVIEW-Rn]` 提交登记，我不修改报告内容。
- 本地 git 身份（只写入仓库本地配置）：`user.name=bernard`、`user.email=bernard@localhost`，和旧仓库一致。
- 远程操作统一加 `-c http.proxy= -c https.proxy=`，绕开当前失效的全局代理（不修改你的全局 git 配置）。如果你之后启动了代理，也可以去掉这两个参数。

### 5.2 目录结构（校准后）

```
E:\MYGAME\opus_work\              ← 仓库根
├─ PRE_PLAN.md  plan.md  PLAN_PROGRESS.md
├─ .gitattributes  .gitignore  .mcp.json（项目级 Unity MCP 注册，T02）
├─ BingZhong\                     ← Unity 6000.6.6f1 项目
│  ├─ Packages\manifest.json
│  └─ Assets\_Project\{Runtime,Editor,Data,Art,Scenes,Tests\{EditMode,PlayMode},Input,Level,Audio}
├─ art\{reference,source,render,palette}\
├─ tools\
│  ├─ requirements.txt            ← Pillow、numpy（工具脚本用系统 Python 3.13）
│  ├─ blender\{render_sprites.py, clean_model.py, rig_*.py, configs\*.json}
│  ├─ art\{sync_reference.py, check_reference.py, contact_sheet.py, quantize_palette.py, check_sprites.py}
│  └─ ci\{run_tests.sh, compile_check.sh, build_windows.sh, run_python_tests.sh}
├─ build\                         ← 已忽略：测试结果、日志
├─ Builds\                        ← 已忽略：打包产物 zip
└─ docs\{agents,design,handoff,review,licenses,playtest}\
```

### 5.3 真实命令（T03 完成后以 `PLAN_PROGRESS.md` 记录的为准）

| 用途 | 命令 |
| --- | --- |
| Unity 测试（全部） | `bash tools/ci/run_tests.sh all` → 内部依次执行 `unity test BingZhong --mode EditMode --output build/test-results/EditMode.xml` 和 PlayMode 的同类命令，`set -o pipefail`，返回真实退出码，并解析 XML 统计通过/失败数 |
| 回退方式（如果 `unity test` 需要已打开的编辑器而当时没开） | `"E:/Unity/6000.6.6f1/Editor/Unity.exe" -batchmode -projectPath BingZhong -runTests -testPlatform EditMode -testResults build/test-results/EditMode.xml -logFile build/logs/EditMode.log`，T03 确定用哪种 |
| 编译检查 | `bash tools/ci/compile_check.sh`（编辑器开着时用 `unity recompile`，没开时用 batchmode `-quit` 并检查日志里的 `error CS`） |
| 驱动编辑器 | `unity command …`、`unity list`；项目级 MCP：`claude mcp add --scope project unity -- unity mcp --project-path E:/MYGAME/opus_work/BingZhong`（重开会话后 MCP 工具才会出现） |
| Blender 渲染 | `blender -b <file.blend> -P tools/blender/render_sprites.py -- --config tools/blender/configs/<name>.json` |
| 资产检查 | `python tools/art/check_sprites.py art/render/<subject>/<clip>`、`python tools/art/check_reference.py art/reference` |
| Python 工具自测 | `bash tools/ci/run_python_tests.sh`（`python -m unittest discover tools/tests`） |
| 打包 | `bash tools/ci/build_windows.sh <版本>` → `unity build BingZhong --target StandaloneWindows64 --execute-method BingZhong.EditorTools.BuildScript.BuildRelease` |
| 远程 | `git -c http.proxy= -c https.proxy= push origin <ref>` |

### 5.4 关键技术约定（实现时写入 `docs/design/contracts.md`）

- **像素规格**：Pixel Perfect Camera 参考分辨率 480×270，**PPU = 16**（瓦片 16×16），整数缩放到 1080p（×4）和 4K（×8）。主角在 Blender 里渲染到 48–56 像素高，正交相机的 `ortho_scale` 由画布像素数和"米/像素"推算，写在渲染配置 JSON 里。
- **分层渲染**：人物层渲染时隐藏武器；武器层渲染时把人物设为 holdout（被人物挡住的武器像素直接透明）。两层同画布、同枢轴点，叠加后遮挡关系正确。法线层同样分两层。
- **帧率无关测试**：PlayMode 测试用 `Time.captureDeltaTime` 分别固定为 1/30、1/60、1/144 秒，在三种步长下跑同一组断言；输入缓冲、无敌帧和冷却统一用 `Time.unscaledTime` 系的自有时钟（打击停顿和慢动作期间不受 `timeScale` 影响）。
- **Unity 插件技能**：Unity 官方插件里的 skill 文档（如 `2d-pixel-perfect`）按需以只读方式从 `~/.claude/plugins/cache/unity-agent-plugin/` 读取，不修改全局设置去启用插件。

## 6. 与 Pre-Plan 的差异

| 类型 | 项目 | 原因 |
| --- | --- | --- |
| 修改 | 仓库根改为 `E:\MYGAME\opus_work`（Pre-Plan 写的是 `paperblade/`） | 你已经按 Agent 分好了工作目录；远程仓库名仍然是 paperblade |
| 修改 | T01 额外备份旧仓库的 **LFS 对象**（`git lfs fetch --all`），不只做 bundle | 旧仓库用了 LFS，bundle 不包含 LFS 内容；本地旧目录已经不在，这是唯一副本 |
| 修改 | T01 设置仓库本地的 git 身份；所有远程命令绕开失效代理 | E13、E14 的探测结果 |
| 修改 | T02 用 `unity projects new` + Universal 2D 模板创建项目；用 `unity pipeline install` 装 Pipeline 包；在项目内写 `.mcp.json` 注册 Unity MCP | E1、E3 的真实工具 |
| 修改 | T03 的测试入口确定为 `tools/ci/run_tests.sh`（bash），封装 `unity test` | Unity CLI 自带测试命令；Git Bash 可用 |
| 修改 | T05 增加从 `gemini_work` 同步参考图的步骤；问题清单加入第 4 节的预检结果 | 参考图实际存放位置 |
| 修改 | T07/T08/T19/T24 改用 Mixamo 自动绑骨 + Blender 原生 IK；T26 用 Blender 原生骨骼 | E4 ❌，你选了方案 B |
| 修改 | T06/T18/T24/T26/T29 的 Tripo 生成由你在网页上操作，我提供参数清单并负责导入、清理和验收 | 你选了手动操作（E6） |
| 修改 | 备份目录为 `E:\Backups\paperblade`（和项目在同一块盘） | 你的选择；盘坏时备份会和项目一起丢失，建议 T04 之后再另外复制一份 bundle 到其他盘或云端 |
| 修改 | T38 的性能验收：本机（RTX 4060）只做预检，并留出余量（本机 1080p 平均 ≥ 144 帧、1% 低帧 ≥ 100 帧作为内部门槛）；正式结论需要在 GTX 1060 级机器上测，或者由你认可用本机加余量代替 | 开发机远强于目标配置（E9） |
| 修改 | T41 默认由你手动上传 itch.io（butler 没装，安装需要你同意） | E20 |
| 新增 | `tools/ci/run_python_tests.sh` 和 `tools/tests/`：Python 工具脚本的单元测试 | Pre-Plan 要求新代码配套测试，Python 工具也包括在内 |
| 新增 | 磁盘空间风险 K12，每个里程碑汇报一次 E: 盘剩余空间 | E: 只剩 37 GB |
| 新增 | 风险 K13：强推后旧 LFS 对象仍然占用 GitHub LFS 额度 | 见第 1 节其他发现 3 |
| 删除 | 无 | — |

## 7. 任务列表（校准后）

通用约束（每个任务都适用）：数值放 ScriptableObject，不写死；新代码配套测试；任务开始前 `git status` 干净；验收通过后提交，信息以 `[TASK-Txx]` 开头；结束时更新 `PLAN_PROGRESS.md`；同一任务自动修复最多 3 次，仍失败就 `git reset --hard <任务开始前提交>` 回退并停止报告（这次回退已在计划里获批，只针对本任务的未推送提交）；运行检查时用 `set -o pipefail`，保留真实退出码。

需要你协助的步骤会提前一次性列出（见第 8.2 节），不会每一步都停下来。

### 第 1 周（2026-11-02 → 11-08）：C1、C2

#### T01 备份旧仓库并初始化本地仓库
- **目标**：把旧 paperblade 完整备份到项目目录以外（包括 LFS 对象），并在 `opus_work` 建立新仓库骨架。
- **产出**：`E:\Backups\paperblade\paperblade-mirror.git\`（mirror 克隆，含 `lfs/objects`）、`E:\Backups\paperblade\paperblade-<日期>.bundle`、`E:\Backups\paperblade\SHA256SUMS.txt`；仓库内的 `.gitattributes`（LFS：fbx/blend/glb/obj/png/jpg/psd/wav/ogg/mp3/ttf 等，加上 Unity YAML 合并规则）、`.gitignore`（Unity、Blender、`build/`、`Builds/`）、`PRE_PLAN.md`、`plan.md`、`PLAN_PROGRESS.md`、`docs/agents/{GEMINI_PREPLAN,GPT_PREPLAN}.md`、各空目录的 `.gitkeep`、`tools/requirements.txt`。
- **命令**：
  ```bash
  B='E:/Backups/paperblade'
  git -c http.proxy= -c https.proxy= clone --mirror https://github.com/bernardleex526/paperblade.git "$B/paperblade-mirror.git"
  git -C "$B/paperblade-mirror.git" -c http.proxy= -c https.proxy= lfs fetch --all
  git -C "$B/paperblade-mirror.git" bundle create "$B/paperblade-$(date +%Y%m%d).bundle" --all
  git init -b main && git config user.name bernard && git config user.email bernard@localhost
  git lfs install --local
  ```
- **验收**：`git bundle verify <bundle>`；`git -C <mirror> for-each-ref` 与 `git ls-remote origin` 逐条对比；`git -C <mirror> lfs fsck`；`git lfs track`；`git status --porcelain` 为空；`git branch` 包含 `main` 和 `dev/demo`。
- **通过标准**：bundle 校验通过，并且包含远程全部分支和标签；LFS 对象完整（`lfs fsck` 无缺失）；新仓库工作区干净，首个提交在 `main`，`dev/demo` 从它分出。
- **回退**：删除新建的 `.git` 和新增文件（只删本任务创建的），备份保留。
- **依赖**：C1。**人工检查点**：否。

#### T02 创建 Unity 项目并完成基础设置
- **目标**：创建 `BingZhong` Unity 项目，配置 URP 2D、Pixel Perfect 480×270、所需包、Pipeline 包和项目级 Unity MCP。
- **产出**：`BingZhong/`（Universal 2D 模板）；`Packages/manifest.json` 加入 Input System、2D Animation、2D Tilemap Extras、Cinemachine、Test Framework、Timeline、Pipeline 包（版本以实际解析结果为准，写入 `PLAN_PROGRESS.md`）；URP 2D Renderer 资源；`Assets/_Project/` 目录骨架；空场景 `Scenes/Boot.unity`，带 Pixel Perfect Camera（480×270，PPU 16）；Player Settings：Windows x64、Mono、产品名"兵冢"、公司名 bernard；仓库根的 `.mcp.json`。
- **命令**：`unity projects new BingZhong --template com.unity.template.universal-2d …`（参数以 `unity projects new --help` 为准）；`unity pipeline install --project-path BingZhong`；`claude mcp add --scope project unity -- unity mcp --project-path E:/MYGAME/opus_work/BingZhong`。
- **验收**：batchmode 打开项目并 `-quit`，日志中 `error CS` 数为 0；读取 `Packages/packages-lock.json` 确认包齐全（E10）；`unity list` 能列出 Pipeline 注册的工具，`unity command` 做一次只读查询（E3）；用 Pipeline 截取 Game 视图，图中像素边缘锐利、没有模糊。
- **通过标准**：编译错误 0；包列表完整；E3、E10 有明确结论；截图不模糊。
- **回退**：删除 `BingZhong/` 后重建。**依赖**：T01。**人工检查点**：否。

#### T03 搭建自动化测试脚手架（基线）
- **目标**：一条命令跑 EditMode 和 PlayMode 测试，输出结果文件；Python 工具也有测试入口。
- **产出**：`Assets/_Project/Tests/EditMode/`、`PlayMode/` 各 1 个示例测试和 asmdef；PlayMode 示例里包含"三种步长"参数化测试样板；`tools/ci/run_tests.sh`、`tools/ci/compile_check.sh`、`tools/ci/run_python_tests.sh`、`tools/ci/build_windows.sh`（占位）。
- **验收**：`bash tools/ci/run_tests.sh all; echo $?`；`bash tools/ci/run_python_tests.sh; echo $?`。
- **通过标准**：示例测试全部通过，退出码 0，生成 `build/test-results/{EditMode,PlayMode}.xml`；人为让一个测试失败时，脚本退出码非 0（验证完再改回）。这就是项目基线。
- **回退**：回到 T02 的提交。**依赖**：T02。**人工检查点**：否。

#### T04 强制推送覆盖 paperblade（高风险确认）
- **目标**：用新仓库覆盖远程 `bernardleex526/paperblade`。
- **前置检查**：T01 备份校验通过；`git -c http.proxy= -c https.proxy= push --dry-run --force origin main` 成功（E14 推送权限）。
- **执行前**：告诉你"即将覆盖 paperblade，远程旧分支 main（2299c8b8）将被替换，备份位于 <路径>"，**等你回复后**才执行：`git -c http.proxy= -c https.proxy= push --force origin main dev/demo`，再删除远程上其他旧分支和标签（目前只有 `main`）。
- **验收**：`git ls-remote origin`；GitHub API 读取根目录文件列表。
- **通过标准**：远程 `main` 等于本地 `main`；根目录不再有 `PaperBlade/` 等旧文件。
- **回退**：`git clone <bundle>` → 推回 LFS 对象 → `git push --mirror --force`（同样需要你确认）。
- **依赖**：T03。**人工检查点**：是（高风险确认，整个项目只强推这一次）。

#### T05 登记 Gemini 参考图并校验清单 → C2
- **目标**：把参考图入库，检查完整性和一致性，为 C2 准备材料。
- **产出**：`tools/art/sync_reference.py`（从 `../gemini_work/art/reference` 单向复制并校验哈希）、`tools/art/check_reference.py`（ID 齐全、命名规范、长边 ≥ 1024、manifest 与文件一致、四视图头顶和脚底对齐、背景色是否接近 #D9D9D9）、`tools/art/contact_sheet.py`（总览拼图，并附 50 像素高的缩小预览）、`art/reference/**`、`docs/handoff/C2-reference-art.md`（含第 4 节问题、4 把武器种类确认项）。
- **验收**：`python tools/art/check_reference.py art/reference; echo $?`；Python 单元测试。
- **通过标准**：G00–G09 全部存在，每张图长边 ≥ 1024（不合格项写入问题清单，由你在 C2 决定）。
- **回退**：标出不合格的图，请你让 Gemini 重做那一张。
- **依赖**：T01；Gemini 交付 G00–G09。**人工检查点**：是（**C2**：画风、配色、造型定稿；4 把武器种类；manifest 状态改为 `approved`）。
- **说明**：目前 G04–G09 还没有生成，T05 会先用现有图完成脚本和测试，等你通知"参考图已就位"后再出正式 C2 材料。等待期间我继续做不依赖 C2 的 T11（输入系统）。

### 第 2 周（11-09 → 11-15）：C3

#### T06 生成并清理主角和首把武器的 3D 模型
- **目标**：主角（Tripo Smart Mesh P2.0 多视图，四边形拓扑、最高精度，再生成纹理）和重剑（H3.1），在 Blender 里清理。
- **产出**：`art/source/player/{player_tripo.glb, player_clean.blend}`、`art/source/weapons/greatsword/{greatsword_tripo.glb, greatsword_clean.blend}`、`tools/blender/clean_model.py`（统一比例、原点放到脚底、应用变换、修复非流形、输出报告 JSON）、`tools/blender/multiview_shot.py`、`docs/handoff/T06-compare.png`。
- **Tripo 方式**：由你在网页上操作。我先写好 `docs/handoff/tripo-T06.md`（每个资产用哪张参考图、选哪个模型、拓扑和精度设置、导出格式和保存路径），你生成并下载到 `art/source/…/` 后告诉我，我负责导入、清理和验收。
- **验收**：`blender -b art/source/player/player_clean.blend -P tools/blender/clean_model.py -- --report-only`；生成多机位对比图。
- **通过标准**：非流形边 0、变换已应用、原点在脚底、主角面数 ≤ 30k 三角面（初值）、武器 ≤ 8k；轮廓和比例与参考图一致。
- **回退**：edit mesh 局部重生成或整体重生成；同一资产超过 3 次先问你。
- **依赖**：T05、C2。**人工检查点**：否（在 C3 一起复核）。

#### T07 主角绑骨蒙皮（Mixamo 自动绑骨 + Blender 原生骨骼）
- **目标**：用 Mixamo 自动绑骨得到 `mixamorig` 骨骼和蒙皮，在 Blender 里修权重，加武器插槽骨骼 `weapon_socket.R`（必要时加 `.L`，供双刃使用）和手臂 IK 控制骨。
- **流程**：我从 `player_clean.blend` 导出 `art/source/player/player_for_mixamo.fbx` → 你上传到 Mixamo 自动绑骨（放 5 个标记点）并下载带蒙皮的 FBX 到 `art/source/mixamo/player_rigged.fbx` → 我在 Blender 里导入、修权重、加插槽和 IK。
- **产出**：`art/source/player/player_rig.blend`、`tools/blender/rig_*.py`、`docs/handoff/T07-poses.png`（抬手、弯腿、转身、下蹲、挥砍、翻滚 6 个姿势）。
- **验收**：脚本把 6 个测试姿势各渲染正面和侧面两张；检查脚本统计权重为 0 的顶点数、每个顶点影响骨骼数 ≤ 4。
- **通过标准**：6 个姿势都没有明显的穿模或拉扯；没有权重为 0 的顶点。
- **回退**：手修权重或请你在 Mixamo 重新放标记点；战袍下摆、背部装饰修不到合格时停下来，带对比图请你决定。
- **依赖**：T06；E8。**人工检查点**：否。

#### T08 接入主角基础动作
- **目标**：接入移动类动作和 1 段重剑轻击，共约 15 个：待机、跑、起跳、上升、下落、落地、二段跳、翻滚、受击、死亡、贴墙下滑、蹬墙跳、攀边、下劈、重剑轻击 1。
- **产出**：`art/source/mixamo/*.fbx`（你下载，我提供关键词清单 `docs/handoff/mixamo-list.md`）、`art/source/player/anims/*.blend`。
- **验收**：每个动作抽 3 帧，做多机位截图；脚本检测脚底在地面接触帧的水平位移（脚滑）。
- **通过标准**：没有脚滑、穿模或武器脱手；循环动作首尾帧姿态差小于阈值。
- **回退**：换一个 Mixamo 动作或手调。**依赖**：T07；E8。**人工检查点**：否。

#### T09 编写像素渲染脚本（E12 小样）
- **目标**：一条命令把 Blender 动作批量渲染成像素序列帧和元数据。
- **产出**：`tools/blender/render_sprites.py`、`tools/blender/configs/*.json`、`tools/art/quantize_palette.py`、`tools/art/check_sprites.py`、`art/palette/bingzhong.png`（按决策 #9 的配色）、`art/render/player/<clip>/{body,weapon,normal_body,normal_weapon}/*.png` 和 `meta.json`（格式见 Pre-Plan §6.5）。
- **技术要点**：EEVEE Next 正交相机；Shader to RGB + 2–3 级色阶卡通着色；关闭抗锯齿（采样 1、滤波 0）；描边和量化在 Python 后处理里完成（1 像素外描边、alpha 二值化、调色板最近色映射）；法线 pass 单独渲染；武器层用人物 holdout。
- **验收**：渲染待机、跑、重剑轻击 1，然后运行 `python tools/art/check_sprites.py art/render/player`。
- **通过标准**：主角高 48–56 像素；alpha 只有 0 和 255；颜色全部在调色板内；人物层和武器层同画布、同枢轴点；检查脚本 0 个错误。
- **回退**：调整描边、量化或比例参数并写进 C3 交接单；需要改主角像素高度时先问你。
- **依赖**：T07。**人工检查点**：否。

#### T10 Unity 序列帧导入管线和样片场景 → C3
- **目标**：渲染结果自动导入 Unity，在铸坊风格样片场景里验证分层渲染和 2D 光照。
- **产出**：`Assets/_Project/Editor/SpriteImport/{SpriteImportPostprocessor.cs, SpriteClipBuilder.cs}`（点过滤、无压缩、PPU 16、枢轴点、法线设为次级贴图 `_NormalMap`、生成 `SpriteClip` 资源）；`Runtime/Animation/{LayeredSpriteAnimator.cs, SpriteClip.cs}`；`Scenes/Pipeline_Sample.unity`；`docs/handoff/C3-pipeline.md`（附 1080p 截图和 10 秒录屏）。
- **验收**：`bash tools/ci/run_tests.sh EditMode`（导入设置测试）；PlayMode 运行样片场景并截图、录屏。
- **通过标准**：1080p 下像素不模糊、没有亚像素抖动；2D 光照生效，法线起作用；人物层和武器层没有错位。
- **回退**：E11 不成立时改成把武器烘焙进人物帧，并在 C3 说明。
- **依赖**：T02、T08、T09。**人工检查点**：是（**C3**）。

### 第 3 周（11-16 → 11-22）：C4，之后 R1

| ID | 目标 | 主要产出（路径前缀 `BingZhong/Assets/_Project/`） | 验收命令 | 通过标准 | 依赖 |
| --- | --- | --- | --- | --- | --- |
| T11 | 键鼠和手柄输入、改键、改键持久化 | `Input/BingZhong.inputactions`（移动、跳跃、翻滚、武器 A、武器 B、斩杀、交互、暂停）、`Runtime/Input/{InputReader,InputBuffer,RebindService}.cs`、`Tests/PlayMode/InputTests.cs`（InputTestFixture） | `bash tools/ci/run_tests.sh all` | 键盘和手柄都能触发全部动作；改键写入后重新加载仍然生效 | T02 |
| T12 | 决策 #14 的全部移动招式 | `Runtime/Player/{PlayerMotor,MovementConfig}.cs`、`Data/Player/MovementConfig.asset`、`Tests/PlayMode/MovementTests.cs`；超过 3 个文件时拆成 T12a 地面和跳跃、T12b 贴墙和蹬墙、T12c 翻滚和攀边 | 同上 | 离开边缘 0.09 秒起跳成功、0.12 秒失败；落地前 0.14 秒按跳被缓冲；二段跳在落地或贴墙后重置；蹬墙跳方向正确；翻滚全程无敌；能穿过单向平台；**1/30、1/60、1/144 三种步长结果一致** | T10、T11 |
| T13 | 帧数据驱动的攻击、命中、伤害、韧性与破绽、斩杀入口 | `Runtime/Combat/{AttackData,HitBox,HurtBox,Poise,ExecutionController,DamageInfo}.cs` 及 EditMode/PlayMode 测试 | 同上 | 只有判定帧能命中；同一攻击不重复命中同一目标；韧性归零进入 1.5 秒破绽；只有破绽中能斩杀；翻滚无敌期间不受伤 | T12 |
| T14 | 双持槽和衔接技 | `Runtime/Weapons/{WeaponSlots,SwitchLinkResolver}.cs` 及测试 | 同上 | 窗口内按另一把武器键触发衔接技（分地面版和空中版），窗口外只触发普通起手；衔接技触发 `OnSwitchLink` | T13 |
| T15 | 打击停顿、震屏、手柄震动、5 种材质喷溅、斩杀慢动作和闪屏 | `Runtime/Feedback/{HitStop,ShakeService,RumbleService,SplatterVfx,TimeScaleService}.cs`、5 个喷溅预制体、测试 | 同上 + 录屏 | 每种命中都有完整反馈；暂停、场景切换、主角死亡后 `timeScale` 都能恢复为 1；键鼠玩家不触发震动；设置里能调震屏强度、关闭震动 | T13 |
| T16 | 敌人状态机和锈卒灰盒 | `Runtime/Enemies/{EnemyBrain,EnemyDefinition}.cs`、`Data/Enemies/RustSoldier.asset`、测试 | 同上 | 感知范围正确；前摇 ≥ 0.4 秒；两连砍；能被打出破绽并被斩杀；没有卡死状态 | T13 |
| T17 | 灰盒沙盒和调试 HUD → **C4**，之后 R1 | `Scenes/Sandbox.unity`（平台、墙、单向平台、木桩、3 个锈卒）、`Runtime/DebugTools/DebugHud.cs`（状态、帧数据、韧性、输入记录、运行时调参，Release 构建中剔除）、第二把武器用通用挥砍占位、`docs/handoff/C4-combat.md`、`docs/handoff/R1.md` | 全部测试 + 开发构建 + 2 分钟战斗录屏 | 测试全部通过；原型连续玩 10 分钟无异常 | T14、T15、T16 |

T11–T17 的回退方案都是"回到任务开始前的提交"，人工检查点只有 T17（C4）。R1 交接单写好后我继续做第 4 周任务，不等审查结果。

### 第 4–5 周（11-23 → 12-06）：C5 在第 4 周末，之后 R2

| ID | 目标 | 主要产出 | 验收命令 | 通过标准 | 依赖 |
| --- | --- | --- | --- | --- | --- |
| T18-1..4 | 每把武器的基础形态和进化形态模型 | `art/source/weapons/<武器>/{base,evolved}.{glb,blend}`、挂到插槽后的多机位对比图 | `blender -b … -P tools/blender/clean_model.py -- --report-only`；对比图 | 造型与 G04/G05 一致；挂到插槽上的位置和角度正确 | T09、C4 |
| T19-1..4 | 每把武器约 11 段主角动作（轻击 3、蓄力/释放 2、空中 2、特殊 1、衔接 1、处决 1、进化招式 1）并渲染 | `art/source/player/anims/<武器>_*.blend`、`art/render/player/<武器>_*/`（人物层、基础/进化两种武器层、法线） | `python tools/art/check_sprites.py art/render/player`；握持多机位截图 | 手掌和武器接触稳定，没有脱手、穿模或悬空；检查脚本 0 个错误 | T08、T18-n |
| T20-1..4 | 武器招式数据、连段、取消窗口、特有逻辑（连弩投射物用对象池） | `Data/Weapons/<武器>/*.asset`、`WeaponDefinition`；T20-1 时写 `docs/design/weapons.md` | `bash tools/ci/run_tests.sh all` | 连段能打完；衔接技能触发；木桩 10 秒 DPS 落在设计区间内 | T14、T19-n |
| T24-1..4 | 锈卒、熔浆炉灵、链锤监工、老铸匠残魂的模型、骨骼、动画和渲染 | `art/source/enemies/<名称>/*`、`art/render/enemies/<名称>/*`；精英变体在 Unity 里用换色着色器 | `python tools/art/check_sprites.py art/render/enemies` | 动画数量符合决策 #13（锈卒约 8、炉灵约 6、监工约 9、老铸匠 2）；0 个错误 | T09、C4 |
| T26 | Boss 模型、自定义骨骼（Blender 原生，分段刚体）、待机/熔炉喷火/砸地 3 段 → **C5** | `art/source/boss/*`、`art/render/boss/*`、`docs/handoff/C5-boss.md`、附 `docs/design/parts.md` | 多机位截图；`check_sprites.py` | 动作流畅、没有明显穿插 | T09、C4 |
| T21 | 配件系统框架：3 槽、定义、10 个效果钩子、替换和分解、武器层外观变化 | `Runtime/Weapons/{WeaponInstance,PartDefinition,PartEffect,PartHookBus}.cs`；`docs/design/parts.md`（18–24 个配件、4 个进化配方） | `bash tools/ci/run_tests.sh EditMode` | 槽位规则、钩子调用顺序（有文档）、防递归（来源标记 + 深度上限）、叠加规则、配方识别的测试全部通过 | T20-1 |
| T22-1..3 | 锋刃、握柄、核芯各 6–8 个配件和特效 | `Data/Parts/<槽位>/*.asset`、效果脚本、特效预制体 | `bash tools/ci/run_tests.sh all` | 每个配件一个 PlayMode 测试，效果可观测；每个配件都有图鉴文本占位 | T21 |
| T23 | 武器进化：演出、换武器层、解锁进化招式、图鉴记录；之后 R2 | `Runtime/Weapons/EvolutionService.cs`、`Data/Recipes/*.asset`、`docs/handoff/R2.md` | 同上 | 配方组合触发进化，非配方组合不触发；替换配方配件后状态正确；外观、招式、图鉴三者一致 | T22-全部、T20-全部 |
| T25-1..3 | 3 种敌人行为和 3 个精英词缀（护盾光环、狂暴、死亡爆炸） | `Data/Enemies/*.asset`、行为脚本、`EliteAffix` 及测试 | 同上 | 前摇时长、攻击范围、韧性、各词缀效果的测试通过 | T16、T24-n |
| T27 | Boss 剩余约 9–12 段动画 | `art/render/boss/*` | `check_sprites.py`；多机位截图 | 数量和质量与前 3 段一致 | C5 |
| T28 | Boss 战 AI：两阶段招式表、抓兵器换武器层、专属处决、血条、台词钩子 | `Runtime/Boss/*`、`Data/Boss/*` 及测试 | `bash tools/ci/run_tests.sh all` | 血量 50% 切换阶段；每个招式都能触发；处决造成最大生命 15% 的伤害；完整打一场不卡死 | T27、T13、T15 |

美术任务的回退是"重新生成或简化动作，同一资产重新生成超过 3 次先问你"；T19 前 3 段轻击迭代 3 次仍不达标时停下，触发"是否购买动作包"的推迟决策。代码任务的回退是"回到任务开始前的提交"。

### 第 6 周（12-07 → 12-13）：关卡与系统，R3

| ID | 目标 | 主要产出 | 验收命令 | 通过标准 | 依赖 |
| --- | --- | --- | --- | --- | --- |
| T29 | 锈火铸坊瓦片集、约 10 个装饰物、2–3 层视差背景 | `docs/design/scene-assets.md`（哪些用 Tripo、哪些在 Blender 搭）、Rule Tile、装饰物、视差层 | 测试房间截图与 G08 对比；瓦片拼接检查 | 风格和配色与 G08 一致；瓦片无接缝 | T09、C2 |
| T30 | 房间模板格式、拼接器、可通行检查；之后 R3 | `Runtime/Level/{RoomTemplate,LevelGenerator,TraversalValidator}.cs`、5 个占位模板、`Tests/EditMode/LevelGenSeedTests.cs`（含 2–3 个"跳不过去"的反例模板）、`docs/handoff/R3.md` | `bash tools/ci/run_tests.sh EditMode` | 1000 个种子 100% 通过；同种子结果相同；单次生成 < 200 ms；跳跃参数直接读 MovementConfig；反例被判失败 | T12、T03 |
| T31-1..5 | 约 20 个房间模板（每批 5 个）+ Boss 竞技场和枢纽区 | `Assets/_Project/Level/Rooms/*.prefab` | 每个模板单独做可通行检查；全部模板重跑 1000 个种子 | 全部通过 | T29、T30 |
| T32 | 单局流程与掉落：枢纽 → 选武器 → 3 张地图 → Boss → 结算；配件掉落节奏、残片、金币、出口回血 30%、淬火台；调试指令（Release 中剔除） | `Runtime/Core/{GameManager,RunState}.cs`、掉落表、`Runtime/DebugTools/DebugCommands.cs` | 模拟 100 局统计掉落；调试指令快速走完一局 | 每局 6–8 个配件；完整流程能走通 | T31-全部、T22-全部、T25-全部、T28 |
| T33 | 存档（JSON、version、校验和、`.bak`）、局外解锁、配件池、图鉴 | `Runtime/Meta/{SaveService,MetaProgress,Codex}.cs` 及测试 | `bash tools/ci/run_tests.sh EditMode` | 保存后读取完全一致；损坏时用 `.bak` 恢复；`.bak` 也损坏时用默认存档且不崩溃，损坏文件保留 | T32 |
| T34 | 熔度 1–3、神兵记忆、真结局钩子 | `Data/Heat/*.asset`、`Runtime/Meta/Memories.cs` 及测试 | `bash tools/ci/run_tests.sh all` | 各级熔度参数生效且能单独开关；首次进化后解锁记忆；满足条件触发真结局 | T33、T23 |

### 第 7 周（12-14 → 12-20）：打磨、审查、外部试玩

| ID | 目标 | 主要产出 | 验收命令 | 通过标准 | 依赖 |
| --- | --- | --- | --- | --- | --- |
| T35 | 叙事内容：开场演出（约 30 秒，可跳过）、老铸匠约 15 句、4–6 处碑文、全部图鉴文本、Boss 台词、4 段记忆（G10 插图）、结局画面 | `Data/Narrative/*.json`、开场 Timeline | `bash tools/ci/run_tests.sh all` | 所有文本编号都有内容；开场能播放也能跳过（文案在 C6 由你审阅） | T32、T34 |
| T36 | UI：HUD、配件拾取/替换、图鉴、解锁商店、暂停、设置、改键；中文像素字体 | `Runtime/UI/*`、UI 预制体、字体和 `docs/licenses/fonts.md` | 同上 | 手柄和键盘导航测试通过；1080p 和 4K 截图清晰 | T33、T11 |
| T37 | 40–60 个音效、3 首背景音乐、混音 | `Assets/_Project/Audio/*`、`docs/design/audio.md`、`docs/licenses/audio.md` | 同上 | 每个音效编号都有音频；每个音频都有授权记录（AI 音乐生成前先报费用） | T32；E18 |
| T38 | 性能：压力场景（15 个敌人 + 最多特效） | `Scenes/Stress.unity`、`docs/handoff/perf.md`、Profiler 截图和帧时间 CSV | 开发构建 + 采样 60 秒 | 本机 1080p 平均 ≥ 144 帧、1% 低帧 ≥ 100 帧（内部门槛）；目标机或你认可的替代方式下平均 60 帧、1% 低帧 ≥ 50 帧；每帧没有 GC 分配 | T32；E9 |
| T39 | 自动化冒烟：不同种子连续跑完 10 局 | `Tests/PlayMode/SmokeRunTests.cs` | `bash tools/ci/run_tests.sh PlayMode` | 10 局无异常，流程完整，存档正确更新 | T32、T33、T03 |

### 第 8 周（12-21 → 12-27）：缓冲、打包、C6

| ID | 目标 | 主要产出 | 验收命令 | 通过标准 | 依赖 | 检查点 |
| --- | --- | --- | --- | --- | --- | --- |
| T40 | Windows Release 打包，剔除调试功能和作弊键，在干净环境运行 | `tools/ci/build_windows.sh`、`Editor/Build/BuildScript.cs`、`Builds/BingZhong_Demo_<版本>.zip`（不入库） | `bash tools/ci/build_windows.sh 0.1.0`；检查打包日志；搜索 Release 程序集中的调试入口 | 打包错误 0；干净电脑（或新建的 Windows 用户）上能完整打一局；调试功能不可用 | T35–T39；E19 | 否 |
| T41 | 上传 itch.io（草稿或受限访问） | itch.io 项目页 | 另一个浏览器打开链接，下载并运行 | 能下载、能运行 | T40；E20 | **是（对外发布前问你）** |
| T42 | 整理最终验收材料 | `docs/handoff/C6-final.md`（内容清单逐项写触发方法 + 测试结果 + 1000 种子报告 + 性能报告 + 基线对比）、`docs/playtest/` 模板 | 逐项对照 Pre-Plan §12 | GPT 结论为"通过"或"有条件通过"，且你完成了人工判断项 | T41 | **C6**（GPT `V-final.md` 之后） |

**进度落后超过 1 周时的砍项顺序**（需要你确认后执行）：熔度 3 → 第 4 把武器 → 配件降到 18 个 → 模板降到 16 个 → 神兵记忆插图改成纯文字。

## 8. C1 待确认事项

### 8.1 需要你现在决定的（影响第 1–2 周）

1. **批准本计划**（C1）——唯一剩下的阻塞项。
2. ~~绑骨方案~~ → 已定：方案 B（Mixamo 自动绑骨 + Blender 原生 IK）。
3. ~~仓库位置~~ → 已定：`E:\MYGAME\opus_work`；GPT 直接在这里只读审查，Gemini 的图由 T05 从 `gemini_work` 同步。
4. ~~备份目录~~ → 已定：`E:\Backups\paperblade`（注意和项目在同一块盘，建议 T04 后再复制一份 bundle 到其他盘）。
5. **git 身份**：默认沿用旧仓库的 `bernard <bernard@localhost>`，只写入仓库本地配置。需要用 GitHub noreply 邮箱的话请在批准时告诉我，不说就按默认值。

### 8.2 需要你在对应任务前准备的清单（一次性列出）

| 时间 | 事项 | 对应 |
| --- | --- | --- |
| C2 前 | 让 Gemini 补齐 G04–G09，并处理第 4 节的问题；通知我"参考图已就位" | T05 |
| C2 前 | 确认 4 把武器种类（默认：双刃、重剑、长枪、连弩，Gemini 队列已经按这 4 种排好） | C2 |
| T06 前 | Tripo 剩余 credits 数量（生成用网页手动操作，参数清单由我提供） | E7 |
| T07/T08 前 | Mixamo 账号可用；按我给的清单上传模型自动绑骨、下载动作 FBX 到 `art/source/mixamo/` | E8 |
| 第 1 周内 | 在 GitHub 设置里查看 LFS 存储和流量额度（Settings → Billing） | E13 |
| T37 前 | 确认 CC0 音效来源和 AI 音乐服务（以及费用） | E18 |
| T40 前 | 有没有没装 Unity 的 Windows 电脑；没有的话我用新建的 Windows 本地用户测试（新建用户属于系统级修改，到时再问你） | E19 |
| T41 前 | itch.io 账号；是否允许安装 butler（不安装就由你手动上传） | E20 |
| 任意时间 | 在 GPT 侧确认 Codex 能读取 `E:\MYGAME\opus_work` 并运行 `bash tools/ci/run_tests.sh` | E17 |

### 8.3 可选

- 是否在全局设置里启用 Unity 官方插件 `unity@unity-agent-plugin`（会修改 `~/.claude/settings.json`，属于项目目录以外的修改）。不启用也不影响执行，我会以只读方式读取它的 skill 文档。
- 是否启动 `127.0.0.1:12000` 的代理。不启动也可以，我对远程命令加 `-c http.proxy= -c https.proxy=` 直连。

## 9. 风险登记（在 Pre-Plan §10 基础上补充）

Pre-Plan 的 K1–K11 全部保留，新增：

| # | 风险 | 可能性 | 影响 | 缓解 | 回退 |
| --- | --- | --- | --- | --- | --- |
| K12 | E: 盘只剩 37 GB，Unity Library、LFS 缓存和渲染帧会持续占用空间 | 中 | 中 | 每个里程碑汇报剩余空间；渲染中间文件放 `build/`，可以随时清理；低于 10 GB 时提醒你 | 把渲染缓存移到 C:；`unity projects clean` 清理可重建缓存（需要你同意） |
| K13 | 强推后旧仓库的 LFS 对象仍然占用 GitHub LFS 额度 | 高 | 低 | 在 T04 确认时告知你 | 如需释放，只能删除并重建仓库（属于高风险操作，由你决定） |
| K14 | 参考图画风（国风水墨、写实比例）和像素管线结果差距大 | 中 | 中 | C2 时用 50 像素缩小预览让你提前看到效果；C3 用真实渲染确认 | 调整参考图，或者调整卡通着色参数 |
| K15 | `unity` CLI 是 beta 版（1.0.0-beta.13） | 中 | 低 | 所有 CI 脚本都保留直接调用 `Unity.exe -batchmode` 的回退路径 | 只用 `Unity.exe` 批处理 |

## 10. 执行协议（批准后生效）

- 批准后先创建 `PLAN_PROGRESS.md`（任务 checkbox、当前任务、已知问题、真实命令、包版本、消耗记录），每完成一个任务就更新。
- 每次会话开始：读 `PRE_PLAN.md`、`plan.md`、`PLAN_PROGRESS.md`，检查 `docs/review/` 里有没有还没处理的报告，把"阻断"和"严重"问题插入为 `TF-Rn-x`。
- 每个任务完成后记录一行：`[TASK-Txx] 完成 | 验收输出摘要`，然后直接进入下一个任务。
- 只在这些情况停下：C2–C6 检查点、T04 强推前、T41 发布前、自动修复 3 次仍失败（已回退）、需要修改计划外的文件或执行未批准的高风险操作、发现与已确认决策冲突的新事实。
- 任何时候都要先问你的操作：强推（只有 T04）、删除资产文件/分支/存档、改写已推送的历史、付费操作、对外发布、修改项目目录以外的文件或安装系统级软件。T01 写入备份目录属于本计划批准的例外。
- 紧急停止：你说"停"之后，我做完当前这一小步，把未完成的工作以 `[WIP]` 开头提交到 `dev/demo`，更新 `PLAN_PROGRESS.md`，然后等待指示。
