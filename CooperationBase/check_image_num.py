# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能312  行間撮像枚数チェック
# ----------------------------------------
import codecs
import csv
import re
import sys
from concurrent.futures.process import ProcessPoolExecutor
import configparser
import logging.config
import time
import traceback
import os
import logging.handlers

import error_detail
import file_util
import db_util
import error_util
import custom_handler

logging.handlers.CustomTimedRotatingFileHandler = custom_handler.ParallelTimedRotatingFileHandler

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_check_image_num.conf", disable_existing_loggers=False)
logger = logging.getLogger("check_image_num")

# 機能304設定読込
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_image_num_config.ini', 'SJIS')
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
    sql = 'select fi.product_name, fi.fabric_name, fi.inspection_num, fi.imaging_starttime, ii.inspection_direction from '\
          'fabric_info as fi, inspection_info_header as ii ' \
          'where fi.unit_num = \'%s\' and fi.imaging_endtime IS NULL and fi.status != 0 and fi.product_name = ii.product_name and '\
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and '\
          'fi.imaging_starttime = ii.start_datetime order by imaging_starttime asc' \
          % (unit_num)

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから検査情報を取得する。
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, fabric_info, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# 処理名             ：検査行数取得
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
def select_inspection_line(conn, cur, product_name, fabric_name, inspection_num, inspection_date, unit_num):
    func_name = sys._getframe().f_code.co_name
    #  クエリを作成する
    sql = 'select inspection_start_line from inspection_info_header ' \
          'where unit_num = \'%s\' and product_name = \'%s\' and fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and cast(start_datetime as date) = \'%s\' and branch_num = 1' % (unit_num, product_name, fabric_name, inspection_num, inspection_date)

    logger.debug('[%s:%s] 検査行数取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから検査情報を取得する。
    result, inspection_start_line, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, inspection_start_line, error, conn, cur, func_name


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
    sql = 'select imaging_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 反物情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    if fabric_info is not None:
        # 取得結果から撮像完了時刻を抽出する。
        imaging_endtime = fabric_info[0]
    else:
        imaging_endtime = None

    return result, imaging_endtime, error, conn, cur, func_name

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
#                      ディレクトリ名
#                      撮像日時
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      処理済枚数
#                      処理ID
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def exec_check_image_num_multi_thread(product_name, fabric_name, inspection_num, image_dir,
                                            move_image_dir, rapid_host_name, inspection_date,
                                            after_tmp_file_info, line_info_index, last_flag):
    result = False
    check_image_list = None
    error = None
    func_name = sys._getframe().f_code.co_name
    ### ログ設定
    logger_name = "check_image_num_" + str(rapid_host_name)
    logger_subprocess = logging.getLogger(logger_name)
    try:

        logger_subprocess.debug('[%s:%s] %sマルチプロセス処理を開始します。 ホスト名=[%s]' %
                                (app_id, app_name, app_name, rapid_host_name))

        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        camera_key = 'cam_' + str(rapid_host_name[-4:]).lower()
        camera_num = common_inifile.get('CAMERA', camera_key)

        # ログ設定読込
        logger_subprocess.info(
            '[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % (
                app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
        logger_subprocess.debug('[%s:%s] 撮像画像リストの取得を開始します。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))

        # 取得する画像名を定義する。
        image_file_name = "\\*" + fabric_name + "_" + inspection_date + "_" + str(inspection_num).zfill((2)) + image_file_pattern
        # 撮像画像格納フォルダ内にある撮像画像のリストを取得する
        tmp_result, file_list, error, func_name = monitor_image_file(image_dir, image_file_name, logger_subprocess)

        if tmp_result:
            logger_subprocess.debug('[%s:%s] 撮像画像リストの取得が終了しました。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))
            logger_subprocess.debug('[%s:%s] 撮像画像リスト 撮像画像リスト=[%s] ホスト名=[%s]' % (app_id, app_name, file_list, rapid_host_name))
            pass
        else:
            logger_subprocess.error('[%s:%s] 撮像画像リストの取得が失敗しました。 ホスト名=[%s]' % (app_id, app_name, rapid_host_name))
            return result, rapid_host_name, file_list, error, func_name

        if last_flag == 0:
            if len(after_tmp_file_info) == 1 or line_info_index == 0:
                check_target_front_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index-1][0])[6])
                check_target_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[6])
                face = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[4])
                check_image_list = [x for x in file_list if int(re.split('[._]', x.split('\\')[-1])[6]) < check_target_line_image_num and int(re.split('[._]', x.split('\\')[-1])[4]) == face]
            else:
                check_target_front_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index-1][0])[6])
                check_target_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[6])
                face = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[4])
                check_image_list = [x for x in file_list if check_target_front_line_image_num <= int(re.split('[._]', x.split('\\')[-1])[6]) and
                                int(re.split('[._]', x.split('\\')[-1])[6]) < check_target_line_image_num and int(re.split('[._]', x.split('\\')[-1])[4]) == face]

            logger_subprocess.debug('[%s:%s] 行間内画像リスト取得。 画像リスト=[%s], ホスト名=[%s]' % (app_id, app_name, check_image_list, rapid_host_name))

            if len(check_image_list) < int(after_tmp_file_info[line_info_index][5]) * int(camera_num):
                result = 'image_shortage'
            else:
                result = True

            if len(check_image_list) == 0:
                result = True
                return result, rapid_host_name, check_image_list, error, func_name
            else:
                pass

            tmp_result, error, func_name = move_file(check_image_list, move_image_dir)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] 撮像画像の移動が終了しました。 ホスト名=[%s]'
                                        % (app_id, app_name, rapid_host_name))
            else:
                logger_subprocess.error('[%s:%s] 撮像画像の移動が失敗しました。 '
                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                raise Exception
        else:
            check_image_list = file_list
            if len(check_image_list) == 0:
                result = True
                return result, rapid_host_name, check_image_list, error, func_name
            else:
                pass

            tmp_result, error, func_name = move_file(file_list, move_image_dir)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] 撮像画像の移動が終了しました。 ホスト名=[%s]'
                                        % (app_id, app_name, rapid_host_name))
                result = True

            else:
                logger_subprocess.error('[%s:%s] 撮像画像の移動が失敗しました。 '
                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))


        return result, rapid_host_name, check_image_list, error, func_name

    except Exception as error:
        logger_subprocess.error('[%s:%s] 予期しないエラーが発生しました '
                                '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                % (app_id, app_name, fabric_name, inspection_num, inspection_date))
        logger_subprocess.error(traceback.format_exc())
        result = False
        return result, rapid_host_name, check_image_list, error, func_name

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
# 関数名             ：ファイル取得
#
# 処理概要           ：1.撮像完了通知ファイルのファイルリストを取得する。
#
# 引数               ：撮像完了通知ファイル格納フォルダパス
#                      撮像完了通知ファイル名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像完了通知ファイルリスト
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name, network_path_error, flag):
    result = False
    sorted_files = None
    error = None
    func_name = sys._getframe().f_code.co_name
    if flag == 'tmp':
        message = 'tmpファイル'
    elif flag == 'scan':
        message = '撮像完了通知ファイル'
    else:
        message = 'レジマーク読取結果ファイル'

    try:
        logger.debug('[%s:%s] %s格納フォルダパス=[%s]', app_id, app_name, message, file_path)
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result == True:
            # 成功時
            pass
        elif tmp_result == network_path_error:
            # 失敗時
            logger.debug("[%s:%s] %s格納フォルダにアクセス出来ません。", app_id, app_name, message)
            return tmp_result, sorted_files, error, func_name
        else:
            # 失敗時
            logger.error("[%s:%s] %s格納フォルダにアクセス出来ません。", app_id, app_name, message)
            return result, sorted_files, error, func_name

        # 取得したファイルパスをファイル名でソートする（古い順に処理するため）
        sorted_files = sorted(file_list)

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, sorted_files, error, func_name

