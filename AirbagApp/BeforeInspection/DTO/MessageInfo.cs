﻿using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection.DTO
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
        public readonly string strMsgE0003;
        public readonly string strMsgE0007;
        public readonly string strMsgE0012;
        public readonly string strMsgE0013;
        public readonly string strMsgE0021;
        public readonly string strMsgE0031;
        public readonly string strMsgE0032;
        public readonly string strMsgE0033;
        public readonly string strMsgE0034;
        public readonly string strMsgE0035;
        public readonly string strMsgE0036;
        public readonly string strMsgE0037;
        public readonly string strMsgE0065;
        public readonly string strMsgE0069;
        public readonly string strMsgE0071;
        public readonly string strMsgE0073;
        public readonly string strMsgE0074;
        public readonly string strMsgE0075;
        public readonly string strMsgQ0008;
        public readonly string strMsgW0013;

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
                GetMessageContent("E0003", ref strMsgE0003);
                GetMessageContent("E0007", ref strMsgE0007);
                GetMessageContent("E0012", ref strMsgE0012);
                GetMessageContent("E0013", ref strMsgE0013);
                GetMessageContent("E0021", ref strMsgE0021);
                GetMessageContent("E0031", ref strMsgE0031);
                GetMessageContent("E0032", ref strMsgE0032);
                GetMessageContent("E0033", ref strMsgE0033);
                GetMessageContent("E0034", ref strMsgE0034);
                GetMessageContent("E0035", ref strMsgE0035);
                GetMessageContent("E0036", ref strMsgE0036);
                GetMessageContent("E0037", ref strMsgE0037);
                GetMessageContent("E0065", ref strMsgE0065);
                GetMessageContent("E0069", ref strMsgE0069);
                GetMessageContent("E0071", ref strMsgE0071);
                GetMessageContent("E0073", ref strMsgE0073);
                GetMessageContent("E0074", ref strMsgE0074);
                GetMessageContent("E0075", ref strMsgE0075);
                GetMessageContent("Q0008", ref strMsgQ0008);
                GetMessageContent("W0013", ref strMsgW0013);

                if (m_sbErrMessage.Length > 0)
                {
                    throw new Exception(m_sbErrMessage.ToString());
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format( "メッセージ情報取得時にエラーが発生しました。{0}{1}" ,Environment.NewLine, ex.Message));
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
            DataRow[] dr = m_dtMessageInfo.Select(string.Format("id = '{0}'" , strId ));
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
