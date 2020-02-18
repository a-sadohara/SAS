using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker.DTO
{
    class MessageInfo
    {
        // 正常終了フラグ
        public readonly bool bolNormalEnd = false;

        // データテーブル
        private DataTable m_dtMessageInfo = new DataTable();

        // エラーメッセージ格納用
        private StringBuilder m_sbErrMessage = new StringBuilder();

        //==============================
        // メッセージ内容
        //==============================
        public readonly string strMsgE0001;
        public readonly string strMsgE0002;
        public readonly string strMsgE0003;
        public readonly string strMsgE0021;
        public readonly string strMsgE0031;
        public readonly string strMsgE0035;
        public readonly string strMsgE0038;
        public readonly string strMsgE0039;
        public readonly string strMsgE0040;
        public readonly string strMsgE0041;
        public readonly string strMsgE0042;
        public readonly string strMsgE0043;
        public readonly string strMsgE0045;
        public readonly string strMsgE0046;
        public readonly string strMsgE0050;
        public readonly string strMsgE0051;
        public readonly string strMsgE0052;
        public readonly string strMsgE0054;
        public readonly string strMsgE0057;
        public readonly string strMsgQ0009;
        public readonly string strMsgQ0010;
        public readonly string strMsgQ0011;
        public readonly string strMsgQ0012;
        public readonly string strMsgQ0013;
        public readonly string strMsgW0003;
        public readonly string strMsgW0005;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MessageInfo()
        {
            try
            {
                GetMessageInfo();

                // メッセージ情報から内容を取得
                GetMessageContent("E0001", ref strMsgE0001);
                GetMessageContent("E0002", ref strMsgE0002);
                GetMessageContent("E0003", ref strMsgE0003);
                GetMessageContent("E0021", ref strMsgE0021);
                GetMessageContent("E0031", ref strMsgE0031);
                GetMessageContent("E0035", ref strMsgE0035);
                GetMessageContent("E0038", ref strMsgE0038);
                GetMessageContent("E0039", ref strMsgE0039);
                GetMessageContent("E0040", ref strMsgE0040);
                GetMessageContent("E0041", ref strMsgE0041);
                GetMessageContent("E0042", ref strMsgE0042);
                GetMessageContent("E0043", ref strMsgE0043);
                GetMessageContent("E0045", ref strMsgE0045);
                GetMessageContent("E0046", ref strMsgE0046);
                GetMessageContent("E0050", ref strMsgE0050);
                GetMessageContent("E0051", ref strMsgE0051);
                GetMessageContent("E0052", ref strMsgE0052);
                GetMessageContent("E0054", ref strMsgE0054);
                GetMessageContent("E0057", ref strMsgE0057);
                GetMessageContent("Q0009", ref strMsgQ0009);
                GetMessageContent("Q0010", ref strMsgQ0010);
                GetMessageContent("Q0011", ref strMsgQ0011);
                GetMessageContent("Q0012", ref strMsgQ0012);
                GetMessageContent("Q0013", ref strMsgQ0013);
                GetMessageContent("W0003", ref strMsgW0003);
                GetMessageContent("W0005", ref strMsgW0005);

                if (m_sbErrMessage.Length > 0)
                    throw new Exception(m_sbErrMessage.ToString());

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "メッセージ情報取得時にエラーが発生しました。" + "\r\n" + ex.Message);
                // メッセージ出力
                System.Windows.Forms.MessageBox.Show("メッセージ情報取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// メッセージ情報取得
        /// </summary>
        public void GetMessageInfo()
        {
            string strSQL = "";

            try
            {
                // SQL抽出から情報を取得
                m_dtMessageInfo = new DataTable();
                strSQL = @"SELECT id,content FROM message_info";

                g_clsConnectionNpgsql.SelectSQL(ref m_dtMessageInfo, strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// メッセージ内容取得
        /// </summary>
        /// <param name="strId">ID</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetMessageContent(string strId, ref string strValue)
        {
            DataRow[] dr = m_dtMessageInfo.Select("id = '" + strId + "'");
            if (dr.Length > 0)
            {
                strValue = dr[0]["content"].ToString();
            }
            else
            {
                m_sbErrMessage.AppendLine("Id[" + strId + "] メッセージ情報テーブルに存在しません。");
            }
        }
    }
}