# ------------------------------------------------------------------------------------
# 関数名             ：ファイル読込
#
# 処理概要           ：1.撮像完了通知ファイルを読込む
#
# 引数               ：撮像完了通知ファイルのファイルフルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像ファイル名
#                      種別
#                      座標幅
#                      座標高
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    regimark_info = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # レジマーク読取結果ファイルパスからファイル名を取得し、反番、検査番号を取得する
        # なお、ファイル名は「RM_品名_反番_検査番号_日付_検反部_レジマーク種別.CSV」を想定している

        # レジマーク読取結果ファイルから、項目を取得する
        with codecs.open(file, "r", "SJIS") as f:
            notification = [re.sub('\r', '', s[:-1]).split(',') for s in f.readlines()][1:]

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, notification, error, func_name

# ------------------------------------------------------------------------------------
# 処理名             ：撮像画像移動
#
# 処理概要           ：1.撮像画像を機能304で監視しているフォルダに移動する。
#
# 引数               ：撮像画像
#                      移動先フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    func_name = sys._getframe().f_code.co_name
    result = None
    error = None

    for file in target_file:
        # ファイル移動
        result, error = file_util.move_file(file, move_dir, logger, app_id, app_name)

    return result, error, func_name

# ------------------------------------------------------------------------------------
# 関数名             ：tmpファイル出力
#
# 処理概要           ：1.tmpファイルに出力する。
#
# 引数               ：出力先パス
#                      品番
#                      反番
#                      検査番号
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def write_checked_linenum_file(output_file_path, base_filename, inspection_direction, regimark_info, line_imagenum, tmp_files):
    result = False
    file_name = base_filename.replace('RM', 'LINECHECK_' + inspection_direction)
    tmp_file = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        if len(tmp_files) != 0:
            tmp_file = output_file_path + "\\" + file_name
            with codecs.open(tmp_file, "a", "SJIS") as f:
                for i in range(len(regimark_info)):
                    write_data = regimark_info[i][0] + ',' + regimark_info[i][1] + ',' + regimark_info[i][2] + ',' \
                                 + regimark_info[i][3] + ',' + regimark_info[i][4] + ',' + str(line_imagenum[i]) + ','
                    f.write(write_data)
                    f.write("\r\n")
        else:
            tmp_file = output_file_path + "\\" + file_name
            with codecs.open(tmp_file, "w", "SJIS") as f:
                f.write("撮像ファイル名,種別,座標幅,座標高,パルス,行間枚数,チェックフラグ")
                f.write("\r\n")
                for i in range(len(regimark_info)):
                    write_data = regimark_info[i][0] + ',' + regimark_info[i][1] + ',' + regimark_info[i][2] + ',' \
                                 + regimark_info[i][3] + ',' + regimark_info[i][4] + ',' + str(line_imagenum[i]) + ','
                    f.write(write_data)
                    f.write("\r\n")

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, tmp_file, error, func_name

