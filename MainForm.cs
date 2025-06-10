using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using TRPGLogArrangeTool.resource;


namespace TRPGLogArrangeTool
{
    public partial class MainForm : Form
    {

        #region const
        private const string CONST_ERROR = "エラー";
        private const string CONST_INFOMATION = "情報";
        private const string CONST_WARNING = "警告";

        private const string NAME_LABEL = "NameLabel";
        private const string COLOR_SELECTED_BUTTON = "ColorSelectedButton";
        private const string COLOR_PALET_TEXT = "カラーパレットから選択";
        private const string COLOR_SELECTED_BOX = "ColorSelectedBox";
        private const string CHARACTER_IMAGE_BUTTON = "CharacterImageButton";
        private const string CHARACTER_IMAGE = "CharacterImage";
        private const string COLOR_FFFFFF = "FFFFFF";

        #endregion

        #region クラス定義
        /// <summary>
        /// 画像データクラス
        /// </summary>
        public class ImageList
        {
            public string ImageName { get; set; }
            public string ImageBase64 { get; set; }
        }
        /// <summary>
        /// 名前確認用クラス
        /// </summary>
        public class NameDataClass
        {
            public string Name { get; set; }
            public string Color { get; set; }
        }
        /// <summary>
        /// チャットメッセージ
        /// </summary>
        private class ChatMessage
        {
            public string Area { get; set; }
            public string Name { get; set; }
            public long TimeStamp { get; set; }
            public string Color { get; set; }
            public string ImageFileName { get; set; }
            public string Text { get; set; }
        }
        #endregion

        #region Control
        /// <summary>
        /// Colorダイアログ用クラス
        /// </summary>
        public class ColorDialogSelected : Button
        {
            public TextBox TargetTextBoxID { get; set; }

            public void EventMaking()
            {
                Click += new EventHandler(DoClickEvent);
            }

            // ボタンへのイベントを解除する
            public void EventSuspend()
            {
                Click -= new EventHandler(DoClickEvent);
            }

            // クリックイベントの実体(参照するリストボックスに文言テキストを追加)
            public void DoClickEvent(object sender, EventArgs e)
            {
                //ColorDialogクラスのインスタンスを作成
                ColorDialog cd = new ColorDialog
                {
                    //色の作成部分を表示不可にする
                    //デフォルトがTrueのため必要はない
                    AllowFullOpen = false,
                    //純色だけに制限しない
                    //デフォルトがFalseのため必要はない
                    SolidColorOnly = false
                };

                string targetData;
                //ダイアログを表示する
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    //選択された色の取得
                    targetData = RGBChange(cd.Color);
                }
                else
                {
                    targetData = "#" + COLOR_FFFFFF;
                }
                TargetTextBoxID.Text = targetData;
            }
        }
        /// <summary>
        /// 画像選択用クラス
        /// </summary>
        public class ImageSelected : Button
        {
            public PictureBox TargetImageBoxID { get; set; }
            public string OriginalBitMap { get; set; }
            public void EventMaking()
            {
                Click += new EventHandler(DoClickEvent);
            }

            // ボタンへのイベントを解除する
            public void EventSuspend()
            {
                Click -= new EventHandler(DoClickEvent);
            }

