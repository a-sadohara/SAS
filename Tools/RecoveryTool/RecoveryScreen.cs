using RecoveryTool.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
        private const string m_FILENAME_COMMANDPROMPT = "cmd.exe";
        private const string m_CON_COMMAND_TASKKILL = @"taskkill /S {0} /U {1} /P {2} /FI ""IMAGENAME eq function_*""";
        private const string m_CON_COMMAND_TASKLIST = @"tasklist /S {0} /U {1} /P {2} /FI ""IMAGENAME eq function_*""";
        private const string m_CON_COMMAND_SCHTASKS = @"SCHTASKS /Run /S {0} /U {1} /P {2} /I /TN ""{3} startup""";

        // 検査情報リスト
        private List<InspectionInfo> m_lstInspectionInfo;
        #endregion

        #region イベント
        /// <summary>
        /// 画面初期表示
        /// </summary>
        public RecoveryScreen()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SelectAIModelName_Load(object sender, EventArgs e)
        {
            string strResult = string.Empty;

            // 行選択モードに変更
            this.dgvData.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 新規行を追加させない
            this.dgvData.AllowUserToAddRows = false;

            // 複数選択させない
            this.dgvData.MultiSelect = false;

            IEnumerable<string> lstFiles =
                Directory.EnumerateFiles(
                    g_strErrorFileOutputPath, "*", SearchOption.AllDirectories);

            // エラーファイルの有無確認
            if (lstFiles.Count() == 0)
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

            using (ProgressForm frmProgress = new ProgressForm())
            {
                frmProgress.StartPosition = FormStartPosition.CenterScreen;
                frmProgress.Height = this.Height;
                frmProgress.Width = this.Width;
                frmProgress.Show(this);

                try
                {
                    // プロセスを停止する
                    Task<string> taskExecuteTaskKill =
                        Task.Run(() => strExecuteCommandPrompt(m_CON_COMMAND_TASKKILL));

                    await Task.Delay(10000);
                    await taskExecuteTaskKill;

                    // プロセスの状態を確認する
                    Task<string> taskExecuteTaskList =
                        Task.Run(() => strExecuteCommandPrompt(m_CON_COMMAND_TASKLIST));

                    await taskExecuteTaskList;

                    strResult = taskExecuteTaskList.Result;
                    frmProgress.Close();
                }
                catch (Exception ex)
                {
                    string strErrorMessage = "プロセス停止中にエラーが発生しました";

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
            }

            // ★TaskListの戻り値(strResult)の結果確認

            // 初期表示処理
            dispDataGridView();
        }

        /// <summary>
        /// 復旧ボタン押下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRecovery_Click(object sender, EventArgs e)
        {
            // 反物情報テーブル更新を行う
            bolUpdateFabricInfo();

            // 処理ステータステーブル更新を行う
            bolUpdateProcessingStatus();

            // RAPID解析情報テーブル更新を行う
            bolUpdateRapidAnalysisInfo();

            // ★更新結果確認

            // ★プロセス全起動

            // ★プロセス起動確認

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
            string strSQL = string.Empty;
            m_lstInspectionInfo = new List<InspectionInfo>();
            dgvData.Rows.Clear();
            bolGetFabricInfo();

            // データグリッドビューに反映
            foreach (InspectionInfo row in m_lstInspectionInfo)
            {
                this.dgvData.Rows.Add(true, row.dtInspectionDate, row.intInspectionNum, row.strFabricName);
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
            else
            {
                // メッセージ出力
                MessageBox.Show(
                    "復旧対象のデータが存在しません",
                    g_CON_MESSAGE_TITLE_WARN,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// コマンドプロンプト実行
        /// </summary>
        /// <param name="strCommand">コマンド</param>

        private async Task<string> strExecuteCommandPrompt(string strCommand)
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
                        g_strConnectionPassword);
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
        /// 反物情報テーブル取得
        /// </summary>
        private bool bolGetFabricInfo()
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
                    clsInspectionInfo.dtInspectionDate = DateTime.Parse(row[2].ToString());
                    m_lstInspectionInfo.Add(clsInspectionInfo);
                }
            }
            catch (Exception ex)
            {
                // ログ出力
                //WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0001, Environment.NewLine, ex.Message));

                //// メッセージ出力
                //MessageBox.Show(g_clsMessageInfo.strMsgE0066, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

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

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Selected))
                {
                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "status", DbType = DbType.Int32, Value = g_intFabricInfoUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = dgvRow.Cells[m_CON_COL_FABRICNAME] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = dgvRow.Cells[m_CON_COL_INSPECTION_NUM] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "imaging_starttime", DbType = DbType.DateTime2, Value = dgvRow.Cells[m_CON_COL_INSPECTION_DATE] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                    g_clsConnectionNpgsql.DbCommit();
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                //WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                //// メッセージ出力
                //MessageBox.Show(g_clsMessageInfo.strMsgE0067, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 処理ステータステーブル更新
        /// </summary>
        private bool bolUpdateProcessingStatus()
        {
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

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Selected))
                {
                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "status", DbType = DbType.Int32, Value = g_intProcessingStatusUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = dgvRow.Cells[m_CON_COL_FABRICNAME] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = dgvRow.Cells[m_CON_COL_INSPECTION_NUM] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "imaging_starttime", DbType = DbType.DateTime2, Value = dgvRow.Cells[m_CON_COL_INSPECTION_DATE] });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                    g_clsConnectionNpgsql.DbCommit();
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                //WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                //// メッセージ出力
                //MessageBox.Show(g_clsMessageInfo.strMsgE0067, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// RAPID解析情報テーブル更新
        /// </summary>
        private bool bolUpdateRapidAnalysisInfo()
        {
            string strFabricName = string.Empty;
            int intInspectionNum = 0;
            DateTime dtInspectionDate = DateTime.MinValue;
            string strRapidTableName = string.Empty;
            string strUpdateSql = string.Empty;

            try
            {
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                foreach (DataGridViewRow dgvRow in dgvData.Rows.Cast<DataGridViewRow>().Where(x => x.Cells[m_CON_COL_CHK_SELECT].Selected))
                {
                    strFabricName = dgvRow.Cells[m_CON_COL_FABRICNAME].ToString();
                    intInspectionNum = int.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_NUM].ToString());
                    dtInspectionDate = DateTime.Parse(dgvRow.Cells[m_CON_COL_INSPECTION_DATE].ToString());

                    // テーブル名を設定する
                    strRapidTableName = "rapid_" + strFabricName + "_" + intInspectionNum + "_" + dtInspectionDate.ToString().Replace("/", string.Empty);

                    // SQL文を作成する
                    strUpdateSql = @"
                        UPDATE FROM """ + strRapidTableName + @""" SET
                            rapid_result = :rapid_result,
                            edge_result = :edge_result,
                            masking_result = :masking_result
                        WHERE fabric_name = :fabric_name
                            AND inspection_num = :inspection_num
                            AND unit_num = :unit_num";

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Clear();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "rapid_result", DbType = DbType.String, Value = g_strRapidAnalysisRapidResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "edge_result", DbType = DbType.String, Value = g_strRapidAnalysisEdgeResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "masking_result", DbType = DbType.String, Value = g_strRapidAnalysisMaskingResultUpdateStatus });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int32, Value = intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = g_strUnitNum });

                    // SQLを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);

                    g_clsConnectionNpgsql.DbCommit();
                }

                return true;
            }
            catch (Exception ex)
            {
                // ログ出力
                //WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));

                //// メッセージ出力
                //MessageBox.Show(g_clsMessageInfo.strMsgE0067, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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