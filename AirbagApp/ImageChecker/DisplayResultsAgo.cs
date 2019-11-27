using System;
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
using Npgsql;

namespace ImageChecker
{
    public partial class DisplayResultsAgo : Form
    {
        public NpgsqlConnection NpgsqlCon;
        public DataTable dtData;

        public String parUserNo;

        TargetInfoDto objTargetInfoDto;
        int intRow;

        public DisplayResultsAgo()
        {
            InitializeComponent();

            //// DataGridViewのフォント調整
            //if (1920 > (int)System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width)
            //{
            //    this.dgvData.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("メイリオ", 8.00F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            //    this.dgvData.ColumnHeadersHeight = 20;
            //    this.dgvData.DefaultCellStyle.Font = new System.Drawing.Font("メイリオ", 8.00F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            //    this.dgvData.RowTemplate.Height = 20;
            //}

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;

            // テキストボックス初期化
            txtGoki.Text = "";
            txtHinNm.Text = "";
            txtSashizu.Text = "";
            txtHanNo.Text = "";
            txtKensaNo.Text = "";
            txtHansoStaDate_From.Text = "";
            txtHansoStaDate_To.Text = "";
            txtKensakuRow_From.Text = "";
            txtKensakuRow_To.Text = "";
            txtHansoEndDate_From.Text = "";
            txtHansoEndDate_To.Text = "";
            txtJudgeStaDate_From.Text = "";
            txtJudgeStaDate_To.Text = "";
            txtJudgeEndDate_From.Text = "";
            txtJudgeEndDate_To.Text = "";
            txtUserNm.Text = "";
            txtRow.Text = "";
            txtCol.Text = "";
            txtNgSide.Text = "";
            txtNgReason.Text = "";

            // 列のスタイル変更
            this.dgvData.Columns[0].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;     //№
            this.dgvData.Columns[1].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;     //号機
            this.dgvData.Columns[10].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;    //検査番号
            //this.dgvData.Columns[11].DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;    //行
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


            foreach (string line in File.ReadLines("検査結果確認過去分.tsv", Encoding.Default))
            {
                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dgvData.Rows.Add(data);

            }

            // 検索件数更新
            lblSearchCountMax.Text = this.dgvData.Rows.Count.ToString();
            lblSearchCount.Text = lblSearchCountMax.Text;

        }

        private void Button1_Click(object sender, EventArgs e)
        {
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
                try
                {
                    if (bolModeNonDBCon == true)
                        throw new Exception("DB非接続モードです");

                    // PostgreSQLへ接続
                    using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
                    {
                        NpgsqlCon.Open();

                        // SQL抽出
                        NpgsqlCommand NpgsqlCom = null;
                        NpgsqlDataAdapter NpgsqlDtAd = null;
                        dtData = new DataTable();
                        strSQL += "SELECT USERNO,USERNAME,USERYOMIGANA FROM SAGYOSYA ";
                        strSQL += "WHERE USERNO = '" + txtUserNm.Text + "'";
                        NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                        NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                        NpgsqlDtAd.Fill(dtData);
                    }
                }
                catch
                {
                    dtData = new DataTable();
                    dtData.Columns.Add("USERNO");
                    dtData.Columns.Add("USERNAME");
                    dtData.Columns.Add("USERYOMIGANA");

                    // 後々この処理は消す
                    foreach (string line in System.IO.File.ReadLines("作業者.tsv", Encoding.Default))
                    {
                        // 改行コードを変換
                        string strLine = line.Replace("\\rn", Environment.NewLine);

                        string[] csv = strLine.Split('\t');
                        string[] data = new string[csv.Length];
                        Array.Copy(csv, 0, data, 0, data.Length);

                        if (data[0].ToString() != txtUserNm.Text)
                            continue;

                        dtData.Rows.Add(data);
                    }
                }

                if (dtData.Rows.Count > 0)
                {
                    parUserNo = dtData.Rows[0][1].ToString();
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

        private void btnSearch_Click(object sender, EventArgs e)
        {
            this.dgvData.Rows.Clear();

            // データ抽出
            foreach (string line in File.ReadLines("検査結果確認過去分.tsv", Encoding.Default))
            {
                // 改行コードを変換
                string strLine = line.Replace("\\rn", Environment.NewLine);

                string[] csv = strLine.Split('\t');
                string[] data = new string[csv.Length];
                Array.Copy(csv, 0, data, 0, data.Length);
                this.dgvData.Rows.Add(data);

            }

            // 検索件数更新
            lblSearchCountMax.Text = this.dgvData.Rows.Count.ToString();
            lblSearchCount.Text = lblSearchCountMax.Text;
        }

        private void BackResultCheck_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("以下の情報で合否確認に戻ります。よろしいですか？\r\n" +
            //                    this.dataGridView1.SelectedRows[0].Cells[1].Value +
            //                    this.dataGridView1.SelectedRows[0].Cells[2].Value
            //                  , "確認"
            //                  , MessageBoxButtons.YesNo) == DialogResult.No)
            //{
            //    return;
            //}

            TargetInfoDto dtTagetInfo = new TargetInfoDto();
            DataTable dtWkInfo = dtTagetInfo.getTargetInfoDTO();

            //TODO
            //検査番号,品名,反番等で、対象選択の行と紐づけ

            ResultCheck frmResultCheck = new ResultCheck(dtTagetInfo, this.dgvData.SelectedRows[0].Index, true);
            frmResultCheck.ShowDialog(this);
            this.Visible = true;
        }

        private void dgvData_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            ViewEnlargedimage frmViewEnlargedimage = new ViewEnlargedimage(System.Drawing.Image.FromFile(".\\Image\\05_F1_B0019.jpg"), ".\\Image\\05_F1_B0019.jpg");
            frmViewEnlargedimage.ShowDialog(this);
            this.Visible = true;
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
