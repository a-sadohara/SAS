# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能  DB運用機能
# ----------------------------------------
import configparser
import datetime
import logging.config
import sys
import traceback
from dateutil.relativedelta import relativedelta

import db_util
import error_detail
import error_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_operate_db.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")


db_inifile = configparser.ConfigParser()
db_inifile.read('D:/CI/programs/config/db_config.ini', 'SJIS')

common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

common_ope_inifile = configparser.ConfigParser()
common_ope_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/operate_db_config.ini', 'SJIS')

app_id =inifile.get('APP', 'app_id')
app_name =inifile.get('APP', 'app_name')

def create_connection():
    # DBに接続する。
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


def confirm_inspection_info(limit_month, conn, cur):
    result = False
    try:
        logger.info('[%s:%s] 検査情報ヘッダテーブル情報取得を開始します。' % (app_id, app_name))
        sql = 'select insert_datetime, fabric_name, inspection_num, branch_num, start_datetime, unit_num from inspection_info_header ' \
              'where insert_datetime <= \'%s\'' % limit_month
        tmp_result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] 検査情報ヘッダテーブル情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 検査情報ヘッダテーブル情報取得が失敗しました。' % (app_id, app_name))
            return result, conn, cur
        if len(select_result) != 0:
            logger.info('[%s:%s] 対象の検査情報ヘッダテーブルのレコード削除を開始します。' % (app_id, app_name))
            for i in range(len(select_result)):
                insert_datetime = select_result[i][0].strftime('%Y-%m-%d %H:%M:%S')
                fabric_name = select_result[i][1]
                inspection_num = select_result[i][2]
                branch_num = select_result[i][3]
                start_datetime = select_result[i][4]
                unit_num = select_result[i][5]

                sql = 'delete from inspection_info_header where fabric_name = \'%s\' and inspection_num = %s ' \
                      'and branch_num = %s and start_datetime = \'%s\' and unit_num = \'%s\'' % (fabric_name, inspection_num, branch_num, start_datetime, unit_num)
                tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
                if tmp_result:
                    logger.info('[%s:%s] 対象の検査情報ヘッダテーブルのレコード削除が終了しました。 [反番=%s] [検査番号=%s] '
                                '[枝番=%s] [登録日時=%s]' % (app_id, app_name, fabric_name, inspection_num,
                                                       branch_num, insert_datetime))
                    conn.commit()
                    result = True
                else:
                    logger.info('[%s:%s] 対象の検査情報ヘッダテーブルのレコード削除が失敗しました。[反番=%s] [検査番号=%s] '
                                '[枝番=%s] [登録日時=%s]' % (app_id, app_name, fabric_name, inspection_num,
                                                       branch_num, insert_datetime))
                    conn.rollback()
                    return result, conn, cur

        else:
            logger.info('[%s:%s] 検査情報ヘッダテーブルに削除対象のデータは存在しません。' % (app_id, app_name))
            result = True

    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


def confirm_regimark_info(limit_month, conn, cur):
    result = False
    try:
        logger.info('[%s:%s] レジマーク情報テーブル情報取得を開始します。' % (app_id, app_name))
        sql = 'select insert_datetime, fabric_name, inspection_num, line_num, face, imaging_starttime, unit_num from regimark_info ' \
              'where insert_datetime <= \'%s\'' %  limit_month
        tmp_result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] レジマーク情報テーブル情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] レジマーク情報テーブル情報取得が失敗しました。' % (app_id, app_name))
            return result, conn, cur
        if len(select_result) != 0:
            logger.info('[%s:%s] 対象のレジマーク情報テーブルのレコードの削除を開始します。' % (app_id, app_name))
            for i in range(len(select_result)):
                insert_datetime = select_result[i][0].strftime('%Y-%m-%d %H:%M:%S')
                fabric_name = select_result[i][1]
                inspection_num = select_result[i][2]
                line_num = select_result[i][3]
                face = select_result[i][4]
                imaging_starttime = select_result[i][5]
                unit_num = select_result[i][6]

                sql = 'delete from regimark_info where fabric_name = \'%s\' and inspection_num = %s ' \
                      'and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
                      % (fabric_name, inspection_num, line_num, face, imaging_starttime, unit_num)
                tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
                if tmp_result:
                    logger.info('[%s:%s] 対象のレジマーク情報テーブルのレコード削除が終了しました。 [反番=%s] [検査番号=%s] '
                                '[行番号=%s] [検反部=%s] [登録日時=%s]' % (app_id, app_name, fabric_name, inspection_num,
                                                                 line_num, face, insert_datetime))
                    conn.commit()
                    result = True
                else:
                    logger.info('[%s:%s] 対象のレジマーク情報テーブルのレコード削除が失敗しました。 [反番=%s] [検査番号=%s] '
                                '[行番号=%s] [検反部=%s] [登録日時=%s]' % (app_id, app_name, fabric_name, inspection_num,
                                                                 line_num, face, insert_datetime))
                    conn.rollback()
                    return result, conn, cur

        else:
            logger.info('[%s:%s] レジマーク情報テーブルに削除対象のデータは存在しません。' % (app_id, app_name))
            result = True
    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


