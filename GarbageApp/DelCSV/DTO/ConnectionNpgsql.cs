using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace DelCSV
{
    class ConnectionNpgsql
    {
        // 接続文字列
        private static string m_strConnectionString;
        // DBコネクションオブジェクト
        public static NpgsqlConnection NpgsqlCon;
        // DBトランザクションオブジェクト
        public static NpgsqlTransaction NpgsqlTran;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="strConnectionString">接続文字列</param>
        public ConnectionNpgsql(string strConnectionString)
        {
            m_strConnectionString = strConnectionString;
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~ConnectionNpgsql()
        {
            DbClose();
        }

        /// <summary>
        /// DBオープン
        /// </summary>
        /// <example>既存のDBオープンが存在しない場合、DBオープンを実施する</example>
        public void DbOpen()
        {
            try
            {
                if (NpgsqlCon == null)
                {
                    NpgsqlCon = new NpgsqlConnection(m_strConnectionString);
                }

                if (NpgsqlCon.FullState != System.Data.ConnectionState.Open)
                {
                    NpgsqlCon.Open();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBクローズ
        /// </summary>
        /// <example>既存のDBオープンが存在しない場合、DBオープンを実施する</example>
        public void DbClose()
        {
            try
            {
                if (NpgsqlTran != null && NpgsqlTran.IsCompleted == false)
                {
                    DbRollback();
                }

                if (NpgsqlCon != null && NpgsqlCon.FullState == System.Data.ConnectionState.Open)
                {
                    NpgsqlCon.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (NpgsqlTran != null)
                {
                    NpgsqlTran.Dispose();
                    NpgsqlTran = null;
                }

                if (NpgsqlCon != null)
                {
                    NpgsqlCon.Dispose();
                    NpgsqlCon = null;
                }
            }
        }

        /// <summary>
        /// DBトランザクション開始
        /// </summary>
        public void DbBeginTran()
        {
            try
            {
                if (NpgsqlCon == null || NpgsqlCon.FullState != System.Data.ConnectionState.Open)
                {
                    DbOpen();
                }

                if (NpgsqlTran == null || NpgsqlTran.IsCompleted == true)
                {
                    NpgsqlTran = NpgsqlCon.BeginTransaction();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBコミット
        /// </summary>
        public void DbCommit()
        {
            try
            {
                if (NpgsqlTran != null && NpgsqlTran.IsCompleted == false)
                {
                    NpgsqlTran.Commit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DBロールバック
        /// </summary>
        public void DbRollback()
        {
            try
            {
                if (NpgsqlTran != null && NpgsqlTran.IsCompleted == false)
                {
                    NpgsqlTran.Rollback();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 抽出処理
        /// </summary>
        /// <param name="dtData">データテーブル</param>
        /// <param name="strSQL">SQL</param>
        /// <param name="lstNpgsqlCommand">コマンド配列</param>
        public void SelectSQL(ref DataTable dtData, string strSQL)
        {
            NpgsqlDataAdapter NpgsqlDtAd = null;
            try
            {
                DbOpen();

                NpgsqlDtAd = new NpgsqlDataAdapter(strSQL, NpgsqlCon);

                NpgsqlDtAd.Fill(dtData);
            }
            catch (NpgsqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (NpgsqlTran == null || NpgsqlTran.IsCompleted == true)
                {
                    DbClose();
                }
            }
        }
    }
}