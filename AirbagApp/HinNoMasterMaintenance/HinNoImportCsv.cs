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

namespace HinNoMasterMaintenance
{
    public partial class HinNoImportCsv : Form
    {
        #region 変数・定数
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

        /// <summary>
        /// 品番情報CSV
        /// </summary>
        private class IniDataRegister
        {
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
            public string TempPoint1 { get; set; }
            public string TempSize1 { get; set; }
            public string TempFile2 { get; set; }
            public string TempPoint2 { get; set; }
            public string TempSize2 { get; set; }
            public string BasePoint1 { get; set; }
            public string BasePoint2 { get; set; }
            public string BasePoint3 { get; set; }
            public string BasePoint4 { get; set; }
            public string BasePoint5 { get; set; }
            public string Plus1 { get; set; }
            public string Plus2 { get; set; }
            public string Plus3 { get; set; }
            public string Plus4 { get; set; }
            public string Plus5 { get; set; }
            public string AreaMagX { get; set; }
            public string AreaMagY { get; set; }
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
        public HinNoImportCsv()
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
        private static void ImportRegisterIniData(FileInfo Inputfile) 
        {
            List<string> lstRegisterIniData = new List<string>();

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
                        lstRegisterIniData.Add(strFileTextLine);

                    }
                }

                // セクションの数だけループする
                foreach (string strRegister in lstRegisterIniData) 
                {
                    IniDataRegister idrCurrentData = new IniDataRegister();

                    // 各項目の値を取得する
                    // FieldInfoを取得する
                    Type typeOfMyStruct = typeof(IniDataRegister);
                    System.Reflection.FieldInfo[] fieldInfos = typeOfMyStruct.GetFields();

                    // 各フィールドに値を設定する
                    foreach (var fieldInfo in fieldInfos)
                    {
                        fieldInfo.SetValue(idrCurrentData
                                         , GetIniValue(Inputfile.FullName, strRegister, GetName(() => fieldInfo.Name)));
                    }

                    // 読み込んだ値をDBに登録する

                }
            }    
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
        #endregion
    }
}
