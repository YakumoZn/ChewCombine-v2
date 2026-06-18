using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel; 
using System.Windows.Controls; 

using ChewCombine_V2.Models;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Text;

namespace ChewCombine_V2
{
    //何意为
    public partial class MainWindow : Window
    {
        private ObservableCollection<OsuBeatmap> _allBeatmaps = new ObservableCollection<OsuBeatmap>();
        private ObservableCollection<OsuBeatmap> _selectedBeatmaps = new ObservableCollection<OsuBeatmap>();
        private string _customSongsPath = "";
        public static string SelectedRestAudioPath = "";
        public static string SelectedBgPath = "";

        public static string CustomTitle = "your dans";
        public static string CustomCreator = "your name";
        public static int CustomMode = 3;       // 默认 3 (osu!mania)
        public static int CustomKeys = 4;       // 默认 4K
        public static double CustomOD = 9.0;    // 默认 OD 9
        public static double CustomHP = 8.0;    // 默认 HP 8

        public MainWindow()
        {
            InitializeComponent();
            RightListBox.ItemsSource = _allBeatmaps;
            LeftListBox.ItemsSource = _selectedBeatmaps;
        }

        // 选文件夹
        private void SelectSongsFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                _customSongsPath = dialog.FolderName;
                MessageBox.Show($"Please click the refresh button \n {_customSongsPath} ");
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_customSongsPath))
            {
                MessageBox.Show("pls click the folder icon to select the Songs path");
                return;
            }

            _allBeatmaps.Clear();
            await Task.Run(() => LoadOsuFiles(_customSongsPath));
        }

        private void LoadOsuFiles(string path)// 加载
        {
            var osuFiles = Directory.GetFiles(path, "*.osu", SearchOption.AllDirectories);
            foreach (var file in osuFiles)
            {
                var beatmap = ParseOsuFile(file);
                // 神秘传值，看不懂但是大受震撼，gemini大人写的
                if (beatmap != null)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        _allBeatmaps.Add(beatmap);
                        BeatmapCountText.Text = $"当前谱面数: {_allBeatmaps.Count}";
                    });
                }
            }
        }

        private OsuBeatmap? ParseOsuFile(string filePath) 
        { // 取所有属性 
            var map = new OsuBeatmap { FilePath = filePath };
            bool inMetadata = false;

            foreach (var line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim();

                // 音频
                if (trimmedLine.StartsWith("AudioFilename:"))
                {
                    map.AudioFilename = trimmedLine.Substring(14).Trim();
                }

                // 别的
                if (trimmedLine == "[Metadata]")
                {
                    inMetadata = true;
                    continue;
                }
                else if (trimmedLine.StartsWith("["))
                {
                    if (inMetadata) break;
                }

                if (inMetadata)
                {
                    if (trimmedLine.StartsWith("Title:"))
                        map.Title = trimmedLine.Substring(6).Trim();
                    else if (trimmedLine.StartsWith("Creator:"))
                        map.Creator = trimmedLine.Substring(8).Trim();
                    else if (trimmedLine.StartsWith("Version:"))
                        map.Version = trimmedLine.Substring(8).Trim();
                }
            }

            // 如果没读到名字，返回 null
            if (string.IsNullOrEmpty(map.Title)) return null;

            return map;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWin = new SettingsWindow();
            settingsWin.Owner = this;
            settingsWin.ShowDialog();
        }

        // 搜索这块
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            ICollectionView view = System.Windows.Data.CollectionViewSource.GetDefaultView(_allBeatmaps);

            if (view != null)
            {
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = (item) =>
                    {
                        var beatmap = item as OsuBeatmap;
                        if (beatmap == null) return false;

                        return (beatmap.Title != null && beatmap.Title.ToLower().Contains(searchText)) ||
                               (beatmap.Creator != null && beatmap.Creator.ToLower().Contains(searchText)) ||
                               (beatmap.Version != null && beatmap.Version.ToLower().Contains(searchText));
                    };
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is OsuBeatmap selectedMap)
            {
                if (!_selectedBeatmaps.Contains(selectedMap))
                {
                    _selectedBeatmaps.Add(selectedMap);
                }
            }
        }

        // 点击左边的叉
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is OsuBeatmap targetMap)
            {
                _selectedBeatmaps.Remove(targetMap);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is OsuBeatmap map)
            {
                map.IsExpanded = !map.IsExpanded;
            }
        }

        // ???? who is [Crz]ChewYakuwo????
        private void GitHubLink_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/YakumoZn/ChewCombine-v2",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        //=======================================================================================
        //生成这块，主程是gemini写的，后面一大块函数都是直接复制的V1逻辑
        private async void CombineButton_Click(object sender, RoutedEventArgs e)
        {
            // 防呆不防傻
            if (_selectedBeatmaps.Count == 0) return;

            var btn = (Wpf.Ui.Controls.Button)sender;

            try
            {
                // 定义
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string songsDir = Path.Combine(baseDir, "songs");
                string relaxDir = Path.Combine(baseDir, "relax");
                string imgDir = Path.Combine(baseDir, "img");
                string createDir = Path.Combine(baseDir, "Create");

                // 初始化文件夹
                if (!Directory.Exists(songsDir)) Directory.CreateDirectory(songsDir);
                if (!Directory.Exists(createDir)) Directory.CreateDirectory(createDir);
                string oldOsu = Path.Combine(createDir, "dan.osu");
                string oldOgg = Path.Combine(createDir, "dan.ogg");
                if (File.Exists(oldOsu)) File.Delete(oldOsu);
                if (File.Exists(oldOgg)) File.Delete(oldOgg);

                // 2. 清理songs文件夹
                foreach (var dir in Directory.GetDirectories(songsDir)) Directory.Delete(dir, true);
                foreach (var file in Directory.GetFiles(songsDir)) File.Delete(file);

                // 同步songs文件
                for (int i = 0; i < _selectedBeatmaps.Count; i++)
                {
                    string folderPath = Path.Combine(songsDir, (i + 1).ToString());
                    Directory.CreateDirectory(folderPath);
                    File.Copy(_selectedBeatmaps[i].FilePath, Path.Combine(folderPath, "map.osu"), true);

                    string sourceAudio = Path.Combine(Path.GetDirectoryName(_selectedBeatmaps[i].FilePath), _selectedBeatmaps[i].AudioFilename);
                    if (File.Exists(sourceAudio))
                        File.Copy(sourceAudio, Path.Combine(folderPath, "audio.mp3"), true);
                }

                await Task.Run(() =>
                {
                    // 准备资源
                    string bgPath = Path.Combine(imgDir, "normal_bg.png"); // 默认
                    if (!string.IsNullOrEmpty(SelectedBgPath) && File.Exists(SelectedBgPath))
                    {
                        bgPath = SelectedBgPath;
                    }

                    string restAudio = Path.Combine(relaxDir, "normal_Ocean.mp3");
                    if (!string.IsNullOrEmpty(SelectedRestAudioPath) && File.Exists(SelectedRestAudioPath))
                    {
                        restAudio = SelectedRestAudioPath;
                    }

                    List<MapInfo> maps = new List<MapInfo>();
                    // 读文件夹
                    for (int i = 0; i < _selectedBeatmaps.Count; i++)
                    {
                        var uiMap = _selectedBeatmaps[i];
                        TryParseTime(uiMap.StartTime, out long sMs);
                        TryParseTime(uiMap.EndTime, out long eMs);

                        maps.Add(new MapInfo
                        {
                            FolderName = (i + 1).ToString(),
                            OsuPath = Path.Combine(songsDir, (i + 1).ToString(), "map.osu"),
                            AudioPath = Path.Combine(songsDir, (i + 1).ToString(), "audio.mp3"),
                            StartMs = sMs,
                            EndMs = eMs
                        });
                    }

                    // 预处理 BPM
                    foreach (var map in maps)
                        (map.r_Length, map.BaseBPM) = GetMapLength(map.OsuPath, map.StartMs, map.EndMs);

                    double masterBPM = maps.OrderByDescending(m => m.r_Length).First().BaseBPM;

                    // 核心循环
                    string tempDir = Path.Combine(Path.GetTempPath(), "DanMerger_" + Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);

                    List<string> audioSegments = new List<string>();
                    long currentOffset = 0;
                    List<TimingPoint> allTimingPoints = new List<TimingPoint>();
                    List<HitObject> allHitObjects = new List<HitObject>();

                    for (int i = 0; i < maps.Count; i++)
                    {
                        var map = maps[i];

                        // 音频裁剪
                        string cropped = Path.Combine(tempDir, $"map_{i}_cropped.wav");
                        CropAudio(map.AudioPath, cropped, map.StartMs, map.EndMs);
                        string faded = Path.Combine(tempDir, $"map_{i}_faded.wav");
                        AddFade(cropped, faded, 1000, 1000);
                        audioSegments.Add(faded);

                        // 谱面解析
                        ParseOsu(map.OsuPath, out var timings, out var hits);

                        // TimingPoints 偏移处理
                        foreach (var tp in timings)
                        {
                            if (tp.Time >= map.StartMs && tp.Time <= map.EndMs)
                            {
                                tp.Time = currentOffset + (tp.Time - map.StartMs);
                                allTimingPoints.Add(tp);
                            }
                        }

                        // 绿线/红线补全逻辑
                        bool hasRed = timings.Any(t => t.Time >= map.StartMs && t.Time <= map.EndMs && t.Uninherited == 1);
                        long firstRedTime = hasRed ? allTimingPoints.Last(t => t.Uninherited == 1).Time : currentOffset;
                        if (!hasRed)
                        {
                            double dBL = timings.FirstOrDefault(t => t.Uninherited == 1)?.BeatLength ?? 500;
                            allTimingPoints.Add(new TimingPoint { Time = currentOffset, BeatLength = dBL, Meter = 4, Uninherited = 1, Volume = 100 });
                            firstRedTime = currentOffset;
                        }
                        allTimingPoints.Add(new TimingPoint { Time = firstRedTime, BeatLength = -100 / (masterBPM / map.BaseBPM), Uninherited = 0, Volume = 100, Meter = 4 });

                        // HitObjects 偏移处理
                        foreach (var hit in hits)
                        {
                            if (hit.StartTime >= map.StartMs && hit.StartTime <= map.EndMs)
                            {
                                hit.StartTime = currentOffset + (hit.StartTime - map.StartMs);
                                if (hit.IsLong) hit.EndTime = currentOffset + (hit.EndTime - map.StartMs);
                                allHitObjects.Add(hit);
                            }
                        }

                        currentOffset += GetAudioDuration(faded);

                        // 休息段处理
                        if (i < maps.Count - 1 && restAudio != null)
                        {
                            long restDur = GetAudioDuration(restAudio);
                            string rCrop = Path.Combine(tempDir, $"rest_{i}_c.wav");
                            CropAudio(restAudio, rCrop, 0, restDur);
                            string rFade = Path.Combine(tempDir, $"rest_{i}_f.wav");
                            AddFade(rCrop, rFade, 1000, 1000);
                            audioSegments.Add(rFade);
                            currentOffset += GetAudioDuration(rFade);
                        }
                    }

                    // 合成最终文件
                    string finalAudio = Path.Combine(createDir, "dan.ogg");
                    ConcatAudio(audioSegments, finalAudio);
                    string finalOsu = Path.Combine(createDir, "dan.osu");
                    GenerateOsu(finalOsu, finalAudio, allTimingPoints, allHitObjects, bgPath);
                    PackToOsz(finalAudio, finalOsu, bgPath, Path.Combine(createDir, GetNextOszNumber(createDir)));

                    try { Directory.Delete(tempDir, true); } catch { }
                });

                MessageBox.Show("Build succeeded！Pls check the Create folder。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Build failed: {ex.Message}");
            }
        }

        static bool TryParseTime(string timeStr, out long ms)
        {
            ms = 0;
            var parts = timeStr.Split(':');
            if (parts.Length != 3) return false;
            if (!int.TryParse(parts[0], out int min)) return false;
            if (!int.TryParse(parts[1], out int sec)) return false;
            if (!int.TryParse(parts[2], out int millis)) return false;
            ms = min * 60 * 1000 + sec * 1000 + millis;
            return true;
        }

        static string GetAudioFromOsu(string osuPath, string folderPath)
        {
            foreach (string line in File.ReadLines(osuPath))
            {
                if (line.StartsWith("AudioFilename:"))
                {
                    string filename = line.Substring("AudioFilename:".Length).Trim();
                    string fullPath = Path.Combine(folderPath, filename);
                    if (File.Exists(fullPath)) return fullPath;
                    return null;
                }
            }
            return null;
        }

        static void RunFFmpeg(string args)
        {
            string genmulu = AppDomain.CurrentDomain.BaseDirectory; //根目录
            string ffmpegPath = Path.Combine(genmulu, "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
            {
                ffmpegPath = Path.Combine(genmulu, "ffmpeg", "ffmpeg.exe");
            }
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using (Process p = Process.Start(psi))
            {
                string err = p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg 错误: {err}");
                }
            }
        }

        static void CropAudio(string input, string output, long startMs, long endMs)
        {
            double startSec = startMs / 1000.0;
            double durationSec = (endMs - startMs) / 1000.0;
            RunFFmpeg($"-ss {startSec} -t {durationSec} -i \"{input}\" -vn -ar 44100 -ac 2 -c:a pcm_s16le \"{output}\"");
        }

        static void AddFade(string input, string output, int fadeInMs, int fadeOutMs)
        {
            double fadeIn = fadeInMs / 1000.0;
            double totalDur = GetAudioDuration(input) / 1000.0;
            double fadeOutStart = totalDur - (fadeOutMs / 1000.0);
            RunFFmpeg($"-i \"{input}\" -vn -af \"afade=t=in:st=0:d={fadeIn},afade=t=out:st={fadeOutStart}:d={fadeOutMs / 1000.0}\" -ar 44100 -ac 2 -c:a pcm_s16le \"{output}\"");
        }

        static long GetAudioDuration(string audioPath)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string ffmpegPath = Path.Combine(baseDir, "ffmpeg.exe");
            if (!File.Exists(ffmpegPath))
            {
                ffmpegPath = Path.Combine(baseDir, "ffmpeg", "ffmpeg.exe");
            }
            if (!File.Exists(ffmpegPath))
            {
                throw new Exception("找不到 ffmpeg.exe");
            }

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{audioPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            using (Process p = Process.Start(psi))
            {
                string output = p.StandardError.ReadToEnd();
                p.WaitForExit();
                var match = Regex.Match(output, @"Duration: (\d{2}):(\d{2}):(\d{2}\.\d+)");
                if (match.Success)
                {
                    int hours = int.Parse(match.Groups[1].Value);
                    int minutes = int.Parse(match.Groups[2].Value);
                    double seconds = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                    return (long)((hours * 3600 + minutes * 60 + seconds) * 1000);
                }
            }
            return 0;
        }

        static void ConcatAudio(List<string> files, string output)
        {
            string listFile = Path.GetTempFileName();
            File.WriteAllLines(listFile, files.Select(f => $"file '{f}'"));
            RunFFmpeg($"-f concat -safe 0 -i \"{listFile}\" -c:a libvorbis -q:a 3 \"{output}\"");
            File.Delete(listFile);
        }

        static void ParseOsu(string osuPath, out List<TimingPoint> timings, out List<HitObject> hits)
        {
            timings = new List<TimingPoint>();
            hits = new List<HitObject>();
            string currentSection = "";

            foreach (string line in File.ReadLines(osuPath))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    currentSection = trimmed;
                    continue;
                }

                if (currentSection == "[TimingPoints]" && !string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("//"))
                { 
                    string[] parts = trimmed.Split(',');
                    if (parts.Length >= 2 && long.TryParse(parts[0], out long time))
                    {
                        timings.Add(new TimingPoint
                        {
                            Time = time,
                            BeatLength = double.Parse(parts[1]),
                            Meter = parts.Length > 2 ? int.Parse(parts[2]) : 4,
                            SampleSet = parts.Length > 3 ? int.Parse(parts[3]) : 0,
                            SampleIndex = parts.Length > 4 ? int.Parse(parts[4]) : 0,
                            Volume = parts.Length > 5 ? int.Parse(parts[5]) : 100,
                            Uninherited = parts.Length > 6 ? int.Parse(parts[6]) : 1,
                            Effects = parts.Length > 7 ? int.Parse(parts[7]) : 0
                        });
                    }
                }
                else if (currentSection == "[HitObjects]" && !string.IsNullOrWhiteSpace(trimmed))
                {
                    string[] parts = trimmed.Split(',');
                    if (parts.Length >= 5)
                    {
                        int x = int.Parse(parts[0]);
                        int y = int.Parse(parts[1]);
                        long start = long.Parse(parts[2]);
                        int type = int.Parse(parts[3]);
                        int hitSound = int.Parse(parts[4]);
                        string extras = "";
                        if (parts.Length >= 6)
                        {
                            extras = parts[5]; 
                        }

                        bool isLong = (type & 128) != 0;
                        long endTime = start;
                        if (isLong && extras.Contains(':'))
                        {
                            long.TryParse(extras.Split(':')[0], out endTime);
                        }
                        hits.Add(new HitObject
                        {
                            X = x,
                            Y = y,
                            StartTime = start,
                            Type = type,
                            HitSound = hitSound,
                            IsLong = isLong,
                            EndTime = endTime,
                            Extras = extras
                        });
                    }
                }
            }
        }
        static (long duration, double bpm) GetMapLength(string osuPath, long startMs, long endMs)
        {
            long duration = endMs - startMs;
            double bpm = 120.0;

            ParseOsu(osuPath, out var timings, out _);
            foreach (var tp in timings)
            {
                if (tp.Time >= startMs && tp.Time <= endMs && tp.Uninherited == 1)
                {
                    bpm = 60000.0 / tp.BeatLength;
                    return (duration, bpm);
                }
            }
            foreach (var tp in timings)
            {
                if (tp.Uninherited == 1)
                {
                    bpm = 60000.0 / tp.BeatLength;
                    return (duration, bpm);
                }
            }
            return (duration, bpm);
        }

        // 纯手工初始化谱面文件, 不嘻嘻(指.osu
        static void GenerateOsu(string outputPath, string audioPath, List<TimingPoint> timings, List<HitObject> hits, string bgPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("osu file format v14");
            sb.AppendLine();
            sb.AppendLine("[General]");
            sb.AppendLine($"AudioFilename: {Path.GetFileName(audioPath)}");
            sb.AppendLine("AudioLeadIn: 0");
            sb.AppendLine("PreviewTime: -1");
            sb.AppendLine("Countdown: 0");
            sb.AppendLine("SampleSet: None");
            sb.AppendLine("StackLeniency: 0.7");
            sb.AppendLine($"Mode: {CustomMode}");
            sb.AppendLine("LetterboxInBreaks: 0");
            sb.AppendLine("SpecialStyle: 0");
            sb.AppendLine("WidescreenStoryboard: 0");
            sb.AppendLine();
            sb.AppendLine("[Editor]");
            sb.AppendLine("DistanceSpacing: 1");
            sb.AppendLine("BeatDivisor: 4");
            sb.AppendLine("GridSize: 8");
            sb.AppendLine("TimelineZoom: 2.5");
            sb.AppendLine();
            sb.AppendLine("[Metadata]");
            sb.AppendLine($"Title:{CustomTitle}");
            sb.AppendLine($"TitleUnicode:{CustomTitle}");
            sb.AppendLine("Artist:V.A");
            sb.AppendLine("ArtistUnicode:V.A");
            sb.AppendLine($"Creator:{CustomCreator}");
            sb.AppendLine("Version:chew");
            sb.AppendLine("Source:");
            sb.AppendLine("Tags:");
            sb.AppendLine("BeatmapID:0");
            sb.AppendLine("BeatmapSetID:0");
            sb.AppendLine();
            sb.AppendLine("[Difficulty]");
            sb.AppendLine($"HPDrainRate:{CustomHP.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}");
            sb.AppendLine($"CircleSize:{CustomKeys}");
            sb.AppendLine($"OverallDifficulty:{CustomOD.ToString("F1", System.Globalization.CultureInfo.InvariantCulture)}");
            sb.AppendLine("ApproachRate:5");
            sb.AppendLine("SliderMultiplier:1.4");
            sb.AppendLine("SliderTickRate:1");
            sb.AppendLine();
            sb.AppendLine("[Events]");
            sb.AppendLine("//Background and Video events");
            sb.AppendLine($"0,0,\"bg.png\",0,0");
            sb.AppendLine("//Break Periods");
            sb.AppendLine("//Storyboard Layer 0 (Background)");
            sb.AppendLine("//Storyboard Layer 1 (Fail)");
            sb.AppendLine("//Storyboard Layer 2 (Pass)");
            sb.AppendLine("//Storyboard Layer 3 (Foreground)");
            sb.AppendLine("//Storyboard Layer 4 (Overlay)");
            sb.AppendLine("//Storyboard Sound Samples");
            sb.AppendLine();
            sb.AppendLine("[TimingPoints]");
            foreach (var tp in timings.OrderBy(t => t.Time))
            {
                sb.AppendLine($"{tp.Time},{tp.BeatLength},{tp.Meter},{tp.SampleSet},{tp.SampleIndex},{tp.Volume},{tp.Uninherited},{tp.Effects}");
            }
            sb.AppendLine();
            sb.AppendLine("[HitObjects]");
            foreach (var hit in hits.OrderBy(h => h.StartTime))
            {
                string extras = hit.Extras;
                if (hit.IsLong)
                    extras = $"{hit.EndTime}:0:0:0:0:";
                
                if (extras == "")
                {
                    sb.AppendLine($"{hit.X},{hit.Y},{hit.StartTime},{hit.Type},{hit.HitSound}");
                }
                else
                {
                    sb.AppendLine($"{hit.X},{hit.Y},{hit.StartTime},{hit.Type},{hit.HitSound},{extras}");
                }
            }
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        static string GetNextOszNumber(string createDir)
        {
            int num = 1;
            while (File.Exists(Path.Combine(createDir, $"{num}.osz")))
                num++;
            return $"{num}.osz";
        }

        static void PackToOsz(string audioFile, string osuFile, string bgFile, string oszPath)
        {
            string tempZipDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempZipDir);
            File.Copy(audioFile, Path.Combine(tempZipDir, Path.GetFileName(audioFile)));
            File.Copy(osuFile, Path.Combine(tempZipDir, Path.GetFileName(osuFile)));
            File.Copy(bgFile, Path.Combine(tempZipDir, Path.GetFileName(bgFile)));
            ZipFile.CreateFromDirectory(tempZipDir, oszPath);
            Directory.Delete(tempZipDir, true);
        }
    }

    class MapInfo
    {
        public string FolderName;
        public string OsuPath;
        public string AudioPath;
        public long StartMs;
        public long EndMs;
        public long r_Length;
        public double BaseBPM;
    }

    class TimingPoint
    {
        public long Time;
        public double BeatLength;
        public int Meter;
        public int SampleSet;
        public int SampleIndex;
        public int Volume;
        public int Uninherited;
        public int Effects;
    }

    class HitObject
    {
        public int X, Y;
        public long StartTime;
        public int Type;
        public int HitSound;
        public bool IsLong;
        public long EndTime;
        public string Extras;
    }
}