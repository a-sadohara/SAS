using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        private List<String> lststrErrorMessage = new List<String>();

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
                GetAppConfigValue("LogFileOutputDirectory", ref strLogFileOutputDirectory);
                GetAppConfigValue("ProcessTypeCre", ref strProcessTypeCre);
                GetAppConfigValue("ProcessTypeUpd", ref strProcessTypeUpd);
                GetAppConfigValue("ProcessTypeDel", ref strProcessTypeDel);

                if (lststrErrorMessage.Count > 0)
                {
                    foreach (string Message in lststrErrorMessage)
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, "システム設定取得時にエラーが発生しました。" + "\r\n" + Message);

                    // メッセージ出力
                    MessageBox.Show("システム設定取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "システム設定取得時にエラーが発生しました。" + "\r\n" + ex.Message);
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
                lststrErrorMessage.Add("Key[" + strKey + "] AppConfigに存在しません。");
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
            string strValue = "";

            try
            {
                GetAppConfigValue(strKey, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                lststrErrorMessage.Add("Key[" + strKey + "] " + ex.Message);
            }
        }

        /// <summary>
        /// システム設定情報から設定値を取得
        /// </summary>
        /// <returns>true:正常終了 false:異常終了</returns>
        private void GetSystemSettingValue()
        {
            string strSQL = "";

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
            string strValue = "";

            try
            {
                GetSystemSettingValue(strId, ref strValue);
                intValue = int.Parse(strValue);
            }
            catch (Exception ex)
            {
                lststrErrorMessage.Add("Id[" + strId + "] " + ex.Message);
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
            DataRow[] dr = m_dtSystemSettingInfo.Select("id = '" + strId + "'");
            if (dr.Length > 0)
            {
                strValue = dr[0]["value"].ToString();
            }
            else
            {
                lststrErrorMessage.Add("Id[" + strId + "] システム情報設定テーブルに存在しません。");
            }
        }
    }
}
