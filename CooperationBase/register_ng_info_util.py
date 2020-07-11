# -*- coding: SJIS -*-
# NG行列判定機能
#
import configparser
import logging.config
import re
import sys

import cv2
import numpy as np

import db_util
import error_detail

# ログ設定取得
logging.config.fileConfig("D:/CI/programs/config/logging_register_ng_info.conf")
logger = logging.getLogger("register_ng_info")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_ng_info_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

#

master_data_dict = {'file_num': 0, 'register_flg': 1, 'selection_flg': 2, 'product_name': 3,
                    'inspection_param_num': 4, 'airbag_imagepath': 5, 'length': 6, 'width': 7,
                    'marker_color_flat': 8, 'marker_color_back': 9, 'auto_print': 10, 'auto_inspection_stop': 11,
                    'regimark_1_imagepath': 12, 'regimark_1_point_x': 13, 'regimark_1_point_y': 14,
                    'regimark_1_size_w': 15, 'regimark_1_size_h': 16, 'regimark_2_imagepath': 17,
                    'regimark_2_point_x': 18, 'regimark_2_point_y': 19, 'regimark_2_size_w': 20,
                    'regimark_2_size_h': 21, 'base_point_1_x': 22, 'base_point_1_y': 23, 'base_point_2_x': 24,
                    'base_point_2_y': 25, 'base_point_3_x': 26, 'base_point_3_y': 27, 'base_point_4_x': 28,
                    'base_point_4_y': 29, 'base_point_5_x': 30, 'base_point_5_y': 31,
                    'point_1_plus_direction_x': 32, 'point_1_plus_direction_y': 33, 'point_2_plus_direction_x': 34,
                    'point_2_plus_direction_y': 35, 'point_3_plus_direction_x': 36, 'point_3_plus_direction_y': 37,
                    'point_4_plus_direction_x': 38, 'point_4_plus_direction_y': 39, 'point_5_plus_direction_x': 40,
                    'point_5_plus_direction_y': 41, 'stretch_rate_x': 42, 'stretch_rate_y': 43,
                    'stretch_rate_x_upd': 44, 'stretch_rate_y_upd': 45, 'regimark_3_imagepath': 46,
                    'regimark_4_imagepath': 47, 'stretch_rate_auto_calc_flg': 48, 'width_coefficient': 49,
                    'correct_value': 50, 'black_thread_cnt_per_line': 51, 'measuring_black_thread_num': 52,
                    'camera_num': 53, 'column_cnt': 54, 'illumination_information': 55,
                    'start_regimark_camera_num': 56, 'end_regimark_camera_num': 57, 'line_length': 58,
                    'regimark_between_length': 59, 'taking_camera_cnt': 60, 'column_threshold_01': 61,
                    'column_threshold_02': 62, 'column_threshold_03': 63, 'column_threshold_04': 64,
                    'line_threshold_a1': 65, 'line_threshold_a2': 66, 'line_threshold_b1': 67,
                    'line_threshold_b2': 68, 'line_threshold_c1': 69, 'line_threshold_c2': 70,
                    'line_threshold_d1': 71, 'line_threshold_d2': 72, 'line_threshold_e1': 73,
                    'line_threshold_e2': 74, 'top_point_a': 75, 'top_point_b': 76, 'top_point_c': 77,
                    'top_point_d': 78, 'top_point_e': 79, 'ai_model_non_inspection_flg': 80, 'ai_model_name': 81}

