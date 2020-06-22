using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static ProductMstMaintenance.Common;

namespace ProductMstMaintenance
{
    public partial class AIModelMstSelection : Form
    {
        #region 定数・変数
        private const int m_CON_COL_CHK_SELECT = 0;
        private const int m_CON_COL_CHK_SELECT_INITIAL_VALUE = 1;
        private const int m_CON_COL_PRODUCT_NAME = 2;
        private const int m_CON_COL_AI_MODEL_NAME = 3;
        private const int m_CON_UPDATE_THRESHOLD_VALUE = 10;

        private string m_strProductName = string.Empty;
        private string m_strAIModelName = string.Empty;
        private bool m_bolEditModeFlg = false;

        private DataTable m_dtData;

        // 戻り値用変数
        public string strAIModelName;
        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        /// <param name="strProductName">品名</param>
        /// <param name="strAIModelName">AIモデル名</param>
        /// <param name="bolEditModeFlg">編集モードフラグ</param>
        public AIModelMstSelection(
            string strProductName,
            string strAIModelName,
            bool bolEditModeFlg)
        {
            m_strProductName = strProductName;
            m_strAIModelName = strAIModelName;
            m_bolEditModeFlg = bolEditModeFlg;

            InitializeComponent();

            if (!m_bolEditModeFlg)
            {
                // 編集モード以外の場合、品名列を非表示とする
                this.dgvData.Columns[m_CON_COL_PRODUCT_NAME].Visible = false;
            }

            // フォームの表示位置調整
            this.StartPosition = FormStartPosition.CenterParent;
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAIModelName_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;

            // 複数選択させない
            this.dgvData.MultiSelect = false;

            dgvData.Rows.Clear();
            strAIModelName = string.Empty;

            // 初期表示処理
            if (!dispDataGridView())
            {
                this.Close();
            }
        }

        /// <summary>
        /// OKボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            strAIModelName = string.Empty;

            // 編集モードの場合、表示フラグの情報をDBに反映させる
            if (m_bolEditModeFlg)
            {
                // 編集行を抽出する
                DataGridViewRow[] dgvRow =
                    dgvData.Rows.Cast<DataGridViewRow>().Where(
                        x => !x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(x.Cells[m_CON_COL_CHK_SELECT_INITIAL_VALUE].Value)).AsEnumerable().ToArray();

                // 編集行が存在する場合、下記の処理を行う
                if (dgvRow.Length != 0)
                {
                    // 編集行数が更新閾値を超過している場合、エラーとする
                    if (dgvRow.Length > m_CON_UPDATE_THRESHOLD_VALUE)
                    {
                        MessageBox.Show(string.Format(g_clsMessageInfo.strMsgW0011, m_CON_UPDATE_THRESHOLD_VALUE), "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    StringBuilder sbAIModelNameInfo = new StringBuilder();

                    // 空行を追加
                    sbAIModelNameInfo.AppendLine();

                    // 表示→非表示へ変更される情報を抽出する
                    if (dgvRow.Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(false)).Count() != 0)
                    {
                        CreateDialogMessage(
                            dgvRow,
                            false,
                            sbAIModelNameInfo,
                            "▼表示⇒非表示");
                    }

                    // 非表示→表示へ変更される情報を抽出する
                    if (dgvRow.Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)).Count() != 0)
                    {
                        CreateDialogMessage(
                            dgvRow,
                            true,
                            sbAIModelNameInfo,
                            "▼非表示⇒表示");
                    }

                    if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0014, sbAIModelNameInfo.ToString()), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        return;
                    }

