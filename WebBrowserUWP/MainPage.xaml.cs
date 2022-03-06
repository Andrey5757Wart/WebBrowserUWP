using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace WebBrowserUWP
{
    /// <summary>
    /// Страница пользовательского взаимодействия, MainPage, она же Вью по MVVM 
    /// </summary>
    public sealed partial class MainPage : Page
    {      
        public MainPage()
        {
            this.InitializeComponent();              
            //TODO: Просмотр всех TODO -- Ctrl + W, T (три кнопки)          

        }

        /// <summary>
        /// Метод, обрабатывающий нажатие кнопки светлой темы (и устанавливающий её в настройках)
        /// </summary>
        /// <param name="sender">Объект, вызвавший метод</param>
        /// <param name="e">Дополнительная информация о вызове</param>
        private void Light_Click(object sender, RoutedEventArgs e)
        {
            RequestedTheme = ElementTheme.Light;
            ApplicationData.Current.LocalSettings.Values["Theme"] = "Light";
        }

        /// <summary>
        /// Метод, обрабатывающий нажатие кнопки темной темы (и устанавливающий её в настройках)
        /// </summary>
        /// <param name="sender">Объект, вызвавший метод</param>
        /// <param name="e">Дополнительная информация о вызове</param>
        private void Dark_Click(object sender, RoutedEventArgs e)
        {
            RequestedTheme = ElementTheme.Dark;
            ApplicationData.Current.LocalSettings.Values["Theme"] = "Dark";
        }      
    }
}
