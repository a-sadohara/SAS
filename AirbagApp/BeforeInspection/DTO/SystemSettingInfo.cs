using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows.Forms;
using static BeforeInspection.Common;

namespace BeforeInspection.DTO
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
        public readonly string strUnitNum;
        public readonly string strImagingDeviceCooperationDirectory;

        //==============================
        //システム情報設定テーブル
        //==============================
        public readonly int intStatusBef;
        public readonly int intStatusChk;
        public readonly int intStatusStp;
        public readonly int intStatusEnd;
        public readonly string strInspectionDirectionS;
        public readonly string strInspectionDirectionX;
        public readonly string strInspectionDirectionY;
        public readonly string strInspectionDirectionR;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SystemSettingInfo()
        {
            try
            {
                GetSystemSettingValue();

                // App.Configから情報を取得
                GetAppConfigValue("UnitNum", ref strUnitNum);

                // システム設定から情報を取得
                GetSystemSettingValue("StatusBef", ref intStatusBef);
                GetSystemSettingValue("StatusChk", ref intStatusChk);
                GetSystemSettingValue("StatusStp", ref intStatusStp);
                GetSystemSettingValue("StatusEnd", ref intStatusEnd);
                GetSystemSettingValue("InspectionDirectionS", ref strInspectionDirectionS);
                GetSystemSettingValue("InspectionDirectionX", ref strInspectionDirectionX);
                GetSystemSettingValue("InspectionDirectionY", ref strInspectionDirectionY);
                GetSystemSettingValue("InspectionDirectionR", ref strInspectionDirectionR);
                GetAppConfigValue("ImagingDeviceCooperationDirectory", ref strImagingDeviceCooperationDirectory);

                if (m_sbErrMessage.Length > 0)
                    throw new Exception(m_sbErrMessage.ToString());

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
                m_sbErrMessage.AppendLine("Key[" + strKey + "] AppConfigに存在しません。");
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
                m_sbErrMessage.AppendLine("Key[" + strKey + "] " + ex.Message);
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
                m_sbErrMessage.AppendLine("Id[" + strId + "] " + ex.Message);
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
                m_sbErrMessage.AppendLine("Id[" + strId + "] システム情報設定テーブルに存在しません。");
            }
        }
    }
}
