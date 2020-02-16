using log4net;
using ProductMstMaintenance.DTO;
using System;
using System.Configuration;
using System.Text;
using System.Windows.Forms;

namespace ProductMstMaintenance
{
    static class Common
    {
        // DB接続文字列
        public static string g_strConnectionString = "";
        private const string g_CON_CONNECTION_STRING = "Server={0};Port={1};User ID={2};Database={3};Password={4};Enlist=true";
        public static string g_strDBName = "";
        public static string g_strDBUser = "";
        public static string g_strDBUserPassword = "";
        public static string g_strDBServerName = "";
        public static string g_strDBPort = "";

        // コネクションクラス
        public static ConnectionNpgsql g_clsConnectionNpgsql;

        // システム設定情報クラス
        public static SystemSettingInfo g_clsSystemSettingInfo;

        // メッセージ情報クラス
        public static MessageInfo g_clsMessageInfo;

        // システム設定情報取得時のエラーメッセージ格納用
        private static StringBuilder m_sbErrMessage = new StringBuilder();

        // イベントログ出力関連
        public static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const int g_CON_LEVEL_FATAL = 1;
        public const int g_CON_LEVEL_ERROR = 2;
        public const int g_CON_LEVEL_WARN = 3;
        public const int g_CON_LEVEL_INFO = 4;
        public const int g_CON_LEVEL_DEBUG = 5;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Mutex名を決める
            string mutexName = "WokerMstManagement";
            // Mutexオブジェクトを作成する
            System.Threading.Mutex mutex = new System.Threading.Mutex(false, mutexName);

            bool hasHandle = false;
            try
            {
                try
                {
                    // ミューテックスの所有権を要求する
                    hasHandle = mutex.WaitOne(0, false);
                }
                // .NET Framework 2.0以降の場合
                catch (System.Threading.AbandonedMutexException)
                {
                    // 別のアプリケーションがミューテックスを解放しないで終了した時
                    hasHandle = true;
                }
                // ミューテックスを得られたか調べる
                if (hasHandle == false)
                {
                    //得られなかった場合は、すでに起動していると判断して終了
                    MessageBox.Show("多重起動はできません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // 接続文字列をApp.configファイルから取得
                    GetAppConfigValue("DBName", ref g_strDBName);
                    GetAppConfigValue("DBUser", ref g_strDBUser);
                    GetAppConfigValue("DBUserPassword", ref g_strDBUserPassword);
                    GetAppConfigValue("DBServerName", ref g_strDBServerName);
                    GetAppConfigValue("DBPort", ref g_strDBPort);

                    if (m_sbErrMessage.Length > 0)
                    {
                        // ログ出力
                        WriteEventLog(g_CON_LEVEL_ERROR, "接続文字列取得時にエラーが発生しました。\r\n" + m_sbErrMessage.ToString());
                        // メッセージ出力
                        System.Windows.Forms.MessageBox.Show("接続文字列取得時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }

                    g_strConnectionString = string.Format(g_CON_CONNECTION_STRING, g_strDBServerName,
                                                                                   g_strDBPort,
                                                                                   g_strDBUser,
                                                                                   g_strDBName,
                                                                                   g_strDBUserPassword);
                    // 接続確認
                    g_clsConnectionNpgsql = new ConnectionNpgsql(g_strConnectionString);
                    g_clsConnectionNpgsql.DbOpen();
                    g_clsConnectionNpgsql.DbClose();

                    // システム設定情報取得
                    g_clsSystemSettingInfo = new SystemSettingInfo();
                    if (g_clsSystemSettingInfo.bolNormalEnd == false)
                        return;

                    // メッセージ情報取得
                    g_clsMessageInfo = new MessageInfo();
                    if (g_clsMessageInfo.bolNormalEnd == false)
                        return;
                }
                catch (Exception ex)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "初期起動時にエラーが発生しました。" + "\r\n" + ex.Message);
                    // メッセージ出力
                    System.Windows.Forms.MessageBox.Show("初期起動時に例外が発生しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    if (g_clsConnectionNpgsql != null)
                    {
                        g_clsConnectionNpgsql.DbClose();
                    }
                }

                // フォーム画面を起動
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ProductMstMaintenance());
            }
            finally
            {
                if (hasHandle)
                {
                    // ミューテックスを解放する
                    mutex.ReleaseMutex();
                }
                mutex.Close();
            }
        }

        /// <summary>
        /// NULLを""に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static string NulltoString(object objNValue)
        {
            if (objNValue == null)
            {
                return "";
            }
            else if (objNValue.ToString() == "")
            {
                return "";
            }
            else
            {
                return objNValue.ToString();
            }
        }

