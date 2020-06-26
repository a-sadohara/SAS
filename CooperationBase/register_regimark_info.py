# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能303  レジマーク情報登録
# ----------------------------------------
import codecs
import configparser
import sys
import datetime
import time
import logging.config
import traceback
import os
import re
import datetime

import db_util
import file_util
import error_detail
import error_util
import register_ng_info

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_regimark_info.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_regimark_info")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_regimark_info_config.ini', 'SJIS')

error_inifile = configparser.ConfigParser()
error_inifile.read('D:/CI/programs/config/error_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 関数名             ：DB接続
#
# 処理概要           ：1.DBと接続する
#
# 引数               ：機能ID

#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def create_connection():
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：レジマークマスター情報取得
#
# 処理概要           ：1.品種登録情報テーブルからレジマークマスター情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      レジマークマスタ情報(設定レジマーク長さ、伸縮率X軸、伸縮率Y軸)
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#
#
# ------------------------------------------------------------------------------------
def select_master_data(conn, cur, product_name):
    ### クエリを作成する
    sql = 'select regimark_between_length, stretch_rate_x, stretch_rate_y, ai_model_non_inspection_flg ' \
          'from mst_product_info where product_name = \'%s\'' % product_name

    logger.debug('[%s:%s] レジマークマスター情報取得SQL %s' % (app_id, app_name, sql))
    ### 品種登録情報テーブルからデータ取得
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：検査情報取得
#
# 処理概要           ：1.検査情報ヘッダーテーブルから検査情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      検査開始行
#                      最終行番
#                      検査方向
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, product_name, fabric_name, inspection_num, start_datetime, unit_num):
    ### クエリを作成する
    sql = 'select inspection_start_line, inspection_end_line, inspection_direction ' \
          'from inspection_info_header where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, start_datetime, unit_num)

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))
    ### 検査情報ヘッダーテーブルからデータ取得
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：レジマーク情報登録済み確認
#
# 処理概要           ：1.開始/終了レジマーク読取結果ファイルが時間差で連携された場合、
#                       どちらかがすでに登録済みであるか確認する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      検査開始行
#                      最終行番
#                      検査方向
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_regimark_info(conn, cur, product_name, fabric_name, inspection_num, line_num,
                         regimark_face, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'select * from regimark_info where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' ' \
          'and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, line_num, regimark_face, imaging_starttime, unit_num)

    logger.debug('[%s:%s] レジマーク情報登録済み確認SQL %s' % (app_id, app_name, sql))
    ### 検査情報ヘッダーテーブルからデータ取得
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：レジマーク情報登録
#
# 処理概要           ：1.レジマーク情報テーブルへレジマーク情報を登録する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      レジマーク情報
#                      品名
#                      反番
#                      検査番号
#                      設定レジマーク長さ
#                      現画像高さ(Y軸)
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def insert_regimark_info(conn, cur, regimark, regimark_between_length_list, strech_ratio_list,
                         product_name, fabric_name, inspection_num, conf_regimark_len, image_height, image_width,
                         regimark_type, regimark_face, imaging_starttime, unit_num):
    result = False
    sql = None
    try:
        # 撮像画像(リサイズ画像)の幅(pix)
        resize_image_width = float(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        # 撮像画像(リサイズ画像)の高さ(pix)
        resize_image_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        for i in range(len(regimark)):

            line_num = regimark[i][0]
            tmp_result, registed_info, conn, cur = select_regimark_info(conn, cur, product_name, fabric_name,
                                                                        inspection_num, line_num, regimark_face,
                                                                        imaging_starttime, unit_num)

            if tmp_result:
                pass
            else:
                # 失敗時
                logger.error("[%s:%s] 登録済みレジマーク情報確認が失敗しました。", app_id, app_name)
                return result

            # 同一反番、検査番号、行番号が未登録の場合
            if len(registed_info) == 0:
                # 登録対象が開始レジマークの場合
                if re.split('\.', regimark_type)[0] == '1':
                    # 登録対象に実測レジマーク、長さ/幅伸縮率が含まれる場合
                    # 開始レジマークの場合、検査方向がS、Xの際に実測レジマークを算出している
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'setting_regimark_length, actual_regimark_length, len_stretchrate, width_stretchrate, ' \
                              'start_regimark_file, start_regimark_point_org, start_regimark_point_resize, face, ' \
                              'imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, %s, %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, conf_regimark_len,
                                 actual_regimark_length,
                                 len_stretchrate, width_stretchrate, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize, face, imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                # 登録対象が終了レジマークの場合
                else:
                    # 登録対象に実測レジマーク、長さ/幅伸縮率が含まれる場合
                    # 終了レジマークの場合、検査方向がY、Rの際に実測レジマークを算出している
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'setting_regimark_length, actual_regimark_length, len_stretchrate, width_stretchrate, ' \
                              'end_regimark_file, end_regimark_point_org, end_regimark_point_resize, face, ' \
                              'imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, %s, %s, %s, %s, \'%s\', \'%s\', \'%s\',\'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, conf_regimark_len,
                                 actual_regimark_length,
                                 len_stretchrate, width_stretchrate, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize, face, imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)


            # 同一反番、検査番号、行番号が登録済の場合
            else:
                # 登録対象が開始レジマークの場合
                if re.split('\.', regimark_type)[0] == '1':
                    # 登録対象に実測レジマーク、長さ/幅伸縮率が含まれる場合
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'update regimark_info set (setting_regimark_length, actual_regimark_length, ' \
                              'len_stretchrate, width_stretchrate, start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize) = (%s, %s, %s, %s, \'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and inspection_num = %s ' \
                              'and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
                              % (
                                  conf_regimark_len, actual_regimark_length, len_stretchrate, width_stretchrate,
                                  image_name,
                                  regimark_point_org, regimark_point_resize, product_name, fabric_name, inspection_num,
                                  line_num, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'update regimark_info set (start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize) = (\'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and inspection_num = %s ' \
                              'and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' and unit_num =\'%s\'' \
                              % (image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                 inspection_num, line_num, regimark_face, imaging_starttime, unit_num)
                # 登録対象が終了レジマークの場合
                else:
                    # 登録対象に実測レジマーク、長さ/幅伸縮率が含まれる場合
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = strech_ratio_list[i][0]
                        width_stretchrate = strech_ratio_list[i][1]
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'update regimark_info set (setting_regimark_length, actual_regimark_length, ' \
                              'len_stretchrate, width_stretchrate, end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize) = (%s, %s, %s, %s, \'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and ' \
                              'inspection_num = %s and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\'' \
                              'and unit_num = \'%s\'' \
                              % (
                                  conf_regimark_len, actual_regimark_length, len_stretchrate, width_stretchrate,
                                  image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                  inspection_num, line_num, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### クエリを作成する
                        sql = 'update regimark_info set (end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize) = ' \
                              '(\'%s\', \'%s\', \'%s\') where product_name = \'%s\' and fabric_name = \'%s\' and ' \
                              'inspection_num = %s and line_num =%s and face = \'%s\' and imaging_starttime = \'%s\'' \
                              'and unit_num = \'%s\'' \
                              % (image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                 inspection_num, line_num, regimark_face, imaging_starttime, unit_num)

            logger.debug('[%s:%s] レジマーク情報登録SQL %s' % (app_id, app_name, sql))
            # レジマーク情報を登録する
            tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

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
# 関数名             ：ファイル取得
#
# 処理概要           ：1.レジマーク読取結果ファイルのファイルリストを取得する。
#
# 引数               ：レジマーク読取結果ファイル格納フォルダパス
#                      レジマーク読取結果ファイル名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      レジマーク読取結果ファイルリスト
# ------------------------------------------------------------------------------------
def get_regimark_file(file_path, file_name_pattern):
    result = False
    sorted_files = None

    try:

        # 共通関数でレジマーク読取結果格納フォルダ情報を取得する
        tmp_result, file_list = file_util.get_file_list(file_path, file_name_pattern, logger, app_id, app_name)

        if tmp_result:
            # 成功時
            pass
        else:
            # 失敗時
            logger.error("[%s:%s] レジマーク読取結果格納フォルダにアクセス出来ません。", app_id, app_name)
            return result, sorted_files

        # 取得したファイルパスをファイル更新日時でソートする（古い順に処理するため）
        file_names = []
        for files in file_list:
            file_names.append((os.path.getmtime(files), files))

        sorted_files = []
        for mtime, path in sorted(file_names):
            sorted_files.append(path)

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, sorted_files


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
def read_regimark_file(file):
    result = False
    regimark_info = []

    try:
        logger.debug('[%s:%s] レジマーク読取結果ファイル=%s', app_id, app_name, file)
        # レジマーク読取結果ファイルパスからファイル名を取得し、反番、検査番号を取得する
        # なお、ファイル名は「RM_品名_反番_検査番号_日付_検反部_レジマーク種別.CSV」を想定している

        # レジマーク読取結果ファイルから、項目を取得する
        with codecs.open(file, "r", "SJIS") as f:
            notification = [re.sub('\r', '', s[:-1]).split(',') for s in f.readlines()][1:]

        regimark_info = notification
        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, regimark_info


# ------------------------------------------------------------------------------------
# 関数名             ：レジマーク読取重複確認
#
# 処理概要           ：1.リスト化した情報から、レジマーク検知画像のカメラ番号、撮像番号が
#                      連番となっているものを確認する。
#                      2.連番となっているものが存在する場合、採用レジマーク特定を呼び出し、
#                      レジマークとして採用する画像の特定を行う。
#
# 引数               ：開始レジマークリスト/終了レジマークリスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      開始レジマークリスト/終了レジマークリスト
#
# ------------------------------------------------------------------------------------
def confirm_duplicate_registremark(regimark_list, fabric_name, inspection_num, inspection_info, image_height,
                                   image_width):
    result = False
    confirmed_regimark_list = []
    line_num = None

    try:
        inspection_start_line = inspection_info[0]
        inspection_end_line = inspection_info[1]
        inspection_direction = inspection_info[2]

        # レジマーク情報リスト数分処理を行う。(開始レジマークリスト、終了レジマークリストの2回)
        for i in range(len(regimark_list)):
            # 検査方向が S or Xの場合、開始行側から撮像されるため行番号の初期値を検査開始行数とする。
            if inspection_direction == 'S' or inspection_direction == 'X':
                line_num = inspection_start_line
            # 検査方向が Y or Rの場合、最終行側から撮像されるため行番号の初期値を最終行番とする。
            elif inspection_direction == 'Y' or inspection_direction == 'R':
                line_num = inspection_start_line

            logger.debug('[%s:%s] レジマークリスト %s', app_id, app_name, regimark_list[i])
            # 画像ファイル名を[._]で分割
            sp_list = [re.split('[_.]', x[:][0]) + x[1:] for x in regimark_list[i]]
            logger.debug('[%s:%s] スプリットレジマークリスト %s', app_id, app_name, sp_list)

            # レジマーク読取結果内のレコード数分処理を行う。
            while len(sp_list) > 0:
                # 重複確認用キーを抽出(撮像番号、カメラ番号)
                key_image_num = int(sp_list[0][6])
                key_cam_num = int(sp_list[0][5])

                # 以下条件に当てはまるレジマーク読取結果を抽出する
                #     カメラ番号＋１ 且つ 撮像番号同じ画像が存在
                #     カメラ番号同じ 且つ 撮像番号＋１の画像が存在
                #     カメラ番号＋１ 且つ 撮像番号＋１の画像が存在
                duplicate_registremarks = [s for s in sp_list if
                                           (key_image_num == int(s[:][6]) and key_cam_num == int(s[:][5])) or \
                                           ((key_image_num + 1) == int(s[:][6]) and key_cam_num == int(s[:][5])) or \
                                           (key_image_num == int(s[:][6]) and (key_cam_num + 1) == int(s[:][5])) or \
                                           ((key_image_num + 1) == int(s[:][6]) and (key_cam_num + 1) == int(s[:][5]))]

                # 条件に当てはまるレジマーク読取結果が1件の場合は、レジマーク情報として利用する。
                if len(duplicate_registremarks) == 1:
                    add_line_num = duplicate_registremarks
                    add_line_num[0].insert(0, line_num)
                    confirmed_regimark_list += add_line_num

                # 条件に当てはまるレジマーク読取結果が2件以上存在する場合は、採用レジマーク特定処理を実行する。
                else:
                    tmp_result, specificed_regimark_list = specific_regimark(duplicate_registremarks, image_height,
                                                                             image_width)

                    if tmp_result:
                        logger.debug('[%s:%s] 採用レジマーク特定が終了しました。:[反番,検査番号]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        logger.debug('[%s:%s] 採用レジマーク :%s',
                                     app_id, app_name, specificed_regimark_list)
                    else:
                        logger.error('[%s:%s] 採用レジマーク特定に失敗しました。:[反番,検査番号]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        return result, confirmed_regimark_list

                    add_line_num = specificed_regimark_list
                    add_line_num.insert(0, line_num)
                    confirmed_regimark_list += [add_line_num]

                # 処理済レジマーク情報をリストから削除する。
                for image in duplicate_registremarks:
                    sp_list.remove(image)

                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_num += 1
                elif inspection_direction == 'Y' or inspection_direction == 'R':
                    line_num -= 1

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, confirmed_regimark_list


# ------------------------------------------------------------------------------------
# 関数名             ：採用レジマーク特定
#
# 処理概要           ：1.レジマーク読取画像が重複した場合、レジマークとして利用する画像を特定する。
#                      2.カメラ番号、撮像番号を確認する。
#                      3.カメラ番号が連番の場合は、撮像画像の端からの幅(X軸)から採用するレジマークを特定する。
#                      4.撮像番号が連番の場合は、撮像画像の端からの高さ(Y軸)から採用するレジマークを特定する
#                      5.カメラ番号が連番、撮像番号が連番の場合、撮像画像の端からの幅(X軸)、高さ(Y軸)それぞれから採用するレジマークを特定する
#
# 引数               ：重複が存在するレジマーク情報
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      採用レジマーク情報
#
# ------------------------------------------------------------------------------------
def specific_regimark(duplicate_registremarks, image_height, image_width):
    result = False
    pickup_regimark = []

    try:
        # キーとなるカメラ番号を抽出
        min_camera_num = min([int(mi_cam_num[5]) for mi_cam_num in duplicate_registremarks])
        min_image_num = min([int(mi_img_num[6]) for mi_img_num in duplicate_registremarks])

        ng_face = duplicate_registremarks[0][4]

        regimark_points = []
        x_regimark_points = []
        y_regimark_points = []

        # レジマーク読取画像重複が2枚の場合
        if len(duplicate_registremarks) == 2:
            # 撮像番号が等しい確認し、等しい場合は、X軸方向の長さで採用レジマークを特定する。
            if int(duplicate_registremarks[0][6]) == int(duplicate_registremarks[1][6]):
                if ng_face == '1':
                    for k in range(len(duplicate_registremarks)):
                        if int(duplicate_registremarks[k][5]) == min_camera_num:
                            x_point = int(duplicate_registremarks[k][9])
                            regimark_points.append(x_point)
                        elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                            x_point = (image_width - int(duplicate_registremarks[k][9]))
                            regimark_points.append(x_point)
                else:
                    for k in range(len(duplicate_registremarks)):
                        if int(duplicate_registremarks[k][5]) == min_camera_num:
                            x_point = image_width - int(duplicate_registremarks[k][9])
                            regimark_points.append(x_point)
                        elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                            x_point = (int(duplicate_registremarks[k][9]))
                            regimark_points.append(x_point)
            # カメラ番号が等しいか確認し、等しい場合は、Y軸方向の長さで採用レジマークを特定する。
            elif int(duplicate_registremarks[0][5]) == int(duplicate_registremarks[1][5]):
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][6]) == min_image_num:
                        y_point = (image_height - int(duplicate_registremarks[k][10]))
                        regimark_points.append(y_point)
                    elif int(duplicate_registremarks[k][6]) == (min_image_num + 1):
                        y_point = int(duplicate_registremarks[k][10])
                        regimark_points.append(y_point)

            # 採用したレジマーク情報がリスト内のどの位置に格納されているか確認し、レジマーク情報を抽出する。
            index = regimark_points.index(max(regimark_points))
            pickup_regimark = duplicate_registremarks[index]

            result = True

        # レジマーク読取画像重複が3枚以上の場合
        elif len(duplicate_registremarks) >= 3:
            # X軸方向の長さを確認し、最も大きい画像のカメラ番号を採用する。
            if ng_face == '1':
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][5]) == min_camera_num:
                        x_point = (int(duplicate_registremarks[k][9]))
                        x_regimark_points.append(x_point)
                    elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                        x_point = image_width - int(duplicate_registremarks[k][9])
                        x_regimark_points.append(x_point)
            else:
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][5]) == min_camera_num:
                        x_point = (image_width - int(duplicate_registremarks[k][9]))
                        x_regimark_points.append(x_point)
                    elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                        x_point = int(duplicate_registremarks[k][9])
                        x_regimark_points.append(x_point)

            # X軸方向の長さがもっと大きいレジマーク情報の位置を特定し、該当カメラ番号と等しい画像を抽出する。
            x_index = x_regimark_points.index(max(x_regimark_points))
            new_duplicate_registremarks = [y for y in duplicate_registremarks if
                                           duplicate_registremarks[x_index][5] in y[5]]

            # 該当カメラ番号と等しい画像の撮像番号と等しい画像のY軸方向の長さを確認し、最も大きい画像を確認する。
            for m in range(len(new_duplicate_registremarks)):
                if int(new_duplicate_registremarks[m][6]) == min_image_num:
                    y_point = (image_height - int(new_duplicate_registremarks[m][10]))
                    y_regimark_points.append(y_point)
                elif int(new_duplicate_registremarks[m][6]) == (min_image_num + 1):
                    y_point = int(new_duplicate_registremarks[m][10])
                    y_regimark_points.append(y_point)

            # Y軸方向の長さが最も大きいレジマーク情報の位置を特定し、採用レジマークを特定する。
            y_index = y_regimark_points.index(max(y_regimark_points))
            pickup_regimark = new_duplicate_registremarks[y_index]

            result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, pickup_regimark


