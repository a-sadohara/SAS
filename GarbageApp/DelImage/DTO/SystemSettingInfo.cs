﻿using System;
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
        public readonly string strNgImageCooperationDirectory;                      // NG画像連携
        public readonly string strCompletionNoticeCooperationDirectory;             // 完了通知連携
        public readonly string strFaultImageDirectory;                              // 欠点画像格納
        public readonly string strMasterImageDirectory;                             // マスタ画像格納
        public readonly string strNotDetectedImageCooperationDirectory;             // 未検知画像連携
        public readonly string strProductionManagementCooperationDirectory;         // 生産管理システム連携
        public readonly string strInspectionResltCsvDirectory;                      // 検査結果CSV保存先

        // 連携基盤部インスタンス名
        public readonly string strCooperationBaseInstanceName;
        // 待ち時間
        public readonly int intWaitingTimeProcessed;
        // 検査方向
        public readonly string strInspectionDirectionS;
        public readonly string strInspectionDirectionX;
        public readonly string strInspectionDirectionY;
        public readonly string strInspectionDirectionR;
        // 過検知除外ステータス
        public readonly int intOverDetectionExceptStatusBef;
        public readonly int intOverDetectionExceptStatusChk;
        public readonly int intOverDetectionExceptStatusStp;
        public readonly int intOverDetectionExceptStatusEnd;
        public readonly int intOverDetectionExceptStatusExc;
        public readonly string strOverDetectionExceptStatusNameBef;
        public readonly string strOverDetectionExceptStatusNameChk;
        public readonly string strOverDetectionExceptStatusNameStp;
        public readonly string strOverDetectionExceptStatusNameEnd;
        public readonly string strOverDetectionExceptStatusNameExc;
        // 合否確認ステータス
        public readonly int intAcceptanceCheckStatusBef;
        public readonly int intAcceptanceCheckStatusChk;
        public readonly int intAcceptanceCheckStatusStp;
        public readonly int intAcceptanceCheckStatusEnd;
        public readonly int intAcceptanceCheckStatusExc;
        public readonly string strAcceptanceCheckStatusNameBef;
        public readonly string strAcceptanceCheckStatusNameChk;
        public readonly string strAcceptanceCheckStatusNameStp;
        public readonly string strAcceptanceCheckStatusNameEnd;
        public readonly string strAcceptanceCheckStatusNameExc;
        // RAPID解析結果
        public readonly int intRapidResultNon;
        public readonly int intRapidResultOk;
        public readonly int intRapidResultNg;
        public readonly int intRapidResultErr;
        // 端判定結果
        public readonly int intEdgeResultNon;
        public readonly int intEdgeResultOk;
        public readonly int intEdgeResultNg;
        public readonly int intEdgeResultErr;
        // マスキング判定結果
        public readonly int intMaskingResultNon;
        public readonly int intMaskingResultOk;
        public readonly int intMaskingResultNg;
        public readonly int intMaskingResultErr;
        // 過検知除外結果
        public readonly int intOverDetectionExceptResultNon;
        public readonly int intOverDetectionExceptResultOk;
        public readonly int intOverDetectionExceptResultNg;
        public readonly string strOverDetectionExceptResultNameNon;
        public readonly string strOverDetectionExceptResultNameOk;
        public readonly string strOverDetectionExceptResultNameNg;
        // 合否確認結果
        public readonly int intAcceptanceCheckResultNon;
        public readonly int intAcceptanceCheckResultOk;
        public readonly int intAcceptanceCheckResultNgDetect;
        public readonly int intAcceptanceCheckResultNgNonDetect;
        public readonly string strAcceptanceCheckResultNameNon;
        public readonly string strAcceptanceCheckResultNameOk;
        public readonly string strAcceptanceCheckResultNameNgDetect;
        public readonly string strAcceptanceCheckResultNameNgNonDetect;

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
                GetSystemSettingValue("NgImageCooperationDirectory", ref strNgImageCooperationDirectory);
                GetSystemSettingValue("CompletionNoticeCooperationDirectory", ref strCompletionNoticeCooperationDirectory);
                GetSystemSettingValue("FaultImageDirectory", ref strFaultImageDirectory);
                GetSystemSettingValue("MasterImageDirectory", ref strMasterImageDirectory);
                GetSystemSettingValue("NotDetectedImageCooperationDirectory", ref strNotDetectedImageCooperationDirectory);
                GetSystemSettingValue("CooperationBaseInstanceName", ref strCooperationBaseInstanceName);
                GetSystemSettingValue("WaitingTimeProcessed", ref intWaitingTimeProcessed);
                GetSystemSettingValue("ProductionManagementCooperationDirectory", ref strProductionManagementCooperationDirectory);
                GetSystemSettingValue("InspectionResltCsvDirectory", ref strInspectionResltCsvDirectory);
                GetSystemSettingValue("InspectionDirectionS", ref strInspectionDirectionS);
                GetSystemSettingValue("InspectionDirectionX", ref strInspectionDirectionX);
                GetSystemSettingValue("InspectionDirectionY", ref strInspectionDirectionY);
                GetSystemSettingValue("InspectionDirectionR", ref strInspectionDirectionR);
                GetSystemSettingValue("OverDetectionExceptStatusBef", ref intOverDetectionExceptStatusBef);
                GetSystemSettingValue("OverDetectionExceptStatusChk", ref intOverDetectionExceptStatusChk);
                GetSystemSettingValue("OverDetectionExceptStatusStp", ref intOverDetectionExceptStatusStp);
                GetSystemSettingValue("OverDetectionExceptStatusEnd", ref intOverDetectionExceptStatusEnd);
                GetSystemSettingValue("OverDetectionExceptStatusExc", ref intOverDetectionExceptStatusExc);
                GetSystemSettingValue("OverDetectionExceptStatusNameBef", ref strOverDetectionExceptStatusNameBef);
                GetSystemSettingValue("OverDetectionExceptStatusNameChk", ref strOverDetectionExceptStatusNameChk);
                GetSystemSettingValue("OverDetectionExceptStatusNameStp", ref strOverDetectionExceptStatusNameStp);
                GetSystemSettingValue("OverDetectionExceptStatusNameEnd", ref strOverDetectionExceptStatusNameEnd);
                GetSystemSettingValue("OverDetectionExceptStatusNameExc", ref strOverDetectionExceptStatusNameExc);
                GetSystemSettingValue("AcceptanceCheckStatusBef", ref intAcceptanceCheckStatusBef);
                GetSystemSettingValue("AcceptanceCheckStatusChk", ref intAcceptanceCheckStatusChk);
                GetSystemSettingValue("AcceptanceCheckStatusStp", ref intAcceptanceCheckStatusStp);
                GetSystemSettingValue("AcceptanceCheckStatusEnd", ref intAcceptanceCheckStatusEnd);
                GetSystemSettingValue("AcceptanceCheckStatusExc", ref intAcceptanceCheckStatusExc);
                GetSystemSettingValue("AcceptanceCheckStatusNameBef", ref strAcceptanceCheckStatusNameBef);
                GetSystemSettingValue("AcceptanceCheckStatusNameChk", ref strAcceptanceCheckStatusNameChk);
                GetSystemSettingValue("AcceptanceCheckStatusNameStp", ref strAcceptanceCheckStatusNameStp);
                GetSystemSettingValue("AcceptanceCheckStatusNameEnd", ref strAcceptanceCheckStatusNameEnd);
                GetSystemSettingValue("AcceptanceCheckStatusNameExc", ref strAcceptanceCheckStatusNameExc);
                GetSystemSettingValue("RapidResultNon", ref intRapidResultNon);
                GetSystemSettingValue("RapidResultOk", ref intRapidResultOk);
                GetSystemSettingValue("RapidResultNg", ref intRapidResultNg);
                GetSystemSettingValue("RapidResultErr", ref intRapidResultErr);
                GetSystemSettingValue("EdgeResultNon", ref intEdgeResultNon);
                GetSystemSettingValue("EdgeResultOk", ref intEdgeResultOk);
                GetSystemSettingValue("EdgeResultNg", ref intEdgeResultNg);
                GetSystemSettingValue("EdgeResultErr", ref intEdgeResultErr);
                GetSystemSettingValue("MaskingResultNon", ref intMaskingResultNon);
                GetSystemSettingValue("MaskingResultOk", ref intMaskingResultOk);
                GetSystemSettingValue("MaskingResultNg", ref intMaskingResultNg);
                GetSystemSettingValue("MaskingResultErr", ref intMaskingResultErr);
                GetSystemSettingValue("OverDetectionExceptResultNon", ref intOverDetectionExceptResultNon);
                GetSystemSettingValue("OverDetectionExceptResultOk", ref intOverDetectionExceptResultOk);
                GetSystemSettingValue("OverDetectionExceptResultNg", ref intOverDetectionExceptResultNg);
                GetSystemSettingValue("OverDetectionExceptResultNameNon", ref strOverDetectionExceptResultNameNon);
                GetSystemSettingValue("OverDetectionExceptResultNameOk", ref strOverDetectionExceptResultNameOk);
                GetSystemSettingValue("OverDetectionExceptResultNameNg", ref strOverDetectionExceptResultNameNg);
                GetSystemSettingValue("AcceptanceCheckResultNon", ref intAcceptanceCheckResultNon);
                GetSystemSettingValue("AcceptanceCheckResultOk", ref intAcceptanceCheckResultOk);
                GetSystemSettingValue("AcceptanceCheckResultNgDetect", ref intAcceptanceCheckResultNgDetect);
                GetSystemSettingValue("AcceptanceCheckResultNgNonDetect", ref intAcceptanceCheckResultNgNonDetect);
                GetSystemSettingValue("AcceptanceCheckResultNameNon", ref strAcceptanceCheckResultNameNon);
                GetSystemSettingValue("AcceptanceCheckResultNameOk", ref strAcceptanceCheckResultNameOk);
                GetSystemSettingValue("AcceptanceCheckResultNameNgDetect", ref strAcceptanceCheckResultNameNgDetect);
                GetSystemSettingValue("AcceptanceCheckResultNameNgNonDetect", ref strAcceptanceCheckResultNameNgNonDetect);

                if (lststrErrorMessage.Count > 0)
                {
                    foreach (string Message in lststrErrorMessage)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format( "システム設定取得時にエラーが発生しました。{0}{1}",Environment.NewLine, Message));
                    }

                    // メッセージ出力
                    Console.WriteLine("システム設定取得時に例外が発生しました。");

                    return;
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR,string.Format( "システム設定取得時にエラーが発生しました。{0}{1}" ,Environment.NewLine , ex.Message));
                // メッセージ出力
                Console.WriteLine("システム設定取得時に例外が発生しました。");
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
                lststrErrorMessage.Add(string.Format( "Key[{0}] AppConfigに存在しません。", strKey));
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
                lststrErrorMessage.Add(string.Format("Key[{0}]{1}" , strKey , ex.Message));
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
