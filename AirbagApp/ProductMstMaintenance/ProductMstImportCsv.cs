using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using Npgsql;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using static ProductMstMaintenance.Common;

namespace ProductMstMaintenance
{
    public partial class ProductMstImportCsv : Form
    {
        #region 定数
        // ファイル名指定
        private const string m_CON_FILE_NAME_REGISTER_INI_DATA = "RegisterIniData";
        private const string m_CON_FILE_NAME_CONFIG_PLC = "ConfigPLC";
        private const string m_CON_FILE_NAME_AIRBAG_COORD = "AirBagCoord";
        private const string m_CON_FILE_NAME_CAMERA_INFO = "カメラ情報";
        private const string m_CON_FILE_NAME_THRESHOLD_INFO = "閾値情報";
        private const string m_CON_FILE_NAME_REASON_JUDGMENT = "判定理由マスタ";
        private const string m_CON_FILE_NAME_COPY_PNG = "*";

        // INIファイルのセクション
        private const string m_CON_INI_SECTION_REGISTER = "REGISTER";
        private const string m_CON_INI_SECTION_KIND = "KIND";
        private const string m_CON_INI_SECTION_AIRBAG = "AIRBAG";

        // ログファイルの出力アプリ名
        private const string m_CON_OUTLOGFILE_NAME = "HinNoImportLog";

        // 品番情報INI 取り込み時特殊対応項目
        private const string m_CON_COL_FILE_NUM = "file_num";
        private const string m_CON_COL_REG_NUM = "register_num";
        private const string m_CON_COL_RATEXUPD = "stretch_rate_x_upd";
        private const string m_CON_COL_RATEYUPD = "stretch_rate_y_upd";

        // PTCINI 取り込み時特殊対応項目
        private const string m_CON_COL_PLC_KIND = "KIND";

        // エアバッグINI 取り込み時特殊対応項目
        private const string m_CON_COL_AIRBAG_NUMBER = "Number";

        // CSVファイル配置情報
        private const int m_CON_COL_PRODUCT_NAME = 0;
        private const int m_CON_COL_ILLUMINATION_INFORMATION = 1;
        private const int m_CON_COL_START_REGIMARK_CAMERA_NUM = 2;
        private const int m_CON_COL_END_REGIMARK_CAMERA_NUM = 3;

        /// <summary>
        /// INIファイルのセパレータ
        /// </summary>
        private const Char m_CON_SEPARATOR_XY = ' ';
        #endregion

        #region 変数
        // 品番登録情報テーブルへの成功・失敗件数
        private static int m_intSuccesRegProductInfo = 0;
        private static int m_intErrorRegProductInfo = 0;

        // PTCの成功・失敗件数
        private static int m_intSuccesRegPTC = 0;
        private static int m_intErrorRegPTC = 0;

        // エアバッグの成功・失敗件数
        private static int m_intSuccesRegAirBag = 0;
        private static int m_intErrorRegAirBag = 0;

        /// <summary>
        /// 特殊対応ディクショナリ
        /// </summary>
        private static Dictionary<string, string> m_RegisterUniqueIni = new Dictionary<string, string>
        {
            {"regimark_1_point_x","TempPoint1"},
            {"regimark_1_point_y","TempPoint1"},
            {"regimark_1_size_w","TempSize1"},
            {"regimark_1_size_h","TempSize1"},
            {"regimark_2_point_x","TempPoint2"},
            {"regimark_2_point_y","TempPoint2"},
            {"regimark_2_size_w","TempSize2"},
            {"regimark_2_size_h","TempSize2"},
            {"base_point_1_x","BasePoint1"},
            {"base_point_1_y","BasePoint1"},
            {"base_point_2_x","BasePoint2"},
            {"base_point_2_y","BasePoint2"},
            {"base_point_3_x","BasePoint3"},
            {"base_point_3_y","BasePoint3"},
            {"base_point_4_x","BasePoint4"},
            {"base_point_4_y","BasePoint4"},
            {"base_point_5_x","BasePoint5"},
            {"base_point_5_y","BasePoint5"},
            {"point_1_plus_direction_x","Plus1"},
            {"point_1_plus_direction_y","Plus1"},
            {"point_2_plus_direction_x","Plus2"},
            {"point_2_plus_direction_y","Plus2"},
            {"point_3_plus_direction_x","Plus3"},
            {"point_3_plus_direction_y","Plus3"},
            {"point_4_plus_direction_x","Plus4"},
            {"point_4_plus_direction_y","Plus4"},
            {"point_5_plus_direction_x","Plus5"},
            {"point_5_plus_direction_y","Plus5"}
        };

