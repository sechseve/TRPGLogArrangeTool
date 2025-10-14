using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using TRPGLogArrangeTool.resource;
using TRPGLogArrangeTool.ViewModel;

namespace TRPGLogArrangeTool
{
    public partial class SelectImageWindow : Window
    {
        #region
        private const string CONST_ERROR = "エラー";
        private readonly string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
        #endregion

        public ObservableCollection<string> ImageKeys { get; } = new ObservableCollection<string>();

        public string SelectedKey { get; private set; }

        public SelectImageWindow(ObservableCollection<string> keys)
        {
            InitializeComponent();
            ImageKeys = keys;
            DataContext = this;
        }
        /// <summary>
        /// 画像選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((sender as System.Windows.Controls.Image)?.DataContext is string key)
            {
                SelectedKey = key;
                DialogResult = true;
                Close();
            }
        }
        /// <summary>
        /// 画像拡大表示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ((sender as System.Windows.Controls.Image)?.DataContext is string key)
            {
                // キャッシュから画像を取得
                var imageSource = ImageCache.GetImageSource(key);
                if (imageSource != null)
                {
                    var preview = new ImagePreviewWindow(imageSource)
                    {
                        Owner = this
                    };
                    try
                    {
                        IsHitTestVisible = false;
                        OverlayPanel.Visibility = Visibility.Visible;
                        preview.ShowDialog();

                    }
                    finally
                    {
                        OverlayPanel.Visibility = Visibility.Collapsed;
                        IsHitTestVisible = true;
                    }

                }
            }
        }
        /// <summary>
        /// ドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }
        /// <summary>
        /// ドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0)
            {
                return;
            }

            List<string> errorPath = new List<string>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    continue;
                }


                // キャッシュに登録してキーを取得
                var bmp = ImageCache.GetOrAddFromFile(file, out string key);
                if (bmp != null && !ImageKeys.Contains(key))
                {
                    ImageKeys.Add(key);
                }
                else if (bmp == null)
                {
                    errorPath.Add(Path.GetFileName(file));
                }
            }
            if (errorPath.Count > 0)
            {
                MainViewModel.ImageAddErrorMessage(errorPath);
            }

        }
        /// <summary>
        /// 画像追加メニュー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddImageContext_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "画像を選択してください",
                Filter = "画像ファイル (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
                List<string> errorPath = new List<string>();

                foreach (var file in dlg.FileNames)
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        continue;
                    }

                    // キャッシュに登録してキーを取得
                    var bmp = ImageCache.GetOrAddFromFile(file, out string key);
                    if (bmp != null && !ImageKeys.Contains(key))
                    {
                        ImageKeys.Add(key);
                    }
                    else if(bmp == null)
                    {
                        errorPath.Add(Path.GetFileName(file));
                    }
                }

                if (errorPath.Count > 0)
                {
                    MainViewModel.ImageAddErrorMessage(errorPath);
                }
            }
        }
        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }

}
