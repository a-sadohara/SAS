using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using static ImageChecker.Common;

namespace ImageChecker.DTO
{
    class SystemSettingInfo
    {
        // 正常終了フラグ
        public readonly bool bolNormalEnd = false;

        // データテーブル
        private DataTable m_dtSystemSettingInfo = new DataTable();
        private DataTable m_dtMstDecisionReason = new DataTable();

        // エラーメッセージ格納用
        private List<String> lststrErrorMessage = new List<String>();

        private const string con_strInstanceName = "InstanceName";
        private const string con_strTemporaryDirectory = "TemporaryDirectory";
        private const string con_strNgImageCooperationDirectoryN1 = "NgImageCooperationDirectoryN1";
        private const string con_strNgImageCooperationDirectoryN2 = "NgImageCooperationDirectoryN2";
        private const string con_strNgImageCooperationDirectoryN3 = "NgImageCooperationDirectoryN3";
        private const string con_strNgImageCooperationDirectoryN4 = "NgImageCooperationDirectoryN4";
        private const string con_strCompletionNoticeCooperationDirectoryN1 = "CompletionNoticeCooperationDirectoryN1";
        private const string con_strCompletionNoticeCooperationDirectoryN2 = "CompletionNoticeCooperationDirectoryN2";
        private const string con_strCompletionNoticeCooperationDirectoryN3 = "CompletionNoticeCooperationDirectoryN3";
        private const string con_strCompletionNoticeCooperationDirectoryN4 = "CompletionNoticeCooperationDirectoryN4";
        private const string con_strFaultImageDirectory = "FaultImageDirectory";
        private const string con_strMasterImageDirectory = "MasterImageDirectory";
        private const string con_strNotDetectedImageCooperationDirectoryN1 = "NotDetectedImageCooperationDirectoryN1";
        private const string con_strNotDetectedImageCooperationDirectoryN2 = "NotDetectedImageCooperationDirectoryN2";
        private const string con_strNotDetectedImageCooperationDirectoryN3 = "NotDetectedImageCooperationDirectoryN3";
        private const string con_strNotDetectedImageCooperationDirectoryN4 = "NotDetectedImageCooperationDirectoryN4";
        private const string con_strCooperationBaseInstanceName = "CooperationBaseInstanceName";
        private const string con_strWaitingTimeProcessed = "WaitingTimeProcessed";
        private const string con_strProductionManagementCooperationDirectory = "ProductionManagementCooperationDirectory";
        private const string con_strInspectionResltCsvDirectory = "InspectionResltCsvDirectory";
        private const string con_strInspectionDirectionS = "InspectionDirectionS";
        private const string con_strInspectionDirectionX = "InspectionDirectionX";
        private const string con_strInspectionDirectionY = "InspectionDirectionY";
        private const string con_strInspectionDirectionR = "InspectionDirectionR";
        private const string con_strOverDetectionExceptStatusBef = "OverDetectionExceptStatusBef";
        private const string con_strOverDetectionExceptStatusChk = "OverDetectionExceptStatusChk";
        private const string con_strOverDetectionExceptStatusStp = "OverDetectionExceptStatusStp";
        private const string con_strOverDetectionExceptStatusEnd = "OverDetectionExceptStatusEnd";
        private const string con_strOverDetectionExceptStatusExc = "OverDetectionExceptStatusExc";
        private const string con_strOverDetectionExceptStatusNameBef = "OverDetectionExceptStatusNameBef";
        private const string con_strOverDetectionExceptStatusNameChk = "OverDetectionExceptStatusNameChk";
        private const string con_strOverDetectionExceptStatusNameStp = "OverDetectionExceptStatusNameStp";
        private const string con_strOverDetectionExceptStatusNameEnd = "OverDetectionExceptStatusNameEnd";
        private const string con_strOverDetectionExceptStatusNameExc = "OverDetectionExceptStatusNameExc";
        private const string con_strAcceptanceCheckStatusBef = "AcceptanceCheckStatusBef";
        private const string con_strAcceptanceCheckStatusChk = "AcceptanceCheckStatusChk";
        private const string con_strAcceptanceCheckStatusStp = "AcceptanceCheckStatusStp";
        private const string con_strAcceptanceCheckStatusEnd = "AcceptanceCheckStatusEnd";
        private const string con_strAcceptanceCheckStatusExc = "AcceptanceCheckStatusExc";
        private const string con_strAcceptanceCheckStatusNameBef = "AcceptanceCheckStatusNameBef";
        private const string con_strAcceptanceCheckStatusNameChk = "AcceptanceCheckStatusNameChk";
        private const string con_strAcceptanceCheckStatusNameStp = "AcceptanceCheckStatusNameStp";
        private const string con_strAcceptanceCheckStatusNameEnd = "AcceptanceCheckStatusNameEnd";
        private const string con_strAcceptanceCheckStatusNameExc = "AcceptanceCheckStatusNameExc";
        private const string con_strRapidResultNon = "RapidResultNon";
        private const string con_strRapidResultOk = "RapidResultOk";
        private const string con_strRapidResultNg = "RapidResultNg";
        private const string con_strRapidResultErr = "RapidResultErr";
        private const string con_strRapidResultDis = "RapidResultDis";
        private const string con_strEdgeResultNon = "EdgeResultNon";
        private const string con_strEdgeResultOk = "EdgeResultOk";
        private const string con_strEdgeResultNg = "EdgeResultNg";
        private const string con_strEdgeResultErr = "EdgeResultErr";
        private const string con_strEdgeResultDis = "EdgeResultDis";
        private const string con_strMaskingResultNon = "MaskingResultNon";
        private const string con_strMaskingResultOk = "MaskingResultOk";
        private const string con_strMaskingResultNg = "MaskingResultNg";
        private const string con_strMaskingResultErr = "MaskingResultErr";
        private const string con_strMaskingResultDis = "MaskingResultDis";
        private const string con_strOverDetectionExceptResultNon = "OverDetectionExceptResultNon";
        private const string con_strOverDetectionExceptResultOk = "OverDetectionExceptResultOk";
        private const string con_strOverDetectionExceptResultNg = "OverDetectionExceptResultNg";
        private const string con_strOverDetectionExceptResultNgNonDetect = "OverDetectionExceptResultNgNonDetect";
        private const string con_strOverDetectionExceptResultNameNon = "OverDetectionExceptResultNameNon";
        private const string con_strOverDetectionExceptResultNameOk = "OverDetectionExceptResultNameOk";
        private const string con_strOverDetectionExceptResultNameNg = "OverDetectionExceptResultNameNg";
        private const string con_strOverDetectionExceptResultNameNgNonDetect = "OverDetectionExceptResultNameNgNonDetect";
        private const string con_strAcceptanceCheckResultNon = "AcceptanceCheckResultNon";
        private const string con_strAcceptanceCheckResultOk = "AcceptanceCheckResultOk";
        private const string con_strAcceptanceCheckResultNgDetect = "AcceptanceCheckResultNgDetect";
        private const string con_strAcceptanceCheckResultNgNonDetect = "AcceptanceCheckResultNgNonDetect";
        private const string con_strAcceptanceCheckResultNameNon = "AcceptanceCheckResultNameNon";
        private const string con_strAcceptanceCheckResultNameOk = "AcceptanceCheckResultNameOk";
        private const string con_strAcceptanceCheckResultNameNgDetect = "AcceptanceCheckResultNameNgDetect";
        private const string con_strAcceptanceCheckResultNameNgNonDetect = "AcceptanceCheckResultNameNgNonDetect";
        private const string con_strSuperUser = "SuperUser";
        private const string con_strRetryTimes = "RetryTimes";
        private const string con_strRetryWaitSeconds = "RetryWaitSeconds";
        private const string con_strNgImageAcquisitionPeriod = "NgImageAcquisitionPeriod";
        private const string con_strMinDecompressionWaitingTime = "MinDecompressionWaitingTime";
        private const string con_strMaxDecompressionWaitingTime = "MaxDecompressionWaitingTime";
        private const string con_strProcessingPriority = "ProcessingPriority";
        private const string con_strMainNGReason1 = "MainNGReason1";
        private const string con_strMainNGReason2 = "MainNGReason2";
        private const string con_strMainNGReason3 = "MainNGReason3";
        private const string con_strMainNGReason4 = "MainNGReason4";

