﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private const string m_CON_FILE_NAME_REASON_JUDGMENT = "ScratchName";

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

        // AIモデルマスタ情報登録の成功・失敗件数
        private static int m_intSuccesAIModelNameReg = 0;
        private static int m_intErrorAIModelNameReg = 0;

        // エラー出力用ファイル名
        private static string m_strErrorOutFileName = string.Empty;

        // マスタ画像出力先フォルダ
        private static string m_strCheckMstFile = string.Empty;

        // ログ出力追記フラグ
        private static bool m_bolAppendFlag = false;

        // プロセス終了フラグ
        private static bool m_bolProcEnd = false;

        // ファイルNoリスト
        private static List<string> m_lstImportFileNum = new List<string>();

        // 品名リスト
        private static List<string> m_lstImportProductName = new List<string>();

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
        /// 判定理由情報CSVファイル
        /// </summary>
        private struct IniDecisionReasonInfo
        {
            public int intReasonCode;
            public string strDecisionReason;
        }

        /// <summary>
        /// AIモデルマスタ情報CSVファイル
        /// </summary>
        private struct AIModelNameCsvInfo
        {
            public string strProductName;
            public string strAIModelName;
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
            m_intSuccesAIModelNameReg = 0;
            m_intErrorAIModelNameReg = 0;
            m_strErrorOutFileName = string.Empty;
            m_strCheckMstFile = string.Empty;

            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
        }

        /// <summary>
        /// CSV取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnImport_Click(object sender, EventArgs e)
        {
            // 未入力チェック
            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show(g_clsMessageInfo.strMsgE0024);
                btnSearchFolder.Focus();
                return;
            }

            // ログ出力を上書きモードに変更する
            m_bolAppendFlag = false;

            m_bolProcEnd = false;

            string[] strInputIni = Directory.GetFiles(txtFolder.Text, "*.ini", SearchOption.TopDirectoryOnly);

            string[] strInputCsv = Directory.GetFiles(txtFolder.Text, "*.csv", SearchOption.TopDirectoryOnly);

            string[] strInputPng = Directory.GetFiles(txtFolder.Text, "*.bmp", SearchOption.TopDirectoryOnly);

            m_lstImportFileNum.Clear();
            m_lstImportProductName.Clear();

            ProgressForm frmProgress = new ProgressForm();
            frmProgress.StartPosition = FormStartPosition.CenterScreen;
            frmProgress.Height = Screen.FromControl(this).Bounds.Height;
            frmProgress.Width = Screen.FromControl(this).Bounds.Width;
            frmProgress.Show(this);

            await Task.Run(() =>
            {

                // マスタ画像取り込み
                ProcessMasterPng(strInputPng);

                if (m_bolProcEnd)
                {
                    return;
                }

                // 品番マスタ情報取り込み
                ProcessRegisterIni(strInputIni);

                if (m_bolProcEnd)
                {
                    return;
                }

                // PLCマスタ情報取り込み
                ProcessPLCIni(strInputIni);

                if (m_bolProcEnd)
                {
                    return;
                }

                // エアバッグ情報取り込み
                ProcessAirBagIni(strInputIni);

                if (m_bolProcEnd)
                {
                    return;
                }

                // カメラ情報CSV取り込み
                ProcessCameraCsv(strInputCsv);

                if (m_bolProcEnd)
                {
                    return;
                }

                // 閾値情報取り込み
                ProcessImportThreshold();

                if (m_bolProcEnd)
                {
                    return;
                }

                // 判定理由取り込み
                ProcessDecisionReasonIni(strInputIni);

                if (m_bolProcEnd)
                {
                    return;
                }

                // AIモデルマスタ取り込み
                ProcessAIModelNameCsv(strInputCsv);

                if (m_bolProcEnd)
                {
                    return;
                }

            });

            frmProgress.Close();

            // 非同期処理終了後のフラグをチェックする
            if (m_bolProcEnd)
            {
                return;
            }

            // 出力ファイル設定
            string strOutPutFilePath = Path.Combine(g_clsSystemSettingInfo.strLogFileOutputDirectory,
                                                                                         m_CON_OUTLOGFILE_NAME
                                                                                        + ".csv");

            // エラーが一つでもある場合は警告表示する
            if (m_intErrorRegProductInfo > 0 ||
                m_intErrorRegPTC > 0 ||
                m_intErrorRegAirBag > 0 ||
                m_intErrorCameraReg > 0 ||
                m_intErrorThresholdReg > 0 ||
                m_intErrorMasterImg > 0 ||
                m_intErrorDecisionReasonReg > 0 ||
                m_intErrorAIModelNameReg > 0)
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgW0002,
                                              (m_intSuccesRegProductInfo + m_intErrorRegProductInfo), m_intSuccesRegProductInfo, m_intErrorRegProductInfo,
                                              (m_intSuccesRegPTC + m_intErrorRegPTC), m_intSuccesRegPTC, m_intErrorRegPTC,
                                              (m_intSuccesRegAirBag + m_intErrorRegAirBag), m_intSuccesRegAirBag, m_intErrorRegAirBag,
                                              (m_intSuccesCameraReg + m_intErrorCameraReg), m_intSuccesCameraReg, m_intErrorCameraReg,
                                              (m_intSuccesMasterImg + m_intErrorMasterImg), m_intSuccesMasterImg, m_intErrorMasterImg,
                                              (m_intSuccesDecisionReasonReg + m_intErrorDecisionReasonReg), m_intSuccesDecisionReasonReg, m_intErrorDecisionReasonReg,
                                              (m_intSuccesAIModelNameReg + m_intErrorAIModelNameReg), m_intSuccesAIModelNameReg, m_intErrorAIModelNameReg) + Environment.NewLine +
                                string.Format(g_clsMessageInfo.strMsgI0004, strOutPutFilePath),
                                "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(string.Format(g_clsMessageInfo.strMsgI0003,
                                              (m_intSuccesRegProductInfo + m_intErrorRegProductInfo), m_intSuccesRegProductInfo, m_intErrorRegProductInfo,
                                              (m_intSuccesRegPTC + m_intErrorRegPTC), m_intSuccesRegPTC, m_intErrorRegPTC,
                                              (m_intSuccesRegAirBag + m_intErrorRegAirBag), m_intSuccesRegAirBag, m_intErrorRegAirBag,
                                              (m_intSuccesCameraReg + m_intErrorCameraReg), m_intSuccesCameraReg, m_intErrorCameraReg,
                                              (m_intSuccesMasterImg + m_intErrorMasterImg), m_intSuccesMasterImg, m_intErrorMasterImg,
                                              (m_intSuccesDecisionReasonReg + m_intErrorDecisionReasonReg), m_intSuccesDecisionReasonReg, m_intErrorDecisionReasonReg,
                                              (m_intSuccesAIModelNameReg + m_intErrorAIModelNameReg), m_intSuccesAIModelNameReg, m_intErrorAIModelNameReg) + Environment.NewLine +
                                string.Format(g_clsMessageInfo.strMsgI0004, strOutPutFilePath),
                                "取り込み結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();

        }

        /// <summary>
        /// フォルダ検索ボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearchFolder_Click(object sender, EventArgs e)
        {
            SelectFolder();
        }

        /// <summary>
        /// 取込フォルダパス
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtFolder_Click(object sender, EventArgs e)
        {
            SelectFolder();
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
        /// フォルダ選択
        /// </summary>
        private void SelectFolder()
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";

            // 初期ディレクトリを指定する
            fbd.SelectedPath = @"C:\";

            // システム設定情報テーブルに品番情報取込ディレクトリが指定されている場合、初期ディレクトリを更新する
            if (!string.IsNullOrWhiteSpace(g_clsSystemSettingInfo.strProductInfoImportDirectory) &&
                Directory.Exists(g_clsSystemSettingInfo.strProductInfoImportDirectory))
            {
                fbd.SelectedPath = g_clsSystemSettingInfo.strProductInfoImportDirectory;
            }

            //ユーザーが新しいフォルダを作成できるようにする
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                //選択されたフォルダを表示する
                txtFolder.Text = fbd.SelectedPath;
            }
        }

        #region 品番情報取り込み
        /// <summary>
        /// 品番情報取り込み
        /// </summary>
        /// <param name="strInputIni">読み込みINIファイル全種類</param>
        private static void ProcessRegisterIni(string[] strInputIni)
        {
            List<IniDataRegister> lstDataRegistersToDB = new List<IniDataRegister>();

            // 取込対象の品番情報ファイルを特定する
            foreach (string InputfilePath in strInputIni.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}[0-9][0-9].ini$", m_CON_FILE_NAME_REGISTER_INI_DATA), RegexOptions.IgnoreCase)))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);

                try
                {
                    // 品番情報ファイルの場合、取り込みを行う
                    lstDataRegistersToDB = ImportRegisterIniData(InputfilePath);
                }
                catch (Exception ex)
                {
                    // ログファイルにエラー出力を行う
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    m_intErrorRegProductInfo = m_intErrorRegProductInfo + 1;
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    if (m_bolProcEnd)
                    {
                        return;
                    }

                    continue;
                }

                // 読み込んだ値に対してチェック処理を行う
                if (CheckRegisterIniData(lstDataRegistersToDB
                                       , Path.GetFileName(InputfilePath)) == false)
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

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0011,
                                                    (m_intSuccesRegProductInfo + m_intErrorRegProductInfo),
                                                    m_intSuccesRegProductInfo,
                                                    m_intErrorRegProductInfo) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// 品番情報取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniDataRegister> ImportRegisterIniData(string strInputfilePath)
        {
            List<IniDataRegister> lstDataRegisterDB = new List<IniDataRegister>();
            List<string> lstRegisterIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;
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
                    if (Regex.IsMatch(strFileTextLine, string.Format(@"^\[{0}[0-9][0-9][0-9]\]$", m_CON_INI_SECTION_REGISTER), RegexOptions.IgnoreCase) == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", string.Empty);
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", string.Empty);
                        lstRegisterIniSection.Add(strFileTextLineReplace);
                    }
                }

                // セクションの数だけループする
                foreach (string strRegister in lstRegisterIniSection)
                {
                    IniDataRegister idrCurrentData = new IniDataRegister();

                    // Iniファイルの値をクラスへ格納する
                    idrCurrentData = DataRegIniToDBClass(strInputfilePath, strRegister);

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
        /// <param name="strInputfilePath">読み込みINIファイル</param>
        /// <param name="strRegister">セクション</param>
        /// <returns></returns>
        private static IniDataRegister DataRegIniToDBClass(string strInputfilePath
                                                         , string strRegister)
        {
            IniDataRegister idrCurrentData = new IniDataRegister();

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniDataRegister);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // ファイルナンバーの格納を行う
            idrCurrentData.file_num =
                SubstringRight(Path.GetFileNameWithoutExtension(strInputfilePath), 2);

            string strTenmIniValue = string.Empty;

            // 一旦構造体インスタンスをボックス化する(SetValueはobject型じゃないとできない)
            object boxed = idrCurrentData;

            // Iniファイルから各値を読み込む
            foreach (var fieldInfo in fieldInfos)
            {
                // 特殊対応ディクショナリに読み込むキーが存在するかチェックする
                string strUniKey = string.Empty;
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

                if (fieldInfo.Name == m_CON_COL_IMAGE_FILE)
                {
                    m_strCheckMstFile = string.Empty;
                    // 特殊対応項目の場合は特殊対応して設定
                    strTenmIniValue = NulltoString(GetIniValue(strInputfilePath
                                                             , strRegister
                                                             , fieldInfo.Name));

                    string strFileName = Path.GetFileName(strTenmIniValue);
                    m_strCheckMstFile = g_clsSystemSettingInfo.strMasterImageDirectory + @"\" + strFileName;
                    fieldInfo.SetValue(boxed, m_strCheckMstFile);

                    continue;

                }

                if (strUniKey is null == false)
                {
                    // 特殊対応項目の場合は特殊対応して設定
                    strTenmIniValue = NulltoString(GetIniValue(strInputfilePath
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
                                         , NulltoInt(GetIniValue(strInputfilePath
                                                               , strRegister
                                                               , fieldInfo.Name)));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(boxed
                                         , NulltoString(GetIniValue(strInputfilePath
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
            foreach (IniDataRegister idrCurrentData in lstDataRegistersToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfo(idrCurrentData) == true)
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
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 品番登録情報登録SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfo(IniDataRegister idrCurrentData)
        {
            string strFileNum = string.Empty;
            string strProductName = string.Empty;

            try
            {
                DataTable dtData = new DataTable();

                // SQL文を作成する
                string strSelectSql = g_CON_SELECT_MST_PRODUCT_INFO_FILE_NUM;
                string strUpdateSql = g_CON_UPDATE_MST_PRODUCT_INFO_PRODUCT_NAME;
                string strCreateSql = g_CON_INSERT_MST_PRODUCT_INFO;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniDataRegister);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.Int32, Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.String, Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });

                        if (fieldInfo.Name.Equals(m_CON_COL_FILE_NUM))
                        {
                            // 処理対象のファイルNoを退避する
                            strFileNum = NulltoString(fieldInfo.GetValue(idrCurrentData));
                        }
                        else if (fieldInfo.Name.Equals("Name"))
                        {
                            // 処理対象の品名を退避する
                            strProductName = NulltoString(fieldInfo.GetValue(idrCurrentData));
                        }
                    }
                }

                // SQL(SELECT)を実行し、品名が同一のレコードをチェックする
                g_clsConnectionNpgsql.SelectSQL(ref dtData, strSelectSql, lstNpgsqlCommand);

                if (dtData.Rows.Count > 0)
                {
                    // SQL(UPDATE)を実行し、既存レコードの品名を「品名_ファイルNo」に更新する
                    g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand);
                }

                // SQL(UPSERT)を実行し、新規レコードを追加する
                g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                // 処理対象のファイルNoをリストに追加する
                m_lstImportFileNum.Add(strFileNum);

                // 処理対象の品名をリストに追加する
                m_lstImportProductName.Add(strProductName);

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion

        #region PLC取り込み
        /// <summary>
        /// PLC情報取り込み
        /// </summary>
        /// <param name="strInputIni">読み込みINIファイル全種類</param>
        private static void ProcessPLCIni(string[] strInputIni)
        {
            List<IniConfigPLC> lstPLCDataToDB = new List<IniConfigPLC>();

            // 取込対象のPLC設定ファイルを特定する
            foreach (string InputfilePath in strInputIni.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}.ini$", m_CON_FILE_NAME_CONFIG_PLC), RegexOptions.IgnoreCase)))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);

                try
                {
                    // PLC設定より、レジマーク間距離を取得し登録する。
                    lstPLCDataToDB = ImportPLCIniData(InputfilePath);
                }
                catch (Exception ex)
                {
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    m_intErrorRegPTC = m_intErrorRegPTC + 1;
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    if (m_bolProcEnd)
                    {
                        return;
                    }
                    continue;
                }

                // 読み込んだ値に対してチェック処理を行う
                if (CheckPLCIniData(lstPLCDataToDB
                                  , Path.GetFileName(InputfilePath)) == false)
                {
                    continue;
                }

                // 読み込んだ値をDBに登録する
                UPDMstProductInfoInPTC(lstPLCDataToDB);
            }

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0005,
                                                    (m_intSuccesRegPTC + m_intErrorRegPTC),
                                                    m_intSuccesRegPTC,
                                                    m_intErrorRegPTC) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// PLC情報取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniConfigPLC> ImportPLCIniData(string strInputfilePath)
        {
            List<IniConfigPLC> lstDataPLCDB = new List<IniConfigPLC>();
            List<string> lstPLCIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;
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
                    if (Regex.IsMatch(strFileTextLine, string.Format(@"^\[{0}100\]$|^\[{0}[0-9][0-9]\]$", m_CON_INI_SECTION_KIND), RegexOptions.IgnoreCase) == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", string.Empty);
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", string.Empty);
                        lstPLCIniSection.Add(strFileTextLineReplace);
                    }
                }

                if (lstPLCIniSection.Count == 0)
                {
                    return null;
                }

                // セクションの数だけループする
                foreach (string strRegister in lstPLCIniSection)
                {
                    IniConfigPLC icpCurrentData = new IniConfigPLC();

                    // Iniファイルの値をクラスへ格納する
                    icpCurrentData = DataPLCIniToDBClass(strInputfilePath, strRegister);

                    // 取込対象の情報かチェックする
                    if (m_lstImportFileNum.Contains(icpCurrentData.KIND))
                    {
                        lstDataPLCDB.Add(icpCurrentData);
                    }
                }

                return lstDataPLCDB;
            }
        }

        /// <summary>
        /// PLCINIファイル読み込み格納処理
        /// </summary>
        /// <param name="strInputfilePath">読み込みINIファイル</param>
        /// <param name="strRegister">セクション</param>
        /// <returns></returns>
        private static IniConfigPLC DataPLCIniToDBClass(string strInputfilePath
                                                      , string strRegister)
        {
            IniConfigPLC icpCurrentData = new IniConfigPLC();

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniConfigPLC);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // セクションナンバーの格納を行う
            int intSection = NulltoInt(strRegister.Replace(m_CON_COL_PLC_KIND, string.Empty)) - 1;
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
                                         , NulltoInt(GetIniValue(strInputfilePath
                                                               , strRegister
                                                               , fieldInfo.Name)));
                    }
                    else
                    {
                        // それ以外の項目は普通に設定
                        fieldInfo.SetValue(boxed
                                         , NulltoString(GetIniValue(strInputfilePath
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
            if (lstCurrentData == null)
            {
                OutPutImportLog("PLCファイルのデータが存在しません");
                m_intErrorRegPTC++;
                return false;
            }
            else
            {
                if (m_lstImportFileNum.Count != lstCurrentData.Count)
                {
                    foreach (string strFileNum in m_lstImportFileNum)
                    {
                        if (lstCurrentData.Select(x => x.KIND.Equals(strFileNum)).Count() == 0)
                        {
                            OutPutImportLog(string.Format("更新対象のデータが存在しません。[KIND{0:D2}]", NulltoInt(strFileNum) + 1));
                            m_intErrorRegPTC++;
                        }
                    }

                    return false;
                }
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
            foreach (IniConfigPLC icpCurrentData in lstDataPTCToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfoPLC(icpCurrentData) == true)
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
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// PTC更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoPLC(IniConfigPLC idrCurrentData)
        {
            try
            {
                string strKINDNumber = string.Empty;

                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_PTC;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniConfigPLC);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.Int32, Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.String, Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });
                        strKINDNumber = fieldInfo.Name + (NulltoInt(fieldInfo.GetValue(idrCurrentData)) + 1).ToString("D2");
                    }
                }

                // sqlを実行する
                if (g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand) > 0)
                {
                    return true;
                }

                string strErrorMessage = "更新対象のレコードが存在しません。[{0}]";
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strKINDNumber)));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2},{3}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strKINDNumber), m_strErrorOutFileName));
                return false;
            }
            catch (Exception ex)
            {
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion

        #region エアバッグ領域設定取込み
        /// <summary>
        /// エアバッグ情報取り込み
        /// </summary>
        /// <param name="strInputIni">読み込みINIファイル全種類</param>
        private static void ProcessAirBagIni(string[] strInputIni)
        {
            List<string> lstAirBagCoordFile = new List<string>();
            List<IniAirBagCoord> lstAirBagCoordToDB = new List<IniAirBagCoord>();

            // 取込対象のエアバック領域設定ファイルを特定する
            foreach (string InputfilePath in strInputIni.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}[0-9][0-9].ini$", m_CON_FILE_NAME_AIRBAG_COORD), RegexOptions.IgnoreCase) &&
                m_lstImportFileNum.Contains(SubstringRight(Path.GetFileNameWithoutExtension(x), 2))))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);
                lstAirBagCoordFile.Add(Path.GetFileNameWithoutExtension(InputfilePath));

                try
                {
                    // エアバック領域設定より、列数を取得し登録する。
                    lstAirBagCoordToDB = ImportAirBagCoordIniData(InputfilePath);
                }
                catch (Exception ex)
                {
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    if (m_bolProcEnd)
                    {
                        return;
                    }
                    continue;
                }

                // 読み込んだ値に対してチェック処理を行う
                if (CheckAirbagIniData(lstAirBagCoordToDB
                                     , Path.GetFileName(InputfilePath)) == false)
                {
                    m_intErrorRegAirBag = m_intErrorRegAirBag + 1;
                    continue;
                }

                // 読み込んだ値をDBに登録する
                UPDMstProductInfoInAirbag(lstAirBagCoordToDB);
            }

            if (m_lstImportFileNum.Count != lstAirBagCoordFile.Count)
            {
                foreach (string strFileNum in m_lstImportFileNum)
                {
                    if (lstAirBagCoordFile.Select(x => x.Equals(strFileNum)).Count() == 0)
                    {
                        m_strErrorOutFileName = string.Format("{0}{1}.ini", m_CON_FILE_NAME_AIRBAG_COORD, strFileNum);
                        OutPutImportLog("更新対象のデータが存在しません。");
                        m_intErrorRegAirBag++;
                    }
                }
            }

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0006,
                                                    (m_intSuccesRegAirBag + m_intErrorRegAirBag),
                                                    m_intSuccesRegAirBag,
                                                    m_intErrorRegAirBag) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// エアバッグ領域設定取込み取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniAirBagCoord> ImportAirBagCoordIniData(string strInputfilePath)
        {
            List<IniAirBagCoord> lstAirBagCoord = new List<IniAirBagCoord>();
            List<string> lstRegisterIniSection = new List<string>();

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;
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
                    if (Regex.IsMatch(strFileTextLine, string.Format(@"^\[{0}[0-9][0-9][0-9]\]$", m_CON_INI_SECTION_AIRBAG), RegexOptions.IgnoreCase) == true)
                    {
                        // セクションの場合はセクションリストに追加
                        string strFileTextLineReplace = strFileTextLine.Replace("[", string.Empty);
                        strFileTextLineReplace = strFileTextLineReplace.Replace("]", string.Empty);
                        lstRegisterIniSection.Add(strFileTextLineReplace);

                    }
                }

                // エアバッグの設定ファイルの値を格納する
                IniAirBagCoord iabCurrentData = new IniAirBagCoord();
                iabCurrentData = DataAirbagIniToDBClass(strInputfilePath, lstRegisterIniSection);
                lstAirBagCoord.Add(iabCurrentData);

                return lstAirBagCoord;
            }
        }

        /// <summary>
        /// エアバッグINIファイル読み込み格納処理
        /// </summary>
        /// <param name="strInputfilePath">読み込みINIファイル</param>
        /// <param name="lstRegisterIniSection">セクション</param>
        /// <returns></returns>
        private static IniAirBagCoord DataAirbagIniToDBClass(string strInputfilePath
                                                           , List<string> lstRegisterIniSection)
        {
            IniAirBagCoord iabCurrentData = new IniAirBagCoord();

            int intRecordCount = 0;

            string strTopPointATran = string.Empty;
            string strTopPointBTran = string.Empty;
            string strTopPointCTran = string.Empty;
            string strTopPointDTran = string.Empty;
            string strTopPointETran = string.Empty;

            // 各項目の値を取得する
            // FieldInfoを取得する
            Type typeOfMyStruct = typeof(IniAirBagCoord);
            System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

            // ファイルナンバーの格納を行う
            iabCurrentData.file_num =
                SubstringRight(Path.GetFileNameWithoutExtension(strInputfilePath), 2);

            // Iniファイルから各値を読み込む
            foreach (var fieldInfo in fieldInfos)
            {
                // フィールド名ごとに値の格納処理を行う
                if (fieldInfo.Name == m_CON_COL_FILE_NUM)
                {
                    // ファイル名セクション名は設定済みのためスキップ
                    continue;
                }

                if (fieldInfo.Name == m_CON_COL_AIRBAG_NUMBER)
                {
                    // セクションの数だけループする
                    foreach (string strRegister in lstRegisterIniSection)
                    {
                        int intNumberValue = NulltoInt(GetIniValue(strInputfilePath
                                                                 , strRegister
                                                                 , fieldInfo.Name));
                        if (intNumberValue > 0)
                        {
                            intRecordCount = intRecordCount + 1;

                            string strTopPoint = string.Empty;
                            // 頂点座標の設定を行う
                            for (int i = 0; i < intNumberValue; i++)
                            {
                                string strPointValue = GetIniValue(strInputfilePath
                                                                 , strRegister
                                                                 , m_CON_COL_AIRBAG_COORD + String.Format("{0:00}", i));

                                if (!string.IsNullOrEmpty(strTopPoint))
                                {
                                    strTopPoint = strTopPoint + ",";
                                }

                                if (string.IsNullOrEmpty(strPointValue))
                                {
                                    strTopPoint = strTopPoint + "(,)";
                                }
                                else
                                {
                                    string[] strPointValueSplit = strPointValue.Split(m_CON_SEPARATOR_AIRBAG_COORD);
                                    if (strPointValueSplit.Length == 1)
                                    {
                                        strTopPoint = string.Format("{0}({1},)", strTopPoint, strPointValueSplit[1]);
                                    }
                                    else
                                    {
                                        strTopPoint = string.Format("{0}({1},{2})", strTopPoint, strPointValueSplit[0], strPointValueSplit[1]);
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
            foreach (IniAirBagCoord iabCurrentData in lstDataAirBagToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfoAirbag(iabCurrentData) == true)
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
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// エアバッグ更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoAirbag(IniAirBagCoord idrCurrentData)
        {
            try
            {
                string strFileNum = string.Empty;

                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_AIRBAG;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniAirBagCoord);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.Int32, Value = NulltoInt(fieldInfo.GetValue(idrCurrentData)) });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.String, Value = NulltoString(fieldInfo.GetValue(idrCurrentData)) });

                        if (fieldInfo.Name.Equals(m_CON_COL_FILE_NUM))
                        {
                            strFileNum = string.Format("{0}:{1}", m_CON_COL_FILE_NUM, NulltoString(fieldInfo.GetValue(idrCurrentData)));
                        }
                    }
                }

                // sqlを実行する
                if (g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand) > 0)
                {
                    return true;
                }

                string strErrorMessage = "更新対象のレコードが存在しません。[{0}]";
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strFileNum)));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2},{3}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strFileNum), m_strErrorOutFileName));

                return false;
            }
            catch (Exception ex)
            {
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion

        #region カメラ情報CSV取り込み
        /// <summary>
        /// カメラ情報取り込み
        /// </summary>
        /// <param name="strInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessCameraCsv(string[] strInputCsv)
        {
            List<CameraCsvInfo> lstCamCsvInfo = new List<CameraCsvInfo>();

            // 取込対象のカメラ情報ファイルを特定する
            foreach (string InputfilePath in strInputCsv.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}.csv$", m_CON_FILE_NAME_CAMERA_INFO), RegexOptions.IgnoreCase)))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);

                try
                {
                    // CSVファイルを取り込み、カメラ情報を取得し登録する。
                    lstCamCsvInfo = ImportCameraCsvData(InputfilePath);
                }
                catch (Exception ex)
                {
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    m_intErrorCameraReg = m_intErrorCameraReg + 1;
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    if (m_bolProcEnd)
                    {
                        return;
                    }
                    continue;
                }

                // 読み込み行が存在する場合は登録を行う
                if (lstCamCsvInfo.Count > 0)
                {
                    // 読み込んだ値をDBに登録する
                    UPDMstProductInfoInCamera(lstCamCsvInfo);
                }
            }

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0007,
                                                    (m_intSuccesCameraReg + m_intErrorCameraReg),
                                                    m_intSuccesCameraReg,
                                                    m_intErrorCameraReg) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// カメラ情報取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<CameraCsvInfo> ImportCameraCsvData(string strInputfilePath)
        {
            List<CameraCsvInfo> lstCameraCsvInfo = new List<CameraCsvInfo>();
            // 読み込みデータ
            CameraCsvInfo cciCurrentData = new CameraCsvInfo();

            int intRowCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // カメラ情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    intRowCount = intRowCount + 1;
                    if (string.IsNullOrEmpty(strFileTextLine) || intRowCount == 1)
                    {
                        // 空行（最終行）またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (ReadCameraCsvData(strFileTextLine
                                        , intRowCount
                                        , Path.GetFileName(strInputfilePath)
                                        , out cciCurrentData) == false)
                    {
                        m_intErrorCameraReg = m_intErrorCameraReg + 1;
                        continue;
                    }

                    // csvのリストに現在行を追加
                    lstCameraCsvInfo.Add(cciCurrentData);

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
                OutPutImportLog(string.Format("{0},\"{1} \"", string.Format(g_clsMessageInfo.strMsgE0010, intRowCount), strFileReadLine));
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
                CheckLengthInput(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION], "照度情報", intRowCount, 1, 4, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM], "開始レジマークカメラ番号", intRowCount, 1, 2, strFileReadLine) == false ||
                CheckLengthInput(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM], "終了レジマークカメラ番号", intRowCount, 1, 2, strFileReadLine) == false)
            {
                return false;
            }

            // 最大範囲入力チェック
            if (CheckRangeInput(stArrayData[m_CON_COL_ILLUMINATION_INFORMATION], "照度情報", intRowCount, 0, 1024, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_START_REGIMARK_CAMERA_NUM], "開始レジマークカメラ番号", intRowCount, 1, 27, strFileReadLine) == false ||
                CheckRangeInput(stArrayData[m_CON_COL_END_REGIMARK_CAMERA_NUM], "終了レジマークカメラ番号", intRowCount, 1, 27, strFileReadLine) == false)
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
            foreach (CameraCsvInfo cciCurrentData in lstCameraCsvToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfoCamera(cciCurrentData) == true)
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
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// カメラ更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoCamera(CameraCsvInfo cciCurrentData)
        {
            string strData =
                string.Join(
                    ",",
                    cciCurrentData.strProductName,
                    cciCurrentData.intIlluminationInformation.ToString(),
                    cciCurrentData.intStartRegimarkCameraNum.ToString(),
                    cciCurrentData.intEndRegimarkCameraNum.ToString());

            string strErrorMessage = string.Empty;

            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_UPDATE_MST_PRODUCT_INFO_CAMERA;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(CameraCsvInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.Int32, Value = NulltoInt(fieldInfo.GetValue(cciCurrentData)) });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.String, Value = NulltoString(fieldInfo.GetValue(cciCurrentData)) });
                    }
                }

                // sqlを実行する
                if (g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand) > 0)
                {
                    return true;
                }

                strErrorMessage = "更新対象のレコードが存在しません。[{0}]";
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0025, (m_intSuccesCameraReg + m_intErrorCameraReg) + 1) + Environment.NewLine + string.Format(strErrorMessage, strData));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2},{3}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strData), m_strErrorOutFileName));
                return false;
            }
            catch (Exception ex)
            {
                strErrorMessage = "{0},[{1}]";
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0025, (m_intSuccesCameraReg + m_intErrorCameraReg) + 1) + Environment.NewLine + string.Format(strErrorMessage, ex.Message, strData));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion

        #region 閾値情報取り込み
        /// <summary>
        /// 閾値情報取り込み
        /// </summary>
        private static void ProcessImportThreshold()
        {
            #region 変数宣言
            int intTakingCameraCnt = 27;
            int intCommaPosition = 0;
            int intColumnCnt = 0;
            int intLineThresholda1 = 0;
            int intLineThresholda2 = 0;
            int intLineThresholdb1 = 0;
            int intLineThresholdb2 = 0;
            int intLineThresholdc1 = 0;
            int intLineThresholdc2 = 0;
            int intLineThresholdd1 = 0;
            int intLineThresholdd2 = 0;
            int intLineThresholde1 = 0;
            int intLineThresholde2 = 0;
            int intColumnThreshold01 = 0;
            int intColumnThreshold02 = 0;
            int intColumnThreshold03 = 0;
            int intColumnThreshold04 = 0;
            int intTopPointA_XMin = 0;
            int intTopPointA_XMax = 0;
            int intTopPointA_YMin = 0;
            int intTopPointA_YMax = 0;
            int intTopPointB_XMin = 0;
            int intTopPointB_XMax = 0;
            int intTopPointB_YMin = 0;
            int intTopPointB_YMax = 0;
            int intTopPointC_XMin = 0;
            int intTopPointC_XMax = 0;
            int intTopPointC_YMin = 0;
            int intTopPointC_YMax = 0;
            int intTopPointD_XMin = 0;
            int intTopPointD_XMax = 0;
            int intTopPointD_YMin = 0;
            int intTopPointD_YMax = 0;
            int intTopPointE_XMin = 0;
            int intTopPointE_XMax = 0;
            int intTopPointE_YMin = 0;
            int intTopPointE_YMax = 0;
            List<int> intTopPointAList_X = new List<int>();
            List<int> intTopPointAList_Y = new List<int>();
            List<int> intTopPointBList_X = new List<int>();
            List<int> intTopPointBList_Y = new List<int>();
            List<int> intTopPointCList_X = new List<int>();
            List<int> intTopPointCList_Y = new List<int>();
            List<int> intTopPointDList_X = new List<int>();
            List<int> intTopPointDList_Y = new List<int>();
            List<int> intTopPointEList_X = new List<int>();
            List<int> intTopPointEList_Y = new List<int>();
            string strSpace = " ";
            string strReplacementCharacter = "),(";
            string strLeftBracket = "(";
            string strRightBracket = ")";
            string strErrorMessage = string.Empty;
            List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();
            DataTable dtData = new DataTable();
            #endregion

            // 取込対象の品番に対して、閾値情報の算出を行う
            foreach (string strProductName in m_lstImportProductName)
            {
                #region 変数初期化
                intCommaPosition = 0;
                intColumnCnt = 0;
                intLineThresholda1 = 0;
                intLineThresholda2 = 0;
                intLineThresholdb1 = 0;
                intLineThresholdb2 = 0;
                intLineThresholdc1 = 0;
                intLineThresholdc2 = 0;
                intLineThresholdd1 = 0;
                intLineThresholdd2 = 0;
                intLineThresholde1 = 0;
                intLineThresholde2 = 0;
                intColumnThreshold01 = 0;
                intColumnThreshold02 = 0;
                intColumnThreshold03 = 0;
                intColumnThreshold04 = 0;
                intTopPointA_XMin = 0;
                intTopPointA_XMax = 0;
                intTopPointA_YMin = 0;
                intTopPointA_YMax = 0;
                intTopPointB_XMin = 0;
                intTopPointB_XMax = 0;
                intTopPointB_YMin = 0;
                intTopPointB_YMax = 0;
                intTopPointC_XMin = 0;
                intTopPointC_XMax = 0;
                intTopPointC_YMin = 0;
                intTopPointC_YMax = 0;
                intTopPointD_XMin = 0;
                intTopPointD_XMax = 0;
                intTopPointD_YMin = 0;
                intTopPointD_YMax = 0;
                intTopPointE_XMin = 0;
                intTopPointE_XMax = 0;
                intTopPointE_YMin = 0;
                intTopPointE_YMax = 0;
                intTopPointAList_X.Clear();
                intTopPointAList_Y.Clear();
                intTopPointBList_X.Clear();
                intTopPointBList_Y.Clear();
                intTopPointCList_X.Clear();
                intTopPointCList_Y.Clear();
                intTopPointDList_X.Clear();
                intTopPointDList_Y.Clear();
                intTopPointEList_X.Clear();
                intTopPointEList_Y.Clear();
                strErrorMessage = string.Empty;
                lstNpgsqlCommand.Clear();
                dtData.Clear();
                #endregion

                #region 品番情報取得
                try
                {
                    // SQL文を設定する
                    string strSelectSql = g_CON_SELECT_MST_PRODUCT_INFO_TOP_POINT;

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "strProductName", DbType = DbType.String, Value = strProductName });

                    // SQL文を実行する
                    g_clsConnectionNpgsql.SelectSQL(ref dtData, strSelectSql, lstNpgsqlCommand);
                }
                catch (Exception ex)
                {
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0021, Environment.NewLine, ex.Message));
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0021, Environment.NewLine, ex.Message));
                    m_intErrorThresholdReg++;

                    // 閾値算出エラー時、品番情報取込の成功・失敗件数を補正する
                    m_intSuccesRegProductInfo--;
                    m_intErrorRegProductInfo++;
                    continue;
                }
                #endregion

                #region 閾値情報算出
                try
                {
                    intColumnCnt = NulltoInt(dtData.Rows[0]["column_cnt"].ToString());

                    // 頂点座標Aの設定値より、X座標・Y座標を抽出する
                    foreach (string strTopPointA in
                        NulltoString(dtData.Rows[0]["top_point_a"].ToString())
                        .Replace(strReplacementCharacter, strSpace)
                        .Replace(strLeftBracket, string.Empty)
                        .Replace(strRightBracket, string.Empty)
                        .Split(strSpace.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(strTopPointA))
                        {
                            continue;
                        }

                        intCommaPosition = strTopPointA.IndexOf(",");
                        intTopPointAList_X.Add(NulltoInt(strTopPointA.Substring(0, intCommaPosition)));
                        intTopPointAList_Y.Add(NulltoInt(strTopPointA.Substring(++intCommaPosition)));
                    }

                    // 頂点座標Bの設定値より、X座標・Y座標を抽出する
                    foreach (string strTopPointB in
                        NulltoString(dtData.Rows[0]["top_point_b"].ToString())
                        .Replace(strReplacementCharacter, strSpace)
                        .Replace(strLeftBracket, string.Empty)
                        .Replace(strRightBracket, string.Empty)
                        .Split(strSpace.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(strTopPointB))
                        {
                            continue;
                        }

                        intCommaPosition = strTopPointB.IndexOf(",");
                        intTopPointBList_X.Add(NulltoInt(strTopPointB.Substring(0, intCommaPosition)));
                        intTopPointBList_Y.Add(NulltoInt(strTopPointB.Substring(++intCommaPosition)));
                    }

                    // 頂点座標Cの設定値より、X座標・Y座標を抽出する
                    foreach (string strTopPointC in
                        NulltoString(dtData.Rows[0]["top_point_c"].ToString())
                        .Replace(strReplacementCharacter, strSpace)
                        .Replace(strLeftBracket, string.Empty)
                        .Replace(strRightBracket, string.Empty)
                        .Split(strSpace.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(strTopPointC))
                        {
                            continue;
                        }

                        intCommaPosition = strTopPointC.IndexOf(",");
                        intTopPointCList_X.Add(NulltoInt(strTopPointC.Substring(0, intCommaPosition)));
                        intTopPointCList_Y.Add(NulltoInt(strTopPointC.Substring(++intCommaPosition)));
                    }

                    // 頂点座標Dの設定値より、X座標・Y座標を抽出する
                    foreach (string strTopPointD in
                        NulltoString(dtData.Rows[0]["top_point_d"].ToString())
                        .Replace(strReplacementCharacter, strSpace)
                        .Replace(strLeftBracket, string.Empty)
                        .Replace(strRightBracket, string.Empty)
                        .Split(strSpace.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(strTopPointD))
                        {
                            continue;
                        }

                        intCommaPosition = strTopPointD.IndexOf(",");
                        intTopPointDList_X.Add(NulltoInt(strTopPointD.Substring(0, intCommaPosition)));
                        intTopPointDList_Y.Add(NulltoInt(strTopPointD.Substring(++intCommaPosition)));
                    }

                    // 頂点座標Eの設定値より、X座標・Y座標を抽出する
                    foreach (string strTopPointE in
                        NulltoString(dtData.Rows[0]["top_point_e"].ToString())
                        .Replace(strReplacementCharacter, strSpace)
                        .Replace(strLeftBracket, string.Empty)
                        .Replace(strRightBracket, string.Empty)
                        .Split(strSpace.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(strTopPointE))
                        {
                            continue;
                        }

                        intCommaPosition = strTopPointE.IndexOf(",");
                        intTopPointEList_X.Add(NulltoInt(strTopPointE.Substring(0, intCommaPosition)));
                        intTopPointEList_Y.Add(NulltoInt(strTopPointE.Substring(++intCommaPosition)));
                    }

                    // 頂点座標AのX座標から最小値・最大値を取得する
                    if (intTopPointAList_X.Count != 0)
                    {
                        intTopPointA_XMin = intTopPointAList_X.Min();
                        intTopPointA_XMax = intTopPointAList_X.Max();
                    }

                    // 頂点座標AのY座標から最小値・最大値を取得する
                    if (intTopPointAList_Y.Count != 0)
                    {
                        intTopPointA_YMin = intTopPointAList_Y.Min();
                        intTopPointA_YMax = intTopPointAList_Y.Max();
                    }

                    // 頂点座標BのX座標から最小値・最大値を取得する
                    if (intTopPointBList_X.Count != 0)
                    {
                        intTopPointB_XMin = intTopPointBList_X.Min();
                        intTopPointB_XMax = intTopPointBList_X.Max();
                    }

                    // 頂点座標BのY座標から最小値・最大値を取得する
                    if (intTopPointBList_Y.Count != 0)
                    {
                        intTopPointB_YMin = intTopPointBList_Y.Min();
                        intTopPointB_YMax = intTopPointBList_Y.Max();
                    }

                    // 頂点座標CのX座標から最小値・最大値を取得する
                    if (intTopPointCList_X.Count != 0)
                    {
                        intTopPointC_XMin = intTopPointCList_X.Min();
                        intTopPointC_XMax = intTopPointCList_X.Max();
                    }

                    // 頂点座標CのY座標から最小値・最大値を取得する
                    if (intTopPointCList_Y.Count != 0)
                    {
                        intTopPointC_YMin = intTopPointCList_Y.Min();
                        intTopPointC_YMax = intTopPointCList_Y.Max();
                    }

                    // 頂点座標DのX座標から最小値・最大値を取得する
                    if (intTopPointDList_X.Count != 0)
                    {
                        intTopPointD_XMin = intTopPointDList_X.Min();
                        intTopPointD_XMax = intTopPointDList_X.Max();
                    }

                    // 頂点座標DのY座標から最小値・最大値を取得する
                    if (intTopPointDList_Y.Count != 0)
                    {
                        intTopPointD_YMin = intTopPointDList_Y.Min();
                        intTopPointD_YMax = intTopPointDList_Y.Max();
                    }

                    // 頂点座標EのX座標から最小値・最大値を取得する
                    if (intTopPointEList_X.Count != 0)
                    {
                        intTopPointE_XMin = intTopPointEList_X.Min();
                        intTopPointE_XMax = intTopPointEList_X.Max();
                    }

                    // 頂点座標EのY座標から最小値・最大値を取得する
                    if (intTopPointEList_Y.Count != 0)
                    {
                        intTopPointE_YMin = intTopPointEList_Y.Min();
                        intTopPointE_YMax = intTopPointEList_Y.Max();
                    }

                    if (intColumnCnt > 0)
                    {
                        // 行閾値を設定する
                        intLineThresholda1 = intTopPointA_XMin;
                        intLineThresholda2 = intTopPointA_XMax;

                        // 次列の情報をチェックする
                        if (intTopPointB_YMin != 0)
                        {
                            // 列閾値を算出し設定する
                            intColumnThreshold01 = NulltoInt(Math.Round(intTopPointA_YMax + (intTopPointB_YMin - intTopPointA_YMax) / 2m));
                        }
                        else
                        {
                            // 次列の情報が無い場合、A列Y座標の最大値を設定する
                            intColumnThreshold01 = intTopPointA_YMax;
                        }
                    }

                    if (intColumnCnt > 1)
                    {
                        // 行閾値を設定する
                        intLineThresholdb1 = intTopPointB_XMin;
                        intLineThresholdb2 = intTopPointB_XMax;

                        // 次列の情報をチェックする
                        if (intTopPointB_YMin != 0)
                        {
                            // 列閾値を算出し設定する
                            intColumnThreshold01 = NulltoInt(Math.Round(intTopPointA_YMax + (intTopPointB_YMin - intTopPointA_YMax) / 2m));
                        }
                        else
                        {
                            // 次列の情報が無い場合、A列Y座標の最大値を設定する
                            intColumnThreshold01 = intTopPointA_YMax;
                        }
                    }

                    if (intColumnCnt > 2)
                    {
                        // 行閾値を設定する
                        intLineThresholdc1 = intTopPointC_XMin;
                        intLineThresholdc2 = intTopPointC_XMax;

                        // 次列の情報をチェックする
                        if (intTopPointC_YMin != 0)
                        {
                            // 列閾値を算出し設定する
                            intColumnThreshold02 = NulltoInt(Math.Round(intTopPointB_YMax + (intTopPointC_YMin - intTopPointB_YMax) / 2m));
                        }
                        else
                        {
                            // 次列の情報が無い場合、B列Y座標の最大値を設定する
                            intColumnThreshold02 = intTopPointB_YMax;
                        }
                    }

                    if (intColumnCnt > 3)
                    {
                        // 行閾値を設定する
                        intLineThresholdd1 = intTopPointD_XMin;
                        intLineThresholdd2 = intTopPointD_XMax;

                        // 次列の情報をチェックする
                        if (intTopPointD_YMin != 0)
                        {
                            // 列閾値を算出し設定する
                            intColumnThreshold03 = NulltoInt(Math.Round(intTopPointC_YMax + (intTopPointD_YMin - intTopPointC_YMax) / 2m));
                        }
                        else
                        {
                            // 次列の情報が無い場合、C列Y座標の最大値を設定する
                            intColumnThreshold03 = intTopPointC_YMax;
                        }
                    }

                    if (intColumnCnt > 4)
                    {
                        // 行閾値を設定する
                        intLineThresholde1 = intTopPointE_XMin;
                        intLineThresholde2 = intTopPointE_XMax;

                        // 次列の情報をチェックする
                        if (intTopPointE_YMin != 0)
                        {
                            // 列閾値を算出し設定する
                            intColumnThreshold04 = NulltoInt(Math.Round(intTopPointD_YMax + (intTopPointE_YMin - intTopPointD_YMax) / 2m));
                        }
                        else
                        {
                            // 次列の情報が無い場合、D列Y座標の最大値を設定する
                            intColumnThreshold04 = intTopPointD_YMax;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OutPutImportLog(string.Format("{0}{1}{2}[{3}]", g_clsMessageInfo.strMsgE0026, Environment.NewLine, ex.Message, strProductName));
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}[{3}]", g_clsMessageInfo.strMsgE0026, Environment.NewLine, ex.Message, strProductName));
                    m_intErrorThresholdReg++;

                    // 閾値算出エラー時、品番情報取込の成功・失敗件数を補正する
                    m_intSuccesRegProductInfo--;
                    m_intErrorRegProductInfo++;
                    continue;
                }
                #endregion

                #region 品番情報更新
                try
                {
                    // SQL文を設定する
                    string strUpdateSql = g_CON_UPDATE_MST_PRODUCT_INFO_THRESHOLD;

                    // SQLコマンドに各パラメータを設定する
                    lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intTakingCameraCnt", DbType = DbType.Int32, Value = intTakingCameraCnt });

                    if (intColumnThreshold01 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold01", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold01", DbType = DbType.Int32, Value = intColumnThreshold01 });
                    }

                    if (intColumnThreshold02 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold02", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold02", DbType = DbType.Int32, Value = intColumnThreshold02 });
                    }

                    if (intColumnThreshold03 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold03", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold03", DbType = DbType.Int32, Value = intColumnThreshold03 });
                    }

                    if (intColumnThreshold04 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold04", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intColumnThreshold04", DbType = DbType.Int32, Value = intColumnThreshold04 });
                    }

                    if (intLineThresholda1 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholda1", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholda1", DbType = DbType.Int32, Value = intLineThresholda1 });
                    }

                    if (intLineThresholda2 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholda2", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholda2", DbType = DbType.Int32, Value = intLineThresholda2 });
                    }

                    if (intLineThresholdb1 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdb1", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdb1", DbType = DbType.Int32, Value = intLineThresholdb1 });
                    }

                    if (intLineThresholdb2 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdb2", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdb2", DbType = DbType.Int32, Value = intLineThresholdb2 });
                    }

                    if (intLineThresholdc1 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdc1", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdc1", DbType = DbType.Int32, Value = intLineThresholdc1 });
                    }

                    if (intLineThresholdc2 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdc2", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdc2", DbType = DbType.Int32, Value = intLineThresholdc2 });
                    }

                    if (intLineThresholdd1 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdd1", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdd1", DbType = DbType.Int32, Value = intLineThresholdd1 });
                    }

                    if (intLineThresholdd2 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdd2", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholdd2", DbType = DbType.Int32, Value = intLineThresholdd2 });
                    }

                    if (intLineThresholde1 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholde1", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholde1", DbType = DbType.Int32, Value = intLineThresholde1 });
                    }

                    if (intLineThresholde2 == 0)
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholde2", DbType = DbType.Int32, Value = DBNull.Value });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = "intLineThresholde2", DbType = DbType.Int32, Value = intLineThresholde2 });
                    }

                    // SQL文を実行する
                    if (g_clsConnectionNpgsql.ExecTranSQL(strUpdateSql, lstNpgsqlCommand) != 1)
                    {
                        strErrorMessage = "更新対象のレコードが存在しません。[{0}]";
                        OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strProductName)));
                        WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, strProductName)));
                        m_intErrorThresholdReg++;

                        // 閾値算出エラー時、品番情報取込の成功・失敗件数を補正する
                        m_intSuccesRegProductInfo--;
                        m_intErrorRegProductInfo++;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    strErrorMessage = "{0},[{1}]";
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, string.Format(strErrorMessage, ex.Message, strProductName)));
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                    m_intErrorThresholdReg++;

                    // 閾値算出エラー時、品番情報取込の成功・失敗件数を補正する
                    m_intSuccesRegProductInfo--;
                    m_intErrorRegProductInfo++;
                    continue;
                }

                m_intSuccesThresholdReg++;
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
                #endregion
            }

            #region ログ出力
            m_strErrorOutFileName = string.Empty;

            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0008,
                                                    (m_intSuccesThresholdReg + m_intErrorThresholdReg),
                                                    m_intSuccesThresholdReg,
                                                    m_intErrorThresholdReg) + "\"";
            OutPutImportLog(strOutMsg);
            #endregion
        }
        #endregion

        #region AIモデルマスタ情報CSV取り込み
        /// <summary>
        /// AIモデルマスタ情報取り込み
        /// </summary>
        /// <param name="strInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessAIModelNameCsv(string[] strInputCsv)
        {
            List<AIModelNameCsvInfo> lstAImCsvInfo = new List<AIModelNameCsvInfo>();

            // 取込対象のAIモデルマスタ情報ファイルを特定する
            foreach (string InputfilePath in strInputCsv.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}.csv$", m_CON_FILE_NAME_AI_MODEL_NAME_INFO), RegexOptions.IgnoreCase)))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);

                try
                {
                    // CSVファイルを取り込み、AIモデルマスタ情報を取得し登録する。
                    lstAImCsvInfo = ImportAIModelNameCsvData(InputfilePath);
                }
                catch (Exception ex)
                {
                    m_intErrorAIModelNameReg++;

                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));

                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));

                    if (m_bolProcEnd)
                    {
                        return;
                    }

                    continue;
                }

                // 読み込み行が存在する場合は登録を行う
                if (lstAImCsvInfo.Count > 0)
                {
                    // 読み込んだ値をDBに登録する
                    UPDMstProductInfoInAIModelName(lstAImCsvInfo);
                }
            }

            m_strErrorOutFileName = string.Empty;

            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0013,
                                                    (m_intSuccesAIModelNameReg + m_intErrorAIModelNameReg),
                                                    m_intSuccesAIModelNameReg,
                                                    m_intErrorAIModelNameReg) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// AIモデルマスタ情報取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<AIModelNameCsvInfo> ImportAIModelNameCsvData(string strInputfilePath)
        {
            List<AIModelNameCsvInfo> lstAIModelNameCsvInfo = new List<AIModelNameCsvInfo>();

            // 読み込みデータ
            AIModelNameCsvInfo cciCurrentData = new AIModelNameCsvInfo();

            int intRowCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath, Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;

                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // AIモデルマスタ情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    intRowCount++;

                    if (string.IsNullOrEmpty(strFileTextLine) || intRowCount == 1)
                    {
                        // 空行(最終行)またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    // CSVファイル読み込み＆入力データチェックを行う
                    if (!ReadAIModelNameCsvData(
                            strFileTextLine,
                            intRowCount,
                            Path.GetFileName(strInputfilePath),
                            out cciCurrentData))
                    {
                        m_intErrorAIModelNameReg++;
                        continue;
                    }

                    lstAIModelNameCsvInfo.Add(cciCurrentData);
                }

                return lstAIModelNameCsvInfo;
            }
        }

        /// <summary>
        /// AIモデルマスタCSVファイル読み込み
        /// </summary>
        /// <param name="strFileTextLine">CSVファイル行テキスト</param>
        /// <param name="lstUserData">CSVファイルデータ</param>
        /// <returns></returns>
        private static bool ReadAIModelNameCsvData(
            string strFileTextLine,
            int intRowCount,
            string strFileName,
            out AIModelNameCsvInfo cciCurrentData)
        {
            cciCurrentData = new AIModelNameCsvInfo();

            // データチェック＆CSVを読み込む
            if (!SetAIModelNameInfoCsv(strFileTextLine, out cciCurrentData, intRowCount))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// CSV→構造体格納(AIモデルマスタ情報CSV)
        /// </summary>
        /// <param name="strFileReadLine">読み込みCSV情報</param>
        /// <returns></returns>
        private static bool SetAIModelNameInfoCsv(
            string strFileReadLine,
            out AIModelNameCsvInfo cciData,
            int intRowCount)
        {
            string[] stArrayData;

            cciData = new AIModelNameCsvInfo();

            // 半角スペース区切りで分割して配列に格納する
            stArrayData = strFileReadLine.Split(',');

            if (!InputDataCheckAIModelName(stArrayData, intRowCount, strFileReadLine))
            {
                return false;
            }

            // データに問題がない場合、CSVの各項目を構造体へ格納する
            cciData.strProductName = stArrayData[m_CON_COL_PRODUCT_NAME_MST_AI_MODEL];
            cciData.strAIModelName = stArrayData[m_CON_COL_AI_MODEL_NAME];

            return true;
        }

        /// <summary>
        /// AIモデルマスタ入力チェック
        /// </summary>
        /// <param name="cciCheckData">読み込みユーザ情報リスト</param>
        /// <param name="intRowCount">対象行番号</param>
        /// <returns></returns>
        private static bool InputDataCheckAIModelName(
            string[] stArrayData,
            int intRowCount,
            string strFileReadLine)
        {
            // 各項目のチェックを行う
            // 列数チェック
            if (stArrayData.Length < m_CON_COL_MST_AI_MODEL_COLUMN_NUM)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(string.Format("{0},\"{1} \"", string.Format(g_clsMessageInfo.strMsgE0010, intRowCount), strFileReadLine));
                return false;
            }

            // 文字列項目
            // 必須入力チェック
            if (!CheckRequiredInput(stArrayData[m_CON_COL_PRODUCT_NAME_MST_AI_MODEL], "品名", intRowCount, strFileReadLine) ||
                !CheckRequiredInput(stArrayData[m_CON_COL_AI_MODEL_NAME], "AIモデル名", intRowCount, strFileReadLine))
            {
                return false;
            }

            // 桁数入力チェック
            if (!CheckLengthInput(stArrayData[m_CON_COL_PRODUCT_NAME_MST_AI_MODEL], "品名", intRowCount, 1, 16, strFileReadLine) ||
                !CheckLengthInput(stArrayData[m_CON_COL_AI_MODEL_NAME], "AIモデル名", intRowCount, 1, 259, strFileReadLine))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// AIモデルマスタ情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInAIModelName(List<AIModelNameCsvInfo> lstAIModelNameCsvToDB)
        {
            foreach (AIModelNameCsvInfo cciCurrentData in lstAIModelNameCsvToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfoAIModelName(cciCurrentData))
                {
                    m_intSuccesAIModelNameReg++;
                }
                else
                {
                    m_intErrorAIModelNameReg++;
                }
            }

            if (m_intSuccesAIModelNameReg > 0)
            {
                // トランザクションコミット
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// AIモデルマスタ更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static bool ExecRegProductInfoAIModelName(AIModelNameCsvInfo cciCurrentData)
        {
            string strData =
                string.Join(
                    ",",
                    cciCurrentData.strProductName,
                    cciCurrentData.strAIModelName);

            string strProductName = string.Empty;
            string strAIModelName = string.Empty;
            string strErrorMessage = string.Empty;

            try
            {
                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(AIModelNameCsvInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // CSVファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.Name.Equals("strProductName"))
                    {
                        strProductName = NulltoString(fieldInfo.GetValue(cciCurrentData));
                    }

                    if (fieldInfo.Name.Equals("strAIModelName"))
                    {
                        strAIModelName = NulltoString(fieldInfo.GetValue(cciCurrentData));
                    }
                }

                UpsertAIModelName(strProductName, strAIModelName);

                return true;
            }
            catch (Exception ex)
            {
                strErrorMessage = "{0},[{1}]";
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0025, (m_intSuccesAIModelNameReg + m_intErrorAIModelNameReg) + 1) + Environment.NewLine + string.Format(strErrorMessage, ex.Message, strData));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0053, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion

        #region マスタ画像取り込む
        /// <summary>
        /// マスタ画像取り込み
        /// </summary>
        /// <param name="strInputCsv">読み込みcsvファイル全種類</param>
        private static void ProcessMasterPng(string[] strInputPng)
        {
            int intFileCount = 0;

            // フォルダ内のファイルの数だけループする
            foreach (string InputfilePath in strInputPng)
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);
                intFileCount = intFileCount + 1;

                // マスタ画像を取り込み先のフォルダにコピーする。
                try
                {
                    File.Copy(InputfilePath, Path.Combine(g_clsSystemSettingInfo.strMasterImageDirectory, Path.GetFileName(InputfilePath)), true);
                    m_intSuccesMasterImg = m_intSuccesMasterImg + 1;
                }
                catch (Exception ex)
                {
                    OutPutImportLog("\"" + string.Format(g_clsMessageInfo.strMsgE0027, intFileCount) + Environment.NewLine + ex.Message + "\"," +
                                    Path.GetFileName(InputfilePath));
                    m_intErrorMasterImg = m_intErrorMasterImg + 1;
                    if (m_bolProcEnd)
                    {
                        return;
                    }
                    continue;
                }
            }

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0009,
                                                    (m_intSuccesMasterImg + m_intErrorMasterImg),
                                                    m_intSuccesMasterImg,
                                                    m_intErrorMasterImg) + "\"";
            OutPutImportLog(strOutMsg);
        }
        #endregion

        #region 判定理由情報CSV取り込む
        /// <summary>
        /// 判定理由情報取り込み
        /// </summary>
        /// <param name="strInputIni">読み込みcsvファイル全種類</param>
        private static void ProcessDecisionReasonIni(string[] strInputIni)
        {
            List<IniDecisionReasonInfo> lstDecisionReasonCsvInfo = new List<IniDecisionReasonInfo>();

            // 取込対象の判定理由マスタファイルを特定する
            foreach (string InputfilePath in strInputIni.Where(
                x => Regex.IsMatch(Path.GetFileName(x), string.Format("^{0}.ini$", m_CON_FILE_NAME_REASON_JUDGMENT), RegexOptions.IgnoreCase)))
            {
                m_strErrorOutFileName = Path.GetFileName(InputfilePath);

                try
                {
                    // Iniファイルを取り込み、判定理由マスタを登録する
                    lstDecisionReasonCsvInfo = ImportDecisionReasonIniData(InputfilePath);
                }
                catch (Exception ex)
                {
                    WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    m_intErrorDecisionReasonReg = m_intErrorDecisionReasonReg + 1;
                    OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                    if (m_bolProcEnd)
                    {
                        return;
                    }
                    continue;
                }

                // 読み込み行が存在する場合は登録を行う
                if (lstDecisionReasonCsvInfo.Count > 0)
                {
                    // 読み込んだ値をDBに登録する
                    UPDMstProductInfoInDecisionReason(lstDecisionReasonCsvInfo);
                }
            }

            m_strErrorOutFileName = string.Empty;
            // ログファイル結果出力を行う
            string strOutMsg = "\"" + string.Format(g_clsMessageInfo.strMsgI0010,
                                                    (m_intSuccesDecisionReasonReg + m_intErrorDecisionReasonReg),
                                                    m_intSuccesDecisionReasonReg,
                                                    m_intErrorDecisionReasonReg) + "\"";
            OutPutImportLog(strOutMsg);
        }

        /// <summary>
        /// 判定理由情報取り込み
        /// </summary>
        /// <param name="strInputfilePath">読み込みファイル情報</param>
        /// <returns></returns>
        private static List<IniDecisionReasonInfo> ImportDecisionReasonIniData(string strInputfilePath)
        {
            List<IniDecisionReasonInfo> lstDecisionReasonCsvInfo = new List<IniDecisionReasonInfo>();
            // 読み込みデータ
            IniDecisionReasonInfo driCurrentData = new IniDecisionReasonInfo();

            int intRowCount = 0;

            // ファイル読み込み処理を行う
            using (StreamReader sr = new StreamReader(strInputfilePath
                                                    , Encoding.GetEncoding("Shift_JIS")))
            {
                string strFileTextLine = string.Empty;
                // ストリームの末尾まで繰り返す
                while (!sr.EndOfStream)
                {
                    // 判定理由情報ファイルを１行読み込む
                    strFileTextLine = sr.ReadLine();
                    intRowCount = intRowCount + 1;
                    if (string.IsNullOrEmpty(strFileTextLine) || intRowCount == 1)
                    {
                        // 空行（最終行）またはヘッダ行の場合読み飛ばす
                        continue;
                    }

                    if (!strFileTextLine.StartsWith("Name"))
                    {
                        // 判定理由ではないため、読み飛ばす
                        continue;
                    }

                    // ファイル読み込み＆入力データチェックを行う
                    if (ReadDecisionReasonCsvData(strFileTextLine
                                                , intRowCount
                                                , Path.GetFileName(strInputfilePath)
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
                                                       , out IniDecisionReasonInfo drcCurrentData)
        {
            drcCurrentData = new IniDecisionReasonInfo();

            // INIを読み込む
            if (SetDecisionReasonInfoCsv(strFileTextLine, out drcCurrentData, intRowCount) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判定理由入力チェック
        /// </summary>
        /// <param name="tciCheckData">読み込み判定理由情報リスト</param>
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
                OutPutImportLog(string.Format(g_clsMessageInfo.strMsgE0010, intRowCount) + ",\"" + strFileReadLine + "\"");
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
        /// INI→構造体格納（判定理由情報CSV）
        /// </summary>
        /// <param name="strFileReadLine">読み込みINI情報</param>
        /// <returns></returns>
        private static Boolean SetDecisionReasonInfoCsv(string strFileReadLine
                                                      , out IniDecisionReasonInfo drcData
                                                      , int intRowCount)
        {
            string[] stArrayData;

            drcData = new IniDecisionReasonInfo();

            // 不要な文字を削除する。
            string strWk = strFileReadLine.Replace("Name", string.Empty);
            strWk = strWk.Substring(0, strWk.LastIndexOf(" "));

            // =で分割して配列に格納する
            stArrayData = strWk.Split('=');

            try
            {
                // 理由コードを編集する
                stArrayData[0] = int.Parse(stArrayData[0]).ToString();
            }
            catch
            {
                // 例外を無視する
            }


            // 入力データチェックを行う
            if (InputDataCheckDecisionReason(stArrayData, intRowCount, strFileReadLine) == false)
            {
                return false;
            }

            // INIの各項目を構造体へ格納する
            drcData.intReasonCode = NulltoInt(stArrayData[m_CON_COL_REASON_CODE]);
            drcData.strDecisionReason = stArrayData[m_CON_COL_DECISION_REASON];

            return true;
        }

        /// <summary>
        /// 判定理由情報テーブル登録処理
        /// </summary>
        /// <param name="lstDataPTCToDB"></param>
        /// <returns></returns>
        private static void UPDMstProductInfoInDecisionReason(List<IniDecisionReasonInfo> lstDecisionReasonIniToDB)
        {
            // テーブルの全件削除を行う
            ExecDelProductInfoDecisionReason();

            foreach (IniDecisionReasonInfo drcCurrentData in lstDecisionReasonIniToDB)
            {
                // 登録処理実施
                if (ExecRegProductInfoDecisionReason(drcCurrentData) == true)
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
                g_clsConnectionNpgsql.DbCommit();
                g_clsConnectionNpgsql.DbClose();
            }
        }

        /// <summary>
        /// 特別判定理由削除SQL処理
        /// </summary>
        /// <param name="NpgsqlCon"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        private static Boolean ExecDelProductInfoDecisionReason()
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_DELETE_MST_PRODUCT_INFO_DECISION_REASON;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strCreateSql);

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0028, Environment.NewLine, ex.Message));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 特別判定理由更新SQL処理
        /// </summary>
        /// <param name="lstUserData">読み込みデータ一覧</param>
        /// <returns></returns>
        private static Boolean ExecRegProductInfoDecisionReason(IniDecisionReasonInfo drcCurrentData)
        {
            try
            {
                // SQL文を作成する
                string strCreateSql = g_CON_INSERT_MST_PRODUCT_INFO_DECISION_REASON;

                // SQLコマンドに各パラメータを設定する
                List<ConnectionNpgsql.structParameter> lstNpgsqlCommand = new List<ConnectionNpgsql.structParameter>();

                // 各項目の値を取得する
                // FieldInfoを取得する
                Type typeOfMyStruct = typeof(IniDecisionReasonInfo);
                System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                // Iniファイルから各値を読み込む
                foreach (var fieldInfo in fieldInfos)
                {
                    if (fieldInfo.FieldType == typeof(int))
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.Int32, Value = NulltoInt(fieldInfo.GetValue(drcCurrentData)) });
                    }
                    else
                    {
                        lstNpgsqlCommand.Add(new ConnectionNpgsql.structParameter { ParameterName = fieldInfo.Name, DbType = DbType.String, Value = NulltoString(fieldInfo.GetValue(drcCurrentData)) });
                    }
                }

                // sqlを実行する
                g_clsConnectionNpgsql.ExecTranSQL(strCreateSql, lstNpgsqlCommand);

                return true;
            }
            catch (Exception ex)
            {
                OutPutImportLog(string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0015, Environment.NewLine, ex.Message));
                WriteEventLog(g_CON_LEVEL_ERROR, string.Format("{0}{1}{2}", g_clsMessageInfo.strMsgE0029, Environment.NewLine, ex.Message));
                return false;
            }
        }
        #endregion
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
            if (string.IsNullOrEmpty(strCheckData))
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
            if (string.IsNullOrEmpty(strCheckData))
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
                                            + "の範囲で設定してください。 "
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
            if (string.IsNullOrEmpty(strCheckData))
            {
                return true;
            }

            int intCheckData = 0;

            try
            {
                intCheckData = NulltoInt(strCheckData);
            }
            catch (Exception ex)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目　"
                                            + strItemName + "の値が不正です。"
                                            + "数値を設定してください。 "
                                            + strTextLine);
                return false;
            }

            // 桁数チェック
            if (intCheckData < intMinRange || intCheckData > intMaxRange)
            {
                // ログファイルにエラー出力を行う
                OutPutImportLog(intRowCount + "行目　"
                                            + strItemName + "の値が不正です。"
                                            + intMinRange.ToString() + "～" + intMaxRange.ToString()
                                            + "の範囲で設定してください。 "
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
            string strOutPutFilePath = string.Empty;
            string time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            // 出力ファイル設定
            strOutPutFilePath = Path.Combine(g_clsSystemSettingInfo.strLogFileOutputDirectory,
                                                                                  m_CON_OUTLOGFILE_NAME + ".csv");

            try
            {
                //Shift JISで書き込む
                //書き込むファイルが既に存在している場合は、上書きする
                using (StreamWriter sw = new StreamWriter(strOutPutFilePath
                                                        , m_bolAppendFlag
                                                        , Encoding.GetEncoding("shift_jis")))
                {
                    // １行ずつ出力を行う
                    sw.WriteLine(time + "," + strLogText + "," + m_strErrorOutFileName);
                }
            }
            catch (Exception ex)
            {
                // ログファイル結果出力を行う
                WriteEventLog(g_CON_LEVEL_ERROR, g_clsMessageInfo.strMsgE0016 + Environment.NewLine + ex.Message);
                MessageBox.Show(g_clsMessageInfo.strMsgE0055, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                m_bolProcEnd = true;
                return;
            }

            // ログ出力を追記モードに変更する
            m_bolAppendFlag = true;
        }
        #endregion
    }
}