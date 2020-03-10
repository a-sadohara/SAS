using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using static DelImage.Program;

namespace DelImage.DTO
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
        // インスタンス名
        public readonly string strInstanceName;

        //==============================
        //システム情報設定テーブル
        //==============================
        // ディレクトリ
        // 欠点画像格納
        public readonly string strFaultImageDirectory;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SystemSettingInfo()
        {
            try
            {
                GetSystemSettingValue();

                // App.Configから情報を取得
                GetAppConfigValue("InstanceName", ref strInstanceName);

                // システム設定から情報を取得
                GetSystemSettingValue("FaultImageDirectory", ref strFaultImageDirectory);

                if (lststrErrorMessage.Count > 0)
                {
                    foreach (string Message in lststrErrorMessage)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_WARN, string.Format("システム設定取得時にエラーが発生しました。{0}{1}", Environment.NewLine, Message));
                    }

                    return;
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_WARN, string.Format("システム設定取得時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));
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
            if (string.IsNullOrEmpty(strValue))
            {
                lststrErrorMessage.Add(string.Format("Key[{0}] AppConfigに存在しません。", strKey));
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
                lststrErrorMessage.Add(string.Format("Key[{0}]{1}", strKey, ex.Message));
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
                lststrErrorMessage.Add(string.Format("Id[{0}]{1}", strId, ex.Message));
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
                lststrErrorMessage.Add(string.Format("Id[{0}] システム情報設定テーブルに存在しません。", strId));
            }
        }
    }
}