# ------------------------------------------------------------------------------------
# 関数名             ：実測レジマーク間長さ算出
#
# 処理概要           ：1.行間の撮像画像枚数より、実測レジマーク間長さを算出する。
#
# 引数               ：重複確認済みレジマーク情報
#                      現画像の幅(X軸)(オーバーラップ除外分)
#                      現画像のオーバーラップの幅(X軸)
#                      設定レジマーク間長さ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      実測レジマーク間長さ
#
# ------------------------------------------------------------------------------------
def calc_actual_regimark_between_length(confirmed_regimark, nonoverlap_image_length, overlap_length,
                                        conf_regimark_between_length, actual_image_height, one_pix_length):
    result = False
    actual_regimark_between_length_list = []

    try:
        # レジマークリストが1件の場合、次行開始との間の長さを確認できないため、設定レジマーク間長さを利用する。
        if len(confirmed_regimark) == 1:
            actual_regimark_between_length_list.append(conf_regimark_between_length)

        else:
            # 開始レジマーク情報と次行開始レジマーク情報を利用して、実測レジマーク間長さを算出する。
            for i in range(len(confirmed_regimark)):
                if i != (len(confirmed_regimark) - 1):

                    start_image_num = int(confirmed_regimark[i][7])
                    start_regmark_y = int(confirmed_regimark[i][11])

                    next_start_image_num = int(confirmed_regimark[i + 1][7])
                    next_start_regmark_y = int(confirmed_regimark[i + 1][11])

                    between_image_count = next_start_image_num - start_image_num - 1
                    # 撮像枚数(レジマーク部分除外)×image_length＋(現画像長さ - [開始レジマークy座標 * 1pixあたりの長さ]-[overlap_length])＋[次行開始レジマークy座標 * 1pixあたりの長さ]
                    regimark_length_mm = between_image_count * nonoverlap_image_length + (
                            actual_image_height - (int(start_regmark_y) * one_pix_length) - overlap_length) + \
                                         (int(next_start_regmark_y) * one_pix_length)
                    actual_regimark_between_length_list.append(regimark_length_mm)


                # レジマークリストが最後の1件の場合、次行開始との間の長さを確認できないため、前行のレジマーク間長さを利用する。
                else:
                    actual_regimark_between_length_list.append(actual_regimark_between_length_list[-1])

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, actual_regimark_between_length_list