def confirm_processing_status(limit_month, conn, cur):
    result = False
    try:
        logger.info('[%s:%s] 処理ステータステーブル情報取得を開始します。' % (app_id, app_name))
        sql = 'select insert_datetime, fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime, unit_num from processing_status ' \
              'where insert_datetime <= \'%s\'' % limit_month
        tmp_result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] 処理ステータステーブル情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 処理ステータステーブル情報取得が失敗しました。' % (app_id, app_name))
            return result, conn, cur
        if len(select_result) != 0:
            logger.info('[%s:%s] 対象の処理ステータステーブルのレコード削除を開始します。' % (app_id, app_name))
            for i in range(len(select_result)):

                insert_datetime = select_result[i][0].strftime('%Y-%m-%d %H:%M:%S')
                fabric_name = select_result[i][1]
                inspection_num = select_result[i][2]
                processing_id = select_result[i][3]
                rapid_host_name = select_result[i][4]
                imaging_starttime = select_result[i][5]
                unit_num = select_result[i][6]

                sql = 'delete from processing_status where fabric_name = \'%s\' and inspection_num = %s ' \
                      'and processing_id = %s and rapid_host_name = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
                      % (fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime, unit_num)
                tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
                if tmp_result:
                    logger.info('[%s:%s] 対象の処理ステータステーブルのレコード削除が終了しました。 [反番=%s] [検査番号=%s] '
                                '[処理ID=%s] [RAPIDサーバーホスト名=%s] [登録日時=%s]'
                                % (app_id, app_name, fabric_name, inspection_num, processing_id, rapid_host_name,
                                   insert_datetime))
                    conn.commit()
                    result = True
                else:
                    logger.error('[%s:%s] 対象の処理ステータステーブルのレコード削除が失敗しました。 [反番=%s] [検査番号=%s] '
                                 '[処理ID=%s] [RAPIDサーバーホスト名=%s] [登録日時=%s]'
                                 % (app_id, app_name, fabric_name, inspection_num, processing_id, rapid_host_name,
                                    insert_datetime))
                    conn.rollback()
                    return result, conn, cur
        else:
            logger.info('[%s:%s] 処理ステータステーブルに削除対象のデータは存在しません。' % (app_id, app_name))
            result = True
    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


def confirm_rapid_table(limit_month, conn, cur):
    result = False
    try:
        db_name = inifile.get('DB', 'db_name')
        logger.info('[%s:%s] RAPID解析情報テーブル情報取得を開始します。' % (app_id, app_name))
        sql = 'SELECT datid FROM pg_stat_database  where datname like \'%s\'' % db_name
        tmp_result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
        dat_id = select_result[0]

        sql = 'select relname, (pg_stat_file(\'./base/%s\' || \'/\' || relfilenode::text)).creation FROM pg_class ' \
              'WHERE relkind LIKE \'r\' AND relfilenode <> 0 AND relname like \'%s\'' % (dat_id, 'rapid%')

        tmp_result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] RAPID解析情報テーブル情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] RAPID解析情報テーブル情報取得が終了しました。' % (app_id, app_name))
            return result, conn, cur

        if len(select_result) != 0:
            
            for i in range(len(select_result)):
                rel_name = select_result[i][0]
                create_datetime = select_result[i][1].strftime('%Y-%m-%d %H:%M:%S')
                tcreate_datetime = datetime.datetime.strptime(create_datetime, '%Y-%m-%d %H:%M:%S')

                if tcreate_datetime <= limit_month:
                    logger.info('[%s:%s] 対象のRAPID解析情報テーブルの削除を開始します。' % (app_id, app_name))
                    sql = 'drop table "%s"' % rel_name
                    tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
                    if tmp_result:
                        logger.info('[%s:%s] 対象のRAPID解析情報テーブル削除が終了しました。 [テーブル名=%s] [登録日時=%s]'
                                    % (app_id, app_name, rel_name, create_datetime))
                        conn.commit()
                        result = True
                    else:
                        logger.error('[%s:%s] 対象のRAPID解析情報テーブル削除が失敗しました。 [テーブル名=%s] [登録日時=%s]'
                                     % (app_id, app_name, rel_name, create_datetime))
                        conn.rollback()
                        return result, conn, cur
                else:
                    logger.info('[%s:%s] 削除対象のRAPID解析情報テーブルは存在しません。' % (app_id, app_name))
                    result = True

        else:
            logger.info('[%s:%s] 削除対象のRAPID解析情報テーブルは存在しません。' % (app_id, app_name))
            result = True
    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


