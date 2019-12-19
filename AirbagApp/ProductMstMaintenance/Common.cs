using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using System.Windows.Forms;

namespace HinNoMasterMaintenance
{
    class Common
    {
        public const string g_CON_DB_INFO = "Server=192.168.2.17;Port=5432;User ID=postgres;Database=postgres;Password=password;Enlist=true";

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
        /// 文字列からの部分文字列の抽出を右から行います
        /// </summary>
        /// <param name="target">対象の文字列</param>
        /// <param name="length">部分文字列の長さ</param>
        /// <returns>文字列の右から抽出された部分文字列</returns>
        public static string SubstringRight(string target, int length)
        {
            return target.Substring(target.Length - length, length);
        }
    }
}
