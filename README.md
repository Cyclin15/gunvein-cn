# Gunvein 简体中文汉化补丁

[GUNVEIN](https://store.steampowered.com/app/2025840/GUNVEIN/)（Steam，AppID 2025840）的简体中文民间汉化补丁。

## 下载安装

1. 到本仓库的 **[Releases](../../releases)** 页面，下载最新的 `Gunvein简体中文汉化补丁.zip`
2. 在 Steam 中右键 GUNVEIN → 管理 → 浏览本地文件，打开游戏目录
   （默认为 `Steam\steamapps\common\Gunvein`）
3. 把压缩包里的 `data.win` 解压到游戏目录，覆盖原文件
4. 启动游戏，完成

**卸载**：Steam 中右键 GUNVEIN → 属性 → 已安装文件 → 验证游戏文件的完整性，即可恢复英文原版。

> 本补丁为民间作品，与游戏开发商无关。请先在 Steam 购买并安装游戏后再使用本补丁。

## 汉化内容

- 主菜单、设置、排行榜、制作名单等全部界面文字
- 模式说明（街机 / 自由游玩 / 肉鸽改编 / 练习 / 任务等）
- 关卡、任务、成就条件名称
- 游戏内教学（新手上路、弹幕基础）与 TIP 提示
- 结算画面（得分、金星、续关、奖牌等）

术语约定：Fragment Meter → 碎片槽、Gold Medal → 金牌、streaming → 引弹、
微躲 / 宏躲、温和 / 激烈 / 专家、肉鸽改编、续关、检查点 等。

## 技术方案

游戏为 GameMaker 引擎（data.win，bytecode 17）。本补丁：

1. **字符串翻译**：直接替换 data.win 中 STRG 字符串表里的 408 条显示文本
2. **中文字体**：游戏 UI 文字全部通过 `font_add_sprite_ext()` 用精灵字体渲染，
   不含 CJK 字形。补丁新建 4 个内置字体资源（fnt_cn_20 / 24 / 48 / 72），
   并替换菜单字体 fnt_inteptor 的字形与纹理，再把 `boghog_functions` 中
   4 行字体初始化代码重编译为直接引用新字体资源
3. **字体**：使用开源像素字体 [Fusion Pixel](https://github.com/TakWolf/fusion-pixel-font)
   （OFL 协议），以 12px/10px 基准整数倍放大（×2/×4/×6），保持像素完美
4. 字符集覆盖全部译文用字 + ASCII + Latin-1 + 希腊 + 西里尔 + 假名 + CJK 标点 +
   全角符号，Steam 好友名也能正常显示

### 从源码重建

```
pipeline: dump 字符串 → translations.py 翻译 → gen_fonts.py 生成图集
        → gen_tsv.py 转中间格式 → patch_data.csx 注入（UndertaleModCli）→ make_zip.py 打包
```

需要 [UndertaleModCli](https://github.com/UnderminersTeam/UndertaleModTool)（本仓库不含）。
注意：脚本中的路径为开发机的绝对路径（`D:\GunveinCN\...`），复现时需自行调整。

## 目录结构

```
translations.py        翻译表（英文原文 → 中文，键须与 STRG 完全一致）
scripts/               注入与生成脚本
fonts/                 生成的字体图集（PNG）+ 字形度量（JSON/TSV）
assets/fonts/          Fusion Pixel 字体文件（OFL 开源协议）
dump/translations.json 翻译表导出（TSV 上游）
```

## 许可

- 翻译与脚本部分：随意使用，注明出处即可
- 字体：[Fusion Pixel](https://github.com/TakWolf/fusion-pixel-font)，SIL OFL 1.1
  （见 `assets/fonts/OFL.txt`）
- GUNVEIN 游戏本体及 data.win 内容版权归原开发商所有，本仓库不包含游戏本体
