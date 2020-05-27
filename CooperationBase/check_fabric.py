# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能307 端判定
# ----------------------------------------

import os
import sys
import traceback
import cv2
import numpy as np
import csv
import logging.config
import codecs
import datetime
import configparser
import time
from multiprocessing import Pool
import multiprocessing as multi
import gc

import db_util
import error_detail
import error_util
import file_util
import masking_fabric
import marking_image

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_check_fabric.conf", disable_existing_loggers=False)
logger = logging.getLogger("check_fabric")

# 画像リサイズ設定ファイル読込
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_fabric_config.ini', 'SJIS')
# 共通設定ファイル読込
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：DB接続
#
# 処理概要           ：1.DBに接続する。
#
# 引数               ：なし
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def create_connection():
    # DBに接続する。
    conn, cur, res = db_util.create_connection(logger, app_id, app_name)
    return conn, cur, res


# ------------------------------------------------------------------------------------
# 処理名             ：検査情報取得
#
# 処理概要           ：1.反物情報テーブルから検査情報を取得する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      ステータス
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
#                      検査情報
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, status, unit_num):
    # クエリを作成する。
    sql = 'select product_name, fabric_name, inspection_num, imaging_starttime from processing_status ' \
          'where unit_num = \'%s\' and rapid_endtime IS NOT NULL and marking_endtime IS NULL ' \
          'and (status = %s or status = %s) order by imaging_starttime asc, rapid_endtime asc ' \
          % (unit_num, status, int(status) + 1)

    logger.debug('[%s:%s] 検査情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：NG情報取得
#
# 処理概要           ：1.RAPID解析情報テーブルからNG情報を取得する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      RAPID解析情報テーブル名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
#                      NG情報
# ------------------------------------------------------------------------------------
def select_ng_info(conn, cur, fabric_info, inspection_num, rapid_ng_status, inspection_date, imaging_starttime,
                   unit_num):
    # クエリを作成する。
    sql = 'select rp.num, rp.processing_id, rp.ng_image, rp.ng_point, rp.confidence, rp.rapid_host_name, ' \
          'fi.rapid_endtime from "rapid_%s_%s_%s" as rp inner join fabric_info as fi on ' \
          'rp.fabric_name = fi.fabric_name and rp.inspection_num = fi.inspection_num ' \
          'and rp.unit_num = fi.unit_num where rp.rapid_result = %s and fi.imaging_starttime = \'%s\' ' \
          'and fi.unit_num = \'%s\' and rp.edge_result is null and rp.masking_result is null ' \
          % (fabric_info, inspection_num, inspection_date, rapid_ng_status, imaging_starttime, unit_num)

    logger.debug('[%s:%s] NG情報取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, ng_list, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)

    return result, ng_list, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：NG情報CSV作成
#
# 処理概要           ：1.RAPID解析情報テーブルから取得したNG情報をCSVファイルに出力する。
#
# 引数               ：NG画像格納パス
#                      CSV出力パス
#                      品名
#                      反番
#                      検査番号
#                      取得NG情報
#                      ファイル連番
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      CSVファイルパス
# ------------------------------------------------------------------------------------
def create_csv(image_path, csv_path, product_name, fabric_name, inspection_num, ng_list, file_num, label_ng,
               label_others):
    csv_file_name = None
    result = False
    try:
        # 変数定義
        base_image_name = ng_list[0][2]
        date = base_image_name.split('_')[2]
        flag = inifile.get('CSV_INFO', 'flag')
        width = inifile.get('CSV_INFO', 'width')
        height = inifile.get('CSV_INFO', 'height')
        others_confidence = inifile.get('CSV_INFO', 'others_confidence')
        ng_csv_name = inifile.get('CSV_INFO', 'ng_csv_name')

        csv_file_name = csv_path + "\\" + date + "_" + product_name + "_" + fabric_name + "_" + str(inspection_num) \
                        + "_" + str(file_num) + "_" + ng_csv_name

        # CSVファイル出力先の作成
        tmp_result = file_util.make_directory(csv_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            return result, csv_file_name

        # 対象CSVファイルを開く
        with codecs.open(csv_file_name, "w", "SJIS") as ofp:

            # ヘッダーにNG画像格納パスを書き込む
            header = "[DataPath]\"" + image_path + "\""
            ofp.write(header)
            ofp.write("\r\n")
            writer = csv.writer(ofp, quotechar=None)

            # NG情報件数分、CSVの形に整形して書き込む
            for i in range(len(ng_list)):
                file_name = '"' + ng_list[i][2] + '"'
                point_xy = ng_list[i][3].split(',')
                point_x = point_xy[0]
                point_y = point_xy[1]
                confidence = ng_list[i][4]

                writer.writerow([flag, file_name, point_x, point_y, width, height, label_ng,
                                 confidence, label_others, others_confidence])

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, csv_file_name


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報テーブル更新
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
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column, time, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, time, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 反物情報テーブル更新SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス更新
#
# 処理概要           ：1.処理ステータステーブルのステータスを更新する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      処理ID
#                      RAPIDサーバーホスト名
#                      ステータス
#                      更新カラム名
#                      更新時刻
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def update_processing_status(conn, cur, fabric_name, inspection_num, processing_id, rapid_host_name, status,
                             column, time, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'update processing_status set status = %s, %s = \'%s\' where fabric_name = \'%s\' and inspection_num = %s ' \
          'and processing_id = %s and rapid_host_name = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column, time, fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime,
           unit_num)

    logger.debug('[%s:%s] 処理ステータス更新SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス更新(処理対象反番、検査番号の全レコード)
#
# 処理概要           ：1.処理ステータステーブルのステータスを更新する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      ステータス
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def update_processing_status_all(conn, cur, fabric_name, inspection_num, update_status, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'update processing_status set status = %s where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (update_status, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 全処理ステータス更新SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：端判定処理
#
# 処理概要           ：1.加工した画像データから、閾値を用いて反物の端かどうかを判定する。
#
# 引数               ：NG画像
#                      NG情報
#                      白画素データ
#
# 戻り値             ：端判定結果
# ------------------------------------------------------------------------------------
def filter_fabric(file, fabric_result, white):
    fname, fext = os.path.splitext(os.path.basename(file))

    try:
        crop_x = fabric_result["point"]["x"]
        crop_y = fabric_result["point"]["y"]
        window_width = fabric_result["width"]
        window_height = fabric_result["height"]

        # 画像読み込み
        result, img_org = file_util.read_image(file, logger, app_id, app_name)
        crop_img = img_org[crop_y:crop_y + window_height, crop_x:crop_x + window_width]

        # グレースケール化
        # ノイズ低減用のぼかし処理
        img_gray = cv2.cvtColor(crop_img, cv2.COLOR_BGR2GRAY)
        img_gray = cv2.blur(img_gray, (5, 5))

        # 閾値254で2値化
        ret, img_bin = cv2.threshold(img_gray, 254, white, cv2.THRESH_BINARY)

        # 白画素の割合で反物の端が判定
        img_bin[img_bin == white] = 1
        rate = np.sum(img_bin) / (100 * 100)
        rate_result = {}
        if rate >= 0.001:  # 0.001→白画素が100x100の中で、10px以上なら反物の端
            fabric_result["label"] = "_others"
            rate_result[fname + "," + str(crop_x) + "," + str(crop_y)] = "_others"
        else:
            rate_result[fname + "," + str(crop_x) + "," + str(crop_y)] = "NG"

        return rate_result
    except Exception as e:
        # エラーになった画像名を出力する。

        raise e


# ------------------------------------------------------------------------------------
# 処理名             ：端判定引数処理
#
# 処理概要           ：1.端判定処理へと引数を渡す
#
# 引数               ：引数
#
# 戻り値             ：端判定結果
# ------------------------------------------------------------------------------------
def wrapper_filter_fabric(args):
    result = filter_fabric(*args)
    return result


# ------------------------------------------------------------------------------------
# 処理名             ：端判定
#
# 処理概要           ：1.結果ファイルを読みこみ、端判定を行って、結果をCSVファイルとして出力する。
#
# 引数               ：NG情報CSVファイル
#                      端判定結果CSVファイル
#                      カテゴリー名(NG)
#                      カテゴリー名(others)
#                      処理情報
#                      白画素データ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      処理情報
# ------------------------------------------------------------------------------------
def check_fabric(ng_result_file, checkfabric_result_file, label_ng, label_others, process_list, white):
    result = False
    try:
        # 結果ファイル読み込み
        logger.debug('[%s:%s] 結果ファイル読込を開始します。 ' % (app_id, app_name))
        tmp_result, result_data = file_util.read_result_file(ng_result_file, logger, app_id, app_name)

        if tmp_result:
            logger.debug('[%s:%s] 結果ファイル読込が終了しました。 ' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 結果ファイル読込が失敗しました。 ' % (app_id, app_name))
            return result, process_list

        # NG画像パスの変数定義
        image_dir = result_data["datapath"]

        logger.debug('[%s:%s] 端判定処理準備を開始します。 ' % (app_id, app_name))
        # 並列処理用に引数を準備
        process_file_args = []
        for ng_result in result_data["data"]:
            if ng_result["label"] != label_ng:
                continue
            args = []
            args.append(image_dir + ng_result["filename"])
            args.append(ng_result)
            args.append(white)
            process_file_args.append(args)
        logger.debug('[%s:%s] 端判定処理準備が終了しました。 ' % (app_id, app_name))

        # 並列処理実行
        logger.debug('[%s:%s] 端判定処理の並列実行を開始します。 ' % (app_id, app_name))
        # p = Pool(multi.cpu_count())
        p = Pool(3)
        check_result = p.map(wrapper_filter_fabric, process_file_args)
        p.close()
        logger.debug('[%s:%s] 端判定処理の並列実行が終了しました。 ' % (app_id, app_name))

        # 並列処理結果取得
        logger.debug('[%s:%s] 端判定処理結果取得を開始します。 ' % (app_id, app_name))
        for data in check_result:
            if data is None:
                continue
            else:
                for key, value in data.items():
                    if value == label_others:
                        fname, x, y = key.split(",")
                        for ng_result in result_data["data"]:
                            if fname + ".jpg" == ng_result["filename"] and x == str(
                                    ng_result["point"]["x"]) and y == str(ng_result["point"]["y"]):
                                ng_result["label"] = label_others
                                break
                            else:
                                pass
                    else:
                        pass

        for ng_result in result_data["data"]:
            for i in range(len(process_list)):
                if ng_result["label"] == label_others and process_list[i][3] == ng_result["filename"] \
                        and str(process_list[i][4]) == str(ng_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(ng_result["point"]["y"]):
                    process_list[i].append(label_others)
                    continue

                elif ng_result["label"] == label_ng and process_list[i][3] == ng_result["filename"] \
                        and str(process_list[i][4]) == str(ng_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(ng_result["point"]["y"]):
                    process_list[i].append(label_ng)
                    continue
                else:
                    pass

        logger.debug('[%s:%s] 端判定結果取得が終了しました。 ' % (app_id, app_name))

        # 結果ファイルの出力
        logger.debug('[%s:%s] 端判定結果CSVファイルの出力を開始します。 ' % (app_id, app_name))
        tmp_result = file_util.write_result_file(checkfabric_result_file, result_data, image_dir, logger,
                                                 app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] 端判定結果CSVファイルの出力が終了しました。 ' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 端判定結果CSVファイルの出力が失敗しました。 ' % (app_id, app_name))
            return result, process_list

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_list


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID完了時刻取得
#
# 処理概要           ：1.反物情報テーブルからRAPID完了時刻を取得する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      反番
#                      検査番号
#
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
#                      RAPID完了時刻
# ------------------------------------------------------------------------------------
def select_rapid_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    # クエリを作成する。
    sql = 'select rapid_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] RAPID完了時刻取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル更新
#
# 処理概要           ：1.端判定、マスキング判定結果をRAPID解析情報テーブルに反映する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクトステータス
#                      反番
#                      検査番号
#                      処理ID
#                      端判定結果
#                      マスキング判定結果
#                      連番
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def update_rapid_anarysis(conn, cur, fabric_name, inspection_num, processing_id, edge_result, masking_result,
                          num, inspection_date, unit_num):
    ### クエリを作成する
    sql = 'update "rapid_%s_%s_%s" set edge_result = %s, masking_result = %s ' \
          'where num = %s and processing_id = %s and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, edge_result, masking_result, num, processing_id, unit_num)

    logger.debug('[%s:%s] RAPID解析情報テーブル更新SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：判定結果登録
#
# 処理概要           ：1.端判定とマスキング判定結果を処理情報と紐づけて、RAPID解析情報テーブルを更新する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      反番
#                      検査番号
#                      処理情報
#                      カテゴリー（NG）
#                      カテゴリー（_others）
#                      判定NG（2）
#                      判定OK（1）
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理情報紐づけ結果
# ------------------------------------------------------------------------------------
def update_check_result(conn, cur, fabric_name, inspection_num, process_list, label_ng,
                        judge_ng, judge_ok, inspection_date, unit_num):
    result = False
    process_id_list = []
    try:
        logger.debug('[%s:%s] 判定結果登録処理対象情報 %s' % (app_id, app_name, process_list))

        for i in range(len(process_list)):

            # 変数定義
            num = process_list[i][0]
            processing_id = process_list[i][1]
            rapid_host_name = process_list[i][2]
            file_name = process_list[i][3]

            # 端判定結果=NGの場合
            if process_list[i][6] == label_ng:
                logger.debug(
                    '[%s:%s] 端判定結果=NG [連番=%s] [処理ID=%s] [画像名=%s]' % (app_id, app_name, num, processing_id, file_name))
                edge_result = judge_ng
            # 端判定結果=OKの場合
            else:
                logger.debug(
                    '[%s:%s] 端判定結果=OK [連番=%s] [処理ID=%s] [画像名=%s]' % (app_id, app_name, num, processing_id, file_name))
                edge_result = judge_ok

            # マスキング判定結果=NGの場合
            if process_list[i][7] == label_ng:
                logger.debug('[%s:%s] マスキング判定結果=NG [連番=%s] [処理ID=%s] [画像名=%s]' % (
                    app_id, app_name, num, processing_id, file_name))
                masking_result = judge_ng
                process_id_list += [[processing_id, rapid_host_name]]

            # マスキング判定結果=OKの場合
            else:
                logger.debug('[%s:%s] マスキング判定結果=OK [連番=%s] [処理ID=%s] [画像名=%s]' % (
                    app_id, app_name, num, processing_id, file_name))
                masking_result = judge_ok

            logger.debug('[%s:%s] RAPID解析情報テーブルのNG情報更新を開始します。' % (app_id, app_name))
            tmp_result, conn, cur = update_rapid_anarysis(conn, cur, fabric_name, inspection_num, processing_id,
                                                          edge_result, masking_result, num, inspection_date, unit_num)

            if tmp_result:
                logger.debug('[%s:%s] RAPID解析情報テーブルのNG情報更新が終了しました。' % (app_id, app_name))
                conn.commit()
            else:
                logger.error('[%s:%s] RAPID解析情報テーブルのNG情報更新が失敗しました。' % (app_id, app_name))
                conn.rollback()
                return result, process_id_list, conn, cur

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_id_list, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：マスキング判定件数取得
#
# 処理概要           ：1.RAPID解析情報テーブルからマスキング判定件数を取得する。
#
# 引数               ：カーソルオブジェクト
#                      コネクションオブジェクト
#                      反番
#                      検査番号
#                      RAPID解析情報テーブル名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
#                      スキング判定件数
# ------------------------------------------------------------------------------------
def select_masking_result(conn, cur, fabric_name, inspection_num, inspection_date, unit_num):
    # クエリを作成する。
    sql = 'select count(num) from "rapid_%s_%s_%s" ' \
          'where fabric_name = \'%s\'and inspection_num = %s and unit_num = \'%s\' and masking_result is null ' \
          % (fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, unit_num)

    logger.debug('[%s:%s] マスキング判定件数取得SQL %s' % (app_id, app_name, sql))
    # 反物情報テーブルから総撮像枚数、処理済枚数、撮像完了時刻を取得する。
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


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
# 関数名             ：基底判定処理
#
# 処理概要           ：1.端判定、マスキング判定を呼び出す
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      NG情報
#                      品名
#                      反番
#                      検査番号
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def base_check_fabric(conn, cur, ng_list, product_name, fabric_name, inspection_num, file_num,
                      imaging_starttime, unit_num):
    result = False
    try:
        ng_csv_name = inifile.get('CSV_INFO', 'ng_csv_name')
        fabric_csv_name = inifile.get('CSV_INFO', 'fabric_csv_name')
        masking_csv_name = inifile.get('CSV_INFO', 'masking_csv_name')
        root_path = common_inifile.get('FILE_PATH', 'rk_path')
        ng_path = inifile.get('FILE_PATH', 'ng_path')

        processing_column_start = inifile.get('COLUMN', 'processing_column_start')
        processing_status_start = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_start')
        fabric_status_start = common_inifile.get('FABRIC_STATUS', 'imageprocessing_start')
        fabric_column_start = inifile.get('COLUMN', 'fabric_column_start')
        processing_column_end = inifile.get('COLUMN', 'processing_column_end')
        processing_status_end = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_end')

        white = int(inifile.get('VALUE', 'white'))
        label_ng = inifile.get('CSV_INFO', 'label_ng')
        label_others = inifile.get('CSV_INFO', 'label_others')
        judge_ok = int(common_inifile.get('ANALYSIS_STATUS', 'ok'))
        judge_ng = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))

        process_list = []
        processing_status_list = []

        logger.debug('[%s:%s] NG情報が%s件以上存在します。' % (app_id, app_name, len(ng_list)))
        logger.debug('[%s:%s] NG情報CSVファイル作成を開始します。' % (app_id, app_name))

        # 変数定義

        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))
        csv_path = root_path + "\\CSV\\" + inspection_date + "_" + product_name + "_" + fabric_name + "_" + \
                   str(inspection_num)
        image_path = root_path + "\\" + ng_path + "\\" + inspection_date + "_" + product_name + "_" \
                     + fabric_name + "_" + str(inspection_num) + "\\"

        # 取得したNG情報からCSVファイルを作成する
        tmp_result, ng_result_file = create_csv(image_path, csv_path, product_name, fabric_name,
                                                inspection_num, ng_list, file_num, label_ng, label_others)
        if tmp_result:
            logger.debug('[%s:%s] NG情報CSVファイル作成が終了しました。' % (app_id, app_name))

        else:
            logger.error('[%s:%s] NG情報CSVファイル作成に失敗しました。' % (app_id, app_name))
            return result, conn, cur

        # 取得したNG情報件数分、処理ステータスを更新する
        for i in range(len(ng_list)):
            num = ng_list[i][0]
            processing_id = ng_list[i][1]
            image_name = ng_list[i][2]
            rapid_host_name = ng_list[i][5]
            point_xy = ng_list[i][3].split(',')
            point_x = int(point_xy[0])
            point_y = int(point_xy[1])
            process_list += [[num, processing_id, rapid_host_name, image_name, point_x, point_y]]
            check_list = [x for x in processing_status_list if processing_id == x[:][0] and rapid_host_name in x[:][1]]
            if i == 1:
                processing_status_list += [[processing_id, rapid_host_name]]
            elif len(check_list) == 0:
                processing_status_list += [[processing_id, rapid_host_name]]
            else:
                pass

        for i in range(len(processing_status_list)):
            start_time = datetime.datetime.now()
            processing_id = processing_status_list[i][0]
            rapid_host_name = processing_status_list[i][1]
            logger.debug('[%s:%s] 処理ステータス更新を開始します。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                         (app_id, app_name, processing_id, rapid_host_name))

            tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num,
                                                             processing_id, rapid_host_name, processing_status_start,
                                                             processing_column_start, start_time, imaging_starttime,
                                                             unit_num)
            if tmp_result:
                logger.debug('[%s:%s] 処理ステータス更新が終了しました。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
            else:
                logger.error('[%s:%s] 処理ステータス更新が失敗しました。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
                conn.rollback()
                return result, conn, cur

        conn.commit()

        checkfabric_result_file = csv_path + "\\" + inspection_date + "_" + product_name + "_" + fabric_name + \
                                  "_" + str(inspection_num) + "_" + str(file_num) + "_" + fabric_csv_name
        logger.debug('[%s:%s] 端判定を開始します。' % (app_id, app_name))

        # 端判定処理を行う
        tmp_result, process_list = check_fabric(ng_result_file, checkfabric_result_file, label_ng,
                                                label_others, process_list, white)

        if tmp_result:
            logger.debug('[%s:%s] 端判定が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] 端判定が失敗しました。' % (app_id, app_name))
            return result, conn, cur

        masking_result_file = csv_path + "\\" + inspection_date + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + str(file_num) + "_" + masking_csv_name

        logger.debug('[%s:%s] マスキング判定を開始します。' % (app_id, app_name))

        # マスキング判定を呼び出す
        tmp_result, process_list = masking_fabric.main(checkfabric_result_file, masking_result_file,
                                                       label_ng, label_others, process_list, inspection_date)
        if tmp_result:
            logger.debug('[%s:%s] マスキング判定が終了しました。 %s' % (app_id, app_name, process_list))
        else:
            logger.error('[%s:%s] マスキング判定が失敗しました。' % (app_id, app_name))
            return result, conn, cur
        end_time = datetime.datetime.now()

        logger.debug('[%s:%s] 端判定結果及びマスキング判定の結果登録を開始します。' % (app_id, app_name))

        tmp_result, process_id_list, conn, cur = update_check_result(conn, cur, fabric_name, inspection_num,
                                                                     process_list, label_ng, judge_ng, judge_ok,
                                                                     inspection_date, unit_num)

        if tmp_result:
            logger.debug('[%s:%s] 端判定結果及びマスキング判定の結果登録が終了しました。' % (app_id, app_name))
            conn.commit()
        else:
            logger.error('[%s:%s] 端判定結果及びマスキング判定の結果登録が失敗しました。' % (app_id, app_name))
            return result, conn, cur

        for i in range(len(processing_status_list)):
            processing_id = processing_status_list[i][0]
            rapid_host_name = processing_status_list[i][1]
            logger.debug('[%s:%s] 処理ステータスの更新を開始します。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                         (app_id, app_name, processing_id, rapid_host_name))
            tmp_result = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                  rapid_host_name, processing_status_end, processing_column_end,
                                                  end_time, imaging_starttime, unit_num)
            if tmp_result:
                logger.debug('[%s:%s]  処理ステータスの更新が終了しました。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
            else:
                logger.error('[%s:%s]  処理ステータスの更新が失敗しました。 [処理ID=%s] [RAPIDサーバーホスト名=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
                return conn, cur, result, process_list

        conn.commit()

        process_id_list.sort(key=lambda x: x[0])
        logger.debug('[%s:%s]  画像マーキング処理を開始します。' % (app_id, app_name))
        tmp_result, conn, cur = marking_image.main(conn, cur, product_name, fabric_name, inspection_num,
                                                   masking_result_file, process_id_list, imaging_starttime,
                                                   inspection_date, unit_num)

        if tmp_result:
            logger.debug('[%s:%s]  画像マーキング処理が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s]  画像マーキング処理が失敗しました。' % (app_id, app_name))
            result = False
            return result, conn, cur

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：メイン処理
#
# 処理概要           ：1.反物情報テーブルから検査情報を取得し、NG件数閾値に達した場合
#                       NG情報からCSVファイルを作成し、端判定を行う。
#                      2.マスキング判定を呼び出す。
#                      3.一検査番号の処理が完了したかどうかの判定を行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    result = False
    try:

        error_file_name = inifile.get('ERROR_FILE', 'file')
        select_status = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
        fabric_column_end = inifile.get('COLUMN', 'fabric_column_end')
        fabric_status_end = common_inifile.get('FABRIC_STATUS', 'imageprocessing_end')
        ng_num = int(inifile.get('VALUE', 'ng_num'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        rapid_ng_status = common_inifile.get('ANALYSIS_STATUS', 'ng')
        processing_status_end = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_end')
        file_num = 1
        fabric_status_start = common_inifile.get('FABRIC_STATUS', 'imageprocessing_start')
        fabric_column_start = inifile.get('COLUMN', 'fabric_column_start')

        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s機能が起動しました。' % (app_id, app_name, app_name))

        while True:
            # DB接続を行う
            logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
            result, conn, cur = create_connection()
            if result:
                logger.debug('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
            else:
                logger.error('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
                sys.exit()

            while True:
                # 検査情報を取得する
                logger.debug('[%s:%s] 検査情報取得を開始します。' % (app_id, app_name))
                result, fabric_info, conn, cur = select_fabric_info(conn, cur, select_status, unit_num)
                if result:
                    logger.debug('[%s:%s] 検査情報取得が終了しました。検査情報 %s ' % (app_id, app_name, fabric_info))
                    conn.commit()
                else:
                    logger.debug('[%s:%s] 検査情報取得が失敗しました。' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                if fabric_info is not None:

                    logger.debug('[%s:%s] 検査情報が存在します。' % (app_id, app_name))

                    # 変数定義
                    product_name, fabric_name, inspection_num, imaging_starttime = \
                        fabric_info[0], fabric_info[1], str(fabric_info[2]), fabric_info[3]
                    logger.debug('[%s:%s] 反物情報テーブル更新を開始します。' % (app_id, app_name))

                    # 開始時刻を取得する
                    start_time = datetime.datetime.now()

                    # 検査日時
                    inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                    # 反物情報テーブルを更新する
                    tmp_result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                               fabric_status_start, fabric_column_start, start_time,
                                                               imaging_starttime, unit_num)

                    if tmp_result:
                        logger.debug('[%s:%s] 反物情報テーブル更新が終了しました。' % (app_id, app_name))
                        conn.commit()
                    else:
                        logger.error('[%s:%s]  反物情報テーブル更新に失敗しました。' % (app_id, app_name))
                        conn.rollback()
                        return result, conn, cur

                    while True:
                        logger.debug('[%s:%s] NG情報取得を開始します。' % (app_id, app_name))
                        # NG情報を取得する
                        result, ng_list, conn, cur = select_ng_info(conn, cur, fabric_name, inspection_num,
                                                                    rapid_ng_status, inspection_date, imaging_starttime,
                                                                    unit_num)

                        if result:
                            logger.debug('[%s:%s] NG情報取得が終了しました。' % (app_id, app_name))

                        else:
                            logger.error('[%s:%s] NG情報取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_list) == 0:
                            logger.debug('[%s:%s] NG情報が存在しません。' % (app_id, app_name))

                        else:
                            rapid_endtime = ng_list[0][6]

                            if len(ng_list) >= ng_num:
                                logger.info('[%s:%s] 端判定・マスキング判定・マーキング処理を開始します。 '
                                            '[反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                            (app_id, app_name, fabric_name, inspection_num, inspection_date))
                                result, conn, cur = base_check_fabric(conn, cur, ng_list, product_name, fabric_name,
                                                                      inspection_num, file_num, imaging_starttime,
                                                                      unit_num)
                                if result:
                                    logger.debug('[%s:%s] 端判定・マスキング判定・マーキング処理が終了しました。' % (app_id, app_name))
                                else:
                                    logger.error('[%s:%s] 端判定・マスキング判定・マーキング処理が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                            else:
                                if rapid_endtime is not None:
                                    logger.debug('[%s:%s] 端判定・マスキング判定・マーキング処理を開始します。' % (app_id, app_name))
                                    result, conn, cur = base_check_fabric(conn, cur, ng_list, product_name, fabric_name,
                                                                          inspection_num, file_num, imaging_starttime,
                                                                          unit_num)
                                    if result:
                                        logger.debug('[%s:%s] 端判定・マスキング判定・マーキング処理が終了しました。' % (app_id, app_name))
                                    else:
                                        logger.error('[%s:%s] 端判定・マスキング判定・マーキング処理が失敗しました。' % (app_id, app_name))
                                        sys.exit()
                                else:
                                    logger.debug('[%s:%s] NG情報が%s件に達しません。' % (app_id, app_name, ng_num))
                                    time.sleep(sleep_time)
                                    continue

                        logger.debug('[%s:%s] 処理完了判定を開始します。' % (app_id, app_name))
                        logger.debug('[%s:%s] RAPID解析完了時刻の取得を開始します。' % (app_id, app_name))
                        # RAPID解析完了時刻を取得する
                        result, fabric_info, conn, cur = select_rapid_endtime(conn, cur, fabric_name, inspection_num,
                                                                              imaging_starttime, unit_num)
                        if result:
                            logger.debug('[%s:%s] RAPID解析完了時刻の取得が終了しました。' % (app_id, app_name))
                            logger.debug('[%s:%s] RAPID解析完了時刻: %s' % (app_id, app_name, fabric_info))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] RAPID解析完了時刻の取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] 端判定・マスキング判定結果の取得を開始します。' % (app_id, app_name))
                        # 端判定・マスキング判定結果を取得する
                        result, rapid_info, conn, cur = select_masking_result(conn, cur, fabric_name, inspection_num,
                                                                              inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] 端判定・マスキング判定結果の取得が終了しました。' % (app_id, app_name))
                            logger.debug('[%s:%s] 端判定・マスキング判定結果件数: %s' % (app_id, app_name, rapid_info))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] 端判定・マスキング判定結果の取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        # 変数定義
                        rapid_endtime = fabric_info[0]
                        count_masking_result = rapid_info[0]

                        if rapid_endtime is not None and count_masking_result == 0:
                            logger.info('[%s:%s] 検査番号の画像全ての判定が終了しました。 '
                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            # 処理ステータステーブルを更新する
                            logger.debug('[%s:%s] 処理ステータステーブルの更新を開始します。' % (app_id, app_name))
                            result, conn, cur = update_processing_status_all(conn, cur, fabric_name, inspection_num,
                                                                             processing_status_end, imaging_starttime,
                                                                             unit_num)

                            if result:
                                logger.debug('[%s:%s] 処理ステータステーブルの更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] 処理ステータステーブルの更新が失敗しました。' % (app_id, app_name))
                                sys.exit()

                            # 終了時刻を取得する
                            end_time = datetime.datetime.now()

                            # 反物情報テーブルを更新する
                            logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_status_end, fabric_column_end, end_time,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] 反物情報テーブルの更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] 反物情報テーブルの更新が失敗しました。' % (app_id, app_name))
                                sys.exit()

                            logger.info('[%s:%s] 端判定、マスキング判定、マーキング処理は正常に終了しました。 '
                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            file_num = 1
                            break

                        else:
                            logger.info('[%s:%s] 判定対象の画像が残っています。 '
                                        '[反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            file_num += 1
                            time.sleep(sleep_time)
                            break

                else:
                    logger.info('[%s:%s] 検査情報が存在しません。' % (app_id, app_name))
                    logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                    result = close_connection(conn, cur)

                    if result:
                        logger.debug('[%s:%s] DB接続の切断が完了しました。' % (app_id, app_name))
                    else:
                        logger.error('[%s:%s] DB接続の切断に失敗しました。' % (app_id, app_name))
                        sys.exit()

                    logger.debug('[%s:%s] %s秒スリープします' % (app_id, app_name, sleep_time))
                    time.sleep(sleep_time)
                    break


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

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))
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


if __name__ == "__main__":
    import multiprocessing

    multiprocessing.freeze_support()
    main()