line_name_dict = {1: 'A', 2: 'B', 3: 'C', 4: 'D', 5: 'E'}
line_num_dict = {'A': 1, 'B': 2, 'C': 3, 'D': 4, 'E': 5}


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
def create_connection(logger):
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：レジマーク情報取得
#
# 処理概要           ：1.レジマーク情報テーブルから処理対象反番、検査番号のレジマーク情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      レジマーク情報(行番号、開始レジマーク画像名、開始レジマーク座標、終了レジマーク画像名、終了レジマーク座標)
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_regimark_info(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'select line_num, start_regimark_file, start_regimark_point_resize, end_regimark_file, ' \
          'end_regimark_point_resize, len_stretchrate, width_stretchrate, face from regimark_info where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] レジマーク情報取得SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：検査マスタ情報取得
#
# 処理概要           ：1.品種登録情報テーブルからマスタ情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      マスタ情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_product_master_info(conn, cur, product_name, logger):
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'select * from mst_product_info where product_name = \'%s\' ' % product_name

    logger.debug('[%s:%s] 検査マスタ情報取得SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、品種登録情報テーブルからマスタ情報を取得する。
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG情報登録
#
# 処理概要           ：1.品種登録情報テーブルからマスタ情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      NG行
#                      NG列
#                      マスタ座標
#                      基準点からの距離(X)
#                      基準点からの距離(Y)
#                      連番
#                      NG画像名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_ng_info(conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point, ng_distance_x,
                   ng_distance_y, num, ng_file, undetected_image_flag_is_undetected, inspection_date, unit_num, logger):
    inspection_num = str(int(inspection_num))
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    if undetected_image_flag_is_undetected == 1:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s  where ng_image = \'%s\' and unit_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point,
                 ng_distance_x, ng_distance_y, ng_file, unit_num)

    elif undetected_image_flag_is_undetected == 2:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s where unti_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point,
                 ng_distance_x, ng_distance_y, unit_num)
    else:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s  where num = %s and ng_image = \'%s\' and unit_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point, ng_distance_x,
                 ng_distance_y, num, ng_file, unit_num)

    logger.debug('[%s:%s] NG情報登録SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、品種登録情報テーブルからマスタ情報を取得する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


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
def close_connection(conn, cur, logger):
    func_name = sys._getframe().f_code.co_name
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)
    return result, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG画像該当行特定
