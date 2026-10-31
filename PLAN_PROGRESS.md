# PLAN_PROGRESS — 兵冢执行进度

> 唯一的进度记录。会话中断或上下文压缩后，先读 `PRE_PLAN.md`、`plan.md` 和本文件再继续。
> 命令、路径、版本以本文件为准（GPT 审查也以本文件为准）。

## 当前状态

- **当前任务**：T02
- **当前分支**：`dev/demo`
- **最近检查点**：C1 计划批准（2026-10-31）
- **下一个停点**：T04 强推前确认

## 已确认的执行设置（C1，2026-10-31）

- 仓库根 `E:\MYGAME\opus_work`；远程 `https://github.com/bernardleex526/paperblade.git`；工作分支 `dev/demo`
- 本地 git 身份 `bernard <bernard@localhost>`（仅仓库本地配置）；`core.autocrlf=false`、`core.longpaths=true`
- 绑骨：Mixamo 自动绑骨 + Blender 原生骨骼和 IK（替代 Auto-Rig Pro，决策 #16 的调整）
- Tripo：你在网页上手动操作，我提供参数清单并负责导入和验收
- 旧仓库备份：`E:\Backups\paperblade`
- 远程命令一律加 `-c http.proxy= -c https.proxy=`（全局代理 127.0.0.1:12000 未运行）

## 任务清单

### 第 1 周
- [x] T01 备份旧仓库并初始化本地仓库
- [ ] T02 创建 Unity 项目并完成基础设置
- [ ] T03 搭建自动化测试脚手架（基线）
- [ ] T04 强制推送覆盖 paperblade（高风险确认）
- [ ] T05 登记 Gemini 参考图并校验清单 → C2

### 第 2 周
- [ ] T06 主角和重剑 3D 模型
- [ ] T07 主角绑骨蒙皮（Mixamo + 原生 IK）
- [ ] T08 主角基础动作
- [ ] T09 像素渲染脚本
- [ ] T10 序列帧导入管线和样片场景 → C3

### 第 3 周
- [ ] T11 输入系统与改键
- [ ] T12 主角移动控制器
- [ ] T13 战斗核心
- [ ] T14 双持切换与衔接技
- [ ] T15 打击反馈
- [ ] T16 敌人 AI 框架和锈卒灰盒
- [ ] T17 灰盒沙盒与调试 HUD → C4，R1

### 第 4–5 周
- [ ] T18-1 [ ] T18-2 [ ] T18-3 [ ] T18-4 武器资产
- [ ] T19-1 [ ] T19-2 [ ] T19-3 [ ] T19-4 主角武器动作
- [ ] T20-1 [ ] T20-2 [ ] T20-3 [ ] T20-4 武器招式数据
- [ ] T24-1 [ ] T24-2 [ ] T24-3 [ ] T24-4 敌人和 NPC 资产
- [ ] T26 Boss 模型、骨骼和前 3 段动画 → C5
- [ ] T21 配件系统框架
- [ ] T22-1 [ ] T22-2 [ ] T22-3 配件内容
- [ ] T23 武器进化系统 → R2
- [ ] T25-1 [ ] T25-2 [ ] T25-3 敌人 AI 与精英词缀
- [ ] T27 Boss 剩余动画
- [ ] T28 Boss 战 AI

### 第 6 周
- [ ] T29 瓦片集、装饰物和视差背景
- [ ] T30 房间模板、拼接器与可通行检查 → R3
- [ ] T31-1 [ ] T31-2 [ ] T31-3 [ ] T31-4 [ ] T31-5 房间模板
- [ ] T32 单局流程与掉落
- [ ] T33 存档与局外成长
- [ ] T34 熔度、神兵记忆与真结局钩子

### 第 7 周
- [ ] T35 叙事内容
- [ ] T36 UI
- [ ] T37 音频
- [ ] T38 性能优化
- [ ] T39 自动化冒烟测试

### 第 8 周
- [ ] T40 Windows 打包与干净电脑测试
- [ ] T41 上传 itch.io（高风险确认）
- [ ] T42 整理最终验收材料 → C6

## 真实命令（随任务校准）

| 用途 | 命令 | 校准于 |
| --- | --- | --- |
| 旧仓库备份校验 | `git init -q <空目录> && git -C <空目录> bundle verify E:/Backups/paperblade/paperblade-<日期>.bundle`；`git -C E:/Backups/paperblade/paperblade-mirror.git lfs fsck` | T01 |
| 旧仓库恢复 | `git clone file:///E:/Backups/paperblade/paperblade-mirror.git <目录>`（带 LFS 内容；只用 bundle 克隆会因缺 LFS 对象而检出失败） | T01 |

## 环境与版本

| 项 | 版本 / 路径 |
| --- | --- |
| Unity Editor | 6000.6.6f1，`E:\Unity\6000.6.6f1\Editor\Unity.exe`（Mono，无 IL2CPP） |
| Unity CLI | `unity` 1.0.0-beta.13 |
| Blender | 5.2.2 LTS，`E:\Tools\blender-5.2.2-windows-x64\blender.exe`，内置 Python 3.13.13、numpy 2.3.4 |
| Python（工具脚本） | 3.13.14，Pillow 12.3.0，numpy 2.5.1 |
| Git / LFS | 2.54.0.windows.1 / 3.7.1 |

## 消耗记录（每个里程碑汇报）

| 日期 | Tripo credits | 生成图 | AI 音乐 | 备注 |
| --- | --- | --- | --- | --- |
| 2026-10-31 | 0 | 0 | 0 | 阶段 A/B 无付费消耗 |

## 已知问题

- 参考图预检问题（画风方向、G01 与 G02 角色不一致、G02 头身比、G03 分辨率不足等），见 `plan.md` 第 4 节，T05/C2 处理。
- E: 盘剩余空间约 37 GB（K12）。
- 强推后旧仓库的 LFS 对象仍占用 GitHub LFS 额度（K13）。

## 执行日志

（每完成一个任务记一行：`[TASK-Txx] 完成 | 验收输出摘要`）

- [TASK-T01] 完成 | 远程仅 refs/heads/main=2299c8b8（6 个提交）；bundle `E:/Backups/paperblade/paperblade-20261031.bundle` 在空仓库中 verify 通过（complete history）；mirror `lfs fetch --all` 305 个对象、`lfs fsck` OK；从 mirror 克隆恢复 331 个 LFS 文件全部为真实内容；SHA256SUMS.txt 306 行；新仓库 34 条 LFS 规则，main 首个提交，dev/demo 已创建

## 失败尝试记录

（记录自动修复的尝试和回退）