# ------------------------------------------------------------------------------------
# 関数名             ：tmpファイル編集
#
# 処理概要           ：1.tmpファイルに出力する。
#
# 引数               ：出力先パス
#                      品番
#                      反番
#                      検査番号
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def edit_checked_linenum_file(file_name, line_info_index, after_line_info):
    result = False
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        print(after_line_info)
        for i in range(len(after_line_info)):
            if i == line_info_index:
                after_line_info[i][-1] = '1'
            else:
                pass

        with codecs.open(file_name, "w", "SJIS") as f:
            f.write("撮像ファイル名,種別,座標幅,座標高,パルス,行間枚数,チェックフラグ")
            f.write("\r\n")
            writer = csv.writer(f)
            writer.writerows(after_line_info)


        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, file_name, error, func_name

# ------------------------------------------------------------------------------------
# 関数名             ：行間撮像枚数チェック
#
# 処理概要           ：1.レジマークファイルを読み込む
#                     2.N行とN-1行の
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def confirm_line_between_imagenum(regimark_info, tmp_file, read_tmp_file):
    result = False
    line_imagenum = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        if len(regimark_info) == 1 and (len(tmp_file) == 0):
            image_num = int(re.split('[._]', regimark_info[0][0])[6])
            line_imagenum.append(image_num - 1)
            regimark_info = regimark_info
            result = True
        elif len(regimark_info) == len(read_tmp_file):
            line_imagenum = []
            regimark_info = []
        else:
            print("sss")
            checked_line = len(read_tmp_file)
            print(checked_line)
            for i in range(checked_line, len(regimark_info)):
                front_line = int(re.split('[._]',  regimark_info[i-1][0])[6])
                line = int(re.split('[._]',  regimark_info[i][0])[6])
                line_imagenum.append(line - front_line)

            regimark_info = regimark_info[checked_line:]
            result = True

        return result, regimark_info, line_imagenum, error, func_name
    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)
        return result, regimark_info, line_imagenum, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：出力先フォルダ存在チェック
