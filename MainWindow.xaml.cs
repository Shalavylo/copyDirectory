using System;
using System.IO;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _03_homework
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Виберіть вихідну директорію"
            };

            if (dialog.ShowDialog() == true)
            {
                txtSource.Text = dialog.FolderName;
            }
        }

        private void SelectDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Виберіть цільову директорію"
            };

            if (dialog.ShowDialog() == true)
            {
                txtDestination.Text = dialog.FolderName;
            }
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            string sourceDir = txtSource.Text.Trim();
            string destDir = txtDestination.Text.Trim();

            if (!Directory.Exists(sourceDir))
            {
                MessageBox.Show("Вихідна директорія не існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(destDir))
            {
                MessageBox.Show("Виберіть цільову директорію!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ToggleControls(false);
            progressBar.Value = 0;
            txtProgress.Text = "Прогрес: 0%";
            txtStatus.Text = "Підрахунок файлів...";

            try
            {
                string[] allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                int totalFiles = allFiles.Length;

                if (totalFiles == 0)
                {
                    MessageBox.Show("Вихідна директорія порожня!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ToggleControls(true);
                    return;
                }

                txtStatus.Text = "Копіювання...";

                int[] copiedFilesWrapper = new int[1];
                await Task.Run(() =>
                {
                    CopyDirectoryRecursive(sourceDir, destDir, totalFiles, copiedFilesWrapper);
                });

                txtStatus.Text = "Копіювання завершено успішно!";
                MessageBox.Show("Копіювання успішно завершено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Помилка копіювання.";
            }
            finally
            {
                ToggleControls(true);
            }
        }

        private void CopyDirectoryRecursive(string currentSource, string currentDest, int totalFiles, int[] copiedFilesWrapper)
        {
            Directory.CreateDirectory(currentDest);

            foreach (string file in Directory.GetFiles(currentSource))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string destFile = System.IO.Path.Combine(currentDest, fileName);

                File.Copy(file, destFile, true);

                copiedFilesWrapper[0]++;
                int copiedFiles = copiedFilesWrapper[0];

                int percent = (int)((double)copiedFiles / totalFiles * 100);

                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = percent;
                    txtProgress.Text = $"Прогрес: {percent}% ({copiedFiles}/{totalFiles})";
                    txtStatus.Text = $"Копіюється: {fileName}";
                });
            }

            foreach (string subDir in Directory.GetDirectories(currentSource))
            {
                string subDirName = System.IO.Path.GetFileName(subDir);
                string destSubDir = System.IO.Path.Combine(currentDest, subDirName);
                CopyDirectoryRecursive(subDir, destSubDir, totalFiles, copiedFilesWrapper);
            }
        }

        private void ToggleControls(bool enabled)
        {
            btnStart.IsEnabled = enabled;
            txtSource.IsEnabled = enabled;
            txtDestination.IsEnabled = enabled;
        }
    }
}