        /// <summary>
        /// NULLを0に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static int NulltoInt(object objNValue)
        {
            if (objNValue == null)
            {
                return 0;
            }
            else if (objNValue.ToString() == "")
            {
                return 0;
            }
            else
            {
                return int.Parse(objNValue.ToString());
            }
        }

        /// <summary>
        /// NULLを0に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static double NulltoDbl(object objNValue)
        {
            if (objNValue == null)
            {
                return 0;
            }
            else if (objNValue.ToString() == "")
            {
                return 0;
            }
            else
            {
                return double.Parse(objNValue.ToString());
            }
        }

        /// <summary>
        /// NULLを0に変換
        /// </summary>
        /// <param name="objNValue"></param>
        /// <returns></returns>
        public static decimal NulltoDcm(object objNValue)
        {
            if (objNValue == null)
            {
                return 0;
            }
            else if (objNValue.ToString() == "")
            {
                return 0;
            }
            else
            {
                return decimal.Parse(objNValue.ToString());
            }
        }

        /// <summary>
        /// 文字列からの部分文字列の抽出を右から行います
        /// </summary>
        /// <param name="target">対象の文字列</param>
        /// <param name="length">部分文字列の長さ</param>
        /// <returns>文字列の右から抽出された部分文字列</returns>
        public static string SubstringRight(string target, int length)
        {
            return target.Substring(target.Length - length, length);
        }