# ------------------------------------------------------------------------------------
# 関数名             ：長さ/幅伸縮率算出
#
# 処理概要           ：1.長さ伸縮率/幅伸縮率を算出する
#                         Xa：設定レジマーク間長さ
#                         Xb：領域伸び率X軸
#                         Yb：領域伸び率Y軸
#                         Xa*Xb：領域伸び率の平均値
#                         Xd：実測レジマーク間
#
#                         長さ伸縮率 ＝ Xd  / Xa
#                         幅伸縮率 = Yb * (Xa*Xb)/Xd
#
# 引数               ：実測レジマーク間長さ
#                      設定レジマーク間長さ
#                      伸縮率X軸
#                      伸縮率Y軸
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      伸縮率(長さ伸縮率、幅伸縮率)
#
# ------------------------------------------------------------------------------------
def calc_stretch_ratio(actual_regimark_between_length_list, conf_regimark_between_length, stretch_rate_x,
                       stretch_rate_y):
    result = False
    strech_ratio_list = []

    try:
        for i in range(len(actual_regimark_between_length_list)):
            length_strech_ratio = (actual_regimark_between_length_list[i] / conf_regimark_between_length) * 100
            width_strech_ratio = (stretch_rate_y / 100 * (conf_regimark_between_length * stretch_rate_x / 100) /
                                  actual_regimark_between_length_list[i]) * 100
            ratio = [length_strech_ratio, width_strech_ratio]
            strech_ratio_list.append(ratio)
        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, strech_ratio_list


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
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：レジマーク読取結果ファイル退避
#
# 処理概要           ：1.レジマーク読取結果ファイルを、退避フォルダに移動させる。
#
# 引数               ：レジマーク読取結果
#                      退避フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    # ファイル移動
    result = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 関数名             ：レジマーク間距離csv出力
