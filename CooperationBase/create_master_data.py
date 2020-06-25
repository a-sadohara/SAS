# -*- coding: SJIS -*-
# マスタ情報定義ファイル作成機能
#
import configparser
import csv
import glob
import re
import sys
import logging.config
import traceback

import error_detail

# ログ設定
import error_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_create_master_data.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# 設定ファイル読み込み（共通設定）
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/create_master_data_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：csvファイル読込
#
# 処理概要           ：1.閾値情報を作成する品名をｃSVファイルから読込む
#
# 引数               ：CSVファイルパス
#
# 戻り値             ：読込結果
# ------------------------------------------------------------------------------------
def read_csv(csv_file):
    read_csv_line = None
    try:
        with open(csv_file, 'r', encoding='shift_jis') as f:
            reader = csv.reader(f)
            read_csv_line = [row for row in reader]
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, read_csv_line


# ------------------------------------------------------------------------------------
# 処理名             ：iniファイル読込
#
# 処理概要           ：1.各品番のiniファイルを読込む
#
# 引数               ：iniファイルパス
#
# 戻り値             ：読込結果
# ------------------------------------------------------------------------------------
def readfile(filename):
    config_data = None
    try:
        file = open(filename)
        config_data = [line.strip() for line in file.readlines()]
        file.close()
        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, config_data


# ------------------------------------------------------------------------------------
# 処理名             ：列数計算
#
# 処理概要           ：1.iniファイルのデータから、列数を計算する
#
# 引数               ：iniファイル読込結果
#
# 戻り値             ：読込結果
# ------------------------------------------------------------------------------------
def column_num_calc(airbag_data):
    column_num = None
    try:
        vertex_num = [num for num in airbag_data if 'Number=' in num]
        # Number=0のものは対象列が存在しない品番
        column_num = len([column for column in vertex_num if 'Number=0' not in column])
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, column_num


# ------------------------------------------------------------------------------------
# 処理名             ：座標取得
#
# 処理概要           ：1.列数とiniファイルから閾値の座標を取得する
#
# 引数               ：列数
#                      iniファイルパス
#
# 戻り値             ：座標リスト
# ------------------------------------------------------------------------------------
def get_coordinate(colum_num, airbag_ini_file):
    coords = None
    try:
        coord_inifile = configparser.ConfigParser()
        coord_inifile.read(airbag_ini_file, 'SJIS')

        coords = []
        for colum in range(0, colum_num):
            one_colum_coords = []
            section = 'AIRBAG00' + str(colum)
            for key in coord_inifile.options(section):
                if ('coord' in key):
                    coord = coord_inifile.get(section, key)
                    one_colum_coords.append("(" + coord.replace(' ', ',') + ")")

            coords.append(one_colum_coords)
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, coords


# ------------------------------------------------------------------------------------
# 処理名             ：行閾値算出
#
# 処理概要           ：1.iniファイルから取得した閾値座標から、行閾値を算出する。
#
# 引数               ：座標リスト
#                      列数
#
# 戻り値             ：行閾値リスト
# ------------------------------------------------------------------------------------
def get_x_threshold(coord_list, colum_num):
    x_threshold = []
    x_threshold_list = []
    try:

        for i in range(colum_num):
            coords = []
            for j in range(len(coord_list[i])):
                sp_coord_list = (re.sub('[()]', '', coord_list[i][j])).split(',')
                coords.append(sp_coord_list)

            x_coords = []
            for k in range(len(coords)):
                x_coords.append(int(coords[k][0]))

            x_min = min([int(x_min_coords) for x_min_coords in x_coords if 0 < int(x_min_coords)])
            x_max = max(x_coords)
            x = x_min, x_max
            x_threshold.append(x)

        x_list = []
        limit_column = 5
        for l in range(len(x_threshold)):
            str(x_threshold[l]).split(',')
            x_list.append(re.sub('[()| ]', '', str(x_threshold[l])).split(','))

        for n in range(limit_column):

            if n <= (len(x_list) - 1):
                x_threshold_list.append(int(x_list[n][0]))
                x_threshold_list.append(int(x_list[n][1]))
            else:
                no_th = ','
                x_threshold_list.append(no_th)
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, x_threshold_list


