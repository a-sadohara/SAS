using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using log4net;
using Npgsql;
using System.Data;

namespace ProductMstMaintenance
{
    static class Common
    {
        // DB接続文字列
        public static string g_ConnectionString;

        // イベントログ出力関連
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            try
            {
                // 接続文字列をApp.configファイルから取得
                ConnectionStringSettings setConStr = null;
                setConStr = ConfigurationManager.ConnectionStrings["ConnectionString"];
                if (setConStr == null)
                {
                    // ログ出力
                    WriteEventLog(g_CON_LEVEL_ERROR, "App.configの[ConnectionString]の設定値取得時にエラーが発生しました。");
                    // メッセージ出力
                    MessageBox.Show("App.configの[ConnectionString]の設定値取得に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                else
                {
                    // TODO ★接続確認ロジックを付与★
                }
                g_ConnectionString = setConStr.ConnectionString;
            }
            catch (Exception ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "画面起動時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("画面起動処理に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProductMstMaintenance());
        }

        /// <summary>
        /// App.configファイルから設定値を取得
        /// </summary>
        /// <param name="strName">要素名</param>
        /// <returns>null:設定なし(取得失敗) それ以外:設定あり(取得成功)</returns>
        public static string strGetAppConfigValue(string strName)
        {
            string strValue = ConfigurationManager.AppSettings[strName];
            if (strValue == null)
                MessageBox.Show("App.configの[" + strName + "]の設定値取得に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return strValue;
        }

        /// <summary>
        /// システム設定情報から設定値を取得
        /// </summary>
        /// <param name="strId">ID</param>
        /// <param name="strValue">設定値</param>
        /// <returns>true:正常終了 false:異常終了</returns>
        public static bool bolGetSystemSettingValue(string strId, out string strValue)
        {
            string strSQL = "";
            DataTable dtData;
            string strGetValue = "";

            try
            {
                // SQL抽出から情報を取得
                using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
                {
                    NpgsqlCon.Open();

                    NpgsqlCommand NpgsqlCom = null;
                    NpgsqlDataAdapter NpgsqlDtAd = null;
                    dtData = new DataTable();
                    strSQL = @"SELECT value FROM system_setting_info WHERE id = '" + strId + "'; ";
                    NpgsqlCom = new NpgsqlCommand(strSQL, NpgsqlCon);
                    NpgsqlDtAd = new NpgsqlDataAdapter(NpgsqlCom);
                    NpgsqlDtAd.Fill(dtData);

                    //  検査番号
                    strGetValue = dtData.Rows[0]["value"].ToString();
                }

                return true;
            }
            catch (NpgsqlException ex)
            {
                // ログ出力
                WriteEventLog(g_CON_LEVEL_ERROR, "DBアクセス時にエラーが発生しました。\r\n" + ex.Message);
                // メッセージ出力
                MessageBox.Show("システム設定情報の取得で例外が発生しました。", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            finally
            {
                strValue = strGetValue;
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
        /// 登録・更新処理実行
        /// </summary>
        /// <param name="nscCommand">実行SQLコマンド</param>
        /// <param name="transaction">トランザクション情報</param>
        /// <returns></returns>
        public static Boolean ExecTranSQL(NpgsqlCommand nscCommand
                                        , NpgsqlTransaction transaction)
        {
            try
            {
                nscCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException ex)
            {
                transaction.Rollback();
                MessageBox.Show("DB更新時にエラーが発生しました。"
                              + Environment.NewLine
                              + ex.Message);
                return false;
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
        /// 指定されたファイルがロックされているかどうかを返します。
        /// </summary>
        /// <param name="path">検証したいファイルへのフルパス</param>
        /// <returns>ロックされているかどうか</returns>
        public static Boolean IsFileLocked(string path)
        {
            FileStream stream = null;

            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }

        #region SQL定数
        // 品番情報更新SQL
        public const string g_CON_INSERT_MST_PRODUCT_INFO =
            @"INSERT INTO mst_product_info (file_num
                                          , register_num
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
                                          , :register_num
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
                                          , :AreaMagX
                                          , :AreaMagY
                                          , :stretch_rate_x_upd
                                          , :stretch_rate_y_upd
                                          , :TempFile3
                                          , :TempFile4
                                          , :AutoCalcAreaMagFlg
                                          , :AreaMagCoefficient
                                          , :AreaMagCorrection
                                          , :BThreadNum
                                          , :BThreadNo
                                          , :BTCamNo
                                         ) ON CONFLICT (file_num)
                                           DO UPDATE SET register_num = :register_num
                                                       , register_flg = :RegistFlg
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
                                                       , stretch_rate_x = :AreaMagX
                                                       , stretch_rate_y = :AreaMagY
                                                       , stretch_rate_x_upd = :stretch_rate_x_upd
                                                       , stretch_rate_y_upd = :stretch_rate_y_upd
                                                       , regimark_3_imagepath = :TempFile3
                                                       , regimark_4_imagepath = :TempFile4
                                                       , stretch_rate_auto_calc_flg = :AutoCalcAreaMagFlg
                                                       , width_coefficient = :AreaMagCoefficient
                                                       , correct_value = :AreaMagCorrection
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
                                    WHERE file_num = :file_num ";
        #endregion
    }
}