        /// <summary>
        /// イベントログ出力
        /// </summary>
        /// <param name="intLevel">レベル</param>
        /// <param name="strMessage">メッセージ</param>
        public static void WriteEventLog(int intLevel, string strMessage)
        {
            switch (intLevel)
            {
                case g_CON_LEVEL_FATAL:
                    log.Fatal(strMessage);
                    break;
                case g_CON_LEVEL_ERROR:
                    log4net.ThreadContext.Properties["EventID"] = 99;
                    log.Error(strMessage);
                    break;
                case g_CON_LEVEL_WARN:
                    log4net.ThreadContext.Properties["EventID"] = 88;
                    log.Warn(strMessage);
                    break;
                case g_CON_LEVEL_INFO:
                    log4net.ThreadContext.Properties["EventID"] = 11;
                    log.Info(strMessage);
                    break;
                case g_CON_LEVEL_DEBUG:
                    log.Debug(strMessage);
                    break;
            }
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strKey">キー</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        private static void GetAppConfigValue(string strKey, ref string strValue)
        {
            strValue = ConfigurationManager.AppSettings[strKey];
            if (strValue == null)
            {
                m_sbErrMessage.AppendLine("Key[" + strKey + "] AppConfigに存在しません。");
            }
        }

        #region SQL定数
        //品番情報更新SQL
        public const string g_CON_INSERT_MST_PRODUCT_INFO =
            @"INSERT INTO mst_product_info (file_num
                                          , register_flg
                                          , selection_flg
                                          , product_name
                                          , inspection_param_num
                                          , airbag_imagepath
                                          , length
                                          , width
                                          , marker_color_flat
                                          , marker_color_back
                                          , auto_print
                                          , auto_inspection_stop
                                          , regimark_1_imagepath
                                          , regimark_1_point_x
                                          , regimark_1_point_y
                                          , regimark_1_size_w
                                          , regimark_1_size_h
                                          , regimark_2_imagepath
                                          , regimark_2_point_x
                                          , regimark_2_point_y
                                          , regimark_2_size_w
                                          , regimark_2_size_h
                                          , base_point_1_x
                                          , base_point_1_y
                                          , base_point_2_x
                                          , base_point_2_y
                                          , base_point_3_x
                                          , base_point_3_y
                                          , base_point_4_x
                                          , base_point_4_y
                                          , base_point_5_x
                                          , base_point_5_y
                                          , point_1_plus_direction_x
                                          , point_1_plus_direction_y
                                          , point_2_plus_direction_x
                                          , point_2_plus_direction_y
                                          , point_3_plus_direction_x
                                          , point_3_plus_direction_y
                                          , point_4_plus_direction_x
                                          , point_4_plus_direction_y
                                          , point_5_plus_direction_x
                                          , point_5_plus_direction_y
                                          , stretch_rate_x
                                          , stretch_rate_y
                                          , stretch_rate_x_upd
                                          , stretch_rate_y_upd
                                          , regimark_3_imagepath
                                          , regimark_4_imagepath
                                          , stretch_rate_auto_calc_flg
                                          , width_coefficient
                                          , correct_value
                                          , black_thread_cnt_per_line
                                          , measuring_black_thread_num
                                          , camera_num
                                         ) VALUES ( 
                                            :file_num
                                          , :RegistFlg
                                          , :SelectFlg
                                          , :Name
                                          , :ParamNo
                                          , :ImageFile
                                          , :Length
                                          , :Width
                                          , :MarkerColor1
                                          , :MarkerColor2
                                          , :AutoPrint 
                                          , :AutoStop
                                          , :TempFile1
                                          , :regimark_1_point_x
                                          , :regimark_1_point_y
                                          , :regimark_1_size_w
                                          , :regimark_1_size_h
                                          , :TempFile2
                                          , :regimark_2_point_x
                                          , :regimark_2_point_y
                                          , :regimark_2_size_w
                                          , :regimark_2_size_h
                                          , :base_point_1_x
                                          , :base_point_1_y
                                          , :base_point_2_x
                                          , :base_point_2_y
                                          , :base_point_3_x
                                          , :base_point_3_y
                                          , :base_point_4_x
                                          , :base_point_4_y
                                          , :base_point_5_x
                                          , :base_point_5_y
                                          , :point_1_plus_direction_x
                                          , :point_1_plus_direction_y
                                          , :point_2_plus_direction_x
                                          , :point_2_plus_direction_y
                                          , :point_3_plus_direction_x
                                          , :point_3_plus_direction_y
                                          , :point_4_plus_direction_x
                                          , :point_4_plus_direction_y
                                          , :point_5_plus_direction_x
                                          , :point_5_plus_direction_y
                                          , (CAST(:AreaMagX as NUMERIC) / 100)
                                          , (CAST(:AreaMagY as NUMERIC) / 100)
                                          , null
                                          , null
                                          , :TempFile3
                                          , :TempFile4
                                          , :AutoCalcAreaMagFlg
                                          , (CAST(:AreaMagCoefficient as NUMERIC) / 100)
                                          , (CAST(:AreaMagCorrection as NUMERIC) / 100)
                                          , :BThreadNum
                                          , :BThreadNo
                                          , :BTCamNo
                                         ) ON CONFLICT (file_num)
                                           DO UPDATE SET register_flg = :RegistFlg
                                                       , selection_flg = :SelectFlg
                                                       , product_name = :Name
                                                       , inspection_param_num = :ParamNo
                                                       , airbag_imagepath = :ImageFile
                                                       , length = :Length
                                                       , width = :Width
                                                       , marker_color_flat = :MarkerColor1
                                                       , marker_color_back = :MarkerColor2
                                                       , auto_print = :AutoPrint 
                                                       , auto_inspection_stop = :AutoStop
                                                       , regimark_1_imagepath = :TempFile1
                                                       , regimark_1_point_x = :regimark_1_point_x
                                                       , regimark_1_point_y = :regimark_1_point_y
                                                       , regimark_1_size_w = :regimark_1_size_w
                                                       , regimark_1_size_h = :regimark_1_size_h
                                                       , regimark_2_imagepath = :TempFile2
                                                       , regimark_2_point_x = :regimark_2_point_x
                                                       , regimark_2_point_y = :regimark_2_point_y
                                                       , regimark_2_size_w = :regimark_2_size_w
                                                       , regimark_2_size_h = :regimark_2_size_h
                                                       , base_point_1_x = :base_point_1_x
                                                       , base_point_1_y = :base_point_1_y
                                                       , base_point_2_x = :base_point_2_x
                                                       , base_point_2_y = :base_point_2_y
                                                       , base_point_3_x = :base_point_3_x
                                                       , base_point_3_y = :base_point_3_y
                                                       , base_point_4_x = :base_point_4_x
                                                       , base_point_4_y = :base_point_4_y
                                                       , base_point_5_x = :base_point_5_x
                                                       , base_point_5_y = :base_point_5_y
                                                       , point_1_plus_direction_x = :point_1_plus_direction_x
                                                       , point_1_plus_direction_y = :point_1_plus_direction_y
                                                       , point_2_plus_direction_x = :point_2_plus_direction_x
                                                       , point_2_plus_direction_y = :point_2_plus_direction_y
                                                       , point_3_plus_direction_x = :point_3_plus_direction_x
                                                       , point_3_plus_direction_y = :point_3_plus_direction_y
                                                       , point_4_plus_direction_x = :point_4_plus_direction_x
                                                       , point_4_plus_direction_y = :point_4_plus_direction_y
                                                       , point_5_plus_direction_x = :point_5_plus_direction_x
                                                       , point_5_plus_direction_y = :point_5_plus_direction_y
                                                       , stretch_rate_x = (CAST(:AreaMagX as NUMERIC) / 100)
                                                       , stretch_rate_y = (CAST(:AreaMagY as NUMERIC) / 100)
                                                       , stretch_rate_x_upd = null
                                                       , stretch_rate_y_upd = null
                                                       , regimark_3_imagepath = :TempFile3
                                                       , regimark_4_imagepath = :TempFile4
                                                       , stretch_rate_auto_calc_flg = :AutoCalcAreaMagFlg
                                                       , width_coefficient = (CAST(:AreaMagCoefficient as NUMERIC) / 100)
                                                       , correct_value = (CAST(:AreaMagCorrection as NUMERIC) / 100)
                                                       , black_thread_cnt_per_line = :BThreadNum
                                                       , measuring_black_thread_num = :BThreadNo
                                                       , camera_num = :BTCamNo";

