using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TRPGLogArrangeTool.resource;
using TRPGLogArrangeTool.ViewModel;

namespace TRPGLogArrangeTool
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        #region const
        private const string CONST_ERROR = "エラー";
        private const string CONST_WARNING = "警告";
        private const string CONST_EVENT = "EVENT";
        private readonly string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
        #endregion

        public MainViewModel viewModel;

        /// <summary>
        /// 詳細Columnサイズ
        /// </summary>
        private GridLength DetailColumnWidth;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
            DetailColumnWidth = DetailColumn.Width;
            DetailColumn.Width = new GridLength(0);
            Width = 600;

        }

        #region ヘッダー部
        /// <summary>
        /// 詳細解析チェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxDetail_Checked(object sender, RoutedEventArgs e)
        {
            DetailColumn.Width = DetailColumnWidth;
            // Windowサイズを広げる
            DetailGrid.Visibility = Visibility.Visible;
            Width = 1000;
        }

        /// <summary>
        /// 詳細解析チェック解除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            // 右側を非表示（幅0）
            DetailColumn.Width = new GridLength(0);
            // Windowサイズを縮める
            Width = 600;
            DetailGrid.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 対象ファイルドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFileAddress_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
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
        /// 対象ファイルドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFileAddress_Drop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] dropFiles))
            {
                return;
            }
            textBoxFileAddress.Text = dropFiles[0];
            textBoxFileAddress.Background = new SolidColorBrush(Colors.White);
            buttonFileRead.IsEnabled = true;
            buttonReWriteStart.IsEnabled = false;
        }
        /// <summary>
        /// 対象ファイル選択ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonFileSelected_Click(object sender, RoutedEventArgs e)
        {
            TargetSelect();
        }
        /// <summary>
        /// 対象ファイルテキストボックスダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxFileAddress_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TargetSelect();
        }
        /// <summary>
        /// 対象ファイル設定処理
        /// </summary>
        private void TargetSelect()
        {

            string filterCode;

            if (radioButtonCC.IsChecked.Value)
            {
                filterCode = "HTMLファイル (*.html;*.htm)|*.html;*.htm|すべてのファイル (*.*)|*.*";
            }
            else
            {
                filterCode = "ZIPファイル (*.zip)|*.zip|すべてのファイル (*.*)|*.*";
            }

            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog
            {
                //[ファイルの種類]に表示される選択肢を指定する
                Filter = filterCode,
                //[ファイルの種類]ではじめに選択されるものを指定する
                FilterIndex = 1,
                //タイトルを設定する
                Title = "開くファイルを選択してください",
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                RestoreDirectory = true,
                //存在しないファイルの名前が指定されたとき警告を表示する
                CheckFileExists = true,
                //存在しないパスが指定されたとき警告を表示する
                CheckPathExists = true,
                //複数項目選択の可否
                Multiselect = false
            };
            //ダイアログを表示する
            if (ofd.ShowDialog() == true)
            {
                textBoxFileAddress.Text = ofd.FileName;
                textBoxFileAddress.Background = new SolidColorBrush(Colors.White);
                buttonFileRead.IsEnabled = true;
                buttonReWriteStart.IsEnabled = false;
            }
        }
        /// <summary>
        /// ファイル読み込み処理
        /// </summary>
        /// <prm name="sender"></prm>
        /// <prm name="e"></prm>
        private void ButtonFileRead_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(textBoxFileAddress.Text))
            {
                MessageBox.Show("ファイルを指定してください", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!File.Exists(textBoxFileAddress.Text))
            {
                MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // 小文字に統一
            string extension = System.IO.Path.GetExtension(textBoxFileAddress.Text).ToLower();
            if (radioButtonCC.IsChecked.Value)
            {
                if (extension == ".html" || extension == ".htm")
                {
                    ZipAnalyzeText.Visibility = Visibility.Hidden;
                    CharacterImageList.Visibility = Visibility.Visible;
                    if (viewModel.HtmlAnalyze(textBoxFileAddress.Text))
                    {
                        buttonReWriteStart.IsEnabled = true;
                    }
                    else
                    {
                        buttonReWriteStart.IsEnabled = false;
                    }
                }
                else
                {
                    MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else if (radioButtonUD.IsChecked.Value)
            {
                if (extension == ".zip")
                {
                    CharacterImageList.Visibility = Visibility.Hidden;
                    ZipAnalyzeText.Visibility = Visibility.Visible;
                    if (viewModel.ZipAnalyze(textBoxFileAddress.Text, checkBoxDetail.IsChecked.Value, checkStandUse.IsChecked.Value))
                    {
                        buttonReWriteStart.IsEnabled = true;
                    }
                    else
                    {
                        buttonReWriteStart.IsEnabled = false;
                    }
                }
                else
                {
                    MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("ラジオボタンが選択されていません", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        /// <summary>
        /// 出力ボタン押下
        /// </summary>
        /// <prm name="sender"></prm>
        /// <prm name="e"></prm>
        private void ButtonReWriteStart_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ConvertWrite();
        }
        #endregion

        #region 名称一覧部
        /// <summary>
        /// 画像追加処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button))
            {
                return;
            }
            if (!(button.DataContext is ChatName chatName))
            {
                return;
            }
            // ファイル選択（複数枚選択可能）
            var dlg = new OpenFileDialog
            {
                Filter = "画像ファイル|*.png;*.jpg;*.jpeg;",
                RestoreDirectory = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = true
            };
            if (dlg.ShowDialog() != true)
            {
                return;
            }

            foreach (var file in dlg.FileNames)
            {
                viewModel.AddImageToCharacter(file, chatName);
            }
        }
        /// <summary>
        /// ドラッグアンドドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            // カーソル位置を取得
            if (!(sender is ListBox listBox))
            {
                return;
            }
            Point position = e.GetPosition(listBox);
            // カーソル下の要素をHitTest
            var element = listBox.InputHitTest(position) as DependencyObject;

            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            if (element is ListBoxItem item)
            {
                // カーソル下のアイテムを選択状態にする
                listBox.SelectedItem = item.DataContext;
            }

            // 受け入れ可能ファイル形式をチェック
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string ext = System.IO.Path.GetExtension(files[0]).ToLowerInvariant();

                    if (allowedExtensions.Contains(ext))
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }
            }

            // 受け入れ不可の場合
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
        /// <summary>
        /// ドラッグアンドドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_Drop(object sender, DragEventArgs e)
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

            var listBox = sender as ListBox;
            if (listBox?.SelectedItem is ChatName item)
            {
                foreach (var file in files)
                {
                    viewModel.AddImageToCharacter(file, item);
                }
            }
        }
        /// <summary>
        /// キャラクター画像差し替え処理
        /// </summary>
        private void CharacterImageDefaultChange()
        {
            // 選択されている ChatName を取得
            if (viewModel.SelectedName == null)
            {
                return;
            }
            try
            {
                // SelectImageWindow を開く
                var window = new SelectImageWindow(viewModel.SelectedName.ImageKeys)
                {
                    Owner = this // 親ウィンドウ設定
                };
                IsHitTestVisible = false;
                OverlayPanel.Visibility = Visibility.Visible;
                if (window.ShowDialog() == true)
                {
                    string selectedKey = window.SelectedKey;
                    if (!string.IsNullOrEmpty(selectedKey))
                    {
                        // ChatNameのDefaultImageKeyを更新
                        viewModel.SelectedName.DefaultImageKey = selectedKey;

                        // 同じNameを持つChatMessageのImageKeyも更新
                        foreach (var msg in viewModel.ChatMessageList.Where(m => m.Name == viewModel.SelectedName.Name))
                        {
                            msg.ImageKey = selectedKey;
                        }
                    }
                }
            }
            finally
            {
                OverlayPanel.Visibility = Visibility.Collapsed;
                IsHitTestVisible = true;
            }
        }
        #region キャラクター一覧右クリックメニュー
        private void CharacterImageList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (viewModel.SelectedName != null)
            {
                ChangeDefaultImageContext.IsEnabled = true;
                if (viewModel.SelectedName.ImageKeys.Count > 0)
                {
                    DeleteAllImageContext.IsEnabled = true;
                }
                else
                {
                    DeleteAllImageContext.IsEnabled = false;
                }
            }
            else
            {
                ChangeDefaultImageContext.IsEnabled = false;
                DeleteAllImageContext.IsEnabled = false;
            }
        }

        private void ChangeDefaultImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedName != null)
            {
                CharacterImageDefaultChange();
            }
        }

        private void DeleteAllImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedName != null || viewModel.ChatNameList.Where(x => x.Name == viewModel.SelectedName.Name).Count() > 0)
            {
                if (MessageBox.Show("このキャラクターのすべての画像を削除します、本当によろしいでしょうか？", CONST_WARNING, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    // このキャラのキーをバックアップ
                    var keysToRemove = viewModel.SelectedName.ImageKeys.ToList();

                    // ChatName の画像情報を削除
                    viewModel.SelectedName.ImageKeys.Clear();
                    viewModel.SelectedName.DefaultImageKey = null;

                    // ChatMessageList の画像を削除
                    foreach (var msg in viewModel.ChatMessageList.Where(m => m.Name == viewModel.SelectedName.Name))
                    {
                        msg.ImageKey = null;
                    }

                    // キャッシュ削除対象を精査
                    foreach (var key in keysToRemove)
                    {
                        bool stillUsed =
                            viewModel.ChatNameList.Any(cn => cn.ImageKeys.Contains(key) || cn.DefaultImageKey == key) ||
                            viewModel.ChatMessageList.Any(cm => cm.ImageKey == key);

                        if (!stillUsed)
                        {
                            // どこでも使われていなければキャッシュから削除
                            ImageCache.Remove(key);
                        }
                    }
                }
            }
        }
        #endregion

        #endregion

        #region 詳細解析部
        /// <summary>
        /// イベント画像欄追加ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddEventImage_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventImage();
            }
        }
        /// <summary>
        /// イベントキャラクター画像欄追加ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddCharacterImage_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventCharacterImage();
            }
        }
        /// <summary>
        /// 追加欄削除ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDeleteImage_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.DeleteMessage(AllDeleteCheck.IsChecked.Value);
            }
        }
        /// <summary>
        /// メッセージ部ダブルクリック対応
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailMessageList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ChangeImageMessage();
        }
        /// <summary>
        /// 画像差し替え処理
        /// </summary>
        private void ChangeImageMessage()
        {
            if (viewModel.SelectedMessage == null)
            {
                return;
            }
            if (viewModel.SelectedMessage.Area != HtmlResource.StringMainJP && viewModel.SelectedMessage.Area != CONST_EVENT)
            {
                return;
            }
            var chatName = viewModel.ChatNameList.FirstOrDefault(c => c.Name == viewModel.SelectedMessage.Name);
            if (chatName != null)
            {
                try
                {
                    var win = new SelectImageWindow(chatName.ImageKeys)
                    {
                        Owner = this
                    };
                    IsHitTestVisible = false;
                    OverlayPanel.Visibility = Visibility.Visible;
                    if (win.ShowDialog() == true)
                    {
                        if (!viewModel.SelectedMessage.IsAddedMessage)
                        {
                            if (OverrideCheck.IsChecked.Value)
                            {
                                int index = viewModel.ChatMessageList.IndexOf(viewModel.SelectedMessage);
                                for (int i = 0; i < viewModel.ChatMessageList.Count; i++)
                                {
                                    if (i >= index && viewModel.ChatMessageList[i].Name == viewModel.SelectedMessage.Name)
                                    {
                                        viewModel.ChatMessageList[i].ImageKey = win.SelectedKey;
                                    }
                                }
                            }
                            else
                            {
                                viewModel.SelectedMessage.ImageKey = win.SelectedKey;
                            }
                        }
                        else
                        {
                            viewModel.SelectedMessage.ImageKey = win.SelectedKey;
                        }
                    }
                }
                finally
                {
                    OverlayPanel.Visibility = Visibility.Collapsed;
                    IsHitTestVisible = true;
                }
            }
        }
        /// <summary>
        /// ドラッグアンドドロップ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DetailMessageList_PreviewDragOver(object sender, DragEventArgs e)
        {
            // カーソル位置を取得
            if (!(sender is ListBox listBox))
            {
                return;
            }
            Point position = e.GetPosition(listBox);

            // カーソル下の要素をHitTest
            var element = listBox.InputHitTest(position) as DependencyObject;

            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            if (element is ListBoxItem item)
            {
                // カーソル下のアイテムを選択状態にする
                listBox.SelectedItem = item.DataContext;
            }

            // 受け入れ可能ファイル形式をチェック
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                {
                    string ext = System.IO.Path.GetExtension(files[0]).ToLowerInvariant();

                    if (allowedExtensions.Contains(ext))
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }
            }

            // 受け入れ不可の場合
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void DetailMessageList_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0 || files.Length > 1)
            {
                return;
            }

            var listBox = sender as ListBox;
            if (listBox?.SelectedItem is ChatMessage selectMessage)
            {
                var chatName = viewModel.ChatNameList.FirstOrDefault(x => x.Name == selectMessage.Name);

                foreach (var file in files)
                {
                    int index = viewModel.ChatMessageList.IndexOf(viewModel.SelectedMessage);
                    int overrideIndex = OverrideCheck.IsChecked.Value ? index : -1;
                    viewModel.AddImageToCharacter(file, chatName, overrideIndex);
                }
            }
        }

        #region 詳細解析右クリックメニュー
        private void DetailMessageList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                AddBeforeEventImageContext.IsEnabled = true;
                AddBeforeEventCharacterImageContext.IsEnabled = true;
                AddAfterEventImageContext.IsEnabled = true;
                AddAfterEventCharacterImageContext.IsEnabled = true;

                if (viewModel.SelectedMessage.Area == HtmlResource.StringMainJP || viewModel.SelectedMessage.Area == CONST_EVENT)
                {
                    ChangeImageContext.IsEnabled = true;
                    if (!string.IsNullOrEmpty(viewModel.SelectedMessage.ImageKey))
                    {
                        ShowImageContext.IsEnabled = true;
                    }
                    else
                    {
                        ShowImageContext.IsEnabled = false;
                    }
                }
                else
                {
                    ChangeImageContext.IsEnabled = false;
                    ShowImageContext.IsEnabled = false;
                }

                if (viewModel.SelectedMessage.IsAddedMessage || AllDeleteCheck.IsChecked.Value)
                {
                    DeleteMessageContext.IsEnabled = true;
                }
                else
                {
                    DeleteMessageContext.IsEnabled = false;
                }
            }
            else
            {
                ChangeImageContext.IsEnabled = false;
                ShowImageContext.IsEnabled = false;
                AddBeforeEventImageContext.IsEnabled = false;
                AddBeforeEventCharacterImageContext.IsEnabled = false;
                AddAfterEventImageContext.IsEnabled = false;
                AddAfterEventCharacterImageContext.IsEnabled = false;
                DeleteMessageContext.IsEnabled = false;
            }
        }
        /// <summary>
        /// 画像全削除可能モードチェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllDeleteCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                DeleteMessageContext.IsEnabled = true;
            }
        }

        /// <summary>
        /// 画像全削除可能モードアンチェック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllDeleteCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                DeleteMessageContext.IsEnabled = false;
            }
        }

        private void ChangeImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                ChangeImageMessage();
            }
        }
        private void AddBeforeEventImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventImage(false);
            }
        }
        private void AddAfterEventImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventImage(true);
            }
        }
        private void AddBeforeEventCharacterImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventCharacterImage(false);
            }
        }

        private void AddAfterEventCharacterImageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.InsertEventCharacterImage(true);
            }
        }
        private void DeleteMessageContext_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SelectedMessage != null)
            {
                viewModel.DeleteMessage(AllDeleteCheck.IsChecked.Value);
            }
        }

        private void ShowImageContext_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(viewModel.SelectedMessage.ImageKey))
            {
                // キャッシュから画像を取得
                var imageSource = ImageCache.GetImageSource(viewModel.SelectedMessage.ImageKey);
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
        private async void menuUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            await MainViewModel.CheckForUpdatesAsync();
        }
        #endregion

        #endregion

    }
}