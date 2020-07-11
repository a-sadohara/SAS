# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能 ステータス情報更新
# ----------------------------------------

import configparser
import logging.config
import sys
import traceback

import db_util
import error_detail
import error_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_update_status.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/update_status_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 関数名             ：DB接続
#
# 処理概要           ：1.DBと接続する
#
# 引数               ：機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def create_connection():
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur

# ------------------------------------------------------------------------------------
# 処理名             ：反物情報テーブルステータス更新
#
# 処理概要           ：1.反物情報テーブルのステータスを更新する。
#
# 引数               ：ステータス
#                      反番
#                      検査番号
#                      撮像開始日（YYYY/MM/DD形式）
#                      号機
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info_status(status, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):

    fabric_imaging_starttime = inifile.get('COLUMN', 'fabric_imaging_starttime')

    ### クエリを作成する
    sql = 'update fabric_info set status = %s ,imaging_endtime = now(), ' \
          'separateresize_starttime = (case when separateresize_starttime is Null then now() else separateresize_starttime END), '\
          'separateresize_endtime = (case when separateresize_endtime is Null then now() else separateresize_endtime END), '\
          'rapid_starttime = (case when rapid_starttime is Null then now() else rapid_starttime END), ' \
          'rapid_endtime = (case when rapid_endtime is Null then now() else rapid_endtime END), '\
          'imageprocessing_starttime = (case when imageprocessing_starttime is Null then now() else imageprocessing_starttime END), '\
          'imageprocessing_endtime = (case when imageprocessing_endtime is Null then now() else imageprocessing_endtime END), '\
          'ng_starttime = (case when ng_starttime is Null then now() else ng_starttime END), '\
          'ng_endtime = (case when ng_endtime is Null then now() else ng_endtime END), '\
          'ng_ziptrans_starttime = (case when ng_ziptrans_starttime is Null then now() else ng_ziptrans_starttime END), '\
          'ng_ziptrans_endtime = (case when ng_ziptrans_endtime is Null then now() else ng_ziptrans_endtime END) '\
          'where fabric_name = \'%s\' and inspection_num = %s and %s::date = \'%s\' and unit_num = \'%s\''  \
          % (status, fabric_name, inspection_num, fabric_imaging_starttime, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur

# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータステーブルステータス更新
#
# 処理概要           ：1.処理ステータステーブルのステータスを更新する。
#
# 引数               ：処理ステータス
#                     版番
#                     検査番号
#                      撮像開始時刻（YYYY/MM/DD形式）
#                      号機
#                     カーソルオブジェクト
#                     コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_processing_status(status, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):

    processing_imaging_starttime = inifile.get('COLUMN', 'processing_imaging_starttime')
    ### クエリを作成する
    sql = 'update processing_status set status = %s ' \
          '  where fabric_name = \'%s\' and inspection_num = %s and %s::date = \'%s\' and unit_num = \'%s\''  \
          % (status, fabric_name, inspection_num, processing_imaging_starttime, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブルステータス更新
#
# 処理概要           ：1.RAPID解析情報ステーブルのステータスを更新する。
#
# 引数               ：RAPID解析結果
#                     端判定結果
#                     マスキング判定結果
#                     版番
#                     検査番号
#                      撮像開始時刻（YYYY/MM/DD形式）
#                      号機
#                     カーソルオブジェクト
#                     コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_rapid_analysis_info(rapid_result, edge_result, masking_result, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):
    target_date = imaging_starttime.replace('/', '')
    ### クエリを作成する
    sql = 'update "rapid_%s_%s_%s" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          '  where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\''  \
          % (fabric_name, inspection_num, target_date, rapid_result, edge_result, masking_result, fabric_name, inspection_num, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur

# ------------------------------------------------------------------------------------
# 関数名             ：DB切断
#
# 処理概要           ：1.DBとの接続を切断する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def close_connection(conn, cur):
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 関数名             ：日付文字列変換
#
# 処理概要           ：1.入力された日付文字列を、YYYY/MM/DD形式の文字列に変換する。
#
# 引数               ：日付文字列
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    日付文字列（YYYY/MM/DD）
# ------------------------------------------------------------------------------------
def conv_date(imaging_starttime):
    result = False
    conv_date = None
    dates = []
    try:
        # YYYY-MM-DDをYYYY/MM/DDに変換、かつ、分割する
        dates = imaging_starttime.replace('-', '/').split('/')
        if len(dates) == 3:
            # YYYYとMMとYYが分割できた場合
            yyyy = dates[0]
            mm = dates[1]
            dd = dates[2]
        elif len(dates) == 1 and len(imaging_starttime) == 8:
            # YYYYMMDD形式の場合
            yyyy = imaging_starttime[:4]
            mm = imaging_starttime[4:6]
            dd = imaging_starttime[6:]
        # 日付文字列（YYYY/MM/DD）に変換する。
        conv_date = '%04d/%02d/%02d' % (int(yyyy), int(mm), int(dd))
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
    return result, conv_date

# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.ステータス情報更新機能を行う。
#
# 引数               ：反番
#                    検査番号
#                    検査日
#                    号機（数字のみ）
#
# 戻り値             ：処理結果（0:正常、1:失敗）
# ------------------------------------------------------------------------------------
def main(fabric_name, inspection_num, inspection_date, unit_num):
    # 変数定義
    result = False
    error_file_name = None
    conn = None
    cur = None

    try:
        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # 更新ステータス：反物情報テーブル.ステータス
        fabric_status = inifile.get('FABRIC_INFO', 'status')
        # 更新ステータス：処理ステータステーブル.ステータス
        processing_status = inifile.get('PROCESSING_STATUS', 'status')
        # 更新ステータス：RAPID解析情報テーブル.RAPID解析結果
        rapid_result = inifile.get('RAPID_ANALYSIS_INFO', 'rapid_result')
        # 更新ステータス：RAPID解析情報テーブル.端判定結果
        edge_result = inifile.get('RAPID_ANALYSIS_INFO', 'edge_result')
        # 更新ステータス：RAPID解析情報テーブル.マスキング判定結果
        masking_result = inifile.get('RAPID_ANALYSIS_INFO', 'masking_result')
        # 号機の接頭辞
        unit_prefix = inifile.get('UNIT_INFO', 'unit_prefix')

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        # 日付形式変換
        logger.debug('[%s:%s] 日付形式変換を開始します。', app_id, app_name)
        tmp_result, format_inspection_date = conv_date(inspection_date)
        if tmp_result:
            logger.debug('[%s:%s] 日付形式変換が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 日付形式変換にエラーが発生しました。', app_id, app_name)
            sys.exit()

        # 号機
        format_unit_num = unit_prefix + str(unit_num)

        ### 各種ステータス更新処理
        logger.info('[%s:%s] %s処理を開始します。:[反番,検査番号,検査日,号機]=[%s, %s, %s, %s]',
                    app_id, app_name, app_name, fabric_name, inspection_num, format_inspection_date, format_unit_num)


        # DB共通処理を呼び出して、DB接続を行う
        logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
        tmp_result, conn, cur = create_connection()

        if tmp_result:
            logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
            sys.exit()

        # 反物情報テーブルのステータス更新
        logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。', app_id, app_name)
        tmp_result, conn, cur = update_fabric_info_status(fabric_status, fabric_name, inspection_num,
                                                          format_inspection_date, format_unit_num, cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] 反物情報テーブルの更新が終了しました。', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] 反物情報テーブルの更新に失敗しました。', app_id, app_name)
            conn.rollback()
            sys.exit()

        # 処理ステータステーブルのステータス更新
        logger.debug('[%s:%s] 処理ステータステーブルの更新を開始します。', app_id, app_name)
        tmp_result, conn, cur = update_processing_status(processing_status, fabric_name, inspection_num,
                                                         format_inspection_date, format_unit_num, cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] 処理ステータステーブルの更新が終了しました。', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] 処理ステータステーブルの更新に失敗しました。', app_id, app_name)
            conn.rollback()

        # RAPID解析情報テーブルのステータス更新
        logger.debug('[%s:%s] RAPID解析情報テーブルの更新を開始します。', app_id, app_name)
        tmp_result, conn, cur = update_rapid_analysis_info(rapid_result, edge_result, masking_result, fabric_name,
                                                           inspection_num, format_inspection_date, format_unit_num,
                                                           cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] RAPID解析情報テーブルの更新が終了しました。', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] RAPID解析情報テーブルの更新に失敗しました。', app_id, app_name)
            conn.rollback()

        conn.commit()
        logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)

        result = True

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))

    finally:
        if conn is not None:
            # DB接続済の際はクローズする
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)
        else:
            # DB未接続の際は何もしない
            pass

    # 戻り値設定
    # 呼出側（バッチプログラム側）で戻り値判定（ERRORLEVEL）する際の戻り値を設定する。
    if result:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    fabric_name = None
    inspection_num = None
    inspection_date = None
    unit_num = None
    args = sys.argv
    if len(args) > 4:
        fabric_name = args[1]
        inspection_num = args[2]
        inspection_date = args[3]
        unit_num = args[4]
    main(fabric_name, inspection_num, inspection_date, unit_num)
