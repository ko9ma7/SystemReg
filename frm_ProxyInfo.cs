﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NinjaSystem
{
    public partial class frm_ProxyInfo : Form
    {
        public frm_ProxyInfo(List<DetailLDModel> list_detail, frm_MainLD frm)
        {
            InitializeComponent();
            this.list_detail = list_detail;
            this.frm_main = frm;
        }
        
       
        DetailLDModel model = new DetailLDModel();

        List<DetailLDModel> list_detail;
        CancellationTokenSource tokenSource;
        bool stop;
        List<DataGridViewRow> list_dr;
        Thread thread_1;
        frm_MainLD frm_main;
        private void btnTao_Click(object sender, EventArgs e)
        {
            stop = false;
            tokenSource = new CancellationTokenSource();
                        
            startTuongTac();
        }
        private void startTuongTac()
        {
            stop = false;

           
                list_dr = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvUser.Rows)
                {
                    if ((bool)row.Cells[0].Value)
                    { 
                        list_dr.Add(row);
                       
                    }
                } 
                if (list_dr.Count == 0)
                {
                    MessageBox.Show("Hãy chọn những LDPlayer cần chạy");
                    return;
                }
                else
                {

                    this.thread_1 = new Thread(new ThreadStart(this.setProxy));
                    thread_1.IsBackground = true;
                    this.thread_1.Start();
                }
            
        }
        private void setProxy()
        {
            //List<PositionLD> lsPosition = new List<PositionLD>();
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
        Lb_quayvong:
            int numthread = SettingTool.configld.numthread;
            if (numthread > list_dr.Count)
            {
                numthread = list_dr.Count;
            }
            if (list_dr.Count > 0)
            {
                object synDevice = new object();
                Task[] tasks = new Task[numthread];
                for (int p = 0; p < numthread; p++)
                {
                    int t = p;
                    tasks[t] = Task.Factory.StartNew(() =>
                    {
                        if(list_dr.Count>0)
                        {
                            DataGridViewRow dr = list_dr[0];
                            list_dr.Remove(dr);
                            runsetProxy(dr, token);
                        } 
                      
                    }, token);
                    Thread.Sleep(SettingTool.configld.timedelay * 1000);
                }
                try
                {
                    Task.WaitAll(tasks);
                }
                catch
                {
                }

                if (list_dr.Count > 0 && stop == false)
                {
                    goto Lb_quayvong;
                } 
            }

        }
        private void runsetProxy(DataGridViewRow dr, CancellationToken token)
        {
            DetailLDModel model = (DetailLDModel)dr.Tag;
            string ldID = model.LDID.ToString();
            LDController ld = new LDController();
            string proxy = dr.Cells["clproxy"].Value.ToString();

            string[] arrproxy = proxy.Split(':');
            if (arrproxy.Length <=1)
            {
                dr.Cells["Message"].Value = "Vui lòng nhập đúng proxy";
                
            }
            else
            {

                userLD u = frm_main.checkExits(ldID);
                frm_main.addLDToPanel(u);
                u.setStatus(ldID, "Open Ldplayer: " + ldID);
                if (ld.launchSetPosion(ldID, u, token))
                {
                    u.setStatus(ldID, "Kết nối thành công LD...");
                }
                else
                {
                    if (ld.autoRunLDSetPosition(ldID, u, token))
                    {
                        u.setStatus(ldID, "Kết nối thành công LD...");

                    }
                    else
                    {
                        u.setStatus(ldID, "Disconnected...");
                        goto Lb_Finish;
                    }
                }

                if (arrproxy.Length == 2)
                {

                    if (ld.setProxyAdb(ldID, proxy))
                    {
                        dr.Cells["Message"].Value = "Hoàn thành thiết lập proxy";
                    }
                    else
                        dr.Cells["Message"].Value = "Lỗi thiết lập proxy";
                }
                else
                {
                    //kiem tra app proxy
                    if (ld.checkAppProxy(ldID) == false)
                    {
                        string path = Application.StartupPath + "\\App\\Proxy.apk";
                        if (File.Exists(path))
                        {
                            u.setStatus(ldID, "Đang cài app Proxy...");
                            ld.installApp(ldID, path);
                            Thread.Sleep(15000);
                        }
                        else
                        {
                            u.setStatus(ldID, "Chưa cài app proxy...");
                            dr.Cells["clStatus"].Value = "Chưa cài app proxy";
                            return;
                        }
                    }
                    u.setStatus(ldID, "Bắt đầu thay đổi proxy");

                    if (ld.setProxyAuthentica(ldID, proxy, token))
                    {
                        u.setStatus(ldID, "Hoàn thành đổi proxy");
                        dr.Cells["Message"].Value = "Hoàn thành thiết lập proxy";
                    }
                    else
                    {
                        dr.Cells["Message"].Value = "Lỗi thiết lập proxy";
                    }
                }

            Lb_Finish:
                ld.quit(ldID);
                frm_main.removeLDToPanel(u);
            }
        }


     

        private void frmProxyInfo_Load(object sender, EventArgs e)
        {
            method_LoadAccount(list_detail);
        }
        private void method_LoadAccount(List<DetailLDModel> list_ld)
        {
            //Data dt = new Data();
            //List<DetailLDModel> ls = dt.selectDetailLD("select * FROM DetailLD order by ldid  ");
            foreach (DetailLDModel acc in list_ld)
            {
                method_Datagridview(acc);
            }
        }
        private void method_Datagridview(DetailLDModel acc)
        {
            try
            {
                DataGridViewRow dataGridViewRow = new DataGridViewRow();

                DataGridViewCheckBoxCell check = new DataGridViewCheckBoxCell();
                check.Value = true;
                dataGridViewRow.Cells.Add(check);

                DataGridViewTextBoxCell cell1 = new DataGridViewTextBoxCell();
                cell1.Value = (dgvUser.Rows.Count + 1).ToString();
                dataGridViewRow.Cells.Add(cell1);

                DataGridViewTextBoxCell cell2 = new DataGridViewTextBoxCell();
                cell2.Value = acc.LDID;
                dataGridViewRow.Cells.Add(cell2);

                DataGridViewTextBoxCell cell3 = new DataGridViewTextBoxCell();
                cell3.Value = acc.Proxy;
                dataGridViewRow.Cells.Add(cell3);


                dataGridViewRow.Tag = acc;
                this.Invoke(new MethodInvoker(delegate()
                {
                    this.dgvUser.Rows.Add(dataGridViewRow);

                }));



            }
            catch
            {
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow dr in dgvUser.Rows)
            {
                DetailLDModel acc = (DetailLDModel)dr.Tag;
                acc.Proxy = ""; ;
                dr.Cells["clproxy"].Value = "";
                DetailLD_BLL detail_bll = new DetailLD_BLL();
                detail_bll.update(acc);
              //  LDController ld = new LDController();
               // ld.setProxyAdb(acc.LDID.ToString(), ":0");
                
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            bool check = false;
            if (checkBox2.Checked)
            {
                check = true;
            }
            else
                check = false;
            foreach (DataGridViewRow row2 in this.dgvUser.Rows)
            {
                row2.Cells[0].Value = check;

            }
        }

       

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string clipboardText = Clipboard.GetText(TextDataFormat.Text);
                List<string> ls_proxy = new List<string>();
                if (clipboardText.Length > 0)
                {
                    ls_proxy = clipboardText.Split('\n').ToList();
                    if (ls_proxy.Count == 0)
                    {
                        ls_proxy.Add(clipboardText.Trim());
                    }

                    //if (clipboardText.Contains("\r\n"))
                    //{
                    //    char[] chars = new char[2];
                    //    chars[0] = '\r';
                    //    chars[1] = '\n';
                    //    string[] lines = clipboardText.Split(chars);
                    //    ls_proxy = lines.ToList();
                    //}
                    //else if (clipboardText.Contains("\n"))
                    //{
                    //    char[] chars = new char[1];
                    //    chars[0] = '\n';
                    //    string[] lines = clipboardText.Split(chars);
                    //    ls_proxy = lines.ToList();
                    //}
                    ls_proxy.RemoveAll(x => x == "");
                    List<DataGridViewRow> list_dr = new List<DataGridViewRow>();
                    foreach (DataGridViewRow row in dgvUser.Rows)
                    {
                        if ((bool)row.Cells[0].Value)
                        {
                            list_dr.Add(row);
                        }
                    }

                    foreach (DataGridViewRow dr in list_dr)
                    {
                        if (ls_proxy.Count > 0)
                        {
                            DetailLDModel acc = (DetailLDModel)dr.Tag;
                            acc.Proxy = ls_proxy[0].Trim();
                            dr.Cells["clproxy"].Value = ls_proxy[0];
                            DetailLD_BLL detail_bll = new DetailLD_BLL();

                            detail_bll.update(acc);
                            ls_proxy.RemoveAt(0);
                        }
                        else
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Không có nội dung để Paste");
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.youtube.com/watch?v=PNo7AzMg8gw&t=19s");
        }

        private void dgvUser_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                foreach (DataGridViewRow row2 in dgvUser.SelectedRows)
                {
                    row2.Cells[0].Value = true;

                }
            }
        }


    }
}
