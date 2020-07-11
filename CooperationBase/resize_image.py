# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能305 画像リサイズ
# ----------------------------------------

import configparser
import sys
from multiprocessing import Pool
import multiprocessing as multi
import os
from PIL import Image
from PIL import ImageFile
import traceback
import datetime
import logging.config
import win32_setctime

import db_util
import error_detail
import error_util
import file_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_resize_image.conf",disable_existing_loggers=False)

# 画像リサイズ設定ファイル読込
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/resize_image_config.ini', 'SJIS')
# 共通設定ファイル読込
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

ImageFile.LOAD_TRUNCATED_IMAGES = True


# ------------------------------------------------------------------------------------
# 処理名             ：リサイズ処理
#
# 処理概要           ：1.対象の画像を開いて指定サイズにリサイズして保存する
#
# 引数               ：撮像画像パス
#                      撮像画像格納フォルダパス
#                      幅
#                      高さ
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def resize_image(file, input_dir, width, height):
    try:
        # 撮像画像ファイルパスから、画像ファイル名を取得する。
        file_name = os.path.basename(file)
        ctime = datetime.datetime.fromtimestamp(os.path.getctime(file)).timestamp()


        # 撮像画像を開く。
        img = Image.open(file)

        # 撮像画像を、幅740、高さ648にリサイズをする。
        rimg = img.resize((width, height), Image.BICUBIC)

        # リサイズした画像を保存する。
        rimg.save(input_dir + "\\" + file_name)

        win32_setctime.setctime(file, ctime)

        result = True
        return result

    except Exception as e:

        raise e


# ------------------------------------------------------------------------------------
# 処理名             ：画像リサイズ引数処理
#
# 処理概要           ：1.画像リサイズ処理に対して、引数を渡す
#
# 引数               ：画像リサイズ引数リスト
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def wrapper_resize_image(args):
    # 画像リサイズ処理へ引数を渡す

    resize_image(*args)


# ------------------------------------------------------------------------------------
# 処理名             ：画像フォルダ特定
#
# 処理概要           ：1.リサイズを行う画像を特定する
#
# 引数               ：撮像画像格納パス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像画像リスト
# ------------------------------------------------------------------------------------
def specific_image(image_file_path, image_pattern, logger):
    func_name = sys._getframe().f_code.co_name
    # リサイズ対象画像リストを取得する。
    result, file_list, error = file_util.get_file_list(image_file_path, image_pattern, logger, app_id, app_name)

    return result, file_list, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：並行実行処理
