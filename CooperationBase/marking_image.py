# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能308  画像マーキング
# ----------------------------------------

import traceback
import configparser
from PIL import Image, ImageDraw, ImageFile
import datetime
import os
import logging.config

import db_util
import error_detail
import file_util

#  共通設定ファイル読込み
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
#  画像マーキング設定ファイル読込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/marking_image_config.ini', 'SJIS')

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_marking_image.conf")
logger = logging.getLogger("marking_image")

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス更新
#
# 処理概要           ：1.画像マーキング処理開始・完了時に処理ステータスを更新する
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
def update_processing_status(conn, cur, fabric_name, inspection_num, processing_id, column_name, time,
                             rapid_host_name, status, imaging_starttime, unit_num):
    # クエリを作成する
    sql = 'UPDATE processing_status SET status=%s, %s =\'%s\' ' \
          'WHERE fabric_name=\'%s\' AND inspection_num = %s AND processing_id = %s AND rapid_host_name = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column_name, time, fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime,
           unit_num)

    logger.debug('[%s:%s] 処理ステータス更新SQL %s' % (app_id, app_name, sql))
    # 処理ステータス（リサイズ完了）を更新する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：マーキング処理
#
# 処理概要           ：1.NG画像のNG箇所にマーキングを行う。
#
# 引数               ：NG・マーキング画像パス
#                      結果ファイル読込データ
#                      マーキングカラー
#                      マーキング幅
#                      マーキング画像名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def marking_process(output_path, masking_result, marking_color, line_width):
    result = False
    image = None
    try:

        # 画像を開く
        image = Image.open(output_path)

        # 線描情報を定義する
        upper_left = {"x": masking_result["point"]["x"], "y": masking_result["point"]["y"]}
        upper_right = {"x": masking_result["point"]["x"] + masking_result["width"], "y": masking_result["point"]["y"]}
        lower_right = {"x": masking_result["point"]["x"] + masking_result["width"], "y": masking_result["point"]["y"]
                                                                                         + masking_result["height"]}
        lower_left = {"x": masking_result["point"]["x"], "y": masking_result["point"]["y"] + masking_result["height"]}

        # 線描する
        draw = ImageDraw.Draw(image)
        draw.line((upper_left["x"], upper_left["y"], upper_right["x"], upper_right["y"]),
                  fill=marking_color, width=line_width)
        draw.line((upper_right["x"], upper_right["y"], lower_right["x"], lower_right["y"]),
                  fill=marking_color, width=line_width)
        draw.line((lower_right["x"], lower_right["y"], lower_left["x"], lower_left["y"]),
                  fill=marking_color, width=line_width)
        draw.line((lower_left["x"], lower_left["y"], upper_left["x"], upper_left["y"]),
                  fill=marking_color, width=line_width)

        # 線描した画像を保存する
        image.save(output_path, quality=100)

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, image


# ------------------------------------------------------------------------------------
# 処理名             ：NG情報取得
#
# 処理概要           ：1.マスキング判定結果ファイルを読込んでデータを返す。
#
# 引数               ：マスキング判定結果ファイルパス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      読込データ
# ------------------------------------------------------------------------------------
def read_result_file(result_file):
    # マスキング判定結果CSVファイルを読込む
    result, result_data, error = file_util.read_result_file(result_file, logger, app_id, app_name)

    return result, result_data


# ------------------------------------------------------------------------------------
# 処理名             ：NG画像移動
#
# 処理概要           ：1.マスキング判定結果NGの画像を別フォルダに移動する。
#
# 引数               ：NG画像パス
#                      NG画像名ファイルパス
#                      マーキング画像名ファイルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def move_image(image_path, output_path, save_path):
    result = False
    file_name = os.path.basename(image_path)
    logger.info('[%s:%s] %s %s' % (app_id, app_name, file_name, output_path))
    # 移動先に既にファイルが存在するか確認する。
    if os.path.exists(output_path + "\\" + file_name):
        result = True
        return result

    # ファイルが存在しない場合、ファイルを移動させる。
    else:
        mk_result, error = file_util.make_directory(output_path, logger, app_id, app_name)
        if mk_result:
            mv_result, error = file_util.move_file(image_path, output_path, logger, app_id, app_name)
            if mv_result:
                # ファイル移動後に、別名（marking_*.jpg）でコピーする。
                cp_result, error = file_util.copy_file(output_path + "\\" + file_name, save_path, logger, app_id, app_name)
                if cp_result:
                    result = True
                    return result
                else:
                    return result
            else:
                return result
        else:
            return result