        /// <summary>
        /// スプリット対象項目
        /// </summary>
        private static Dictionary<string, int> m_RegisterUniqueIniSplit = new Dictionary<string, int>
        {
            {"regimark_1_point_x",0},
            {"regimark_1_point_y",1},
            {"regimark_1_size_w",0},
            {"regimark_1_size_h",1},
            {"regimark_2_point_x",0},
            {"regimark_2_point_y",1},
            {"regimark_2_size_w",0},
            {"regimark_2_size_h",1},
            {"base_point_1_x",0},
            {"base_point_1_y",1},
            {"base_point_2_x",0},
            {"base_point_2_y",1},
            {"base_point_3_x",0},
            {"base_point_3_y",1},
            {"base_point_4_x",0},
            {"base_point_4_y",1},
            {"base_point_5_x",0},
            {"base_point_5_y",1},
            {"point_1_plus_direction_x",0},
            {"point_1_plus_direction_y",1},
            {"point_2_plus_direction_x",0},
            {"point_2_plus_direction_y",1},
            {"point_3_plus_direction_x",0},
            {"point_3_plus_direction_y",1},
            {"point_4_plus_direction_x",0},
            {"point_4_plus_direction_y",1},
            {"point_5_plus_direction_x",0},
            {"point_5_plus_direction_y",1}
        };

        /// <summary>
        /// 品番情報INI格納構造体
        /// </summary>
        private class IniDataRegister
        {
            public string file_num { get; set; }
            public string register_num { get; set; }
            public int RegistFlg { get; set; }
            public int SelectFlg { get; set; }
            public string Name { get; set; }
            public int ParamNo { get; set; }
            public string ImageFile { get; set; }
            public int Length { get; set; }
            public int Width { get; set; }
            public int MarkerColor1 { get; set; }
            public int MarkerColor2 { get; set; }
            public int AutoPrint { get; set; }
            public int AutoStop { get; set; }
            public string TempFile1 { get; set; }
            public int regimark_1_point_x { get; set; }
            public int regimark_1_point_y { get; set; }
            public int regimark_1_size_w { get; set; }
            public int regimark_1_size_h { get; set; }
            public string TempFile2 { get; set; }
            public int regimark_2_point_x { get; set; }
            public int regimark_2_point_y { get; set; }
            public int regimark_2_size_w { get; set; }
            public int regimark_2_size_h { get; set; }
            public int base_point_1_x { get; set; }
            public int base_point_1_y { get; set; }
            public int base_point_2_x { get; set; }
            public int base_point_2_y { get; set; }
            public int base_point_3_x { get; set; }
            public int base_point_3_y { get; set; }
            public int base_point_4_x { get; set; }
            public int base_point_4_y { get; set; }
            public int base_point_5_x { get; set; }
            public int base_point_5_y { get; set; }
            public int point_1_plus_direction_x { get; set; }
            public int point_1_plus_direction_y { get; set; }
            public int point_2_plus_direction_x { get; set; }
            public int point_2_plus_direction_y { get; set; }
            public int point_3_plus_direction_x { get; set; }
            public int point_3_plus_direction_y { get; set; }
            public int point_4_plus_direction_x { get; set; }
            public int point_4_plus_direction_y { get; set; }
            public int point_5_plus_direction_x { get; set; }
            public int point_5_plus_direction_y { get; set; }
            public int AreaMagX { get; set; }
            public int AreaMagY { get; set; }
            public int stretch_rate_x_upd { get; set; }
            public int stretch_rate_y_upd { get; set; }
            public string TempFile3 { get; set; }
            public string TempFile4 { get; set; }
            public int AutoCalcAreaMagFlg { get; set; }
            public int AreaMagCoefficient { get; set; }
            public int AreaMagCorrection { get; set; }
            public int BThreadNum { get; set; }
            public string BThreadNo { get; set; }
            public int BTCamNo { get; set; }

        }

        /// <summary>
        /// PLCINI格納構造体
        /// </summary>
        private class IniConfigPLC 
        {
            public string KIND { get; set; }
            public int LINE_LENGTH { get; set; }
            public int MARK_INTERVAL { get; set; }
        }