#
# 処理概要           ：1.開始レジマーク画像名とNG画像名の撮像番号を比較し、該当行番号を特定する。
#                      2.該当行番号のレジマーク情報と該当行数+1の行番号のレジマークを抽出する。
#
# 引数               ：レジマーク情報
#                      NG画像名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG行レジマーク情報
# ------------------------------------------------------------------------------------
def specific_line_num(regimark_info, ng_file, inspection_direction, logger):
    result = False
    line_info = None
    last_flag = 0
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        # 画像名の撮像番号から該当行番号を特定するため、画像名を[._]で分割する。
        sp_ng_file = re.split('[_.]', ng_file[1])
        sp_ng_file.append(ng_file[2])
        ng_face = int(sp_ng_file[4])

        logger.debug('[%s:%s] レジマーク情報 %s' % (app_id, app_name, regimark_info))
        logger.debug('[%s:%s] NG画像情報 %s, 検査方向 %s' % (app_id, app_name, sp_ng_file, inspection_direction))

        if inspection_direction[0] == 'S' or inspection_direction[0] == 'X':
            sp_regimark_file = [[x[:][0]] + re.split('[._]', x[:][1]) for x in regimark_info if
                                (int(sp_ng_file[6]) >= int(re.split('[._]', x[:][1])[6])) and ng_face == int(x[:][7])]
            logger.debug('[%s:%s] S・X方向 スプリットレジマーク情報 %s' % (app_id, app_name, sp_regimark_file))

            if len(sp_regimark_file) == 0:
                result = 'error'
                return result, line_info, last_flag, error, func_name
            else:
                pass

            # NG画像の撮像番号以下の最大値を該当行の開始レジマーク画像として特定する。
            line_num_index = max([int(y[7]) for y in sp_regimark_file])
            line_num = int([z[0] for z in sp_regimark_file if line_num_index == int(z[:][7])][0])
            line_info = sorted([i for i in regimark_info if
                                ((line_num == i[:][0]) or ((line_num + 1) == i[:][0])) and ng_face == int(i[:][7])])
            last_flag = 0

            if len(line_info) == 1:
                line_info = sorted([i for i in regimark_info if
                                    ((line_num == i[:][0]) or ((line_num -1) == i[:][0])) and ng_face == int(i[:][7])])
                last_flag = 1
            else:
                pass

        else:
            sp_regimark_file = [[x[:][0]] + re.split('[._]', x[:][3]) for x in regimark_info if
                                (int(sp_ng_file[6]) >= int(re.split('[._]', x[:][3])[6])) and ng_face == int(x[:][7])]
            logger.debug('[%s:%s] スプリットレジマーク情報 %s' % (app_id, app_name, sp_ng_file))

            if len(sp_regimark_file) == 0:
                result = 'error'
                return result, line_info, last_flag, error, func_name
            else:
                pass

            # NG画像の撮像番号以下の最大値を該当行の開始レジマーク画像として特定する。
            line_num_index = max([int(y[7]) for y in sp_regimark_file])
            line_num = int([z[0] for z in sp_regimark_file if line_num_index == int(z[:][7])][0])
            line_info = sorted([i for i in regimark_info if
                                ((line_num == i[:][0]) or ((line_num - 1) == i[:][0])) and ng_face == int(i[:][7])],
                               reverse=True)
            last_flag = 0

            if len(line_info) == 1:
                line_info = sorted([i for i in regimark_info if
                                    ((line_num == i[:][0]) or ((line_num + 1) == i[:][0])) and ng_face == int(i[:][7])], reverse=True)
                last_flag = 1
            else:
                pass

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, line_info, last_flag, error, func_name


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像とマスタ画像の行・列の長さの比率算出
#
# 処理概要           ：1.レジマーク間長さ/レジマーク間幅の撮像枚数を算出する。
#                      2.撮像画像上のレジマーク間長さ[pix]・レジマーク間幅[pix]を算出する。
#                      3.マスタ画像上のレジマーク間長さ[pix]を算出する。
#                      4.撮像画像とマスタ画像のレジマーク間長さ・レジマーク間幅の比率を算出する。
#
# 引数               ：レジマーク情報
#                      オーバーラップ除外分1撮像画像の幅(X方向長さ)
#                      オーバーラップ除外分1撮像画像の高さ(Y方向長さ)
#                      オーバーラップ分の幅(X方向長さ)
#                      オーバーラップ分の高さ(Y方向長さ)
#                      リサイズ後画像の幅(X方向長さ)
#                      リサイズ後画像の高さ(Y方向長さ)
#                      マスタ情報
#                      マスタ画像の幅(X方向長さ)
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      長さ比率
#                      幅比率
#                      設定レジマーク間長さ[pix]
#
# ------------------------------------------------------------------------------------
def calc_length_ratio(regimark_info, line_info, nonoverlap_image_height_pix,
                      overlap_height_pix, resize_image_height, mst_data, master_image_width,
                      actual_image_height, inspection_direction, logger):
    result = False
    regimark_length_ratio = None
    regimark_width_ratio = None
    conf_regimark_between_length_pix = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # マスタ情報から必要な情報を取得する。※カラム数が多いためマスタ情報辞書を利用して情報を取得する。
        conf_regimark_between_length = int(mst_data[master_data_dict['regimark_between_length']])
        length = int(mst_data[master_data_dict['length']])
        regimark_1_point_y = int(mst_data[master_data_dict['regimark_1_point_y']])
        regimark_2_point_y = int(mst_data[master_data_dict['regimark_2_point_y']])

        ng_face = int(line_info[0][5])
        # マスタ画像幅[pix]:マスタ画像実測幅[mm]=X:設定レジマーク間長さ[mm]
        conf_regimark_between_length_pix = master_image_width * conf_regimark_between_length / length

        # NG画像が含まれる行情報は以下形式で取得
        # [(1, 'S354_380613-0AC_20191120_01_1_01_00001.jpg', '(619,646)',
        # 'S354_380613-0AC_20191120_01_1_01_00010.jpg', '(619,646)', '1'),
        # (2, 'S354_380613-0AC_20191120_01_1_01_00011.jpg', '(619,646)',
        # 'S354_380613-0AC_20191120_01_1_01_00020.jpg', '(619,646)', '1')]
        # 検査方向が S, Xの場合、開始レジマーク、次行開始レジマークの撮像番号、座標を抽出する。
        if len(regimark_info) != 1 and len(line_info) == 2:
            if inspection_direction == 'S' or inspection_direction == 'X':
                # 行番号、開始レジマークファイル名、座標が必要。
                sp_regimark_list = [[x[0]] + re.split('[._]', x[:][1]) + [x[2]] + [x[3]] + [x[4]]
                                    for x in line_info]
                start_image_num = int(sp_regimark_list[0][7])
                start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[1])

                next_start_image_num = int(sp_regimark_list[1][7])
                next_start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[1][9])))[1])

            # 検査方向が Y, Rの場合、終了レジマーク、次行終了レジマークの撮像番号、座標を抽出する。
            else:
                sp_regimark_list = [[x[0]] + [x[1]] + [x[2]] + re.split('[._]', x[:][3]) + [x[4]]
                                    for x in line_info]
                print(sp_regimark_list)

                start_image_num = int(sp_regimark_list[0][9])
                start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[1])

                next_start_image_num = int(sp_regimark_list[1][9])
                next_start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[1][11])))[1])

            # 開始-次行開始間長さ、開始-終了間幅の撮像枚数を算出する。
            if start_image_num != next_start_image_num:
                start_between_x_image_count = abs(next_start_image_num - start_image_num) - 1
            else:
                start_between_x_image_count = 0

            # 撮像枚数×1撮像画像の高さ(オーバーラップ除外分)＋(リサイズ画像高さ-[開始レジマークy座標]-[オーバーラップ分高さ])＋[次行開始レジマークy座標]
            start_regimark_x_pix = start_between_x_image_count * nonoverlap_image_height_pix + (
                    resize_image_height - start_regimark_y - overlap_height_pix) + next_start_regimark_y

             # 比率算出
            regimark_length_ratio = start_regimark_x_pix / conf_regimark_between_length_pix

        else:
            stretch_rate_x = float(mst_data[master_data_dict['stretch_rate_x']])
            regimark_x_pix = (conf_regimark_between_length * (
                        stretch_rate_x / 100)) * resize_image_height / actual_image_height
            regimark_length_ratio = regimark_x_pix / conf_regimark_between_length_pix

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, regimark_length_ratio, conf_regimark_between_length_pix, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG位置特定
#
# 処理概要           ：1.開始レジマーク画像名とNG画像名の撮像番号を比較し、該当行番号を特定する。
#
# 引数               ：レジマーク情報
#                      NG画像情報
#                      オーバーラップ除外分1撮像画像の幅(X方向長さ)
#                      オーバーラップ除外分1撮像画像の高さ(Y方向長さ)
#                      オーバーラップ分の幅(X方向長さ)
#                      オーバーラップ分の高さ(Y方向長さ)
#                      リサイズ後画像の幅(X方向長さ)
#                      リサイズ後画像の高さ(Y方向長さ)
#                      レジマーク間長さ比率
#                      レジマーク間幅比率
#                      マスタ情報
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      マスタ画像上のNG座標(X座標)
#                      マスタ画像上のNG座標(Y座標)
#
# ------------------------------------------------------------------------------------
def specific_ng_point(line_info, ng_image_info, nonoverlap_image_width_pix, nonoverlap_image_height_pix,
                      overlap_width_pix, overlap_height_pix, resize_image_height, resize_image_width,
                      regimark_length_ratio, mst_data, inspection_direction, master_image_width,
                      master_image_height, actual_image_width, actual_image_overlap, logger):
    result = False
    length_on_master = None
    width_on_master = None
    plus_direction = None
    ng_face = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        # マスタ情報から必要な情報を取得する。※カラム数が多いためマスタ情報辞書を利用して情報を取得する。
        regimark_1_point_x = int(mst_data[master_data_dict['regimark_1_point_x']])
        regimark_2_point_x = int(mst_data[master_data_dict['regimark_2_point_x']])
        regimark_1_point_y = int(mst_data[master_data_dict['regimark_1_point_y']])
        regimark_2_point_y = int(mst_data[master_data_dict['regimark_2_point_y']])
        master_width = int(mst_data[master_data_dict['width']])

        stretch_rate_x = float(line_info[0][5])
        stretch_rate_y = float(line_info[0][6])
         # 開始レジマークの撮像番号、座標を抽出する。
        sp_ng_file = re.split('[_.]', ng_image_info[1]) + [ng_image_info[2]]
        ng_face = int(sp_ng_file[4])

        if inspection_direction == 'S' or inspection_direction == 'X':
            logger.debug('[%s:%s] 検査方向S or X' % (app_id, app_name))
            sp_regimark_list = [[x[0]] + re.split('[._]', x[:][1]) + [x[2]] + [x[3]] + [x[4]]
                                for x in line_info]

            start_image_num = int(sp_regimark_list[0][7])
            start_regimark_x = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[0])
            start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[1])
            start_camera_num = int(sp_regimark_list[0][6])

            regimark_x = regimark_1_point_x

            if inspection_direction == 'S' and ng_face == 2:
                logger.debug('[%s:%s] 検査方向S 検反部2' % (app_id, app_name))
                regimark_y = master_image_height - regimark_1_point_y
            elif inspection_direction == 'X' and ng_face == 1:
                logger.debug('[%s:%s] 検査方向X 検反部1' % (app_id, app_name))
                regimark_y = master_image_height - regimark_1_point_y
            else:
                regimark_y = regimark_1_point_y

        else:
            logger.debug('[%s:%s] 検査方向Y or R' % (app_id, app_name))
            sp_regimark_list = [[x[0]] + [x[1]] + [x[2]] + re.split('[._]', x[:][3]) + [x[4]]
                                for x in line_info]

            start_image_num = int(sp_regimark_list[0][9])
            start_regimark_x = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[0])
            start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[1])
            start_camera_num = int(sp_regimark_list[0][8])

            regimark_x = master_image_width - regimark_2_point_x

            if inspection_direction == 'Y' and ng_face == 2:
                logger.debug('[%s:%s] 検査方向Y 検反部2' % (app_id, app_name))
                regimark_y = master_image_height - regimark_2_point_y
            elif inspection_direction == 'R' and ng_face == 1:
                logger.debug('[%s:%s] 検査方向R 検反部1' % (app_id, app_name))
                regimark_y = master_image_height - regimark_2_point_y
            else:
                regimark_y = regimark_2_point_y

        ng_image_num = int(sp_ng_file[6])
        ng_x = int(re.split(',', (re.sub('[()]', '', sp_ng_file[8])))[0])
        ng_y = int(re.split(',', (re.sub('[()]', '', sp_ng_file[8])))[1])

        ng_camera_num = int(sp_ng_file[5])

        if ng_image_num == start_image_num:
            logger.debug('[%s:%s] 撮像番号等しい' % (app_id, app_name))
            ng_x_pix = abs(start_regimark_y - ng_y)
            if start_regimark_y < ng_y:
                x_plus_direction = 1
            else:
                x_plus_direction = -1
        else:
            between_x_image_count = ng_image_num - start_image_num - 1
            # 撮像枚数×1撮像画像の高さ(オーバーラップ除外分)＋(リサイズ画像高さ-[開始レジマークy座標]-[オーバーラップ分高さ])＋[NG画像y座標]
            ng_x_pix = between_x_image_count * nonoverlap_image_height_pix + (
                    resize_image_height - start_regimark_y - overlap_height_pix) + ng_y
            x_plus_direction = 1

        if start_camera_num == ng_camera_num:
            logger.debug('[%s:%s] カメラ番号等しい' % (app_id, app_name))
            between_y_image_count = 0
        else:
            between_y_image_count = abs(ng_camera_num - start_camera_num) - 1

        if ng_face == 1:
            if start_camera_num == ng_camera_num:
                logger.debug('[%s:%s] 検反部1 カメラ番号等しい' % (app_id, app_name))
                ng_y_pix = abs(start_regimark_x - ng_x)
                ng_y_mm = ng_y_pix * actual_image_width / resize_image_width
                plus_direction = 1
            elif start_camera_num > ng_camera_num:
                logger.debug('[%s:%s] 検反部1 レジマークカメラ番号＞NGカメラ番号' % (app_id, app_name))
                # 撮像枚数×1撮像画像の幅(オーバーラップ除外分)＋(リサイズ画像幅-[開始レジマークx座標]-[オーバーラップ分幅])＋[NG画像x座標]
                register_image_fraction = (resize_image_width - start_regimark_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (ng_x * actual_image_width / resize_image_width)
                plus_direction = -1
            else:
                logger.debug('[%s:%s] 検反部1 レジマークカメラ番号＜NGカメラ番号' % (app_id, app_name))
                # 撮像枚数×1撮像画像の幅(オーバーラップ除外分)＋(リサイズ画像幅-[NG画像x座標]-[オーバーラップ分幅])＋[開始レジマーク x座標]
                register_image_fraction = (resize_image_width - ng_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (start_regimark_x * actual_image_width / resize_image_width)
                plus_direction = 1

        else:
            if start_camera_num == ng_camera_num:
                logger.debug('[%s:%s] 検反部2 カメラ番号等しい' % (app_id, app_name))
                ng_y_pix = abs(start_regimark_x - ng_x)
                ng_y_mm = ng_y_pix * actual_image_width / resize_image_width
                plus_direction = 1
            elif start_camera_num > ng_camera_num:
                logger.debug('[%s:%s] 検反部2 レジマークカメラ番号＞NGカメラ番号' % (app_id, app_name))
                # 撮像枚数×1撮像画像の幅(オーバーラップ除外分)＋(リサイズ画像幅-[開始レジマークx座標]-[オーバーラップ分幅])＋[NG画像x座標]
                register_image_fraction = (resize_image_width - ng_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (start_regimark_x * actual_image_width / resize_image_width )
                plus_direction = 1
            else:
                logger.debug('[%s:%s] 検反部2 レジマークカメラ番号＜NGカメラ番号' % (app_id, app_name))
                # 撮像枚数×1撮像画像の幅(オーバーラップ除外分)＋(リサイズ画像幅-[NG画像x座標]-[オーバーラップ分幅])＋[開始レジマーク x座標]
                register_image_fraction = (resize_image_width - start_regimark_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (ng_x * actual_image_width / resize_image_width )
                plus_direction = -1

        # 比率計算
        between_length_on_master = ng_x_pix * (stretch_rate_x / 100 )  / regimark_length_ratio
        between_width_on_master = master_image_height * ng_y_mm / (stretch_rate_y / 100)  / master_width

        # マスタ画像上のNG座標特定
        length_on_master = regimark_x + (between_length_on_master * x_plus_direction)
        width_on_master = regimark_y + (between_width_on_master * plus_direction)

        if inspection_direction == 'S' and ng_face == 2:
            logger.debug('[%s:%s] 検査方向S 検反部2 Y軸反転' % (app_id, app_name))
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'X' and ng_face == 1:
            logger.debug('[%s:%s] 検査方向X 検反部1 Y軸反転' % (app_id, app_name))
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'Y' and ng_face == 1:
            logger.debug('[%s:%s] 検査方向Y 検反部1 X軸反転' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master

        elif inspection_direction == 'Y' and ng_face == 2:
            logger.debug('[%s:%s] 検査方向Y 検反部2 X,Y軸反転' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'R' and ng_face == 1:
            logger.debug('[%s:%s] 検査方向R 検反部1 X,Y軸反転' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'R' and ng_face == 2:
            logger.debug('[%s:%s] 検査方向R 検反部2 X軸反転' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master

        else:
            pass

        result = True
    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)
        return result, length_on_master, width_on_master, ng_face, error, func_name

    return result, length_on_master, width_on_master, ng_face, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG行列特定
#
# 処理概要           ：1.頂点座標よりNG箇所の行・列を特定する。
#
# 引数               ：レジマーク情報
#                      マスタ画像上のNG座標(X座標)
#                      マスタ画像上のNG座標(Y座標)
#                      マスタ情報
#                      設定レジマーク間長さ[pix]
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG画像行・列情報
# ------------------------------------------------------------------------------------
def specific_ng_line_colum(line_info, length_on_master, width_on_master, mst_data, conf_regimark_between_length_pix,
                           inspection_direction, last_flag, logger):
    result = False
    judge_result = None
    top_points = []
    last_top_points = []
    next_top_points = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # マスタ情報から列数を取得する。※カラム数が多いためマスタ情報辞書を利用して情報を取得する。
        colum_count = mst_data[master_data_dict['column_cnt']]
        ng_point = (length_on_master, width_on_master)

        # 列数に応じたN行目の頂点座標抽出
        for i in range(colum_count):
            top_point = mst_data[master_data_dict['top_point_a'] + i]
            sp_top_points = re.split('\),', (re.sub('\(|\)$| ', '', top_point)))
            top_points.append([[int(re.split(',', x)[0]), int(re.split(',', x)[1])] for x in sp_top_points])
            last_top_points.append(
                [[round(int(re.split(',', x)[0]) - conf_regimark_between_length_pix), int(re.split(',', x)[1])] for x in
                 sp_top_points])
            next_top_points.append(
                [[round(int(re.split(',', x)[0]) + conf_regimark_between_length_pix), int(re.split(',', x)[1])] for x in
                 sp_top_points])

        if last_flag == 0:
            line_num = int(line_info[0][0])
        else:
            if len(line_info) == 1:
                line_num = int(line_info[0][0])
            else:
                line_num = int(line_info[1][0])
        # N行目の頂点座標内外判定
        # 内側と判定された時点で結果を返却する
        for j in range(len(top_points)):
            array_top_points = np.array(top_points[j])
            judge_line = cv2.pointPolygonTest(array_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                judge_result = [line_num, line_name_dict[j + 1]]
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        # N-1行目の頂点座標内外判定
        # 内側と判定された時点で結果を返却する
        for k in range(len(last_top_points)):
            array_last_top_points = np.array(last_top_points[k])
            judge_line = cv2.pointPolygonTest(array_last_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                if inspection_direction == 'S' or inspection_direction == 'X':
                    logger.debug('[%s:%s] 検査方向S or X' % (app_id, app_name))
                    line = line_num - 1
                else:
                    logger.debug('[%s:%s] 検査方向Y or R' % (app_id, app_name))
                    line = line_num - 1

                judge_result = [line, line_name_dict[k + 1]]
                length_on_master = length_on_master + conf_regimark_between_length_pix
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        # N+1行目の頂点座標内外判定
        # 内側と判定された時点で結果を返却する
        for l in range(len(next_top_points)):
            array_next_top_points = np.array(next_top_points[l])
            judge_line = cv2.pointPolygonTest(array_next_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                if inspection_direction == 'S' or inspection_direction == 'X':
                    logger.debug('[%s:%s] 検査方向S or X' % (app_id, app_name))
                    line = line_num + 1
                else:
                    logger.debug('[%s:%s] 検査方向Y or R' % (app_id, app_name))
                    line = line_num + 1

                judge_result = [line, line_name_dict[l + 1]]
                length_on_master = length_on_master - conf_regimark_between_length_pix
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        result = True
        logger.debug('[%s:%s] 頂点座標外のNG' % (app_id, app_name))

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, judge_result, length_on_master, width_on_master, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG行列特定（頂点座標外）
#
# 処理概要           ：1.行閾値、列閾値情報よりNG箇所の行・列を特定する。
#
# 引数               ：レジマーク情報
#                      マスタ画像上のNG座標(X座標)
#                      マスタ画像上のNG座標(Y座標)
#                      マスタ情報
#                      設定レジマーク間長さ[pix]
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG画像行・列情報
# ------------------------------------------------------------------------------------
def specific_ng_line_colum_border(line_info, length_on_master, width_on_master, mst_data,
                                  conf_regimark_between_length_pix, inspection_direction, last_flag, logger):
    result = False
    judge_result = None
    colum_result = None
    line_result = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # マスタ情報から列数を取得する。※カラム数が多いためマスタ情報辞書を利用して情報を取得する。
        colum_count = mst_data[master_data_dict['column_cnt']]

        # 列数分隙間判定を行う
        for i in range(colum_count):
            # 列特定を行う
            # 1列目は列閾値01未満を確認
            if i == 0:
                colum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + i])
                if width_on_master <= colum_threshold:
                    colum_result = line_name_dict[i + 1]
                    break
                else:
                    pass
            # 2列目から(列数-1)列目は列閾値01から列閾値0(N-2)のそれぞれの間を確認
            elif i == colum_count - 1:
                colum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + (i - 1)])
                if colum_threshold < width_on_master:
                    colum_result = line_name_dict[i + 1]
                    break
                else:
                    pass
            # 上記以外(最終列)は列閾値0(N-1)以上を確認
            else:
                min_cloum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + (i - 1)])
                max_cloum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + i])
                if min_cloum_threshold < width_on_master <= max_cloum_threshold:
                    colum_result = line_name_dict[i + 1]
                    break

        # 行閾値を取得
        find_str = colum_result.lower()
        find_str_1 = 'line_threshold_' + find_str + '1'
        find_str_2 = 'line_threshold_' + find_str + '2'
        min_line_threshold = int(mst_data[master_data_dict[find_str_1]])
        max_line_threshold = int(mst_data[master_data_dict[find_str_2]])


        if last_flag == 0:
            line_num = int(line_info[0][0])
        else:
            if len(line_info) == 1:
                line_num = int(line_info[0][0])
            else:
                line_num = int(line_info[1][0])
        # 特定した列の行閾値(最小)以下の場合、N-1行とN行の行特定を行う
        if length_on_master <= min_line_threshold:
            # N-1行の行閾値(最大)とN行の行閾値(最小)の間の場合、各閾値からの絶対値で該当行を特定する
            if (max_line_threshold - conf_regimark_between_length_pix) < length_on_master <= min_line_threshold:
                abs_line = abs(length_on_master - min_line_threshold)
                abs_last_line = abs(length_on_master - (max_line_threshold - conf_regimark_between_length_pix))
                if abs_line <= abs_last_line:
                    line_result = line_num
                else:
                    if inspection_direction == 'S' or inspection_direction == 'X':
                        line_result = line_num - 1
                    else:
                        line_result = line_num - 1

                    length_on_master = length_on_master + conf_regimark_between_length_pix
            else:
                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_result = line_num - 1
                else:
                    line_result = line_num - 1

                length_on_master = length_on_master + conf_regimark_between_length_pix
        # 特定した列の行閾値(最小)と行閾値(最大)の間の場合、N行と特定する
        elif min_line_threshold < length_on_master <= max_line_threshold:
            line_result = line_num

        # 特定した列の行閾値(最大)以上の場合、N行とN+1行の行特定を行う
        elif max_line_threshold <= length_on_master:
            # N行の行閾値(最大)とN+1行の行閾値(最小)の間の場合、各閾値からの絶対値で該当行を特定する
            if max_line_threshold < length_on_master <= (min_line_threshold + conf_regimark_between_length_pix):
                abs_line = abs(length_on_master - max_line_threshold)
                abs_next_line = abs(length_on_master - (min_line_threshold + conf_regimark_between_length_pix))
                if abs_next_line <= abs_line:
                    line_result = line_num
                else:
                    if inspection_direction == 'S' or inspection_direction == 'X':
                        line_result = line_num + 1
                    else:
                        line_result = line_num + 1

                    length_on_master = length_on_master - conf_regimark_between_length_pix
            else:
                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_result = line_num + 1
                else:
                    line_result = line_num + 1

                length_on_master = length_on_master - conf_regimark_between_length_pix

        judge_result = [line_result, colum_result]
        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, judge_result, length_on_master, width_on_master, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：基準点からのNG距離算出
#
# 処理概要           ：1.基準点からNG箇所までの距離を算出する。
#
# 引数               ：レジマーク情報
#                      マスタ画像上のNG座標(X座標)
#                      マスタ画像上のNG座標(Y座標)
#                      NG画像行・列情報
#                      マスタ情報
#                      マスタ画像の幅(X方向長さ)
#                      マスタ画像の高さ(Y方向長さ)
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      基準点からのNG距離情報
# ------------------------------------------------------------------------------------
def calc_distance_from_basepoint(length_on_master, width_on_master, judge_result, mst_data, master_image_width,
                                 master_image_height, logger):
    result = False
    ng_dist = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        line_name = judge_result[1]
        line_num = str(line_num_dict[line_name])

        # マスタ情報から必要な情報を取得する。※カラム数が多いためマスタ情報辞書を利用して情報を取得する。
        length = master_data_dict['length']
        width = master_data_dict['width']
        find_str_basepoint_x = 'base_point_' + line_num + '_x'
        find_str_basepoint_y = 'base_point_' + line_num + '_y'
        find_str_plusdirect_x = 'point_' + line_num + '_plus_direction_x'
        find_str_plusdirect_y = 'point_' + line_num + '_plus_direction_y'

        length = int(mst_data[length])
        width = int(mst_data[width])
        base_point_x = int(mst_data[master_data_dict[find_str_basepoint_x]])
        base_point_y = int(mst_data[master_data_dict[find_str_basepoint_y]])
        plus_direction_x = mst_data[master_data_dict[find_str_plusdirect_x]]
        plus_direction_y = mst_data[master_data_dict[find_str_plusdirect_y]]

        logger.debug('[%s:%s] 基準点 [ %s, %s ]' % (app_id, app_name, base_point_x, base_point_y))
        logger.debug('[%s:%s] プラス方向 [ %s, %s ]' % (app_id, app_name, plus_direction_x, plus_direction_y))
        # 基準点からの距離[pix]を算出する
        x_point_from_basepoint = length_on_master - base_point_x
        y_point_from_basepoint = base_point_y - width_on_master
        x_dist_ratio = length / master_image_width
        y_dist_ratio = width / master_image_height

        # マスタ情報のプラス方向情報から符号反転有無を確認し、基準点からの距離[mm]を算出する。
        if x_point_from_basepoint >= 0:
            if plus_direction_x == 0:
                x_dist_mm = (x_point_from_basepoint * -1) * x_dist_ratio
            else:
                x_dist_mm = x_point_from_basepoint * x_dist_ratio
        else:
            if plus_direction_x == 0:
                x_dist_mm = (x_point_from_basepoint * -1) * x_dist_ratio
            else:
                x_dist_mm = x_point_from_basepoint * x_dist_ratio

        if y_point_from_basepoint >= 0:
            if plus_direction_y == 0:
                y_dist_mm = y_point_from_basepoint * y_dist_ratio
            else:
                y_dist_mm = (y_point_from_basepoint * -1) * y_dist_ratio
        else:
            if plus_direction_y == 0:
                y_dist_mm = y_point_from_basepoint * y_dist_ratio
            else:
                y_dist_mm = (y_point_from_basepoint * -1) * y_dist_ratio

        result = True
        ng_dist = [round(x_dist_mm / 10), round(y_dist_mm / 10)]

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, ng_dist, error, func_name
