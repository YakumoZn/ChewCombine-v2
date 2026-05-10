using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace ChewCombine_V2
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();//关
        }

        // 选音频
        private void SelectRestAudio_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "音频文件 (*.mp3;*.ogg;*.wav)|*.mp3;*.ogg;*.wav";
            if (openFileDialog.ShowDialog() == true)
            {
                string relaxDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "relax");
                if (!Directory.Exists(relaxDir)) Directory.CreateDirectory(relaxDir);

                // 懒 替换为 AL.mp3覆盖
                string targetPath = Path.Combine(relaxDir, "AL.mp3");
                File.Copy(openFileDialog.FileName, targetPath, true);

                RestPathTextBox.Text = openFileDialog.FileName;
                MessageBox.Show("rest audio has been replaced, Nya!！");
            }
        }

        // 选择背景图
        private void SelectBgImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图片文件 (*.png;*.jpg)|*.png;*.jpg";
            if (openFileDialog.ShowDialog() == true)
            {
                string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
                if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);

                // 自动替换为 bg.png
                string targetPath = Path.Combine(imgDir, "bg.png");
                File.Copy(openFileDialog.FileName, targetPath, true);

                BgPathTextBox.Text = openFileDialog.FileName;
                MessageBox.Show("bg has been replaced, Nya!！");
            }
        }

    }
    
}