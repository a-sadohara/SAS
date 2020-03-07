using System;
using System.Data;
using System.Text;
using static DelRecord.Program;

namespace DelRecord.DTO
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
        public readonly string strMsgE0011;
        public readonly string strMsgE0021;
        public readonly string strMsgE0030;
        public readonly string strMsgE0031;
        public readonly string strMsgE0034;
        public readonly string strMsgE0035;
        public readonly string strMsgE0038;
        public readonly string strMsgE0039;
        public readonly string strMsgE0040;
        public readonly string strMsgE0041;
        public readonly string strMsgE0042;
        public readonly string strMsgE0043;
        public readonly string strMsgE0044;
        public readonly string strMsgE0047;
        public readonly string strMsgE0048;
        public readonly string strMsgE0049;
        public readonly string strMsgE0050;
        public readonly string strMsgE0051;
        public readonly string strMsgE0052;
        public readonly string strMsgE0054;
        public readonly string strMsgE0057;
        public readonly string strMsgE0058;
        public readonly string strMsgE0059;
        public readonly string strMsgE0060;
        public readonly string strMsgE0061;
        public readonly string strMsgE0062;
        public readonly string strMsgQ0009;
        public readonly string strMsgQ0010;
        public readonly string strMsgQ0011;
        public readonly string strMsgQ0012;
        public readonly string strMsgQ0013;
        public readonly string strMsgW0003;
        public readonly string strMsgW0005;
        public readonly string strMsgW0006;
        public readonly string strMsgW0007;
        public readonly string strMsgW0008;

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
                GetMessageContent("E0011", ref strMsgE0011);
                GetMessageContent("E0021", ref strMsgE0021);
                GetMessageContent("E0030", ref strMsgE0030);
                GetMessageContent("E0031", ref strMsgE0031);
                GetMessageContent("E0034", ref strMsgE0034);
                GetMessageContent("E0035", ref strMsgE0035);
                GetMessageContent("E0038", ref strMsgE0038);
                GetMessageContent("E0039", ref strMsgE0039);
                GetMessageContent("E0040", ref strMsgE0040);
                GetMessageContent("E0041", ref strMsgE0041);
                GetMessageContent("E0042", ref strMsgE0042);
                GetMessageContent("E0043", ref strMsgE0043);
                GetMessageContent("E0044", ref strMsgE0044);
                GetMessageContent("E0047", ref strMsgE0047);
                GetMessageContent("E0048", ref strMsgE0048);
                GetMessageContent("E0049", ref strMsgE0049);
                GetMessageContent("E0050", ref strMsgE0050);
                GetMessageContent("E0051", ref strMsgE0051);
                GetMessageContent("E0052", ref strMsgE0052);
                GetMessageContent("E0054", ref strMsgE0054);
                GetMessageContent("E0057", ref strMsgE0057);
                GetMessageContent("E0058", ref strMsgE0058);
                GetMessageContent("E0059", ref strMsgE0059);
                GetMessageContent("E0060", ref strMsgE0060);
                GetMessageContent("E0061", ref strMsgE0061);
                GetMessageContent("E0062", ref strMsgE0062);
                GetMessageContent("Q0009", ref strMsgQ0009);
                GetMessageContent("Q0010", ref strMsgQ0010);
                GetMessageContent("Q0011", ref strMsgQ0011);
                GetMessageContent("Q0012", ref strMsgQ0012);
                GetMessageContent("Q0013", ref strMsgQ0013);
                GetMessageContent("W0003", ref strMsgW0003);
                GetMessageContent("W0005", ref strMsgW0005);
                GetMessageContent("W0006", ref strMsgW0006);
                GetMessageContent("W0007", ref strMsgW0007);
                GetMessageContent("W0008", ref strMsgW0008);

                if (m_sbErrMessage.Length > 0)
                {
                    throw new Exception(m_sbErrMessage.ToString());
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format( "メッセージ情報取得時にエラーが発生しました。{0}{1} ",Environment.NewLine , ex.Message));
                
                // メッセージ出力
                Console.WriteLine("メッセージ情報取得時に例外が発生しました。");
            }

        }

        /// <summary>
        /// メッセージ情報取得
        /// </summary>
        public void GetMessageInfo()
        {
            string strSQL = string.Empty;

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
