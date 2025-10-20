using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF_SQL_NET_Framework.Command;
using WPF_SQL_NET_Framework.Model;

namespace WPF_SQL_NET_Framework.ViewModel
{
    public partial class Main_ViewModel : NotifyProperty
    {
        //private ObservableCollection<TB_CLSLVCD> _EF_RcvTable = new ObservableCollection<TB_CLSLVCD>();
        //public ObservableCollection<TB_CLSLVCD> EF_RcvTable
        //{
        //    get { return _EF_RcvTable; }
        //    set { _EF_RcvTable = value; OnPropertyChanged(nameof(EF_RcvTable)); }
        //}

        private void Init_EF()
        {
            EF_Data efdata = new EF_Data
            {
                EF_Name = "박성제",
                EF_ID = "psj",
                EF_PW = "password",
            };

            //DB_EF_Info.Add(efdata);
        }

        private void DB_EF_Read()
        {
            //using (var temp = new GS425_IEE_DBEntities())
            //{
            //    var curInfo = temp.TB_CLSLVCD.ToList();

            //    EF_RcvTable.Clear();

            //    foreach (var item in curInfo)
            //    {
            //        EF_RcvTable.Add(item);
            //    }
            //}


            //EF_DB_DATA temp = new EF_DB_DATA();

            //var info = temp.DB_TEST_Address.ToList();
            //EF_RcvTable.Clear(); 

            //foreach (var item in info)
            //{
            //    EF_RcvTable.Add(item);
            //}
        }

        private void DB_EF_Create()
        {
            //using (var temp = new EF_DB_DATA())
            //{
            //    var curInfo = temp.DB_TEST_Address.ToList();

            //    foreach (var item in DB_EF_Info)
            //    {
            //        DB_TEST_Address addinfo = new DB_TEST_Address();

            //        //addinfo.ID = curInfo.Count() + 1;
            //        addinfo.NAME = item.EF_Name;
            //        addinfo.USERNAME = item.EF_ID;
            //        addinfo.PASSWORD = item.EF_PW;
            //        addinfo.ADMIN = item.EF_Admin;

            //        temp.DB_TEST_Address.Add(addinfo);
            //        temp.SaveChanges();
            //    }
            //}

            //DB_EF_Read();
        }

        private void DB_EF_Del()
        {
            //using (var temp = new EF_DB_DATA())
            //{
            //    foreach (var item in DB_EF_Info)
            //    {
            //        var target = temp.DB_TEST_Address
            //            .FirstOrDefault(x => x.NAME == item.EF_Name
            //                              && x.USERNAME == item.EF_ID
            //                              && x.PASSWORD == item.EF_PW);

            //        if (target != null)
            //        {
            //            temp.DB_TEST_Address.Remove(target);
            //        }
            //        else
            //        {
            //            MessageBox.Show("삭제 시도 중 오류: 이름, ID, PW가 일치한 데이터가 존재하지 않습니다.");
            //        }
            //    }
            //    temp.SaveChanges();
            //}

            //DB_EF_Read();
        }

        private void DB_EF_Update()
        {
            //using (var temp = new EF_DB_DATA())
            //{
            //    var curInfo = temp.DB_TEST_Address.ToList();
            //    var targetInfo = curInfo.Where(x => x.ID == EF_SelectTable.ID).ToList();

            //    foreach (var updateItem in DB_EF_Info)
            //    {
            //        foreach (var item in targetInfo)
            //        {
            //            item.NAME = updateItem.EF_Name;
            //            item.USERNAME = updateItem.EF_ID;
            //            item.PASSWORD = updateItem.EF_PW;
            //            item.ADMIN = updateItem.EF_Admin;
            //        }
            //    }

            //    temp.SaveChanges();
            //}

            //DB_EF_Read();
        }

        private void Show_SelectItem()
        {
            //// EF_SelectTable
            //var curItem = DB_EF_Info.FirstOrDefault();

            //if (EF_SelectTable != null)
            //{
            //    curItem.EF_Name = EF_SelectTable.NAME;
            //    curItem.EF_ID = EF_SelectTable.USERNAME;
            //    curItem.EF_PW = EF_SelectTable.PASSWORD;
            //    curItem.EF_Admin = EF_SelectTable.ADMIN;
            //}

            
        }
    }
}
