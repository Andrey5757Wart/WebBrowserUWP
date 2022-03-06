using GalaSoft.MvvmLight.Command;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using WebBrowserUWP.CustomTab;

namespace WebBrowserUWP.ViewModels
{
    /// <summary>
    /// ВьюМодель главной страницы приложения (и в нашем случае единственная страница в приложении)
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
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
        /// Различные классы, списки, текстовые и прочие переменные для реализации логики класса
        /// </summary>
        #region Различные переменные для работы класса

        /// <summary>
        /// Приватный нотифицируемый список 
        /// </summary>
        private ObservableCollection<CustomTabViewItem> _tabsView { get; set; }

        /// <summary>
        /// Публичный нотифицируемый список с нотификацией об изменении
        /// </summary>
        public ObservableCollection<CustomTabViewItem> TabsView
        {
            get
            {
                return _tabsView ?? (_tabsView = new ObservableCollection<CustomTabViewItem>() { new CustomTabViewItem() }) ;
            }
        }

        public CustomTabViewItem _selectedItem { get; set; }

        /// <summary>
        /// Выбранная в данный момент вкладка
        /// </summary>
        public CustomTabViewItem SelectedItem { get { return _selectedItem; } set { _selectedItem = value; 
                NotifyPropertyChanged(nameof(SelectedItem)); } }

        #endregion

        /// <summary>
        /// Команды, вызываемые интерфейсом для обработки тех или иных событиый
        /// </summary>
        #region Команды модели

        

        private RelayCommand _addTabCommand { get; set; }

        /// <summary>
        /// Команда добавления новой вкладки
        /// </summary>
        public RelayCommand AddTabCommand
        {
            get
            {
                return _addTabCommand ?? (_addTabCommand = new RelayCommand(() =>
                {

                    CustomTabViewItem newitem = new CustomTabViewItem();
                    TabsView.Add(newitem);
                    SelectedItem = newitem;
                }));
            }
        }

        private RelayCommand<TabViewTabCloseRequestedEventArgs> _removeTabCommand { get; set; }

        /// <summary>
        /// Команда удаления вкладки
        /// </summary>
        public RelayCommand<TabViewTabCloseRequestedEventArgs> RemoveTabCommand {
            get
            {
                return _removeTabCommand ?? (_removeTabCommand = new RelayCommand<TabViewTabCloseRequestedEventArgs>((TabViewTabCloseRequestedEventArgs e) =>
                {
                    CustomTabViewItem item = e.Item as CustomTabViewItem;
                    TabsView.Remove(item);
                    item.Dispose();
                    item = null;
                }));
            }
        }  

        #endregion

        /// <summary>
        /// Стандартный конструктор класса ViewModel страницы MainPage
        /// </summary>
        public MainPageViewModel()
        {            
            SelectedItem = TabsView.ElementAt(0);              
        }
    }
}