#
# 処理概要           ：1.レジマーク間距離算出結果をcsvファイルに出力する。
#
# 引数               ：結合済レジマーク情報
#                      設定レジマーク間長さ
#                      csv出力先パス
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def output_regimark_between_length_file(confirmed_regimark, regimark_between_length_list, strech_ratio_list,
                                        conf_regimark_between_length, output_file_path, fabric_name, inspection_num):
    result = False
    date = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    file_name = str(date) + "_" + fabric_name + "_" + inspection_num.zfill(3) + "_N.csv"

    try:
        if os.path.exists(output_file_path + '\\' + file_name):
            # 存在する場合、退避ファイルに別名（※）をつける。
            # （※）退避ファイルの最終更新日時（YYYYMMDD_HHMMSS）を、ファイル名の末尾に付与する。
            # xxxx.csv  →  xxxx.csv.20200101_235959
            timestamp = datetime.datetime.now()
            file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
            file_name = file_name + '.' + file_timestamp

        csv_file = output_file_path + "\\" + file_name
        with codecs.open(output_file_path + "\\" + file_name, "w", "SJIS") as f:
            f.write("行番号,設定レジマーク間長さ(mm),実測レジマーク間長さ(mm),長さ伸縮率(),幅伸縮率()")
            f.write("\r\n")

            for i in range(len(confirmed_regimark)):
                line_num = confirmed_regimark[i][0]
                conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                actual_regimark_len = '{:.6f}'.format(float(regimark_between_length_list[i]))
                length_ratio = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                width_ratio = '{:.6f}'.format(float(strech_ratio_list[i][1]))

                string = str(line_num) + "," + str(conf_regimark_len) + "," + str(actual_regimark_len) + "," + str(
                    length_ratio) + "," + str(width_ratio)

                f.write(string)
                f.write("\r\n")

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, csv_file


