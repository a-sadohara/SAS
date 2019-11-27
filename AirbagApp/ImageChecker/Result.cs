﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ImageChecker.DTO;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class Result : Form
    {

        TargetInfoDto objTargetInfoDto;
        int intRow;
        bool m_dummy;

        public int intRet { get; set; }

        public Result(TargetInfoDto objTargetInfo,int intRowIndex, bool dummy = false)
        {
            InitializeComponent();

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;
            m_dummy = dummy;

            if (dummy == true)
            {
                btnBackTargetSelection.Text = "検査結果確認\r\n(過去分)へ戻る";
            }

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            // 列のスタイル変更
            this.dgvData.Columns[0].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;     //№
            //this.dgvData.Columns[1].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;     //行
        }

        private void Result_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            lblUser.Text = "作業者名：" + g_parUserNm;

            dgvData.Rows.Clear();

            dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvData.MultiSelect = false;
            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;
            dgvData.ReadOnly = true;


            foreach (string line in File.ReadLines("判定登録.tsv", Encoding.Default))
            {

                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dgvData.Rows.Add(data);

            }

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (m_dummy == false)
            {
                DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
                dtTargetInfo.Rows[intRow]["Status"] = "3";

                objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);
            }
            intRet = 1;
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("判定登録します。よろしいですか？"
                 , "確認"
                 , MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            intRet = 1;

            if (m_dummy == false)
            {
                DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
                dtTargetInfo.Rows[intRow]["Status"] = "4";
                dtTargetInfo.Rows[intRow]["Process"] = "検査結果";

                objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);
            }

            //var result = "";
            //var error = "";
            //dynamic excel = null;
            //dynamic workbooks = null;
            //dynamic book = null;
            //dynamic sheets = null;
            //string strPrintReportName = "PrintReport.xlsm";
            //string strPrintReportPath = @"..\..\" + strPrintReportName;
            //string strCsvFilePath = @".\判定登録.tsv";
            //string strSaveDirPath = @".\OutPrint";
            //string strSaveFileNm = "検査結果照会_" + DateTime.Now.ToString("yyyyMMddHHmmss");

            //// ファイルチェック
            //System.Environment.CurrentDirectory = Application.StartupPath;
            //if (!File.Exists(strPrintReportPath))
            //{
            //    MessageBox.Show("[" + strPrintReportPath + "]ファイルが存在しません");
            //    return;
            //}
            //if (!File.Exists(strCsvFilePath))
            //{
            //    MessageBox.Show("[" + strCsvFilePath + "]ファイルが存在しません");
            //    return;
            //}
            //if (!Directory.Exists(strSaveDirPath))
            //{
            //    MessageBox.Show("[" + strSaveDirPath + "]ディレクトリが存在しません");
            //    return;
            //}

            //// 絶対パスを取得
            //strPrintReportPath = System.IO.Path.GetFullPath(strPrintReportPath);
            //strCsvFilePath = System.IO.Path.GetFullPath(strCsvFilePath);
            //strSaveDirPath = System.IO.Path.GetFullPath(strSaveDirPath);

            //Type type = Type.GetTypeFromProgID("Excel.Application");
            //excel = Activator.CreateInstance(type);
            //if (excel != null)
            //{
            //    try
            //    {
            //        // 非表示
            //        excel.DisplayAlerts = false;

            //        // ワークブック保持
            //        workbooks = excel.Workbooks;

            //        // ブックを生成
            //        book = excel.Workbooks.Open(strPrintReportPath);

            //        // シートを保持
            //        sheets = book.Sheets;

            //        // マクロ実行
            //        result = (string)excel.Run(strPrintReportName + "!" + "RunForApp", strCsvFilePath, strSaveDirPath, strSaveFileNm);
            //    }
            //    catch (Exception ex)
            //    {
            //        error = ex.Message;
            //    }



            //    // 後処理
            //    dispose(sheets);
            //    if (book != null)
            //    {
            //        book.Close(Type.Missing, Type.Missing, Type.Missing);
            //        dispose(book);
            //    }
            //    dispose(workbooks);
            //    if (excel != null)
            //    {
            //        excel.Quit();
            //        dispose(excel);
            //    }

            //    if (!string.IsNullOrEmpty(error))
            //    {
            //        throw new Exception(error);
            //    }
            //}
            //else
            //{
            //    throw new Exception("Excelアプリケーションがインストールされていません。");
            //}

            //// 表示　※デバッグ用
            //excel.Visible = true;

            this.Close();
        }

        /// <summary>
        /// COMオブジェクト破棄
        /// </summary>
        /// <param name="comObject"></param>
        private static void dispose(dynamic comObject)
        {
            try
            {
                if (comObject != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(comObject);
                    comObject = null;
                }
            }
            catch
            {
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(System.Drawing.Image.FromFile(".\\Image\\05_F1_B0019.jpg"), ".\\Image\\05_F1_B0019.jpg");
            frmViewEnlargedimage.ShowDialog(this);
            this.Visible = true;
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
        }

        private void dgvData_MouseUp(object sender, MouseEventArgs e)
        {
            //string s = "";
            //for (int i = 0; i <= dgvData.Columns.Count - 1; i++)
            //{
            //    s = s + dgvData.Columns[i].HeaderText + ":" + dgvData.Columns[i].Width.ToString() + ",";
            //}
            //MessageBox.Show(s);
        }
    }
}