        /// <summary>
        /// エアバッグ領域設定INI構造体
        /// </summary>
        private class IniAirBagCoord 
        {
            public string file_num { get; set; }
            public int Number { get; set; }
        }

        /// <summary>
        /// カメラ情報CSVファイル
        /// </summary>
        public class CameraCsvInfo
        {
            public string strProductName { get; set; }
            public string strIlluminationInformation { get; set; }
            public string strStartRegimarkCameraNum { get; set; }
            public string strEndRegimarkCameraNum { get; set; }
        }
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public ProductMstImportCsv()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// CSV取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {
            List<IniDataRegister> lstDataRegistersToDB = new List<IniDataRegister>();
            List<IniConfigPLC> lstPLCDataToDB = new List<IniConfigPLC>();
            List<IniAirBagCoord> lstAirBagCoordToDB = new List<IniAirBagCoord>();

            // 未入力チェック
            if (txtFolder.Text == "")
            {
                MessageBox.Show("フォルダを選択してください");
                btnSearchFolder.Focus();
                return;
            }

            if (MessageBox.Show("指定したフォルダのファイルを取込みます。\r\nよろしいですか？"
                              , "確認"
                              , MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                DirectoryInfo directorySearchFolder = new DirectoryInfo(txtFolder.Text);

                FileInfo[] fileInfosInput = directorySearchFolder.GetFiles("*.*"
                                                                         , SearchOption.AllDirectories);
                if (fileInfosInput.Length > 0)
                {
                    // フォルダ内のファイルの数だけループする
                    foreach (FileInfo Inputfile in fileInfosInput)
                    {
                        // 対象のファイルがロックされているか確認する
                        if (IsFileLocked(Inputfile.FullName) == true)
                        {
                            // ファイルがロックされている場合、スキップする
                            continue;
                        }

                        // 品番情報ファイルを判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_REGISTER_INI_DATA + "[0-9][0-9]*.INI") == true)
                        {
                            // 品番情報ファイルの場合、取り込みを行う
                            lstDataRegistersToDB = ImportRegisterIniData(Inputfile);

                            // 読み込んだ値に対してチェック処理を行う
                            if (CheckRegisterIniData(lstDataRegistersToDB
                                                   , Inputfile.Name) == false)
                            {
                                continue;
                            }

                            // 読み込んだ値をDBに登録する
                            InsertMstProductInfo(lstDataRegistersToDB);
                        }

                        // PLC設定ファイルを判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_CONFIG_PLC + ".INI") == true)
                        {
                            // PLC設定より、レジマーク間距離を取得し登録する。
                            lstPLCDataToDB = ImportPLCIniData(Inputfile);

                            // 読み込んだ値に対してチェック処理を行う
                            if (CheckPLCIniData(lstPLCDataToDB
                                              , Inputfile.Name) == false)
                            {
                                continue;
                            }

                            // 読み込んだ値をDBに登録する
                            UPDMstProductInfoInPTC(lstPLCDataToDB);
                        }

                        // エアバック領域設定ファイルを判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_AIRBAG_COORD + "[0-9][0-9]*.INI") == true)
                        {
                            // エアバック領域設定より、列数を取得し登録する。
                            lstAirBagCoordToDB = ImportAirBagCoordIniData(Inputfile);

                            // 読み込んだ値に対してチェック処理を行う
                            if (CheckAirbagIniData(lstAirBagCoordToDB
                                                 , Inputfile.Name) == false)
                            {
                                continue;
                            }

                            // 読み込んだ値をDBに登録する
                            UPDMstProductInfoInAirbag(lstAirBagCoordToDB);
                        }

                        // カメラ情報を判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_CAMERA_INFO + ".CSV") == true)
                        {
                            // CSVファイルを取り込み、カメラ情報を取得し登録する。

                        }

                        // 閾値情報を判定する。
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_THRESHOLD_INFO + ".CSV") == true)
                        {
                            // CSVファイルを取り込み、閾値情報を取得し登録する。

                        }

                        // 判定理由マスタを判定する。
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_REASON_JUDGMENT + ".CSV") == true)
                        {
                            // CSVファイルを取り込み、判定理由マスタを登録する

                        }