#
# 処理概要           ：1.マルチプロセスを作成し、処理を並行実行させる
#
# 引数               ：撮像画像格納パス
#                      撮像画像リスト
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def parallel_execution_process(image_file_path, image_file_list, logger):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        # 設定ファイルから、リサイズする幅、高さの値を取得する。
        width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        # マルチプロセスに渡す引数を準備する。
        resize_image_args = [[file, image_file_path, width, height]
                             for file in image_file_list]
        logger.debug('[%s:%s] マルチプロセス引数準備' % (app_id, app_name))

        # 実行するマルチプロセス数をCPU数から判断する。
        #p = Pool(multi.cpu_count())
        p = Pool(2)

        # 並行実行処理を実行する。
        logger.debug('[%s:%s] 並列処理実行' % (app_id, app_name))
        p.map(wrapper_resize_image, resize_image_args)

        # マルチプロセスを閉じる。
        p.close()

        result = True

    except Exception as error:
        # エラー詳細判定を行う
        error_detail.exception(error, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス更新
#
# 処理概要           ：1.リサイズ開始・完了時に処理ステータスを更新する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      処理ID
#                      更新カラム名
#                      更新時刻
#                      RAPIDサーバーホスト名
#                      ステータス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_processing_status(conn, cur, fabric_name, inspection_num,processing_id, column_name, now_datetime,
                             rapid_host_name, status, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    # クエリを作成する
    sql = 'UPDATE processing_status SET status=%s, %s =\'%s\' ' \
          'WHERE fabric_name=\'%s\' AND inspection_num = %s AND processing_id=%s AND rapid_host_name=\'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column_name, now_datetime, fabric_name, inspection_num, processing_id,
           rapid_host_name, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 処理ステータス更新SQL %s' % (app_id, app_name, sql))
    # 処理ステータス（リサイズ完了）を更新する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.対象画像を特定し、リサイズ処理を行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      処理ID
#                      RAPIDサーバーホスト名
#                      撮像画像格納パス
#                      AIモデル未検査フラグ
#                      ロガー
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def main(conn, cur, fabric_name, inspection_num, processed_id, rapid_host_name, separate_image_path, ai_model_flag,
         date, imaging_starttime, unit_num):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    ### ログ設定
    logger_name = "resize_image_" + str(rapid_host_name)
    logger = logging.getLogger(logger_name)
    try:

        # 設定ファイルからの値取得
        resize_start_column = inifile.get('COLUMN', 'resize_start')
        resize_end_column = inifile.get('COLUMN', 'resize_end')
        resize_start_status = common_inifile.get('PROCESSING_STATUS', 'resize_start')
        resize_end_status = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        rapid_model = common_inifile.get('PROCESSING_STATUS', 'rapid_model')
        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')

        # 変数設定

        logger.info('[%s:%s] %s機能を起動します。' % (app_id, app_name, app_name))
        logger.info(
            '[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s] ' 
            % (app_id, app_name, app_name, fabric_name, inspection_num, date))
        logger.debug('[%s:%s] リサイズ対象の撮像画像特定を開始します。' % (app_id, app_name))

        image_file_name = "\\*" + fabric_name + "_" + date + "_" + str(inspection_num).zfill((2)) + image_file_pattern

        # リサイズ画像を特定する。
        result, image_file_list, error, func_name = specific_image(separate_image_path, image_file_name, logger)

        if result:
            logger.debug('[%s:%s] リサイズ対象の撮像画像特定が終了しました。' % (app_id, app_name))
            logger.debug('[%s:%s] リサイズ対象の撮像画像 = [%s]' % (app_id, app_name, image_file_list))

        else:
            logger.debug('[%s:%s] リサイズ対象の撮像画像特定に失敗しました。' % (app_id, app_name))
            sys.exit()


        logger.debug('[%s:%s] 処理ステータス（リサイズ開始）の更新を開始しました。' % (app_id, app_name))

        # 処理ステータステーブルのステータスを更新する（リサイズ開始）
        now_datetime = datetime.datetime.now()
        result, error, conn, cur, func_name = \
            update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_start_column,
                                     now_datetime, rapid_host_name, resize_start_status, imaging_starttime, unit_num,
                                     logger)
        if result:
            logger.debug('[%s:%s] 処理ステータス（リサイズ開始）の更新が終了しました。' % (app_id, app_name))
            conn.commit()

        else:
            logger.debug('[%s:%s] 処理ステータス（リサイズ開始）の更新に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] リサイズ処理の並列実行を開始します。' % (app_id, app_name))

        # 画像リサイズ処理を並行実行する。
        result, error, func_name = parallel_execution_process(separate_image_path, image_file_list, logger)

        if result:
            logger.debug('[%s:%s] リサイズ処理の並列実行が終了しました。' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] リサイズ処理の並列実行が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] 処理ステータス（リサイズ完了）の更新を開始しました。' % (app_id, app_name))

        if ai_model_flag == 0:
            now_datetime = datetime.datetime.now()
            # 処理ステータステーブルのステータスを更新する（リサイズ完了）
            result, error, conn, cur, func_name = \
                update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_end_column,
                                         now_datetime, rapid_host_name, resize_end_status, imaging_starttime,
                                         unit_num, logger)
        else:
            # 処理ステータステーブルのステータスを更新する（学習用画像リサイズ完了）
            now_datetime = datetime.datetime.now()
            result, error, conn, cur, func_name = \
                update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_end_column,
                                         now_datetime, rapid_host_name, rapid_model, imaging_starttime, unit_num,
                                         logger)

        if result:
            logger.debug('[%s:%s] 処理ステータス（リサイズ完了）の更新が終了しました。' % (app_id, app_name))
            conn.commit()

        else:
            logger.debug('[%s:%s] 処理ステータス（リサイズ完了）の更新に失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] %s処理は正常に終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s] ' 
            % (app_id, app_name, app_name, fabric_name, inspection_num, date))

        result = True
        return result, conn, cur, error, func_name

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

        logger.debug('[%s:%s] エラー詳細を取得します。' % (app_id, app_name))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)

        logger.error('[%s:%s] %s [ホスト名, エラーコード:%s, %s]' % (app_id, app_name, error_message, rapid_host_name, error_id))

        event_log_message = '[機能名, ホスト名, エラーコード]=[%s, %s, %s] %s' % (app_name, rapid_host_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message)

        result = False

    except Exception as error:
        # 想定外エラー発生
        logger.error('[%s:%s] 予期しないエラーが発生しました。' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, conn, cur, error, func_name