def confirm_fabric_info(limit_month, conn, cur):
    result = False
    try:
        logger.info('[%s:%s] 反物情報テーブル情報取得を開始します。' % (app_id, app_name))
        sql = 'select insert_datetime, fabric_name, inspection_num, imaging_starttime, unit_num from fabric_info ' \
              'where insert_datetime <= \'%s\'' % limit_month
        tmp_result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] 反物情報テーブル情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 反物情報テーブル情報取得が失敗しました。' % (app_id, app_name))
            return result
        if len(select_result) != 0:
            
            for i in range(len(select_result)):

                insert_datetime = select_result[i][0].strftime('%Y-%m-%d %H:%M:%S')
                fabric_name = select_result[i][1]
                inspection_num = select_result[i][2]
                imaging_starttime = select_result[i][3]
                unit_num = select_result[i][4]
                
                logger.info('[%s:%s] 対象の反物情報テーブルのレコード削除を開始します。[反番=%s] [検査番号=%s] [登録日時=%s] '
                            % (app_id, app_name, fabric_name, inspection_num, insert_datetime))
                sql = 'delete from fabric_info where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
                      % (fabric_name, inspection_num, imaging_starttime, unit_num)
                tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
                if tmp_result:
                    logger.info('[%s:%s] 対象の反物情報テーブルのレコード削除が終了しました。' % (app_id, app_name))
                    conn.commit()
                    result = True
                else:
                    logger.error('[%s:%s] 対象の反物情報テーブルのレコード削除が失敗しました。' % (app_id, app_name))
                    conn.rollback()
                    return result, conn, cur

        else:
            logger.info('[%s:%s] 反物情報テーブルに削除対象のデータは存在しません。' % (app_id, app_name))
            result = True

    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur



def close_connection(conn, cur):
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result

def main():
    error_file_name = None
    conn = None
    cur = None

    try:

        db_name = db_inifile.get('DB_INFO', 'dbname')
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        limit_month_num = int(common_ope_inifile.get('OPE_LIMIT', 'month'))
        now_date = datetime.datetime.today()
        limit_month = now_date - relativedelta(months=limit_month_num)



        logger.info('[%s:%s] %sを起動します。' % (app_id, app_name, app_name))

        logger.info('[%s:%s] DB接続を開始します。' % (app_id, app_name))
        result, conn, cur = create_connection()
        if result:
            logger.info('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
        else:
            logger.info('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] 検査情報ヘッダテーブルの確認を開始します。' % (app_id, app_name))
        # 検査情報ヘッダテーブルを確認
        result, conn, cur = confirm_inspection_info(limit_month, conn, cur)
        if result:
            logger.info('[%s:%s] 検査情報ヘッダテーブルの確認が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 検査情報ヘッダテーブルの確認に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] レジマーク情報テーブルの確認を開始します。' % (app_id, app_name))
        # レジマーク情報テーブルを確認
        result, conn, cur = confirm_regimark_info(limit_month, conn, cur)
        if result:
            logger.info('[%s:%s] レジマーク情報テーブルの確認が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] レジマーク情報テーブルの確認に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] 処理ステータステーブルの確認を開始します。' % (app_id, app_name))
        # 処理ステータステーブルを確認
        result, conn, cur = confirm_processing_status(limit_month, conn, cur)
        if result:
            logger.info('[%s:%s] 処理ステータステーブルの確認が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 処理ステータステーブルの確認に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] RAPID解析情報テーブルの確認を開始します。' % (app_id, app_name))
        # RAPID解析情報テーブルを確認
        result, conn, cur = confirm_rapid_table(limit_month, conn, cur)
        if result:
            logger.info('[%s:%s] RAPID解析情報テーブルの確認が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] RAPID解析情報テーブルの確認に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] 反物情報テーブルの確認を開始します。' % (app_id, app_name))
        # 反物情報テーブルを確認
        result, conn, cur = confirm_fabric_info(limit_month, conn, cur)
        if result:
            logger.info('[%s:%s] 反物情報テーブルの確認が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 反物情報テーブルの確認に失敗しました。' % (app_id, app_name))
            sys.exit()



    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)
        #logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        #result = error_util.common_execute(error_file_name, logger, app_id, app_name)

        #if result:
        #    logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        #else:
        #    logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
        #    logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        #logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))
        #result = error_util.common_execute(error_file_name, logger, app_id, app_name)

        #if result:
        #    logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        #else:
        #    logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
        #    logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

    finally:

        if conn is not None:
            # DB接続済の際はクローズする
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)
        else:
            # DB未接続の際は何もしない
            pass


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    main()
