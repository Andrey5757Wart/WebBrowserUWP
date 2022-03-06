using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;


namespace WebBrowserUWP.ViewModels
{
    /// <summary>
    /// Класс, реализующий работу паттерна проектирования MVVM, хранит в специальном стеке все классы
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Стандартная инициализация локатора
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainPageViewModel>();       
        }

        ///ВьюМодели, по которым интерфейс общается с Моделями данных
        #region ВьюМодели приложения
        /// <summary>
        /// Инициализация модели главной страницы (MainPage) 
        /// </summary>
        public static MainPageViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainPageViewModel>();
            }
        }

      
        #endregion

    }
}