﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace UserMasterMaintenance
{
    static class Common
    {
        public const int CON_EDITMODE_REG = 1;
        public const int CON_EDITMODE_UPD = 2;

        public const string NAME_SEPARATE = "　";

        public const string CON_DB_INFO = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";

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
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UserMasterMaintenance());
        }
    }
}