                        // マスタ画像を判定する。
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_COPY_PNG + ".PNG") == true)
                        {
                            // マスタ画像を取り込み先のフォルダにコピーする。

                        }
                    }

                    MessageBox.Show("取込み処理が完了しました");
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("フォルダ内にファイルが存在しません。");
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
            }
        }

        /// <summary>
        /// フォルダ検索ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchFolder_Click(object sender, EventArgs e)
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //RootFolder以下にあるフォルダである必要がある
            fbd.SelectedPath = @"C:\Windows";
            //ユーザーが新しいフォルダを作成できるようにする
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                //選択されたフォルダを表示する
                txtFolder.Text = fbd.SelectedPath;
            }
        }
        #endregion

        #region ラッパー
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(
            string lpApplicationName,
            string lpKeyName,
            string lpDefault,
            StringBuilder lpReturnedstring,
            int nSize,
            string lpFileName);

        private static string GetIniValue(string path, string section, string key)
        {
            StringBuilder sb = new StringBuilder(256);
            GetPrivateProfileString(section, key, string.Empty, sb, sb.Capacity, path);
            return sb.ToString();
        }

        private static string GetName<T>(Expression<Func<T>> e)
        {
            var member = (MemberExpression)e.Body;
            return member.Member.Name;
        }
        #endregion

        #region メソッド
        #region 品番情報取り込み
        /// <summary>
        /// 品番情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniDataRegister> ImportRegisterIniData(FileInfo Inputfile)
        {
            List<IniDataRegister> lstDataRegisterDB = new List<IniDataRegister>();
            List<string> lstRegisterIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // 品番情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    if (strFileTextLine == " ")
                    {
                        // 空行（最終行）の場合読み飛ばす
                        continue;
                    }

                    // 品番情報ファイル内のセクションを取得する
                    if (Regex.IsMatch(strFileTextLine, "[" + m_CON_INI_SECTION_REGISTER + "[0-9][0-9][0-9]]") == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", "");
                        strFileTextLineReplace = strFileTextLine.Replace("]", "");
                        lstRegisterIniSection.Add(strFileTextLine);

                    }
                }

                // セクションの数だけループする
                foreach (string strRegister in lstRegisterIniSection)
                {
                    IniDataRegister idrCurrentData = new IniDataRegister();

                    // Iniファイルの値をクラスへ格納する
                    idrCurrentData = DataRegIniToDBClass(Inputfile, strRegister);

                    lstDataRegisterDB.Add(idrCurrentData);
                }

                return lstDataRegisterDB;
            }
        }

        /// <summary>
        /// 品番情報チェック処理
        /// </summary>
        /// <param name="lstDataRegisters">読み込んだ品番情報</param>
        /// <param name="strFileName">ファイル名</param>
        /// <returns></returns>
        private static Boolean CheckRegisterIniData(List<IniDataRegister> idrCurrentData
                                                  , string strFileName)
        {
            // 入力項目が0件の場合エラー
            if (idrCurrentData.Count == 0)
            {
                OutPutImportLog("品番ファイル内0件", strFileName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 品番マスタメンテINIファイル読み込み格納処理
        /// </summary>
        /// <param name="Inputfile">読み込みINIファイル</param>
        /// <param name="strRegister">セクション</param>
        /// <returns></returns>
        private static IniDataRegister DataRegIniToDBClass(FileInfo Inputfile
                                                         , string strRegister)
        {
            IniDataRegister idrCurrentData = new IniDataRegister();

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniDataRegister);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // ファイルナンバーの格納を行う
            idrCurrentData.file_num =
                SubstringRight(System.IO.Path.GetFileNameWithoutExtension(Inputfile.Name), 2);
            // セクションナンバーの格納を行う
            idrCurrentData.register_num = SubstringRight(strRegister, 3);

            string strTenmIniValue = "";

            // Iniファイルから各値を読み込む
            foreach (var fieldInfo in fieldInfos)
            {
                // 特殊対応ディクショナリに読み込むキーが存在するかチェックする
                string strUniKey = "";
                m_RegisterUniqueIni.TryGetValue(fieldInfo.Name, out strUniKey);

                int intUniSplitVal = 99999;
                m_RegisterUniqueIniSplit.TryGetValue(fieldInfo.Name, out intUniSplitVal);

                // フィールド名ごとに値の格納処理を行う
                if (fieldInfo.Name == m_CON_COL_FILE_NUM ||
                    fieldInfo.Name == m_CON_COL_REG_NUM)
                {
                    // ファイル名セクション名は設定済みのためスキップ
                    continue;
                }
                else if (fieldInfo.Name == m_CON_COL_RATEXUPD ||
                         fieldInfo.Name == m_CON_COL_RATEYUPD)
                {
                    // 更新用項目は空文字
                    fieldInfo.SetValue(idrCurrentData, "");
                }
                else if (strUniKey != "")
                {
                    // 特殊対応項目の場合は特殊対応して設定
                    strTenmIniValue = NulltoString(GetIniValue(Inputfile.FullName
                                                             , strRegister
                                                             , GetName(() => strUniKey)));
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        fieldInfo.SetValue(idrCurrentData
                                         , NulltoString(strTenmIniValue.Split(m_CON_SEPARATOR_XY)[intUniSplitVal]));
                    }
                    else
                    {
                        fieldInfo.SetValue(idrCurrentData
                                         , NulltoInt(strTenmIniValue.Split(m_CON_SEPARATOR_XY)[intUniSplitVal]));
                    }
                }
                else
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(idrCurrentData
                                         , NulltoInt(GetIniValue(Inputfile.FullName
                                                               , strRegister
                                                               , GetName(() => fieldInfo.Name))));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(idrCurrentData
                                         , NulltoString(GetIniValue(Inputfile.FullName
                                                                  , strRegister
                                                                  , GetName(() => fieldInfo.Name))));
                    }
                }
            }

            return idrCurrentData;
        }

        /// <summary>
        /// 品番登録情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataRegistersToDB"></param>
        /// <returns></returns>
        private static void InsertMstProductInfo(List<IniDataRegister> lstDataRegistersToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {

                    foreach (IniDataRegister idrCurrentData in lstDataRegistersToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfo(idrCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesRegProductInfo = m_intSuccesRegProductInfo + 1;
                        }
                        else
                        {
                            m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                        }
                    }

                    // トランザクションコミット
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// 品番登録情報登録SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfo(IniDataRegister idrCurrentData
                                                , NpgsqlConnection NpgsqlCon
                                                , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_INSERT_MST_PRODUCT_INFO;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniDataRegister);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                        { Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command, transaction) == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("品番情報登録時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }
        #endregion

        #region PLC取り込み
        /// <summary>
        /// PLC情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniConfigPLC> ImportPLCIniData(FileInfo Inputfile)
        {
            List<IniConfigPLC> lstDataPLCDB = new List<IniConfigPLC>();
            List<string> lstPLCIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // PLCINIファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    if (strFileTextLine == " ")
                    {
                        // 空行（最終行）の場合読み飛ばす
                        continue;
                    }

                    // PLC情報ファイル内のセクションを取得する
                    if (Regex.IsMatch(strFileTextLine, "[" + m_CON_INI_SECTION_KIND + "[0-9][0-9]]") == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", "");
                        strFileTextLineReplace = strFileTextLine.Replace("]", "");
                        lstPLCIniSection.Add(strFileTextLine);
                    }
                }

                // セクションの数だけループする
                foreach (string strRegister in lstPLCIniSection)
                {
                    IniConfigPLC icpCurrentData = new IniConfigPLC();

                    // Iniファイルの値をクラスへ格納する
                    icpCurrentData = DataPLCIniToDBClass(Inputfile, strRegister);

                    lstDataPLCDB.Add(icpCurrentData);
                }

                return lstDataPLCDB;
            }
        }

        /// <summary>
        /// PLCINIファイル読み込み格納処理
        /// </summary>
        /// <param name="Inputfile">読み込みINIファイル</param>
        /// <param name="strRegister">セクション</param>
        /// <returns></returns>
        private static IniConfigPLC DataPLCIniToDBClass(FileInfo Inputfile
                                                      , string strRegister)
        {
            IniConfigPLC icpCurrentData = new IniConfigPLC();

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniDataRegister);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // セクションナンバーの格納を行う
            int intSection = NulltoInt(SubstringRight(strRegister, 2)) - 1;
            icpCurrentData.KIND = String.Format("{0:D2}", intSection);

            // Iniファイルから各値を読み込む
            foreach (var fieldInfo in fieldInfos)
            {
                // フィールド名ごとに値の格納処理を行う
                if (fieldInfo.Name == m_CON_COL_PLC_KIND)
                {
                    // ファイル名セクション名は設定済みのためスキップ
                    continue;
                }
                else
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(icpCurrentData
                                         , NulltoInt(GetIniValue(Inputfile.FullName
                                                               , strRegister
                                                               , GetName(() => fieldInfo.Name))));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(icpCurrentData
                                         , NulltoString(GetIniValue(Inputfile.FullName
                                                                  , strRegister
                                                                  , GetName(() => fieldInfo.Name))));
                    }
                }
            }

            return icpCurrentData;
        }

        /// <summary>
        /// PLCチェック処理
        /// </summary>
        /// <param name="lstCurrentData">読み込んだPLC情報</param>
        /// <param name="strFileName">ファイル名</param>
        /// <returns></returns>
        private static Boolean CheckPLCIniData(List<IniConfigPLC> lstCurrentData
                                             , string strFileName)
        {
            // 入力項目が0件の場合エラー
            if (lstCurrentData.Count == 0)
            {
                OutPutImportLog("PLCファイル内0件", strFileName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// PTC情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInPTC(List<IniConfigPLC> lstDataPTCToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {

                    foreach (IniConfigPLC icpCurrentData in lstDataPTCToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfoPTC(icpCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesRegPTC = m_intSuccesRegPTC + 1;
                        }
                        else
                        {
                            m_intErrorRegPTC = m_intErrorRegPTC + 1;
                        }
                    }

                    // トランザクションコミット
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// PTC更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoPTC(IniConfigPLC idrCurrentData
                                                   , NpgsqlConnection NpgsqlCon
                                                   , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_PTC;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniConfigPLC);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                        { Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command, transaction) == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("PTC更新時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }
        #endregion

        #region エアバッグ領域設定取込み
        /// <summary>
        /// エアバッグ領域設定取込み取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniAirBagCoord> ImportAirBagCoordIniData(FileInfo Inputfile)
        {
            List<IniAirBagCoord> lstAirBagCoord = new List<IniAirBagCoord>();
            List<string> lstRegisterIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // 品番情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    if (strFileTextLine == " ")
                    {
                        // 空行（最終行）の場合読み飛ばす
                        continue;
                    }

                    // エアバッグ情報ファイル内のセクションを取得する
                    if (Regex.IsMatch(strFileTextLine, "[" + m_CON_INI_SECTION_AIRBAG + "[0-9][0-9][0-9]]") == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", "");
                        strFileTextLineReplace = strFileTextLine.Replace("]", "");
                        lstRegisterIniSection.Add(strFileTextLine);

                    }
                }

                // エアバッグの設定ファイルの値を格納する
                IniAirBagCoord iabCurrentData = new IniAirBagCoord();
                iabCurrentData = DataAirbagIniToDBClass(Inputfile, lstRegisterIniSection);
                lstAirBagCoord.Add(iabCurrentData);

                return lstAirBagCoord;
            }
        }

        /// <summary>
        /// エアバッグINIファイル読み込み格納処理
        /// </summary>
        /// <param name="Inputfile">読み込みINIファイル</param>
        /// <param name="strRegister">セクション</param>
        /// <returns></returns>
        private static IniAirBagCoord DataAirbagIniToDBClass(FileInfo Inputfile
                                                           , List<string> lstRegisterIniSection)
        {
            IniAirBagCoord iabCurrentData = new IniAirBagCoord();

            int intRecordCount = 0;

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniAirBagCoord);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // ファイルナンバーの格納を行う
            iabCurrentData.file_num =
                SubstringRight(System.IO.Path.GetFileNameWithoutExtension(Inputfile.Name), 2);

            // Iniファイルから各値を読み込む
            foreach (var fieldInfo in fieldInfos)
            {
                // フィールド名ごとに値の格納処理を行う
                if (fieldInfo.Name == m_CON_COL_FILE_NUM)
                {
                    // ファイル名セクション名は設定済みのためスキップ
                    continue;
                }
                else if(fieldInfo.Name == m_CON_COL_AIRBAG_NUMBER)
                {
                    // セクションの数だけループする
                    foreach (string strRegister in lstRegisterIniSection)
                    {
                        int intNumberValue = NulltoInt(GetIniValue(Inputfile.FullName
                                                                 , strRegister
                                                                 , GetName(() => fieldInfo.Name)));
                        if (intNumberValue > 0) 
                        {
                            intRecordCount = intRecordCount + 1;
                        }
                    }

                    fieldInfo.SetValue(iabCurrentData, intRecordCount);
                }
            }

            return iabCurrentData;
        }

        /// <summary>
        /// エアバッグチェック処理
        /// </summary>
        /// <param name="lstCurrentData">読み込んだPLC情報</param>
        /// <param name="strFileName">ファイル名</param>
        /// <returns></returns>
        private static Boolean CheckAirbagIniData(List<IniAirBagCoord> lstCurrentData
                                                , string strFileName)
        {
            // 入力項目が0件の場合エラー
            if (lstCurrentData.Count == 0)
            {
                OutPutImportLog("エアバッグファイル内0件", strFileName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// エアバッグテーブル登録処理
        /// </summary>
        /// <param name="IniAirBagCoord"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInAirbag(List<IniAirBagCoord> lstDataAirBagToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_ConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {

                    foreach (IniAirBagCoord iabCurrentData in lstDataAirBagToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfoAirbag(iabCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesRegAirBag = m_intSuccesRegAirBag + 1;
                        }
                        else
                        {
                            m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                        }
                    }

                    // トランザクションコミット
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// エアバッグ更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoAirbag(IniAirBagCoord idrCurrentData
                                                      , NpgsqlConnection NpgsqlCon
                                                      , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_AIRBAG;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniAirBagCoord);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                        { Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command, transaction) == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("エアバッグ更新時にエラーが発生しました。"
                               + Environment.NewLine
                               + ex.Message);
                return false;
            }
        }
        #endregion

        #region カメラ情報CSV取り込み
        /// <summary>
        /// カメラ情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<CameraCsvInfo> ImportCameraIniData(FileInfo Inputfile)
        {
            List<CameraCsvInfo> lstCameraCsvInfo = new List<CameraCsvInfo>();
            // 読み込みデータ
            CameraCsvInfo cciCurrentData = new CameraCsvInfo();

            int intRowCount = 0;
            int intErrorCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // カメラ情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    if (strFileTextLine == "")
                    {
                        // 空行（最終行）の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (ReadCsvData(strFileTextLine, intRowCount, Inputfile.Name, out cciCurrentData) == false)
                    {
                        intErrorCount = intErrorCount + 1;
                        continue;
                    }

                }

                return lstCameraCsvInfo;
            }
        }

        /// <summary>
        /// CSVファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private static Boolean ReadCsvData(string strFileTextLine
                                         , int intRowCount
                                         , string strFileName
                                         , out CameraCsvInfo cciCurrentData)
        {
            cciCurrentData = new CameraCsvInfo();

            // CSVを読み込む
            if (SetCameraInfoCsv(strFileTextLine, out cciCurrentData) == false)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目の列数が不足しています", strFileName);
                return false;
            }

            // 入力データチェックを行う
            if (InputDataCheck(cciCurrentData, intRowCount) == false)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// /入力チェック
        /// </summary>
        /// <param name="cciCheckData">読み込みユーザ情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private static Boolean InputDataCheck(CameraCsvInfo cciCheckData
                                            , int intRowCount)
        {


            return true;
        }
        /// <summary>
        /// ＣＳＶ→構造体格納（ユーザ情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        /// <returns></returns>
        private static Boolean SetCameraInfoCsv(string strFileReadLine
                                              , out CameraCsvInfo cciData)
        {
            string[] stArrayData;

            cciData = new CameraCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 列数チェック
            if (stArrayData.Length <= m_CON_COL_END_REGIMARK_CAMERA_NUM)
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            cciData.strProductName = stArrayData[m_CON_COL_PRODUCT_NAME];
            cciData.strIlluminationInformation = stArrayData[m_CON_COL_ILLUMINATION_INFORMATION];
            cciData.strStartRegimarkCameraNum = stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM];
            cciData.strEndRegimarkCameraNum = stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM];

            return true;
        }
        #endregion
        #endregion

        #region ログ出力
        /// <summary>
        /// インポートログ出力
        /// </summary>
        /// <param name="strLogText">ログのテキスト</param>
        /// <param name="strFileName">ログ対象ファイル名</param>
        private static void OutPutImportLog(string strLogText
                                          , string strFileName)
        {
            string strOutPutFilePath = "";
            string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            // 出力ファイル設定
            strOutPutFilePath = System.IO.Directory.GetCurrentDirectory() + @"\"
                                                                          + m_CON_OUTLOGFILE_NAME
                                                                          + "_"
                                                                          + DateTime.Now.ToString("yyyyMMdd")
                                                                          + ".txt";

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(strOutPutFilePath
                                                        , true
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(time + " " + strFileName + ":" + strLogText);
                }
            }
            catch
            {
                return;
            }
        }
        #endregion
    }
}