#
# 処理概要           ：1.レジマーク間距離CSVを出力するフォルダが存在するかチェックする。
#                      2.フォルダが存在しない場合は作成する。
#
# 引数               ：出力先フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result, error, func_name

# ------------------------------------------------------------------------------------
# 関数名             ：行間撮像枚数チェック
#
# 処理概要           ：1.レジマークファイルを読み込む
#                     2.N行とN-1行の
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    scaninfo_file = None
    error = None
    func_name = sys._getframe().f_code.co_name
    error_list = None
    check_result = []
    try:

        # 設定ファイルから値を取得する。
        error_file_name = inifile.get('ERROR_FILE', 'file')
        image_dir = inifile.get('FILE_PATH', 'file_path')
        move_image_dir = inifile.get('FILE_PATH', 'move_dir')
        tmp_file_dir = inifile.get('FILE_PATH', 'tmp_file')
        input_path = common_inifile.get('FILE_PATH', 'input_path')
        input_dir_name = inifile.get('FILE_PATH', 'input_file_path')
        file_name_pattern = inifile.get('FILE_PATTREN', 'file_name_pattern')
        scan_input_dir_name = inifile.get('FILE_PATH', 'input_scan_file_path')
        scan_file_name_pattern = inifile.get('FILE_PATTREN', 'scan_file_name_pattern')
        file_extension_pattern = inifile.get('FILE_PATTREN', 'file_extension_pattern')
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        ip_address = common_inifile.get('RAPID_SERVER', 'ip_address')
        ip_address_list = ip_address.split(',')
        # ネットワークパスエラーメッセージ
        network_path_error = inifile.get('VALUE', 'networkpath_error')
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s機能が起動しました。' % (app_id, app_name, app_name))
        # DBへ接続する。
        logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
        tmp_result, error, conn, cur, func_name = create_connection()
        # Trueの場合、DB接続終了
        if tmp_result:
            logger.debug('[%s:%s] DB接続が終了します。' % (app_id, app_name))
            pass
        # Falseの場合、DB接続失敗
        else:
            logger.debug('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
            raise Exception

        # tmpファイルを出力するフォルダの存在を確認する
        logger.debug('[%s:%s] tmpファイル格納フォルダ存在チェックを開始します。', app_id, app_name)
        result, error, func_name = exists_dir(tmp_file_dir)

        if result:
            logger.debug('[%s:%s] tmpファイル格納フォルダ存在チェックが終了しました。', app_id, app_name)
            pass
        else:
            logger.error('[%s:%s] tmpファイル格納フォルダ存在チェックに失敗しました。:格納先フォルダ名[%s]',
                         app_id, app_name, tmp_file_dir)
            raise Exception

        # 検査中(チェック中)の検査情報を確認する。
        # フォルダ内にtmpファイルが存在するか確認する。
        tmp_file = None
        logger.debug('[%s:%s] tmpファイルの確認を開始します。', app_id, app_name)
        for i in range(error_continue_num):
            result, tmp_file, error, func_name = get_file(tmp_file_dir, '\\*.CSV', network_path_error, 'tmp')
            if result == True:
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] tmpファイルにアクセスできません。', app_id, app_name)
                message = 'tmpファイルにアクセスできません。'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] tmpファイルの確認に失敗しました。', app_id, app_name)
                raise Exception

        if len(tmp_file) == 0:
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
                raise Exception

            if fabric_info is not None:
                # 取得したデータを変数に代入する
                print(fabric_info)
                product_name = fabric_info[0]
                fabric_name = fabric_info[1]
                inspection_num = str(fabric_info[2])
                inspection_date = str(fabric_info[3].strftime('%Y%m%d'))
                inspection_direction = fabric_info[4]
                logger.debug('[%s:%s] 検査情報が存在します。 [品名=%s] [反番=%s] [検査番号=%s]' %
                             (app_id, app_name, product_name, fabric_name, inspection_num))
            else:
                logger.info('[%s:%s] チェック対象が存在しません。' % (app_id, app_name))
                return True, scaninfo_file, 'continue', func_name

        else:
            basename = os.path.basename(tmp_file[0])
            file_name = re.split('[_.]', basename)
            product_name = file_name[2]
            fabric_name = file_name[3]
            inspection_num = file_name[4]
            inspection_date = file_name[5]
            inspection_direction = file_name[1]
            # 取得したデータを変数に代入する
            logger.debug('[%s:%s] 行間枚数チェック中の検査情報が存在します。 [品名=%s] [反番=%s] [検査番号=%s]' %
                         (app_id, app_name, product_name, fabric_name, inspection_num))

        logger.debug('[%s:%s] 検査行数取得を開始します。' % (app_id, app_name))
        # 検査情報ヘッダーテーブルから検査開始行数を取得する
        result, inspection_start_line, error, conn, cur, func_name = select_inspection_line(conn, cur, product_name,
                                                                                            fabric_name, inspection_num,
                                                                                            inspection_date, unit_num)
        # 処理結果確認。Trueの場合、検査情報取得成功
        if result:
            logger.debug('[%s:%s] 検査行数取得を終了しました。' % (app_id, app_name))
            conn.commit()
        # Falseの場合、検査情報取得失敗
        else:
            logger.debug('[%s:%s] 検査行数取得が失敗しました。' % (app_id, app_name))
            conn.rollback()
            raise Exception

        join_file_name = [product_name, fabric_name, inspection_num, inspection_date]
        # レジマークファイルを読み込む
        input_file_path = input_path + '\\' + input_dir_name

        scan_input_file_path = input_path + '\\' + scan_input_dir_name
        scan_file_pattern = '\\' + scan_file_name_pattern + '_' + '_'.join(join_file_name) + '*' + file_extension_pattern

        # 撮像完了通知ファイル確認。
        logger.debug('[%s:%s] 撮像完了通知ファイルの確認を開始します。', app_id, app_name)
        for i in range(error_continue_num):
            result, scan_info, error, func_name = get_file(scan_input_file_path, scan_file_pattern,
                                                                network_path_error, 'scan')
            if result == True:
                logger.error('[%s:%s] 撮像完了通知ファイル %s', app_id, app_name, scan_info)
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] 撮像完了通知ファイルにアクセスできません。', app_id, app_name)
                message = '撮像完了通知ファイルにアクセスできません。'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] 撮像完了通知ファイルの確認に失敗しました。', app_id, app_name)
                raise Exception

        # 検査方向 S, X の場合、終了レジマークは読込不要
        if inspection_direction == 'S' or inspection_direction == 'X':
            file_pattern = '\\' + file_name_pattern + '_'  + '_'.join(join_file_name) + '_*1' + file_extension_pattern
        # 検査方向 Y, R の場合、終了レジマークは読込不要
        else:
            file_pattern = '\\' + file_name_pattern + '_' + '_'.join(join_file_name) + '_*2' + file_extension_pattern

        scaninfo_file = scan_info
        print(file_pattern)
        print('完了通知ファイル数', len(scan_info))
        logger.debug('[%s:%s] レジマークファイルの確認を開始します。', app_id, app_name)
        for i in range(error_continue_num):
            result, regimark_files, error, func_name = get_file(input_file_path, file_pattern, network_path_error, 'regi')
            if result == True and len(regimark_files) == 0:
                logger.info('[%s:%s] レジマーク読取結果ファイルが存在しません。', app_id, app_name)
                if len(scaninfo_file) != 0:
                    return True, scaninfo_file, 'end', func_name
                else:
                    return True, scaninfo_file, 'continue', func_name

            elif result == True and len(regimark_files) != 0:
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] レジマークファイルにアクセスできません。', app_id, app_name)
                message = 'レジマークファイルにアクセスできません。'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] レジマークファイルの確認に失敗しました。', app_id, app_name)
                raise Exception

        # レジマーク読取結果ファイルを読込む
        logger.debug('[%s:%s] レジマーク読取結果ファイルの読込を開始します。', app_id, app_name)

        for file in sorted(regimark_files):
            base_filename = os.path.basename(file)
            regimark_face = re.split('_', (file.split('\\'))[-1])[5]
            result, regimark_info, error, func_name = read_file(file)
            if result:
                logger.debug('[%s:%s] レジマーク読取結果ファイルの読込が終了しました。:レジマーク読取結果ファイルファイル名[%s]',
                             app_id, app_name, file)
                logger.debug('[%s:%s] レジマーク情報 : %s', app_id, app_name, regimark_info)
            else:
                logger.error('[%s:%s] レジマーク読取結果ファイルの読込に失敗しました。:レジマーク読取結果ファイルファイル名[%s]',
                             app_id, app_name, file)

            if len(tmp_file) == 0:
                tmp_file_info = []
                pass
            else:
                read_tmp_file = [x for x in tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
                result, tmp_file_info, error, func_name = read_file(read_tmp_file[0])
                if result:
                    logger.debug('[%s:%s] tmpファイルの読込が終了しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, file)
                    logger.debug('[%s:%s] tmpファイル内情報 : %s', app_id, app_name, tmp_file_info)
                else:
                    logger.error('[%s:%s] tmpファイルファイルの読込に失敗しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, file)

            result, regimark_info, line_imagenum ,error, func_name = confirm_line_between_imagenum(regimark_info, tmp_file, tmp_file_info)
            logger.debug('[%s:%s] 行間枚数チェック結果 %s %s', app_id, app_name, regimark_info, line_imagenum)

            # if len(regimark_info) == 0:
            #     pass
            # else:
            logger.debug('[%s:%s] tmpファイルの出力を開始します。', app_id, app_name)
            result, output_tmp_file, error, func_name = write_checked_linenum_file(tmp_file_dir, base_filename,
                                                                            inspection_direction, regimark_info, line_imagenum, tmp_file)
            if result:
                logger.debug('[%s:%s] tmpファイルの出力が終了しました。' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] tmpファイルの出力に失敗しました。' % (app_id, app_name))
                raise Exception

            logger.debug('[%s:%s] tmpファイルの確認を開始します。', app_id, app_name)
            for i in range(error_continue_num):
                result, after_tmp_file, error, func_name = get_file(tmp_file_dir, '\\*.CSV', network_path_error, 'tmp')
                if result == True:
                   break
                elif result == network_path_error:
                    logger.warning('[%s:%s] tmpファイルにアクセスできません。', app_id, app_name)
                    message = 'tmpファイルにアクセスできません。'
                    error_util.write_eventlog_warning(app_name, message)
                    time.sleep(sleep_time)
                    continue
                else:
                    logger.error('[%s:%s] tmpファイルの確認に失敗しました。', app_id, app_name)
                    raise Exception

            read_tmp_file = [x for x in after_tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
            result, after_tmp_file_info, error, func_name = read_file(read_tmp_file[0])
            if result:
                logger.debug('[%s:%s] tmpファイルの読込が終了しました。:レジマーク読取結果ファイルファイル名[%s]',
                             app_id, app_name, read_tmp_file[0])
                logger.debug('[%s:%s] tmpファイル内情報 : %s', app_id, app_name, after_tmp_file_info)
            else:
                logger.error('[%s:%s] tmpファイルの読込に失敗しました。:レジマーク読取結果ファイルファイル名[%s]',
                             app_id, app_name, read_tmp_file[0])

            for i in range(len(after_tmp_file_info)):
                if after_tmp_file_info[i][6] == '1':
                    continue
                else:
                    result_list = []
                    error_list = []
                    move_image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                                move_image_dir for i in range(thread_num)]
                    image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                      image_dir for i in range(thread_num)]

                    line_info_index = i
                    func_list = []

                    with ProcessPoolExecutor() as executor:
                        for j in range(thread_num):
                            # スレッド実行
                            func_list.append(
                                executor.submit(
                                    exec_check_image_num_multi_thread,
                                    product_name, fabric_name, inspection_num, image_dir_list[j],
                                    move_image_dir_list[j], rapid_hostname_list[j], inspection_date,
                                    after_tmp_file_info, line_info_index, 0))

                        for k in range(thread_num):
                            # スレッド戻り値を取得
                            result_list.append(func_list[k].result())
                            # マルチスレッド実行結果を確認する。

                        for l, multi_result in enumerate(result_list):
                            # 処理結果=Trueの場合
                            if multi_result[0] is True:
                                host_name = multi_result[1]
                                file_list = multi_result[2]
                                logger.debug('[%s:%s] マルチスレッドでの行間枚数チェックが終了しました。 ホスト名=[%s] ファイル数=[%s]' %
                                             (app_id, app_name, host_name, len(file_list)))
                            # 処理結果=image_shortageの場合
                            elif multi_result[0] == 'image_shortage':
                                host_name = multi_result[1]
                                file_list = multi_result[2]
                                logger.error('[%s:%s] マルチスレッドでの行間枚数の欠損が発生しています。 ホスト名=[%s], ファイルリスト=[%s], 行番号=[%s]' %
                                             (app_id, app_name, host_name, file_list, int(line_info_index) -1 + int(inspection_start_line[0])))
                                logger.debug('[%s:%s] エラーファイル出力を開始します。', app_id, app_name)
                                error_file_name = 'Image_on_line_' + \
                                                  str(int(line_info_index) -1 + int(inspection_start_line[0])) + '_is_lost.txt'
                                result = error_util.common_execute(error_file_name, logger, app_id, app_name)
                                if result:
                                    logger.debug('[%s:%s] エラーファイル出力を終了しました。' % (app_id, app_name))
                                else:
                                    logger.error('[%s:%s] エラーファイル出力が失敗しました。' % (app_id, app_name))
                                    logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))
                            # 処理結果=Falseの場合
                            else:
                                host_name = multi_result[1]
                                error = multi_result[3]
                                func_name = multi_result[4]
                                logger.error('[%s:%s] マルチスレッドでの行間枚数チェックが失敗しました。 '
                                             '[反番, 検査番号, 検査日付]=[%s, %s, %s] ホスト名=[%s]'
                                             % (app_id, app_name, fabric_name, inspection_num, inspection_date, host_name))
                                error_list.extend([host_name, error, func_name])
                                raise Exception

                result, file_name, error, func_name = edit_checked_linenum_file(read_tmp_file[0], line_info_index, after_tmp_file_info)
                if result:
                    logger.debug('[%s:%s] tmpファイルの編集が終了しました。:tmpファイル名[%s]',
                                 app_id, app_name, file_name)
                else:
                    logger.error('[%s:%s] tmpファイルファイルの編集に失敗しました。:tmpファイル名[%s]',
                                 app_id, app_name, file_name)

        if len(scan_info) == 0:
            return True, scaninfo_file, 'continue', func_name
        else:
            scaninfo_file = scan_info
            for file in regimark_files:
                base_filename = os.path.basename(file)
                regimark_face = re.split('_', (file.split('\\'))[-1])[5]
                result, regimark_info, error, func_name = read_file(file)
                if result:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルの読込が終了しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, regimark_files)
                    logger.debug('[%s:%s] レジマーク情報 : %s', app_id, app_name, regimark_info)
                else:
                    logger.error('[%s:%s] レジマーク読取結果ファイルの読込に失敗しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, regimark_files)
                    raise Exception

                read_tmp_file = [x for x in after_tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
                result, after_tmp_file_info, error, func_name = read_file(read_tmp_file[0])

                if len(regimark_info) == len(after_tmp_file_info):
                    check_result.append('OK')

                    tmp_file_move_dir = tmp_file_dir + '\\CHECKED'
                    # tmpファイルを出力するフォルダの存在を確認する
                    logger.debug('[%s:%s] tmpファイル移動先フォルダ存在チェックを開始します。', app_id, app_name)
                    result, error, func_name = exists_dir(tmp_file_move_dir)

                    if result:
                        logger.debug('[%s:%s] tmpファイル移動先フォルダ存在チェックが終了しました。', app_id, app_name)
                        pass
                    else:
                        logger.error('[%s:%s] tmpファイル移動先フォルダ存在チェックに失敗しました。:格納先フォルダ名[%s]',
                                     app_id, app_name, tmp_file_move_dir)
                        raise Exception

                    result, error, func_name = move_file(read_tmp_file, tmp_file_move_dir)
                    if result:
                        logger.debug('[%s:%s] tmpファイルの移動が終了しました。' % (app_id, app_name))
                    else:
                        logger.error('[%s:%s] tmpファイルの移動が失敗しました。 '
                                     '[反番, 検査番号, 検査日付, 検反部]=[%s, %s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date, regimark_face))
                        raise Exception
                else:
                    pass

        if check_result[0] == 'OK' and check_result[1] == 'OK':
            result_list = []
            error_list = []
            move_image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                   move_image_dir for i in range(thread_num)]
            image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                              image_dir for i in range(thread_num)]
            after_tmp_file_info = []
            line_info_index = len(after_tmp_file_info)
            func_list = []

            with ProcessPoolExecutor() as executor:
                for j in range(thread_num):
                    # スレッド実行
                    func_list.append(
                        executor.submit(
                            exec_check_image_num_multi_thread,
                            product_name, fabric_name, inspection_num, image_dir_list[j],
                            move_image_dir_list[j], rapid_hostname_list[j], inspection_date,
                            after_tmp_file_info, line_info_index, 1))

                for k in range(thread_num):
                    # スレッド戻り値を取得
                    result_list.append(func_list[k].result())
                    # マルチスレッド実行結果を確認する。

                print(result_list)
                for l, multi_result in enumerate(result_list):
                    # 処理結果=Trueの場合
                    if multi_result[0] is True:
                        host_name = multi_result[1]
                        file_list = multi_result[2]
                        logger.debug('[%s:%s] マルチスレッドでの撮像画像移動が終了しました。 ホスト名=[%s] ファイル数=[%s]' %
                                     (app_id, app_name, host_name, len(file_list)))
                    # 処理結果=image_shortageの場合
                    elif multi_result[0] == 'image_shortage':
                        host_name = multi_result[1]
                        file_list = multi_result[2]
                        logger.error('[%s:%s] マルチスレッドでの行間枚数の欠損が発生しています。 ホスト名=[%s], ファイルリスト=[%s], 行番号=[%s]' %
                                     (app_id, app_name, host_name, file_list,
                                      int(line_info_index) + int(inspection_start_line[0])))
                    # 処理結果=Falseの場合
                    else:
                        host_name = multi_result[1]
                        error = multi_result[3]
                        func_name = multi_result[4]
                        logger.error('[%s:%s] マルチスレッドでの撮像画像移動が失敗しました。 '
                                     '[反番, 検査番号, 検査日付]=[%s, %s, %s] ホスト名=[%s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date, host_name))
                        error_list.extend([host_name, error, func_name])
                        raise Exception

            #
            return True, scaninfo_file, 'end', func_name

    except Exception as error:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)
        logger.error('[%s:%s] %s [エラーコード:%s]' % (app_id, app_name, error_message, error_id))

        event_log_message = '[機能名, エラーコード]=[%s, %s] %s' % (app_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)

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
