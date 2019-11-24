using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static UserMasterMaintenance.Common;
using Npgsql;
using System.IO;

namespace UserMasterMaintenance
{
    public partial class UserMasterMaintenance : Form
    {
        #region 定数・変数
        private DataTable m_dtData;

        private string m_strKanaSta = "";
        private string m_strKanaEnd = "";

        private const Int32 m_CON_COL_USERNO = 0;
        private const Int32 m_CON_COL_USERNAME = 1;
        private const Int32 m_CON_COL_USERNAMEKANA = 2;
        #endregion

        #region イベント

        /// <summary>
        /// 初期表示
        /// </summary>
        public UserMasterMaintenance()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            dispDataGridView();
        }

        /// <summary>
        /// 登録ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReg_Click(object sender, EventArgs e)
        {
            // 作業者登録画面を登録モードで表示する
            UserEdit frmUserReg = new UserEdit(g_CON_EDITMODE_REG);
            frmUserReg.ShowDialog();
            if(frmUserReg.g_UpdateFlg == 1) 
            {
                dispDataGridView();
            }
        }

        /// <summary>
        /// 削除ボタンクリック
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDel_Click(object sender, EventArgs e)
        {
            string strSelUserNo = "";
            string strSelUserName = "";
            string strSelUserNameKana = "";

            if (dgvUser.SelectedRows.Count == 0)
            {
                MessageBox.Show("削除する行を選択してください");
                return;
            }
            else
            {
                foreach (DataGridViewRow r in dgvUser.SelectedRows)
                {
                    strSelUserNo = NulltoString(r.Cells[m_CON_COL_USERNO].Value);
                    strSelUserName = NulltoString(r.Cells[m_CON_COL_USERNAME].Value);
                    strSelUserNameKana = NulltoString(r.Cells[m_CON_COL_USERNAMEKANA].Value);
                }

                if (MessageBox.Show("選択されている行（データ）を削除しますか？"
                                   + Environment.NewLine
                                   + "社員番号：" + strSelUserNo + " 作業者名：" + strSelUserName + " 読み仮名：" + strSelUserNameKana
                                   , "確認"
                                   , MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (g_bolModeNonDBCon == true)
                    {
                        MessageBox.Show("削除しました");
                    }
                    else
                    {
                        try
                        {
                            // PostgreSQLへ接続
                            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_CON_DB_INFO))
                            {
                                NpgsqlCon.Open();

                                using (var transaction = NpgsqlCon.BeginTransaction())
                                {
                                    if (DelUser(NpgsqlCon, transaction, strSelUserNo) == true)
                                    {
                                        transaction.Commit();
                                        MessageBox.Show("削除しました");
                                    }
                                    else
                                    {
                                        MessageBox.Show("削除に失敗したためロールバックします");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("削除時にエラーが発生しました。"
                                           + Environment.NewLine
                                           + ex.Message);
                        }
                    }

                    // 再表示
                    dispDataGridView();
                }
            }
        }

        /// <summary>
        /// csvインポートボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            UserImportCsv frmUserImportCsv = new UserImportCsv();

            if (System.Windows.Forms.DialogResult.OK == frmUserImportCsv.ShowDialog())
            {
                //処理を記述する
                dispDataGridView();
            }
        }

        /// <summary>
        /// 文字リンククリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void llk_Click(dynamic sender, EventArgs e)
        {
            if (sender == llkあ) { m_strKanaSta = "ア"; m_strKanaEnd = "オ"; }
            if (sender == llkか) { m_strKanaSta = "カ"; m_strKanaEnd = "コ"; }
            if (sender == llkさ) { m_strKanaSta = "サ"; m_strKanaEnd = "ソ"; }
            if (sender == llkた) { m_strKanaSta = "タ"; m_strKanaEnd = "ト"; }
            if (sender == llkな) { m_strKanaSta = "ナ"; m_strKanaEnd = "ノ"; }
            if (sender == llkは) { m_strKanaSta = "ハ"; m_strKanaEnd = "ホ"; }
            if (sender == llkま) { m_strKanaSta = "マ"; m_strKanaEnd = "モ"; }
            if (sender == llkや) { m_strKanaSta = "ヤ"; m_strKanaEnd = "ヨ"; }
            if (sender == llkら) { m_strKanaSta = "ラ"; m_strKanaEnd = "ロ"; }
            if (sender == llkわ) { m_strKanaSta = "ワ"; m_strKanaEnd = "ン"; }
            if (sender == llkEtc) { m_strKanaSta = "！"; m_strKanaEnd = "！"; }
            if (sender == llkNon) { m_strKanaSta = ""; m_strKanaEnd = ""; }

            foreach (Label lbl in new Label[] { llkあ, llkか, llkさ, llkた, llkな,
                                                llkは, llkま, llkや, llkら, llkわ,
                                                llkEtc, llkNon })
            {
                if (sender == lbl)
                {
                    lbl.BackColor = System.Drawing.SystemColors.Highlight;
                }
                else
                {
                    lbl.BackColor = System.Drawing.Color.Transparent;
                }
            }
        }

        /// <summary>
        /// 作業者グリッドセルダブルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvUser_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {

            // 作業者登録画面を更新モードで表示する
            UserEdit frmUserReg = new UserEdit(g_CON_EDITMODE_UPD,
                                               dgvUser.Rows[e.RowIndex].Cells[0].Value.ToString(),
                                               dgvUser.Rows[e.RowIndex].Cells[1].Value.ToString(),
                                               dgvUser.Rows[e.RowIndex].Cells[2].Value.ToString());
            frmUserReg.ShowDialog();
            if (frmUserReg.g_UpdateFlg == 1)
            {
                dispDataGridView();
            }
        }

