using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker
{
    public partial class CheckExcept : Form
    {
        //パラメータ関連
        private int m_intInspectionNum = 0;
        private string m_strUnitNum = "";
        private string m_strOrderImg = "";
        private string m_strFabricName = "";
        private string m_strProductName = "";
        private string m_strInspectionDate = "";
        private string m_strStartDatetime = "";
        private string m_strEndDatetime = "";
        private int m_intInspectionStartLine = -1;
        private int m_intInspectionEndLine = -1;

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

        public CheckExcept(string strUnitNum,
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
            string strSQL = "";

            if (MessageBox.Show(string.Format(g_clsMessageInfo.strMsgQ0010, m_intInspectionNum), "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    // SQL文を作成する
                    strSQL = @"UPDATE " + g_clsSystemSettingInfo.strInstanceName + @".inspection_info_header
                                  SET over_detection_except_status = 4
                                    , acceptance_check_status = 4
                                WHERE fabric_name = :fabric_name
                                  AND TO_CHAR(inspection_date,'YYYY/MM/DD') = :inspection_date
                                  AND inspection_num = :inspection_num";

                    // SQLコマンドに各パラメータを設定する
                    List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
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
                    WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0002 + "\r\n" + ex.Message);
                    // メッセージ出力
                    MessageBox.Show(g_clsMessageInfo.strMsgE0035, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    // DBクローズ
                    g_clsConnectionNpgsql.DbClose();
                }

                this.Close();
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
        #endregion
    }
}
