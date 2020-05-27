# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能301  検査情報登録機能
# ----------------------------------------
import configparser
import logging.config
import sys
import time
import traceback

import db_util
import error_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_inspection_info.conf")
logger = logging.getLogger("register_inspection_info")
# 共通設定ファイル
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
# 検査情報登録設定ファイル
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_inspection_info_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 関数名             ：DB接続
#
# 処理概要           ：1.DBと接続する
#
# 引数               ：機能ID
#                     機能名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def create_connection():
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：検査情報取得
#
# 処理概要           ：1.検査情報テーブルから検査情報を取得する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：検査情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def select_inspection_data(conn, cur, unit_num):
    ### クエリを作成する
    sql = 'select inspection_info_header.product_name, inspection_info_header.fabric_name ,' \
          'inspection_info_header.inspection_num, ' \
          'inspection_info_header.start_datetime, inspection_info_header.unit_num FROM inspection_info_header left ' \
          'outer join fabric_info on ' \
          '(inspection_info_header.fabric_name = fabric_info.fabric_name and ' \
          'inspection_info_header.inspection_num = fabric_info.inspection_num and ' \
          'inspection_info_header.start_datetime = fabric_info.imaging_starttime) where ' \
          'inspection_info_header.unit_num = \'%s\' and fabric_info.fabric_name is null ' \
          'order by start_datetime asc' % unit_num

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))

    ### 検査情報テーブルからデータ取得
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：検査情報登録
#
# 処理概要           ：1.反物情報テーブルにデータを登録する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査番号
#                      開始時刻
#                      号機
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def insert_fabric_info(conn, cur, product_name, fabric_name, inspection_num, starttime, status, unit_no):
    ### クエリを作成する
    sql = 'insert into fabric_info (product_name, fabric_name, inspection_num, imaging_starttime, status, unit_num) ' \
          'values (\'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' % (product_name, fabric_name, inspection_num,
                                                                   starttime, status, unit_no)

    logger.debug('[%s:%s] 検査情報登録SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルへデータ登録
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：DB切断
#
# 処理概要           ：1.DBとの接続を切断する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def close_connection(conn, cur):
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)
    return result


# ------------------------------------------------------------------------------------
# 関数名             ：メイン処理
#
# 処理概要           ：1.検査情報テーブルから取得した検査情報を反物情報テーブルに登録する。
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    try:

        # 設定ファイルから取得
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        imaging_status = int(common_inifile.get('FABRIC_STATUS', 'imaging'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s機能を起動します' % (app_id, app_name, app_name))
        ###DB接続を行う
        # もし接続失敗した場合は、再度接続し直す。
        while True:

            logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
            # DBに接続する
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
                sys.exit()

            while True:

                logger.debug('[%s:%s] 検査情報取得を開始します。' % (app_id, app_name))
                # 検査情報テーブルから検査情報を取得する
                result, inspection_info, conn, cur = select_inspection_data(conn, cur, unit_num)

                if result:
                    logger.debug('[%s:%s] 検査情報取得が終了しました。' % (app_id, app_name))
                    logger.debug('[%s:%s] 検査情報=%s' % (app_id, app_name, inspection_info))
                    pass
                else:
                    logger.error('[%s:%s] 検査情報取得が失敗しました。' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                # 検査情報が存在するか確認する
                # 検査情報が存在する場合、検査情報登録処理を行う。
                if len(inspection_info) > 0:
                    logger.debug('[%s:%s] 検査情報が存在します。' % (app_id, app_name))
                    logger.info('[%s:%s] %s処理を開始します。' % (app_id, app_name, app_name))
                    # 取得した検査情報分、処理を行う

                    for i in range(len(inspection_info)):

                        # 取得した検査情報から品名、反番、検査番号、開始時刻、号機を抽出する。
                        product_name = inspection_info[i][0]
                        fabric_name = inspection_info[i][1]
                        inspection_num = inspection_info[i][2]
                        starttime = inspection_info[i][3]
                        unit_no = inspection_info[i][4]

                        inspection_date = str(starttime.strftime('%Y%m%d'))

                        logger.debug('[%s:%s] 検査情報 [品番=%s] [反番=%s] [検査番号=%s]' %
                                     (app_id, app_name, product_name, fabric_name, inspection_num))

                        logger.debug('[%s:%s] %s処理を開始します。 [反番, 検査番号]=[%s, %s]' %
                                     (app_id, app_name, app_name, fabric_name, inspection_num))

                        # 検査情報を反物情報テーブルに登録する
                        result, conn, cur = insert_fabric_info(conn, cur, product_name, fabric_name, inspection_num,
                                                               starttime, imaging_status, unit_no)
                        if result:
                            conn.commit()
                            logger.info('[%s:%s] 検査情報登録が終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        else:
                            logger.error('[%s:%s] 検査情報登録に失敗しました。 ' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                    logger.info('[%s:%s] %s処理は正常に終了しました。' % (app_id, app_name, app_name))

                # 検査情報が存在しない場合、ループを抜ける
                else:
                    logger.info('[%s:%s] 検査情報がありません。' % (app_id, app_name))
                    break

            # 検査情報が存在しないため、DB接続を切り、一定時間スリープしてから、再取得を行う。
            logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
            result = close_connection(conn, cur)

            if result:
                logger.debug('[%s:%s] DB接続の切断が完了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] DB接続の切断に失敗しました。' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] %s秒スリープします' % (app_id, app_name, sleep_time))
            time.sleep(sleep_time)
            continue

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によってプログラムを終了します。' % (app_id, app_name))

        logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))
    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました' % (app_id, app_name))
        logger.error(traceback.format_exc())

        logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))
    finally:
        if conn is not None:
            logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
            # DB接続済の際はクローズする
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断を終了しました。' % (app_id, app_name))
        else:
            # DB未接続の際は何もしない
            pass


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    main()
