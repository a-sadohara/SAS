using RecoveryTool.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private const string m_CON_RESULT_ERROR = "処理の実行に失敗しました。";
        private const string m_CON_CONFIG_ERROR = "設定不備の可能性があるため、Config情報を確認してください。";
        private const string m_CON_RETRY_ERROR = "管理者に連絡してサーバの状態を確認してください。";
        private const string m_CON_UNIT_NUM_FORMAT = "{0}号機";
        private const string m_CON_LOG_FORMAT = "{0}号機、検査日付:{1}、検査番号:{2}、反番:{3}";
        private const string m_CON_TEXT_ACTIV = "起動中";
        private const string m_CON_TEXT_STOP = "停止";

        // 検査情報リスト
        private List<InspectionInfo> m_lstInspectionInfo;

        // エラーファイルリスト
        private IEnumerable<string> m_lstFiles;

        // 初回起動フラグ
        private bool bolInitialLaunchFlg = true;

        // プロセス復旧対象フラグ
        private bool bolProcessTargetFlg = false;
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
            string strResult = string.Empty;
            bool bolProcessCheckResult = true;

            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;

            // 複数選択させない
            this.dgvData.MultiSelect = false;

            // リセットボタンを無効化
            btnRecovery.Enabled = false;

            // プロセスの状態を確認する
            Task<string> taskExecuteTaskList =
                Task.Run(() => strExecuteCommandPrompt(
                    m_CON_COMMAND_TASKLIST,
                    string.Empty));

            await taskExecuteTaskList;

            strResult = taskExecuteTaskList.Result;

            if (string.IsNullOrWhiteSpace(strResult))
            {
                string strErrorMessage =
                    string.Format(
                        "{0}{1}{2}",
                        m_CON_RESULT_ERROR,
                        Environment.NewLine,
                        m_CON_CONFIG_ERROR);

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    strErrorMessage);

                ExecutionResultTextAdded(
                    false,
                    strErrorMessage);

                // メッセージ出力
                MessageBox.Show(
                    strErrorMessage,
                    g_CON_MESSAGE_TITLE_ERROR,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            // プロセスの状態を表示
            ExecutionResultTextAdded(true, string.Format(
                                "{0}{1}",
                                "■連携基盤のプロセス状態を確認します。",
                                Environment.NewLine));


            string strStatus = string.Empty;
            foreach (string strFileName in g_strExecutionFileName)
            {

                if (strResult.Contains(strFileName))
                {
                    strStatus = m_CON_TEXT_ACTIV;
                    bolProcessCheckResult = false;
                }
                else
                {
                    strStatus = m_CON_TEXT_STOP;
                }

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        " ・{0} {1}{2}",
                        strFileName.Replace(".exe", string.Empty),
                        strStatus,
                        Environment.NewLine));

                if (!strResult.Contains(strFileName))
                {
                    bolProcessTargetFlg = true;
                }
            }

            if (bolProcessTargetFlg)
            {
                ExecutionResultTextAdded(false, string.Format(
                    "{0}{1}",
                    "■□■□　　連携基盤のプロセスが異常な状態です。リセットボタンを押してください　　■□■□",
                    Environment.NewLine));
            }
            else
            {
                ExecutionResultTextAdded(false, string.Format("{0}{1}",
                    "⇒　連携基盤のプロセスは正常に稼働しています。",
                    Environment.NewLine));
            }


            ExecutionResultTextAdded(false, string.Format("{0}{1}{2}{3}",
                "-----------------------------------------------------",
                Environment.NewLine,
                "■復旧対象のディレクトリを確認します。",
                Environment.NewLine));

            // アラートファイル格納ディレクトリの有無確認
            if (!Directory.Exists(g_strErrorFileOutputPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "アラートファイル格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strErrorFileOutputPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // 撮像完了通知格納ディレクトリの有無確認
            if (!Directory.Exists(g_strInputScanInfomationPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "撮像完了通知格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strInputScanInfomationPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // 撮像完了通知格納ディレクトリの有無確認
            if (!Directory.Exists(g_strTempScanInfomationPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "撮像完了通知格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strTempScanInfomationPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // レジマーク読取結果格納ディレクトリの有無確認
            if (!Directory.Exists(g_strInputRegistrationMarkPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "レジマーク読取結果格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strInputRegistrationMarkPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // レジマーク読取結果格納ディレクトリの有無確認
            if (!Directory.Exists(g_strTempRegistrationMarkPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "レジマーク読取結果格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strTempRegistrationMarkPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // 行間枚数チェック用ファイル格納ディレクトリの有無確認
            if (!Directory.Exists(g_strImagecheckPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "行間枚数チェック用ファイル格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strImagecheckPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // 行間枚数チェック用ファイル格納ディレクトリの有無確認
            if (!Directory.Exists(g_strImagecheckPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "行間枚数チェック用ファイル格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strImagecheckPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            // 行間枚数チェック用ファイル格納ディレクトリの有無確認
            if (!Directory.Exists(g_strCheckedImagecheckPath))
            {
                // メッセージ出力
                MessageBox.Show(
                    string.Format(
                        "行間枚数チェック用ファイル格納ディレクトリが参照できません。{0}{1}",
                        Environment.NewLine,
                        g_strCheckedImagecheckPath),
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                this.Close();
                return;
            }

            m_lstFiles =
                Directory.EnumerateFiles(
                    g_strErrorFileOutputPath,
                    "*",
                    SearchOption.AllDirectories);

            // エラーファイルの有無確認
            if (m_lstFiles.Count() == 0)
            {
                string strErrorMessage = "⇒アラートファイルは出力されていません。";

                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        strErrorMessage,
                        Environment.NewLine));

            }
            else
            {
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "⇒アラートファイルが出力されています。",
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

            // リセットボタン有効化
            btnRecovery.Enabled = true;

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
                int intTrialCount = 0;
                bool bolProcessCheckResult = true;
                string strResult = string.Empty;
                string strErrorFile = string.Empty;
                string strStatus = string.Empty;
                List<string> strFunctionName = new List<string>();
                List<Task<string>> lstTask = new List<Task<string>>();

                ExecutionResultTextAdded(
                    !bolInitialLaunchFlg,
                    string.Format(
                        "{0}{1}{2}{3}",
                        "-----------------------------------------------------",
                        Environment.NewLine,
                        "■□■□　　　　　リセット処理を開始します。　　　　　　□■□■",
                        Environment.NewLine));

                bolInitialLaunchFlg = false;


                if (dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Value.Equals(true)).Count() != 0)
                {

                    if (MessageBox.Show("当該検査番号は、検査無効となります。本当に 【リセット】 しますか？", "リセット確認",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    {
                        ExecutionResultTextAdded(
                            false,
                            string.Format("{0}{1}",
                            "⇒　処理を中断します。",
                            Environment.NewLine));
                        return;
                    }

                    bolProcessTargetFlg = true;
                }
                else
                {

                    if (bolProcessTargetFlg)
                    {
                        if (MessageBox.Show("本当に復旧しますか?", "リセット確認",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            ExecutionResultTextAdded(
                                false,
                                string.Format("{0}{1}",
                                "⇒　処理を中断します。",
                                Environment.NewLine));
                            return;
                        }

                    }
                }


                // プロセス復旧が必要な場合、下記処理を行う
                if (bolProcessTargetFlg)
                {
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

                        if (string.IsNullOrWhiteSpace(taskExecuteTaskKill.Result))
                        {
                            string strErrorMessage =
                                string.Format(
                                    "{0}{1}{2}",
                                    m_CON_RESULT_ERROR,
                                    Environment.NewLine,
                                    m_CON_CONFIG_ERROR);

                            // ログ出力
                            WriteEventLog(
                                g_CON_LEVEL_ERROR,
                                strErrorMessage);

                            ExecutionResultTextAdded(
                                false,
                                strErrorMessage);

                            // メッセージ出力
                            MessageBox.Show(
                                strErrorMessage,
                                g_CON_MESSAGE_TITLE_ERROR,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                            return;
                        }

                        await Task.Delay(2000);

                        do
                        {
                            if (intTrialCount > g_intRetryTimes)
                            {
                                string strErrorMessage =
                                    string.Format(
                                        "{0}{1}{2}",
                                        "プロセスを停止できませんでした。",
                                        Environment.NewLine,
                                        m_CON_RETRY_ERROR);

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    strErrorMessage);

                                ExecutionResultTextAdded(
                                    false,
                                    strErrorMessage);

                                // メッセージ出力
                                MessageBox.Show(
                                    strErrorMessage,
                                    g_CON_MESSAGE_TITLE_ERROR,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(strResult))
                            {
                                ExecutionResultTextAdded(
                                    false,
                                    string.Format(
                                        "⇒全てのプロセス停止が確認できないため、{0}秒後に再確認します。{1}{1}",
                                        g_intRetryWaitSeconds,
                                        Environment.NewLine));

                                await Task.Delay(g_intRetryWaitMilliSeconds);
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

                            if (string.IsNullOrWhiteSpace(strResult))
                            {
                                string strErrorMessage =
                                    string.Format(
                                        "{0}{1}{2}",
                                        m_CON_RESULT_ERROR,
                                        Environment.NewLine,
                                        m_CON_CONFIG_ERROR);

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    strErrorMessage);

                                ExecutionResultTextAdded(
                                    false,
                                    strErrorMessage);

                                // メッセージ出力
                                MessageBox.Show(
                                    strErrorMessage,
                                    g_CON_MESSAGE_TITLE_ERROR,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                                return;
                            }

                            bolProcessCheckResult = true;

                            foreach (string strFileName in g_strExecutionFileName)
                            {
                                if (strResult.Contains(strFileName))
                                {
                                    strStatus = m_CON_TEXT_ACTIV;
                                    bolProcessCheckResult = false;
                                }
                                else
                                {
                                    strStatus = m_CON_TEXT_STOP;
                                }

                                ExecutionResultTextAdded(
                                    false,
                                    string.Format(
                                        " ・{0} {1}{2}",
                                        strFileName.Replace(".exe", string.Empty),
                                        strStatus,
                                        Environment.NewLine));
                            }

                            intTrialCount++;
                        }
                        while (!bolProcessCheckResult);
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

                        #region ファイル移動
                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}",
                                "■作業用ファイルの移動を開始します。",
                                Environment.NewLine));

                        // 撮像完了通知の移動を行う。
                        Task<bool> taskMoveFile =
                            Task<bool>.Run(() => bolMoveFile(
                                g_strInputScanInfomationPath,
                                g_strTempScanInfomationPath,
                                "撮像完了通知",
                                true));

                        await taskMoveFile;

                        if (!taskMoveFile.Result)
                        {
                            return;
                        }

                        // レジマーク読取結果の移動を行う。
                        taskMoveFile =
                            Task<bool>.Run(() => bolMoveFile(
                                g_strInputRegistrationMarkPath,
                                g_strTempRegistrationMarkPath,
                                "レジマーク読取結果",
                                true));

                        await taskMoveFile;

                        if (!taskMoveFile.Result)
                        {
                            return;
                        }

                        // 行間枚数チェックファイルの移動を行う。
                        taskMoveFile =
                            Task<bool>.Run(() => bolMoveFile(
                                g_strImagecheckPath,
                                g_strCheckedImagecheckPath,
                                "行間枚数チェックファイル",
                                false));

                        await taskMoveFile;

                        if (!taskMoveFile.Result)
                        {
                            return;
                        }

                        ExecutionResultTextAdded(
                            false,
                            string.Format(
                                "{0}{1}{1}",
                                "⇒作業用ファイルの移動が完了しました。",
                                Environment.NewLine));
                        #endregion
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
                        await Task.Delay(2000);

                        strResult = string.Empty;
                        intTrialCount = 0;

                        do
                        {
                            if (intTrialCount > g_intRetryTimes)
                            {
                                string strErrorMessage =
                                    string.Format(
                                        "{0}{1}{2}",
                                        "プロセスを起動できませんでした。",
                                        Environment.NewLine,
                                        m_CON_RETRY_ERROR);

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    strErrorMessage);

                                ExecutionResultTextAdded(
                                    false,
                                    strErrorMessage);

                                // メッセージ出力
                                MessageBox.Show(
                                    strErrorMessage,
                                    g_CON_MESSAGE_TITLE_ERROR,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                                return;
                            }

                            if (!string.IsNullOrWhiteSpace(strResult))
                            {
                                ExecutionResultTextAdded(
                                    false,
                                    string.Format(
                                        "⇒全てのプロセス起動が確認できないため、{0}秒後に再確認します。{1}{1}",
                                        g_intRetryWaitSeconds,
                                        Environment.NewLine));

                                await Task.Delay(g_intRetryWaitMilliSeconds);
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

                            if (string.IsNullOrWhiteSpace(strResult))
                            {
                                string strErrorMessage =
                                    string.Format(
                                        "{0}{1}{2}",
                                        m_CON_RESULT_ERROR,
                                        Environment.NewLine,
                                        m_CON_CONFIG_ERROR);

                                // ログ出力
                                WriteEventLog(
                                    g_CON_LEVEL_ERROR,
                                    strErrorMessage);

                                ExecutionResultTextAdded(
                                    false,
                                    strErrorMessage);

                                // メッセージ出力
                                MessageBox.Show(
                                    strErrorMessage,
                                    g_CON_MESSAGE_TITLE_ERROR,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);

                                return;
                            }

                            bolProcessCheckResult = true;
                            strFunctionName.Clear();

                            foreach (string strFileName in g_strExecutionFileName)
                            {
                                if (strResult.Contains(strFileName))
                                {
                                    strStatus = m_CON_TEXT_ACTIV;
                                }
                                else
                                {
                                    strStatus = m_CON_TEXT_STOP;
                                    bolProcessCheckResult = false;
                                }

                                ExecutionResultTextAdded(
                                    false,
                                    string.Format(
                                        " ・{0} {1}{2}",
                                        strFileName.Replace(".exe", string.Empty),
                                        strStatus,
                                        Environment.NewLine));
                            }

                            intTrialCount++;
                        }
                        while (!bolProcessCheckResult);
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
                }

                #region エラーファイル削除
                ExecutionResultTextAdded(
                    false,
                    string.Format(
                        "{0}{1}",
                        "■アラートファイルを削除します。",
                        Environment.NewLine));

                try
                {
                    if (bolProcessTargetFlg)
                    {
                        // リカバリ済みのため全てのアラートファイルを削除
                        foreach (string strFile in m_lstFiles)
                        {
                            File.Delete(strFile);
                            ExecutionResultTextAdded(
                                false,
                                string.Format("{0}{1}{2}",
                                "   ・",
                                strFile,
                                Environment.NewLine));
                        }
                    }
                    else
                    {
                        foreach (string strFile in m_lstFiles)
                        {
                            // リカバリ対象外のアラートファイルを削除   
                            foreach (string strTagetFile in g_strRecoveryExemptErrorFileName)
                            {
                                if (Path.GetFileName(strFile).StartsWith(strTagetFile))
                                {
                                    File.Delete(strFile);
                                    ExecutionResultTextAdded(
                                        false,
                                        string.Format("{0}{1}{2}",
                                        "   ・",
                                        strFile,
                                        Environment.NewLine));
                                }
                            }
                        }

                    }


                }
                catch (Exception ex)
                {
                    string strErrorMessage = "アラートファイルの削除処理中にエラーが発生しました。";

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
                        "⇒アラートファイルの削除処理が完了しました。",
                        Environment.NewLine));
                #endregion

                ExecutionResultTextAdded(
                    false,
                    "■□■□　　　　　リセット処理が終了しました。　　　　　■□■□");

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
                this.dgvData.Rows.Add(false, row.dtInspectionDate, row.intInspectionNum, row.strFabricName, row.dtImagingStarttime);
            }

            // 行選択の状態をリセットする
            dgvData.CurrentCell = null;
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

        delegate void delegate1(
            bool intInitializationFlg,
            string strText);

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
                        g_strConnectionUser,
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
                for (int index = 0; index < g_intFabricInfoExceptExtractionStatus.Count(); index++)
                {
                    sbSQL.AppendLine(string.Format(" AND fi.status <> {0}", g_intFabricInfoExceptExtractionStatus[index]));
                }

                sbSQL.AppendLine(" ORDER BY ii.inspection_date, fi.inspection_num, fi.fabric_name ");


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
        private bool? bolCheckRapidTable(string strRapidTableName)
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

        /// <summary>
        /// ファイル移動
        /// 「全ファイル」or「CSVファイルのみ」の移動を行う
        /// </summary>
        /// <param name="strSourcePath">移動元パス</param>
        /// <param name="strDestinationPath">移動先パス</param>
        /// <param name="targetDirectoryName">対象ディレクトリ名称</param>
        /// <param name="bolAllFilesMoveFlag">全ファイル移動フラグ</param>
        /// <returns>存在フラグ</returns>
        private async Task<bool> bolMoveFile(
            string strSourcePath,
            string strDestinationPath,
            string targetDirectoryName,
            bool bolAllFilesMoveFlag)
        {
            int intTrialCount = 0;
            int intFileCount = 0;
            int intRemainingCount = 0;
            string strDestinationName = string.Empty;
            FileInfo[] fiTargetInfo = null;

            try
            {
                Invoke(
                    new delegate1(ExecutionResultTextAdded),
                    false,
                    string.Format(
                        "{0}ファイルの移動を開始します。{1}",
                        targetDirectoryName,
                        Environment.NewLine));

                // 移動元のファイル情報を取得する。
                fiTargetInfo =
                    GetFileInfo(
                        strSourcePath,
                        bolAllFilesMoveFlag);

                intFileCount = fiTargetInfo.Length;

                do
                {
                    if (intTrialCount > g_intRetryTimes)
                    {
                        string strErrorMessage =
                            string.Format(
                                "{0}ファイルの移動に失敗しました。{1}{1}移動元パス：{2}{1}{1}移動先パス：{3}{1}{1}総ファイル数：{4}, 残ファイル数：{5}{1}{1}{6}",
                                targetDirectoryName,
                                Environment.NewLine,
                                strSourcePath,
                                strDestinationPath,
                                intFileCount,
                                intRemainingCount,
                                m_CON_RETRY_ERROR);

                        // ログ出力
                        WriteEventLog(
                            g_CON_LEVEL_ERROR,
                            strErrorMessage);

                        Invoke(
                            new delegate1(ExecutionResultTextAdded),
                            false,
                            strErrorMessage);

                        // メッセージ出力
                        MessageBox.Show(
                            strErrorMessage,
                            g_CON_MESSAGE_TITLE_ERROR,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return false;
                    }

                    if (intRemainingCount != 0)
                    {
                        Invoke(
                            new delegate1(ExecutionResultTextAdded),
                            false,
                            string.Format(
                                "⇒全ファイルの移動が確認できないため、{0}秒後に再確認します。{1}総ファイル数：{2}, 残ファイル数：{3}{1}{1}",
                                g_intRetryWaitSeconds,
                                Environment.NewLine,
                                intFileCount,
                                intRemainingCount));

                        await Task.Delay(g_intRetryWaitMilliSeconds);
                    }

                    // ファイル移動(上書きコピー+元ファイル削除)を行う。
                    foreach (FileInfo fiSourceInfo in fiTargetInfo)
                    {
                        strDestinationName = Path.Combine(strDestinationPath, fiSourceInfo.Name);
                        try
                        {
                            File.Copy(fiSourceInfo.FullName, strDestinationName, true);
                            File.Delete(fiSourceInfo.FullName);
                        }
                        catch (Exception ex)
                        {
                            // リトライ処理実行のため、例外をスキップする。
                        }
                    }

                    // 移動元のファイル情報を取得する。
                    fiTargetInfo =
                        GetFileInfo(
                            strSourcePath,
                            bolAllFilesMoveFlag);

                    intRemainingCount = fiTargetInfo.Length;
                    intTrialCount++;
                }
                while (intRemainingCount != 0);

                Invoke(
                    new delegate1(ExecutionResultTextAdded),
                    false,
                    string.Format(
                        "{0}ファイルの移動が完了しました。{1}総ファイル数：{2}{1}{1}",
                        targetDirectoryName,
                        Environment.NewLine,
                        intFileCount));

                return true;
            }
            catch (Exception ex)
            {
                string strErrorMessage =
                    string.Format(
                        "ファイル移動中にエラーが発生しました。{0}移動元パス：{1}{0}移動先パス：{2}{0}{3}",
                        Environment.NewLine,
                        strSourcePath,
                        strDestinationPath,
                        m_CON_RETRY_ERROR);

                // ログ出力
                WriteEventLog(
                    g_CON_LEVEL_ERROR,
                    string.Format(
                        "{0}{1}{2}",
                        strErrorMessage,
                        Environment.NewLine,
                        ex.Message));

                Invoke(
                    new delegate1(ExecutionResultTextAdded),
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
        }

        /// <summary>
        /// ファイル情報取得
        /// </summary>
        /// <param name="strTargetPath">対象ディレクトリ名称</param>
        /// <param name="bolAllFilesMoveFlag">全ファイル移動フラグ</param>
        /// <returns>ファイル情報</returns>
        private FileInfo[] GetFileInfo(
            string strTargetPath,
            bool bolAllFilesMoveFlag)
        {
            if (bolAllFilesMoveFlag)
            {
                return new DirectoryInfo(strTargetPath).GetFiles().ToArray();
            }
            else
            {
                return new DirectoryInfo(strTargetPath).GetFiles().Where(x => string.Compare(x.Extension, ".csv", true) == 0).ToArray();
            }
        }
        #endregion
    }
}