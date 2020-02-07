using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static WokerMstManagement.Common;

namespace WokerMstManagement.DTO
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
        public readonly string strMsgE0004;
        public readonly string strMsgE0005;
        public readonly string strMsgE0006;
        public readonly string strMsgE0007;
        public readonly string strMsgE0008;
        public readonly string strMsgE0009;
        public readonly string strMsgE0010;
        public readonly string strMsgE0011;
        public readonly string strMsgE0012;
        public readonly string strMsgE0013;
        public readonly string strMsgE0014;
        public readonly string strMsgE0015;
        public readonly string strMsgE0016;
        public readonly string strMsgE0017;
        public readonly string strMsgE0018;
        public readonly string strMsgE0019;
        public readonly string strMsgE0020;
        public readonly string strMsgE0055;
        public readonly string strMsgI0001;
        public readonly string strMsgI0002;
        public readonly string strMsgQ0001;
        public readonly string strMsgQ0002;
        public readonly string strMsgQ0003;
        public readonly string strMsgQ0004;
        public readonly string strMsgW0001;


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
                GetMessageContent("E0004", ref strMsgE0004);
                GetMessageContent("E0005", ref strMsgE0005);
                GetMessageContent("E0006", ref strMsgE0006);
                GetMessageContent("E0007", ref strMsgE0007);
                GetMessageContent("E0008", ref strMsgE0008);
                GetMessageContent("E0009", ref strMsgE0009);
                GetMessageContent("E0010", ref strMsgE0010);
                GetMessageContent("E0011", ref strMsgE0011);
                GetMessageContent("E0012", ref strMsgE0012);
                GetMessageContent("E0013", ref strMsgE0013);
                GetMessageContent("E0014", ref strMsgE0014);
                GetMessageContent("E0015", ref strMsgE0015);
                GetMessageContent("E0016", ref strMsgE0016);
                GetMessageContent("E0017", ref strMsgE0017);
                GetMessageContent("E0018", ref strMsgE0018);
                GetMessageContent("E0019", ref strMsgE0019);
                GetMessageContent("E0020", ref strMsgE0020);
                GetMessageContent("E0055", ref strMsgE0055);
                GetMessageContent("I0001", ref strMsgI0001);
                GetMessageContent("I0002", ref strMsgI0002);
                GetMessageContent("Q0001", ref strMsgQ0001);
                GetMessageContent("Q0002", ref strMsgQ0002);
                GetMessageContent("Q0003", ref strMsgQ0003);
                GetMessageContent("Q0004", ref strMsgQ0004);
                GetMessageContent("W0001", ref strMsgW0001);

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
