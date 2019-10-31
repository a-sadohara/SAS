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
using WindowsFormsApp1.DTO;
using static WindowsFormsApp1.Common;
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class DisplayResults : Form
    {
        public NpgsqlConnection NpgsqlCon;
        public DataTable dtData;

        public String parUserNo;

        TargetInfoDto objTargetInfoDto;
        int intRow;

        public DisplayResults(TargetInfoDto objTargetInfo,int intRowIndex)
        {
            InitializeComponent();

            objTargetInfoDto = objTargetInfo;
            intRow = intRowIndex;

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;
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

            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "3";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);

            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DataTable dtTargetInfo = objTargetInfoDto.getTargetInfoDTO();
            dtTargetInfo.Rows[intRow]["Status"] = "4";
            dtTargetInfo.Rows[intRow]["Process"] = "検査結果";

            objTargetInfoDto.setTargetInfoDTO(dtTargetInfo);

            this.Close();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(System.Drawing.Image.FromFile(".\\Image\\05_F1_B0019.jpg"));
            frmViewEnlargedimage.ShowDialog(this);
            this.Visible = true;
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LogOut();
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

        private void txtUser_FocusIn(object sender, EventArgs e)
        {
            // 選択情報を初期化
            parUserNo = "";
            txtUserNm.Text = "";

            // 入力制限
            txtUserNm.ReadOnly = false;
            txtUserNm.MaxLength = 4;
        }

        private void txtUserNm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (String.IsNullOrEmpty(txtUserNm.Text))
                {
                    // 入力されていない　⇒　チェックせずに選択画面に遷移
                    SelectUser(false);
                }
                else
                {
                    // 入力されている　⇒　チェック
                    SelectUser();
                }
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (!String.IsNullOrEmpty(parUserNo))
                {
                    // フォーカスを当てた状態（初期化＆入力制限）にする
                    txtUser_FocusIn(sender, e);
                }
            }
        }

        private void txtUserNm_DoubleClick(object sender, EventArgs e)
        {
            SelectUser(false);
        }

        /// <summary>
        /// 職員選択
        /// </summary>
        /// <param name="parChk">チェック有無</param>
        private void SelectUser(bool parChk = true)
        {
            string strSQL = "";

            if (parChk)
            {
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL += "SELECT USERNAME FROM SAGYOSYA ";
                    strSQL += "WHERE USERNO = '" + txtUserNm.Text + "'";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    if (dtData.Rows.Count > 0)
                    {
                        parUserNo = dtData.Rows[0][0].ToString();
                    }
                    else
                    {
                        MessageBox.Show("入力された職員番号は存在しません");

                        UserSelection frmTargetSelection = new UserSelection();
                        frmTargetSelection.ShowDialog(this);
                        this.Visible = true;

                        parUserNo = frmTargetSelection.strUserNm;
                    }
                }
            }
            else
            {
                UserSelection frmTargetSelection = new UserSelection();
                frmTargetSelection.ShowDialog(this);
                this.Visible = true;

                parUserNo = frmTargetSelection.strUserNm;
            }

            if (!String.IsNullOrEmpty(parUserNo))
            {
                txtUserNm.Text = parUserNo;

                // 入力不可にする
                txtUserNm.ReadOnly = true;
                txtUserNm.BackColor = SystemColors.Window;
            }
            else
            {
                // 選択情報を初期化
                parUserNo = "";
                txtUserNm.Text = "";

                // 入力制限
                txtUserNm.ReadOnly = false;
                txtUserNm.MaxLength = 4;
            }

        }

        private void txtUserNm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\r' && e.KeyChar != '\b' && e.KeyChar < '0' || '9' < e.KeyChar)
            {
                e.Handled = true;
            }
        }
    }

}
