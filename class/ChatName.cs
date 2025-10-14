using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace TRPGLogArrangeTool.resource
{
    public class ChatName : INotifyPropertyChanged
    {
        /// <summary>
        /// キャラクター名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 画像一覧
        /// </summary>
        public ObservableCollection<string> ImageKeys { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// 基本画像
        /// </summary>
        private string _defaultImageKey;
        /// <summary>
        /// 基本画像情報
        /// </summary>
        public string DefaultImageKey
        {
            get => _defaultImageKey;
            set
            {
                if (_defaultImageKey != value)
                {
                    _defaultImageKey = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DefaultImage));
                }
            }
        }

        /// <summary>
        /// 実際の画像取得（キャッシュ経由）
        /// </summary>
        public BitmapImage DefaultImage =>
            string.IsNullOrEmpty(DefaultImageKey) ? null : ImageCache.GetByKey(DefaultImageKey);

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
