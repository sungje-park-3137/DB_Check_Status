using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_SQL_NET_Framework.Model;
using WPF_SQL_NET_Framework.ViewModel;

namespace WPF_SQL_NET_Framework
{
    /// <summary>
    /// DB_LogIN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DB_LogIN : Window
    {
        //private DB_ViewModel _viewmodel;
        public DB_LogIN()
        {
            InitializeComponent();
            //_viewmodel = new DB_ViewModel();
            //this.DataContext = _viewmodel;
        }

        // password box 이거 뭐 동작이 이상한데?
        private void DB_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //if (sender is PasswordBox pb && DataContext is Main_ViewModel vm)
            //{
            //    var selectList = vm.DB_Info.ElementAt(vm.SelectTabIndex);

            //    if (string.IsNullOrEmpty(selectList.DB_Password) == true)
            //    {
            //        selectList.DB_Password = pb.Password;
            //    }
            //    else
            //    {
            //        //
            //    }
            //}
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (sender is TabControl tab && DataContext is Main_ViewModel vm)
            //{
            //    var selectList = vm.DB_Info.ElementAt(vm.SelectTabIndex);

            //    ;
            //}
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is VM_DBLogin vm)
            {
                if (vm.Server_List.Count() < 1)
                {
                    M_DBLogin _temp = new M_DBLogin();
                    _temp.Add_List();
                }
            }
        }
    }
}