# ------------------------------------------------------------------------------------
# 処理名             ：列閾値算出
#
# 処理概要           ：1.iniファイルから取得した閾値座標から、列閾値を算出する。
#
# 引数               ：座標リスト
#                      列数
#
# 戻り値             ：列閾値リスト
# ------------------------------------------------------------------------------------
def get_y_coordinate(coord_list, colum_num):
    y_threshold = []
    y_threshold_list = []
    try:
        for i in range(colum_num):
            coords = []
            for j in range(len(coord_list[i])):
                sp_coord_list = (re.sub('[()]', '', coord_list[i][j])).split(',')
                coords.append(sp_coord_list)

            y_coords = []
            for k in range(len(coords)):
                y_coords.append(int(coords[k][1]))

            y_min = min([y_min_coords for y_min_coords in y_coords if 0 < int(y_min_coords)])
            y_max = max([y_max_coords for y_max_coords in y_coords if 0 < int(y_max_coords)])
            y = y_min, y_max
            y_threshold.append(y)

        y_list = []

        for l in range(0, colum_num):
            str(y_threshold[l]).split(',')
            y_list.append(re.sub('[()| ]', '', str(y_threshold[l])).split(','))

        for n in range(0, colum_num - 1):
            y_th = round(int(y_list[n][1]) + (int(y_list[n + 1][0]) - int(y_list[n][1])) / 2)
            y_threshold_list.append(y_th)

        no_th = ''
        if 5 - int(colum_num) > 0:
            for m in range(5 - int(colum_num)):
                y_threshold_list.append(no_th)
        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, y_threshold_list


# ------------------------------------------------------------------------------------
# 処理名             ：閾値情報CSV作成
#
# 処理概要           ：1.取得した閾値情報をCSVで出力する。
#
# 引数               ：閾値情報
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def create_master_data_file(output_file_path, master_data):
    try:
        master_file_path = output_file_path + "\\" + "閾値情報.CSV"

        master_file = open(master_file_path, 'a')

        master_file.write(
            '品名,撮像カメラ数,列閾値01,列閾値02,列閾値03,列閾値04,行閾値A1,行閾値A2,行閾値B1,行閾値B2,行閾値C1,行閾値C2,行閾値D1,行閾値D2,行閾値E1,行閾値E2,AIモデル名')
        master_file.write("\n")

        for i in range(len(master_data)):
            master_file.write(master_data[i][0])
            master_file.write(master_data[i][1])
            master_file.write(master_data[i][2])
            master_file.write(master_data[i][3])
            master_file.write(master_data[i][4])
            master_file.write("\n")
        master_file.close()
        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：閾値情報を作成する