        // PTC情報更新SQL
        public const string g_CON_UPDATE_MST_PRODUCT_INFO_PTC =
            @"UPDATE mst_product_info SET line_length = :LINE_LENGTH
                                        , regimark_between_length = :MARK_INTERVAL
                                    WHERE file_num = :KIND ";

        // エアバッグ情報更新SQL
        public const string g_CON_UPDATE_MST_PRODUCT_INFO_AIRBAG =
            @"UPDATE mst_product_info SET column_cnt = :Number
                                        , top_point_a = :strTopPointA
                                        , top_point_b = :strTopPointB
                                        , top_point_c = :strTopPointC
                                        , top_point_d = :strTopPointD
                                        , top_point_e = :strTopPointE
                                    WHERE file_num = :file_num ";

        // 品番カメラ更新SQL
        public const string g_CON_UPDATE_MST_PRODUCT_INFO_CAMERA =
            @"UPDATE mst_product_info SET illumination_information = :intIlluminationInformation
                                        , start_regimark_camera_num = :intStartRegimarkCameraNum
                                        , end_regimark_camera_num = :intEndRegimarkCameraNum
                                    WHERE product_name = :strProductName ";

        // 閾値マスタ更新SQL
        public const string g_CON_UPDATE_MST_PRODUCT_INFO_THRESHOLD =
            @"UPDATE mst_product_info SET taking_camera_cnt = :intTakingCameraCnt
                                        , column_threshold_01 = :intColumnThreshold01
                                        , column_threshold_02 = :intColumnThreshold02
                                        , column_threshold_03 = :intColumnThreshold03
                                        , column_threshold_04 = :intColumnThreshold04
                                        , line_threshold_a1 = :intLineThresholda1
                                        , line_threshold_a2 = :intLineThresholda2
                                        , line_threshold_b1 = :intLineThresholdb1
                                        , line_threshold_b2 = :intLineThresholdb2
                                        , line_threshold_c1 = :intLineThresholdc1
                                        , line_threshold_c2 = :intLineThresholdc2
                                        , line_threshold_d1 = :intLineThresholdd1
                                        , line_threshold_d2 = :intLineThresholdd2
                                        , line_threshold_e1 = :intLineThresholde1
                                        , line_threshold_e2 = :intLineThresholde2
                                        , ai_model_name = :strAiModelName
                                    WHERE product_name = :strProductName ";

        // 判定理由マスタ登録SQL
        public const string g_CON_INSERT_MST_PRODUCT_INFO_DECISION_REASON =
            @"INSERT INTO mst_decision_reason (reason_code, decision_reason)
                                       VALUES (:intReasonCode, :strDecisionReason)";

        // 判定理由マスタ削除SQL
        public const string g_CON_DELETE_MST_PRODUCT_INFO_DECISION_REASON =
            @"DELETE FROM mst_decision_reason";

        // 品名取得SQL
        public const string g_CON_SELECT_MST_PRODUCT_INFO_PMS =
            @"SELECT 'false',product_name FROM mst_product_info ORDER BY product_name";

        // 判定理由取得SQL
        public const string g_CON_SELECT_INFO_DECISION_REASON_SER =
            @"SELECT reason_code, decision_reason FROM mst_decision_reason ORDER BY reason_code";

