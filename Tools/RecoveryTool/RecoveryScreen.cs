﻿using RecoveryTool.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static RecoveryTool.Common;

namespace RecoveryTool
{
    public partial class RecoveryScreen : Form
    {
        #region 定数・変数
        private const int m_CON_COL_CHK_SELECT = 0;
        private const int m_CON_COL_INSPECTION_DATE = 1;
        private const int m_CON_COL_INSPECTION_NUM = 2;
        private const int m_CON_COL_FABRICNAME = 3;
        private const int m_CON_COL_IMAGING_STARTTIME_DATE = 4;
        private const string m_FILENAME_COMMANDPROMPT = "cmd.exe";
        private const string m_CON_COMMAND_TASKKILL = @"/c taskkill {0} {1} {2} /F /FI ""IMAGENAME eq function_*""";
        private const string m_CON_COMMAND_TASKLIST = @"/c tasklist {0} {1} {2} /FI ""IMAGENAME eq function_*""";
        private const string m_CON_COMMAND_SCHTASKS = @"/c schtasks /RUN {0} {1} {2} /I /TN ""{3}""";
        private const string m_CON_RESULT_SUCCESS = "情報: 指定された条件に一致するタスクは実行されていません。";
        private const string m_CON_UNIT_NUM_FORMAT = "{0}号機";
        private const string m_CON_LOG_FORMAT = "{0}号機、検査日付:{1}、検査番号:{2}、反番:{3}";
        private const string m_CON_FUNCTION_NAME_PATTERN = "function_[0-9][0-9][0-9].exe";

        // 検査情報リスト
        private List<InspectionInfo> m_lstInspectionInfo;

        // エラーファイルリスト
        private IEnumerable<string> m_lstFiles;

        // 初回起動フラグ
        private bool bolInitialLaunch = true;
        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        public RecoveryScreen()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            lblUnitNum.Text = string.Format(m_CON_UNIT_NUM_FORMAT, g_strUnitNum);
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RecoveryScreen_Load(object sender, EventArgs e)
        {
            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;

            // 複数選択させない
            this.dgvData.MultiSelect = false;

            m_lstFiles =
                Directory.EnumerateFiles(
                    g_strErrorFileOutputPath,
                    "*",
                    SearchOption.AllDirectories);

            // エラーファイルの有無確認
            if (m_lstFiles.Count() == 0)
            {
                // メッセージ出力
                MessageBox.Show(
                    "エラーファイルは出力されていません。",
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }
            else
            {
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "エラーファイルが出力されています。",
                        Environment.NewLine));

                foreach (string strFile in m_lstFiles)
                {
                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "・{0}{1}",
                            strFile,
                            Environment.NewLine));
                }

