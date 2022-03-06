using GalaSoft.MvvmLight.Command;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace WebBrowserUWP.CustomTab
{
    /// <summary>
    /// Класс кастомной вкладки с расширенным функционалом (он же Модель по MVVM)
    /// </summary>
    /// sealed запрещает наследование класса - это необходимо для корректной работы сборщика мусора (интерфейс IDisposable), INotifyPropertyChanged необходим для уведомления View об изменениях значения перменных
    public sealed class CustomTabViewItem : TabViewItem, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Команды, вызываемые кодом для отображения изменений в интерфейсе
        /// </summary>
        #region Информирование об изменении свойств
        /// <summary>
        /// Событие, информирующие об изменении того или иного свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Публичный метод для вызова события об изменении конкретного метода
        /// </summary>
        /// <param name="_propertyName">Наименование свойства</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        /// <summary>
        /// Статическая (глобальная) инициализация истории и закладок и домашней страницы (всё берётся из настроек)
        /// </summary>        
        #region Статические конструкторы
     

        /// <summary>
        /// Статическая общая для всех переменная с главной страницой WebBrowserUWPа
        /// </summary>
        public static string HomePage { get { return ApplicationData.Current.LocalSettings.Values["homepage"] == null ? "https://yandex.com/" : (string)ApplicationData.Current.LocalSettings.Values["homepage"]; } set { ApplicationData.Current.LocalSettings.Values["homepage"] = value; } }
        #endregion

        /// <summary>
        /// Регион, содержащий внутри комманды, необходимые для работы с интерфейсом посредством Binding, приватные переменные необходимы для нормальный работы всех алгоритмов привязки данных
        /// </summary>
        #region Команды взаимодействия с View

        private RelayCommand _goBack { get; set; }

        /// <summary>
        /// Команда обработки перехода назад
        /// </summary>
        public RelayCommand GoBack { get
            {
                return _goBack ?? (_goBack = new RelayCommand(() =>
                {
                    WebContent.GoBack();
                })); 
            } 
        }

        private RelayCommand _goForward { get; set; }

        /// <summary>
        /// Команда обработки перехода вперед
        /// </summary>
        public RelayCommand GoForward
        {
            get
            {
                return _goForward ?? (_goForward = new RelayCommand(() =>
                {
                    WebContent.GoForward();
                }));
            }
        }


        private RelayCommand _reload { get; set; }

        /// <summary>
        /// Команда обновления страницы
        /// </summary>
        public RelayCommand Reload
        {
            get
            {
                return _reload ?? (_reload = new RelayCommand(() =>
                {
                    WebContent.Refresh();
                }));
            }
        }

        private RelayCommand _enter { get; set; }

        /// <summary>
        /// Команда начала навигации при нажатии на кнопку поиска (или поиск, или навигация на сайт)
        /// </summary>
        public RelayCommand Enter
        {
            get
            {
                return _enter ?? (_enter = new RelayCommand(() =>
                {
                    if (Uri.IsWellFormedUriString(SearchBox, UriKind.Absolute))
                        WebContent.Navigate(new Uri(SearchBox));
                    else
                        WebContent.Navigate(new Uri(string.Format("{0}/search/?text={1}", HomePage, WebUtility.UrlEncode(SearchBox))));
                }));
            }
        }

 

        private RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> _navigateResult { get; set; }

        /// <summary>
        /// Команда, запускающая навигацию по выбранной строчке в поиске
        /// </summary>
        public RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> NavigateResult
        {
            get
            {
                return _navigateResult ?? (_navigateResult = new RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>((AutoSuggestBoxSuggestionChosenEventArgs args) =>
                {                  
                    SearchBox = args.SelectedItem as string;
                    IsSuggestionOpen = false;
                }));
            }
        }

        private RelayCommand _home { get; set; }

        /// <summary>
        /// Команда перехода на домашнюю страницу
        /// </summary>
        public RelayCommand Home
        {
            get
            {
                return _home ?? (_home = new RelayCommand(() =>
                {
                    WebContent.Navigate(new Uri(HomePage));
                }));
            }
        }       

        private RelayCommand<string> _searchChanged { get; set; }

        /// <summary>
        /// Команда, обрабатывающая изменение поискового запроса в поле ввода
        /// </summary>
        public RelayCommand<string> SearchChanged
        {
            get
            {
                return _searchChanged ?? (_searchChanged = new RelayCommand<string>((string e) =>
                {
                    SearchBox = e;
                }));
            }
        }

     


        #endregion

        /// <summary>
        /// Различные классы, списки, текстовые и прочие переменные для реализации логики класса
        /// </summary>
        #region Различные переменные для работы класса

        private ObservableCollection<string> _showResultList { get; set; }

        /// <summary>
        /// Коллекция, отображающая элементы поиска в истории
        /// </summary>
        public ObservableCollection<string> ShowResultsList { get { return _showResultList; } set { _showResultList = value; NotifyPropertyChanged(nameof(ShowResultsList)); } }

        /// <summary>
        /// Приватная текстовая переменная, необходимая для хранения текущего названия страницы (и последующего использования в одной из команд)
        /// </summary>
        private string _currentTitle { get; set; }

        private bool _isSuggestionOpen { get; set; }
        /// <summary>
        /// Булевая перменная, определяющая, открыт ли список поиска по истории или нет
        /// </summary>
        public bool IsSuggestionOpen { get { return _isSuggestionOpen; } set { _isSuggestionOpen = value; NotifyPropertyChanged(nameof(IsSuggestionOpen)); } }

        private string _searchBox { get; set; }

        /// <summary>
        /// Строковая переменная, хранящая значение поискового запроса (и выполняющая функцию адресной строки)
        /// </summary>
        public string SearchBox { get { return _searchBox; } set { _searchBox = value; 
                NotifyPropertyChanged(nameof(SearchBox)); } }


 


        private WebView _webView { get; set; }
        
        /// <summary>
        /// Публичный WebView вкладки
        /// </summary>
        public WebView WebContent {
            get {
                return _webView ?? (_webView = new WebView()
                {
                    HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch,
                    VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Stretch
                }); 
            } 
        }

        #endregion

        /// <summary>
        /// Конструктор класса вкладки с инициализацией по ссылке, которую необходимо открыть во вкладке
        /// </summary>
        /// <param name="_url">Ссылка, которую необходимо открыть во вкладке</param>
        public CustomTabViewItem(string _url)
        {
            _loadInfo(_url);           
        }      

        /// <summary>
        /// Конструктор класска вкладки со стандартной инициализацией (откроется домашняя страница)
        /// </summary>
        public CustomTabViewItem()
        {
            _loadInfo();           
        }

        /// <summary>
        /// Различные приватные (частные) методы, реализующую внутреннюю работу класса
        /// </summary>
        #region Приватные методы, реализующие внутреннюю логику класса

        /// <summary>
        /// Загрузка всех необходимых для работы делегатов, подписки на события, иницализация переменных и различных классов, начало навигации
        /// </summary>
        /// <param name="url">Ссылка, которую необходимо открыть во вкладке</param>
        private void _loadInfo(string url = null)
        {
            Header = "Новая вкладка";        
            WebContent.DOMContentLoaded += _content_DOMContentLoaded;
            WebContent.NavigationCompleted += _content_NavigationCompleted;
            WebContent.NavigationFailed += _content_NavigationFailed;
            WebContent.NavigationStarting += _content_NavigationStarting;
            WebContent.ContentLoading += _content_ContentLoading;
            WebContent.NewWindowRequested += _content_NewWindowRequested;
            WebContent.Navigate(new Uri(url ?? HomePage));          
            this.Content = WebContent;       
            NotifyPropertyChanged("SelectedItem");
        }


        /// <summary>
        /// Метод, загружающий иконку сайта и отображащий её в качестве иконки вкладки
        /// </summary>
        /// <param name="url">Ссылка, по которой нужно получить иконку</param>
        private void _loadIcon(string url = null)
        {
            IconSource = new Microsoft.UI.Xaml.Controls.BitmapIconSource()
            {
                UriSource = new Uri(new Uri(url ?? HomePage).Scheme + "://" + new Uri(url ?? HomePage).Host + "/favicon.ico"),
                ShowAsMonochrome = false
            };
        }

        /// <summary>
        /// Запрос на открытие новой страницы. Есть отказаться от использования этого метода, WebBrowserUWP будет выкидывать ссылки во внешний мир. 
        /// </summary>
        /// <param name="sender">WebView, вызвавший это событие</param>
        /// <param name="args">Сопроводительная информация о случившемся событии</param>
        private void _content_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            WebContent.Navigate(args.Uri);
            args.Handled = true;
        }

        /// <summary>
        /// Метод, вызываемый при начале навигации. Метод вызывается первым (регламент работы WebView)
        /// </summary>
        /// <param name="sender">WebView, вызвавший это событие</param>
        /// <param name="args">Сопроводительная информация о случившемся событии</param>
        private void _content_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {

            Header = _currentTitle = "Загрузка...";

        }

        /// <summary>
        /// Метод, вызываемый при начале прорисовки содержимого и загрузке всех скриптов. Метод вызывается вторым
        /// </summary>
        /// <param name="sender">WebView, вызвавший это событие</param>
        /// <param name="args">Сопроводительная информация о случившемся событии</param>
        private void _content_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
             Header = _currentTitle = sender.DocumentTitle ?? "Загрузка...";
            _loadIcon(args.Uri.AbsoluteUri ?? HomePage);           
            SearchBox = args.Uri.AbsoluteUri;
        }


        /// <summary>
        /// Метод, вызываемый при полной прорисовке содержимого окна. Метод вызывается третьим (и вызывается единожды -- в случае успешной загрузке страницы, в отличие от предыдущих методов)
        /// </summary>
        /// <param name="sender">WebView, вызвавший это событие</param>
        /// <param name="args">Сопроводительная информация о случившемся событии</param>
        private void _content_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            Header = _currentTitle = sender.DocumentTitle ?? "Загрузка...";          
        }

        /// <summary>
        /// Метод, вызываемый при окончательной прорисовке. Метод вызывается четвёртым. 
        /// </summary>
        /// <param name="sender">WebView, вызвавший это событие</param>
        /// <param name="args">Сопроводительная информация о случившемся событии</param>
        private void _content_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Header = _currentTitle = sender.DocumentTitle ?? "Загрузка...";
        }

        /// <summary>
        /// Метод, вызываемый при ошибке навигации. Проверяет, есть ли интернет и в случае его отсутствия отправляет нас на страницу с динозавриком
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _content_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {

            ConnectionProfile getter = NetworkInformation.GetInternetConnectionProfile();
            var conn = getter?.GetNetworkConnectivityLevel();
            if (e.WebErrorStatus == Windows.Web.WebErrorStatus.NotFound && conn == null ? true : conn == NetworkConnectivityLevel.None)
                WebContent.Navigate(new Uri("ms-appx-web:///Dino/index.html"));

        }
        #endregion

        /// <summary>
        /// Очищает класс после удаления из списка вкладок. Вызывать необходимо вручную
        /// </summary>
        #region Код сборщика мусора
        /// <summary>
        /// Принудительный запуск сборщика мусора для удаления вкладки из оперативной памяти и её освобождение (а также отключение WebView)
        /// </summary>
        public void Dispose()
        {
            WebContent.DOMContentLoaded -= _content_DOMContentLoaded;
            WebContent.NavigationCompleted -= _content_NavigationCompleted;
            WebContent.NavigationFailed -= _content_NavigationFailed;
            WebContent.NavigationStarting -= _content_NavigationStarting;
            WebContent.ContentLoading -= _content_ContentLoading;
            WebContent.NewWindowRequested -= _content_NewWindowRequested;
            WebContent.NavigateToString("null");           //Обнуление информации во вкладке
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion
    }
}
