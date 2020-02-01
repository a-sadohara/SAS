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

        // INIファイルのセクション
        private const string m_CON_INI_SECTION_REGISTER = "REGISTER";
        private const string m_CON_INI_SECTION_KIND = "KIND";
        private const string m_CON_INI_SECTION_AIRBAG = "AIRBAG";
        private const string m_CON_INI_SECTION_AIRBAG_POINT_A = "000";
        private const string m_CON_INI_SECTION_AIRBAG_POINT_B = "001";
        private const string m_CON_INI_SECTION_AIRBAG_POINT_C = "002";
        private const string m_CON_INI_SECTION_AIRBAG_POINT_D = "003";
        private const string m_CON_INI_SECTION_AIRBAG_POINT_E = "004";

        // ログファイルの出力アプリ名
        private const string m_CON_OUTLOGFILE_NAME = "品番情報取り込みログ";

        // 品番情報INI 取り込み時特殊対応項目
        private const string m_CON_COL_FILE_NUM = "file_num";
        private const string m_CON_COL_REG_NUM = "register_num";
        private const string m_CON_COL_IMAGE_FILE = "ImageFile";

        // PTCINI 取り込み時特殊対応項目
        private const string m_CON_COL_PLC_KIND = "KIND";

        // エアバッグINI 取り込み時特殊対応項目
        private const string m_CON_COL_AIRBAG_NUMBER = "Number";
        private const string m_CON_COL_AIRBAG_COORD = "Coord";

        // カメラCSVファイル配置情報
        private const int m_CON_COL_PRODUCT_NAME = 0;
        private const int m_CON_COL_ILLUMINATION_INFORMATION = 1;
        private const int m_CON_COL_START_REGIMARK_CAMERA_NUM = 2;
        private const int m_CON_COL_END_REGIMARK_CAMERA_NUM = 3;

        // 閾値CSVファイル配置情報
        private const int m_CON_COL_PRODUCT_NAME_THRESHOLD = 0;
        private const int m_CON_COL_TAKING_CAMERA_CNT = 1;
        private const int m_CON_COL_COLUMN_THRESHOLD_01 = 2;
        private const int m_CON_COL_COLUMN_THRESHOLD_02 = 3;
        private const int m_CON_COL_COLUMN_THRESHOLD_03 = 4;
        private const int m_CON_COL_COLUMN_THRESHOLD_04 = 5;
        private const int m_CON_COL_LINE_THRESHOLD_A1 = 6;
        private const int m_CON_COL_LINE_THRESHOLD_A2 = 7;
        private const int m_CON_COL_LINE_THRESHOLD_B1 = 8;
        private const int m_CON_COL_LINE_THRESHOLD_B2 = 9;
        private const int m_CON_COL_LINE_THRESHOLD_C1 = 10;
        private const int m_CON_COL_LINE_THRESHOLD_C2 = 11;
        private const int m_CON_COL_LINE_THRESHOLD_D1 = 12;
        private const int m_CON_COL_LINE_THRESHOLD_D2 = 13;
        private const int m_CON_COL_LINE_THRESHOLD_E1 = 14;
        private const int m_CON_COL_LINE_THRESHOLD_E2 = 15;
        private const int m_CON_COL_AI_MODEL_NAME = 16;

        // 判定理由CSVファイル配置情報
        private const int m_CON_COL_REASON_CODE = 0;
        private const int m_CON_COL_DECISION_REASON = 1;

        // INIファイルのセパレータ
        private const Char m_CON_SEPARATOR_XY = ' ';
        private const Char m_CON_SEPARATOR_AIRBAG_COORD = ' ';

        // ログファイル出力先を設定
        private const string m_CON_MASTER_IMAGE = "MasterImageDirectory";
        private const string m_CON_LOG_OUT_DIRECTORY = "LogFileOutputDirectory";
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

        // カメラ情報登録の成功・失敗件数
        private static int m_intSuccesCameraReg = 0;
        private static int m_intErrorCameraReg = 0;

        // 閾値情報登録の成功・失敗件数
        private static int m_intSuccesThresholdReg = 0;
        private static int m_intErrorThresholdReg = 0;

        // マスタ画像情報登録の成功・失敗件数
        private static int m_intSuccesMasterImg = 0;
        private static int m_intErrorMasterImg = 0;

        // 判定理由情報登録の成功・失敗件数
        private static int m_intSuccesDecisionReasonReg = 0;
        private static int m_intErrorDecisionReasonReg = 0;

        // ログフォルダパス
        private static string m_strOutPutLogFolder = "";

        // エラー出力用ファイル名
        private static string m_strErrorOutFileName = "";

        // マスタ画像出力先フォルダ
        private static string m_strOutMstFolder = "";
        private static string m_strCheckMstFile = "";

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
        private struct IniDataRegister
        {
            public string file_num;
            public int RegistFlg;
            public int SelectFlg;
            public string Name;
            public int ParamNo;
            public string ImageFile;
            public int Length;
            public int Width;
            public int MarkerColor1;
            public int MarkerColor2;
            public int AutoPrint;
            public int AutoStop;
            public string TempFile1;
            public int regimark_1_point_x;
            public int regimark_1_point_y;
            public int regimark_1_size_w;
            public int regimark_1_size_h;
            public string TempFile2;
            public int regimark_2_point_x;
            public int regimark_2_point_y;
            public int regimark_2_size_w;
            public int regimark_2_size_h;
            public int base_point_1_x;
            public int base_point_1_y;
            public int base_point_2_x;
            public int base_point_2_y;
            public int base_point_3_x;
            public int base_point_3_y;
            public int base_point_4_x;
            public int base_point_4_y;
            public int base_point_5_x;
            public int base_point_5_y;
            public int point_1_plus_direction_x;
            public int point_1_plus_direction_y;
            public int point_2_plus_direction_x;
            public int point_2_plus_direction_y;
            public int point_3_plus_direction_x;
            public int point_3_plus_direction_y;
            public int point_4_plus_direction_x;
            public int point_4_plus_direction_y;
            public int point_5_plus_direction_x;
            public int point_5_plus_direction_y;
            public int AreaMagX;
            public int AreaMagY;
            public string TempFile3;
            public string TempFile4;
            public int AutoCalcAreaMagFlg;
            public int AreaMagCoefficient;
            public int AreaMagCorrection;
            public int BThreadNum;
            public string BThreadNo;
            public int BTCamNo;
        }

        /// <summary>
        /// PLCINI格納構造体
        /// </summary>
        private struct IniConfigPLC 
        {
            public string KIND;
            public int LINE_LENGTH;
            public int MARK_INTERVAL;
        }

        /// <summary>
        /// エアバッグ領域設定INI構造体
        /// </summary>
        private struct IniAirBagCoord 
        {
            public string file_num;
            public int Number;
            public string strTopPointA;
            public string strTopPointB;
            public string strTopPointC;
            public string strTopPointD;
            public string strTopPointE;
        }

        /// <summary>
        /// カメラ情報CSVファイル
        /// </summary>
        private struct CameraCsvInfo
        {
            public string strProductName;
            public int intIlluminationInformation;
            public int intStartRegimarkCameraNum;
            public int intEndRegimarkCameraNum;
        }

        /// <summary>
        /// 閾値情報CSVファイル
        /// </summary>
        private struct ThresholdCsvInfo
        {
            public string strProductName;
            public int intTakingCameraCnt;
            public int intColumnThreshold01;
            public int intColumnThreshold02;
            public int intColumnThreshold03;
            public int intColumnThreshold04;
            public int intLineThresholda1;
            public int intLineThresholda2;
            public int intLineThresholdb1;
            public int intLineThresholdb2;
            public int intLineThresholdc1;
            public int intLineThresholdc2;
            public int intLineThresholdd1;
            public int intLineThresholdd2;
            public int intLineThresholde1;
            public int intLineThresholde2;
            public string strAiModelName;
        }

        /// <summary>
        /// 判定理由情報CSVファイル
        /// </summary>
        private struct DecisionReasonCsvInfo
        {
            public int intReasonCode;
            public string strDecisionReason;
        }
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public ProductMstImportCsv()
        {
            InitializeComponent();

            // 変数初期化
            m_intSuccesRegProductInfo = 0;
            m_intErrorRegProductInfo = 0;
            m_intSuccesRegPTC = 0;
            m_intErrorRegPTC = 0;
            m_intSuccesRegAirBag = 0;
            m_intErrorRegAirBag = 0;
            m_intSuccesCameraReg = 0;
            m_intErrorCameraReg = 0;
            m_intSuccesThresholdReg = 0;
            m_intErrorThresholdReg = 0;
            m_intSuccesMasterImg = 0;
            m_intErrorMasterImg = 0;
            m_intSuccesDecisionReasonReg = 0;
            m_intErrorDecisionReasonReg = 0;
            m_strOutPutLogFolder = "";
            m_strErrorOutFileName = "";
            m_strOutMstFolder = "";
            m_strCheckMstFile = "";

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// CSV取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {

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
                // 出力先ログフォルダパスを取得する
                if (bolGetSystemSettingValue(m_CON_LOG_OUT_DIRECTORY
                                           , out m_strOutPutLogFolder) == true)
                {
                    // 対象フォルダなし
                    if (Directory.Exists(m_strOutPutLogFolder) == false)
                    {
                        // フォルダ作成する
                        Directory.CreateDirectory(m_strOutPutLogFolder);
                    }
                }
                else 
                {
                    m_strOutPutLogFolder = System.IO.Directory.GetCurrentDirectory();
                }

                DirectoryInfo directorySearchFolder = new DirectoryInfo(txtFolder.Text);

                FileInfo[] fiInputIni = directorySearchFolder.GetFiles("*.ini"
                                                                     , SearchOption.TopDirectoryOnly);
                FileInfo[] fiInputCsv = directorySearchFolder.GetFiles("*.csv"
                                                                     , SearchOption.TopDirectoryOnly);
                FileInfo[] fiInputPng = directorySearchFolder.GetFiles("*.bmp"
                                                                     , SearchOption.TopDirectoryOnly);

                // マスタ画像取り込み
                ProcessMasterPng(fiInputPng);

                // 品番マスタ情報取り込み
                ProcessRegisterIni(fiInputIni);

                // PLCマスタ情報取り込み
                ProcessPLCIni(fiInputIni);

                // エアバッグ情報取り込み
                ProcessAirBagIni(fiInputIni);

                // カメラ情報CSV取り込み
                ProcessCameraCsv(fiInputCsv);

                // 閾値情報CSV取り込み
                ProcessThresholdCsv(fiInputCsv);

                // 判定理由取り込み
                ProcessDecisionReasonCsv(fiInputCsv);

                // 出力ファイル設定
                string strOutPutFilePath = m_strOutPutLogFolder + @"\" 
                                                                + m_CON_OUTLOGFILE_NAME
                                                                + ".csv";

                // 出力ダイアログメッセージを設定する
                string strMsg = "取り込み処理が終了しました。" + Environment.NewLine + 
                 "　品番登録　取り込み件数：" + (m_intSuccesRegProductInfo + m_intErrorRegProductInfo) + "件　正常：" + m_intSuccesRegProductInfo + "件　異常：" + m_intErrorRegProductInfo + "件 " + Environment.NewLine +
                 "　PLC設定　取り込み件数：" + (m_intSuccesRegPTC + m_intErrorRegPTC) + "件　正常：" + m_intSuccesRegPTC + "件　異常：" + m_intErrorRegPTC + "件 " + Environment.NewLine +
                 "　エアバッグ領域設定　取り込み件数：" + (m_intSuccesRegAirBag + m_intErrorRegAirBag) + "件　正常：" + m_intSuccesRegAirBag + "件　異常：" + m_intErrorRegAirBag + "件 " + Environment.NewLine +
                 "　カメラ情報　取り込み件数：" + (m_intSuccesCameraReg + m_intErrorCameraReg) + "件　正常：" + m_intSuccesCameraReg + "件　異常：" + m_intErrorCameraReg + "件 " + Environment.NewLine +
                 "　閾値情報　取り込み件数：" + (m_intSuccesThresholdReg + m_intErrorThresholdReg) + "件　正常：" + m_intSuccesThresholdReg + "件　異常：" + m_intErrorThresholdReg + "件 " + Environment.NewLine +
                 "　マスタ画像　取り込み件数：" + (m_intSuccesMasterImg + m_intErrorMasterImg) + "件　正常：" + m_intSuccesMasterImg + "件　異常：" + m_intErrorMasterImg + "件 " + Environment.NewLine +
                 "　判定理由マスタ　取り込み件数：" + (m_intSuccesDecisionReasonReg + m_intErrorDecisionReasonReg) + "件　正常：" + m_intSuccesDecisionReasonReg + "件　異常：" + m_intErrorDecisionReasonReg + "件 " + Environment.NewLine +
                 "詳細は、下記のログファイルを参照ください。" + Environment.NewLine +
                 "　" + strOutPutFilePath;

                // エラーが一つでもある場合は警告表示する
                if (m_intErrorRegProductInfo > 0 ||
                    m_intErrorRegPTC > 0 ||
                    m_intErrorRegAirBag > 0 ||
                    m_intErrorCameraReg > 0 ||
                    m_intErrorThresholdReg > 0 ||
                    m_intErrorMasterImg > 0 ||
                    m_intErrorDecisionReasonReg > 0)
                {
                    MessageBox.Show(strMsg, "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else 
                {
                    MessageBox.Show(strMsg, "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
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
        /// <param name="fiInputIni">読み込みINIファイル全種類</param>
        private static void ProcessRegisterIni(FileInfo[] fiInputIni)
        {
            List<IniDataRegister> lstDataRegistersToDB = new List<IniDataRegister>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputIni)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                    continue;
                }

                // 品番情報ファイルを判定する
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_REGISTER_INI_DATA + "[0-9][0-9]*.ini") == true)
                {
                    try
                    {
                        // 品番情報ファイルの場合、取り込みを行う
                        lstDataRegistersToDB = ImportRegisterIniData(Inputfile);
                    }
                    catch (Exception ex) 
                    {
                        // ログファイルにエラー出力を行う
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込んだ値に対してチェック処理を行う
                    if (CheckRegisterIniData(lstDataRegistersToDB
                                           , Inputfile.Name) == false)
                    {
                        m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                        continue;
                    }

                    // 読み込んだ値をDBに登録する
                    InsertMstProductInfo(lstDataRegistersToDB);

                    // 登録成功している場合、マスタ画像が存在しているか確認する
                    if (CheckMstImage() == false)
                    {
                        m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                        continue;
                    }
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "品番登録　取り込み件数：" + (m_intSuccesRegProductInfo + m_intErrorRegProductInfo) + "件　"
                             + "正常：" + m_intSuccesRegProductInfo + "件　"
                             + "異常：" + m_intErrorRegProductInfo + "件 ";
            OutPutImportLog(strOutMsg);
        }

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
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", "");
                        lstRegisterIniSection.Add(strFileTextLineReplace);

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
                OutPutImportLog("品番ファイルのデータが存在しません");
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

            string strTenmIniValue = "";

            // 一旦構造体インスタンスをボックス化する(SetValueはobject型じゃないとできない)
            object boxed = idrCurrentData;

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
                else if (fieldInfo.Name == m_CON_COL_IMAGE_FILE)
                {
                    m_strCheckMstFile = "";
                    // 特殊対応項目の場合は特殊対応して設定
                    strTenmIniValue = NulltoString(GetIniValue(Inputfile.FullName
                                                             , strRegister
                                                             , fieldInfo.Name));

                    string strFileName = Path.GetFileName(strTenmIniValue);
                    m_strCheckMstFile = m_strOutMstFolder + @"\" + strFileName;
                    fieldInfo.SetValue(boxed, m_strCheckMstFile);

                }
                else if (strUniKey is null == false)
                {
                    // 特殊対応項目の場合は特殊対応して設定
                    strTenmIniValue = NulltoString(GetIniValue(Inputfile.FullName
                                                             , strRegister
                                                             , strUniKey));
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        fieldInfo.SetValue(boxed
                                         , NulltoInt(strTenmIniValue.Split(m_CON_SEPARATOR_XY)[intUniSplitVal]));
                    }
                    else
                    {
                        fieldInfo.SetValue(boxed
                                         , NulltoString(strTenmIniValue.Split(m_CON_SEPARATOR_XY)[intUniSplitVal]));
                    }
                }
                else
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(boxed
                                         , NulltoInt(GetIniValue(Inputfile.FullName
                                                               , strRegister
                                                               , fieldInfo.Name)));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(boxed
                                         , NulltoString(GetIniValue(Inputfile.FullName
                                                                  , strRegister
                                                                  , fieldInfo.Name)));
                    }
                }
            }

            // 型の復元
            idrCurrentData = (IniDataRegister)boxed;

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
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
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

                    if (m_intSuccesRegProductInfo > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
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
                if (ExecTranSQL(command
                              , transaction
                              , "品番登録情報テーブルの更新に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("品番情報登録時にエラーが発生しました " + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "品番情報登録時にエラーが発生しました " + ex.Message);
                return false;
            }
        }
        #endregion

        #region PLC取り込み
        /// <summary>
        /// PLC情報取り込み
        /// </summary>
        /// <param name="fiInputIni">読み込みINIファイル全種類</param>
        private static void ProcessPLCIni(FileInfo[] fiInputIni)
        {
            List<IniConfigPLC> lstPLCDataToDB = new List<IniConfigPLC>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputIni)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorRegPTC = m_intErrorRegPTC + 1;
                    continue;
                }

                // PLC設定ファイルを判定する
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_CONFIG_PLC + ".ini") == true)
                {
                    try
                    {
                        // PLC設定より、レジマーク間距離を取得し登録する。
                        lstPLCDataToDB = ImportPLCIniData(Inputfile);
                    }
                    catch (Exception ex) 
                    {
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorRegPTC = m_intErrorRegPTC + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込んだ値に対してチェック処理を行う
                    if (CheckPLCIniData(lstPLCDataToDB
                                      , Inputfile.Name) == false)
                    {
                        m_intErrorRegPTC = m_intErrorRegPTC + 1;
                        continue;
                    }

                    // 読み込んだ値をDBに登録する
                    UPDMstProductInfoInPTC(lstPLCDataToDB);
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "PLC設定　取り込み件数：" + (m_intSuccesRegPTC + m_intErrorRegPTC) + "件　"
                             + "正常：" + m_intSuccesRegPTC + "件　"
                             + "異常：" + m_intErrorRegPTC + "件 ";
            OutPutImportLog(strOutMsg);
        }

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
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", "");
                        lstPLCIniSection.Add(strFileTextLineReplace);
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
            Type typeOfMyStruct = typeof(IniConfigPLC);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // セクションナンバーの格納を行う
            int intSection = NulltoInt(SubstringRight(strRegister, 2)) - 1;
            icpCurrentData.KIND = String.Format("{0:D2}", intSection);

            // 一旦構造体インスタンスをボックス化する(SetValueはobject型じゃないとできない)
            object boxed = icpCurrentData;

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
                        fieldInfo.SetValue(boxed
                                         , NulltoInt(GetIniValue(Inputfile.FullName
                                                               , strRegister
                                                               , fieldInfo.Name)));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(boxed
                                         , NulltoString(GetIniValue(Inputfile.FullName
                                                                  , strRegister
                                                                  , fieldInfo.Name)));
                    }
                }
            }

            icpCurrentData = (IniConfigPLC)boxed;

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
                OutPutImportLog("PLCファイルのデータが存在しません");
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
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
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

                    if (m_intSuccesRegPTC > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
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
                if (ExecTranSQL(command
                              , transaction
                              , "品番登録情報テーブルの更新に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("PTC更新時にエラーが発生しました。" + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "PTC更新時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }
        #endregion

        #region エアバッグ領域設定取込み
        /// <summary>
        /// エアバッグ情報取り込み
        /// </summary>
        /// <param name="fiInputIni">読み込みINIファイル全種類</param>
        private static void ProcessAirBagIni(FileInfo[] fiInputIni)
        {
            List<IniAirBagCoord> lstAirBagCoordToDB = new List<IniAirBagCoord>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputIni)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                    continue;
                }

                // エアバック領域設定ファイルを判定する
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_AIRBAG_COORD + "[0-9][0-9]*.ini") == true)
                {
                    try
                    {
                        // エアバック領域設定より、列数を取得し登録する。
                        lstAirBagCoordToDB = ImportAirBagCoordIniData(Inputfile);
                    }
                    catch (Exception ex)
                    {
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込んだ値に対してチェック処理を行う
                    if (CheckAirbagIniData(lstAirBagCoordToDB
                                         , Inputfile.Name) == false)
                    {
                        m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                        continue;
                    }

                    // 読み込んだ値をDBに登録する
                    UPDMstProductInfoInAirbag(lstAirBagCoordToDB);
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "エアバッグ領域設定　取り込み件数：" + (m_intSuccesRegAirBag + m_intErrorRegAirBag) + "件　"
                             + "正常：" + m_intSuccesRegAirBag + "件　"
                             + "異常：" + m_intErrorRegAirBag + "件 ";
            OutPutImportLog(strOutMsg);
        }

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
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", "");
                        lstRegisterIniSection.Add(strFileTextLineReplace);

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

            string strTopPointATran = "";
            string strTopPointBTran = "";
            string strTopPointCTran = "";
            string strTopPointDTran = "";
            string strTopPointETran = "";

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
                else if (fieldInfo.Name == m_CON_COL_AIRBAG_NUMBER)
                {
                    // セクションの数だけループする
                    foreach (string strRegister in lstRegisterIniSection)
                    {
                        int intNumberValue = NulltoInt(GetIniValue(Inputfile.FullName
                                                                 , strRegister
                                                                 , fieldInfo.Name));
                        if (intNumberValue > 0)
                        {
                            intRecordCount = intRecordCount + 1;

                            string strTopPoint = "";
                            // 頂点座標の設定を行う
                            for (int i = 0; i < intNumberValue; i++)
                            {
                                string strPointValue = GetIniValue(Inputfile.FullName
                                                                 , strRegister
                                                                 , m_CON_COL_AIRBAG_COORD + String.Format("{0:00}", i));

                                if (strTopPoint != "")
                                {
                                    strTopPoint = strTopPoint + ",";
                                }

                                if (strPointValue == "")
                                {
                                    strTopPoint = strTopPoint + "(,)";
                                }
                                else
                                {
                                    string[] strPointValueSplit = strPointValue.Split(m_CON_SEPARATOR_AIRBAG_COORD);
                                    if (strPointValueSplit.Length == 1)
                                    {
                                        strTopPoint = strTopPoint + "(" + strPointValueSplit[1] + ",)";
                                    }
                                    else
                                    {
                                        strTopPoint = strTopPoint + "(" + strPointValueSplit[0] + "," + strPointValueSplit[1] + ")";
                                    }

                                }
                            }

                            // セクションによって頂点座標の値を設定する
                            switch (strRegister)
                            {
                                case m_CON_INI_SECTION_AIRBAG + m_CON_INI_SECTION_AIRBAG_POINT_A:
                                    strTopPointATran = strTopPoint;
                                    break;
                                case m_CON_INI_SECTION_AIRBAG + m_CON_INI_SECTION_AIRBAG_POINT_B:
                                    strTopPointBTran = strTopPoint;
                                    break;
                                case m_CON_INI_SECTION_AIRBAG + m_CON_INI_SECTION_AIRBAG_POINT_C:
                                    strTopPointCTran = strTopPoint;
                                    break;
                                case m_CON_INI_SECTION_AIRBAG + m_CON_INI_SECTION_AIRBAG_POINT_D:
                                    strTopPointDTran = strTopPoint;
                                    break;
                                case m_CON_INI_SECTION_AIRBAG + m_CON_INI_SECTION_AIRBAG_POINT_E:
                                    strTopPointETran = strTopPoint;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    // 構造体に値を格納する
                    iabCurrentData.Number = intRecordCount;
                    iabCurrentData.strTopPointA = strTopPointATran;
                    iabCurrentData.strTopPointB = strTopPointBTran;
                    iabCurrentData.strTopPointC = strTopPointCTran;
                    iabCurrentData.strTopPointD = strTopPointDTran;
                    iabCurrentData.strTopPointE = strTopPointETran;
                }
                else 
                {
                    continue;
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
                OutPutImportLog("エアバッグファイルのデータが存在しません");
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
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
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

                    if (m_intSuccesRegAirBag > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
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
                if (ExecTranSQL(command
                              , transaction
                              , "品番登録情報テーブルの更新に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("エアバッグ更新時にエラーが発生しました。" + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "エアバッグ更新時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }
        #endregion

        #region カメラ情報CSV取り込み
        /// <summary>
        /// カメラ情報取り込み
        /// </summary>
        /// <param name="fiInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessCameraCsv(FileInfo[] fiInputCsv)
        {
            List<CameraCsvInfo> lstCamCsvInfo = new List<CameraCsvInfo>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputCsv)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorCameraReg = m_intErrorCameraReg + 1;
                    continue;
                }

                // カメラ情報を判定する
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_CAMERA_INFO + ".csv") == true)
                {
                    try
                    {
                        // CSVファイルを取り込み、カメラ情報を取得し登録する。
                        lstCamCsvInfo = ImportCameraCsvData(Inputfile);
                    }
                    catch (Exception ex)
                    {
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorCameraReg = m_intErrorCameraReg + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込み行が存在する場合は登録を行う
                    if (lstCamCsvInfo.Count > 0)
                    {
                        // 読み込んだ値をDBに登録する
                        UPDMstProductInfoInCamera(lstCamCsvInfo);
                    }
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "カメラ情報　取り込み件数：" + (m_intSuccesCameraReg + m_intErrorCameraReg) + "件　"
                             + "正常：" + m_intSuccesCameraReg + "件　"
                             + "異常：" + m_intErrorCameraReg + "件 ";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// カメラ情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<CameraCsvInfo> ImportCameraCsvData(FileInfo Inputfile)
        {
            List<CameraCsvInfo> lstCameraCsvInfo = new List<CameraCsvInfo>();
            // 読み込みデータ
            CameraCsvInfo cciCurrentData = new CameraCsvInfo();

            int intRowCount = 0;

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
                    intRowCount = intRowCount + 1;
                    if (strFileTextLine == "" || intRowCount == 1)
                    {
                        // 空行（最終行）またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (ReadCameraCsvData(strFileTextLine
                                        , intRowCount
                                        , Inputfile.Name
                                        , out cciCurrentData) == false)
                    {
                        m_intErrorCameraReg = m_intErrorCameraReg + 1;
                        continue;
                    }
                    else 
                    {
                        // csvのリストに現在行を追加
                        lstCameraCsvInfo.Add(cciCurrentData);
                    }
                }

                return lstCameraCsvInfo;
            }
        }

        /// <summary>
        /// カメラCSVファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private static Boolean ReadCameraCsvData(string strFileTextLine
                                               , int intRowCount
                                               , string strFileName
                                               , out CameraCsvInfo cciCurrentData)
        {
            cciCurrentData = new CameraCsvInfo();

            // データチェック＆CSVを読み込む
            if (SetCameraInfoCsv(strFileTextLine, out cciCurrentData, intRowCount) == false)
            { 
                return false;
            }

            return true;
        }

        /// <summary>
        /// ＣＳＶ→構造体格納（カメラ情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        /// <returns></returns>
        private static Boolean SetCameraInfoCsv(string strFileReadLine
                                              , out CameraCsvInfo cciData
                                              , int intRowCount)
        {
            string[] stArrayData;

            cciData = new CameraCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            if (InputDataCheckCamera(stArrayData, intRowCount, strFileReadLine) == false) 
            {
                return false;
            }

            // データに問題がない場合、CSVの各項目を構造体へ格納する
            cciData.strProductName = stArrayData[m_CON_COL_PRODUCT_NAME];
            cciData.intIlluminationInformation = NulltoInt(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION]);
            cciData.intStartRegimarkCameraNum = NulltoInt(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM]);
            cciData.intEndRegimarkCameraNum = NulltoInt(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM]);

            return true;
        }

        /// <summary>
        /// カメラ入力チェック
        /// </summary>
        /// <param name="cciCheckData">読み込みユーザ情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private static Boolean InputDataCheckCamera(string[] stArrayData
                                                  , int intRowCount
                                                  , string strFileReadLine)
        {
            // 各項目のチェックを行う
            // 列数チェック
            if (stArrayData.Length <= m_CON_COL_END_REGIMARK_CAMERA_NUM)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目のファイルレイアウトが不正です。" + strFileReadLine);
                return false;
            }

            // 文字後列項目
            // 必須入力チェック
            if (CheckRequiredInput(stArrayData[m_CON_COL_PRODUCT_NAME], "品名", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION], "照度情報", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM], "開始レジマークカメラ番号", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM], "終了レジマークカメラ番号", intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // 桁数入力チェック
            if (CheckLengthInput(stArrayData[m_CON_COL_PRODUCT_NAME], "品名", intRowCount, 1, 16, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION], "照度情報", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM], "開始レジマークカメラ番号", intRowCount, 1, 2, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM], "終了レジマークカメラ番号", intRowCount, 1, 2, strFileReadLine) == false)
            {
                return false;
            }

            // 最大範囲入力チェック
            if (CheckRangeInput(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION], "照度情報", intRowCount, 0, 255, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM], "開始レジマークカメラ番号", intRowCount, 1, 26, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM], "終了レジマークカメラ番号", intRowCount, 1, 26, strFileReadLine) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// カメラ情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInCamera(List<CameraCsvInfo> lstCameraCsvToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {

                    foreach (CameraCsvInfo cciCurrentData in lstCameraCsvToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfoCamera(cciCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesCameraReg = m_intSuccesCameraReg + 1;
                        }
                        else
                        {
                            m_intErrorCameraReg = m_intErrorCameraReg + 1;
                        }
                    }

                    if (m_intSuccesCameraReg > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// カメラ更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoCamera(CameraCsvInfo cciCurrentData
                                                      , NpgsqlConnection NpgsqlCon
                                                      , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_CAMERA;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(CameraCsvInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                        { Value = NulltoInt(fieldInfo.GetValue(cciCurrentData)) });
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(cciCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command
                              , transaction
                              , "品番登録情報テーブルの更新に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, "カメラ情報更新時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }
        #endregion

        #region 閾値情報CSV取り込み
        /// <summary>
        /// 閾値情報取り込み
        /// </summary>
        /// <param name="fiInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessThresholdCsv(FileInfo[] fiInputCsv)
        {
            List<ThresholdCsvInfo> lstThresholdCsvInfo = new List<ThresholdCsvInfo>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputCsv)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorThresholdReg = m_intErrorThresholdReg + 1;
                    continue;
                }

                // 閾値情報を判定する。
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_THRESHOLD_INFO + ".csv") == true)
                {
                    try
                    {
                        // CSVファイルを取り込み、閾値情報を取得し登録する。
                        lstThresholdCsvInfo = ImportThresholdCsvData(Inputfile);
                    }
                    catch (Exception ex) 
                    {
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorThresholdReg = m_intErrorThresholdReg + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込み行が存在する場合は登録を行う
                    if (lstThresholdCsvInfo.Count > 0)
                    {
                        // 読み込んだ値をDBに登録する
                        UPDMstProductInfoInThreshold(lstThresholdCsvInfo);
                    }
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "閾値情報　取り込み件数：" + (m_intSuccesThresholdReg + m_intErrorThresholdReg) + "件　"
                             + "正常：" + m_intSuccesThresholdReg + "件　"
                             + "異常：" + m_intErrorThresholdReg + "件 ";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// 閾値情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<ThresholdCsvInfo> ImportThresholdCsvData(FileInfo Inputfile)
        {
            List<ThresholdCsvInfo> lstThresholdCsvInfo = new List<ThresholdCsvInfo>();
            // 読み込みデータ
            ThresholdCsvInfo tciCurrentData = new ThresholdCsvInfo();

            int intRowCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // 閾値情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    intRowCount = intRowCount + 1;
                    if (strFileTextLine == "" || intRowCount == 1)
                    {
                        // 空行（最終行）またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (ReadThresholdCsvData(strFileTextLine
                                           , intRowCount
                                           , Inputfile.Name
                                           , out tciCurrentData) == false)
                    {
                        m_intErrorThresholdReg = m_intErrorThresholdReg + 1;
                        continue;
                    }
                    else
                    {
                        // csvのリストに現在行を追加
                        lstThresholdCsvInfo.Add(tciCurrentData);
                    }
                }

                return lstThresholdCsvInfo;
            }
        }

        /// <summary>
        /// 閾値CSVファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private static Boolean ReadThresholdCsvData(string strFileTextLine
                                                  , int intRowCount
                                                  , string strFileName
                                                  , out ThresholdCsvInfo tciCurrentData)
        {
            tciCurrentData = new ThresholdCsvInfo();

            // CSVを読み込む
            if (SetThresholdInfoCsv(strFileTextLine, out tciCurrentData, intRowCount) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 閾値入力チェック
        /// </summary>
        /// <param name="tciCheckData">読み込み閾値情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private static Boolean InputDataCheckThreshold(string[] stArrayData
                                                     , int intRowCount
                                                     , string strFileReadLine)
        {
            // 各項目のチェックを行う
            // 列数チェック
            if (stArrayData.Length <= m_CON_COL_AI_MODEL_NAME)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目のファイルレイアウトが不正です。" + strFileReadLine);
                return false;
            }

            // 必須入力チェック
            if (CheckRequiredInput(stArrayData[m_CON_COL_PRODUCT_NAME_THRESHOLD], "品名", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_TAKING_CAMERA_CNT], "撮像カメラ数", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_01], "列閾値01", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A1], "行閾値A1", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A2], "行閾値A2", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_AI_MODEL_NAME], "AIモデル名", intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // 桁数入力チェック
            if (CheckLengthInput(stArrayData[m_CON_COL_PRODUCT_NAME_THRESHOLD], "品名", intRowCount, 1, 16, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_TAKING_CAMERA_CNT], "撮像カメラ数", intRowCount, 1, 2, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_01], "列閾値01", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_02], "列閾値02", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_03], "列閾値03", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_04], "列閾値04", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A1], "行閾値A1", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A2], "行閾値A2", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_B1], "行閾値B1", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_B2], "行閾値B2", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_C1], "行閾値C1", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_C2], "行閾値C2", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_D1], "行閾値D1", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_D2], "行閾値D2", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_E1], "行閾値E1", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_LINE_THRESHOLD_E2], "行閾値E2", intRowCount, 1, 3, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_AI_MODEL_NAME], "AIモデル名", intRowCount, 1, 256, strFileReadLine) == false)
            {
                return false;
            }

            // 最大範囲入力チェック
            if (CheckRangeInput(stArrayData[m_CON_COL_TAKING_CAMERA_CNT], "撮像カメラ数", intRowCount, 1, 54, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_01], "列閾値01", intRowCount, 1, 640, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_02], "列閾値02", intRowCount, 1, 640, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_03], "列閾値03", intRowCount, 1, 640, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_COLUMN_THRESHOLD_04], "列閾値04", intRowCount, 1, 640, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A1], "行閾値A1", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_A2], "行閾値A2", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_B1], "行閾値B1", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_B2], "行閾値B2", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_C1], "行閾値C1", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_C2], "行閾値C2", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_D1], "行閾値D1", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_D2], "行閾値D2", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_E1], "行閾値E1", intRowCount, 1, 740, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_LINE_THRESHOLD_E2], "行閾値E2", intRowCount, 1, 740, strFileReadLine) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ＣＳＶ→構造体格納（閾値情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        /// <returns></returns>
        private static Boolean SetThresholdInfoCsv(string strFileReadLine
                                                 , out ThresholdCsvInfo tciData
                                                 , int intRowCount)
        {
            string[] stArrayData;

            tciData = new ThresholdCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 入力チェック
            if (InputDataCheckThreshold(stArrayData, intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            tciData.strProductName = stArrayData[m_CON_COL_PRODUCT_NAME_THRESHOLD];
            tciData.intTakingCameraCnt = NulltoInt(stArrayData[m_CON_COL_TAKING_CAMERA_CNT]);
            tciData.intColumnThreshold01 = NulltoInt(stArrayData[m_CON_COL_COLUMN_THRESHOLD_01]);
            tciData.intColumnThreshold02 = NulltoInt(stArrayData[m_CON_COL_COLUMN_THRESHOLD_02]);
            tciData.intColumnThreshold03 = NulltoInt(stArrayData[m_CON_COL_COLUMN_THRESHOLD_03]);
            tciData.intColumnThreshold04 = NulltoInt(stArrayData[m_CON_COL_COLUMN_THRESHOLD_04]);
            tciData.intLineThresholda1 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_A1]);
            tciData.intLineThresholda2 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_A2]);
            tciData.intLineThresholdb1 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_B1]);
            tciData.intLineThresholdb2 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_B2]);
            tciData.intLineThresholdc1 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_C1]);
            tciData.intLineThresholdc2 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_C2]);
            tciData.intLineThresholdd1 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_D1]);
            tciData.intLineThresholdd2 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_D2]);
            tciData.intLineThresholde1 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_E1]);
            tciData.intLineThresholde2 = NulltoInt(stArrayData[m_CON_COL_LINE_THRESHOLD_E2]);
            tciData.strAiModelName = stArrayData[m_CON_COL_AI_MODEL_NAME];

            return true;
        }

        /// <summary>
        /// 閾値情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInThreshold(List<ThresholdCsvInfo> lstThresholdCsvToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {

                    foreach (ThresholdCsvInfo tciCurrentData in lstThresholdCsvToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfoThreshold(tciCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesThresholdReg = m_intSuccesThresholdReg + 1;
                        }
                        else
                        {
                            m_intErrorThresholdReg = m_intErrorThresholdReg + 1;
                        }
                    }

                    if (m_intSuccesThresholdReg > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// 閾値更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoThreshold(ThresholdCsvInfo tciCurrentData
                                                         , NpgsqlConnection NpgsqlCon
                                                         , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_THRESHOLD;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(ThresholdCsvInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        if (NulltoInt(fieldInfo.GetValue(tciCurrentData)) == 0)
                        {
                            command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                            { Value = DBNull.Value });
                        }
                        else 
                        {
                            command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                            { Value = NulltoInt(fieldInfo.GetValue(tciCurrentData)) });
                        }
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(tciCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command
                              , transaction
                              , "品番登録情報テーブルの更新に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("閾値情報更新時にエラーが発生しました。" + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "閾値情報更新時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }
        #endregion

        #region マスタ画像取り込む
        /// <summary>
        /// マスタ画像取り込み
        /// </summary>
        /// <param name="fiInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessMasterPng(FileInfo[] fiInputPng)
        {
            string strOutFilePath = "";
            int intFileCount = 0;

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputPng)
            {
                m_strErrorOutFileName = Inputfile.Name;
                intFileCount = intFileCount + 1;

                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorMasterImg = m_intErrorMasterImg + 1;
                    continue;
                }

                // 出力先フォルダパスを取得する
                if (bolGetSystemSettingValue(m_CON_MASTER_IMAGE
                                           , out strOutFilePath) == true)
                {
                    // 対象フォルダなし
                    if (Directory.Exists(strOutFilePath) == false)
                    {
                        // フォルダ作成する
                        Directory.CreateDirectory(strOutFilePath);
                    }

                    // 品番取込時のマスタ画像チェック用にフォルダをコピーする
                    m_strOutMstFolder = strOutFilePath;

                    // マスタ画像を取り込み先のフォルダにコピーする。
                    try
                    {
                        Inputfile.CopyTo(strOutFilePath + @"\" + Inputfile.Name, true);
                        m_intSuccesMasterImg = m_intSuccesMasterImg + 1;
                    }
                    catch (Exception ex)
                    {
                        OutPutImportLog(intFileCount + "件目のマスタ画像の取り込みで例外が発生しました。" + ex.Message);
                        m_intErrorMasterImg = m_intErrorMasterImg + 1;
                        continue;
                    }
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "マスタ画像　取り込み件数：" + (m_intSuccesMasterImg + m_intErrorMasterImg) + "件　"
                             + "正常：" + m_intSuccesMasterImg + "件　"
                             + "異常：" + m_intErrorMasterImg + "件 ";
            OutPutImportLog(strOutMsg);
        }
        #endregion

        #region 判定理由情報CSV取り込む
        /// <summary>
        /// 判定理由情報取り込み
        /// </summary>
        /// <param name="fiInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessDecisionReasonCsv(FileInfo[] fiInputCsv)
        {
            List<DecisionReasonCsvInfo> lstDecisionReasonCsvInfo = new List<DecisionReasonCsvInfo>();

            // フォルダ内のファイルの数だけループする
            foreach (FileInfo Inputfile in fiInputCsv)
            {
                m_strErrorOutFileName = Inputfile.Name;
                // 対象のファイルがロックされているか確認する
                if (IsFileLocked(Inputfile.FullName) == true)
                {
                    // ファイルがロックされている場合、スキップする
                    // ログファイルにエラー出力を行う
                    OutPutImportLog("ファイルがロックされています");
                    m_intErrorDecisionReasonReg = m_intErrorDecisionReasonReg + 1;
                    continue;
                }

                // 判定理由マスタを判定する。
                if (Regex.IsMatch(Inputfile.Name, m_CON_FILE_NAME_REASON_JUDGMENT + ".csv") == true)
                {
                    try
                    {
                        // CSVファイルを取り込み、判定理由マスタを登録する
                        lstDecisionReasonCsvInfo = ImportDecisionReasonCsvData(Inputfile);
                    }
                    catch (Exception ex)
                    {
                        WriteEventLog(g_CON_LEVEL_ERROR, "ファイルOPENに失敗しました " + ex.Message);
                        m_intErrorDecisionReasonReg = m_intErrorDecisionReasonReg + 1;
                        OutPutImportLog("ファイルOPENに失敗しました " + ex.Message);
                        continue;
                    }

                    // 読み込み行が存在する場合は登録を行う
                    if (lstDecisionReasonCsvInfo.Count > 0)
                    {
                        // 読み込んだ値をDBに登録する
                        UPDMstProductInfoInDecisionReason(lstDecisionReasonCsvInfo);
                    }
                }
            }

            m_strErrorOutFileName = "";
            // ログファイル結果出力を行う
            string strOutMsg = "取り込み処理が終了しました。"
                             + "判定理由マスタ　取り込み件数：" + (m_intSuccesDecisionReasonReg + m_intErrorDecisionReasonReg) + "件　"
                             + "正常：" + m_intSuccesDecisionReasonReg + "件　"
                             + "異常：" + m_intErrorDecisionReasonReg + "件 ";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// 判定理由情報取り込み
        /// </summary>
        /// <param name="Inputfile">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<DecisionReasonCsvInfo> ImportDecisionReasonCsvData(FileInfo Inputfile)
        {
            List<DecisionReasonCsvInfo> lstDecisionReasonCsvInfo = new List<DecisionReasonCsvInfo>();
            // 読み込みデータ
            DecisionReasonCsvInfo driCurrentData = new DecisionReasonCsvInfo();

            int intRowCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(Inputfile.FullName
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = "";
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // 閾値情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    intRowCount = intRowCount + 1;
                    if (strFileTextLine == "" || intRowCount == 1)
                    {
                        // 空行（最終行）またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (ReadDecisionReasonCsvData(strFileTextLine
                                                , intRowCount
                                                , Inputfile.Name
                                                , out driCurrentData) == false)
                    {
                        m_intErrorDecisionReasonReg = m_intErrorDecisionReasonReg + 1;
                        continue;
                    }
                    else
                    {
                        // csvのリストに現在行を追加
                        lstDecisionReasonCsvInfo.Add(driCurrentData);
                    }
                }

                return lstDecisionReasonCsvInfo;
            }
        }

        /// <summary>
        /// 判定理由csvファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private static Boolean ReadDecisionReasonCsvData(string strFileTextLine
                                                       , int intRowCount
                                                       , string strFileName
                                                       , out DecisionReasonCsvInfo drcCurrentData)
        {
            drcCurrentData = new DecisionReasonCsvInfo();

            // CSVを読み込む
            if (SetDecisionReasonInfoCsv(strFileTextLine, out drcCurrentData, intRowCount) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判定理由入力チェック
        /// </summary>
        /// <param name="tciCheckData">読み込み閾値情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private static Boolean InputDataCheckDecisionReason(string[] stArrayData
                                                          , int intRowCount
                                                          , string strFileReadLine)
        {
            // 各項目のチェックを行う
            // 列数チェック
            if (stArrayData.Length <= m_CON_COL_DECISION_REASON)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目のファイルレイアウトが不正です。" + strFileReadLine);
                return false;
            }

            // 文字後列項目
            // 必須入力チェック
            if (CheckRequiredInput(stArrayData[m_CON_COL_REASON_CODE], "理由コード", intRowCount, strFileReadLine) == false ||
                CheckRequiredInput(stArrayData[m_CON_COL_DECISION_REASON], "判定理由", intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // 桁数入力チェック
            if (CheckLengthInput(stArrayData[m_CON_COL_REASON_CODE], "理由コード", intRowCount, 1, 2, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_DECISION_REASON], "判定理由", intRowCount, 1, 300, strFileReadLine) == false)
            {
                return false;
            }

            // 最大範囲入力チェック
            if (CheckRangeInput(stArrayData[m_CON_COL_REASON_CODE], "理由コード", intRowCount, 1, 99, strFileReadLine) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ＣＳＶ→構造体格納（閾値情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        /// <returns></returns>
        private static Boolean SetDecisionReasonInfoCsv(string strFileReadLine
                                                      , out DecisionReasonCsvInfo drcData
                                                      , int intRowCount)
        {
            string[] stArrayData;

            drcData = new DecisionReasonCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            // 入力データチェックを行う
            if (InputDataCheckDecisionReason(stArrayData, intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // CSVの各項目を構造体へ格納する
            drcData.intReasonCode = NulltoInt(stArrayData[m_CON_COL_REASON_CODE]);
            drcData.strDecisionReason = stArrayData[m_CON_COL_DECISION_REASON];

            return true;
        }

        /// <summary>
        /// 判定理由情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInDecisionReason(List<DecisionReasonCsvInfo> lstDecisionReasonCsvToDB)
        {
            // PostgreSQLへ接続
            using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(g_strConnectionString))
            {
                NpgsqlCon.Open();

                using (var transaction = NpgsqlCon.BeginTransaction())
                {
                    // テーブルの全件削除を行う
                    ExecDelProductInfoDecisionReason(NpgsqlCon, transaction);

                    foreach (DecisionReasonCsvInfo drcCurrentData in lstDecisionReasonCsvToDB)
                    {
                        // 登録処理実施
                        if (ExecRegProductInfoDecisionReason(drcCurrentData, NpgsqlCon, transaction) == true)
                        {
                            m_intSuccesDecisionReasonReg = m_intSuccesDecisionReasonReg + 1;
                        }
                        else
                        {
                            m_intErrorDecisionReasonReg = m_intErrorDecisionReasonReg + 1;
                        }
                    }

                    if (m_intSuccesDecisionReasonReg > 0) 
                    {
                        // トランザクションコミット
                        transaction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// 特別判定理由削除SQL処理
        /// </summary>
        /// <param name="NpgsqlCon"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private static Boolean ExecDelProductInfoDecisionReason(NpgsqlConnection NpgsqlCon
                                                              , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_DELETE_MST_PRODUCT_INFO_DECISION_REASON;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // sqlを実行する
                if (ExecTranSQL(command
                              , transaction
                              , "判定理由マスタテーブルの削除に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("判定理由削除時にエラーが発生しました。" + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "判定理由削除時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 特別判定理由更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoDecisionReason(DecisionReasonCsvInfo drcCurrentData
                                                              , NpgsqlConnection NpgsqlCon
                                                              , NpgsqlTransaction transaction)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_INSERT_MST_PRODUCT_INFO_DECISION_REASON;

                // SQLコマンドに各パラメータを設定する
                var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(DecisionReasonCsvInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.Int32)
                        { Value = NulltoInt(fieldInfo.GetValue(drcCurrentData)) });
                    }
                    else
                    {
                        command.Parameters.Add(new NpgsqlParameter(fieldInfo.Name, DbType.String)
                        { Value = NulltoString(fieldInfo.GetValue(drcCurrentData)) });
                    }
                }

                // sqlを実行する
                if (ExecTranSQL(command
                              , transaction
                              , "判定理由マスタの登録に失敗しました。") == false)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog("判定理由登録時にエラーが発生しました。" + ex.Message);
                WriteEventLog(g_CON_LEVEL_ERROR, "判定理由登録時にエラーが発生しました。" + ex.Message);
                return false;
            }
        }
        #endregion
        #endregion

        #region 登録実行
        /// <summary>
        /// 登録・更新処理実行
        /// </summary>
        /// <param name="nscCommand">実行SQLコマンド</param>
        /// <param name="transaction">トランザクション情報</param>
        /// <returns></returns>
        public static Boolean ExecTranSQL(NpgsqlCommand nscCommand
                                        , NpgsqlTransaction transaction
                                        , string strErrorMsg)
        {
            try
            {
                nscCommand.ExecuteNonQuery();
                return true;
            }
            catch (NpgsqlException ex)
            {
                WriteEventLog(g_CON_LEVEL_ERROR, strErrorMsg + ex.Message);
                OutPutImportLog(strErrorMsg + ex.Message);
                transaction.Rollback();
                return false;
            }
        }
        #endregion

        #region チェック処理
        /// <summary>
        /// 必須入力チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intRowCount">チェック対象行番号</param>
        /// <param name="intMaxLength">項目最大長</param>
        /// <returns></returns>
        private static Boolean CheckRequiredInput(String strCheckData
                                                , String strItemName
                                                , Int32 intRowCount
                                                , string strTextLine)
        {
            // 必須入力チェック
            if (strCheckData is null || strCheckData == "")
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目　"
                                            + strItemName + "が未設定です。"
                                            + strItemName + "を設定してください。"
                                            + strTextLine);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 桁数チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intRowCount">チェック対象行番号</param>
        /// <param name="intMaxLength">項目最大長</param>
        /// <returns></returns>
        private static Boolean CheckLengthInput(String strCheckData
                                              , String strItemName
                                              , Int32 intRowCount
                                              , Int32 intMinLength
                                              , Int32 intMaxLength
                                              , string strTextLine)
        {
            // 未入力データの場合はチェックしない
            // ※未入力データは必須入力チェックではじく
            if (strCheckData == "") 
            {
                return true;
            }

            // 桁数チェック
            if (strCheckData.Length < intMinLength || strCheckData.Length > intMaxLength)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目　"
                                            + strItemName + "の桁数が不正です。"
                                            + intMinLength.ToString() + "～" + intMaxLength.ToString()
                                            + "の範囲で設定してください "
                                            + strTextLine);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 範囲チェック
        /// </summary>
        /// <param name="strCheckData">チェック対象テキスト</param>
        /// <param name="strItemName">チェック対象項目名</param>
        /// <param name="intRowCount">チェック対象行番号</param>
        /// <param name="intMinRange">項目最小範囲</param>
        /// <param name="intMaxRange">項目最大範囲</param>
        /// <returns></returns>
        private static Boolean CheckRangeInput(String strCheckData
                                             , String strItemName
                                             , Int32 intRowCount
                                             , Int32 intMinRange
                                             , Int32 intMaxRange
                                             , string strTextLine)
        {
            // 未入力データの場合はチェックしない
            // ※未入力データは必須入力チェックではじく
            if (strCheckData == "")
            {
                return true;
            }

            int intCheckData = NulltoInt(strCheckData);

            // 桁数チェック
            if (intCheckData < intMinRange || intCheckData > intMaxRange)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目　"
                                            + strItemName + "の値が不正です。"
                                            + intMinRange.ToString() + "～" + intMaxRange.ToString()
                                            + "の範囲で設定してください "
                                            + strTextLine);
                return false;
            }

            return true;
        }

        /// <summary>
        /// マスタ存在チェック
        /// </summary>
        /// <param name="strFileName">チェックファイル名</param>
        /// <param name="strFilePath">フォルダパス</param>
        /// <returns></returns>
        private static Boolean CheckMstImage()
        {
            // ファイルが存在するか確認
            if (File.Exists(m_strCheckMstFile) == false)
            {
                OutPutImportLog("マスタ画像が取り込まれていません。以下の画像ファイルを取り込んで下さい。" + m_strCheckMstFile);
                return false;
            }

            return true;
        }
        #endregion

        #region ログ出力
        /// <summary>
        /// インポートログ出力
        /// </summary>
        /// <param name="strLogText">ログのテキスト</param>
        /// <param name="strFileName">ログ対象ファイル名</param>
        private static void OutPutImportLog(string strLogText)
        {
            string strOutPutFilePath = "";
            string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 出力ファイル設定
            strOutPutFilePath = m_strOutPutLogFolder + @"\"
                                                     + m_CON_OUTLOGFILE_NAME
                                                     + ".csv";

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(strOutPutFilePath
                                                        , true
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(time + "," + strLogText + "," + m_strErrorOutFileName);
                }
            }
            catch(Exception ex)
            {
                // ログファイル結果出力を行う
                WriteEventLog(g_CON_LEVEL_ERROR, "ログファイルの出力でエラーが発生しました。" + ex.Message);
                return;
            }
        }
        #endregion
    }
}
