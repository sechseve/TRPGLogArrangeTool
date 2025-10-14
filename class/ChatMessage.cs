using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace TRPGLogArrangeTool.resource
{
    public class ChatMessage : INotifyPropertyChanged
    {
        /// <summary>
        /// 追加メッセージ欄フラグ
        /// </summary>
        private bool _isAddedByMessage;
        /// <summary>
        /// 追加メッセージ欄フラグ
        /// </summary>
        public bool IsAddedMessage
        {
            get => _isAddedByMessage;
            set
            {
                _isAddedByMessage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 秘匿チャットフラグ
        /// </summary>
        private bool _isSecretMessage;
        /// <summary>
        /// 秘匿チャットフラグ
        /// </summary>
        public bool IsSecretMessage
        {
            get => _isSecretMessage;
            set
            {
                _isSecretMessage = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// チャットタブ情報
        /// </summary>
        private string _area;
        /// <summary>
        /// チャットタブ情報
        /// </summary>
        public string Area
        {
            get => _area;
            set
            {
                _area = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 入力時刻(ソート用)
        /// </summary>
        private long _timeStamp;
        /// <summary>
        /// 入力時刻(ソート用)
        /// </summary>
        public long TimeStamp
        {
            get => _timeStamp;
            set
            {
                _timeStamp = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// チャット内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 画像キー
        /// </summary>
        private string _imageKey;
        /// <summary>
        /// 画像キー
        /// </summary>
        public string ImageKey
        {
            get => _imageKey;
            set
            {
                if (_imageKey != value)
                {
                    _imageKey = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Image));
                }
            }
        }

        public BitmapImage Image =>
            string.IsNullOrEmpty(ImageKey) ? null : ImageCache.GetByKey(ImageKey);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}