        // 品名マスタメンテトップ画面取得SQL
        public const string g_CON_SELECT_MST_PRODUCT_INFO_TOP =
            @"SELECT
                  *
              FROM
                  (
                   SELECT
                       product_name
                     , airbag_imagepath
                     , length
                     , width
                     , line_length
                     , stretch_rate_x
                     , stretch_rate_x_upd
                     , stretch_rate_y
                     , stretch_rate_y_upd
                     , ai_model_non_inspection_flg
                     , ai_model_name
                     , regimark_between_length
                     , regimark_1_point_x
                     , regimark_1_point_y
                     , regimark_2_point_x
                     , regimark_2_point_y
                     , base_point_1_x
                     , base_point_1_y
                     , point_1_plus_direction_x
                     , point_1_plus_direction_y
                     , base_point_2_x
                     , base_point_2_y
                     , point_2_plus_direction_x
                     , point_2_plus_direction_y
                     , base_point_3_x
                     , base_point_3_y
                     , point_3_plus_direction_x
                     , point_3_plus_direction_y
                     , base_point_4_x
                     , base_point_4_y
                     , point_4_plus_direction_x
                     , point_4_plus_direction_y
                     , base_point_5_x
                     , base_point_5_y
                     , point_5_plus_direction_x
                     , point_5_plus_direction_y
                     , start_regimark_camera_num
                     , end_regimark_camera_num
                     , illumination_information
                     , column_threshold_01
                     , column_threshold_02
                     , column_threshold_03
                     , column_threshold_04
                     , line_threshold_a1
                     , line_threshold_a2
                     , line_threshold_b1
                     , line_threshold_b2
                     , line_threshold_c1
                     , line_threshold_c2
                     , line_threshold_d1
                     , line_threshold_d2
                     , line_threshold_e1
                     , line_threshold_e2
                   FROM
                       mst_product_info
                   ORDER BY
                       product_name
                  ) mpitop
              LIMIT 1 OFFSET 0";

        // 品名マスタメンテトップ画面取得SQL
        public const string g_CON_SELECT_MST_PRODUCT_INFO_PRN =
            @"SELECT
                  product_name
                , airbag_imagepath
                , length
                , width
                , line_length
                , stretch_rate_x
                , stretch_rate_x_upd
                , stretch_rate_y
                , stretch_rate_y_upd
                , ai_model_non_inspection_flg
                , ai_model_name
                , regimark_between_length
                , regimark_1_point_x
                , regimark_1_point_y
                , regimark_2_point_x
                , regimark_2_point_y
                , base_point_1_x
                , base_point_1_y
                , point_1_plus_direction_x
                , point_1_plus_direction_y
                , base_point_2_x
                , base_point_2_y
                , point_2_plus_direction_x
                , point_2_plus_direction_y
                , base_point_3_x
                , base_point_3_y
                , point_3_plus_direction_x
                , point_3_plus_direction_y
                , base_point_4_x
                , base_point_4_y
                , point_4_plus_direction_x
                , point_4_plus_direction_y
                , base_point_5_x
                , base_point_5_y
                , point_5_plus_direction_x
                , point_5_plus_direction_y
                , start_regimark_camera_num
                , end_regimark_camera_num
                , illumination_information
                , column_threshold_01
                , column_threshold_02
                , column_threshold_03
                , column_threshold_04
                , line_threshold_a1
                , line_threshold_a2
                , line_threshold_b1
                , line_threshold_b2
                , line_threshold_c1
                , line_threshold_c2
                , line_threshold_d1
                , line_threshold_d2
                , line_threshold_e1
                , line_threshold_e2
              FROM
                  mst_product_info
              WHERE
                  product_name = :product_name";

        // 品名マスタメンテ画面更新SQL
        public const string g_CON_UPDATE_MST_PRODUCT_INFO_DISP_INPUT =
            @"UPDATE mst_product_info SET ai_model_non_inspection_flg = :ai_model_non_inspection_flg
                                        , ai_model_name = :ai_model_name
                                        , stretch_rate_x_upd = :stretch_rate_x_upd
                                        , stretch_rate_y_upd = :stretch_rate_y_upd
                                        , column_threshold_01 = :column_threshold_01
                                        , column_threshold_02 = :column_threshold_02
                                        , column_threshold_03 = :column_threshold_03
                                        , column_threshold_04 = :column_threshold_04
                                        , line_threshold_a1 = :line_threshold_a1
                                        , line_threshold_a2 = :line_threshold_a2
                                        , line_threshold_b1 = :line_threshold_b1
                                        , line_threshold_b2 = :line_threshold_b2
                                        , line_threshold_c1 = :line_threshold_c1
                                        , line_threshold_c2 = :line_threshold_c2
                                        , line_threshold_d1 = :line_threshold_d1
                                        , line_threshold_d2 = :line_threshold_d2
                                        , line_threshold_e1 = :line_threshold_e1
                                        , line_threshold_e2 = :line_threshold_e2
                                      WHERE product_name = :product_name ";

        // 品名マスタメンテ削除SQL
        public const string g_CON_DELETE_MST_PRODUCT_INFO_DISP_INPUT =
            @"DELETE FROM mst_product_info WHERE product_name = :product_name ";
        #endregion
    }
}