                    bolUpdateDisplayFlg();
                }
            }
            else
            {
                // 編集モード以外の場合、選択行のAIモデル名を親画面に返却する
                DataGridViewRow dgvRow = dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)).FirstOrDefault();

                if (dgvRow != null)
                {
                    strAIModelName = dgvRow.Cells[m_CON_COL_AI_MODEL_NAME].Value.ToString();
                }
            }

            this.Close();
        }

        /// <summary>
        /// データグリッドビューセルクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // ヘッダクリックでソートする際、下記処理をスキップする
            if (e.RowIndex == -1)
            {
                return;
            }

            if (m_bolEditModeFlg)
            {
                // チェック状態を反転させる
                if (dgvData.Rows[e.RowIndex].Cells[m_CON_COL_CHK_SELECT].Value.Equals(true))
                {
                    dgvData.Rows[e.RowIndex].Cells[m_CON_COL_CHK_SELECT].Value = false;
                }
                else
                {
                    dgvData.Rows[e.RowIndex].Cells[m_CON_COL_CHK_SELECT].Value = true;
                }
            }
            else
            {
                // 選択行のみチェック状態とする
                foreach (DataGridViewRow dgvRow in dgvData.Rows)
                {
                    dgvRow.Cells[m_CON_COL_CHK_SELECT].Value = false;
                }

                dgvData.Rows[e.RowIndex].Cells[m_CON_COL_CHK_SELECT].Value = true;
            }
        }
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        private bool dispDataGridView()
        {
            string strSQL = string.Empty;
            m_dtData = new DataTable();
            dgvData.Rows.Clear();

            try
            {
                if (m_bolEditModeFlg)
                {
                    // SQL文を作成する
                    strSQL = g_CON_SELECT_MST_AI_MODEL_EDITMODE;

                    // SQLを実行する
                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);
                }
                else
                {
                    // SQL文を作成する
                    strSQL = g_CON_SELECT_MST_AI_MODEL;

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = m_strProductName });

                    // SQLを実行する
                    g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL, lstNpgsqlCommand);
                }

                // データグリッドビューに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvData.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0066, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // 行選択はさせない
            dgvData.CurrentCell = null;

            if (!m_bolEditModeFlg &&
                dgvData.Rows.Count > 0)
            {
                // 現在設定されているAIモデル名に紐付く情報を抽出する
                DataGridViewRow dgvRow =
                    dgvData.Rows.Cast<DataGridViewRow>().Where(
                        x => x.Cells[m_CON_COL_AI_MODEL_NAME].Value.Equals(m_strAIModelName)).FirstOrDefault();

                if (dgvRow != null)
                {
                    // 現在設定されているAIモデル名を選択状態にする
                    dgvRow.Selected = true;
                    dgvData.FirstDisplayedScrollingRowIndex = dgvRow.Index;
                    dgvRow.Cells[m_CON_COL_CHK_SELECT].Value = true;
                }
                else
                {
                    // 1行目を選択状態にする
                    dgvData.Rows[0].Selected = true;
                    dgvData.FirstDisplayedScrollingRowIndex = 0;
                    dgvData.Rows[0].Cells[m_CON_COL_CHK_SELECT].Value = true;
                }
            }

            return true;
        }

        /// <summary>
        /// ダイアログメッセージ作成
        /// </summary>
        /// <param name="dgvRow">対象グリッド行</param>
        /// <param name="bolDisplayFlg">表示フラグ</param>
        /// <param name="sbAIModelNameInfo">AIモデル名情報</param>
        /// <param name="strHeaderMessage">ヘッダメッセージ</param>
        private void CreateDialogMessage(
            DataGridViewRow[] dgvRow,
            bool bolDisplayFlg,
            StringBuilder sbAIModelNameInfo,
            string strHeaderMessage)
        {
            // ヘッダを追加
            sbAIModelNameInfo.AppendLine(strHeaderMessage);

            foreach (DataGridViewRow row in dgvRow.Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(bolDisplayFlg)))
            {
                sbAIModelNameInfo.AppendLine(
                    string.Format(
                        " 品名：{0}、AIモデル名：{1}",
                        NulltoString(row.Cells[m_CON_COL_PRODUCT_NAME].Value),
                        NulltoString(row.Cells[m_CON_COL_AI_MODEL_NAME].Value)));
            }

            // 空行を追加
            sbAIModelNameInfo.AppendLine();
        }

        /// <summary>
        /// 表示フラグ更新
        /// </summary>
        private bool bolUpdateDisplayFlg()
        {
            try
            {
                int intDisplayFlg = 0;
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // SQL文を作成する
                string strUpdateSql = g_CON_UPDATE_MST_AI_MODEL_DISPLAY_FLG;

                foreach (DataGridViewRow dgvRow in dgvData.Rows)
                {
                    if (dgvRow.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true))
                    {
                        intDisplayFlg = 0;
                    }
                    else
                    {
                        intDisplayFlg = 1;
                    }

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "display_flg", DbType = DbType.Int32, Value = intDisplayFlg });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "product_name", DbType = DbType.String, Value = NulltoString(dgvRow.Cells[m_CON_COL_PRODUCT_NAME].Value) });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "ai_model_name", DbType = DbType.String, Value = NulltoString(dgvRow.Cells[m_CON_COL_AI_MODEL_NAME].Value) });

                    // SQLを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);
                }

                g_clsConnectionNpgsql.DbCommit();

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0067, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }
        #endregion
    }
}