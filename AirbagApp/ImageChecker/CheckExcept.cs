using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class CheckExcept : Form
    {
        // パラメータ関連（不変）
        private readonly int m_intInspectionNum = 0;
        private readonly string m_strUnitNum = string.Empty;
        private readonly string m_strOrderImg = string.Empty;
        private readonly string m_strFabricName = string.Empty;
        private readonly string m_strProductName = string.Empty;
        private readonly string m_strInspectionDate = string.Empty;
        private readonly string m_strStartDatetime = string.Empty;
        private readonly string m_strEndDatetime = string.Empty;

        // パラメータ関連（可変）
        private int m_intInspectionStartLine = -1;
        private int m_intInspectionEndLine = -1;
        private bool m_bolInspection = false;

        #region メソッド
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strUnitNum">号機</param>
        /// <param name="strOrderImg">指図</param>
        /// <param name="strFabricName">反番</param>
        /// <param name="strProductName">品名</param>
        /// <param name="intInspectionNum">検査番号</param>
        /// <param name="strInspectionDate">検査日付</param>
        /// <param name="strStartDatetime">搬送開始日時</param>
        /// <param name="strEndDatetime">搬送終了日時</param>
        /// <param name="intInspectionStartLine">検査開始行</param>
        /// <param name="intInspectionEndLine">最終行数</param>
        /// <param name="bolInspection">検査対象</param>

        public CheckExcept(string strUnitNum,
                           string strOrderImg,
                           string strFabricName,
                           string strProductName,
                           int intInspectionNum,
                           string strInspectionDate,
                           string strStartDatetime,
                           string strEndDatetime,
                           int intInspectionStartLine,
                           int intInspectionEndLine,
                           bool bolInspection)
        {
            // パラメータ設定
            m_strUnitNum = strUnitNum;
            m_strOrderImg = strOrderImg;
            m_strFabricName = strFabricName;
            m_strProductName = strProductName;
            m_intInspectionNum = intInspectionNum;
            m_strInspectionDate = strInspectionDate;
            m_strStartDatetime = strStartDatetime;
            m_strEndDatetime = strEndDatetime;
            m_intInspectionStartLine = intInspectionStartLine;
            m_intInspectionEndLine = intInspectionEndLine;
            m_bolInspection = bolInspection;

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }
        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckExcept_Load(object sender, EventArgs e)
        {
            this.SuspendLayout();

            // ヘッダー表示
            lblUnitNum.Text = m_strUnitNum;
            lblOrderImg.Text = m_strOrderImg;
            lblFabricName.Text = m_strFabricName;
            lblProductName.Text = m_strProductName;
            lblInspectionNum.Text = m_intInspectionNum.ToString();
            lblStartDatetime.Text = m_strStartDatetime;
            lblEndDatetime.Text = m_strEndDatetime;
            lblInspectionLine.Text = m_intInspectionStartLine.ToString() + "～" + m_intInspectionEndLine.ToString();

            this.ResumeLayout();
        }

        /// <summary>
        /// OKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string strMsg = "";
            int intOverDetectionExceptStatus = -1;
            int intAcceptanceCheckStatus = -1;
            string strSQL = string.Empty;

            if (m_bolInspection == true)
            {
                // 検査対象の場合、検査対象外にする
                strMsg = string.Format(g_clsMessageInfo.strMsgQ0010, "検査対象外");
                intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusExc;
                intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusExc;
            }
            else
            {
                // 検査対象外の場合、検査対象にする
                strMsg = string.Format(g_clsMessageInfo.strMsgQ0010, "検査対象");
                intOverDetectionExceptStatus = g_clsSystemSettingInfo.intOverDetectionExceptStatusBef;
                intAcceptanceCheckStatus = g_clsSystemSettingInfo.intAcceptanceCheckStatusBef;
            }

            if (MessageBox.Show(strMsg, g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            try
            {
                // SQL文を作成する
                strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                SET over_detection_except_status = :over_detection_except_status
                                , acceptance_check_status = :acceptance_check_status
                                , decision_start_datetime = current_timestamp
                                , decision_end_datetime = current_timestamp
                            WHERE fabric_name = :fabric_name
                                AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                                AND inspection_num = :inspection_num";

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "over_detection_except_status", DbType = DbType.Int16, Value = intOverDetectionExceptStatus });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "acceptance_check_status", DbType = DbType.Int16, Value = intAcceptanceCheckStatus });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                // DBコミット
                g_clsConnectionNpgsql.DbCommit();
            }
            catch (Exception ex)
            {
                // ロールバック
                g_clsConnectionNpgsql.DbRollback();

                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0002, Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            finally
            {
                // DBクローズ
                g_clsConnectionNpgsql.DbClose();
            }

            this.Close();
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
