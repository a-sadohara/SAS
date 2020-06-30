using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
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

            // AIモデルマスタ情報を出力する
            UpdateAIModelName(string.Empty);
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
            m_dtData = new DataTable();
            dgvData.Rows.Clear();
            List<string> lstProductName = new List<string>();
            string strline = string.Empty;
            string strCooperationFile = Path.Combine(g_strAIModelNameCooperationDirectoryPath, g_CON_FILE_NAME_PRODUCT_NAME_INFO);
            FileInfo fiCooperationFile = new FileInfo(strCooperationFile);
            Encoding encSJIS = Encoding.GetEncoding("shift_jis");

            if (fiCooperationFile.Exists &&
                fiCooperationFile.Length != 0)
            {
                try
                {
                    using (StreamReader sr = new StreamReader(strCooperationFile, encSJIS))
                    {
                        while ((strline = sr.ReadLine()) != null)
                        {
                            if (strline.Equals("product_name"))
                            {
                                continue;
                            }

                            lstProductName.Add(strline);
                        }
                    }

                    lstProductName.Sort();

                    // データグリッドビューに反映
                    foreach (string strProductName in lstProductName)
                    {
                        this.dgvData.Rows.Add(false, strProductName);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorMessage = "品名情報の読込でエラーが発生しました。";

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

                    return false;
                }
            }

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
        /// AIモデルマスタ更新
        /// </summary>
        /// <param name="strProductName">品名</param>
        /// <returns></returns>
        private void UpdateAIModelName(string strProductName)
        {
            string strCooperationFile = string.Empty;
            string strPath = string.Empty;
            FileInfo fiCooperationFile = null;
            bool bolWriteFlg = false;
            Encoding encSJIS = Encoding.GetEncoding("shift_jis");

            foreach (string strAIModelNameCooperationDirectoryPath in g_strAIModelNameCooperationDirectoryPath.Split(','))
            {
                try
                {
                    strPath = strAIModelNameCooperationDirectoryPath.Trim();

                    if (string.IsNullOrWhiteSpace(strPath) ||
                        !Directory.Exists(strPath))
                    {
                        // ログ出力
                        WriteEventLog(
                            g_CON_LEVEL_WARN,
                            string.Format(
                                "{0}{1}出力先:{2}",
                                "AIモデル名情報連携ディレクトリが参照できませんでした。",
                                Environment.NewLine,
                                strCooperationFile));

                        continue;
                    }

                    strCooperationFile = Path.Combine(strPath, g_CON_FILE_NAME_AI_MODEL_NAME_INFO);
                    fiCooperationFile = new FileInfo(strCooperationFile);

                    if (fiCooperationFile.Exists &&
                        fiCooperationFile.Length != 0)
                    {
                        // 既存ファイルに追記する
                        using (StreamWriter sw = new StreamWriter(strCooperationFile, true, encSJIS))
                        {
                            sw.WriteLine(
                                string.Format(
                                    "*,{1}",
                                    strProductName,
                                    txtModelName.Text));
                        }
                    }
                    else
                    {
                        // 新規ファイルを作成する
                        using (StreamWriter sw = new StreamWriter(strCooperationFile, false, encSJIS))
                        {
                            sw.WriteLine(g_CON_FILE_HEADER_AI_MODEL_NAME_INFO);
                            sw.WriteLine(
                                string.Format(
                                    "*,{1}",
                                    strProductName,
                                    txtModelName.Text));
                        }
                    }

                    bolWriteFlg = true;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "{0}{1}出力先:{2}{1}{3}",
                            "AIモデルマスタ情報の出力でエラーが発生しました。",
                            Environment.NewLine,
                            strCooperationFile,
                            ex.Message));

                    continue;
                }
            }

            if (bolWriteFlg)
            {
                // メッセージ出力
                MessageBox.Show(
                    "AIモデルマスタ情報の出力が完了しました。",
                    g_CON_MESSAGE_TITLE_QUESTION,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // メッセージ出力
                MessageBox.Show(
                    "AIモデルマスタ情報の出力が失敗しました。",
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        #endregion
    }
}