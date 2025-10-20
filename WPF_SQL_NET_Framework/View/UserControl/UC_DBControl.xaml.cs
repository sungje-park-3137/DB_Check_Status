using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using WPF_SQL_NET_Framework.ViewModel;

namespace WPF_SQL_NET_Framework.View
{
    public partial class UC_DBControl : System.Windows.Controls.UserControl
    {
        public UC_DBControl()
        {
            InitializeComponent();
            
            if (this.DataContext is VM_DBControl vm)
            {
                vm.Logevent.CollectionChanged += CollectionChange_Logitem;
            }
        }
        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (e.Action == NotifyCollectionChangedAction.Add)
            //{
            //    // 마지막 아이템으로 스크롤
            //    LB_LOG.ScrollIntoView(e.NewItems[0]);
            //}
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //if (DG_Log.Items.Count > 0)
            //{
            //    var lastitem = DG_Log.Items[DG_Log.Items.Count - 1];
            //    DG_Log.ScrollIntoView(lastitem);
            //}
        }

        private void DG_Log_CurrentCellChanged(object sender, EventArgs e)
        {

        }

        private void CollectionChange_Logitem(object sender, NotifyCollectionChangedEventArgs e)
        {
            //if (DG_Log.Items.Count > 0)
            //{
            //    var lastitem = DG_Log.Items[DG_Log.Items.Count - 1];
            //    DG_Log.ScrollIntoView(lastitem);
            //}

            var border = VisualTreeHelper.GetChild(DG_Log, 0) as Border;
            if (border != null)
            {
                var scroll = border.Child as ScrollViewer;
                if (scroll != null)
                {
                    scroll.ScrollToEnd();
                }
            }
        }

        private void DG_Log_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is VM_DBControl vm)
            {
                vm.AutoCheck = true;
            }
        }
    }
}