# ADD 20200626 KQRM 下吉 START
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
def output_dummy_file(product_name, fabric_name, inspection_num, inspection_date):
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

        for i in range(0, 2):
            for j in range(0, 2):
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(i + 1) + "_" + str(
                    j + 1) + file_extension
                with codecs.open(csv_file, "w", "SJIS") as f:
                    f.write("撮像ファイル名,種別,座標幅,座標高")
                    f.write("\r\n")

        # DEL 20200626 KQRM 下吉 START
        # csv_file = output_file_path + "\\" + scaninfo_path + "\\" + scaninfo_file_name
        # with codecs.open(csv_file, "w", "SJIS") as f:
        #     f.write("カメラ台数,カメラ1台の撮像枚数,総撮像枚数")
        #     f.write("\r\n")
        #     f.write("54,0,0")
        #     f.write("\r\n")
        # DEL 20200626 KQRM 下吉 END

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ADD 20200626 KQRM 下吉 END

# ------------------------------------------------------------------------------------
# 関数名             ：メイン処理
#
# 処理概要           ：1.レジマーク読み取り結果ファイルを監視する
#                      2.実測レジマークを算出する
#                      3.実測レジマーク間距離CSVを出力する
#                      4.レジマーク情報をDBに登録する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, imaging_starttime):
    # コネクションオブジェクト, カーソルオブジェクト
    conn = None
    cur = None
    error_file_name = None
    conf_regimark_len = 'null'
    strech_ratio_list = None
    ai_model_flag = None
    try:
        # 変数定義
        ### 設定ファイルからの値取得
        # 共通設定：各種通知ファイルが格納されるルートパス
        input_root_path = common_inifile.get('FILE_PATH', 'input_path')
        # 共通設定：各種通知ファイルを退避させるルートパス
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # レジマーク読取結果が格納されるフォルダパス
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path + '\\'
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # レジマーク読取結果ファイル名パターン
        file_name_pattern = inifile.get('PATH', 'file_name_pattern')
        # レジマーク読取結果拡張子パターン
        file_extension_pattern = inifile.get('PATH', 'file_extension_pattern')
        # レジマーク間距離csvを出力するフォルダパス
        output_file_path = inifile.get('PATH', 'output_file_path')
        # レジマーク読取結果ファイルを退避するフォルダパス
        backup_file_path = inifile.get('PATH', 'backup_file_path')
        backup_file_path = backup_root_path + '\\' + backup_file_path
        # レジマーク間距離csvを移動するフォルダパス
        csv_file_path = inifile.get('PATH', 'csv_file_path')

        # 撮像画像の高さ(mm)
        actual_image_height = float(common_inifile.get('IMAGE_SIZE', 'actual_image_height'))
        # 撮像画像のオーバーラップ(mm)
        actual_image_overlap = float(common_inifile.get('IMAGE_SIZE', 'actual_image_overlap'))
        # 撮像画像(元画像)の高さ(pix)
        image_height = float(common_inifile.get('IMAGE_SIZE', 'image_height'))
        # 撮像画像(元画像)の幅(pix)
        image_width = float(common_inifile.get('IMAGE_SIZE', 'image_width'))
        # オーバーラップを除いた撮像画像1枚の長さ(mm)
        nonoverlap_image_length = actual_image_height - actual_image_overlap
        # 現画像1pixあたりの長さ(mm)
        one_pix_length = actual_image_height / image_height

        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # パーミッションエラーメッセージ
        permission_error = error_inifile.get('ERROR_INFO', 'permission_error')
        # ファイル退避再実行間隔
        file_bk_sleep_time = int(inifile.get('VALUE', 'file_backup_sleep_time'))
        # ファイル退避再実行回数
        file_bk_retry_count = int(inifile.get('VALUE', 'file_backup_retry_count'))
        # 検査対象ライン番号
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        empty_file = False

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### レジマーク読取結果フォルダを監視する
        # フォルダ内にレジマーク読取結果ファイルが存在するか確認する
        logger.debug('[%s:%s] レジマーク読取結果ファイルの確認を開始します。', app_id, app_name)
        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))
        file_pattern = file_name_pattern + "_" + product_name + "_" + fabric_name + "_" + \
                       str(inspection_num) + "_" + inspection_date + file_extension_pattern
        logger.info('%s', file_pattern)
        result, sorted_files = get_regimark_file(file_path, file_pattern)

        if result:
            pass
        else:
            logger.error('[%s:%s] レジマーク読取結果ファイルの確認に失敗しました。', app_id, app_name)
            sys.exit()

        # UPD 20200626 KQRM 下吉 START
        # # 撮像完了通知ファイルがない場合は一定期間sleepして再取得
        # if len(sorted_files) != 4:
        #     logger.error('[%s:%s] レジマーク読取結果ファイルが欠損しています。', app_id, app_name)
        #     sys.exit()

        # 各ファイルの行数を取得する。
        file_lines_list = [0] * 4
        for i in range(len(sorted_files)):
            file_lines_list[i] = sum([1 for _ in open(sorted_files[i])])

        # ファイル数、行数をチェックする
        if (len(sorted_files) != 4 or
            not (file_lines_list[0] ==
                 file_lines_list[1] ==
                 file_lines_list[2] ==
                 file_lines_list[3])):
            logger.error('[%s:%s] レジマーク読取結果ファイルが欠損しています。', app_id, app_name)
        # UPD 20200626 KQRM 下吉 END
            # ADD 20200626 KQRM 下吉 END
            if len(sorted_files) != 0:
                # レジマーク読取結果ファイルを退避するフォルダの存在を確認する
                logger.debug('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックを開始します。', app_id, app_name)
                result = exists_dir(backup_file_path)

                if result:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックが終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                 app_id, app_name, output_file_path)
                    sys.exit()

                # 欠損しているレジマーク読取結果ファイルを退避フォルダに移動させる。
                retry_count = 0
                file = sorted_files
                while len(file) > 0:
                    logger.debug('[%s:%s] 不備のあるレジマーク読取結果ファイルの退避を開始します。:レジマーク読取結果ファイル名[%s]',
                                 app_id, app_name, file)
                    result = move_file(file[0], backup_file_path)

                    if result == permission_error:
                        if retry_count == file_bk_retry_count:
                            logger.error('[%s:%s] 不備のあるレジマーク読取結果ファイルの退避に失敗しました。:レジマーク読取結果ファイル名[%s]',
                                         app_id, app_name, file[0])
                            sys.exit()
                        else:
                            logger.debug('[%s:%s] 不備のあるレジマーク読取結果ファイルの退避を再実行します。', app_id, app_name, app_name)
                            file.append(file[0])
                            del file[0]
                            time.sleep(file_bk_sleep_time)
                            retry_count += 1
                    elif result:
                        logger.debug('[%s:%s] 不備のあるレジマーク読取結果ファイルの退避が終了しました。:退避先フォルダ[%s], レジマーク読取結果ファイル名[%s]',
                                     app_id, app_name, output_file_path, file[0])
                        # sorted_files.remove(file[0])
                        file.remove(file[0])
                    else:
                        logger.error('[%s:%s] 不備のあるレジマーク読取結果ファイルの退避に失敗しました。:レジマーク読取結果ファイル名[%s]',
                                     app_id, app_name, file[0])
                        sys.exit()

            # レジマーク読取結果ファイルのダミーファイルを作成する。
            logger.info('[%s:%s] リカバリ（ダミーファイル出力）を開始します。[品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]'
                        % (app_id, app_name, product_name, fabric_name, inspection_num,
                           inspection_date))
            result = output_dummy_file(product_name, fabric_name, inspection_num,
                                           inspection_date)
            if result:
                logger.info('[%s:%s] リカバリ（ダミーファイル出力）が終了しました。[品番, 反番, 検査番号, 検査日付]=[%s, %s, %s, %s]'
                            % (app_id, app_name, product_name, fabric_name, inspection_num,
                               inspection_date))

                logger.debug('[%s:%s] レジマーク読取結果ファイルの再確認を開始します。', app_id, app_name)
                result, sorted_files = get_regimark_file(file_path, file_pattern)

                if result:
                    pass
                else:
                    logger.error('[%s:%s] レジマーク読取結果ファイルの再確認に失敗しました。', app_id, app_name)
                    sys.exit()
            else:
                logger.error('[%s:%s] リカバリ（ダミーファイル出力）に失敗しました。' % (app_id, app_name))
                sys.exit()
            # ADD 20200626 KQRM 下吉 END

        # DEL 20200626 KQRM 下吉 START
        # else:
        # DEL 20200626 KQRM 下吉 END
        logger.info('[%s:%s] レジマーク読取結果ファイルを発見しました。:レジマーク読取結果ファイル名[%s]',
                    app_id, app_name, sorted_files)

        # DB共通処理を呼び出して、DB接続を行う
        logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
        result, conn, cur = create_connection()

        if result:
            logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
            sys.exit()

        while len(sorted_files) > 0:
            file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(sorted_files[0])).strftime(
                "%Y-%m-%d %H:%M:%S")

            logger.info('[%s:%s] %s処理を開始します。　[反番,　検査番号, 検査日付]=[%s, %s, %s]', app_id, app_name,
                        app_name, fabric_name, inspection_num, inspection_date)

            files = [x for x in sorted_files if
                     (fabric_name + "_" + inspection_num in x)]

            if len(files) == 0:
                break
            else:
                pass

            # 撮像完了通知ファイルを読込む
            logger.debug('[%s:%s] レジマーク読取結果ファイルの読込を開始します。', app_id, app_name)

            for file in sorted(files):
                regimark_list = []
                regimark_face = re.split('_', (file.split('\\'))[-1])[5]
                regimark_type = re.split('_', (file.split('\\'))[-1])[6]
                result, regimark_info = read_regimark_file(file)

                if result:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルの読込が終了しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, file)
                    logger.debug('[%s:%s] レジマーク情報 : %s', app_id, app_name, regimark_info)
                else:
                    logger.error('[%s:%s] レジマーク読取結果ファイルの読込に失敗しました。:レジマーク読取結果ファイルファイル名[%s]',
                                 app_id, app_name, file)
                    sys.exit()

                if len(regimark_info) == 0:
                    logger.info('[%s:%s] レジマーク読取結果ファイルが空です。:レジマーク読取結果ファイルファイル名[%s]',
                                app_id, app_name, file)
                    message = 'レジマーク読取結果ファイルが空です。:レジマーク読取結果ファイルファイル名[%s]' % file
                    empty_file = True
                    error_util.write_eventlog_warning(app_name, message)

                else:
                    regimark_list.append(regimark_info)
                    # 検査開始行数を取得する。
                    logger.debug('[%s:%s] 検査情報取得を開始します。', app_id, app_name)
                    result, inspection_info, conn, cur = select_inspection_info(conn, cur, product_name,
                                                                                fabric_name, inspection_num,
                                                                                imaging_starttime, unit_num)

                    if result:
                        logger.debug('[%s:%s] 検査情報取得が終了しました。' % (app_id, app_name))
                        logger.debug('[%s:%s] [反番, 検査番号]=[%s, %s] [検査開始行数, 最終行番, 検査方向] =[%s, %s, %s]。' % (
                            app_id, app_name, fabric_name, inspection_num, inspection_info[0], inspection_info[1],
                            inspection_info[2]))
                        pass
                    else:
                        logger.error('[%s:%s] 検査情報取得に失敗しました。' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()

                    # レジマーク検知画像の重複確認を行う。
                    logger.debug('[%s:%s] レジマーク検知画像の重複確認を開始します。', app_id, app_name)
                    result, confirmed_regimark = confirm_duplicate_registremark(regimark_list, fabric_name,
                                                                                inspection_num, inspection_info,
                                                                                image_height, image_width)
                    if result:
                        logger.debug('[%s:%s] レジマーク検知画像の重複確認が終了しました。' % (app_id, app_name))
                        logger.debug('[%s:%s] レジマークリスト %s' % (app_id, app_name, confirmed_regimark))
                        pass
                    else:
                        logger.error('[%s:%s] レジマーク検知画像の重複確認に失敗しました。' % (app_id, app_name))
                        sys.exit()

                    inspection_direction = inspection_info[2]

                    regimark_between_length_list = None

                    if (inspection_direction == 'S' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'X' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'Y' and re.split('\.', regimark_type)[0] == '2') or \
                            (inspection_direction == 'R' and re.split('\.', regimark_type)[0] == '2'):

                        # 設定レジマーク間長さ、領域伸び率X/Yを取得する。
                        logger.debug('[%s:%s] レジマークマスター情報取得を開始します。', app_id, app_name)
                        result, regimark_master, conn, cur = select_master_data(conn, cur, product_name)

                        if result:
                            logger.debug('[%s:%s] レジマークマスター情報取得が終了しました。' % (app_id, app_name))
                            logger.debug('[%s:%s] レジマークマスター情報 %s' % (app_id, app_name, regimark_master))
                            pass
                        else:
                            logger.error('[%s:%s] レジマークマスター情報取得に失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        # 長さ伸縮率、幅伸縮率を算出する。
                        conf_regimark_between_length = int(regimark_master[0][0])

                        # AIモデル未検査フラグ
                        ai_model_flag = regimark_master[0][3]

                        ### 実測レジマーク間長さの算出を行う。
                        logger.debug('[%s:%s] 実測レジマーク間長さの算出を開始します。', app_id, app_name)
                        result, regimark_between_length_list = \
                            calc_actual_regimark_between_length(confirmed_regimark, nonoverlap_image_length,
                                                                actual_image_overlap, conf_regimark_between_length,
                                                                actual_image_height, one_pix_length)
                        if result:
                            logger.debug('[%s:%s] 実測レジマーク間長さの算出が終了しました。' % (app_id, app_name))
                            logger.debug(
                                '[%s:%s] 実測レジマーク間長さリスト %s' % (app_id, app_name, regimark_between_length_list))
                            pass
                        else:
                            logger.error('[%s:%s] 実測レジマーク間長さの算出に失敗しました。' % (app_id, app_name))
                            sys.exit()

                        stretch_rate_x = float(regimark_master[0][1])
                        stretch_rate_y = float(regimark_master[0][2])

                        logger.debug('[%s:%s] 長さ伸縮率、幅伸縮率の算出を開始します。', app_id, app_name)
                        result, strech_ratio_list = calc_stretch_ratio(regimark_between_length_list,
                                                                       conf_regimark_between_length,
                                                                       stretch_rate_x, stretch_rate_y)
                        if result:
                            logger.debug('[%s:%s] 長さ伸縮率、幅伸縮率の算出が終了しました。' % (app_id, app_name))
                            logger.debug('[%s:%s] 長さ伸縮率、幅伸縮率リスト %s' % (app_id, app_name, strech_ratio_list))
                            pass
                        else:
                            logger.error('[%s:%s] 長さ伸縮率、幅伸縮率の算出に失敗しました。' % (app_id, app_name))
                            sys.exit()

                        if regimark_face == '2':
                            pass
                        else:
                            # レジマーク間距離CSVファイルを出力するフォルダの存在を確認する
                            logger.debug('[%s:%s] レジマーク間距離CSVファイルを出力するフォルダ存在チェックを開始します。', app_id, app_name)
                            result = exists_dir(output_file_path)

                            if result:
                                logger.debug('[%s:%s] レジマーク間距離CSVファイルを出力するフォルダ存在チェックが終了しました。', app_id, app_name)
                            else:
                                logger.error('[%s:%s] レジマーク間距離CSVファイルを出力するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                             app_id, app_name, output_file_path)
                                sys.exit()

                            # レジマーク間距離csvを出力する。
                            conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                            logger.debug('[%s:%s] レジマーク間距離CSVの出力を開始します。', app_id, app_name)

                            result, csv_file = output_regimark_between_length_file(confirmed_regimark,
                                                                                   regimark_between_length_list,
                                                                                   strech_ratio_list,
                                                                                   conf_regimark_len,
                                                                                   output_file_path, fabric_name,
                                                                                   inspection_num)

                            if result:
                                logger.debug('[%s:%s] レジマーク間距離CSVの出力が終了しました。' % (app_id, app_name))
                                pass
                            else:
                                logger.error('[%s:%s] レジマーク間距離csvの出力に失敗しました。' % (app_id, app_name))
                                sys.exit()

                            # レジマーク間距離CSVを移動する
                            logger.debug('[%s:%s] レジマーク間距離CSVの移動を開始します。:レジマーク読取結果ファイル名[%s]',
                                         app_id, app_name, file)
                            result = move_file(csv_file, csv_file_path)

                            if result:
                                logger.debug('[%s:%s] レジマーク間距離CSVの移動が終了しました。', app_id, app_name)
                            else:
                                logger.warning('[%s:%s] レジマーク間距離CSVの移動に失敗しました。:退避先フォルダ名[%s]',
                                               app_id, app_name, csv_file_path)

                if empty_file:
                    pass
                else:
                    # レジマーク情報をレジマーク情報テーブルに登録する
                    logger.debug('[%s:%s] レジマーク情報の登録を開始します。', app_id, app_name)
                    result, conn, cur = insert_regimark_info(conn, cur, confirmed_regimark,
                                                             regimark_between_length_list,
                                                             strech_ratio_list, product_name, fabric_name,
                                                             inspection_num,
                                                             conf_regimark_len, image_height, image_width,
                                                             regimark_type, regimark_face, imaging_starttime,
                                                             unit_num)

                    if result:
                        pass
                    else:
                        logger.error('[%s:%s] レジマーク情報の登録に失敗しました。:[反番,検査番号]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        sys.exit()

                    logger.info('[%s:%s] レジマーク情報の登録が終了しました。 [反番,　検査番号, 検査日付]=[%s, %s, %s]',
                                 app_id, app_name, fabric_name, inspection_num, inspection_date)
                    # コミットする
                    conn.commit()

                # レジマーク読取結果ファイルを退避するフォルダの存在を確認する
                logger.debug('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックを開始します。', app_id, app_name)
                result = exists_dir(backup_file_path)

                if result:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックが終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] レジマーク読取結果ファイルを退避するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                 app_id, app_name, output_file_path)
                    sys.exit()

                # レジマーク読取結果ファイルを退避フォルダに移動させる。

                retry_count = 0
                file = [file]
                while len(file) > 0:
                    logger.debug('[%s:%s] レジマーク読取結果ファイルの退避を開始します。:レジマーク読取結果ファイル名[%s]',
                                 app_id, app_name, file)
                    result = move_file(file[0], backup_file_path)

                    if result == permission_error:
                        if retry_count == file_bk_retry_count:
                            logger.error('[%s:%s] レジマーク読取結果ファイルの退避に失敗しました。:レジマーク読取結果ファイル名[%s]',
                                         app_id, app_name, file[0])
                            sys.exit()
                        else:
                            logger.debug('[%s:%s] レジマーク読取結果ファイルの退避を再実行します。', app_id, app_name, app_name)
                            file.append(file[0])
                            del file[0]
                            time.sleep(file_bk_sleep_time)
                            retry_count += 1
                    elif result:
                        logger.debug('[%s:%s] レジマーク読取結果ファイルの退避が終了しました。:退避先フォルダ[%s], レジマーク読取結果ファイル名[%s]',
                                     app_id, app_name, output_file_path, file[0])
                        sorted_files.remove(file[0])
                        file.remove(file[0])
                    else:
                        logger.error('[%s:%s] レジマーク読取結果ファイルの退避に失敗しました。:レジマーク読取結果ファイル名[%s]',
                                     app_id, app_name, file[0])
                        sys.exit()

            logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)

        # DB接続を切断
        logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
        close_connection(conn, cur)
        logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)

        # AIモデル未検査フラグを確認
        # 0の場合はNG行・列判定登録機能を呼び出す
        if ai_model_flag == 0 or ai_model_flag == None:

            logger.debug('[%s:%s] NG行・列判定登録機能呼出を開始します。', app_id, app_name)
            result = register_ng_info.main(product_name, fabric_name, inspection_num, imaging_starttime)
            if result:
                logger.debug('[%s:%s] NG行・列判定登録機能が終了しました。', app_id, app_name)
                result = True
                return result
            else:
                logger.debug('[%s:%s] NG行・列判定登録機能が失敗しました。', app_id, app_name)
                sys.exit()

        # 1の場合は処理を終了して、撮像枚数登録機能に戻り値を返す
        else:

            result = True
            return result

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
        result = False

    except:
        logger.error('[%s:%s] %s機能で予期しないエラーが発生しました。[%s]', app_id, app_name, app_name, traceback.format_exc())

        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))
        result = False

    finally:
        if conn is not None:
            # DB接続済の際はクローズする
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)
        else:
            # DB未接続の際は何もしない
            pass

    return result


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    main()