        /// <summary>
        /// 検索ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            dispDataGridView();
        }

        /// <summary>
        /// 作業者番号From入力イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            //0～9と、バックスペース以外の時は、イベントをキャンセルする
            if ((e.KeyChar < '0' || '9' < e.KeyChar) && e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 作業者番号Fromフォーカス消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_From_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            if (txtUserNo_From.TextLength > 0)
            {
                if (Int32.TryParse(txtUserNo_From.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    txtUserNo_From.Focus();
                    return;
                }
                else
                {
                    txtUserNo_From.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }

        /// <summary>
        /// 作業者番号Toフォーカス消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtUserNo_To_Leave(object sender, EventArgs e)
        {
            int intUserNo = 0;

            if (txtUserNo_To.TextLength > 0)
            {
                if (Int32.TryParse(txtUserNo_To.Text, out intUserNo) == false)
                {
                    MessageBox.Show("数値のみ入力してください。");
                    txtUserNo_To.Focus();
                    return;
                }
                else
                {
                    txtUserNo_To.Text = String.Format("{0:D4}", intUserNo);
                }
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        private void dispDataGridView()
        {
            string strSQL = "";

            dgvUser.Rows.Clear();

            int intUserNoSta = 0;
            int intUserNoEnd = 0;

            try
            {
                if (g_bolModeNonDBCon == true)
                    throw new Exception("DB非接続モードです");

                // 条件が指定されていない場合は抽出しない
                // PostgreSQLへ接続
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_CON_DB_INFO))
                {
                    NpgsqlCon.Open();

                    // SQL抽出
                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    m_dtData = new DataTable();
                    strSQL += @"SELECT 
                                    WorkerNo
                                  , WorkerSurname || '" + g_CON_NAME_SEPARATE + @"' || WorkerName as WorkerName
                                  , WorkerSurnameKana || '" + g_CON_NAME_SEPARATE + @"' || WorkerNameKana as WorkerNameKana
                                FROM 
                                    mst_Worker ";
                    strSQL += "WHERE Delflg = 0 ";
                    if (!string.IsNullOrEmpty(m_strKanaSta))
                    {
                        strSQL += "AND SUBSTRING(WorkerSurnameKana,1,1) >= '" + m_strKanaSta + "' ";
                    }
                    if (!string.IsNullOrEmpty(m_strKanaEnd))
                    {
                        strSQL += "AND SUBSTRING(WorkerSurnameKana,1,1) <= '" + m_strKanaEnd + "' ";
                    }
                    if (txtUserNo_From.Text != "")
                    {
                        Int32.TryParse(txtUserNo_From.Text, out intUserNoSta);
                        strSQL += "AND TO_NUMBER(WorkerNo, '0000') >= " + intUserNoSta + " ";
                    }
                    if (txtUserNo_To.Text != "")
                    {
                        Int32.TryParse(txtUserNo_To.Text, out intUserNoEnd);
                        strSQL += "AND TO_NUMBER(WorkerNo, '0000') <= " + intUserNoEnd + " ";
                    }
                    strSQL += "ORDER BY WorkerNo ASC ;";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(m_dtData);

                    // データグリッドビューに反映
                    foreach (DataRow row in m_dtData.Rows)
                    {
                        this.dgvUser.Rows.Add(row.ItemArray);
                    }
                }
            }
            catch (Exception e)
            {
                string strErrMsg = "";
                strErrMsg = e.Message;

                // 後々この処理は消す
                foreach (string line in File.ReadLines("作業者.tsv", Encoding.Default))
                {
                    // 改行コードを変換
                    string strLine = line.Replace("\\rn", Environment.NewLine);

                    string[] csv = strLine.Split('\t');
                    string[] data = new string[csv.Length];
                    Array.Copy(csv, 0, data, 0, data.Length);
                    this.dgvUser.Rows.Add(data);
                }
            }

            dgvUser.CurrentCell = null;

            if (dgvUser.Rows.Count > 0) 
            {
                dgvUser.Rows[0].Selected = true;
            }
        }

        /// <summary>
        /// ユーザ削除処理
        /// </summary>
        /// <param name="NpgsqlCon">接続子</param>
        /// <param name="transaction">トランザクション</param>
        /// <returns></returns>
        private Boolean DelUser(NpgsqlConnection NpgsqlCon
                              , NpgsqlTransaction transaction
                              , string strSelUserNo)
        {
            // SQL文を作成する
            string strUpdateSql = @"UPDATE mst_Worker
                                           SET Delflg = 1
                                         WHERE WorkerNo = :UserNo";

            // SQLコマンドに各パラメータを設定する
            var command = new NpgsqlCommand(strUpdateSql, NpgsqlCon, transaction);

            command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = strSelUserNo });

            // sqlを実行する
            if (ExecTranSQL(command, transaction) == false)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
