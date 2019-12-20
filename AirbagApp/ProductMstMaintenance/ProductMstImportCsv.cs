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

        // ログファイルの出力アプリ名
        private const string m_CON_OUTLOGFILE_NAME = "HinNoImportLog";

        // 品番情報csv 取り込み時特殊対応項目
        private const string m_CON_COL_FILE_NUM = "file_num";
        private const string m_CON_COL_REG_NUM = "register_num";
        private const string m_CON_COL_RATEXUPD = "stretch_rate_x_upd";
        private const string m_CON_COL_RATEYUPD = "stretch_rate_y_upd";

        /// <summary>
        /// INIファイルのセパレータ
        /// </summary>
        private const Char m_CON_SEPARATOR_XY = ' ';
        #endregion

        #region 変数
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
        /// 品番情報CSV
        /// </summary>
        private class IniDataRegister
        {
            public string file_num { get; set; }
            public string register_num { get; set; }
            public string RegistFlg { get; set; }
            public string SelectFlg { get; set; }
            public string Name { get; set; }
            public string ParamNo { get; set; }
            public string ImageFile { get; set; }
            public string Length { get; set; }
            public string Width { get; set; }
            public string MarkerColor1 { get; set; }
            public string MarkerColor2 { get; set; }
            public string AutoPrint { get; set; }
            public string AutoStop { get; set; }
            public string TempFile1 { get; set; }
            public string regimark_1_point_x { get; set; }
            public string regimark_1_point_y { get; set; }
            public string regimark_1_size_w { get; set; }
            public string regimark_1_size_h { get; set; }
            public string TempFile2 { get; set; }
            public string regimark_2_point_x { get; set; }
            public string regimark_2_point_y { get; set; }
            public string regimark_2_size_w { get; set; }
            public string regimark_2_size_h { get; set; }
            public string base_point_1_x { get; set; }
            public string base_point_1_y { get; set; }
            public string base_point_2_x { get; set; }
            public string base_point_2_y { get; set; }
            public string base_point_3_x { get; set; }
            public string base_point_3_y { get; set; }
            public string base_point_4_x { get; set; }
            public string base_point_4_y { get; set; }
            public string base_point_5_x { get; set; }
            public string base_point_5_y { get; set; }
            public string point_1_plus_direction_x { get; set; }
            public string point_1_plus_direction_y { get; set; }
            public string point_2_plus_direction_x { get; set; }
            public string point_2_plus_direction_y { get; set; }
            public string point_3_plus_direction_x { get; set; }
            public string point_3_plus_direction_y { get; set; }
            public string point_4_plus_direction_x { get; set; }
            public string point_4_plus_direction_y { get; set; }
            public string point_5_plus_direction_x { get; set; }
            public string point_5_plus_direction_y { get; set; }
            public string AreaMagX { get; set; }
            public string AreaMagY { get; set; }
            public string stretch_rate_x_upd { get; set; }
            public string stretch_rate_y_upd { get; set; }
            public string TempFile3 { get; set; }
            public string TempFile4 { get; set; }
            public string AutoCalcAreaMagFlg { get; set; }
            public string AreaMagCoefficient { get; set; }
            public string AreaMagCorrection { get; set; }
            public string BThreadNum { get; set; }
            public string BThreadNo { get; set; }
            public string BTCamNo { get; set; }
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
                        }

                        // PLC設定ファイルを判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_CONFIG_PLC + ".INI") == true)
                        {
                            // PLC設定より、レジマーク間距離を取得し登録する。

                        }

                        // エアバック領域設定ファイルを判定する
                        if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_AIRBAG_COORD + "[0-9][0-9]*.INI") == true)
                        {
                            // エアバック領域設定より、列数を取得し登録する。

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
                    // 商品テキストファイルを１行読み込む
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
        /// 指定されたファイルがロックされているかどうかを返します。
        /// </summary>
        /// <param name="path">検証したいファイルへのフルパス</param>
        /// <returns>ロックされているかどうか</returns>
        private static Boolean IsFileLocked(string path)
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
                // 指定したキーが存在する場合、値を取得する
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
                    fieldInfo.SetValue(idrCurrentData, strTenmIniValue.Split(m_CON_SEPARATOR_XY)[intUniSplitVal]);
                }
                else
                {
                    // それ以外の項目は普通に設定
                    fieldInfo.SetValue(idrCurrentData, GetIniValue(Inputfile.FullName
                                                     , strRegister
                                                     , GetName(() => fieldInfo.Name)));
                }
            }

            return idrCurrentData;
        }
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