        //==============
        // App.Config
        //==============
        // インスタンス名
        public readonly string strInstanceName;

        //==============================
        //システム情報設定テーブル
        //==============================
        // ディレクトリ
        public readonly string strNgImageCooperationDirectoryN1;                    // NG画像連携N1
        public readonly string strNgImageCooperationDirectoryN2;                    // NG画像連携N2
        public readonly string strNgImageCooperationDirectoryN3;                    // NG画像連携N3
        public readonly string strNgImageCooperationDirectoryN4;                    // NG画像連携N4
        public readonly string strCompletionNoticeCooperationDirectoryN1;           // 完了通知連携N1
        public readonly string strCompletionNoticeCooperationDirectoryN2;           // 完了通知連携N2
        public readonly string strCompletionNoticeCooperationDirectoryN3;           // 完了通知連携N3
        public readonly string strCompletionNoticeCooperationDirectoryN4;           // 完了通知連携N4
        public readonly string strFaultImageDirectory;                              // 欠点画像格納
        public readonly string strMasterImageDirectory;                             // マスタ画像格納
        public readonly string strNotDetectedImageCooperationDirectoryN1;           // 未検知画像連携N1
        public readonly string strNotDetectedImageCooperationDirectoryN2;           // 未検知画像連携N2
        public readonly string strNotDetectedImageCooperationDirectoryN3;           // 未検知画像連携N3
        public readonly string strNotDetectedImageCooperationDirectoryN4;           // 未検知画像連携N4
        public readonly string strProductionManagementCooperationDirectory;         // 生産管理システム連携
        public readonly string strInspectionResltCsvDirectory;                      // 検査結果CSV保存先
        public readonly string strTemporaryDirectory;                               // 一時

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
        public readonly int intRapidResultDis;
        // 端判定結果
        public readonly int intEdgeResultNon;
        public readonly int intEdgeResultOk;
        public readonly int intEdgeResultNg;
        public readonly int intEdgeResultErr;
        public readonly int intEdgeResultDis;
        // マスキング判定結果
        public readonly int intMaskingResultNon;
        public readonly int intMaskingResultOk;
        public readonly int intMaskingResultNg;
        public readonly int intMaskingResultErr;
        public readonly int intMaskingResultDis;
        // 過検知除外結果
        public readonly int intOverDetectionExceptResultNon;
        public readonly int intOverDetectionExceptResultOk;
        public readonly int intOverDetectionExceptResultNg;
        public readonly int intOverDetectionExceptResultNgNonDetect;
        public readonly string strOverDetectionExceptResultNameNon;
        public readonly string strOverDetectionExceptResultNameOk;
        public readonly string strOverDetectionExceptResultNameNg;
        public readonly string strOverDetectionExceptResultNameNgNonDetect;
        // 合否確認結果
        public readonly int intAcceptanceCheckResultNon;
        public readonly int intAcceptanceCheckResultOk;
        public readonly int intAcceptanceCheckResultNgDetect;
        public readonly int intAcceptanceCheckResultNgNonDetect;
        public readonly string strAcceptanceCheckResultNameNon;
        public readonly string strAcceptanceCheckResultNameOk;
        public readonly string strAcceptanceCheckResultNameNgDetect;
        public readonly string strAcceptanceCheckResultNameNgNonDetect;
        // スーパーユーザ
        public readonly string strSuperUser;
        // リトライ試行回数
        public readonly int intRetryTimes;
        // リトライ待機時間（秒）
        public readonly int intRetryWaitSeconds;
        // NG画像取得期間（日数）
        public readonly int intNgImageAcquisitionPeriod;
        // 解凍待ち時間下限（秒）
        public readonly int intMinDecompressionWaitingTime;
        // 解凍待ち時間上限（秒）
        public readonly int intMaxDecompressionWaitingTime;
        // プロセス優先度
        public readonly int intProcessingPriority;
        public readonly string strProcessingPriority;
        // NG理由(主要)
        public readonly int intMainNGReason1;
        public readonly int intMainNGReason2;
        public readonly int intMainNGReason3;
        public readonly int intMainNGReason4;
        public readonly string strMainNGReason1;
        public readonly string strMainNGReason2;
        public readonly string strMainNGReason3;
        public readonly string strMainNGReason4;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SystemSettingInfo()
        {
            try
            {
                // システム設定情報から設定値を取得
                GetSystemSettingValue();
                GetSystemSettingValue(con_strNgImageCooperationDirectoryN1, ref strNgImageCooperationDirectoryN1);
                GetSystemSettingValue(con_strNgImageCooperationDirectoryN2, ref strNgImageCooperationDirectoryN2);
                GetSystemSettingValue(con_strNgImageCooperationDirectoryN3, ref strNgImageCooperationDirectoryN3);
                GetSystemSettingValue(con_strNgImageCooperationDirectoryN4, ref strNgImageCooperationDirectoryN4);
                GetSystemSettingValue(con_strCompletionNoticeCooperationDirectoryN1, ref strCompletionNoticeCooperationDirectoryN1);
                GetSystemSettingValue(con_strCompletionNoticeCooperationDirectoryN2, ref strCompletionNoticeCooperationDirectoryN2);
                GetSystemSettingValue(con_strCompletionNoticeCooperationDirectoryN3, ref strCompletionNoticeCooperationDirectoryN3);
                GetSystemSettingValue(con_strCompletionNoticeCooperationDirectoryN4, ref strCompletionNoticeCooperationDirectoryN4);
                GetSystemSettingValue(con_strFaultImageDirectory, ref strFaultImageDirectory);
                GetSystemSettingValue(con_strMasterImageDirectory, ref strMasterImageDirectory);
                GetSystemSettingValue(con_strNotDetectedImageCooperationDirectoryN1, ref strNotDetectedImageCooperationDirectoryN1);
                GetSystemSettingValue(con_strNotDetectedImageCooperationDirectoryN2, ref strNotDetectedImageCooperationDirectoryN2);
                GetSystemSettingValue(con_strNotDetectedImageCooperationDirectoryN3, ref strNotDetectedImageCooperationDirectoryN3);
                GetSystemSettingValue(con_strNotDetectedImageCooperationDirectoryN4, ref strNotDetectedImageCooperationDirectoryN4);
                GetSystemSettingValue(con_strCooperationBaseInstanceName, ref strCooperationBaseInstanceName);
                GetSystemSettingValue(con_strWaitingTimeProcessed, ref intWaitingTimeProcessed);
                GetSystemSettingValue(con_strProductionManagementCooperationDirectory, ref strProductionManagementCooperationDirectory);
                GetSystemSettingValue(con_strInspectionResltCsvDirectory, ref strInspectionResltCsvDirectory);
                GetSystemSettingValue(con_strInspectionDirectionS, ref strInspectionDirectionS);
                GetSystemSettingValue(con_strInspectionDirectionX, ref strInspectionDirectionX);
                GetSystemSettingValue(con_strInspectionDirectionY, ref strInspectionDirectionY);
                GetSystemSettingValue(con_strInspectionDirectionR, ref strInspectionDirectionR);
                GetSystemSettingValue(con_strOverDetectionExceptStatusBef, ref intOverDetectionExceptStatusBef);
                GetSystemSettingValue(con_strOverDetectionExceptStatusChk, ref intOverDetectionExceptStatusChk);
                GetSystemSettingValue(con_strOverDetectionExceptStatusStp, ref intOverDetectionExceptStatusStp);
                GetSystemSettingValue(con_strOverDetectionExceptStatusEnd, ref intOverDetectionExceptStatusEnd);
                GetSystemSettingValue(con_strOverDetectionExceptStatusExc, ref intOverDetectionExceptStatusExc);
                GetSystemSettingValue(con_strOverDetectionExceptStatusNameBef, ref strOverDetectionExceptStatusNameBef);
                GetSystemSettingValue(con_strOverDetectionExceptStatusNameChk, ref strOverDetectionExceptStatusNameChk);
                GetSystemSettingValue(con_strOverDetectionExceptStatusNameStp, ref strOverDetectionExceptStatusNameStp);
                GetSystemSettingValue(con_strOverDetectionExceptStatusNameEnd, ref strOverDetectionExceptStatusNameEnd);
                GetSystemSettingValue(con_strOverDetectionExceptStatusNameExc, ref strOverDetectionExceptStatusNameExc);
                GetSystemSettingValue(con_strAcceptanceCheckStatusBef, ref intAcceptanceCheckStatusBef);
                GetSystemSettingValue(con_strAcceptanceCheckStatusChk, ref intAcceptanceCheckStatusChk);
                GetSystemSettingValue(con_strAcceptanceCheckStatusStp, ref intAcceptanceCheckStatusStp);
                GetSystemSettingValue(con_strAcceptanceCheckStatusEnd, ref intAcceptanceCheckStatusEnd);
                GetSystemSettingValue(con_strAcceptanceCheckStatusExc, ref intAcceptanceCheckStatusExc);
                GetSystemSettingValue(con_strAcceptanceCheckStatusNameBef, ref strAcceptanceCheckStatusNameBef);
                GetSystemSettingValue(con_strAcceptanceCheckStatusNameChk, ref strAcceptanceCheckStatusNameChk);
                GetSystemSettingValue(con_strAcceptanceCheckStatusNameStp, ref strAcceptanceCheckStatusNameStp);
                GetSystemSettingValue(con_strAcceptanceCheckStatusNameEnd, ref strAcceptanceCheckStatusNameEnd);
                GetSystemSettingValue(con_strAcceptanceCheckStatusNameExc, ref strAcceptanceCheckStatusNameExc);
                GetSystemSettingValue(con_strRapidResultNon, ref intRapidResultNon);
                GetSystemSettingValue(con_strRapidResultOk, ref intRapidResultOk);
                GetSystemSettingValue(con_strRapidResultNg, ref intRapidResultNg);
                GetSystemSettingValue(con_strRapidResultErr, ref intRapidResultErr);
                GetSystemSettingValue(con_strRapidResultDis, ref intRapidResultDis);
                GetSystemSettingValue(con_strEdgeResultNon, ref intEdgeResultNon);
                GetSystemSettingValue(con_strEdgeResultOk, ref intEdgeResultOk);
                GetSystemSettingValue(con_strEdgeResultNg, ref intEdgeResultNg);
                GetSystemSettingValue(con_strEdgeResultErr, ref intEdgeResultErr);
                GetSystemSettingValue(con_strEdgeResultDis, ref intEdgeResultDis);
                GetSystemSettingValue(con_strMaskingResultNon, ref intMaskingResultNon);
                GetSystemSettingValue(con_strMaskingResultOk, ref intMaskingResultOk);
                GetSystemSettingValue(con_strMaskingResultNg, ref intMaskingResultNg);
                GetSystemSettingValue(con_strMaskingResultErr, ref intMaskingResultErr);
                GetSystemSettingValue(con_strMaskingResultDis, ref intMaskingResultDis);
                GetSystemSettingValue(con_strOverDetectionExceptResultNon, ref intOverDetectionExceptResultNon);
                GetSystemSettingValue(con_strOverDetectionExceptResultOk, ref intOverDetectionExceptResultOk);
                GetSystemSettingValue(con_strOverDetectionExceptResultNg, ref intOverDetectionExceptResultNg);
                GetSystemSettingValue(con_strOverDetectionExceptResultNgNonDetect, ref intOverDetectionExceptResultNgNonDetect);
                GetSystemSettingValue(con_strOverDetectionExceptResultNameNon, ref strOverDetectionExceptResultNameNon);
                GetSystemSettingValue(con_strOverDetectionExceptResultNameOk, ref strOverDetectionExceptResultNameOk);
                GetSystemSettingValue(con_strOverDetectionExceptResultNameNg, ref strOverDetectionExceptResultNameNg);
                GetSystemSettingValue(con_strOverDetectionExceptResultNameNgNonDetect, ref strOverDetectionExceptResultNameNgNonDetect);
                GetSystemSettingValue(con_strAcceptanceCheckResultNon, ref intAcceptanceCheckResultNon);
                GetSystemSettingValue(con_strAcceptanceCheckResultOk, ref intAcceptanceCheckResultOk);
                GetSystemSettingValue(con_strAcceptanceCheckResultNgDetect, ref intAcceptanceCheckResultNgDetect);
                GetSystemSettingValue(con_strAcceptanceCheckResultNgNonDetect, ref intAcceptanceCheckResultNgNonDetect);
                GetSystemSettingValue(con_strAcceptanceCheckResultNameNon, ref strAcceptanceCheckResultNameNon);
                GetSystemSettingValue(con_strAcceptanceCheckResultNameOk, ref strAcceptanceCheckResultNameOk);
                GetSystemSettingValue(con_strAcceptanceCheckResultNameNgDetect, ref strAcceptanceCheckResultNameNgDetect);
                GetSystemSettingValue(con_strAcceptanceCheckResultNameNgNonDetect, ref strAcceptanceCheckResultNameNgNonDetect);
                GetSystemSettingValue(con_strSuperUser, ref strSuperUser);
                GetSystemSettingValue(con_strRetryTimes, ref intRetryTimes);
                GetSystemSettingValue(con_strRetryWaitSeconds, ref intRetryWaitSeconds);
                GetSystemSettingValue(con_strNgImageAcquisitionPeriod, ref intNgImageAcquisitionPeriod);
                GetSystemSettingValue(con_strMinDecompressionWaitingTime, ref intMinDecompressionWaitingTime);
                GetSystemSettingValue(con_strMaxDecompressionWaitingTime, ref intMaxDecompressionWaitingTime);
                GetSystemSettingValue(con_strProcessingPriority, ref intProcessingPriority);
                GetSystemSettingValue(con_strMainNGReason1, ref intMainNGReason1);
                GetSystemSettingValue(con_strMainNGReason2, ref intMainNGReason2);
                GetSystemSettingValue(con_strMainNGReason3, ref intMainNGReason3);
                GetSystemSettingValue(con_strMainNGReason4, ref intMainNGReason4);

                // App.Configから設定値を取得・上書き
                GetAppConfigValue(con_strInstanceName, ref strInstanceName);
                GetAppConfigValue(con_strTemporaryDirectory, ref strTemporaryDirectory);
                GetAppConfigValue(con_strNgImageCooperationDirectoryN1, ref strNgImageCooperationDirectoryN1);
                GetAppConfigValue(con_strNgImageCooperationDirectoryN2, ref strNgImageCooperationDirectoryN2);
                GetAppConfigValue(con_strNgImageCooperationDirectoryN3, ref strNgImageCooperationDirectoryN3);
                GetAppConfigValue(con_strNgImageCooperationDirectoryN4, ref strNgImageCooperationDirectoryN4);
                GetAppConfigValue(con_strCompletionNoticeCooperationDirectoryN1, ref strCompletionNoticeCooperationDirectoryN1);
                GetAppConfigValue(con_strCompletionNoticeCooperationDirectoryN2, ref strCompletionNoticeCooperationDirectoryN2);
                GetAppConfigValue(con_strCompletionNoticeCooperationDirectoryN3, ref strCompletionNoticeCooperationDirectoryN3);
                GetAppConfigValue(con_strCompletionNoticeCooperationDirectoryN4, ref strCompletionNoticeCooperationDirectoryN4);
                GetAppConfigValue(con_strFaultImageDirectory, ref strFaultImageDirectory);
                GetAppConfigValue(con_strMasterImageDirectory, ref strMasterImageDirectory);
                GetAppConfigValue(con_strNotDetectedImageCooperationDirectoryN1, ref strNotDetectedImageCooperationDirectoryN1);
                GetAppConfigValue(con_strNotDetectedImageCooperationDirectoryN2, ref strNotDetectedImageCooperationDirectoryN2);
                GetAppConfigValue(con_strNotDetectedImageCooperationDirectoryN3, ref strNotDetectedImageCooperationDirectoryN3);
                GetAppConfigValue(con_strNotDetectedImageCooperationDirectoryN4, ref strNotDetectedImageCooperationDirectoryN4);
                GetAppConfigValue(con_strCooperationBaseInstanceName, ref strCooperationBaseInstanceName);
                GetAppConfigValue(con_strWaitingTimeProcessed, ref intWaitingTimeProcessed);
                GetAppConfigValue(con_strProductionManagementCooperationDirectory, ref strProductionManagementCooperationDirectory);
                GetAppConfigValue(con_strInspectionResltCsvDirectory, ref strInspectionResltCsvDirectory);
                GetAppConfigValue(con_strInspectionDirectionS, ref strInspectionDirectionS);
                GetAppConfigValue(con_strInspectionDirectionX, ref strInspectionDirectionX);
                GetAppConfigValue(con_strInspectionDirectionY, ref strInspectionDirectionY);
                GetAppConfigValue(con_strInspectionDirectionR, ref strInspectionDirectionR);
                GetAppConfigValue(con_strOverDetectionExceptStatusBef, ref intOverDetectionExceptStatusBef);
                GetAppConfigValue(con_strOverDetectionExceptStatusChk, ref intOverDetectionExceptStatusChk);
                GetAppConfigValue(con_strOverDetectionExceptStatusStp, ref intOverDetectionExceptStatusStp);
                GetAppConfigValue(con_strOverDetectionExceptStatusEnd, ref intOverDetectionExceptStatusEnd);
                GetAppConfigValue(con_strOverDetectionExceptStatusExc, ref intOverDetectionExceptStatusExc);
                GetAppConfigValue(con_strOverDetectionExceptStatusNameBef, ref strOverDetectionExceptStatusNameBef);
                GetAppConfigValue(con_strOverDetectionExceptStatusNameChk, ref strOverDetectionExceptStatusNameChk);
                GetAppConfigValue(con_strOverDetectionExceptStatusNameStp, ref strOverDetectionExceptStatusNameStp);
                GetAppConfigValue(con_strOverDetectionExceptStatusNameEnd, ref strOverDetectionExceptStatusNameEnd);
                GetAppConfigValue(con_strOverDetectionExceptStatusNameExc, ref strOverDetectionExceptStatusNameExc);
                GetAppConfigValue(con_strAcceptanceCheckStatusBef, ref intAcceptanceCheckStatusBef);
                GetAppConfigValue(con_strAcceptanceCheckStatusChk, ref intAcceptanceCheckStatusChk);
                GetAppConfigValue(con_strAcceptanceCheckStatusStp, ref intAcceptanceCheckStatusStp);
                GetAppConfigValue(con_strAcceptanceCheckStatusEnd, ref intAcceptanceCheckStatusEnd);
                GetAppConfigValue(con_strAcceptanceCheckStatusExc, ref intAcceptanceCheckStatusExc);
                GetAppConfigValue(con_strAcceptanceCheckStatusNameBef, ref strAcceptanceCheckStatusNameBef);
                GetAppConfigValue(con_strAcceptanceCheckStatusNameChk, ref strAcceptanceCheckStatusNameChk);
                GetAppConfigValue(con_strAcceptanceCheckStatusNameStp, ref strAcceptanceCheckStatusNameStp);
                GetAppConfigValue(con_strAcceptanceCheckStatusNameEnd, ref strAcceptanceCheckStatusNameEnd);
                GetAppConfigValue(con_strAcceptanceCheckStatusNameExc, ref strAcceptanceCheckStatusNameExc);
                GetAppConfigValue(con_strRapidResultNon, ref intRapidResultNon);
                GetAppConfigValue(con_strRapidResultOk, ref intRapidResultOk);
                GetAppConfigValue(con_strRapidResultNg, ref intRapidResultNg);
                GetAppConfigValue(con_strRapidResultErr, ref intRapidResultErr);
                GetAppConfigValue(con_strRapidResultDis, ref intRapidResultDis);
                GetAppConfigValue(con_strEdgeResultNon, ref intEdgeResultNon);
                GetAppConfigValue(con_strEdgeResultOk, ref intEdgeResultOk);
                GetAppConfigValue(con_strEdgeResultNg, ref intEdgeResultNg);
                GetAppConfigValue(con_strEdgeResultErr, ref intEdgeResultErr);
                GetAppConfigValue(con_strEdgeResultDis, ref intEdgeResultDis);
                GetAppConfigValue(con_strMaskingResultNon, ref intMaskingResultNon);
                GetAppConfigValue(con_strMaskingResultOk, ref intMaskingResultOk);
                GetAppConfigValue(con_strMaskingResultNg, ref intMaskingResultNg);
                GetAppConfigValue(con_strMaskingResultErr, ref intMaskingResultErr);
                GetAppConfigValue(con_strMaskingResultDis, ref intMaskingResultDis);
                GetAppConfigValue(con_strOverDetectionExceptResultNon, ref intOverDetectionExceptResultNon);
                GetAppConfigValue(con_strOverDetectionExceptResultOk, ref intOverDetectionExceptResultOk);
                GetAppConfigValue(con_strOverDetectionExceptResultNg, ref intOverDetectionExceptResultNg);
                GetAppConfigValue(con_strOverDetectionExceptResultNgNonDetect, ref intOverDetectionExceptResultNgNonDetect);
                GetAppConfigValue(con_strOverDetectionExceptResultNameNon, ref strOverDetectionExceptResultNameNon);
                GetAppConfigValue(con_strOverDetectionExceptResultNameOk, ref strOverDetectionExceptResultNameOk);
                GetAppConfigValue(con_strOverDetectionExceptResultNameNg, ref strOverDetectionExceptResultNameNg);
                GetAppConfigValue(con_strOverDetectionExceptResultNameNgNonDetect, ref strOverDetectionExceptResultNameNgNonDetect);
                GetAppConfigValue(con_strAcceptanceCheckResultNon, ref intAcceptanceCheckResultNon);
                GetAppConfigValue(con_strAcceptanceCheckResultOk, ref intAcceptanceCheckResultOk);
                GetAppConfigValue(con_strAcceptanceCheckResultNgDetect, ref intAcceptanceCheckResultNgDetect);
                GetAppConfigValue(con_strAcceptanceCheckResultNgNonDetect, ref intAcceptanceCheckResultNgNonDetect);
                GetAppConfigValue(con_strAcceptanceCheckResultNameNon, ref strAcceptanceCheckResultNameNon);
                GetAppConfigValue(con_strAcceptanceCheckResultNameOk, ref strAcceptanceCheckResultNameOk);
                GetAppConfigValue(con_strAcceptanceCheckResultNameNgDetect, ref strAcceptanceCheckResultNameNgDetect);
                GetAppConfigValue(con_strAcceptanceCheckResultNameNgNonDetect, ref strAcceptanceCheckResultNameNgNonDetect);
                GetAppConfigValue(con_strSuperUser, ref strSuperUser);
                GetAppConfigValue(con_strRetryTimes, ref intRetryTimes);
                GetAppConfigValue(con_strRetryWaitSeconds, ref intRetryWaitSeconds);
                GetAppConfigValue(con_strNgImageAcquisitionPeriod, ref intNgImageAcquisitionPeriod);
                GetAppConfigValue(con_strMinDecompressionWaitingTime, ref intMinDecompressionWaitingTime);
                GetAppConfigValue(con_strMaxDecompressionWaitingTime, ref intMaxDecompressionWaitingTime);
                GetAppConfigValue(con_strProcessingPriority, ref intProcessingPriority);
                GetAppConfigValue(con_strMainNGReason1, ref intMainNGReason1);
                GetAppConfigValue(con_strMainNGReason2, ref intMainNGReason2);
                GetAppConfigValue(con_strMainNGReason3, ref intMainNGReason3);
                GetAppConfigValue(con_strMainNGReason4, ref intMainNGReason4);

                // 判定理由マスタから判定理由を取得
                GetNgReasonValue();
                GetNgReasonValue(intMainNGReason1, ref strMainNGReason1);
                GetNgReasonValue(intMainNGReason2, ref strMainNGReason2);
                GetNgReasonValue(intMainNGReason3, ref strMainNGReason3);
                GetNgReasonValue(intMainNGReason4, ref strMainNGReason4);

                // リトライ回数
                if (intRetryTimes <= 0)
                {
                    intRetryTimes = 1;
                }

                // リトライ待機時間（秒）
                intRetryWaitSeconds *= 1000;

                if (intRetryWaitSeconds <= 0)
                {
                    intRetryWaitSeconds = 1000;
                }

                // NG画像取得期間（日数）
                if (intNgImageAcquisitionPeriod > 0)
                {
                    intNgImageAcquisitionPeriod *= -1;
                }

                // プロセス優先度
                switch (intProcessingPriority)
                {
                    case 1:
                        strProcessingPriority = g_strPriorityRealtime;
                        break;
                    case 2:
                        strProcessingPriority = g_strPriorityHighPriority;
                        break;
                    case 3:
                        strProcessingPriority = g_strPriorityAboveNormal;
                        break;
                    case 4:
                        strProcessingPriority = g_strPriorityNormal;
                        break;
                    default:
                        strProcessingPriority = string.Empty;
                        break;
                }

                m_dtSystemSettingInfo.Dispose();
                m_dtMstDecisionReason.Dispose();

                if (lststrErrorMessage.Count > 0)
                {
                    foreach (string Message in lststrErrorMessage)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("システム設定取得時にエラーが発生しました。{0}{1}", Environment.NewLine, Message));
                    }

                    // メッセージ出力
                    MessageBox.Show("システム設定取得時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }

                bolNormalEnd = true;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("システム設定取得時にエラーが発生しました。{0}{1}", Environment.NewLine, ex.Message));
                // メッセージ出力
                MessageBox.Show("システム設定取得時に例外が発生しました。", g_CON_MESSAGE_TITLE_ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            string strAppConfig = ConfigurationManager.AppSettings[strKey];

            if (!string.IsNullOrWhiteSpace(strAppConfig))
            {
                strValue = strAppConfig;
            }
            else if (strValue == null)
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
            string strValue = intValue.ToString();

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

        /// <summary>
        /// 判定理由マスタ設定値取得
        /// </summary>
        private void GetNgReasonValue()
        {
            string strSQL = string.Empty;

            try
            {
                // SQL抽出から情報を取得
                m_dtMstDecisionReason = new DataTable();
                strSQL = @"SELECT * FROM mst_decision_reason";

                g_clsConnectionNpgsql.SelectSQL(ref m_dtMstDecisionReason, strSQL);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// NG理由取得
        /// </summary>
        /// <param name="intReasonCode">理由コード</param>
        /// <param name="strDecisionReason">判定理由</param>
        public void GetNgReasonValue(int intReasonCode, ref string strDecisionReason)
        {
            DataRow[] dr = m_dtMstDecisionReason.Select("reason_code = '" + intReasonCode + "'");
            if (dr.Length > 0)
            {
                strDecisionReason = dr[0]["decision_reason"].ToString();
            }
            else
            {
                lststrErrorMessage.Add(string.Format("reason_code[{0}] 判定理由マスタテーブルに存在しません。", intReasonCode));
            }
        }
    }
}