#
# 引数               ：iniファイル格納パス
#                      インプットCSVファイル格納パス
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main(file_path, csv_file, output_file_path):
    error_file_name = None
    result = False

    try:
        ## 設定ファイルから値を取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')

        # 変数設定
        file_list = []
        master_data = []
        product_name = []
        camera_num = ',27'

        logger.info('[%s:%s] %s処理を起動します。' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] インプットCSVファイルの読込を開始します。' % (app_id, app_name))
        tmp_result, read_result_list = read_csv(csv_file)
        if tmp_result:
            logger.debug('[%s:%s] インプットCSVファイルの読込が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] インプットCSVファイルの読込が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] %s処理を開始します。' % (app_id, app_name, app_name))
        for i in range(len(read_result_list)):
            file_pattern = 'AirBagCoord' + str(read_result_list[i][0]) + '.ini'

            file_list.append(glob.glob(file_path + '\\' + file_pattern)[0])
            product_name.append(read_result_list[i][1])

        for i in range(len(file_list)):
            logger.debug('[%s:%s] 対象マスタ [品番=%s]' % (app_id, app_name, read_result_list[i][1]))

            master_data_list = [product_name[i], camera_num]

            logger.debug('[%s:%s] マスタファイル読込を開始します。' % (app_id, app_name))
            tmp_result, airbag_data = readfile(file_list[i])
            if tmp_result:
                logger.debug('[%s:%s] マスタファイル読込が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] マスタファイル読込が失敗しました。' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] 列数計算を開始します。' % (app_id, app_name))
            # AirBagCoord.iniから列数を計算する。
            tmp_result, column_num = column_num_calc(airbag_data)
            if tmp_result:
                logger.debug('[%s:%s] 列数計算が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] 列数計算が失敗しました。' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] 座標取得を開始します。' % (app_id, app_name))
            # AirBagCoord.iniから各列の座標をリストに格納する。
            tmp_result, coord_list = get_coordinate(column_num, file_list[i])
            if tmp_result:
                logger.debug('[%s:%s] 座標取得が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] 座標取得が失敗しました。' % (app_id, app_name))
                sys.exit()

            # coord_listには[,],'が含まれるので置換する。
            re_coord_list = []
            for replace in range(len(coord_list)):
                re_coord_list.append(re.sub('[|]|\'', '', str(coord_list[replace])))

            # 行閾値算出
            logger.debug('[%s:%s] 行閾値算出を開始します。' % (app_id, app_name))
            tmp_result, x_threshold_list = get_x_threshold(coord_list, column_num)
            if tmp_result:
                logger.debug('[%s:%s] 行閾値算出が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] 行閾値算出が失敗しました。' % (app_id, app_name))
                sys.exit()

            # 列閾値算出
            logger.debug('[%s:%s] 列閾値算出を開始します。' % (app_id, app_name))
            tmp_result, y_threshold_list = get_y_coordinate(coord_list, column_num)
            if tmp_result:
                logger.debug('[%s:%s] 列閾値算出が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] 列閾値算出が失敗しました。' % (app_id, app_name))
                sys.exit()

            base_y = ',' + str(y_threshold_list)
            logger.debug('[%s:%s] 閾値情報Y座標 %s' % (app_id, app_name, base_y))
            y_threshold = re.sub('[][\'| ]', '', base_y)
            logger.debug('[%s:%s] 閾値情報Y座標 %s' % (app_id, app_name, y_threshold))
            master_data_list.append(y_threshold)

            base_x = ',' + str(x_threshold_list)
            x_threshold = re.sub('[][\'| ]', '', base_x)
            master_data_list.append(x_threshold)

            model_name = ',' + read_result_list[i][2]
            master_data_list.append(model_name)
            master_data.append(master_data_list)

        logger.debug('[%s:%s] 閾値情報 %s' % (app_id, app_name, master_data))

        logger.debug('[%s:%s] 閾値情報CSV作成を開始します。' % (app_id, app_name))
        tmp_result = create_master_data_file(output_file_path, master_data)
        if tmp_result:
            logger.debug('[%s:%s] 閾値情報CSV作成が終了しました。' % (app_id, app_name))
            logger.info('[%s:%s] %s処理は正常に終了しました。' % (app_id, app_name, app_name))
        else:
            logger.error('[%s:%s] 閾値情報CSV作成が失敗しました。' % (app_id, app_name))
            sys.exit()

        result = True

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。' % (app_id, app_name))
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)

    # 戻り値設定
    # 呼出側（バッチプログラム側）で戻り値判定（ERRORLEVEL）する際の戻り値を設定する。
    if result:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":
    ini_file_path = None
    csv_file_path = None
    output_file_path = None

    args = sys.argv

    if len(args) > 3:
        ini_file_path = args[1]
        csv_file_path = args[2]
        output_file_path = args[3]

    main(ini_file_path, csv_file_path, output_file_path)
