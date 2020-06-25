using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static RapidModelImport.Common;

namespace RapidModelImport
{
    public partial class ImportScreen : Form
    {
        #region 定数・変数
        private const int m_CON_COL_CHK_SELECT = 0;
        private const int m_CON_COL_PRODUCT_NAME = 1;

        private DataTable m_dtData;
        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        public ImportScreen()
        {
            InitializeComponent();

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
            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;

            // 複数選択させない
            this.dgvData.MultiSelect = false;

            dgvData.Rows.Clear();

            // 初期表示処理
            if (!dispDataGridView())
            {
                this.Close();
            }
        }

        /// <summary>
        /// 取込フォルダパス押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFolder_Click(object sender, EventArgs e)
        {
            SelectFolder();
        }

        /// <summary>
        /// 参照ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReference_Click(object sender, EventArgs e)
        {
            SelectFolder();
        }

        /// <summary>
        /// インポートボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnImport_Click(object sender, EventArgs e)
        {
            DataGridViewRow dgvRow =
                dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)).FirstOrDefault();

            if (dgvRow == null)
            {
                // メッセージ出力
                MessageBox.Show(
                    "品名が選択されていません。",
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (string.IsNullOrWhiteSpace(txtModelName.Text))
            {
                // メッセージ出力
                MessageBox.Show(
                    "モデル名が入力されていません。",
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (string.IsNullOrWhiteSpace(txtFolder.Text))
            {
                // メッセージ出力
                MessageBox.Show(
                    "モデルファイルフォルダが選択されていません。",
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            string strMessage = string.Empty;

            txtExecutionResult.Text =
                string.Format(
                    "{0}{1}{2}{1}{1}",
                    "処理を開始します。",
                    Environment.NewLine,
                    "しばらくお待ちください...");

            txtExecutionResult.Refresh();

            try
            {
                using (Process prBat = new Process())
                {
                    prBat.StartInfo.FileName = g_strBatchPath;
                    prBat.StartInfo.Arguments =
                        string.Format(
                            @" {0} {1}",
                            txtFolder.Text,
                            txtModelName.Text);
                    prBat.StartInfo.CreateNoWindow = true;
                    prBat.StartInfo.UseShellExecute = false;
                    prBat.StartInfo.RedirectStandardOutput = true;
                    prBat.Start();
                    strMessage = prBat.StandardOutput.ReadToEnd();
                    prBat.WaitForExit();
                }

                txtExecutionResult.Text += strMessage;
            }
            catch (Exception ex)
            {
                string strErrorMessage = "batファイル実行時にエラーが発生しました";

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        strErrorMessage,
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    strErrorMessage,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // AIモデルマスタを更新する
            if (!UpdateAIModelName(dgvRow.Cells[m_CON_COL_PRODUCT_NAME].Value.ToString()))
            {
                return;
            }

            // メッセージ出力
            MessageBox.Show(
                "AIモデルマスタの更新が完了しました。",
                g_CON_MESSAGE_TITLE_QUESTION,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// 閉じるボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
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

            // 選択行のみチェック状態とする
            foreach (DataGridViewRow dgvRow in dgvData.Rows)
            {
                dgvRow.Cells[m_CON_COL_CHK_SELECT].Value = false;
            }

            dgvData.Rows[e.RowIndex].Cells[m_CON_COL_CHK_SELECT].Value = true;
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
                // SQL文を作成する
                strSQL = @"SELECT FALSE, product_name FROM mst_product_info ORDER BY product_name";

                // SQLを実行する
                g_clsConnectionNpgsql.SelectSQL(ref m_dtData, strSQL);

                // データグリッドビューに反映
                foreach (DataRow row in m_dtData.Rows)
                {
                    this.dgvData.Rows.Add(row.ItemArray);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        g_clsMessageInfo.strMsgE0001,
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0066,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            // 行選択はさせない
            dgvData.CurrentCell = null;

            if (dgvData.Rows.Count == 0)
            {
                // メッセージ出力
                MessageBox.Show(
                    "品名情報が存在しません。",
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
            else
            {
                // 1行目を選択状態にする
                dgvData.Rows[0].Selected = true;
                dgvData.FirstDisplayedScrollingRowIndex = 0;
                dgvData.Rows[0].Cells[m_CON_COL_CHK_SELECT].Value = true;

                return true;
            }
        }

        /// <summary>
        /// フォルダ選択
        /// </summary>
        private void SelectFolder()
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                // 上部に表示する説明テキストを指定する
                fbd.Description = "モデルファイルが格納されているフォルダを選択してください。";

                // 初期ディレクトリを指定する
                if (string.IsNullOrWhiteSpace(txtFolder.Text))
                {
                    fbd.SelectedPath = @"C:\";
                }
                else
                {
                    fbd.SelectedPath = txtFolder.Text;
                }

                // ユーザーが新しいフォルダを作成できるようにする
                fbd.ShowNewFolderButton = true;

                // ダイアログを表示する
                if (fbd.ShowDialog(this) == DialogResult.OK)
                {
                    // 選択されたフォルダを表示する
                    txtFolder.Text = fbd.SelectedPath;
                }
            }
        }

        /// <summary>
        /// AIモデルマスタ更新SQL処理
        /// </summary>
        /// <param name="strProductName">品名</param>
        /// <returns></returns>
        private bool UpdateAIModelName(string strProductName)
        {
            try
            {
                // SQL文を作成する
                string strINSERTSql = @"
                    INSERT INTO public.mst_ai_model
                    (
                        product_name,
                        ai_model_name
                    )
                    VALUES
                    (
                        :strProductName,
                        :strAIModelName
                    )
                    ON CONFLICT
                    DO NOTHING ";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "strProductName", DbType = DbType.String, Value = NulltoString(strProductName) });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "strAIModelName", DbType = DbType.String, Value = NulltoString(txtModelName.Text) });

                // SQLを実行する(マスタに存在しないデータのみ登録される)
                g_clsConnectionNpgsql.ExecTranSQL(strINSERTSql, lstNpgsqlCommand);

                // トランザクションコミット
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        g_clsMessageInfo.strMsgE0067,
                        Environment.NewLine,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    g_clsMessageInfo.strMsgE0067,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }
        #endregion
    }
}