# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能304  処理単位分割機能
# ----------------------------------------

import sys
from concurrent.futures.process import ProcessPoolExecutor
import configparser
import logging.config
import datetime
import time
import traceback
import math
import win32_setctime
import os
import logging.handlers

import error_detail
import file_util
import db_util
import error_util
import resize_image
import custom_handler
import light_patlite

logging.handlers.CustomTimedRotatingFileHandler = custom_handler.ParallelTimedRotatingFileHandler

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_separate_process_unit.conf", disable_existing_loggers=False)
logger = logging.getLogger("separate_process_unit")

# 機能304設定読込
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/separate_process_unit.ini', 'SJIS')
# 共通設定読込
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：DB接続
#
# 処理概要           ：1.DBに接続する。
#
# 引数               ：なし
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def create_connection():
    func_name = sys._getframe().f_code.co_name
    # DBに接続する。
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：検査情報取得
#
# 処理概要           ：1.反物情報テーブルから検査情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                       カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#                      検査情報
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, unit_num):
    func_name = sys._getframe().f_code.co_name
    #  クエリを作成する
    sql = 'select product_name, fabric_name, inspection_num, status, imaging_starttime from fabric_info ' \
          'where unit_num = \'%s\' and separateresize_endtime IS NULL and status != 0 order by imaging_starttime asc' \
          % (unit_num)

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから検査情報を取得する。
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, fabric_info, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：処理ID取得（最大値）
#
# 処理概要           ：1.反物情報テーブルから検査情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                       カーソルオブジェクト
#                       反番
#                       検査番号
#                       RAPIDサーバホスト名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#                      検査情報
# ------------------------------------------------------------------------------------
def select_processing_status_info(conn, cur, fabric_name, inspection_num, rapid_hostname, imaging_starttime,
                                  unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    #  クエリを作成する
    sql = 'select processing_id from processing_status where fabric_name = \'%s\' and inspection_num = %s and ' \
          'rapid_host_name = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\' order by processing_id desc' \
          % (fabric_name, inspection_num, rapid_hostname, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 処理ID取得（最大値）SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから検査情報を取得する。
    result, processing_id, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, processing_id, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報ステータス更新
#
# 処理概要           ：1.反物情報テーブルのステータスを更新する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      ステータス
#                      更新カラム名
#                      更新時刻
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column, time, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # クエリを作成する
    sql = 'update fabric_info set status = %s, %s =\'%s\' where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, time, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 反物情報ステータス更新SQL %s' % (app_id, app_name, sql))
    # ステータスを更新する
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：AIモデル未検査フラグ取得
#
# 処理概要           ：1.品番登録情報テーブルからAIモデル未検査フラグを取得する。
#
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      AIモデル未検査フラグ
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
## UPD 20200716 NES 小野 START
#def select_aimodel_flag(conn, cur, product_name):
def select_aimodel_flag(conn, cur, product_name, fabric_name, inspection_num, inspection_starttime, unit_num):

    func_name = sys._getframe().f_code.co_name
    # クエリを作成する
    #sql = 'select ai_model_non_inspection_flg from mst_product_info where product_name = \'%s\'' % product_name
    sql = 'select ai_model_non_inspection_flg from inspection_info_header where product_name = \'%s\' and ' \
          'fabric_name = \'%s\' and inspection_num = %s and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, inspection_starttime, unit_num)
## UPD 20200716 NES 小野 END
    logger.debug('[%s:%s] AIモデル未検査フラグ取得SQL %s' % (app_id, app_name, sql))
    # 品番登録情報テーブルからAIモデル未検査フラグを取得する。
    result, product_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    if product_info:
        ai_model_flag = product_info[0]
    else:
        ai_model_flag = None
    return result, ai_model_flag, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル作成
#
# 処理概要           ：1.RAPID解析情報テーブルを作成する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def create_rapid_table(conn, cur, fabric_name, inspection_num, inspection_date):
    func_name = sys._getframe().f_code.co_name
    # RAPID解析情報テーブルを作成する。
    result, error, conn, cur = db_util.create_table(conn, cur, fabric_name, inspection_num, inspection_date,
                                                    logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             :画像ファイル監視
#
# 処理概要           ：1.撮像画像フォルダ内にある撮像画像リストを取得する
#
# 引数               ：撮像画像パス
#                      撮像画像ファイルパターン
#                      撮像画像リスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像画像リスト
# ------------------------------------------------------------------------------------
def monitor_image_file(image_path, file_pattern, logger):
    func_name = sys._getframe().f_code.co_name
    # 画像ファイルリストを取得する。
    result, file_list, error = file_util.get_file_list(image_path, file_pattern, logger, app_id, app_name)
    return result, file_list, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：画像分割
#
# 処理概要           ：1.100枚単位で処理IDを採番し、フォルダを作成
#                      2.作成したフォルダに撮像画像を移動させる
#
# 引数               ：撮像画像リスト
#                      移動先フォルダ
#                      処理済み枚数
#                      RAPIDサーバーホスト名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像画像リスト
#                      処理済み枚数
# ------------------------------------------------------------------------------------
def separate_image(file_list, output_path, file_num, rapid_host_name, logger, fabric_name, inspection_num, date, original_image_path):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        image_file = file_list[:file_num]
        i = 0
        # エラー判別メッセージ
        networkpath_error = inifile.get('VALUE', 'networkpath_error')
        permission_error = inifile.get('VALUE', 'permission_error')
        max_retry_num = int(inifile.get('VALUE', 'retry_num'))

        # TODO 無限ループにならないよう、失敗リストを保持し、それを超過したらエラーとする（ここから）
        # 中身は、dict（image_file, retry_num）で管理するイメージ
        ng_image_list = []
        # TODO 無限ループにならないよう、失敗リストを保持し、それを超過したらエラーとする（ここまで）

        logger.info('[%s:%s] 処理対象撮像画像 [撮像画像名] = [%s]' % (app_id, app_name, image_file))

        #  元画像保管フォルダを作成する。
        tmp_result, error = file_util.make_directory(original_image_path, logger, app_id, app_name)

        # ファイル分割閾値の数だけwhileループ内の処理を実行す
        while i < file_num:

            #  分割フォルダを作成する。
            tmp_result, error = file_util.make_directory(output_path, logger, app_id, app_name)

            if tmp_result:
                # 撮像画像を元画像保管フォルダにコピーする。
                ctime = datetime.datetime.fromtimestamp(os.path.getctime(image_file[i])).timestamp()
                tmp_result, error = file_util.copy_file(image_file[i], original_image_path, logger,
                                                        app_id, app_name)
                win32_setctime.setctime(original_image_path + '\\' + os.path.basename(image_file[i]), ctime)

                # 撮像画像を分割フォルダに移動する。
                ctime = datetime.datetime.fromtimestamp(os.path.getctime(image_file[i])).timestamp()
                tmp_result, error = file_util.move_file(image_file[i], output_path, logger,
                                                        app_id, app_name)
                win32_setctime.setctime(output_path + '\\' + os.path.basename(image_file[i]), ctime)

                # 処理結果 = Trueの場合、次の画像を移動する
                if tmp_result is True:
                    i += 1
                # 処理結果 = Falseの場合、処理失敗
                elif tmp_result == networkpath_error:
                    logger.error('[%s:%s] 画像分割に失敗しました。 '
                                 '[反番, 検査番号, 検査日付, 画像名]=[%s, %s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, date, image_file[i]))

                    return result, file_list, file_num
                # 上記以外の場合、パーミッションエラーとして対象画像をリストの最後に追加。
                elif tmp_result == permission_error:
                    # TODO 失敗リストの中でリトライ回数を調査している際にはエラー終了する。（ここから）
                    is_empty = True
                    for ng_image in ng_image_list:
                        if ng_image['image_file'] == image_file[i]:
                            # 一致するものが存在
                            is_empty = False
                            # リトライ回数の超過チェック
                            # MAX_RETRY_NUMは、本箇所でのMAXリトライ回数（設定ファイルに切り出した方が良いかも）
                            if ng_image['retry_num'] >= max_retry_num:
                                # 超過している場合はエラー
                                logger.error('[%s:%s] 画像分割に失敗しました。 '
                                             '[反番, 検査番号, 検査日付, 画像名]=[%s, %s, %s, %s]'
                                             % (app_id, app_name, fabric_name, inspection_num, date, image_file[i]))
                                return result, file_list, file_num
                            else:
                                # 超過していない場合はカウントアップ
                                cnt = ng_image['retry_num'] + 1
                                ng_image['retry_num'] = cnt
                                break
                    # 失敗リストへの追加
                    if is_empty:
                        # 失敗リストに無い場合は追加
                        tmp = {'image_file': image_file[i], 'retry_num': 1}
                        ng_image_list.append(tmp)
                    # TODO 失敗リストの中でリトライ回数を調査している際にはエラー終了する。（ここまで）

                    image_file.append(image_file[i])
                    del image_file[i]
                else:
                    file_num = 0
                    return result, file_list, file_num
            else:
                logger.error('[%s:%s] 分割フォルダ作成が失敗しました。 '
                             '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                             % (app_id, app_name, fabric_name, inspection_num, date))
                return result, file_list, file_num

        # ファイルリストから処理を行った枚数削除する
        del file_list[:file_num]

        result = True

    except Exception as error:
        # エラー詳細判定を行う
        error_detail.exception(error, logger, app_id, app_name)

    return result, file_list, file_num, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：画像枚数確認
#
# 処理概要           ：1.撮像画像リスト数を確認して、処理を分岐させる
#
# 引数               ：撮像画像リスト
#                      画像分割数閾値
#                      撮像完了時刻
#                      画像出力パス
#                      RAPIDサーバーホスト名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像画像リスト
#                      処理済み枚数
# ------------------------------------------------------------------------------------
def confirm_image_num(file_list, max_file, endtime, output_path, rapid_host_name, logger,
                      fabric_name, inspection_num, date, original_image_path):
    func_name = sys._getframe().f_code.co_name
    error = None
    # 撮像画像リストが画像分割数閾値以上の場合
    if len(file_list) >= max_file:

        # 画像分割処理を行う。
        separete_image_res = separate_image(file_list, output_path, max_file, rapid_host_name, logger,
                                            fabric_name, inspection_num, date, original_image_path)

    # 撮像画像リストが画像分割数閾値未満の場合、端数分を分割するかの判定を行う。
    else:
        # 撮像が完了していない場合、撮像未完了として処理は行わない。
        if endtime is None:

            result = True
            return result, file_list, 0, error, func_name

        # 撮像が完了している場合、端数分を処理する。
        else:

            separete_image_res = separate_image(file_list, output_path, len(file_list), rapid_host_name, logger,
                                                fabric_name, inspection_num, date, original_image_path)

    # 戻り値から、処理後の撮像画像リストと、処理済枚数を抽出する。
    result = separete_image_res[0]
    after_file_list = separete_image_res[1]
    file_num = separete_image_res[2]
    error = separete_image_res[3]
    func_name = separete_image_res[4]

    return result, after_file_list, file_num, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス登録
#
# 処理概要           ：1.処理ステータステーブルに品名、反番、検査番号、処理ID、ステータス、
#                         分割完了時刻、RAPIDサーバーホスト名を登録する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査番号
#                      処理ID
#                      RAPIDサーバーホスト名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def insert_processing_status(conn, cur, product_name, fabric_name, inspection_num,
                             processed_id, end_time, rapid_host_name, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    # 処理ステータス（撮像完了）
    status = common_inifile.get('PROCESSING_STATUS', 'separate_end')
    # クエリを作成する。
    sql = 'insert into processing_status (product_name, fabric_name, inspection_num, ' \
          'processing_id, status, split_endtime, rapid_host_name, imaging_starttime, unit_num) values ' \
          '(\'%s\', \'%s\', %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\')' \
          % (product_name, fabric_name, inspection_num, processed_id, status, end_time, rapid_host_name,
             imaging_starttime, unit_num)

    logger.debug('[%s:%s] 処理ステータス登録SQL %s' % (app_id, app_name, sql))
    # 処理ステータステーブルにデータを登録する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：処理単位分割マルチスレッド実行
#
# 処理概要           ：1.処理ステータステーブルに品名、反番、検査番号、処理ID、ステータス、
#                         分割完了時刻、RAPIDサーバーホスト名を登録する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査番号
#                      画像パス
#                      分割画像パス
#                      RAPIDサーバーホスト名
#                      処理済み枚数
#                      処理ID
#                      撮像完了時刻
#                      AIモデルフラグ
#                      ディレクトリ名
#                      撮像日時
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      処理済枚数
#                      処理ID
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def exec_separate_process_unit_multi_thread(product_name, fabric_name, inspection_num, image_dir,
                                            separate_image_dir, rapid_host_name, processed_num, processed_id,
                                            imaging_endtime, ai_model_flag, base_dir_name, date, imaging_starttime,
                                            unit_num):
    result = False
    conn = None
    cur = None
    file_list = 0
    error = None
    func_name = sys._getframe().f_code.co_name
    ### ログ設定
    logger_name = "separate_process_unit_" + str(rapid_host_name)
    logger_subprocess = logging.getLogger(logger_name)
    try:

        logger_subprocess.debug('[%s:%s] %sマルチスレッド処理を開始します。 ホスト名=[%s]' %
                                (app_id, app_name, app_name, rapid_host_name))

        max_file = int(inifile.get('VALUE', 'max_file'))
        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        al_model_hostname = inifile.get('RAPID_SERVER', 'al_model_hostname')

        # DB共通機能を呼び出して、DBに接続する。
        logger_subprocess.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
        tmp_result, error, conn, cur, func_name = create_connection()

        if tmp_result:
            logger_subprocess.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
        else:
            logger_subprocess.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

        # ログ設定読込
        logger_subprocess.info(
            '[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % (
                app_id, app_name, app_name, fabric_name, inspection_num, date))
        logger_subprocess.debug('[%s:%s] 撮像画像リストの取得を開始します。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))

        #  撮像画像監視フォルダを作成する。
        tmp_result, error = file_util.make_directory(image_dir, logger, app_id, app_name)
        # 取得する画像名を定義する。

        image_file_name = "\\*" + fabric_name + "_" + date + "_" + str(inspection_num).zfill((2)) + image_file_pattern
        # 撮像画像格納フォルダ内にある撮像画像のリストを取得する
        tmp_result, file_list, error, func_name = monitor_image_file(image_dir, image_file_name, logger_subprocess)

        if tmp_result:
            logger_subprocess.debug('[%s:%s] 撮像画像リストの取得が終了しました。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))
            pass
        else:
            logger_subprocess.error('[%s:%s] 撮像画像リストの取得が失敗しました。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            # 撮像画像リストが存在する場合、撮像画像の分割処理を行う。
        if len(file_list) > 0:
            # 処理ID最大値を取得する。
            logger_subprocess.debug('[%s:%s] 処理ID最大値の取得を開始します。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))
            tmp_result, max_processed_id, error, conn, cur, func_name = \
                select_processing_status_info(conn, cur, fabric_name, inspection_num, rapid_host_name,
                                              imaging_starttime, unit_num, logger_subprocess)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
                if max_processed_id is None:
                    pass
                else:
                    processed_id = int(max_processed_id[0]) + 1
                conn.commit()
            else:
                logger_subprocess.error('[%s:%s] 処理ID最大値の取得に失敗しました。', app_id, app_name)
                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            logger_subprocess.debug('[%s:%s] 撮像画像リストが存在します。 リスト数=[%s] ホスト名=[%s]'
                                    % (app_id, app_name, len(file_list), rapid_host_name))
            # 撮像画像リスト数を処理枚数閾値で割る
            file_length = (math.ceil(len(file_list) / max_file))
            for x in range(file_length):

                # 撮像画像リスト数を確認し、処理単位で撮像画像を分割する
                logger_subprocess.debug('[%s:%s] 撮像画像の分割を開始します。 処理ID=[%s] ホスト名=[%s]'
                                        % (app_id, app_name, processed_id, rapid_host_name))

                # 元画像保管パスの作成
                original_image_path = image_dir + "\\" + 'ORG' + "\\" + base_dir_name
                # 分割フォルダパスの作成
                separate_image_path = separate_image_dir + "\\" + str(processed_id)

                # 撮像枚数確認を行い、撮像画像を分割する。
                tmp_result, file_list, file_num, error, func_name = \
                    confirm_image_num(file_list, max_file, imaging_endtime, separate_image_path, rapid_host_name,
                                      logger_subprocess, fabric_name, inspection_num, date, original_image_path)

                end_time = datetime.datetime.now()

                # confirm_filenum（）の結果=Trueの場合
                # 撮像画像100枚以上 または 撮像画像100枚以下かつ撮像完了のため、後続処理を行う
                if tmp_result is True and file_num != 0:
                    logger_subprocess.debug('[%s:%s] 撮像画像の分割が終了しました。 処理ID=[%s] 処理枚数=[%s]  ホスト名=[%s]'
                                            % (app_id, app_name, processed_id, file_num, rapid_host_name))

                    logger_subprocess.debug('[%s:%s] 処理ステータスの登録を開始します。' % (app_id, app_name))
                    # 処理ステータステーブルにデータを登録する。
                    tmp_result, error, conn, cur, func_name = insert_processing_status(conn, cur, product_name,
                                                                                       fabric_name,
                                                                                       inspection_num, processed_id,
                                                                                       end_time,
                                                                                       rapid_host_name,
                                                                                       imaging_starttime, unit_num,
                                                                                       logger_subprocess)
                    if tmp_result:
                        logger_subprocess.debug('[%s:%s] 処理ステータスの登録が終了しました。 ホスト名=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                        conn.commit()
                    else:
                        logger_subprocess.error('[%s:%s] 処理ステータスの登録が失敗しました。 '
                                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                                % (app_id, app_name, fabric_name, inspection_num, date))
                        conn.rollback()
                        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    logger_subprocess.debug('[%s:%s] 画像リサイズ機能を開始します。 ホスト名=[%s]'
                                            % (app_id, app_name, rapid_host_name))
                    # リサイズ処理を呼び出して、画像のリサイズを行う
                    tmp_result, conn, cur, error, func_name = \
                        resize_image.main(conn, cur, fabric_name, inspection_num, processed_id, rapid_host_name,
                                          separate_image_path, ai_model_flag, date, imaging_starttime, unit_num)
                    if tmp_result:
                        logger_subprocess.debug('[%s:%s] 画像リサイズ機能が終了しました。 ホスト名=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                    else:
                        logger_subprocess.error('[%s:%s] 画像リサイズ機能が失敗しました。 '
                                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                                % (app_id, app_name, fabric_name, inspection_num, date))
                        func_name = '305main'
                        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    if ai_model_flag == 1:

                        move_file_pattern = "\\" + image_file_pattern
                        logger_subprocess.debug('[%s:%s] RAPID学習用画像リストの取得を開始します。 ホスト名=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                        tmp_result, move_file_list, error = \
                            file_util.get_file_list(separate_image_path, move_file_pattern,
                                                    logger_subprocess, app_id, app_name)

                        if tmp_result:
                            logger_subprocess.debug('[%s:%s] RAPID学習用画像リストの取得が終了しました。 ホスト名=[%s]'
                                                    % (app_id, app_name, rapid_host_name))
                            result_list = []
                            ai_model_path = "\\\\" + al_model_hostname + "\\" + base_dir_name + "\\"
                            logger_subprocess.debug('[%s:%s] RAPID学習用画像の移動を開始します。 ホスト名=[%s]'
                                                    % (app_id, app_name, rapid_host_name))
                            tmp_result, error = file_util.make_directory(ai_model_path, logger_subprocess, app_id,
                                                                         app_name)
                            if tmp_result:
                                pass
                            else:
                                logger_subprocess.error('[%s:%s] RAPID学習用画像の移動が失敗しました。 '
                                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                                        % (app_id, app_name, fabric_name, inspection_num, date))
                                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                            for i in range(len(move_file_list)):
                                tmp_result, error = file_util.move_file(move_file_list[i], ai_model_path,
                                                                        logger_subprocess, app_id, app_name)
                                result_list.append(tmp_result)

                            if False in result_list:
                                logger_subprocess.error('[%s:%s] RAPID学習用画像の移動が失敗しました。 '
                                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                                        % (app_id, app_name, fabric_name, inspection_num, date))
                                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                            else:
                                logger_subprocess.debug('[%s:%s] RAPID学習用画像の移動が終了しました。 ホスト名=[%s]'
                                                        % (app_id, app_name, rapid_host_name))
                        else:
                            logger_subprocess.error('[%s:%s] RAPID学習用画像リストの取得が失敗しました。 '
                                                    '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                                    % (app_id, app_name, fabric_name, inspection_num, date))
                            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                    else:
                        pass

                    # 処理ID加算、総処理済枚数に処理済枚数を加算する
                    processed_id += 1
                    processed_num += file_num

                    # 撮像未完了のため、端数分の処理は行わなかった場合
                elif tmp_result is True and file_num == 0:
                    logger_subprocess.info('[%s:%s] 撮像未完了のため、撮像画像の分割を終了しました。 [処理ID, 未処理枚数]=[%s, %s]'
                                           % (app_id, app_name, (processed_id - 1), len(file_list)))
                    processed_num += file_num
                    result = True
                    return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    # 処理結果=Falseの場合
                else:
                    logger_subprocess.error('[%s:%s] 撮像画像の分割が失敗しました。 '
                                            '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                            % (app_id, app_name, fabric_name, inspection_num, date))
                    processed_num += file_num
                    return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            result = True
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

        # 撮像画像リストが存在しない場合、処理を終了する
        else:
            logger_subprocess.info('[%s:%s] 撮像画像が存在しません。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % (
                app_id, app_name, fabric_name, inspection_num, date))
            result = True
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

    except:
        logger_subprocess.error('[%s:%s] 予期しないエラーが発生しました '
                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                % (app_id, app_name, fabric_name, inspection_num, date))
        logger_subprocess.error(traceback.format_exc())
        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

    finally:
        # DB接続済の際はクローズする
        logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
        close_connection(conn, cur)
        logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)


# ------------------------------------------------------------------------------------
# 処理名             ：処理済み枚数更新
#
# 処理概要           ：1.反物情報テーブルの処理済枚数を現在の総処理済枚数で更新する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      処理済枚数
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def update_processed_num(conn, cur, fabric_name, inspection_num, processed_num, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # クエリを作成する。
    sql = 'update fabric_info set processed_num = %s where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (processed_num, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 処理済み枚数更新SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルの処理済み枚数を更新する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報取得
#
# 処理概要           ：1.反物情報テーブルから総撮像枚数、撮像完了時刻を取得する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      反番
#                      検査番号
#
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      総撮像枚数
#                      撮像完了時刻
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # クエリを作成する。
    sql = 'select image_num, imaging_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 反物情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    if fabric_info is not None:
        # 取得結果から総撮像枚数と撮像完了時刻を抽出する。
        image_num = fabric_info[0]
        imaging_endtime = fabric_info[1]
    else:
        image_num = imaging_endtime = None

    return result, image_num, imaging_endtime, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：処理完了確認
#
# 処理概要           ：1.一検査番号の処理完了判定を行う
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      総撮像枚数
#                      処理済み枚数
#                      撮像完了時刻
#                      AIモデル未検査フラグ
#                      処理完了確認フラグ
#
# 戻り値             ：処理結果
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def confirm_processed(conn, cur, fabric_name, inspection_num, image_num, processed_num, imaging_endtime, column, time,
                      ai_flag, confirm_errorflg, status, error_continue_num, imaging_starttime, unit_num,
                      file_list_num):
    func_name = sys._getframe().f_code.co_name
    error = None
    if imaging_endtime is None or file_list_num != 0:
        confirm_errorflg = 0
        result = 'continuance'
        return result, confirm_errorflg, conn, cur, error, func_name

    # 撮像画像枚数=処理済枚数 且つ 撮像完了時刻が登録されている場合、以下分岐に沿って反物情報テーブルの更新を行う。
    elif image_num == processed_num and imaging_endtime is not None:
        confirm_errorflg = 0

        # AIモデル未検査フラグが空の場合、反物情報テーブルのステータスを本番用で更新する。
        if ai_flag == 0:
            update_status = status[0]

            # 反物情報テーブルのステータスを更新する。
            result, error, conn, cur, tmp_func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                         update_status, column, time, imaging_starttime,
                                                                         unit_num)

            return result, confirm_errorflg, conn, cur, error, func_name
            # AIモデル未検査フラグが空の場合、反物情報テーブルのステータスを検査用で更新する。
        else:
            update_status = status[1]

            # 反物情報テーブルのステータスを更新する。
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

            return result, confirm_errorflg, conn, cur, error, func_name

    # 撮像画像≠処理済枚数 且つ 撮像完了時刻が登録されている場合、イベントログとパトライトを点滅させる。
    elif image_num != processed_num and imaging_endtime is not None:
        logger.warning('[%s:%s] 撮像枚数と処理済枚数が不一致です。 [撮像枚数, 処理済枚数]=[%s, %s]', app_id, app_name, image_num, processed_num)
        # AIモデル未検査フラグが空の場合、反物情報テーブルのステータスを本番用で更新する。
        if ai_flag == 0:
            update_status = status[0]

            # 反物情報テーブルのステータスを更新する。
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

            # AIモデル未検査フラグが空の場合、反物情報テーブルのステータスを検査用で更新する。
        else:
            update_status = status[1]

            # 反物情報テーブルのステータスを更新する。
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

        message = '撮像枚数と処理済枚数が不一致です。[撮像枚数, 処理済枚数]=[%s, %s]' % (image_num, processed_num)
        error_util.write_eventlog_warning(app_name, message)

        # # パトライト点灯
        # logger.debug('[%s:%s] パトライト点灯処理を開始します。', app_id, app_name)

        # light_pattern = '010000'
        # result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)

        # if result:
        #     logger.debug('[%s:%s] パトライト点灯処理が終了しました。', app_id, app_name)
        # else:
        #     logger.warning('[%s:%s] パトライト点灯処理に失敗しました。', app_id, app_name)

        return result, confirm_errorflg, conn, cur, error, func_name


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像フォルダ削除
#
# 処理概要           ：1.RAPID学習用画像を移動した際に、空のフォルダを削除する。
#
# 引数               ：削除パス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def delete_dir(path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.delete_dir(path, logger, app_id, app_name)
    return result, error, func_name


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
    func_name = sys._getframe().f_code.co_name
    # DB接続を切断する。
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 関数名             ：処理単位分割
#
# 処理概要           ：1.撮像画像を処理単位で分割し、リサイズ処理を行う。
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    error = None
    func_name = sys._getframe().f_code.co_name
    error_list = None
    try:

        # 設定ファイルから値を取得する。
        error_file_name = inifile.get('ERROR_FILE', 'file')
        image_dir = inifile.get('FILE_PATH', 'file_path')
        separate_image_dir = inifile.get('FILE_PATH', 'image_dir')
        patlite_name = inifile.get('PATLITE', 'patlite_name')
        confirm_errorflg = int(inifile.get('VALUE', 'confirm_errorflg'))
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))
        end_status = [common_inifile.get('FABRIC_STATUS', 'separate_resize_end'),
                      common_inifile.get('FABRIC_STATUS', 'rapid_model')]
        separate_resize_start = common_inifile.get('FABRIC_STATUS', 'separate_resize_start')
        start_column = inifile.get('COLUMN', 'start')
        end_column = inifile.get('COLUMN', 'end')
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        ip_address = common_inifile.get('RAPID_SERVER', 'ip_address')
        ip_address_list = ip_address.split(',')

        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        all_processed_num = 0
        processed_id = 1
        processed_id_list = [processed_id for i in range(thread_num)]

        logger.info('[%s:%s] %s機能が起動しました。' % (app_id, app_name, app_name))
        start_time = datetime.datetime.now()

        while True:

            # DBから検査情報を取得する
            logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
            tmp_result, error, conn, cur, func_name = create_connection()
            # 処理結果確認。Trueの場合、DB接続終了
            if tmp_result:
                logger.debug('[%s:%s] DB接続が終了します。' % (app_id, app_name))
                pass
            # Falseの場合、DB接続失敗
            else:
                logger.debug('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] 検査情報取得を開始します。' % (app_id, app_name))
            # 反物情報テーブルから検査情報を取得する
            result, fabric_info, error, conn, cur, func_name = select_inspection_info(conn, cur, unit_num)
            # 処理結果確認。Trueの場合、検査情報取得成功
            if result:
                logger.debug('[%s:%s] 検査情報取得を終了しました。' % (app_id, app_name))
                conn.commit()
            # Falseの場合、検査情報取得失敗
            else:
                logger.debug('[%s:%s] 検査情報取得が失敗しました。' % (app_id, app_name))
                conn.rollback()
                sys.exit()

            if fabric_info is not None:

                # 取得したデータを変数に代入する
                product_name, fabric_name, inspection_num, status, starttime = \
                    fabric_info[0], fabric_info[1], str(fabric_info[2]), fabric_info[3], fabric_info[4]
                logger.debug('[%s:%s] 検査情報が存在します。 [品名=%s] [反番=%s] [検査番号=%s]' %
                             (app_id, app_name, product_name, fabric_name, inspection_num))

                # 検査開始日時を取得し、撮像画像パスを作成する
                inspection_date = str(starttime.strftime('%Y%m%d'))
                base_dir_name = product_name + "_" + fabric_name + "_" + inspection_date + "_" + str(
                    inspection_num).zfill(
                    (2))
                separate_image_path = separate_image_dir + "\\" + base_dir_name
                separate_image_path_list = ["\\\\" + ip_address_list[i] + "\\" +
                                            separate_image_path for i in range(thread_num)]
                image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                  image_dir for i in range(thread_num)]

                # 反物情報テーブルのステータスを更新する。
                logger.debug('[%s:%s] 反物情報テーブルのステータス更新を開始します。' % (app_id, app_name))
                result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                         separate_resize_start, start_column,
                                                                         start_time, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 反物情報テーブルのステータス更新が終了しました。' % (app_id, app_name))
                    conn.commit()

                else:
                    logger.error('[%s:%s] 反物情報テーブルのステータス更新が失敗しました。 '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # 品番情報登録テーブルからAIモデル未検査フラグ取得する
                # 品番情報登録テーブル名と、AIモデル未検査フラグのカラム名に関しては後で最新化 TODO
                logger.debug('[%s:%s] AIモデル未検査フラグ取得を開始します。' % (app_id, app_name))
                ## UPD 20200716 NES 小野 START
                #result, ai_flag, error, conn, cur, func_name = select_aimodel_flag(conn, cur, product_name)
                inspection_starttime = starttime
                result, ai_flag, error, conn, cur, func_name = select_aimodel_flag(conn, cur, product_name, fabric_name,
                                                                                   inspection_num, inspection_starttime,
                                                                                   unit_num)
                ## UPD 20200716 NES 小野 START

                if result:
                    logger.debug('[%s:%s] AIモデル未検査フラグ取得が終了しました。 AIモデル未検査フラグ=[%s]' % (app_id, app_name, ai_flag))
                    conn.commit()
                else:
                    logger.error('[%s:%s] AIモデル未検査フラグ取得が失敗しました。 '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                if ai_flag == 0:
                    # RAPID解析情報テーブルを作成する
                    logger.debug('[%s:%s] RAPID解析情報テーブル作成を開始します。' % (app_id, app_name))
                    result, error, conn, cur, func_name = create_rapid_table(conn, cur, fabric_name, inspection_num,
                                                                             inspection_date)
                    if result:
                        logger.debug('[%s:%s] RAPID解析情報テーブル作成が終了しました。' % (app_id, app_name))
                        conn.commit()
                    else:
                        logger.error('[%s:%s] RAPID解析情報テーブル作成が失敗しました。 '
                                     '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                        conn.rollback()
                        sys.exit()
                else:
                    pass

                logger.debug('[%s:%s] 総撮像枚数、撮像完了時刻の取得を開始します。' % (app_id, app_name))
                result, image_num, imaging_endtime, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 総撮像枚数、撮像完了時刻の取得が終了しました。' % (app_id, app_name))
                    conn.commit()
                else:
                    logger.error('[%s:%s] 総撮像枚数、撮像完了時刻の取得が失敗しました。  '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # マルチスレッド用の変数定義
                result_list = []
                error_list = []
                conn_status = str(conn)
                processed_num = 0
                file_list_num = 0

                # マルチスレッド実行
                logger.info('[%s:%s] マルチプロセスで処理単位分割を開始します。 [反番, 検査番号,　検査日時]=[%s, %s, %s]' %
                            (app_id, app_name, fabric_name, inspection_num, inspection_date))
                with ProcessPoolExecutor() as executor:
                    func_list = []
                    for i in range(thread_num):
                        # スレッド実行
                        func_list.append(
                            executor.submit(
                                exec_separate_process_unit_multi_thread,
                                product_name, fabric_name, inspection_num, image_dir_list[i],
                                separate_image_path_list[i],
                                rapid_hostname_list[i], processed_num, processed_id_list[i], imaging_endtime,
                                ai_flag, base_dir_name, inspection_date, starttime, unit_num))
                    for i in range(thread_num):
                        # スレッド戻り値を取得
                        result_list.append(func_list[i].result())

                # マルチスレッド実行結果を確認する。
                for i, multi_result in enumerate(result_list):
                    # 処理結果=Trueの場合
                    if multi_result[0] is True:
                        host_name = multi_result[3]
                        logger.debug('[%s:%s] マルチスレッドでの処理単位分割が終了しました。 ホスト名=[%s]' %
                                     (app_id, app_name, host_name))
                        all_processed_num += multi_result[1]
                        processed_id_list[i] = multi_result[2]
                        file_list_num += len(multi_result[4])
                        conn_str = str(conn)
                        confirm_close = conn_str.split(',')[1]

                    # 処理結果=Falseの場合
                    else:
                        host_name = multi_result[3]
                        error = multi_result[5]
                        func_name = multi_result[6]
                        logger.error('[%s:%s] マルチスレッドでの処理単位分割が失敗しました。 '
                                     '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                        error_list.extend([host_name, error, func_name])
                        sys.exit()

                if len(error_list) > 0:
                    sys.exit()
                else:
                    pass
                logger.info('[%s:%s] マルチスレッドでの処理単位分割が終了しました。 '
                            '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                            % (app_id, app_name, fabric_name, inspection_num, inspection_date))

                conn_str = str(conn)
                confirm_close = conn_str.split(',')[1]

                logger.debug('[%s:%s] 分割処理済み枚数更新を開始します。' % (app_id, app_name))
                result, error, conn, cur, func_name = update_processed_num(conn, cur, fabric_name, inspection_num,
                                                                           all_processed_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 分割処理済み枚数更新が終了しました。', app_id, app_name)
                    conn.commit()
                else:
                    logger.error('[%s:%s] 分割処理済み枚数更新が失敗しました。 '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # 処理完了判定
                # 反物情報テーブルから、総撮像枚数、処理済枚数、撮像完了時刻を取得する
                logger.debug('[%s:%s] 総撮像枚数、撮像完了時刻の取得を開始します。' % (app_id, app_name))
                result, image_num, imaging_endtime, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 総撮像枚数、撮像完了時刻の取得が終了しました。' % (app_id, app_name))
                    conn.commit()
                else:
                    logger.error('[%s:%s] 総撮像枚数、撮像完了時刻の取得が失敗しました。 '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                end_time = datetime.datetime.now()

                logger.debug('[%s:%s] 処理完了判定を開始します。' % (app_id, app_name))
                # 処理完了判定を行う。
                result, confirm_errorflg, conn, cur, error, func_name = \
                    confirm_processed(conn, cur, fabric_name, inspection_num, image_num, all_processed_num,
                                      imaging_endtime, end_column, end_time, ai_flag, confirm_errorflg, end_status,
                                      error_continue_num, starttime, unit_num, file_list_num)

                # 処理完了判定結果=Trueの場合、1検査番号の処理単位分割は完了
                if result is True:
                    logger.debug('[%s:%s] 処理完了判定が終了しました。' % (app_id, app_name))
                    all_processed_num = 0

                    if ai_flag == 1:
                        # logger.debug('[%s:%s] 空フォルダを削除します。' % (app_id, app_name))
                        # for separate_path in separate_image_path_list:
                        #     result, error, func_name = delete_dir(separate_path)
                        #     if result:
                        #         logger.debug('[%s:%s] 空フォルダの削除が終了しました。 [フォルダパス=%s]' %
                        #                      (app_id, app_name, separate_path))
                        #     else:
                        #         logger.error('[%s:%s] 空フォルダの削除が失敗しました。  '
                        #                      '[反番, 検査番号, 検査日付, フォルダパス]=[%s, %s, %s, %s]'
                        #                      % (app_id, app_name, fabric_name, inspection_num, inspection_date,
                        #                         separate_path))
                        #         sys.exit()
                        # パトライト点灯させる
                        logger.debug('[%s:%s] パトライト点灯処理を開始します。', app_id, app_name)
                        light_pattern = '001000'
                        result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)
                        if result:
                            logger.debug('[%s:%s] パトライト点灯処理が終了しました。', app_id, app_name)
                        else:
                            logger.warning('[%s:%s] パトライト点灯処理に失敗しました。', app_id, app_name)

                    else:
                        pass

                    logger.info('[%s:%s] %s処理は正常に終了しました。 '
                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                % (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                    conn.commit()
                    processed_id_list = [processed_id for i in range(len(processed_id_list))]
                    confirm_errorflg = 0

                # 処理完了判定結果=処理継続の場合
                elif result == 'continuance':
                    logger.debug('[%s:%s] 処理完了判定が終了しました。' % (app_id, app_name))
                    logger.info('[%s:%s] 撮像が未完了のため、 %s処理を終了します。 '
                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                % (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                    close_connection(conn, cur)
                    logger.debug('[%s:%s] DB接続の切断が終了しました。' % (app_id, app_name))

                    time.sleep(sleep_time)
                    continue

                # 処理完了判定結果=Falseの場合、処理完了判定で失敗
                else:
                    logger.error('[%s:%s] 処理完了判定が失敗しました。 '
                                 '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # DB接続を切断する
                logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                close_connection(conn, cur)
                logger.debug('[%s:%s] DB接続の切断が終了しました。' % (app_id, app_name))

                time.sleep(sleep_time)
                continue

            else:
                logger.info('[%s:%s] 検査情報が存在しません。' % (app_id, app_name))
                logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                close_connection(conn, cur)
                logger.debug('[%s:%s] DB接続の切断が終了しました。' % (app_id, app_name))

                all_processed_num = 0

                time.sleep(sleep_time)
                continue


    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

        logger.debug('[%s:%s] エラー詳細を取得します。' % (app_id, app_name))

        if error_list > 0:
            for list in error_list:
                error_message, error_id = error_detail.get_error_message(list[1], app_id, list[2])
                logger.error('[%s:%s] %s [ホスト名, エラーコード]=[%s, %s]' % (app_id, app_name, error_message, list[0], error_id))

                event_log_message = '[機能名, ホスト名, エラーコード]=[%s, %s, %s] %s' % (app_name, list[0], error_id, error_message)
                error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)
        else:
            error_message, error_id = error_detail.get_error_message(error, app_id, func_name)
            logger.error('[%s:%s] %s [エラーコード:%s]' % (app_id, app_name, error_message, error_id))

            event_log_message = '[機能名, エラーコード]=[%s, %s] %s' % (app_name, error_id, error_message)
            error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))

        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

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
    import multiprocessing

    multiprocessing.freeze_support()
    main()
