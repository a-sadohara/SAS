using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class LineCorrect : Form
    {
        // 定義
        private const string m_CON_FORMAT_CORRECT = "補正値：{0}";

        // パラメータ関連（不変）
        private readonly int m_intInspectionNum = 0;
        private readonly string m_strUnitNum = "";
        private readonly string m_strOrderImg = "";
        private readonly string m_strFabricName = "";
        private readonly string m_strProductName = "";
        private readonly string m_strInspectionDate = "";
        private readonly string m_strStartDatetime = "";
        private readonly string m_strEndDatetime = "";
        private int m_intInspectionStartLine = -1;

        // パラメータ関連（可変）
        private int m_intInspectionEndLine = -1;
        private int m_Correct = 0;

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
        public LineCorrect(string strUnitNum,
                           string strOrderImg,
                           string strFabricName,
                           string strProductName,
                           int intInspectionNum,
                           string strInspectionDate,
                           string strStartDatetime,
                           string strEndDatetime,
                           int intInspectionStartLine,
                           int intInspectionEndLine)
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

            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 補正値表示
        /// </summary>
        private void DispCorrect(int intCorrect)
        {
            m_Correct += intCorrect;
            m_intInspectionStartLine += intCorrect;
            m_intInspectionEndLine += intCorrect;

            if (m_Correct == 0)
            {
                lblCorrect.Text = string.Format(m_CON_FORMAT_CORRECT, "±" + m_Correct.ToString());
            }
            if (m_Correct > 0)
            {
                lblCorrect.Text = string.Format(m_CON_FORMAT_CORRECT, "+" + m_Correct.ToString());
            }
            if (m_Correct < 0)
            {
                lblCorrect.Text = string.Format(m_CON_FORMAT_CORRECT, m_Correct.ToString());
            }

            lblInspectionLineAfter.Text = m_intInspectionStartLine.ToString() + "～" + m_intInspectionEndLine.ToString();
        }

        #endregion

        #region イベント
        /// <summary>
        /// フォームロード
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LineCorrect_Load(object sender, EventArgs e)
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
            lblCorrect.Text = "補正値：±0";

            // 行補正値設定
            m_Correct = 0;
            lblInspectionLineBefore.Text = m_intInspectionStartLine.ToString() + "～" + m_intInspectionEndLine.ToString();
            lblInspectionLineAfter.Text = m_intInspectionStartLine.ToString() + "～" + m_intInspectionEndLine.ToString();

            // マイナスにならないように無効にする
            if (m_intInspectionStartLine + m_Correct <= 1)
            {
                btnMinus.Enabled = false;
            }

            this.ResumeLayout();
        }

        /// <summary>
        /// OKボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            string strSQL = "";
            string strCorrect = "";

            if (m_Correct > 0)
            {
                strCorrect = "+" + m_Correct.ToString();
            }
            else
            {
                strCorrect = m_Correct.ToString();
            }

            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0009,
                                              strCorrect), g_CON_MESSAGE_TITLE_QUESTION, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // 検査行範囲の補正
                try
                {
                    // SQL文を作成する
                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                  SET inspection_start_line = inspection_start_line + :inspection_line_correct
                                    , inspection_end_line = inspection_end_line + :inspection_line_correct
                                WHERE fabric_name = :fabric_name
                                  AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                                  AND inspection_num = :inspection_num
                                  AND unit_num = :unit_num";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_line_correct", DbType = DbType.Int16, Value = m_Correct });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_end_line", DbType = DbType.Int16, Value = m_Correct });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);
                }
                catch (Exception ex)
                {
                    // ロールバック
                    g_clsConnectionNpgsql.DbRollback();

                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0035, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                // 行の補正
                try
                {
                    // SQL文を作成する
                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".decision_result
                                  SET line = line + :line_correct
                                WHERE fabric_name = :fabric_name
                                  AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                                  AND inspection_num = :inspection_num
                                  AND unit_num = :unit_num";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "line_correct", DbType = DbType.Int16, Value = m_Correct });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "fabric_name", DbType = DbType.String, Value = m_strFabricName });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_date", DbType = DbType.String, Value = m_strInspectionDate });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "inspection_num", DbType = DbType.Int16, Value = m_intInspectionNum });
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "unit_num", DbType = DbType.String, Value = m_strUnitNum });

                    // sqlを実行する
                    g_clsConnectionNpgsql.ExecTranSQL(strSQL, lstNpgsqlCommand);

                    // DBコミット
                    g_clsConnectionNpgsql.DbCommit();

                    this.Close();
                }
                catch (Exception ex)
                {
                    // ロールバック
                    g_clsConnectionNpgsql.DbRollback();

                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0043, g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    // DBクローズ
                    g_clsConnectionNpgsql.DbClose();
                }
            }
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

        /// <summary>
        /// マイナスボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMinus_Click(object sender, EventArgs e)
        {
            DispCorrect(-1);

            // 0にならないように無効にする
            if (m_intInspectionStartLine <= 1)
            {
                btnMinus.Enabled = false;
            }
        }

        /// <summary>
        /// プラスボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlus_Click(object sender, EventArgs e)
        {
            DispCorrect(1);

            // 0にはならないので有効にする
            if (m_intInspectionStartLine > 1)
            {
                btnMinus.Enabled = true;
            }
        }
        #endregion
    }
}