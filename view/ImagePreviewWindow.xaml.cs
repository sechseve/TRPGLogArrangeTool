using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TRPGLogArrangeTool
{
    /// <summary>
    /// ImagePreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ImagePreviewWindow : Window
    {
        public ImagePreviewWindow()
        {
            InitializeComponent();
        }
        private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        public ImagePreviewWindow(ImageSource imageSource)
        {
            InitializeComponent();
            PreviewImage.Source = imageSource;
        }
    }
}