# ------------------------------------------------------------------------------------
# 処理名             ：マーキング画像名更新
#
# 処理概要           ：1.RAPID解析情報テーブルにマーキング画像名を登録する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      処理ID
#                      マーキング画像名
#                      RAPIDサーバーホスト名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_marking_image_name(conn, cur, fabric_name, inspection_num, ng_image, marking_image,
                              judge_ng, inspection_date, unit_num):
    # クエリを作成する
    sql = 'UPDATE "rapid_%s_%s_%s" SET marking_image = \'%s\' ' \
          'WHERE ng_image = \'%s\' and masking_result = %s and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, marking_image, ng_image, judge_ng, unit_num)

    logger.debug('[%s:%s] マーキング画像名更新SQL %s' % (app_id, app_name, sql))
    # 処理ステータス（リサイズ完了）を更新する。
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.マスキング判定結果ファイルを読込み、NGの画像に対してマーキングを行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      品名
#                      反番
#                      検査情報
#                      処理情報（処理ID、RAPIDサーバーホスト名）
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def main(conn, cur, product_name, fabric_name, inspection_num, result_file, process_id_list,
         imaging_starttime, inspection_date, unit_num):
    result = False
    try:

        # 設定ファイルから設定値を取得する。
        root_path = common_inifile.get('FILE_PATH', 'rk_path')
        ng_path = inifile.get('MARKING_INFO', 'ng_path')
        label_others = inifile.get('CSV_INFO', 'label_others')
        marking_colors = inifile.get('MARKING_INFO', 'color').split(',')
        marking_color = int(marking_colors[0]), int(marking_colors[1]), int(marking_colors[2])
        line_width = int(inifile.get('MARKING_INFO', 'line_width'))
        marking_name = inifile.get('MARKING_INFO', 'marking_name')
        marking_start_column = inifile.get('COLUMN', 'marking_start')
        marking_end_column = inifile.get('COLUMN', 'marking_end')
        marking_start = common_inifile.get('PROCESSING_STATUS', 'marking_start')
        marking_end = common_inifile.get('PROCESSING_STATUS', 'marking_end')
        judge_ng = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))

        # 処理開始時刻を取得する。
        start_time = datetime.datetime.now()

        # サイズの大きな画像をスキップしない
        ImageFile.LOAD_TRUNCATED_IMAGES = True

        logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % 
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        # 処理情報（処理ID、RAPIDサーバーホスト名）の数だけ、処理ステータスを更新する。
        processing_id = 0
        for i in range(len(process_id_list)):
            if len(process_id_list) == 1 or process_id_list[i] != process_id_list[i - 1]:
                processing_id = process_id_list[i][0]
                rapid_host_name = process_id_list[i][1]

                logger.debug('[%s:%s] 処理ステータスの更新を開始します。 処理ID=[%s] RAPIDサーバーホスト名=[%s]'
                             % (app_id, app_name, processing_id, rapid_host_name))

                # 処理ステータス（マーキング開始）を更新する。
                tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                                 marking_start_column, start_time, rapid_host_name,
                                                                 marking_start, imaging_starttime, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] 処理ステータスの更新が終了しました。 処理ID=[%s] RAPIDサーバーホスト名=[%s] '
                                 % (app_id, app_name, processing_id, rapid_host_name,))
                else:
                    logger.error('[%s:%s] 処理ステータスの更新が失敗しました。 処理ID=[%s] RAPIDサーバーホスト名=[%s] '
                                 % (app_id, app_name, processing_id, rapid_host_name))
                    conn.rollback()
                    return result, conn, cur
            else:
                pass

        conn.commit()

        logger.debug('[%s:%s] マスキング判定結果CSVの読込を開始します。' % (app_id, app_name))
        # 結果ファイル読み込み
        tmp_result, result_data = read_result_file(result_file)

        if tmp_result:
            logger.debug('[%s:%s] マスキング判定結果CSVの読込が終了しました。' % (app_id, app_name))

        else:
            logger.error('[%s:%s] マスキング判定結果CSVの読込が失敗しました。' % (app_id, app_name))
            return result, conn, cur

        # 変数定義
        image_dir = result_data["datapath"]
        date = str(imaging_starttime.strftime('%Y%m%d'))

        # 読取結果ごとに処理を行う。
        for masking_result in result_data["data"]:

            ng_image = masking_result["filename"]
            marking_image = marking_name + masking_result["filename"]

            # 変数定義
            image_path = image_dir + ng_image
            output_path = root_path + '\\' + ng_path + "\\" + date + "_" + product_name + "_" + fabric_name + "_" + \
                          str(inspection_num) + "\\"
            move_path = output_path + ng_image
            save_path = output_path + marking_image

            # カテゴリが「_others」の場合、マーキング不要のため次の読取結果を処理する。
            if masking_result["label"] == label_others:
                logger.debug('[%s:%s] NG画像ではないため、 処理はありません。 ファイル名=[%s]' % (app_id, app_name,
                                                                           masking_result["filename"]))
                continue

            # カテゴリが「NG」の場合、マーキング処理を行う。
            else:
                logger.debug('[%s:%s] NG画像移動を開始します。 移動元=[%s] 移動先=[%s]' % (app_id, app_name, image_path, move_path))

                # NG画像をNG・マーキング用フォルダに移動する。
                tmp_result = move_image(image_path, output_path, save_path)

                if tmp_result:
                    logger.debug('[%s:%s] NG画像移動が終了しました。 ' % (app_id, app_name))
                else:
                    logger.error('[%s:%s] NG画像移動が失敗しました。 ' % (app_id, app_name))
                    return result, conn, cur

                logger.debug('[%s:%s] マーキング処理を開始します。 ファイル名=[%s]' % (app_id, app_name,
                                                                    masking_result["filename"]))

                # 画像のNG箇所にマーキングして保存する。
                tmp_result, image = marking_process(save_path, masking_result, marking_color, line_width)

                if tmp_result:
                    logger.debug('[%s:%s] マーキング処理が終了しました。 ファイル名=[%s]' % (app_id, app_name,
                                                                         masking_result["filename"]))

                else:
                    logger.error('[%s:%s] マーキング処理が失敗しました。 ファイル名=[%s]' % (app_id, app_name,
                                                                         masking_result["filename"]))
                    return result, conn, cur

                # マーキング画像名を登録する。
                logger.debug('[%s:%s] DBへのマーキング画像名登録を開始します。 ファイル名=[%s]' % (app_id, app_name,
                                                                           masking_result["filename"]))
                tmp_result, conn, cur = update_marking_image_name(conn, cur, fabric_name, inspection_num, ng_image,
                                                                  marking_image, judge_ng, inspection_date, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] DBへのマーキング画像名登録が終了しました。 ファイル名=[%s]' % (app_id, app_name,
                                                                                masking_result["filename"]))
                    conn.commit()

                else:
                    logger.error('[%s:%s] DBへのマーキング画像名登録が失敗しました。 ファイル名=[%s]' % (app_id, app_name,
                                                                                masking_result["filename"]))
                    return result, conn, cur

        # 処理終了時刻を取得する。
        end_time = datetime.datetime.now()

        # 処理情報（処理ID、RAPIDサーバーホスト名）の数分、処理ステータスを更新する。
        for i in range(len(process_id_list)):
            if len(process_id_list) == 1 or process_id_list[i] != process_id_list[i - 1]:
                processing_id = process_id_list[i][0]
                rapid_host_name = process_id_list[i][1]

                logger.debug('[%s:%s] 処理ステータスの更新を開始します。 処理ID=[%s] RAPIDサーバーホスト名=[%s]'
                             % (app_id, app_name, processing_id, rapid_host_name))

                # 処理ステータス（マーキング完了）を更新する。
                tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                                 marking_end_column, end_time, rapid_host_name,
                                                                 marking_end, imaging_starttime, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] 処理ステータスの更新が終了しました。 処理ID=[%s] RAPIDサーバーホスト名=[%s]'
                                 % (app_id, app_name, processing_id, rapid_host_name))
                else:
                    logger.error('[%s:%s] 処理ステータスの更新が失敗しました。 処理ID=[%s] RAPIDサーバーホスト名=[%s]'
                                 % (app_id, app_name, processing_id, rapid_host_name))
                    conn.rollback()
                    return result, conn, cur

            else:
                pass

        conn.commit()

        logger.info('[%s:%s] %s処理は正常に終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % 
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        result = True


    except Exception as e:
        # 想定外エラー発生
        logger.error('[%s:%s] 予期しないエラーが発生しました。' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, conn, cur
