using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static WokerMstManagement.Common;

namespace WokerMstManagement.DTO
{
    class SystemSettingInfo
    {
        // 正常終了フラグ
        public readonly bool bolNormalEnd = false;

        // データテーブル
        private DataTable m_dtSystemSettingInfo = new DataTable();

        // エラーメッセージ格納用
        private StringBuilder m_sbErrMessage = new StringBuilder();

        //==============
        // App.Config
        //==============
        // ログ出力ディレクトリ
        public readonly string strLogFileOutputDirectory;
        public readonly string strProcessTypeCre;
        public readonly string strProcessTypeUpd;
        public readonly string strProcessTypeDel;

        //==============================
        //システム情報設定テーブル
        //==============================
        // 

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SystemSettingInfo()
        {
            try
            {
                GetSystemSettingValue();

                // システム設定から情報を取得
                GetSystemSettingValue("LogFileOutputDirectory", ref strLogFileOutputDirectory);
                GetSystemSettingValue("ProcessTypeCre", ref strProcessTypeCre);
                GetSystemSettingValue("ProcessTypeUpd", ref strProcessTypeUpd);
                GetSystemSettingValue("ProcessTypeDel", ref strProcessTypeDel);

                if (m_sbErrMessage.Length > 0)
                {
                    throw new Exception(m_sbErrMessage.ToString());
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format( "システム設定取得時にエラーが発生しました。{0}{1}",Environment.NewLine , ex.Message));
                // メッセージ出力
                MessageBox.Show("システム設定取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetAppConfigValue(string strKey, ref string strValue)
        {
            strValue = ConfigurationManager.AppSettings[strKey];
            if (strValue == null)
            {
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] AppConfigに存在しません。" , strKey ));
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="intValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetAppConfigValue(string strKey, ref int intValue)
        {
            string strValue = string.Empty;

            try
            {
                GetAppConfigValue(strKey, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                m_sbErrMessage.AppendLine(string.Format("Key[{0}] {1}" , strKey , ex.Message));
            }
        }

        /// <summary>
        /// システム設定情報から設定値を取得
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetSystemSettingValue()
        {
            string strSQL = string.Empty;

            try
            {
                // SQL抽出から情報を取得
                m_dtSystemSettingInfo = new DataTable();
                strSQL = @"SELECT * FROM system_setting_info";

                g_clsConnectionNpgsql.SelectSQL(ref m_dtSystemSettingInfo, strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// システム設定値取得
        /// </summary>
        /// <param name="strId">ID</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetSystemSettingValue(string strId, ref int intValue)
        {
            string strValue = string.Empty;

            try
            {
                GetSystemSettingValue(strId, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                m_sbErrMessage.AppendLine(string.Format("Id[{0}] {1}" , strId , ex.Message));
            }
        }

        /// <summary>
        /// システム設定値取得
        /// </summary>
        /// <param name="strId">ID</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        public void GetSystemSettingValue(string strId, ref string strValue)
        {
            DataRow[] dr = m_dtSystemSettingInfo.Select(string.Format("id = '{0}'" , strId ));
            if (dr.Length > 0)
            {
                strValue = dr[0]["value"].ToString();
            }
            else
            {
                m_sbErrMessage.AppendLine(string.Format("Id[{0}] システム情報設定テーブルに存在しません。" , strId ));
            }
        }
    }
}
