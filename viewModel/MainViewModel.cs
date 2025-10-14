using HtmlAgilityPack;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using TRPGLogArrangeTool.resource;

namespace TRPGLogArrangeTool.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region const
        private const string CONST_INFOMATION = "情報";
        private const string CONST_WARNING = "警告";
        private const string CONST_ERROR = "エラー";
        private const string COLOR_FFFFFF = "FFFFFF";
        private const string CONST_EVENT_AREA = "EVENT";
        private const string NAME_EVENT = "EVENT_IMAGE";
        private const string NAME_EVENT_CHARACTER = "EVENT_CHARACTER_IMAGE";
        private const string zipName = "chat.xml";
        private const string zipNameFly = "fly_chat.xml";
        private const string zipStandFly = "fly_data.xml";
        private static readonly string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".gif" };
        #endregion

        #region データ部
        /// <summary>
        /// キャラクターリストデータ
        /// </summary>
        public ObservableCollection<ChatName> ChatNameList { get; set; } = new ObservableCollection<ChatName>();
        /// <summary>
        /// チャット一覧データ
        /// </summary>
        public ObservableCollection<ChatMessage> ChatMessageList { get; set; } = new ObservableCollection<ChatMessage>();

        private ChatName _selectedName;
        /// <summary>
        /// 選択中のキャラクターリストアイテム
        /// </summary>
        public ChatName SelectedName
        {
            get => _selectedName;
            set { _selectedName = value; OnPropertyChanged(); }
        }
        private ChatMessage _selectedMessage;
        /// <summary>
        /// 選択中のチャット一覧アイテム
        /// </summary>
        public ChatMessage SelectedMessage
        {
            get => _selectedMessage;
            set { _selectedMessage = value; OnPropertyChanged(); }
        }
        #endregion

        public MainViewModel()
        {
            //初期処理            
        }

        #region ココフォリア処理
        /// <summary>
        /// HTML処理
        /// </summary>
        /// <param name="address"></param>
        public bool HtmlAnalyze(string address)
        {
            ChatNameList.Clear();
            ChatMessageList.Clear();
            ImageCache.Clear();
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT });
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT_CHARACTER });

            var html = System.IO.File.ReadAllText(address);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            try
            {
                // <p> タグごとにループ
                var ps = doc.DocumentNode.SelectNodes("//p");
                for (int i = 0; i < ps.Count; i++)
                {
                    HtmlNode p = ps[i];
                    var spans = p.SelectNodes("./span"); // p直下のspanを取得
                    if (spans != null)
                    {
                        string area = string.Empty;
                        string name = string.Empty;
                        string message = string.Empty;
                        bool secretTabFlg = false;
                        for (int lp = 0; lp < spans.Count; lp++)
                        {
                            if (lp == 0)
                            {
                                area = spans[lp].InnerText;
                                while (true)
                                {
                                    string oldArea = area;
                                    area = area.Trim('[').Trim(']').Trim(' ');
                                    if (oldArea == area)
                                    {
                                        break;
                                    }
                                }

                                if (area == HtmlResource.StringMainEN)
                                {
                                    area = HtmlResource.StringMainJP;
                                }
                                else if (area == HtmlResource.StringInfoEN)
                                {
                                    name = HtmlResource.StringInfoJP;
                                }
                                else if (area == HtmlResource.StringOtherEN)
                                {
                                    area = HtmlResource.StringOtherJP;
                                }
                                else if (area.ToLower().Contains(HtmlResource.StringSecretJP) || area.ToLower().Contains(HtmlResource.StringSecretEN))
                                {
                                    // 秘匿処理
                                    secretTabFlg = true;
                                }
                            }
                            else if (lp == 1)
                            {
                                name = NameConverter(spans[lp].InnerText);
                            }
                            else
                            {
                                message = TextHtmlEmbellishment(spans[lp].InnerHtml);
                            }
                        }

                        bool tmpFlg = false;
                        foreach (ChatName nameData in ChatNameList)
                        {
                            if (nameData.Name == name)
                            {
                                tmpFlg = true;
                                break;
                            }
                        }
                        if (!tmpFlg)
                        {
                            ChatName nameDate = new ChatName
                            {
                                Name = name
                            };
                            ChatNameList.Add(nameDate);
                        }
                        var chatName = ChatNameList.FirstOrDefault(x => x.Name == name);
                        ChatMessage tmpMessage = new ChatMessage
                        {
                            IsAddedMessage = false,
                            IsSecretMessage = secretTabFlg,
                            Area = area,
                            Name = name,
                            TimeStamp = i * 10,
                            Text = message,
                            ImageKey = chatName?.DefaultImageKey
                        };
                        ChatMessageList.Add(tmpMessage);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("解析に失敗しました、選択ファイルを再確認してください", CONST_WARNING, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        #endregion

        #region Zip処理
        /// <summary>
        /// Zip処理
        /// </summary>
        /// <param name="folderPath">ファイルパス</param>
        /// <param name="detailCheck">詳細解析</param>
        /// <param name="standCheck">スタンド解析</param>
        /// <returns></returns>
        public bool ZipAnalyze(string folderPath, bool detailCheck, bool standCheck)
        {
            ChatNameList.Clear();
            ChatMessageList.Clear();
            ImageCache.Clear();
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT });
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT_CHARACTER });

            bool flyFlg = false;

            string targetPath;
            if (CheckFlyBasic(folderPath, zipName))
            {
                targetPath = zipName;
            }
            else if (CheckFlyBasic(folderPath, zipNameFly))
            {
                targetPath = zipNameFly;
                flyFlg = standCheck;
            }
            else
            {
                return false;
            }
            string xmlContent = ExtractXmlFromZip(folderPath, targetPath);
            ParseChatMessages(xmlContent, folderPath, flyFlg);
            if (!detailCheck)
            {
                ConvertWrite();
            }
            return true;
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
        private void ParseChatMessages(string xmlString, string folderPath, bool flyFlg)
        {
            // 一時領域
            List<ChatMessage> tmpMessageList = new List<ChatMessage>();
            List<string> tmpIconFilePathList = new List<string>();

            XElement root = XElement.Parse(xmlString);

            // ChatMessageList と ChatNameList を初期化
            ChatMessageList.Clear();
            ChatNameList.Clear();
            ImageCache.Clear();
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT });
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT_CHARACTER });

            // Flyの場合以下を解析            
            List<CharacterStandInfo> standInfos = new List<CharacterStandInfo>();
            if (flyFlg)
            {
                standInfos = StandListCreate(folderPath);
            }

            // XMLからChatMessageを作成
            foreach (XElement chatTabElement in root.Elements("chat-tab"))
            {
                string tabName = chatTabElement.Attribute("name")?.Value ?? "その他";

                foreach (XElement chatElement in chatTabElement.Elements("chat"))
                {
                    string strTimeStamp = chatElement.Attribute("timestamp")?.Value ?? "0";
                    long.TryParse(strTimeStamp, out long tmpTimeStamp);

                    string name = chatElement.Attribute("name")?.Value ?? string.Empty;
                    string text = TextHtmlEmbellishment(chatElement.Value.Trim());
                    string imageIdentifier = chatElement.Attribute("imageIdentifier")?.Value ?? string.Empty;

                    if (flyFlg)
                    {
                        string selectedStandName = chatElement.Attribute("standName")?.Value ?? string.Empty;
                        if (!String.IsNullOrEmpty(selectedStandName))
                        {
                            CharacterStandInfo targetCharacter = standInfos.Where(x => x.Name == name).FirstOrDefault();
                            if (targetCharacter != null)
                            {
                                string tmpImage = targetCharacter.StandDictionary[selectedStandName];
                                if (!string.IsNullOrEmpty(tmpImage))
                                {
                                    imageIdentifier = tmpImage;
                                }
                            }
                        }
                    }
                    if (chatElement.Attribute("to")?.Value != null)
                    {
                        // 秘匿はスキップ
                        continue;
                    }

                    // ChatNameを作成・登録（初回）
                    var chatName = ChatNameList.FirstOrDefault(x => x.Name == name);
                    if (chatName == null)
                    {
                        chatName = new ChatName
                        {
                            Name = name,
                            DefaultImageKey = imageIdentifier
                        };
                        ChatNameList.Add(chatName);
                    }

                    if (!tmpIconFilePathList.Contains(imageIdentifier))
                    {
                        tmpIconFilePathList.Add(imageIdentifier);
                    }

                    // ChatMessageを作成
                    var chatMessage = new ChatMessage
                    {
                        IsAddedMessage = false,
                        IsSecretMessage = false,
                        Area = tabName,
                        Name = name,
                        TimeStamp = tmpTimeStamp,
                        Text = text,
                        ImageKey = imageIdentifier
                    };
                    tmpMessageList.Add(chatMessage);
                }
            }

            // ZIP内画像をImageCacheに登録
            using (var archive = ZipFile.OpenRead(folderPath))
            {
                string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

                foreach (var entry in archive.Entries)
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name);
                    string ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        continue;
                    }

                    var matchingMessages = tmpMessageList.Where(x => x.ImageKey == nameWithoutExt).ToList();

                    if (matchingMessages.Count == 0)
                    {
                        continue;
                    }

                    using (var ms = new MemoryStream())
                    {
                        entry.Open().CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);

                        // ImageCache登録
                        string base64 = Convert.ToBase64String(ms.ToArray());
                        var bmp = ImageCache.GetOrAddFromBase64(base64, out string key);

                        // ChatMessageとChatName にキーを設定
                        foreach (var msg in matchingMessages)
                        {
                            msg.ImageKey = key;
                        }

                        foreach (var item in ChatNameList.Where(x => x.Name.Trim() == matchingMessages[0].Name.Trim()))
                        {
                            if (!item.ImageKeys.Contains(key))
                            {
                                item.ImageKeys.Add(key);
                            }
                        }
                    }
                }
            }

            // 時系列でソート
            foreach (var item in tmpMessageList.OrderBy(x => x.TimeStamp).ToList())
            {
                ChatMessageList.Add(item);
            }
        }
        /// <summary>
        /// Stand画像一覧作成
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<CharacterStandInfo> StandListCreate(string path)
        {
            List<CharacterStandInfo> characterStandList = new List<CharacterStandInfo>();
            string fileData = string.Empty;

            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Equals(zipStandFly, StringComparison.OrdinalIgnoreCase))
                    {
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            fileData = reader.ReadToEnd();
                            break;
                        }
                    }
                }
            }
            if (fileData == string.Empty)
            {
                return characterStandList;
            }

            XElement root = XElement.Parse(fileData);

            foreach (var charElem in root.Elements("character"))
            {
                var characterData = charElem.Element("data");
                if (characterData == null)
                {
                    continue;
                }
                // キャラクター名
                var commonData = characterData.Elements("data").FirstOrDefault(x => (string)x.Attribute("name") == "common");

                string name = commonData?
                    .Elements("data")
                    .FirstOrDefault(x => (string)x.Attribute("name") == "name")
                    ?.Value?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }
                var standInfo = new CharacterStandInfo
                {
                    Name = name
                };

                // stand-listを取得
                var standListElement = charElem.Element("stand-list");
                if (standListElement != null)
                {
                    foreach (var standElem in standListElement.Elements("data")
                                 .Where(x => (string)x.Attribute("name") == "stand"))
                    {
                        string standName = standElem.Elements("data")
                            .FirstOrDefault(x => (string)x.Attribute("name") == "name")
                            ?.Value?.Trim() ?? string.Empty;

                        string standImage = standElem.Elements("data")
                            .FirstOrDefault(x => (string)x.Attribute("type") == "image" &&
                                                 (string)x.Attribute("name") == "imageIdentifier")
                            ?.Value?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(standName) && !string.IsNullOrEmpty(standImage))
                        {
                            standInfo.StandDictionary[standName] = standImage;
                        }
                    }
                }
                if (standInfo.StandDictionary.Count > 0)
                {
                    characterStandList.Add(standInfo);
                }
            }
            return characterStandList;
        }
        #endregion

        #region 変換処理
        /// <summary>
        /// 変換処理
        /// </summary>
        public void ConvertWrite()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HtmlResource.HTMLHeader);

            var usedImageKeys = ChatMessageList
                .Where(x => !string.IsNullOrEmpty(x.ImageKey))
                .Select(m => m.ImageKey)
                .Distinct()
                .ToList();
            foreach (var key in usedImageKeys)
            {
                var base64 = ImageCache.GetBase64ByKey(key);
                if (base64 == null)
                {
                    continue;
                }
                var className = key;
                sb.AppendLine(string.Format(HtmlResource.ImageHeader, className));
                sb.AppendLine(string.Format(HtmlResource.ImageData, base64));
                sb.AppendLine(HtmlResource.Imagefooter);
            }

            for (int i = 0; i < ChatNameList.Count; i++)
            {
                string[] nameArray = ChatNameList[i].Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);
                //カラー実装対応は現在未対応
                sb.AppendLine(string.Format(HtmlResource.ChatColor, convertName, COLOR_FFFFFF));
            }
            sb.AppendLine(HtmlResource.StyleEnd);

            string tmpUserName = string.Empty;
            string tmpImageKey = string.Empty;
            string tmpAreaName = string.Empty;
            //初回は末尾を付ける必要がないため
            bool firstFlg = true;
            foreach (var writeData in ChatMessageList)
            {
                string[] nameArray = writeData.Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);

                if (writeData.Area == HtmlResource.StringMainJP || writeData.Area == HtmlResource.StringMainEN
                    || writeData.Area == HtmlResource.StringInfoJP || writeData.Area == HtmlResource.StringInfoEN)
                {
                    if (tmpUserName != writeData.Name || tmpAreaName != writeData.Area || tmpImageKey != writeData.ImageKey)
                    {
                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else if (tmpAreaName != CONST_EVENT_AREA)
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }
                        tmpUserName = writeData.Name;
                        tmpImageKey = writeData.ImageKey;
                        tmpAreaName = writeData.Area;

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, convertName));

                        if (!String.IsNullOrEmpty(writeData.ImageKey))
                        {
                            sb.AppendLine(string.Format(HtmlResource.DivIcon, writeData.ImageKey));
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, tmpUserName,tmpAreaName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));
                }
                else if (writeData.IsAddedMessage)
                {
                    //画像が追加されていない場合スキップ
                    if (string.IsNullOrEmpty(writeData.ImageKey))
                    {
                        continue;
                    }

                    if (firstFlg)
                    {
                        firstFlg = false;
                    }
                    else if (tmpAreaName != CONST_EVENT_AREA)
                    {
                        sb.AppendLine(HtmlResource.DivEndLine);
                    }
                    tmpUserName = writeData.Name;
                    tmpAreaName = writeData.Area;

                    var base64 = ImageCache.GetBase64ByKey(writeData.ImageKey);
                    if (writeData.Name == NAME_EVENT)
                    {
                        sb.AppendLine(string.Format(HtmlResource.EventImage, base64));
                    }
                    else if (writeData.Name == NAME_EVENT_CHARACTER)
                    {
                        sb.AppendLine(string.Format(HtmlResource.EventCharacter, base64));
                    }
                }
                else
                {
                    string areaNameCheck = writeData.Area;
                    //エリア名称日本語変換処理
                    if (areaNameCheck == HtmlResource.StringOtherEN)
                    {
                        areaNameCheck = HtmlResource.StringOtherJP;
                    }

                    if (tmpUserName != writeData.Area)
                    {
                        tmpUserName = writeData.Name;
                        tmpImageKey = string.Empty;
                        tmpAreaName = areaNameCheck;

                        if (firstFlg)
                        {
                            firstFlg = false;
                        }
                        else if (tmpAreaName != CONST_EVENT_AREA)
                        {
                            sb.AppendLine(HtmlResource.DivEndLine);
                        }

                        if (writeData.IsSecretMessage)
                        {
                            //秘匿チャット
                            sb.AppendLine(string.Format(HtmlResource.DivChatUserSecret, convertName));
                        }
                        else
                        {
                            sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, tmpUserName, tmpAreaName));
                    }
                    else if (tmpAreaName != areaNameCheck)
                    {
                        tmpUserName = writeData.Name;
                        tmpImageKey = string.Empty;
                        tmpAreaName = areaNameCheck;
                        sb.AppendLine(HtmlResource.DivEndLine);
                        sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, tmpUserName, tmpAreaName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));
                }
            }
            sb.AppendLine(HtmlResource.DivEndLine);
            sb.AppendLine(HtmlResource.HTMLFooter);

            bool result = SetHTML(sb.ToString());
            if (result)
            {
                MessageBox.Show("正常に出力されました", CONST_INFOMATION, MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
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
                //[ファイルの種類]に表示される選択肢を指定する
                //指定しない（空の文字列）の時は、現在のディレクトリが表示される
                Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*",
                //[ファイルの種類]ではじめに選択されるものを指定する
                FilterIndex = 1,
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
            if (sfd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                {
                    sw.WriteLine(prm);
                }
                returnBool = true;
            }
            return returnBool;

        }
        #endregion

        #region 共通

        #region List追加処理
        /// <summary>
        /// イベント画像欄追加
        /// </summary>
        public void InsertEventImage(bool nextFlg = true)
        {
            if (SelectedMessage == null)
            {
                return;
            }
            var index = ChatMessageList.IndexOf(SelectedMessage);

            int addPosition = 0;
            long timeStampPosition = SelectedMessage.TimeStamp;
            if (nextFlg)
            {
                addPosition = 1;
                timeStampPosition += 1;
            }
            else
            {
                timeStampPosition -= 1;
            }

            if (index >= 0)
            {
                var newMessage = new ChatMessage
                {
                    IsAddedMessage = true,
                    Name = NAME_EVENT,
                    Area = CONST_EVENT_AREA,
                    Text = "イベント画像",
                    TimeStamp = timeStampPosition
                };
                ChatMessageList.Insert(index + addPosition, newMessage);
            }
        }
        /// <summary>
        /// イベントキャラクター画像欄追加
        /// </summary>
        public void InsertEventCharacterImage(bool nextFlg = true)
        {
            if (SelectedMessage == null)
            {
                return;
            }
            var index = ChatMessageList.IndexOf(SelectedMessage);

            int addPosition = 0;
            long timeStampPosition = SelectedMessage.TimeStamp;
            if (nextFlg)
            {
                addPosition = 1;
                timeStampPosition += 1;
            }
            else
            {
                timeStampPosition -= 1;
            }

            if (index >= 0)
            {
                var newMessage = new ChatMessage
                {
                    IsAddedMessage = true,
                    Name = NAME_EVENT_CHARACTER,
                    Area = CONST_EVENT_AREA,
                    Text = "イベントキャラクター画像",
                    TimeStamp = timeStampPosition
                };
                ChatMessageList.Insert(index + addPosition, newMessage);
            }
        }
        /// <summary>
        /// メッセージ削除
        /// </summary>
        public void DeleteMessage(bool deleteMode)
        {
            if (SelectedMessage == null)
            {
                return;
            }
            if (!SelectedMessage.IsAddedMessage && !deleteMode)
            {
                return;
            }
            if (!SelectedMessage.IsAddedMessage && deleteMode)
            {
                if (MessageBox.Show("追加項目ではありません。\r\n元に戻すためには最初からやり直す必要がありますが削除しますか？",
                    CONST_WARNING, MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            var index = ChatMessageList.IndexOf(SelectedMessage);

            ChatMessageList.RemoveAt(index);
        }

        #endregion

        #region テキスト装飾
        /// <summary>
        /// テキスト装飾系
        /// </summary>
        /// <param name="input">検証対象</param>
        /// <returns></returns>
        private string TextHtmlEmbellishment(string input)
        {
            return EmbellishmentConverter(RubyElementConvert(input));
        }

        /// <summary>
        /// ルビ追加
        /// </summary>
        /// <param name="input">検証対象</param>
        /// <returns></returns>
        private string RubyElementConvert(string input)
        {
            // 記号の条件チェック
            bool hasPipe = input.Contains("|") || input.Contains("｜");
            bool hasAngleBracketsBefore = input.Contains("《") && input.Contains("》");
            bool hasAngleBracketsAfter = input.Contains("≪") && input.Contains("≫");

            if (!hasPipe || (!hasAngleBracketsBefore && !hasAngleBracketsAfter))
            {
                return input;
            }

            // パターンマッチングと変換処理
            var pattern = @"[\|｜](.+?)(《|≪)(.+?)(》|≫)";
            var regex = new Regex(pattern);
            int matchCount = 0;
            var result = new StringBuilder();

            int lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                result.Append(input.Substring(lastIndex, match.Index - lastIndex));
                string baseText = match.Groups[1].Value;
                string rubyText = match.Groups[3].Value;
                result.Append($"<ruby>{baseText}<rt>{rubyText}</rt></ruby>");
                lastIndex = match.Index + match.Length;
                matchCount++;
            }

            // 残りの文字列を追加
            result.Append(input.Substring(lastIndex));

            // 有効な変換が1件もなければ元の文字列を返す
            return matchCount > 0 ? result.ToString() : input;
        }
        /// <summary>
        /// 単純文字装飾処理追加
        /// </summary>
        /// <param name="input">検証対象</param>
        /// <returns></returns>
        private string EmbellishmentConverter(string input)
        {

            while (true)
            {
                string oldInput = input;

                input = input.Trim('\n');
                //空白削除
                input = input.Trim(' ');
                input = input.Trim('　');
                //先頭最終改行のみ削除
                if (input.StartsWith("\r\n", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(4);
                }
                if (input.EndsWith("\r\n", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(0, input.Length - 4);
                }
                if (input.StartsWith("<br>", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(4);
                }
                if (input.EndsWith("<br>", StringComparison.OrdinalIgnoreCase))
                {
                    input = input.Substring(0, input.Length - 4);
                }
                if (input == oldInput)
                {
                    break;
                }
            }
            //打ち消し線
            input = Regex.Replace(input, "~~~(.*?)~~~", "<s>$1</s>");
            //強調
            input = Regex.Replace(input, "###(.*?)###", "<b>$1</b>");
            //改行
            input = Regex.Replace(input, @"\r?\n", "<br>");
            //文字装飾修正
            input = input.Replace("\"]+\"", "\"] +\"");

            return input;
        }
        /// <summary>
        /// キャラクター名空白削除
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string NameConverter(string input)
        {
            input = input.Replace(" ", "");
            input = input.Replace("　", "");
            return input;
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

        /// <summary>
        /// 画像ファイルをキャッシュ登録し、指定キャラおよびメッセージに反映
        /// </summary>
        /// <param name="filePath">画像ファイルパス</param>
        /// <param name="chatName">対象キャラ</param>
        /// <param name="overrideFromIndex">上書き開始インデックス（通常は -1）</param>
        public void AddImageToCharacter(string filePath, ChatName chatName, int overrideFromIndex = -1)
        {
            if (string.IsNullOrWhiteSpace(filePath) || chatName == null)
            {
                return;
            }

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return;
            }

            // キャッシュに登録
            var bmp = ImageCache.GetOrAddFromFile(filePath, out string key);
            if (bmp == null)
            {
                List<string> errorPath = new List<string>
                {
                   Path.GetFileName(filePath)
                };
                ImageAddErrorMessage(errorPath);
                return;
            }

            // キャラに画像キーを追加
            if (!chatName.ImageKeys.Contains(key))
            {
                chatName.ImageKeys.Add(key);
                if (string.IsNullOrEmpty(chatName.DefaultImageKey))
                {
                    // デフォルト画像が設定されていない場合設定する。
                    chatName.DefaultImageKey = key;
                }
            }

            // 上書きモード
            if (overrideFromIndex >= 0)
            {
                for (int i = overrideFromIndex; i < ChatMessageList.Count; i++)
                {
                    if (ChatMessageList[i].Name == chatName.Name)
                    {
                        ChatMessageList[i].ImageKey = key;
                    }
                }
            }
            else
            {
                // 通常モード：未設定のメッセージに設定
                foreach (var msg in ChatMessageList.Where(m => m.Name == chatName.Name && string.IsNullOrEmpty(m.ImageKey)))
                {
                    msg.ImageKey = key;
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        #region 静的処理

        /// <summary>
        /// 画像追加エラーダイアログ表示
        /// </summary>
        /// <param name="errorFileName"></param>
        public static void ImageAddErrorMessage(List<string> errorFileName)
        {
            if (errorFileName.Count == 0)
            {
                return;
            }

            string fileNameData = string.Empty;

            if (errorFileName.Count == 1)
            {
                MessageBox.Show("画像の追加に失敗しました。ファイルを再確認してください", CONST_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                foreach (var item in errorFileName)
                {
                    if (!String.IsNullOrEmpty(fileNameData))
                    {
                        fileNameData += "\r\n";
                    }
                    fileNameData += item;
                }
                if (!string.IsNullOrEmpty(fileNameData))
                {
                    if (MessageBox.Show(errorFileName.Count.ToString() + "件の画像の追加に失敗しました。ファイルを再確認してください。\r\n失敗したファイル名をクリップボードに保存しますか？", CONST_ERROR, MessageBoxButton.YesNo, MessageBoxImage.Error)
                        == MessageBoxResult.Yes)
                    {
                        Clipboard.SetText(fileNameData);
                        MessageBox.Show("クリップボードにコピーしました。", CONST_INFOMATION, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        #endregion

    }
}