                ExecutionResultTextAdded(
                    false,
                    Environment.NewLine);
            }

            // 初期表示処理
            dispDataGridView();
        }

        /// <summary>
        /// 復旧ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnRecovery_Click(object sender, EventArgs e)
        {
            this.btnRecovery.Enabled = false;

            try
            {
                string strResult = string.Empty;
                string strErrorFile = string.Empty;
                List<string> strFunctionName = new List<string>();
                List<Task<string>> lstTask = new List<Task<string>>();

                ExecutionResultTextAdded(
                    !bolInitialLaunch,
                    string.Format(
                        "{0}{1}{1}",
                        "復旧処理を開始します。",
                        Environment.NewLine));

                bolInitialLaunch = false;

                #region プロセス停止
                try
                {
                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}",
                            "■連携基盤のプロセスを停止します。",
                            Environment.NewLine));

                    // プロセスを停止する
                    Task<string> taskExecuteTaskKill =
                        Task.Run(() => strExecuteCommandPrompt(
                            m_CON_COMMAND_TASKKILL,
                            string.Empty));

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}",
                            "⇒プロセスの停止コマンドを実行しました。",
                            Environment.NewLine));

                    await taskExecuteTaskKill;

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}",
                            taskExecuteTaskKill.Result,
                            Environment.NewLine));

                    await Task.Delay(1000);

                    do
                    {
                        if (!string.IsNullOrWhiteSpace(strResult))
                        {
                            ExecutionResultTextAdded(
                                false,
                                string.Format(
                                    "{0}{1}{1}{1}",
                                    "⇒全てのプロセス停止が確認できないため、10秒後に再確認します。",
                                    Environment.NewLine));

                            await Task.Delay(10000);
                        }

                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}",
                                "⇒プロセスの状態を確認します。",
                                Environment.NewLine));

                        // プロセスの状態を確認する
                        Task<string> taskExecuteTaskList =
                            Task.Run(() => strExecuteCommandPrompt(
                                m_CON_COMMAND_TASKLIST,
                                string.Empty));

                        await taskExecuteTaskList;

                        strResult = taskExecuteTaskList.Result;

                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}",
                                strResult,
                                Environment.NewLine));
                    }
                    while (!strResult.Replace(Environment.NewLine, string.Empty).Equals(m_CON_RESULT_SUCCESS));
                }
                catch (Exception ex)
                {
                    string strErrorMessage = "プロセス停止中にエラーが発生しました。";

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "{0}{1}{2}",
                            strErrorMessage,
                            Environment.NewLine,
                            ex.Message));

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "⇒{0}{1}{2}",
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

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}{1}",
                        "⇒全てのプロセスが停止しました。",
                        Environment.NewLine));
                #endregion

                #region ステータス更新
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "■処理ステータスを更新します。",
                        Environment.NewLine));

                if (dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)).Count() != 0)
                {
                    try
                    {
                        // ステータス更新を行う
                        if (!bolUpdateFabricInfo() ||
                            !bolUpdateProcessingStatus() ||
                            !bolUpdateRapidAnalysisInfo())
                        {
                            return;
                        }

                        g_clsConnectionNpgsql.DbCommit();
                    }
                    finally
                    {
                        g_clsConnectionNpgsql.DbClose();
                    }

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}{1}",
                            "⇒更新が完了しました。",
                            Environment.NewLine));
                }
                else
                {
                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}{1}",
                            "⇒更新対象のデータがありませんでした。",
                            Environment.NewLine));
                }
                #endregion

                #region プロセス全起動
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "■連携基盤のプロセスを開始します。",
                        Environment.NewLine));

                try
                {
                    foreach (string strTaskName in g_strTaskName)
                    {
                        // プロセスを開始する
                        lstTask.Add(Task.Run(() => strExecuteCommandPrompt(
                                m_CON_COMMAND_SCHTASKS,
                                strTaskName)));
                    }

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "{0}{1}",
                            "⇒プロセス起動コマンドを実行しました。",
                            Environment.NewLine));

                    await Task.WhenAll(lstTask.ToArray());

                    foreach (Task<string> tsk in lstTask)
                    {
                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}",
                                tsk.Result,
                                Environment.NewLine));
                    }

                    await Task.Delay(1000);
                    strResult = string.Empty;

                    do
                    {
                        if (!string.IsNullOrWhiteSpace(strResult))
                        {
                            ExecutionResultTextAdded(
                                false,
                                string.Format(
                                    "{0}{1}{1}{1}",
                                    "⇒全てのプロセス起動が確認できないため、10秒後に再確認します。",
                                    Environment.NewLine));

                            await Task.Delay(10000);
                        }

                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}{0}",
                                Environment.NewLine,
                                "⇒プロセスの状態を確認します。"));

                        // プロセスの状態を確認する
                        Task<string> taskExecuteTaskList =
                            Task.Run(() => strExecuteCommandPrompt(
                                m_CON_COMMAND_TASKLIST,
                                string.Empty));

                        await taskExecuteTaskList;

                        strResult = taskExecuteTaskList.Result;

                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}",
                                strResult,
                                Environment.NewLine));

                        strFunctionName.Clear();

                        foreach (Match mFunctionName in Regex.Matches(strResult, m_CON_FUNCTION_NAME_PATTERN))
                        {
                            strFunctionName.Add(mFunctionName.Value);
                        }
                    }
                    while (g_strTaskName.Length != strFunctionName.Distinct().Count());
                }
                catch (Exception ex)
                {
                    string strErrorMessage = "プロセス起動中にエラーが発生しました。";

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "{0}{1}{2}",
                            strErrorMessage,
                            Environment.NewLine,
                            ex.Message));

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "⇒{0}{1}{2}",
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

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}{1}",
                        "⇒全てのプロセスが起動しました。",
                        Environment.NewLine));
                #endregion

                #region エラーファイル削除
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "■エラーファイルを削除します。",
                        Environment.NewLine));

                try
                {
                    foreach (string strFile in m_lstFiles)
                    {
                        strErrorFile = strFile;
                        File.Delete(strErrorFile);
                    }
                }
                catch (Exception ex)
                {
                    string strErrorMessage = "エラーファイルの削除処理中にエラーが発生しました。";

                    // ログ出力
                    WriteEventLog(
                        g_CON_LEVEL_ERROR,
                        string.Format(
                            "{0}{1}対象ファイル:{2}{1}{3}",
                            strErrorMessage,
                            Environment.NewLine,
                            strErrorFile,
                            ex.Message));

                    ExecutionResultTextAdded(
                        false,
                        string.Format(
                            "⇒{0}{1}対象ファイル:{2}{1}{3}",
                            strErrorMessage,
                            Environment.NewLine,
                            strErrorFile,
                            ex.Message));

                    // メッセージ出力
                    MessageBox.Show(
                        strErrorMessage,
                        g_CON_MESSAGE_TITLE_ERROR,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}{1}",
                        "⇒エラーファイルの削除処理が完了しました。",
                        Environment.NewLine));
                #endregion

                ExecutionResultTextAdded(
                    false,
                    "復旧処理が終了しました。");

                dispDataGridView();
            }
            finally
            {
                this.btnRecovery.Enabled = true;
            }
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
        #endregion

        #region メソッド
        /// <summary>
        /// データグリッドビュー表示処理
        /// </summary>
        private void dispDataGridView()
        {
            m_lstInspectionInfo = new List<InspectionInfo>();
            dgvData.Rows.Clear();
            bolGetInspectionInfo();

            // データグリッドビューに反映
            foreach (InspectionInfo row in m_lstInspectionInfo)
            {
                this.dgvData.Rows.Add(true, row.dtInspectionDate, row.intInspectionNum, row.strFabricName, row.dtImagingStarttime);
            }

            // 行選択の状態をリセットする
            dgvData.CurrentCell = null;

            if (dgvData.Rows.Count > 0)
            {
                // 1行目を選択状態にする
                dgvData.Rows[0].Selected = true;
                dgvData.FirstDisplayedScrollingRowIndex = 0;
                dgvData.Rows[0].Cells[m_CON_COL_CHK_SELECT].Value = true;
            }
            else if (bolInitialLaunch)
            {
                // メッセージ出力
                MessageBox.Show(
                    "復旧対象のデータが存在しません。",
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 実行結果テキスト追記
        /// </summary>
        /// <param name="intInitializationFlg">初期化フラグ</param>
        /// <param name="strText">テキスト</param>
        private void ExecutionResultTextAdded(
            bool intInitializationFlg,
            string strText)
        {
            if (intInitializationFlg)
            {
                txtExecutionResult.Text = string.Empty;
            }

            txtExecutionResult.Text += strText;
            txtExecutionResult.SelectionStart = txtExecutionResult.Text.Length;
            txtExecutionResult.Focus();
            txtExecutionResult.ScrollToCaret();
            txtExecutionResult.Refresh();
        }

        /// <summary>
        /// コマンドプロンプト実行
        /// </summary>
        /// <param name="strCommand">コマンド</param>
        /// <param name="strProcessName">プロセスネーム</param>
        private async Task<string> strExecuteCommandPrompt(
            string strCommand,
            string strProcessName)
        {
            string strResult = string.Empty;

            // プロセスの状態を確認する
            using (Process prCmd = new Process())
            {
                prCmd.StartInfo.FileName = m_FILENAME_COMMANDPROMPT;
                prCmd.StartInfo.Arguments =
                    string.Format(
                        strCommand,
                        g_strConnectionPoint,
                        g_strDomain,
                        g_strConnectionPassword,
                        strProcessName);
                prCmd.StartInfo.CreateNoWindow = true;
                prCmd.StartInfo.UseShellExecute = false;
                prCmd.StartInfo.RedirectStandardOutput = true;
                prCmd.Start();
                strResult = prCmd.StandardOutput.ReadToEnd();
                prCmd.WaitForExit();
            }

            return strResult;
        }

        /// <summary>
        /// 検査情報取得
        /// </summary>
        private bool bolGetInspectionInfo()
        {
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();
            InspectionInfo clsInspectionInfo;
            StringBuilder sbSQL = new StringBuilder();

            try
            {
                // SQL文を作成する
                sbSQL.AppendLine(" SELECT ");
                sbSQL.AppendLine("     fi.fabric_name, ");
                sbSQL.AppendLine("     fi.inspection_num, ");
                sbSQL.AppendLine("     fi.imaging_starttime, ");
                sbSQL.AppendLine("     ii.inspection_date ");
                sbSQL.AppendLine(" FROM fabric_info AS fi ");
                sbSQL.AppendLine(" INNER JOIN inspection_info_header AS ii on  ");
                sbSQL.AppendLine(" fi.fabric_name = ii.fabric_name ");
                sbSQL.AppendLine(" AND fi.inspection_num = ii.inspection_num ");
                sbSQL.AppendLine(" AND fi.imaging_starttime = ii.start_datetime ");
                sbSQL.AppendLine(" AND fi.unit_num = ii.unit_num ");
                sbSQL.AppendLine(" AND fi.unit_num = :unit_num ");

                // 反物情報.ステータスの抽出条件を付与する
                if (g_intFabricInfoExtractionStatus.Count() > 0)
                {
                    sbSQL.AppendLine(" AND ( ");

                    for (int index = 0; index < g_intFabricInfoExtractionStatus.Count(); index++)
                    {
                        if (index != 0)
                        {
                            sbSQL.Append(string.Format(" OR"));
                        }

                        sbSQL.AppendLine(string.Format(" fi.status = {0}", g_intFabricInfoExtractionStatus[index]));
                    }

                    sbSQL.AppendLine(" ) ");
                }

                sbSQL.AppendLine(" UNION ");
                sbSQL.AppendLine(" SELECT ");
                sbSQL.AppendLine("     distinct ");
                sbSQL.AppendLine("     ps.fabric_name, ");
                sbSQL.AppendLine("     ps.inspection_num, ");
                sbSQL.AppendLine("     ps.imaging_starttime, ");
                sbSQL.AppendLine("     ii.inspection_date ");
                sbSQL.AppendLine(" FROM processing_status AS ps ");
                sbSQL.AppendLine(" INNER JOIN inspection_info_header AS ii on  ");
                sbSQL.AppendLine(" ps.fabric_name = ii.fabric_name ");
                sbSQL.AppendLine(" AND ps.inspection_num = ii.inspection_num ");
                sbSQL.AppendLine(" AND ps.imaging_starttime = ii.start_datetime ");
                sbSQL.AppendLine(" AND ps.unit_num = ii.unit_num ");
                sbSQL.AppendLine(" AND ps.unit_num = :unit_num ");

                // 処理ステータス.ステータスの抽出条件を付与する
                if (g_intProcessingStatusExtractionStatus.Count() > 0)
                {
                    sbSQL.AppendLine(" AND ( ");

                    for (int index = 0; index < g_intProcessingStatusExtractionStatus.Count(); index++)
                    {
                        if (index != 0)
                        {
                            sbSQL.Append(string.Format(" OR"));
                        }

                        sbSQL.AppendLine(string.Format(" ps.status = {0}", g_intProcessingStatusExtractionStatus[index]));
                    }

                    sbSQL.AppendLine(" ) ");
                }

                sbSQL.AppendLine(" ORDER BY inspection_date, inspection_num, inspection_date ");


                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                // SQLを実行する
                g_clsConnectionNpgsql.SelectSQL(ref dtData, sbSQL.ToString(), lstNpgsqlCommand);

                // 取得した検査情報をリストに格納する
                foreach (DataRow row in dtData.Rows)
                {
                    clsInspectionInfo = new InspectionInfo();
                    clsInspectionInfo.strFabricName = row[0].ToString();
                    clsInspectionInfo.intInspectionNum = int.Parse(row[1].ToString());
                    clsInspectionInfo.dtImagingStarttime = DateTime.Parse(row[2].ToString());
                    clsInspectionInfo.dtInspectionDate = DateTime.Parse(row[3].ToString());
                    m_lstInspectionInfo.Add(clsInspectionInfo);
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage = "復旧対象データの取得中にエラーが発生しました。";

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        strErrorMessage,
                        Environment.NewLine,
                        ex.Message));

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "⇒{0}{1}{2}",
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
            finally
            {
                dtData.Dispose();
            }

            return true;
        }

        /// <summary>
        /// 反物情報テーブル更新
        /// </summary>
        private bool bolUpdateFabricInfo()
        {
            int intExecCount = 0;
            int intInspectionNum = 0;
            DateTime dtInspectionDate = DateTime.MinValue;
            DateTime dtImagingStartTime = DateTime.MinValue;
            string strFabricName = string.Empty;

            try
            {
                // SQL文を作成する
                string strUpdateSql = @"
                    UPDATE fabric_info SET
                        status = :status,
                        imaging_endtime = now(),
                        separateresize_starttime = (case when separateresize_starttime is Null then now() else separateresize_starttime END),
                        separateresize_endtime = (case when separateresize_endtime is Null then now() else separateresize_endtime END),
                        rapid_starttime = (case when rapid_starttime is Null then now() else rapid_starttime END),
                        rapid_endtime = (case when rapid_endtime is Null then now() else rapid_endtime END),
                        imageprocessing_starttime = (case when imageprocessing_starttime is Null then now() else imageprocessing_starttime END),
                        imageprocessing_endtime = (case when imageprocessing_endtime is Null then now() else imageprocessing_endtime END),
                        ng_starttime = (case when ng_starttime is Null then now() else ng_starttime END),
                        ng_endtime = (case when ng_endtime is Null then now() else ng_endtime END),
                        ng_ziptrans_starttime = (case when ng_ziptrans_starttime is Null then now() else ng_ziptrans_starttime END),
                        ng_ziptrans_endtime = (case when ng_ziptrans_endtime is Null then now() else ng_ziptrans_endtime END)
                    WHERE fabric_name = :fabric_name
                        AND inspection_num = :inspection_num
                        AND imaging_starttime = :imaging_starttime
                        AND unit_num = :unit_num";

                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)))
                {
                    strFabricName = dgvRow.Cells[m_CON_COL_FABRICNAME].Value.ToString();
                    intInspectionNum = int.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_NUM].Value.ToString());
                    dtInspectionDate = DateTime.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_DATE].Value.ToString());
                    dtImagingStartTime = DateTime.Parse(dgvRow.Cells[m_CON_COL_IMAGING_STARTTIME_DATE].Value.ToString());

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "status", DbType = DbType.Int32, Value = g_intFabricInfoUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "imaging_starttime", DbType = DbType.DateTime2, Value = dtImagingStartTime });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    intExecCount = g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);
                }

                return true;
            }
            catch (Exception ex)
            {
                g_clsConnectionNpgsql.DbRollback();

                string strErrorMessage = "反物情報テーブルのステータス更新中にエラーが発生しました。";
                string strLogInfo =
                    string.Format(
                        m_CON_LOG_FORMAT,
                        g_strUnitNum,
                        dtInspectionDate.ToString("yyyy/MM/dd"),
                        intInspectionNum,
                        strFabricName);

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
                        ex.Message));

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "⇒{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
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

        /// <summary>
        /// 処理ステータステーブル更新
        /// </summary>
        private bool bolUpdateProcessingStatus()
        {
            int intExecCount = 0;
            int intInspectionNum = 0;
            DateTime dtInspectionDate = DateTime.MinValue;
            DateTime dtImagingStartTime = DateTime.MinValue;
            string strFabricName = string.Empty;

            try
            {
                // SQL文を作成する
                string strUpdateSql = @"
                    UPDATE processing_status SET
                        status = :status
                    WHERE fabric_name = :fabric_name
                        AND inspection_num = :inspection_num
                        AND imaging_starttime = :imaging_starttime
                        AND unit_num = :unit_num";

                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)))
                {
                    strFabricName = dgvRow.Cells[m_CON_COL_FABRICNAME].Value.ToString();
                    intInspectionNum = int.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_NUM].Value.ToString());
                    dtInspectionDate = DateTime.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_DATE].Value.ToString());
                    dtImagingStartTime = DateTime.Parse(dgvRow.Cells[m_CON_COL_IMAGING_STARTTIME_DATE].Value.ToString());

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "status", DbType = DbType.Int32, Value = g_intProcessingStatusUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "imaging_starttime", DbType = DbType.DateTime2, Value = dtImagingStartTime });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    intExecCount = g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);
                }

                return true;
            }
            catch (Exception ex)
            {
                g_clsConnectionNpgsql.DbRollback();

                string strErrorMessage = "処理ステータステーブルのステータス更新中にエラーが発生しました。";
                string strLogInfo =
                    string.Format(
                        m_CON_LOG_FORMAT,
                        g_strUnitNum,
                        dtInspectionDate.ToString("yyyy/MM/dd"),
                        intInspectionNum,
                        strFabricName);

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
                        ex.Message));

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "⇒{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
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

        /// <summary>
        /// RAPID解析情報テーブル更新
        /// </summary>
        private bool bolUpdateRapidAnalysisInfo()
        {
            int intExecCount = 0;
            int intInspectionNum = 0;
            DateTime dtInspectionDate = DateTime.MinValue;
            string strFabricName = string.Empty;
            string strRapidTableName = string.Empty;
            string strUpdateSql = string.Empty;
            bool? bolRapidTableCheckResult = true;

            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)))
                {
                    strFabricName = dgvRow.Cells[m_CON_COL_FABRICNAME].Value.ToString();
                    intInspectionNum = int.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_NUM].Value.ToString());
                    dtInspectionDate = DateTime.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_DATE].Value.ToString());

                    // テーブル名を設定する
                    strRapidTableName = "rapid_" + strFabricName + "_" + intInspectionNum + "_" + dtInspectionDate.ToString("yyyyMMdd");

                    // テーブルの存在チェックを行う
                    bolRapidTableCheckResult = bolCheckRapidTable(strRapidTableName);

                    if (bolRapidTableCheckResult == null)
                    {
                        // 例外発生時は以降の処理をストップする
                        return false;
                    }
                    if (bolRapidTableCheckResult.Equals(false))
                    {
                        // テーブルが存在しない場合、スキップする
                        continue;
                    }

                    // SQL文を作成する
                    strUpdateSql = @"
                        UPDATE """ + strRapidTableName + @""" SET
                            rapid_result = :rapid_result,
                            edge_result = :edge_result,
                            masking_result = :masking_result
                        WHERE fabric_name = :fabric_name
                            AND inspection_num = :inspection_num
                            AND unit_num = :unit_num";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.Int32, Value = g_intRapidAnalysisRapidResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.Int32, Value = g_intRapidAnalysisEdgeResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.Int32, Value = g_intRapidAnalysisMaskingResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    intExecCount = g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);
                }

                return true;
            }
            catch (Exception ex)
            {
                g_clsConnectionNpgsql.DbRollback();

                string strErrorMessage = "RAPID解析情報テーブルのステータス更新中にエラーが発生しました。";
                string strLogInfo =
                    string.Format(
                        m_CON_LOG_FORMAT,
                        g_strUnitNum,
                        dtInspectionDate.ToString("yyyy/MM/dd"),
                        intInspectionNum,
                        strFabricName);

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
                        ex.Message));

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "⇒{0}{1}{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strLogInfo,
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

        /// <summary>
        /// rapidテーブル存在チェック
        /// </summary>
        /// <param name="strRapidTableName">rapidテーブル名</param>
        /// <returns>存在フラグ</returns>
        public bool? bolCheckRapidTable(string strRapidTableName)
        {
            string strSQL = string.Empty;
            DataTable dtData = new DataTable();

            try
            {
                // テーブルの存在チェックする
                strSQL = @"SELECT COUNT(*) AS TableCount
                           FROM information_schema.tables
                           WHERE table_name = :table_name";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "table_name", DbType = DbType.String, Value = strRapidTableName });

                // SQLを実行する
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSQL, lstNpgsqlCommand);

                if (dtData.Rows.Count == 0 ||
                    int.Parse(dtData.Rows[0]["TableCount"].ToString()) == 0)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                string strErrorMessage = "RAPID解析情報テーブルの存在チェック処理中にエラーが発生しました。";

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}対象テーブル:{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strRapidTableName,
                        ex.Message));

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "⇒{0}{1}対象テーブル:{2}{1}{3}",
                        strErrorMessage,
                        Environment.NewLine,
                        strRapidTableName,
                        ex.Message));

                // メッセージ出力
                MessageBox.Show(
                    strErrorMessage,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return null;
            }
            finally
            {
                dtData.Dispose();
            }

            return true;
        }
        #endregion
    }
}