            // クリックイベントの実体(参照するリストボックスに文言テキストを追加)
            public void DoClickEvent(object sender, EventArgs e)
            {
                //OpenFileDialogクラスのインスタンスを作成
                OpenFileDialog ofd = new OpenFileDialog
                {
                    //[ファイルの種類]に表示される選択肢を指定する
                    Filter = "画像ファイル (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    //[ファイルの種類]ではじめに選択されるものを指定する
                    //2番目の「すべてのファイル」が選択されているようにする
                    FilterIndex = 1,
                    //タイトルを設定する
                    Title = "開くファイルを選択してください",
                    //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                    RestoreDirectory = true,
                    //存在しないファイルの名前が指定されたとき警告を表示する
                    CheckFileExists = true,
                    //存在しないパスが指定されたとき警告を表示する
                    CheckPathExists = true
                };

                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (Bitmap image = ImageFileOpen(ofd.FileName))
                    {
                        if (image == null)
                        {
                            return;
                        }
                        OriginalBitMap = BitmapToBase64StringAuto(image);

                        //this.TargetImageBoxID.Image = image;
                        //縮小サイズの計算（最大サイズに合わせて縮小）
                        double scale_x = ((double)image.Width / 100.0);
                        double scale_y = ((double)image.Height / 100.0);
                        double scale = (scale_x > scale_y) ? scale_x : scale_y;

                        //リサイズ画像の作成
                        Bitmap bmpResize = new Bitmap(image, (int)(image.Width / scale), (int)(image.Height / scale));
                        TargetImageBoxID.Image = bmpResize;
                    }
                }
                ofd.Dispose();

            }
        }

        #endregion

        #region Field
        /// <summary>
        /// 書き出し画像情報
        /// </summary>
        private List<ImageList> ImageDateList = new List<ImageList>();
        /// <summary>
        /// 名前項目の要素一覧
        /// </summary>
        private List<NameDataClass> NameDateList = new List<NameDataClass>();
        /// <summary>
        /// 書き出し内容
        /// </summary>
        private List<ChatMessage> WriteDateList = new List<ChatMessage>();
        /// <summary>
        /// 画像変換エラー発生対象内容記録処理
        /// </summary>
        private List<string> ImageConvertErrorNameList = new List<string>();
        /// <summary>
        /// 名前項目の要素件数(コントロールの数の計測に使用)
        /// </summary>
        private int NameCount = 0;

        /// <summary>
        /// 表示キャラクター名
        /// </summary>
        private Label[] NameLabels;
        /// <summary>
        /// Color選択ボタン
        /// </summary>
        private ColorDialogSelected[] ColorSelectedButtons;
        /// <summary>
        /// Color選択値
        /// </summary>
        private TextBox[] ColorSelectedBoxes;

        /// <summary>
        /// 画像選択ボタン
        /// </summary>
        private ImageSelected[] CharacterImageButtons;
        /// <summary>
        /// 画像選択値
        /// </summary>
        private PictureBox[] CharacterImageBoxes;

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 読み込みファイル選択
        /// </summary>
        /// <prm name="sender"></prm>
        /// <prm name="e"></prm>
        private void ButtonSelected_Click(object sender, EventArgs e)
        {

            string filterCode;

            if (radioButtonCC.Checked)
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
                //2番目の「すべてのファイル」が選択されているようにする
                FilterIndex = 1,
                //タイトルを設定する
                Title = "開くファイルを選択してください",
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                RestoreDirectory = true,
                //存在しないファイルの名前が指定されたとき警告を表示する
                CheckFileExists = true,
                //存在しないパスが指定されたとき警告を表示する
                CheckPathExists = true
            };

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                TextBoxHtmlAddress.Text = ofd.FileName;
                buttonReWriteStart.Enabled = false;
            }
            ofd.Dispose();
        }

        /// <summary>
        /// ファイル読み込み処理
        /// </summary>
        /// <prm name="sender"></prm>
        /// <prm name="e"></prm>
        private void ButtonFileRead_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(TextBoxHtmlAddress.Text))
            {
                MessageBox.Show("ファイルを指定してください", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(TextBoxHtmlAddress.Text))
            {
                MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // 小文字に統一
            string extension = Path.GetExtension(TextBoxHtmlAddress.Text).ToLower();
            if (radioButtonCC.Checked)
            {
                if (extension == ".html" || extension == ".htm")
                {
                    HtmlAnalyze();
                }
                else
                {
                    MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (radioButtonUD.Checked)
            {
                if (extension == ".zip")
                {
                    ZipAnalyze();
                }
                else
                {
                    MessageBox.Show("ファイルの指定が誤っています", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("ラジオボタンが選択されていません", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /// <summary>
        /// 出力ボタン押下
        /// </summary>
        /// <prm name="sender"></prm>
        /// <prm name="e"></prm>
        private void ButtonReWriteStart_Click(object sender, EventArgs e)
        {
            NameDateList = new List<NameDataClass>();
            ImageDateList = new List<ImageList>();

            for (int i = 0; NameCount > i; i++)
            {
                NameDataClass tmpName = new NameDataClass();
                ImageList tmpImage = new ImageList();
                bool nameFlg = false;
                foreach (NameDataClass nameData in NameDateList)
                {
                    if (nameData.Name == NameLabels[i].Text)
                    {
                        nameFlg = true;
                    }
                }
                if (!nameFlg)
                {
                    tmpName.Name = NameLabels[i].Text;
                    tmpName.Color = ColorSelectedBoxes[i].Text;
                    tmpImage.ImageName = NameLabels[i].Text;
                    if (!string.IsNullOrEmpty(CharacterImageButtons[i].OriginalBitMap))
                    {
                        tmpImage.ImageBase64 = CharacterImageButtons[i].OriginalBitMap;
                    }
                    NameDateList.Add(tmpName);
                    ImageDateList.Add(tmpImage);
                }
            }
            ConvertCCFOLIAWrite();
        }


        #region ココフォリア

        /// <summary>
        /// HTML解析
        /// </summary>
        private void HtmlAnalyze()
        {
            //各要素の初期化
            ImageDateList = new List<ImageList>();
            WriteDateList = new List<ChatMessage>();
            NameDateList = new List<NameDataClass>();
            if (NameCount != 0)
            {
                RemoveDynamicControls(NameCount);
                NameCount = 0;
            }
            GetNameCCFOLA();

            buttonReWriteStart.Enabled = true;
            NameLabels = new Label[NameCount];
            ColorSelectedButtons = new ColorDialogSelected[NameCount];
            ColorSelectedBoxes = new TextBox[NameCount];
            CharacterImageButtons = new ImageSelected[NameCount];
            CharacterImageBoxes = new PictureBox[NameCount];


            for (int i = 0; i < NameCount; i++)
            {
                Label newLabel = new Label
                {
                    Name = NAME_LABEL + i,
                    Text = NameDateList[i].Name,
                    Size = new Size(75, 25),
                    Location = new Point(10, 10 + i * 100)
                };

                ColorDialogSelected newButton1 = new ColorDialogSelected
                {
                    Name = COLOR_SELECTED_BUTTON + i,
                    Text = COLOR_PALET_TEXT,
                    Size = new Size(75, 20),
                    Location = new Point(100, 10 + i * 100),
                    //カラー設定は未実装機能
                    Enabled = false
                };

                TextBox newTextBox = new TextBox
                {
                    Name = COLOR_SELECTED_BOX + i,
                    //newTextBox.Text = this.NameDateList[i].Color;
                    //カラー設定は未実装機能
                    Text = COLOR_FFFFFF,
                    Enabled = false,

                    Size = new Size(100, 20),
                    Location = new Point(180, 10 + i * 100)
                };

                ImageSelected newButton2 = new ImageSelected
                {
                    Name = CHARACTER_IMAGE_BUTTON + i,
                    Text = "ファイルを選択",
                    Size = new Size(105, 20),
                    Location = new Point(310, 10 + i * 100)
                };

                PictureBox newPictureBox1 = new PictureBox
                {
                    Name = CHARACTER_IMAGE + i,
                    Size = new Size(100, 100),
                    Location = new Point(440, 10 + i * 100)
                };

                NameLabels[i] = newLabel;
                ColorSelectedButtons[i] = newButton1;
                ColorSelectedBoxes[i] = newTextBox;
                CharacterImageButtons[i] = newButton2;
                CharacterImageBoxes[i] = newPictureBox1;

                newButton1.TargetTextBoxID = ColorSelectedBoxes[i];
                newButton2.TargetImageBoxID = CharacterImageBoxes[i];

                //パネルに設定
                panelGUI.Controls.Add(NameLabels[i]);
                panelGUI.Controls.Add(ColorSelectedButtons[i]);
                panelGUI.Controls.Add(ColorSelectedBoxes[i]);
                panelGUI.Controls.Add(CharacterImageButtons[i]);
                panelGUI.Controls.Add(CharacterImageBoxes[i]);

                //イベントの設定
                ColorSelectedButtons[i].EventMaking();
                CharacterImageButtons[i].EventMaking();
            }
        }
        /// <summary>
        /// 名称一覧を作成
        /// </summary>
        private void GetNameCCFOLA()
        {
            using (StreamReader file = new StreamReader(TextBoxHtmlAddress.Text, Encoding.GetEncoding("UTF-8")))
            {
                while (file.Peek() > 0)
                {
                    List<string> tmpArea = new List<string>
                    {
                        file.ReadLine()
                    };
                    if (tmpArea[0].Contains("p style"))
                    {
                        tmpArea.Add(file.ReadLine());
                        tmpArea.Add(file.ReadLine());
                        tmpArea.Add(file.ReadLine());
                        tmpArea.Add(file.ReadLine());
                        tmpArea.Add(file.ReadLine());
                        string colorCode = RightXSelected(tmpArea[0], 10);
                        colorCode = LeftXSelected(colorCode, 7);

                        string strValue = tmpArea[2].Remove(0, tmpArea[2].IndexOf("<span>") + 6);
                        strValue = strValue.Remove(strValue.IndexOf("</span>"));
                        strValue = System.Text.RegularExpressions.Regex.Replace(strValue, @"\s", string.Empty);

                        bool tmpFlg = false;
                        foreach (NameDataClass nameData in NameDateList)
                        {
                            if (nameData.Name == strValue)
                            {
                                tmpFlg = true;
                                break;
                            }
                        }
                        if (!tmpFlg)
                        {
                            NameDataClass nameDate = new NameDataClass
                            {
                                Name = strValue,
                                Color = colorCode
                            };
                            NameDateList.Add(nameDate);
                            NameCount++;
                        }
                        string areaValue = tmpArea[1].Remove(0, tmpArea[1].IndexOf("[") + 1);
                        areaValue = areaValue.Remove(areaValue.IndexOf("]"));
                        ChatMessage writeDate = new ChatMessage
                        {
                            Name = strValue,
                            Area = areaValue,
                            Text = tmpArea[4].Substring(7)
                        };
                        WriteDateList.Add(writeDate);

                    }
                }
            }
        }
        /// <summary>
        /// ココフォリア版変換処理
        /// </summary>
        private void ConvertCCFOLIAWrite()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HtmlResource.HTMLHeader);
            for (int i = 0; i < NameDateList.Count; i++)
            {
                if (ImageDateList[i].ImageBase64 != null)
                {
                    string[] nameArray = NameDateList[i].Name.Select(x => x.ToString()).ToArray();
                    string convertName = StringConvert16(nameArray);
                    sb.AppendLine(string.Format(HtmlResource.ImageHeader, convertName));
                    sb.AppendLine(string.Format(HtmlResource.ImageData, ImageDateList[i].ImageBase64));
                    sb.AppendLine(HtmlResource.Imagefooter);
                }
            }

            for (int i = 0; i < NameDateList.Count; i++)
            {
                string[] nameArray = NameDateList[i].Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);
                //sb.AppendLine(string.Format(HtmlResource.ChatColor, imageFileName, this.NameDateList[i].Color));
                //カラー実装対応は現在未対応
                sb.AppendLine(string.Format(HtmlResource.ChatColor, convertName, COLOR_FFFFFF));
            }
            sb.AppendLine(HtmlResource.StyleEnd);

            string userName = string.Empty;
            string areaName = string.Empty;
            //初回は末尾を付ける必要がないため
            bool firstFlg = true;
            foreach (var writeData in WriteDateList)
            {
                string[] nameArray = writeData.Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);

                if (writeData.Area == HtmlResource.StringMainJP || writeData.Area == HtmlResource.StringMainEN)
                {
                    if (userName != writeData.Name)
                    {
                        userName = writeData.Name;
                        areaName = writeData.Area;
                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, convertName));

                        foreach (var image in ImageDateList)
                        {
                            if (writeData.Name == image.ImageName && !string.IsNullOrEmpty(image.ImageBase64))
                            {
                                sb.AppendLine(string.Format(HtmlResource.DivIcon, convertName));
                            }
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, userName));
                    }
                    else if (areaName != writeData.Area)
                    {
                        userName = writeData.Name;
                        areaName = writeData.Area;
                        sb.AppendLine(HtmlResource.DivEndLine);
                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, writeData.Name));
                        foreach (var image in ImageDateList)
                        {
                            if (writeData.Name == image.ImageName && !string.IsNullOrEmpty(image.ImageBase64))
                            {
                                sb.AppendLine(string.Format(HtmlResource.DivIcon, convertName));
                            }
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, userName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));

                }
                else
                {
                    string areaNameCheck = writeData.Area;
                    //エリア名称日本語変換処理
                    if (areaNameCheck == HtmlResource.StringOtherEN)
                    {
                        areaNameCheck = HtmlResource.StringOtherJP;
                    }
                    else if (areaNameCheck == HtmlResource.StringInfoEN)
                    {
                        areaNameCheck = HtmlResource.StringInfoJP;
                    }


                    if (userName != writeData.Area)
                    {
                        userName = writeData.Name;
                        areaName = areaNameCheck;

                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, userName, areaName));
                    }
                    else if (areaName != areaNameCheck)
                    {
                        userName = writeData.Name;
                        areaName = areaNameCheck;
                        sb.AppendLine(HtmlResource.DivEndLine);
                        sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, userName, areaName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));
                }
            }
            sb.AppendLine(HtmlResource.DivEndLine);
            sb.AppendLine(HtmlResource.HTMLFooter);

            bool result = SetHTML(sb.ToString());
            if (result)
            {
                MessageBox.Show("正常に出力されました", CONST_INFOMATION, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        #endregion

        #region ユドナリウム
        /// <summary>
        /// Zip内部解析処理
        /// </summary>
        private void ZipAnalyze()
        {
            //各要素の初期化
            ImageDateList = new List<ImageList>();
            WriteDateList = new List<ChatMessage>();
            NameDateList = new List<NameDataClass>();
            if (NameCount != 0)
            {
                RemoveDynamicControls(NameCount);
                NameCount = 0;
            }
            string folderPath = TextBoxHtmlAddress.Text;
            List<ChatMessage> ChatDataList = GetTextUDONA(folderPath);
            if (ChatDataList.Count() == 0)
            {
                MessageBox.Show("ファイルからデータが取得できませんでした", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<string> tmpImageNameList = new List<string>();
            foreach (var chatData in ChatDataList)
            {
                if (!tmpImageNameList.Contains(chatData.ImageFileName))
                {
                    tmpImageNameList.Add(chatData.ImageFileName);
                }
            }
            ConvertImageUDONA(folderPath, tmpImageNameList);
            ConvertWriteUDONA(ChatDataList, tmpImageNameList);
        }
        /// <summary>
        /// テキスト整形
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private List<ChatMessage> GetTextUDONA(string folderPath)
        {
            List<ChatMessage> chatMessages = new List<ChatMessage>();
            try
            {
                string zipName = "chat.xml";
                string zipNameFly = "fly_chat.xml";

                string targetPath = null;

                if (CheckFlyBasic(folderPath, zipName))
                {
                    targetPath = zipName;
                }
                else if (CheckFlyBasic(folderPath, zipNameFly))
                {
                    targetPath = zipNameFly;
                }
                else
                {
                    return chatMessages;
                }

                string xmlContent = ExtractXmlFromZip(folderPath, targetPath);
                if (!string.IsNullOrEmpty(xmlContent))
                {
                    chatMessages = ParseChatMessages(xmlContent);
                }
                else
                {
                    MessageBox.Show("XMLの取得に失敗しました。", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラー: {ex.Message}");
            }
            return chatMessages;
        }

        /// <summary>
        /// ZIP内のファイルに指定の名前があるかどうかの探索
        /// </summary>
        /// <param name="filePath">ファイルのアドレス</param>
        /// <param name="fileName">ファイルの名称</param>
        /// <returns>true/false</returns>
        private bool CheckFlyBasic(string filePath, string fileName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(filePath))
            {
                // エントリを列挙し、指定したファイル名が存在するか確認
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // ファイルが見つかった
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ZIP内のXMLファイルを取得
        /// </summary>
        /// <param name="filePath">ファイルのアドレス</param>
        /// <param name="fileName">ファイルの名称</param>
        /// <returns></returns>
        private string ExtractXmlFromZip(string filePath, string fileName)
        {
            using (ZipArchive archive = ZipFile.OpenRead(filePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            // XMLが見つからなかった場合
            return null;
        }

        /// <summary>
        /// XMLを解析し、リスト化
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>        
        private List<ChatMessage> ParseChatMessages(string xmlString)
        {
            XElement root = XElement.Parse(xmlString);
            List<ChatMessage> chatMessages = new List<ChatMessage>();

            // 各 <chat-tab> を処理
            foreach (XElement chatTabElement in root.Elements("chat-tab"))
            {
                // タブ名取得
                string tabName = chatTabElement.Attribute("name")?.Value ?? "その他";

                // タブ内の <chat> を処理
                foreach (XElement chatElement in chatTabElement.Elements("chat"))
                {
                    string strTimeStamp = chatElement.Attribute("timestamp")?.Value ?? "不明";
                    long tmpTimeStamp = new long();
                    long.TryParse(strTimeStamp, out tmpTimeStamp);
                    chatMessages.Add(new ChatMessage
                    {
                        Area = tabName,
                        Name = chatElement.Attribute("name")?.Value ?? string.Empty,
                        TimeStamp = tmpTimeStamp,
                        //Color = chatElement.Attribute("color")?.Value ?? string.Empty,
                        Color = COLOR_FFFFFF,
                        ImageFileName = chatElement.Attribute("imageIdentifier")?.Value ?? string.Empty,
                        Text = RubyElementConvert(chatElement.Value.Trim())
                    });
                }
            }
            //TimeStamp順にソート
            List<ChatMessage> sortChat = chatMessages.OrderBy(x => x.TimeStamp).ToList();
            return sortChat;
        }
        /// <summary>
        /// ZIP内のPNG JPG画像一覧作成
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="tmpImageNameList"></param>
        private void ConvertImageUDONA(string folderPath, List<string> tmpImageNameList)
        {
            using (ZipArchive archive = ZipFile.OpenRead(folderPath))
            {
                //ファイルをフィルタリング
                string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

                var imageEntries = archive.Entries
                    .Where(e =>
                    {
                        string ext = Path.GetExtension(e.FullName).ToLowerInvariant();
                        return allowedExtensions.Contains(ext);
                    }).ToList();

                // 画像名リストを作成
                List<string> fileNamesExt = imageEntries.Select(e => Path.GetFileName(e.Name)).ToList();
                List<string> fileNamesWithoutExt = imageEntries.Select(e => Path.GetFileNameWithoutExtension(e.Name)).ToList();
                for (int i = 0; i < fileNamesWithoutExt.Count; i++)
                {
                    // 拡張子の確認
                    var ext = System.IO.Path.GetExtension(fileNamesExt[i]).ToLower();
                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                    {
                        continue;
                    }

                    if (!tmpImageNameList.Contains(fileNamesWithoutExt[i]))
                    {
                        continue;
                    }

                    var entry = imageEntries[i];
                    using (Stream zipStream = entry.Open())
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStream);

                        if (memoryStream.Length == 0)
                        {
                            continue;
                        }

                        memoryStream.Seek(0, SeekOrigin.Begin);

                        try
                        {
                            using (Bitmap image = new Bitmap(memoryStream))
                            {
                                string image64 = BitmapToBase64StringAuto(image);
                                ImageList imageData = new ImageList
                                {
                                    ImageName = fileNamesWithoutExt[i],
                                    ImageBase64 = image64
                                };
                                ImageDateList.Add(imageData);
                            }
                        }
                        catch
                        {
                            ImageConvertErrorNameList.Add(fileNamesWithoutExt[i]);
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ユドナリウム版変換処理
        /// </summary>
        /// <param name="chatList"></param>
        /// <param name="tmpImageNameList"></param>
        private void ConvertWriteUDONA(List<ChatMessage> chatList, List<string> tmpImageNameList)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HtmlResource.HTMLHeader);

            for (int i = 0; i < ImageDateList.Count; i++)
            {
                sb.AppendLine(string.Format(HtmlResource.ImageHeader, ImageDateList[i].ImageName));
                sb.AppendLine(string.Format(HtmlResource.ImageData, ImageDateList[i].ImageBase64));
                sb.AppendLine(HtmlResource.Imagefooter);
            }
            for (int i = 0; i < tmpImageNameList.Count; i++)
            {
                sb.AppendLine(string.Format(HtmlResource.ChatColor, tmpImageNameList[i], COLOR_FFFFFF));
            }

            sb.AppendLine(HtmlResource.StyleEnd);

            string userName = string.Empty;
            string areaName = string.Empty;
            //エラーキャラクターチェック用リスト
            List<string> imageErrorCharacterNameList = new List<string>();
            List<string> imageErrorImageNameList = new List<string>();
            //初回は末尾を付ける必要がないためチェックフラグ
            bool firstFlg = true;
            foreach (var chatData in chatList)
            {
                string imageFileName = chatData.ImageFileName;

                if (chatData.Area == HtmlResource.StringMainJP || chatData.Area == HtmlResource.StringMainEN)
                {
                    if (userName != chatData.Name)
                    {
                        userName = chatData.Name;
                        areaName = chatData.Area;
                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, imageFileName));

                        foreach (var image in ImageDateList)
                        {
                            if (chatData.ImageFileName == image.ImageName && !string.IsNullOrEmpty(image.ImageBase64))
                            {
                                sb.AppendLine(string.Format(HtmlResource.DivIcon, imageFileName));
                            }
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, userName));
                    }
                    else if (areaName != chatData.Area)
                    {
                        userName = chatData.Name;
                        areaName = chatData.Area;
                        sb.AppendLine(HtmlResource.DivEndLine);
                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, chatData.Name));
                        foreach (var image in ImageDateList)
                        {
                            if (chatData.Name == image.ImageName && !string.IsNullOrEmpty(image.ImageBase64))
                            {
                                sb.AppendLine(string.Format(HtmlResource.DivIcon, imageFileName));
                            }
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, userName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, chatData.Text));

                    //Error名称チェック
                    if (ImageConvertErrorNameList.Count > 0)
                    {
                        if (ImageConvertErrorNameList.Contains(chatData.ImageFileName))
                        {
                            if (!imageErrorCharacterNameList.Contains(chatData.Name))
                            {
                                imageErrorCharacterNameList.Add(chatData.Name);
                                imageErrorImageNameList.Add(chatData.ImageFileName);
                            }
                        }
                    }
                }
                else
                {
                    string areaNameCheck = chatData.Area;
                    //エリア名称日本語変換処理
                    if (areaNameCheck == HtmlResource.StringOtherEN)
                    {
                        areaNameCheck = HtmlResource.StringOtherJP;
                    }
                    else if (areaNameCheck == HtmlResource.StringInfoEN)
                    {
                        areaNameCheck = HtmlResource.StringInfoJP;
                    }

                    if (userName != chatData.Area)
                    {
                        userName = chatData.Name;
                        areaName = areaNameCheck;

                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, imageFileName));
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, userName, areaName));
                    }
                    else if (areaName != areaNameCheck)
                    {
                        userName = chatData.Name;
                        areaName = areaNameCheck;
                        sb.AppendLine(HtmlResource.DivEndLine);
                        sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, imageFileName));
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, userName, areaName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, chatData.Text));
                }
            }

            sb.AppendLine(HtmlResource.DivEndLine);
            sb.AppendLine(HtmlResource.HTMLFooter);

            var outputBool = SetHTML(sb.ToString());

            if (outputBool && imageErrorCharacterNameList.Count > 0)
            {
                string errorMessage = "出力はされましたが、以下キャラクター画像にエラーが確認されました。\r\n出力を確認してください。\r\n";
                string clipboardData = string.Empty;
                for (int i = 0; i < imageErrorCharacterNameList.Count; i++)
                {
                    string characterName = imageErrorCharacterNameList[i];
                    string imageName = imageErrorImageNameList[i];
                    errorMessage += $"\r\n・{characterName}";
                    if (!string.IsNullOrEmpty(clipboardData))
                    {
                        clipboardData += "\r\n";
                    }
                    clipboardData += $"・{characterName} : {imageName}.png";
                }
                errorMessage += "\r\n\r\n対象の画像情報をクリップボードにコピーしますか？";
                DialogResult result = MessageBox.Show(errorMessage, CONST_WARNING, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    Clipboard.SetText(clipboardData);
                    MessageBox.Show("クリップボードにコピーしました。", CONST_INFOMATION, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ImageConvertErrorNameList = new List<string>();
            }
            else if (outputBool)
            {
                MessageBox.Show("正常に出力されました", CONST_INFOMATION, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region 共通変数
        /// <summary>
        /// 出力処理
        /// </summary>
        /// <param name="prm"></param>
        /// <returns></returns>
        public bool SetHTML(string prm)
        {
            //戻り値の設定
            bool returnBool = false;
            //SaveFileDialogクラスのインスタンスを作成
            SaveFileDialog sfd = new SaveFileDialog
            {
                //はじめのファイル名を指定する
                //はじめに「ファイル名」で表示される文字列を指定する
                FileName = "chatLog" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".html",
                //はじめに表示されるフォルダを指定する
                //sfd.InitialDirectory = @"C:\";
                //[ファイルの種類]に表示される選択肢を指定する
                //指定しない（空の文字列）の時は、現在のディレクトリが表示される
                Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*",
                //[ファイルの種類]ではじめに選択されるものを指定する
                //2番目の「すべてのファイル」が選択されているようにする
                FilterIndex = 2,
                //タイトルを設定する
                Title = "保存先のファイルを選択してください",
                //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                RestoreDirectory = true,
                //既に存在するファイル名を指定したとき警告する
                OverwritePrompt = true,
                //存在しないパスが指定されたとき警告を表示する
                CheckPathExists = true
            };

            //ダイアログを表示する
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                {
                    sw.WriteLine(prm);
                }
                returnBool = true;
            }
            sfd.Dispose();
            return returnBool;

        }
        /// <summary>
        /// ルビ追加
        /// </summary>
        /// <param name="input">検証対象</param>
        /// <returns></returns>
        private string RubyElementConvert(string input)
        {   
            // 必要な記号がすべて含まれているかチェック
            if (!input.Contains("|") || !input.Contains("《") || !input.Contains("》"))
            {
                return input;
            }

            // 正規表現を使用して | と 《》 のペアを見つけ変換する
            string pattern = @"\|(.*?)《(.*?)》";
            string replacement = @"<ruby>$1<rt>$2</rt></ruby>";

            return Regex.Replace(input, pattern, replacement);
        }



        /// <summary>
        /// コントロールの削除
        /// </summary>
        /// <prm name="nameCount"></prm>
        private void RemoveDynamicControls(int nameCount)
        {
            for (int i = 0; i < nameCount; i++)
            {
                Control[] controls1 = Controls.Find(NAME_LABEL + i, true);
                foreach (Control control in controls1)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }

                Control[] controls2 = Controls.Find(COLOR_SELECTED_BUTTON + i, true);
                foreach (Control control in controls2)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
                Control[] controls3 = Controls.Find(COLOR_SELECTED_BOX + i, true);
                foreach (Control control in controls3)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
                Control[] controls4 = Controls.Find(CHARACTER_IMAGE_BUTTON + i, true);
                foreach (Control control in controls4)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
                Control[] controls5 = Controls.Find(CHARACTER_IMAGE + i, true);
                foreach (Control control in controls5)
                {
                    Controls.Remove(control);
                    control.Dispose();
                }
            }
        }

        /// <summary>
        /// CCFOLIA画像名称共通化処理
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string StringConvert16(string[] args)
        {
            string returnData = "name";

            foreach (string arg in args)
            {

                // 文字列からUTF8のバイト列に変換
                byte[] data = Encoding.UTF8.GetBytes(arg);
                //00-11-22形式の文字列に変換
                string hexText = BitConverter.ToString(data);

                //00-11-22の文字列より byte[]に変換
                string[] hexChars = hexText.Split('-');
                foreach (var byteString in hexChars)
                {
                    returnData += byteString;
                }

            }
            return returnData;
        }

        #region 画像変換
        /// <summary>
        /// ファイルパスを指定して画像ファイルを開く
        /// </summary>
        /// <prm name="fileName">画像ファイルのファイルパスを指定します。</prm>
        /// <returns>生成したBitmapクラスオブジェクト</returns>
        private static Bitmap ImageFileOpen(string fileName)
        {
            // 指定したファイルが存在するか？確認
            if (System.IO.File.Exists(fileName) == false)
            {
                return null;
            }
            // 拡張子の確認
            var ext = System.IO.Path.GetExtension(fileName).ToLower();

            // ファイルの拡張子が対応しているファイルかどうか調べる
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
            {
                return null;
            }

            try
            {
                Bitmap bmp;
                // ファイルストリームでファイルを開く
                using (var fs = new System.IO.FileStream(
                    fileName,
                    System.IO.FileMode.Open,
                    System.IO.FileAccess.Read))
                {
                    bmp = new Bitmap(fs);
                }
                return bmp;
            }
            catch
            {
                MessageBox.Show("不正な画像が読み込まれました", CONST_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Bitmapを自動的にJPEGまたはPNGにエンコードしてBase64文字列に変換する。
        /// - 透過あり → PNG
        /// - 透過なし → JPEG
        /// </summary>
        public static string BitmapToBase64StringAuto(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // PNGにすべきかチェック
                bool hasAlpha = ImageHasAlpha(bitmap);
                ImageFormat format = hasAlpha ? ImageFormat.Png : ImageFormat.Jpeg;

                // JPEG保存時にパラメータ指定（品質：90）
                if (format == ImageFormat.Jpeg)
                {
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);

                    // JPEGではインデックス形式など不正になる可能性があるので、変換しておく
                    using (Bitmap converted = ConvertTo24bpp(bitmap))
                    {
                        converted.Save(ms, jpegCodec, encoderParams);
                    }
                }
                else
                {
                    bitmap.Save(ms, ImageFormat.Png);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// アルファチャネル（透過）を含むか判定
        /// </summary>
        private static bool ImageHasAlpha(Bitmap bitmap)
        {
            return (Image.GetPixelFormatSize(bitmap.PixelFormat) == 32 &&
                   (bitmap.PixelFormat == PixelFormat.Format32bppArgb ||
                    bitmap.PixelFormat == PixelFormat.Format32bppPArgb));
        }

        /// <summary>
        /// JPEG保存に適した 24bppRgb に変換
        /// </summary>
        private static Bitmap ConvertTo24bpp(Bitmap original)
        {
            Bitmap newBmp = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(newBmp))
            {
                g.DrawImage(original, 0, 0, original.Width, original.Height);
            }
            return newBmp;
        }

        /// <summary>
        /// 指定した形式のエンコーダーを取得
        /// </summary>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return Array.Find(ImageCodecInfo.GetImageDecoders(), c => c.FormatID == format.Guid);
        }

        #endregion

        #region 未実装処理
        /// <summary>
        /// 文字列の先頭から指定した長さの文字列を取得する
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="len">長さ</param>
        /// <returns>取得した文字列</returns>
        public static string LeftXSelected(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return string.Empty;
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(0, len);
        }

        /// <summary>
        /// 文字列の末尾から指定した長さの文字列を取得する
        /// </summary>
        /// <prm name="str">文字列</prm>
        /// <prm name="len">長さ</prm>
        /// <returns>取得した文字列</returns>
        private string RightXSelected(string str, int len)
        {
            if (len < 0)
            {
                throw new ArgumentException("引数'len'は0以上でなければなりません。");
            }
            if (str == null)
            {
                return "";
            }
            if (str.Length <= len)
            {
                return str;
            }
            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// RGB変換
        /// </summary>
        /// <prm name="color"></prm>
        /// <returns></returns>
        public static string RGBChange(Color color)
        {
            string colorCode = string.Format(
                "#{0:X2}{1:X2}{2:X2}",
                color.R,
                color.G,
                color.B);
            return colorCode;
        }

        #endregion

        #endregion


    }
}
