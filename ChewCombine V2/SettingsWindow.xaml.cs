using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ChewCombine_V2
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadAudios();
            LoadBgs();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ================= 休息段音频逻辑 =================

        private void LoadAudios()
        {
            string relaxDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "relax");
            if (!Directory.Exists(relaxDir)) Directory.CreateDirectory(relaxDir);

            var files = Directory.GetFiles(relaxDir)
                .Where(f => f.EndsWith(".mp3") || f.EndsWith(".ogg") || f.EndsWith(".wav"))
                .Select(Path.GetFileName)
                .ToList();

            RestAudioComboBox.ItemsSource = files;

            if (!string.IsNullOrEmpty(MainWindow.SelectedRestAudioPath))
            {
                RestAudioComboBox.SelectedItem = Path.GetFileName(MainWindow.SelectedRestAudioPath);
            }
        }

        private void SelectRestAudio_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音频文件 (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                string relaxDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "relax");
                string safeFileName = Path.GetFileName(openFileDialog.FileName);
                string targetPath = Path.Combine(relaxDir, safeFileName);

                File.Copy(openFileDialog.FileName, targetPath, true); // 导入并保留原名

                LoadAudios();
                RestAudioComboBox.SelectedItem = safeFileName;
                MessageBox.Show($"成功导入并选中音频: {safeFileName} 喵！");
            }
        }

        private void RestAudioComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (RestAudioComboBox.SelectedItem != null)
            {
                string selectedFile = RestAudioComboBox.SelectedItem.ToString();
                string relaxDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "relax");
                string fullPath = Path.Combine(relaxDir, selectedFile);

                RestPathTextBox.Text = fullPath;
                MainWindow.SelectedRestAudioPath = fullPath; // 同步给主程序
            }
        }

        // ================= 背景图逻辑 =================

        private void LoadBgs()
        {
            string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
            if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);

            var files = Directory.GetFiles(imgDir)
                .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
                .Select(Path.GetFileName)
                .ToList();

            BgComboBox.ItemsSource = files;

            if (!string.IsNullOrEmpty(MainWindow.SelectedBgPath))
            {
                BgComboBox.SelectedItem = Path.GetFileName(MainWindow.SelectedBgPath);
            }
        }

        private void SelectBgImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件 (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openFileDialog.ShowDialog() == true)
            {
                string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
                string safeFileName = Path.GetFileName(openFileDialog.FileName);
                string targetPath = Path.Combine(imgDir, safeFileName);

                File.Copy(openFileDialog.FileName, targetPath, true);

                LoadBgs();
                BgComboBox.SelectedItem = safeFileName;
                MessageBox.Show($"成功导入并选中背景图: {safeFileName} 喵！");
            }
        }

        private void BgComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BgComboBox.SelectedItem != null)
            {
                string selectedFile = BgComboBox.SelectedItem.ToString();
                string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
                string fullPath = Path.Combine(imgDir, selectedFile);

                BgPathTextBox.Text = fullPath;
                MainWindow.SelectedBgPath = fullPath; // 同步给主程序
            }
        }
    }
}