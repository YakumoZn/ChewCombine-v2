using System.ComponentModel;
using System.IO;

namespace ChewCombine_V2.Models
{
    // 实现 INotifyPropertyChanged 是为了以后搜索过滤时界面能自动更新
    public class OsuBeatmap : INotifyPropertyChanged
    {
        //属性，存
        public string FilePath { get; set; } // 完整路径
        public string Title { get; set; }
        public string Creator { get; set; }
        public string Version { get; set; }
        public string AudioFilename { get; set; } = ""; // audio.mp3
        public string DisplaySubtitle => $"{Creator} // {Version}";
        

        private string _startTime = "0";
        public string StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        private string _endTime = "0"; // 这里后面可以根据 .osu 长度初始化
        public string EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(nameof(EndTime)); }
        }

        // 控制UI的状态
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
        }

        // 谱师 // 难度
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}