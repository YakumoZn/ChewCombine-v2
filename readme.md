# ChewCombine-v2

A universal osu! Dan Map Creator for all modes, featuring an intuitive GUI for streamlined map synthesis.
Osu段位合成器，全模式通用; 可视化GUI界面，更方便的合成段位。

[![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![Framework](https://img.shields.io/badge/framework-.NET%208-blueviolet.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Library](https://img.shields.io/badge/UI-WPF--UI-brightgreen.svg)](https://github.com/lepoco/wpfui)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)


ChewCombine v1：https://github.com/YakumoZn/ChewCombine

---

## 准备工作

1. **运行环境**：请确保你的电脑安装了 **.NET 8.0 Desktop Runtime**。
2. **FFmpeg 依赖**：本程序依赖 FFmpeg 处理音频。请下载 `ffmpeg.exe` 并将其放置在程序的**根目录**或根目录下的 `ffmpeg/` 文件夹中。



## 上手

### 1. 开始
* 打开软件后，点击右侧的 **文件夹📁**，选中你的 `osu!\Songs` 目录。随后点击旁边的刷新按钮加载你所有的谱面文件。
* 找到目标谱面后，点击右侧的 `+` 号，将其加入到左侧的列表中。
点击左侧列表中谱面的 **展开按钮**。需要输入该首曲目需要截取的 **起始位置 (ms)** 和 **结束位置 (ms)**。

### 2. 合成导入
确认无误后，点击左下角的 **合成按钮**。
程序会在后台自动执行音频切割、拼接、谱面偏移重算以及打包。完成后，你会得到一个弹窗提示。
前往程序根目录下的 `Create` 文件夹，你会看到一个新鲜出炉的 `.osz` 文件，直接双击导入 osu! 即可游玩！

### 4. 其他
点击下方的 **设置**：
* audio 留空，默认使用自带的 `bg.png`。
* background 留空，默认使用自带的 `normal_Ocean.mp3`。




### 目录结构规范
```shell
程序根目录/
├── songs/
│ ├── 1/ 
│ ├── 2/
│ │ └── ...
│ └── 3/
├── relax/
│ └── rest.mp3
├── img/
│ └── bg.png
└── Create/
```
