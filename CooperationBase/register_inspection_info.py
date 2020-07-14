# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能301  検査情報登録機能
# ----------------------------------------
import configparser
import logging.config
import sys
import time
import traceback
from pathlib import Path
import codecs

import db_util
import error_detail
import error_util
import file_util
# ADD 20200714 KQRM 下吉 START
import os
import csv
# ADD 20200714 KQRM 下吉 END

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
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name

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
    # RAPID解析情報テーブルを作成する。
    result, error, conn, cur = db_util.create_table(conn, cur, fabric_name, inspection_num, inspection_date,
                                             logger, app_id, app_name)
    return result, error, conn, cur

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
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'select inspection_info_header.product_name, inspection_info_header.fabric_name ,' \
          'inspection_info_header.inspection_num, ' \
          'inspection_info_header.start_datetime, inspection_info_header.unit_num FROM inspection_info_header left ' \
          'outer join fabric_info on ' \
          '(inspection_info_header.fabric_name = fabric_info.fabric_name and ' \
          'inspection_info_header.inspection_num = fabric_info.inspection_num and ' \
          'inspection_info_header.start_datetime = fabric_info.imaging_starttime) where ' \
          'inspection_info_header.unit_num = \'%s\' and fabric_info.fabric_name is null and inspection_info_header.branch_num = 1 ' \
          'order by start_datetime asc' % unit_num

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))

    ### 検査情報テーブルからデータ取得
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# 処理名             ：前検査情報取得
#
# 処理概要           ：1.検査情報テーブルから前検査情報を取得する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：検査情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def select_before_inspection_data(conn, cur, starttime, unit_num):
    ### クエリを作成する
    sql = 'select fi.product_name, fi.fabric_name, fi.inspection_num, fi.imaging_endtime, fi.imaging_starttime, '\
          'ii.inspection_direction from fabric_info as fi, inspection_info_header as ii '\
          'where fi.unit_num = \'%s\' and fi.imaging_starttime < \'%s\' and '\
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and '\
          'fi.imaging_starttime = ii.start_datetime order by fi.imaging_starttime desc' % (unit_num, starttime)

    logger.debug('[%s:%s] 前検査情報取得SQL %s' % (app_id, app_name, sql))

    ### 検査情報テーブルからデータ取得
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur

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
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'insert into fabric_info (product_name, fabric_name, inspection_num, imaging_starttime, status, unit_num) ' \
          'values (\'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' % (product_name, fabric_name, inspection_num,
                                                                   starttime, status, unit_no)

    logger.debug('[%s:%s] 検査情報登録SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルへデータ登録
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：出力先フォルダ存在チェック
#
# 処理概要           ：1.撮像完了通知(ダミー)、レジマーク読み取り結果(ダミー)を出力するフォルダが存在するかチェックする。
#                      2.フォルダが存在しない場合は作成する。
#
# 引数               ：出力先フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 関数名             ：インプットファイル出力
#
# 処理概要           ：1.撮像完了通知(ダミー)、レジマーク読み取り結果(ダミー)を出力する。
#
# 引数               ：品番
#                      反番
#                      検査番号
#                      検査日
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def output_dummy_file(product_name, fabric_name, inspection_num, inspection_date, before_inspection_direction):
    result = False
    output_file_path = common_inifile.get('FILE_PATH', 'input_path')
    file_extension = inifile.get('PATH', 'file_extension')

    try:
        scaninfo_path = inifile.get('PATH', 'scaninfo_path')
        scaninfo_prefix = inifile.get('PATH', 'scaninfo_prefix')
        regimark_path = inifile.get('PATH', 'regimark_path')
        regimark_prefix = inifile.get('PATH', 'regimark_prefix')

        scaninfo_file_name = scaninfo_prefix + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + inspection_date + file_extension
        regimark_file_name = regimark_prefix + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + inspection_date + "_"

        # ADD 20200714 KQRM 下吉 START
        number_list_face_no_1 = []
        number_list_face_no_2 = []
        image_num = 0
        image_num_total = 0
        completed_lines = 0
        # ADD 20200714 KQRM 下吉 END

        for i in range(0, 2):
            for j in range(0, 2):
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(j + 1) + "_" + str(
                    i + 1) + file_extension

                # ADD 20200714 KQRM 下吉 START
                if os.path.exists(csv_file):
                    logger.debug('[%s:%s] レジマーク読取結果ファイルが存在します。[%s]', app_id, app_name, csv_file)
                    if i == 0 and (before_inspection_direction == 'S' or before_inspection_direction == 'X'):
                        # 検査方向S, Xの場合、開始レジマークを読み込む
                        if j == 0:
                            get_serial_number_list(csv_file, number_list_face_no_1, logger)
                        else:
                            get_serial_number_list(csv_file, number_list_face_no_2, logger)
                    elif i == 1 and (before_inspection_direction == 'R' or before_inspection_direction == 'Y'):
                        # 検査方向R, Yの場合、終了レジマークを読み込む
                        if j == 0:
                            get_serial_number_list(csv_file, number_list_face_no_1, logger)
                        else:
                            get_serial_number_list(csv_file, number_list_face_no_2, logger)

                    continue
                else:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルが存在しません。ダミーファイルを出力します。[%s]', app_id, app_name, csv_file)
                # ADD 20200714 KQRM 下吉 END

                with codecs.open(csv_file, "w", "SJIS") as f:
                    f.write("撮像ファイル名,種別,座標幅,座標高,パルス")
                    f.write("\r\n")

        # ADD 20200714 KQRM 下吉 START
        # 「少ない方の読取行数 -2」の値を設定する
        if len(number_list_face_no_1) > len(number_list_face_no_2):
            completed_lines = len(number_list_face_no_2) - 2
        else:
            completed_lines = len(number_list_face_no_1) - 2

        # マイナス値の場合、0で補正する
        if completed_lines < 0:
            completed_lines = 0

        # 撮像連番の最大値を設定する
        if max(number_list_face_no_1, default=0) > max(number_list_face_no_2, default=0):
            image_num = max(number_list_face_no_1, default=0)
        else:
            image_num = max(number_list_face_no_2, default=0)

        # カメラ台数を考慮し、総撮像枚数を設定する
        image_num_total = image_num * 54
        # ADD 20200714 KQRM 下吉 END

        csv_file = output_file_path + "\\" + scaninfo_path + "\\" + scaninfo_file_name
        with codecs.open(csv_file, "w", "SJIS") as f:
            f.write("カメラ台数,カメラ1台の撮像枚数,総撮像枚数,検査完了行数")
            f.write("\r\n")
            # UPD 20200714 KQRM 下吉 START
            # f.write("54,0,0,0")
            f.write("54,{},{},{}".format(image_num, image_num_total, completed_lines))
            # UPD 20200714 KQRM 下吉 END
            f.write("\r\n")

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ADD 20200714 KQRM 下吉 START
# ------------------------------------------------------------------------------------
# 関数名             ：連番取得
#
# 処理概要           ：1.レジマーク読取結果ファイルの撮像ファイル名より、連番を抽出しリストへ追加する。
#
# 引数               ：レジマーク読取結果ファイル名
#                      連番リスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      レジマーク読取結果ファイルリスト
# ------------------------------------------------------------------------------------
def get_serial_number_list(file_path, serial_number_list, logger):
    result = False
    file_name = None
    index = None

    try:
        with open(file_path) as f:
            # ヘッダ行を読み飛ばす
            next(csv.reader(f))

            # 撮像ファイル名より撮像連番を抽出し、数値としてリストに追加する
            for row in csv.reader(f):
                file_name = Path(row[0]).stem
                index = (file_name.find('_') + 1) * -1
                serial_number_list.append(int(file_name[index:]))

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result
# ADD 20200714 KQRM 下吉 END

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
    func_name = sys._getframe().f_code.co_name
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)
    return result, error, func_name


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
    error = None
    func_name = None
    try:

        # 設定ファイルから取得
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        imaging_status = int(common_inifile.get('FABRIC_STATUS', 'imaging'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')
        output_file_path = common_inifile.get('FILE_PATH', 'input_path')
        file_extension = inifile.get('PATH', 'file_extension')
        scaninfo_path = inifile.get('PATH', 'scaninfo_path')
        scaninfo_prefix = inifile.get('PATH', 'scaninfo_prefix')

        logger.info('[%s:%s] %s機能を起動します' % (app_id, app_name, app_name))
        ###DB接続を行う
        # もし接続失敗した場合は、再度接続し直す。
        while True:

            logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
            # DBに接続する
            result, error, conn, cur, func_name = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
                sys.exit()

            while True:

                logger.debug('[%s:%s] 検査情報取得を開始します。' % (app_id, app_name))
                # 検査情報テーブルから検査情報を取得する
                result, inspection_info, error, conn, cur, func_name = select_inspection_data(conn, cur, unit_num)

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
                        inspection_date = str(starttime.strftime('%Y%m%d'))
                        unit_no = inspection_info[i][4]
                        
                        logger.debug('[%s:%s] 前検査情報取得を開始します。' % (app_id, app_name))
                        # 検査情報テーブルから前検査情報を取得する
                        result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur, starttime, unit_num)
                        logger.debug('[%s:%s] 前検査情報取得が終了しました。%s ' % (app_id, app_name, before_inspection_info))
                        if before_inspection_info is None:
                            pass
                        elif before_inspection_info[3] == None:
                            before_product_name = before_inspection_info[0]
                            before_fabric_name = before_inspection_info[1]
                            before_inspection_num = before_inspection_info[2]
                            before_starttime = before_inspection_info[4]
                            before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                            logger.debug('[%s:%s] 前検査情報 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]' % (
                                app_id, app_name, before_product_name, before_fabric_name,
                                before_inspection_num, before_inspection_date))

                            before_scan_file_name = scaninfo_prefix + "_" + before_product_name + "_" + before_fabric_name + "_" + str(before_inspection_num) + "_" + before_inspection_date + file_extension
                            if os.path.exists(output_file_path + '\\' + scaninfo_path + '\\' + before_scan_file_name):
                                logger.info('[%s:%s] 前検査の撮像完了通知は出力済です。 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]'
                                            % (app_id, app_name, before_product_name, before_fabric_name,
                                               before_inspection_num, before_inspection_date))
                                time.sleep(sleep_time * 2)
                                break
                            else:
                                for i in range(5):
                                    logger.debug('[%s:%s] 前検査情報取得を開始します。' % (app_id, app_name))
                                    # 検査情報テーブルから前検査情報を取得する
                                    result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur,
                                                                                                              starttime,
                                                                                                              unit_num)

                                    if result:
                                        logger.debug('[%s:%s] 前検査情報取得が終了しました。' % (app_id, app_name))
                                        if before_inspection_info is None:
                                            pass
                                        elif before_inspection_info[3] != None:
                                            before_product_name = before_inspection_info[0]
                                            before_fabric_name = before_inspection_info[1]
                                            before_inspection_num = before_inspection_info[2]
                                            before_starttime = before_inspection_info[4]
                                            before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                                            logger.debug('[%s:%s] 前検査情報 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]' % (
                                                app_id, app_name, before_product_name, before_fabric_name,
                                                before_inspection_num, before_inspection_date))
                                            break
                                        else:
                                            logger.info('[%s:%s] 前検査の検査完了時刻が存在しません。再確認します。' % (app_id, app_name))
                                            time.sleep(sleep_time)
                                            continue
                                    else:
                                        logger.error('[%s:%s] 前検査情報取得が失敗しました。' % (app_id, app_name))
                                        conn.rollback()
                                        sys.exit()

                                if before_inspection_info is None:
                                    pass
                                elif before_inspection_info[3] == None:
                                    before_product_name = before_inspection_info[0]
                                    before_fabric_name = before_inspection_info[1]
                                    before_inspection_num = before_inspection_info[2]
                                    before_starttime = before_inspection_info[4]
                                    before_inspection_direction = before_inspection_info[5]
                                    before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                                    logger.info('[%s:%s] 前検査が完了していません。前検査情報 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]' % (
                                    app_id, app_name, before_product_name, before_fabric_name, before_inspection_num, before_inspection_date))


                                    error_file_path = common_inifile.get('ERROR_FILE', 'path')
                                    Path(error_file_path + '\\' + error_file_name).touch()

                                    logger.info('[%s:%s] リカバリ（ダミーファイル出力）を開始します。 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]'
                                                % (app_id, app_name, before_product_name, before_fabric_name,
                                                   before_inspection_num, before_inspection_date))
                                    tmp_result = output_dummy_file(before_product_name, before_fabric_name,
                                                               before_inspection_num, before_inspection_date, before_inspection_direction)
                                    if tmp_result:
                                        logger.info('[%s:%s] リカバリ（ダミーファイル出力）が終了しました。 [品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]'
                                                    % (app_id, app_name, before_product_name, before_fabric_name,
                                                       before_inspection_num, before_inspection_date))
                                        time.sleep(sleep_time * 2)
                                        break
                                    else:
                                        logger.error('[%s:%s] リカバリ（ダミーファイル出力）に失敗しました。 ' % (app_id, app_name))
                                        sys.exit()
                                else:
                                    pass               
                        else:
                            pass

                        inspection_date = str(starttime.strftime('%Y%m%d'))

                        logger.debug('[%s:%s] 検査情報 [品番=%s] [反番=%s] [検査番号=%s]' %
                                     (app_id, app_name, product_name, fabric_name, inspection_num))

                        logger.debug('[%s:%s] %s処理を開始します。 [反番, 検査番号]=[%s, %s]' %
                                     (app_id, app_name, app_name, fabric_name, inspection_num))

                        # RAPID解析情報テーブルを作成する
                        logger.debug('[%s:%s] RAPID解析情報テーブル作成を開始します。' % (app_id, app_name))
                        result, error, conn, cur = create_rapid_table(conn, cur, fabric_name, inspection_num, inspection_date)
                        if result:
                            logger.debug('[%s:%s] RAPID解析情報テーブル作成が終了しました。' % (app_id, app_name))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] RAPID解析情報テーブル作成が失敗しました。 '
                                         '[反番, 検査番号, 検査日付]=[%s, %s, %s]'
                                         % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            conn.rollback()
                            sys.exit()

                        # 検査情報を反物情報テーブルに登録する
                        result, error, conn, cur, func_name = insert_fabric_info(conn, cur, product_name, fabric_name,
                                                                      inspection_num, starttime, imaging_status, unit_no)
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

        logger.debug('[%s:%s] エラー詳細を取得します。' % (app_id, app_name))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)

        logger.error('[%s:%s] %s [エラーコード:%s]' % (app_id, app_name, error_message, error_id))

        event_log_message = '[機能名, エラーコード]=[%s, %s] %s' % (app_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message)

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
