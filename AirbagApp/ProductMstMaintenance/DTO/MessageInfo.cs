using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static ProductMstMaintenance.Common;

namespace ProductMstMaintenance.DTO
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
        public readonly string strMsgE0007;
        public readonly string strMsgE0010;
        public readonly string strMsgE0015;
        public readonly string strMsgE0016;
        public readonly string strMsgE0021;
        public readonly string strMsgE0022;
        public readonly string strMsgE0023;
        public readonly string strMsgE0024;
        public readonly string strMsgE0025;
        public readonly string strMsgE0026;
        public readonly string strMsgE0027;
        public readonly string strMsgE0028;
        public readonly string strMsgE0029;
        public readonly string strMsgE0030;
        public readonly string strMsgE0053;
        public readonly string strMsgE0055;
        public readonly string strMsgE0056;
        public readonly string strMsgE0066;
        public readonly string strMsgE0067;
        public readonly string strMsgI0003;
        public readonly string strMsgI0004;
        public readonly string strMsgI0005;
        public readonly string strMsgI0006;
        public readonly string strMsgI0007;
        public readonly string strMsgI0008;
        public readonly string strMsgI0009;
        public readonly string strMsgI0010;
        public readonly string strMsgI0011;
        public readonly string strMsgI0013;
        public readonly string strMsgQ0005;
        public readonly string strMsgQ0006;
        public readonly string strMsgQ0014;
        public readonly string strMsgW0002;
        public readonly string strMsgW0004;
        public readonly string strMsgW0009;
        public readonly string strMsgW0010;
        public readonly string strMsgW0011;

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
                GetMessageContent("E0007", ref strMsgE0007);
                GetMessageContent("E0010", ref strMsgE0010);
                GetMessageContent("E0015", ref strMsgE0015);
                GetMessageContent("E0016", ref strMsgE0016);
                GetMessageContent("E0021", ref strMsgE0021);
                GetMessageContent("E0022", ref strMsgE0022);
                GetMessageContent("E0023", ref strMsgE0023);
                GetMessageContent("E0024", ref strMsgE0024);
                GetMessageContent("E0025", ref strMsgE0025);
                GetMessageContent("E0026", ref strMsgE0026);
                GetMessageContent("E0027", ref strMsgE0027);
                GetMessageContent("E0028", ref strMsgE0028);
                GetMessageContent("E0029", ref strMsgE0029);
                GetMessageContent("E0030", ref strMsgE0030);
                GetMessageContent("E0053", ref strMsgE0053);
                GetMessageContent("E0055", ref strMsgE0055);
                GetMessageContent("E0056", ref strMsgE0056);
                GetMessageContent("E0066", ref strMsgE0066);
                GetMessageContent("E0067", ref strMsgE0067);
                GetMessageContent("I0003", ref strMsgI0003);
                GetMessageContent("I0004", ref strMsgI0004);
                GetMessageContent("I0005", ref strMsgI0005);
                GetMessageContent("I0006", ref strMsgI0006);
                GetMessageContent("I0007", ref strMsgI0007);
                GetMessageContent("I0008", ref strMsgI0008);
                GetMessageContent("I0009", ref strMsgI0009);
                GetMessageContent("I0010", ref strMsgI0010);
                GetMessageContent("I0011", ref strMsgI0011);
                GetMessageContent("I0013", ref strMsgI0013);
                GetMessageContent("Q0005", ref strMsgQ0005);
                GetMessageContent("Q0006", ref strMsgQ0006);
                GetMessageContent("Q0014", ref strMsgQ0014);
                GetMessageContent("W0002", ref strMsgW0002);
                GetMessageContent("W0004", ref strMsgW0004);
                GetMessageContent("W0009", ref strMsgW0009);
                GetMessageContent("W0010", ref strMsgW0010);
                GetMessageContent("W0011", ref strMsgW0011);

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
            DataRow[] dr = m_dtMessageInfo.Select(string.Format( "id = '{0}'" , strId ));
            if (dr.Length > 0)
            {
                strValue = dr[0]["content"].ToString();
            }
            else
            {
                m_sbErrMessage.AppendLine(string.Format("Id[{0}] メッセージ情報テーブルに存在しません。" , strId ));
            }
        }
    }
}
