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
//using Npgsql;
//using static HinNoMasterMaintenance.Common;

namespace HinNoMasterMaintenance
{
    public partial class UserImportCsv : Form
    {
        #region 変数・定数
        // CSVファイル配置情報
        public const int COL_USER_INFO_NO = 0;
        public const int COL_USER_INFO_NAME_SEI = 1;
        public const int COL_USER_INFO_NAME_MEI = 2;
        public const int COL_USER_INFO_KANA_SEI = 3;
        public const int COL_USER_INFO_KANA_MEI = 4;

        /// <summary>
        /// 作業者登録CSVファイル
        /// </summary>
        public class UserCsvInfo
        {
            public string strUserID { get; set; }
            public string strUserNameSei { get; set; }
            public string strUserNameMei { get; set; }
            public string strUserKanaSei { get; set; }
            public string strUserKanaMei { get; set; }
        }
        #endregion

        #region イベント
        /// <summary>
        /// 初期表示
        /// </summary>
        public UserImportCsv()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// CSVファイル選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CsvFile_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// CSV取り込みボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnImport_Click(object sender, EventArgs e)
        {
            //// 読み込みデータ
            //List<UserCsvInfo> lstUserData = new List<UserCsvInfo>();

            //// 未入力チェック
            //if (txtCsvFile.Text == "") 
            //{
            //    MessageBox.Show("CSVファイルを選択してください");
            //    btnCsvFile.Focus();
            //    return;
            //}

            if (MessageBox.Show("指定したCSVファイルを取込みます。\r\nよろしいですか？"
                              , "確認"
                              , MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //    // ファイル存在チェック
                //    string fileName = txtCsvFile.Text;
                //    if (System.IO.File.Exists(fileName) == false)
                //    {
                //        MessageBox.Show("指定されたcsvファイルが存在しません。" 
                //                       + Environment.NewLine 
                //                       + fileName);
                //        return;
                //    }

                //    // CSVファイル読み込み＆入力データチェックを行う
                //    if (ReadCsvData(fileName, out lstUserData) == false) 
                //    {
                //        return;
                //    }

                //    // 登録処理実施
                //    if (RegistrationUser(lstUserData) == true) 
                //    {
                MessageBox.Show("取込み処理が完了しました");
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
            //}
        }
        #endregion

        //#region メソッド
        ///// <summary>
        ///// CSVファイル読み込み
        ///// </summary>
        ///// <param name="strFileName">読み込むCSVファイル名</param>
        ///// <param name="lstUserData">CSVファイルデータ</param>
        ///// <returns></returns>
        //private Boolean ReadCsvData(string strFileName
        //                          , out List<UserCsvInfo> lstUserData) 
        //{
        //    int intRowCount = 0;

        //    // 読み込みデータ
        //    lstUserData = new List<UserCsvInfo>();

        //    // 選択ファイル読み込み
        //    using (StreamReader sr = new StreamReader(strFileName, Encoding.GetEncoding("Shift_JIS")))
        //    {
        //        // ストリームの末尾まで繰り返す
        //        while (!sr.EndOfStream)
        //        {
        //            UserCsvInfo uciUserData = new UserCsvInfo();

        //            intRowCount = intRowCount + 1;

        //            // マーカCSVファイルを１行読み込む
        //            string strFileTextLine = sr.ReadLine();
        //            if (strFileTextLine == ""　|| intRowCount == 1)
        //            {
        //                // ヘッダ行(1行目)または空行（最終行）の場合読み飛ばす
        //                continue;
        //            }

        //            // CSVを読み込む
        //            if (SetUserInfoCsv(strFileTextLine, out uciUserData) == false) 
        //            {
        //                MessageBox.Show(intRowCount + "行目の列数が不足しています" 
        //                              + Environment.NewLine 
        //                              + "データ：" + strFileTextLine);
        //                return false;
        //            }

        //            lstUserData.Add(uciUserData);
        //        }
        //    }

        //    if (lstUserData.Count == 0)
        //    {
        //        MessageBox.Show("CSVのデータが0件です");
        //        return false;
        //    }
        //    else 
        //    {
        //        // 入力データチェックを行う
        //        if (InputDataCheck(lstUserData) == false) 
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// ＣＳＶ→構造体格納（ユーザ情報CSV）
        ///// </summary>
        ///// <param name="strFileReadLine">読み込みＣＳＶ情報</param>
        ///// <returns></returns>
        //private static Boolean SetUserInfoCsv(string strFileReadLine
        //                                     ,out UserCsvInfo uciData)
        //{
        //    string[] stArrayData;

        //    uciData = new UserCsvInfo();

        //    // 半角スペース区切りで分割して配列に格納する
        //    stArrayData = strFileReadLine.Split(',');

        //    // 列数チェック
        //    if (stArrayData.Length <= COL_USER_INFO_KANA_MEI) 
        //    {
        //        return false;
        //    }

        //    // CSVの各項目を構造体へ格納する
        //    uciData.strUserID = stArrayData[COL_USER_INFO_NO];
        //    uciData.strUserNameSei = stArrayData[COL_USER_INFO_NAME_SEI];
        //    uciData.strUserNameMei = stArrayData[COL_USER_INFO_NAME_MEI];
        //    uciData.strUserKanaSei = stArrayData[COL_USER_INFO_KANA_SEI];
        //    uciData.strUserKanaMei = stArrayData[COL_USER_INFO_KANA_MEI];

        //    return true;
        //}

        ///// <summary>
        ///// /入力チェック
        ///// </summary>
        ///// <param name="lstUserData">読み込みユーザ情報リスト</param>
        ///// <returns></returns>
        //private Boolean InputDataCheck(List<UserCsvInfo> lstUserData)
        //{
        //    Int32 intRowCount = 2;
        //    Int32 intUserNo = 0;

        //    foreach (UserCsvInfo uciCheckData in lstUserData) 
        //    {

        //        // 必須入力チェック
        //        if (CheckRequiredInput(uciCheckData.strUserID, "社員番号", intRowCount, 4) == false ||
        //            CheckRequiredInput(uciCheckData.strUserNameSei, "作業者名 性", intRowCount, 10) == false ||
        //            CheckRequiredInput(uciCheckData.strUserNameMei, "作業者名 名", intRowCount, 10) == false ||
        //            CheckRequiredInput(uciCheckData.strUserKanaSei, "読み仮名 性", intRowCount, 10) == false ||
        //            CheckRequiredInput(uciCheckData.strUserKanaMei, "読み仮名 名", intRowCount, 10) == false)
        //        {
        //            return false;
        //        }

        //        // 数値入力チェック
        //        if (Int32.TryParse(uciCheckData.strUserID, out intUserNo) == false) 
        //        {
        //            MessageBox.Show(intRowCount + "行目の社員番号は数字のみ入力してください。");
        //            return false;
        //        }

        //        // カナ入力チェック
        //        if (Regex.IsMatch(uciCheckData.strUserKanaSei, "^[ァ-ヶー]*$") == false)
        //        {
        //            MessageBox.Show(intRowCount + "行目の読み仮名 性は全角カナのみ入力してください。");
        //            return false;
        //        }
        //        if (Regex.IsMatch(uciCheckData.strUserKanaMei, "^[ァ-ヶー]*$") == false)
        //        {
        //            MessageBox.Show(intRowCount + "行目の読み仮名 名は全角カナのみ入力してください。");
        //            return false;
        //        }

        //        intRowCount = intRowCount + 1;
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// 必須入力チェック
        ///// </summary>
        ///// <param name="strCheckData">チェック対象テキスト</param>
        ///// <param name="strItemName">チェック対象項目名</param>
        ///// <param name="intRowCount">チェック対象行番号</param>
        ///// <param name="intMaxLength">項目最大長</param>
        ///// <returns></returns>
        //private Boolean CheckRequiredInput(String strCheckData
        //                                 , String strItemName
        //                                 , Int32 intRowCount
        //                                 , Int32 intMaxLength)
        //{
        //    // 必須入力チェック
        //    if (strCheckData == "")
        //    {
        //        MessageBox.Show(intRowCount + "行目の" + strItemName + "は必須入力の項目です。");
        //        return false;
        //    }
        //    else if(strCheckData.Length > intMaxLength)
        //    {
        //        MessageBox.Show(intRowCount + "行目の" + strItemName + "が最大長を超えています。");
        //        return false;
        //    }


        //    return true;
        //}


        ///// <summary>
        ///// 登録処理
        ///// </summary>
        ///// <returns></returns>
        //private Boolean RegistrationUser(List<UserCsvInfo> lstUserData)
        //{
        //    try
        //    {
        //        if (bolModeNonDBCon == true)
        //            return true;

        //        // PostgreSQLへ接続
        //        using (NpgsqlConnection NpgsqlCon = new NpgsqlConnection(CON_DB_INFO))
        //        {
        //            NpgsqlCon.Open();

        //            using (var transaction = NpgsqlCon.BeginTransaction())
        //            {
        //                foreach (UserCsvInfo uciCheckData in lstUserData) 
        //                {
        //                    // SQL文を作成する
        //                    string strCreateSql = @"INSERT INTO SAGYOSYA (USERNO, USERNAME, USERYOMIGANA)
        //                                                          VALUES (:UserNo, :UserName, :UserYomigana)
        //                                                     ON CONFLICT (USERNO)
        //                                                    DO UPDATE SET USERNAME = :UserName
        //                                                                , USERYOMIGANA = :UserYomigana";

        //                    // SQLコマンドに各パラメータを設定する
        //                    var command = new NpgsqlCommand(strCreateSql, NpgsqlCon, transaction);

        //                    command.Parameters.Add(new NpgsqlParameter("UserNo", DbType.String) { Value = String.Format("{0:D4}", Int32.Parse(uciCheckData.strUserID)) });
        //                    command.Parameters.Add(new NpgsqlParameter("UserName", DbType.String) { Value = uciCheckData.strUserNameSei 
        //                                                                                                  + NAME_SEPARATE 
        //                                                                                                  + uciCheckData.strUserNameMei});
        //                    command.Parameters.Add(new NpgsqlParameter("UserYomigana", DbType.String) { Value = uciCheckData.strUserKanaSei 
        //                                                                                                      + NAME_SEPARATE 
        //                                                                                                      + uciCheckData.strUserKanaMei});

        //                    // sqlを実行する
        //                    if (ExecTranSQL(command, transaction) == false)
        //                    {
        //                        return false;
        //                    }
        //                }

        //                transaction.Commit();
        //            }
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("登録時にエラーが発生しました。"
        //                       + Environment.NewLine
        //                       + ex.Message);
        //        return false;
        //    }
        //}
        //#endregion    
    }
}
