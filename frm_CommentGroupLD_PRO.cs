﻿using Newtonsoft.Json;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NinjaSystem
{
    public partial class frm_CommentGroupLD_PRO : Form
    {
        public frm_CommentGroupLD_PRO(List<Account> list_acc, frm_MainLD_PRO frm_main)
        {
            InitializeComponent();
            this.list_acc = list_acc;
            this.frm_main = frm_main;
        }
        xProxyController xcontroller = new xProxyController();
        frm_MainLD_PRO frm_main;
        List<Account> list_acc;
        bool stop = false;
        object synAcc = new object();
        int countComplete = 0;
      
        ninjaDroidHelper droid = new ninjaDroidHelper();
        List<DataGridViewRow> list_dr = new List<DataGridViewRow>();
        Thread thread_1;
        static object syncObjUID = new object();
        //List<string> list_uid = new List<string>();
        Random rd = new Random();
        List<int> list_tuongtac = new List<int>();

        LDController ld = new LDController();
        List<string> list_uid = new List<string>();
        Random rdom = new Random();
        object synUID = new object();
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        private void frmCommentGroupLD_Load(object sender, EventArgs e)
        {
            method_LoadAccount();
            method_Config();
            if (SettingTool.configld.language == "English")
            {
                setupLanguage();
            }
        }
        public void sendLogs(string string_15)
        {
            MethodInvoker method = null;
            Class31 class2 = new Class31
            {
                richTextBox_0 = richLogs,
                string_0 = string_15
            };
            try
            {
                if (method == null)
                {
                    method = new MethodInvoker(class2.method_0);
                }
                this.Invoke(method);
            }
            catch (Exception)
            {
            }
        }

        private bool setupCauHinh()
        {

            return true;
        }
        private void method_Config()
        {
           
        }
        private void method_LoadAccount()
        {
            foreach (Account acc in list_acc)
            {
                method_Datagridview(acc);
            }
        }
        private void method_Datagridview(Account acc)
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
                cell2.Value = acc.ldid;
                dataGridViewRow.Cells.Add(cell2);

                DataGridViewTextBoxCell cell5 = new DataGridViewTextBoxCell();
                cell5.Value = acc.id;
                dataGridViewRow.Cells.Add(cell5);

                DataGridViewTextBoxCell cell6 = new DataGridViewTextBoxCell();
                cell6.Value = acc.name;
                dataGridViewRow.Cells.Add(cell6);

                DataGridViewTextBoxCell cell7 = new DataGridViewTextBoxCell();
                cell7.Value = acc.TrangThai;
                dataGridViewRow.Cells.Add(cell7);

                DataGridViewTextBoxCell cell9 = new DataGridViewTextBoxCell();
                cell9.Value = acc.Thongbao;
                dataGridViewRow.Cells.Add(cell9);
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

        private void btnRun_Click(object sender, EventArgs e)
        {

            if (txtID.Text == "")
            {
                MessageBox.Show("Hãy nhập Id của Group");
                return;
            }


            if (txtComment.Text == "")
            {
                MessageBox.Show("Hãy nhập nội dung bình luận");
                return;
            }

            ClearMessage();

            string pathlog = Application.StartupPath + "\\logs";
            if (!Directory.Exists(pathlog))
            {
                Directory.CreateDirectory(pathlog);
            }
            startTuongTac();
        }
        private void startTuongTac()
        {
            tokenSource = new CancellationTokenSource();
            stop = false;
            pibStatus.Visible = true;
            //chon cau hinh
            if (setupCauHinh())
            {
                list_dr = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvUser.Rows)
                {
                    if ((bool)row.Cells[0].Value)
                    {
                        Account acc = (Account)row.Tag;
                        list_dr.Add(row);

                    }
                }

                if (list_dr.Count == 0)
                {
                    MessageBox.Show("Hãy chọn những tài khoản cần chạy");
                    pibStatus.Visible = false;
                    return;
                }
                else
                {
                    pibStatus.Visible = true;
                    if (changeIpHelper.checkWaitAny())
                    {
                        this.thread_1 = new Thread(new ThreadStart(this.runLoginWaitAny));
                        thread_1.IsBackground = true;
                        this.thread_1.Start();
                    }
                    else
                    {
                        this.thread_1 = new Thread(new ThreadStart(this.runTuongTac));
                        thread_1.IsBackground = true;
                        this.thread_1.Start();
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn cấu hình trước khi chạy tương tác", "Thông báo");
            }
        }
        private void resetproxy()
        {
            try
            {
                if (changeIpHelper.checkGetProxyWaitAny())
                {
                    List<string> lsproxy = new List<string>();
                    xProxyController xcontroller = new xProxyController();

                    foreach (userLD ldopen in this.frm_main.list_ldopen)
                    {
                        string[] proxy = ldopen.ip_proxy.Split('-');
                        if (SettingTool.configld.typeip == 6 || SettingTool.configld.typeip == 8)
                        {
                            string proxyapi = xcontroller.getApifromproxy(proxy[0].Trim());//chuyen doi thanh api
                            lsproxy.Add(proxyapi);

                        }
                        else
                            lsproxy.Add(proxy[0].Trim());
                    }

                    int runs = SettingTool.list_running.Count;
                    lock (SettingTool.lockobj)
                    {
                        while (runs > 0)
                        {
                            foreach (string proxy in SettingTool.list_running)
                            {
                                if (!lsproxy.Contains(proxy))
                                {
                                    foreach (xproxy xp in SettingTool.list_freeproxy)
                                    {
                                        if (xp.proxy == proxy)
                                        {
                                            xp.proxysucess = "";
                                            xp.use = false;
                                        }
                                    }
                                    SettingTool.list_running.Remove(proxy);
                                    break;
                                }
                            }

                            runs--;
                        }
                    }
                }

            }
            catch
            {

            }
        }
        private void runLoginWaitAny()
        {
            var token = tokenSource.Token;
            xProxyController xcontroller = new xProxyController();
            int numthread = SettingTool.configld.numthread;
            if (numthread > list_dr.Count)
            {
                numthread = list_dr.Count;
            }
            Task[] list_task = TaskController.createTask(numthread);
            xcontroller.createProxy(numthread);
            int maxproxy = 0;
        Lb_quayvong:
            if (list_dr.Count > 0)
            {
                #region doi ip truoc khi mo ld
                List<string> list_proxy = new List<string>();

                //if (SettingTool.configld.typeip == 6)
                //{
                //Lb_Start:
                //    method_log("Bắt đầu đổi ip bằng Tinsoft");
                //    TinsoftResult tinsoftresult = changeIpHelper.method_ChangeTinSoft(SettingTool.configld.apitinsoft);

                //    foreach (TinSoftModel ts in tinsoftresult.list_model)
                //    {
                //        method_log(String.Format("Api {0} - IP {1} - Next change {2} - Timout - {3}", ts.api, ts.proxy, ts.next_change, ts.timeout));
                //    }
                //    if (tinsoftresult.list_proxy.Count <= 0)
                //    {

                //        method_log("Lỗi lấy proxy tinsoft.Tiếp tục request");
                //        Thread.Sleep(5000);
                //        goto Lb_Start;
                //    }
                //    else
                //    {
                //        list_proxy = tinsoftresult.list_proxy;
                //    }

                //}
                //else
                {
                    if (SettingTool.configld.typeip == 7)
                    {
                        //ResultRequest kq = changeIpHelper.connectBeforeOpen(richLogs);
                        //if (kq.status)
                        //{
                        //    list_proxy = SettingTool.list_xproxy;
                        //    method_log("Total Proxy: " + list_proxy.Count);
                        //}
                    }
                    else
                    {
                        if (SettingTool.configld.typeip == 2 || SettingTool.configld.typeip == 3)
                        {
                            ResultRequest kq = changeIpHelper.connectBeforeOpen(richLogs);
                            if (kq.status)
                            {
                                method_log(kq.data);
                            }
                            else
                            {
                                method_log("Lỗi đổi ip: " + kq.data);
                                return;
                            }
                            //hma
                        }
                    }
                }
                #endregion
                int i = 0;
                object synDevice = new object();
                while (TaskController.checkAvailableTask(list_task))
                {
                    if (list_dr.Count <= 0)
                    {
                        break;
                    }
                    if (token.IsCancellationRequested == false)
                    {
                        int index = TaskController.getAvailableTask(list_task);
                        if (index >= 0)
                        {
                            if (changeIpHelper.checkGetProxyWaitAny())
                            {
                                method_log("Đang lấy IP ");
                                string proxy = xcontroller.getProxy();
                                if (proxy != null)
                                {
                                    method_log("Đã lấy IP: " + proxy);
                                    string ldid = changeIpHelper.getLD();
                                    if (!string.IsNullOrEmpty(proxy))
                                    {
                                        Task task = Task.Factory.StartNew(() =>
                                        {
                                            if (list_dr.Count > 0)
                                            {
                                                    DataGridViewRow dr = new DataGridViewRow();
                                                    lock (synDevice)
                                                    {
                                                        dr = list_dr[0];
                                                        list_dr.Remove(dr);
                                                    }
                                                    method_Start(ldid, dr, proxy, token);
                                            }
                                            else
                                            {
                                                method_log("Đã hết tài khoản");
                                            }

                                        }, token);
                                        list_task[index] = task;
                                        Thread.Sleep(SettingTool.configld.timedelay * 1000);
                                    }
                                    else
                                    {
                                        xcontroller.finishProxy(proxy);
                                        method_log("Tất cả LD đang được sử dụng");
                                    }
                                }
                                else
                                {
                                    Thread.Sleep(3000);
                                    method_log("Proxy chưa sẵn sàng: " + proxy);
                                    maxproxy++;
                                    if (maxproxy > 50)
                                    {
                                        maxproxy = 0;
                                        resetproxy();
                                    }
                                }
                            }
                            else
                            {

                                Task task = Task.Factory.StartNew(() =>
                                {
                                    string proxy = "";
                                    string ldid = changeIpHelper.getLD();
                                    if (ldid != "-1")
                                    {
                                        if (list_dr.Count > 0)
                                        {
                                            DataGridViewRow dr = new DataGridViewRow();
                                            lock (synDevice)
                                            {
                                                dr = list_dr[0];
                                                list_dr.Remove(dr);

                                                if (list_proxy.Count > 0)
                                                {
                                                    proxy = list_proxy[i];
                                                    i++;
                                                    if (i >= list_proxy.Count)
                                                    {
                                                        i = 0;
                                                    }

                                                }
                                            }
                                            method_Start(ldid, dr, proxy, token);


                                        }
                                        else
                                        {
                                            method_log("Đã hết tài khoản");
                                        }
                                    }
                                    else
                                    {
                                        method_log("Tất cả LD đang được sử dụng");
                                    }
                                }, token);
                                list_task[index] = task;
                                Thread.Sleep(SettingTool.configld.timedelay * 1000);
                            }
                        }


                    }
                    else
                    {
                        break;
                    }
                }
                tokenSource.CancelAfter(SettingTool.configld.timeout * 60000);
                try
                {
                    Task.WaitAny(list_task);
                }
                catch
                { }
                if (list_dr.Count > 0 && stop == false)
                {
                    goto Lb_quayvong;
                }
                else
                {
                    try
                    {
                        Task.WaitAll(list_task);
                    }
                    catch
                    { }
                    if (chkLoopRun.Checked)
                    {
                        method_log(String.Format("Vui lòng đợi {0} phút để tiếp tục tương tác", numWait.Value.ToString()));
                        Thread.Sleep((int)numWait.Value * 60000);
                        list_dr = new List<DataGridViewRow>();
                        foreach (DataGridViewRow row in dgvUser.Rows)
                        {
                            if ((bool)row.Cells[0].Value)
                            {
                                Account acc = (Account)row.Tag;
                                list_dr.Add(row);

                            }
                        }

                        if (list_dr.Count > 0)
                            goto Lb_quayvong;
                        else
                            method_Stop();
                    }
                    else
                        method_Stop();
                }
            }

        }
        private void runTuongTac()
        {
            var token = tokenSource.Token;
            xProxyController xcontroller = new xProxyController();
        Lb_quayvong:
            int numthread = SettingTool.configld.numthread;
            if (numthread > list_dr.Count)
            {
                numthread = list_dr.Count;
            }

            if (list_dr.Count > 0)
            {

                #region doi ip truoc khi mo ld
                List<string> list_proxy = new List<string>();

                if (SettingTool.configld.typeip == 6)
                {

                Lb_Start:
                    method_log("Bắt đầu đổi ip bằng Tinsoft");
                    TinsoftResult tinsoftresult = changeIpHelper.method_ChangeTinSoft(SettingTool.configld.apitinsoft);

                    foreach (TinSoftModel ts in tinsoftresult.list_model)
                    {
                        method_log(String.Format("Api {0} - IP {1} - Next change {2} - Timout - {3} - {4}", ts.api, ts.proxy, ts.next_change, ts.timeout, ts.description));
                    }
                    if (tinsoftresult.list_proxy.Count <= 0)
                    {

                        method_log("Lỗi lấy proxy tinsoft.Tiếp tục request");
                        Thread.Sleep(5000);
                        goto Lb_Start;
                    }
                    else
                    {
                        list_proxy = tinsoftresult.list_proxy;
                    }

                }
                else if (SettingTool.configld.typeip == 8)
                {
                Lb_Start:
                    method_log("Bắt đầu đổi ip bằng TM proxy");
                    TMproxyResult tinsoftresult = changeIpHelper.method_ChangeTMproxy(SettingTool.configld.apiTMproxy);

                    foreach (TMproxyModel ts in tinsoftresult.list_model)
                    {
                        method_log(String.Format("Api {0} - IP {1} - Next change {2} - Timout - {3}", ts.api, ts.proxy, ts.next_request.ToString(), ts.timeout));
                    }
                    if (tinsoftresult.list_proxy.Count <= 0)
                    {

                        method_log("Lỗi lấy TM proxy.Tiếp tục request");
                        Thread.Sleep(5000);
                        goto Lb_Start;
                    }
                    else
                    {
                        list_proxy = tinsoftresult.list_proxy;
                    }
                }
                else
                {
                     if (SettingTool.configld.typeip == 7)
                    {
                        ResultRequest kq = changeIpHelper.connectBeforeOpen(richLogs);
                        if (kq.status)
                        {
                            list_proxy = SettingTool.list_xproxy;
                            method_log("Total Proxy: " + list_proxy.Count);
                        }
                    }
                    else
                    {
                        if (SettingTool.configld.typeip == 2 || SettingTool.configld.typeip == 3)
                        {
                            ResultRequest kq = changeIpHelper.connectBeforeOpen(richLogs);
                            if (kq.status)
                            {
                                method_log(kq.data);
                            }
                            else
                            {
                                method_log("Lỗi đổi ip: " + kq.data);
                                return;
                            }
                            //hma
                        }
                    }
                }
                #endregion
                int i = 0;

                object synDevice = new object();
                Task[] tasks = new Task[numthread];
                for (int p = 0; p < numthread; p++)
                {
                    int t = p;
                    tasks[t] = Task.Factory.StartNew(() =>
                    {
                        if (token.IsCancellationRequested == false)
                        {
                            string proxy = "";
                            string ldid = changeIpHelper.getLD();
                            if (ldid != "-1")
                            {
                                if (list_dr.Count > 0)
                                {
                                    DataGridViewRow dr = new DataGridViewRow();
                                    lock (synDevice)
                                    {
                                        dr = list_dr[0];
                                        list_dr.Remove(dr);

                                        if (list_proxy.Count > 0)
                                        {
                                            proxy = list_proxy[i];
                                            i++;
                                            if (i >= list_proxy.Count)
                                            {
                                                i = 0;
                                            }

                                        }
                                    }
                                    
                                    method_Start(ldid, dr, proxy, token);
                                }
                                else
                                {
                                    method_log("Đã hết tài khoản");
                                }
                            }
                            else
                            {
                                method_log("Tất cả LD đang được sử dụng");
                            }
                        }
                    }, token);
                    Thread.Sleep(SettingTool.configld.timedelay * 1000);
                }
                tokenSource.CancelAfter(SettingTool.configld.timeout * 60000);
                try
                {
                    Task.WaitAll(tasks);
                }
                catch
                {
                    sendLogs("Đang xử lý dừng comment");
                }

                if (list_dr.Count > 0)
                {
                    goto Lb_quayvong;
                }
                else
                {
                    if (chkLoopRun.Checked)
                    {
                        method_log(String.Format("Vui lòng đợi {0} phút để tiếp tục tương tác", numWait.Value.ToString()));
                        Thread.Sleep((int)numWait.Value * 60000);
                        list_dr = new List<DataGridViewRow>();
                        foreach (DataGridViewRow row in dgvUser.Rows)
                        {
                            if ((bool)row.Cells[0].Value)
                            {
                                Account acc = (Account)row.Tag;
                                list_dr.Add(row);

                            }
                        }
                        if (list_dr.Count > 0)
                            goto Lb_quayvong;
                        else
                            method_Stop();
                    }
                    else
                        method_Stop();
                }
            }

        }
        private void method_Stop()
        {
            pibStatus.Visible = false;
            stop = true;
            if (thread_1 != null)
                thread_1.Abort();
        }
        private void method_Start(string ldID, DataGridViewRow dr, string proxy, CancellationToken token)
        {
            method_log("Open LDPlayer Id: " + ldID);

            Account acc = (Account)dr.Tag;
            dr.Cells["clID"].Value = ldID;
            acc.ldid = ldID;
            dr.Cells["Message"].Value = "Restore Data LD";
            ld.setupLD(acc, ldID);
            dr.Cells["Message"].Value = "Open LD";

            userLD u = frm_main.checkExits(ldID);
            frm_main.addLDToPanel(u);
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
                    method_log("Không kết nối được với LD: " + ldID);
                    return;
                }
            }
            ld.restoredatafb(acc.ldid, acc.id);
            try
            {
                DetailLD_BLL detail_bll = new DetailLD_BLL();
                DetailLDModel detailLd = new DetailLDModel();
                #region doi ip sau khi mo ld thanh cong
                if (string.IsNullOrEmpty(proxy) == false)
                {
                    u.setDevice(ldID, acc.id, proxy);
                    u.setStatus(ldID, "Change proxy : " + proxy);
                    if (SettingTool.configld.sock5)
                    {
                        SettingTool.configld.proxytype = "socks5";
                        ld.setProxyAuthentica_proxydroid(ldID, proxy, token);
                        string yourip = ld.checkIPSock5(proxy, 3);
                        u.setDevice(ldID, proxy + " - " + yourip);

                        if (SettingTool.configld.checkproxy)
                        {
                            if (string.IsNullOrEmpty(yourip))
                            {
                                sendLogs("Tắt LD do không lấy được ip public proxy: " + proxy);
                                goto Lb_Finish;

                            }
                        }

                    }
                    else
                    {
                        changeIpHelper.changeProxyAdb(ldID, proxy);
                        //check ip
                        string yourip = ld.checkIP(proxy, 3);
                        if (SettingTool.configld.checkproxy)
                        {
                            if (string.IsNullOrEmpty(yourip))
                            {
                                sendLogs("Tắt LD do không lấy được ip public proxy: " + proxy);
                                goto Lb_Finish;

                            }
                        }
                        u.setDevice(ldID, acc.id, proxy + " - " + yourip);
                    }
                }

                changeIpHelper.connectAfterOpen(u, richLogs, ldID, acc, token);
                #endregion
                Random rd = new Random();
               

                try
                {
                  
                        dr.Cells["Message"].Value = "Running";
                        changeColor(dr, Color.Yellow);
                     
                        ld.killApp(acc.ldid, "com.facebook.katana");
                        ld.restoredatafb(acc.ldid, acc.id);
                        ld.runApp(acc.ldid, "com.facebook.katana");
                        ld.checkOpenFacebookFinish(u, acc.ldid);
                        dr.Cells["Message"].Value = "Login Facebook";
                        u.setStatus(ldID, "Đăng nhập Facebook...");
                        ld.check_Facebook_has_stopped(u,acc.ldid, acc, token);
                        //bool status = ld.checkIsLogin(acc);
                        //if (status)
                        //{
                        //    status = true;
                        //}
                        //else
                        //{
                        //    ld.loginAvatarLD(acc);
                        //    status = ld.loginFacebookLD(acc, token);
                        //}
                        ld.setKeyboard(ldID);
                        bool status = ld.loginFacebookTuongTac(u, acc, token);
                        if (status)
                        {
                            if (stop)
                            {
                                goto Lb_Finish;
                            }
                            u.setStatus(ldID, "Đăng nhập Facebook thành công...");
                            dr.Cells["Message"].Value = "Đăng nhập thành công";
                            acc.TrangThai = "Live";
                            dr.Cells["clStatus"].Value = acc.TrangThai;
                            #region bat dau tuong tac
                            string message = "";
                            int delay = (int)numDelay.Value;
                            acc.app = "com.facebook.katana";

                            DeviceData device = new DeviceData();
                            device.Serial = ldID;
                            u.setStatus(ldID, "Tương tác Group Id...");
                            dr.Cells["Message"].Value = "Tương tác Group Id";
                            message += ld.likecommentID(u, acc,dr, ldID, acc.app, (int)numLike.Value, (int)numComment.Value, chkLike.Checked, chkComment.Checked, txtID.Lines.ToList(), txtComment.Text, 2, delay,chkgidOfacc.Checked, token,(int)nummaxFail.Value,chkLuot.Checked, (int) numLuot.Value);

                            #endregion
                            dr.Cells["Message"].Value = message;
                            u.setStatus(ldID, "Đang backup dataprofile");
                            ld.Zip(acc, ldID);
                        }
                        else
                        {
                            u.setStatus(ldID, "Đăng nhập thất bại...");
                            dr.Cells["Message"].Value = "Đăng nhập thất bại";
                            dr.Cells["Status"].Value = "Die";
                            acc.TrangThai = "Die";

                        }
                        changeColor(dr, Color.White);
                        NguoiDung_Bll nguoidung = new NguoiDung_Bll();
                        nguoidung.updateNoti(acc);

                        dr.Cells[0].Value = false;
                     
                    }
                catch
                { }
              


            }
            catch { }
        Lb_Finish:
           // u.setStatus(ldID, "Backup Profile LD...");
            sendLogs("Tương tác thành công LD : " + ldID);
            if (string.IsNullOrEmpty(proxy) == false)
            {
                ld.setProxyAdb(ldID, ":0");
            }
            ld.quit(acc, ldID);
            frm_main.removeLDToPanel(u);
            if (changeIpHelper.checkGetProxyWaitAny())
            {
                xcontroller.finishProxy(proxy);
            }
        }
        void Delay(double delay)
        {
            double delayTime = 0;
            while (delayTime < delay)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                delayTime++;
            }
        }
        private void method_log(string string_15)
        {
            MethodInvoker method = null;
            Class31 class2 = new Class31
            {
                richTextBox_0 = richLogs,
                string_0 = string_15
            };
            try
            {
                if (method == null)
                {
                    method = new MethodInvoker(class2.method_0);
                }
                this.Invoke(method);
            }
            catch (Exception)
            {
            }
        }
        [CompilerGenerated]
        private sealed class Class31
        {
            public RichTextBox richTextBox_0;
            public string string_0;

            public void method_0()
            {
                try
                {
                    if (richTextBox_0.Lines.Length > 50)
                        richTextBox_0.Text = "";

                    if (this.string_0.Contains("being aborted"))
                    {
                        this.string_0 = "Luồng đang chạy bị tạm ngừng -> STOP !!!";
                    }
                    this.richTextBox_0.Text = string.Format("{0}:{1}\n", DateTime.Now.ToString("HH:mm:ss"), this.string_0) + this.richTextBox_0.Text;

                }
                catch { }
            }

        }
        private void ClearMessage()
        {
            for (int i = 0; i < dgvUser.Rows.Count; i++)
            {
                if ((bool)dgvUser.Rows[i].Cells[0].Value)
                    dgvUser.Rows[i].Cells["Message"].Value = "";

                DataGridViewRow dr = dgvUser.Rows[i];
                changeColor(dr, Color.White);

            }
        }
        private void btn_config_Click(object sender, EventArgs e)
        {

            frm_Config_PRO frm = new frm_Config_PRO();
            frm.ShowDialog();
            method_Config();
        }
        private sealed class Class34
        {
            public Color color_0;
            public DataGridViewRow dataGridViewRow_0;

            public void method_0()
            {
                this.dataGridViewRow_0.DefaultCellStyle.BackColor = this.color_0;
            }
        }
        private void changeColor(DataGridViewRow dataGridViewRow_0, Color color_0)
        {
            Class34 class2 = new Class34
            {
                dataGridViewRow_0 = dataGridViewRow_0,
                color_0 = color_0
            };
            this.Invoke(new MethodInvoker(class2.method_0));
        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            tokenSource.Cancel();
            method_Stop();
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

        private void chọnDòngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row2 in this.dgvUser.SelectedRows)
            {
                row2.Cells[0].Value = true;
            }
        }
        private void setupLanguage()
        {
            this.Text = "Reaction into group id ";
            label3.Text = "List group ID (1 ID / 1 line)";
            label4.Text = "Content comnent {content1 | content2...}";
            label2.Text = "seconds";
            label10.Text = "minutes";
            chkLoopRun.Text = "Restart action delay ";
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
