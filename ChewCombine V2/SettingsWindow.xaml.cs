using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChewCombine_V2
{
    public partial class SettingsWindow : Window
    {
        private bool _isInitializing = true;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadAudios();
            LoadBgs();
            LoadCustomSettings();
            _isInitializing = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void LoadCustomSettings()
        {
            TitleTextBox.Text = MainWindow.CustomTitle;
            CreatorTextBox.Text = MainWindow.CustomCreator;

            foreach (ComboBoxItem item in ModeComboBox.Items)
            {
                if (item.Tag != null && item.Tag.ToString() == MainWindow.CustomMode.ToString())
                {
                    ModeComboBox.SelectedItem = item;
                    break;
                }
            }

            KeysSlider.Value = MainWindow.CustomKeys;
            KeysValueText.Text = $"{MainWindow.CustomKeys}K";

            OdSlider.Value = MainWindow.CustomOD;
            OdValueText.Text = MainWindow.CustomOD.ToString("F1");

            HpSlider.Value = MainWindow.CustomHP;
            HpValueText.Text = MainWindow.CustomHP.ToString("F1");

            ToggleKeysPanel(MainWindow.CustomMode == 3);
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing) return;
            MainWindow.CustomTitle = TitleTextBox.Text;
        }

        private void CreatorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitializing) return;
            MainWindow.CustomCreator = CreatorTextBox.Text;
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                if (int.TryParse(selectedItem.Tag.ToString(), out int mode))
                {
                    if (!_isInitializing) MainWindow.CustomMode = mode;
                    ToggleKeysPanel(mode == 3);
                }
            }
        }

        private void ToggleKeysPanel(bool isMania)
        {
            if (KeysSettingsPanel != null)
            {
                KeysSettingsPanel.Visibility = isMania ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void KeysSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)Math.Round(e.NewValue);
            if (KeysValueText != null) KeysValueText.Text = $"{val}K";
            if (!_isInitializing) MainWindow.CustomKeys = val;
        }

        private void OdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (OdValueText != null) OdValueText.Text = e.NewValue.ToString("F1");
            if (!_isInitializing) MainWindow.CustomOD = Math.Round(e.NewValue, 1);
        }

        private void HpSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (HpValueText != null) HpValueText.Text = e.NewValue.ToString("F1");
            if (!_isInitializing) MainWindow.CustomHP = Math.Round(e.NewValue, 1);
        }

        // ================= 休息段音频 =================

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

                File.Copy(openFileDialog.FileName, targetPath, true);

                LoadAudios();
                RestAudioComboBox.SelectedItem = safeFileName;
                MessageBox.Show($"成功导入并选中音频: {safeFileName} 喵！");
            }
        }

        private void RestAudioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RestAudioComboBox.SelectedItem != null)
            {
                string selectedFile = RestAudioComboBox.SelectedItem.ToString();
                string relaxDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "relax");
                string fullPath = Path.Combine(relaxDir, selectedFile);

                RestPathTextBox.Text = fullPath;
                MainWindow.SelectedRestAudioPath = fullPath;
            }
        }

        // ================= 背景图 =================

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

        private void BgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BgComboBox.SelectedItem != null)
            {
                string selectedFile = BgComboBox.SelectedItem.ToString();
                string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
                string fullPath = Path.Combine(imgDir, selectedFile);

                BgPathTextBox.Text = fullPath;
                MainWindow.SelectedBgPath = fullPath;
            }
        }
    }
}