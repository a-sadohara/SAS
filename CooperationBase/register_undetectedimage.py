# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能311  未検知画像登録
# ----------------------------------------

import configparser
from decimal import Decimal, ROUND_HALF_UP
import logging.config
import os
import re
import sys
import time
import traceback
import datetime
import shutil

import error_detail
import error_util
import db_util
import file_util

import register_ng_info_undetect
import compress_image_undetect

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_undetectedimage")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_undetectedimage_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 関数名             ：ファイル取得
#
# 処理概要           ：1.未検知画像通知ファイルのファイルリストを取得する。
#
# 引数               ：未検知画像通知ファイル格納フォルダパス
#                      未検知画像通知ファイル名
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      未検知画像通知ファイルリスト
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    result = False
    sorted_files = None

    try:

        # 共通関数で未検知画像通知ファイル格納フォルダ情報を取得する
        file_list = None
        tmp_result, file_list, error = file_util.get_file_list(file_path + '\\', file_name, logger, app_id, app_name)

        if tmp_result:
            # 成功時
            pass
        else:
            # 失敗時
            logger.error("[%s:%s] 未検知画像通知ファイル格納フォルダにアクセス出来ません。", app_id, app_name)
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
# 処理名             ：作業者情報取得
#
# 処理概要           ：1.検査情報ヘッダテーブルから作業者情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                     カーソルオブジェクト
#                     反番
#                     検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    検査情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, fabric_name, inspection_num, timestamp, unit_num):
    ### クエリを作成する
    sql = 'select worker_1, worker_2, start_datetime from inspection_info_header ' \
          'where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\' ' \
          'and insert_datetime <= \'%s\' order by insert_datetime desc'\
          % (fabric_name, inspection_num, unit_num, timestamp)

    # DB共通処理を呼び出して、処理ステータステーブルからデータを取得する。
    result, records, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：ファイル読込
#
# 処理概要           ：1.未検知画像通知ファイルを読込む
#
# 引数               ：未検知画像通知ファイルのファイルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      品名
#                      反番
#                      検査番号
#                      面
#                      カメラ番号_検反部No1
#                      カメラ番号_検反部No2
#                      NG画像名
#                      撮像開始時刻
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    product_name = None
    fabric_name = None
    inspection_num = None
    face_num = None
    camera_num_1 = None
    camera_num_2 = None
    ng_image_file_name = None
    file_datetime = None

    try:
        # 画像ファイルの拡張子
        extension = common_inifile.get('FILE_PATTERN', 'image_file')
        extension = extension.replace('*', '')

        logger.debug('[%s:%s] 撮像完了通知ファイル=%s', app_id, app_name, file)
        # 未検知画像通知を読込む。
        # 未検知画像通知のファイル名を取得する。
        # ファイル名は、「YYYYMMDD_[品名]_[反番]_[検査番号]_[カメラ番号]_[FACE情報]_[撮像番号].txt」を想定する。
        basename = os.path.basename(file)
        file_name = re.split('[_.]', basename)

        product_name = file_name[0]
        fabric_name = file_name[1]
        inspection_num = file_name[3]
        face_num = file_name[4]
        file_datetime = file_name[2]
        camera_num = file_name[5]

        # FACE情報から検版部を判断する。
        if face_num == '1':
            camera_num_1 = camera_num
        else:
            camera_num_2 = camera_num

        # 未検知画像通知のファイル名から、未検知画像ファイル名を作成する。
        # ファイル名は、「[品名]_[反番]_[日付]_[検査番号]_ [検反部No]_[カメラ番号]_[連番].jpg」を想定する。
        ng_image_file_name = product_name + '_' + fabric_name + '_' + file_datetime + '_' + inspection_num + '_' + face_num + '_' + camera_num + '_' + file_name[6] + extension

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, product_name, fabric_name, inspection_num, face_num, camera_num_1, camera_num_2, \
           ng_image_file_name, file_datetime


# ------------------------------------------------------------------------------------
# 処理名             ：退避フォルダ存在チェック
#
# 処理概要           ：1.未検知画像通知ファイルを退避するフォルダが存在するかチェックする。
#                    2.フォルダが存在しない場合は作成する。
#
# 引数               ：退避フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    logger.debug('[%s:%s] 未検知画像通知ファイルを退避するフォルダを作成します。フォルダ名：[%s]',
                 app_id, app_name, target_path)
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：未検知画像通知ファイル退避
#
# 処理概要           ：1.未検知画像通知ファイルを、退避フォルダに移動させる。
#
# 引数               ：未検知画像通知ファイル
#                      退避フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    target_file_name = os.path.basename(target_file)
    # ファイル移動
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)
    return result


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル登録
#
# 処理概要           ：1.RAPID解析情報テーブルにNG結果レコードを登録する。
#
# 引数               ：品名
#                     版番
#                     検査番号
#                     面
#                      NG画像名
#                      NG座標
#                      RAPID解析結果
#                      端判定結果
#                      マスキング判定結果
#                      カーソルオブジェクト
#                      コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2,
                               inspection_num, ng_face, ng_image, ng_point, worker_1, worker_2, cur, conn,
                               inspection_date, unit_num):
    # DB登録値:未検知画像
    result_status = common_inifile.get('ANALYSIS_STATUS', 'undetec')

    # カメラ番号 検反部について、値が無い場合にはnull文字に置き換える
    if camera_num_1 is None:
        camera_num_1 = 'null'
    if camera_num_2 is None:
        camera_num_2 = 'null'

    inspection_num = str(int(inspection_num))

    ### クエリを作成する
    sql = 'insert into "rapid_%s_%s_%s" (' \
          'product_name, fabric_name, camera_num_1, camera_num_2, ' \
          'inspection_num, ng_face, ng_image, marking_image, ng_point, ' \
          'rapid_result, edge_result, masking_result, worker_1, worker_2, unit_num ' \
          ') values (' \
          '\'%s\', \'%s\', %s, %s, ' \
          '%s, \'%s\', \'%s\', \'%s\', \'%s\', ' \
          '\'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
          % (fabric_name, inspection_num, inspection_date, product_name, fabric_name, camera_num_1, camera_num_2,
             inspection_num, ng_face, ng_image, 'marking_' + ng_image, ng_point, result_status, result_status,
             result_status, worker_1, worker_2, unit_num)

    ### rapid解析情報テーブルにレコード追加
    return db_util.operate_data(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# 処理名             ：NG座標中心座標取得
#
# 処理概要           ：1.NG座標の中心座標を取得する。
#
# 引数               ：リサイズ画像幅
#                     リサイズ画像高さ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    NG座標中心座標
#
# ------------------------------------------------------------------------------------
def get_center_ng_point(resize_width, resize_height):
    result = False
    center_ng_point = ''

    try:
        # NG画像サイズから中心座標を取得する。
        # ※割り切れない場合、丸める（四捨五入）
        center_width = str(Decimal(str(resize_width / 2)).quantize(Decimal('0'), rounding=ROUND_HALF_UP))
        center_height = str(Decimal(str(resize_height / 2)).quantize(Decimal('0'), rounding=ROUND_HALF_UP))

        center_ng_point = center_width + ',' + center_height

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, center_ng_point


# ------------------------------------------------------------------------------------
# 処理名             ：対象画像ファイル取得
#
# 処理概要           ：1.対象画像ファイルを取得する。
#                     なお、本処理で取得するのは、RAPIDサーバ上の撮像画像ファイルを対象としている。
#                     （未検知画像（NG判定された画像）をRAPIDサーバ上の撮像画像フォルダから探す処理）
#
# 引数               ：撮像開始時刻
#                     品名
#                     反番
#                     検査番号
#                     画像ルートパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    対象画像ファイル
# ------------------------------------------------------------------------------------
def get_ng_image_file(imaging_starttime, product_name, fabric_name, inspection_num, ng_image_path,
                      ng_image_file_name):
    result = False
    ng_image_file = None
    try:
        # 「(品名)_(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)」
        path_name = product_name + '_' + fabric_name + '_' + imaging_starttime + '_' + str(inspection_num)
        target_file_path = ng_image_path + '\\' + path_name
        target_file_name = '\\*\\' + ng_image_file_name

        if os.path.isdir(target_file_path):
            pass
        else:
            result = True
            return result, ng_image_file

        tmp_result, ng_image_files = get_file(target_file_path, target_file_name)

        if tmp_result:
            # 正常終了した場合でも、ファイルが見つからないケースは存在するので、
            # 存在する場合のみ、対象画像ファイル情報を設定する
            if ng_image_files:
                # 検索時に必ず一意になるので、先頭リストのみ返す
                ng_image_file = ng_image_files[0]
            else:
                pass
        else:
            return result, ng_image_file

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
    return result, ng_image_file


# ------------------------------------------------------------------------------------
# 処理名             ：対象画像ファイルコピー
#
# 処理概要           ：1.未検知画像圧縮フォルダを作成する。
#                    2.作成したフォルダに対象画像ファイル、マーキング画像ファイルをコピーする。
#
# 引数               ：画像ファイルパス（画像ファイル名含む）
#                     出力パス
#                     （コピー先の）マーキング画像ファイル名
#                     機能ID
#                     機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def copy_ng_image_file(ng_image_file_path, output_path,
                       marking_ng_image_file_name):
    result = False

    try:
        # 未検知画像格納フォルダを作成する
        logger.debug('[%s:%s] 未検知画像格納フォルダ:[%s]', app_id, app_name, output_path)
        tmp_result, error = file_util.make_directory(output_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] 未検知画像格納フォルダの作成に失敗しました。:[%s]', app_id, app_name, output_path)
            return result

        # マーキング画像ファイルを、未検知画像格納フォルダにコピーする。
        tmp_result, error = file_util.copy_file(ng_image_file_path, output_path, logger, app_id, app_name)
        tmp_input_path = output_path + '\\' + os.path.basename(ng_image_file_path)
        tmp_output_path = output_path + '\\' + marking_ng_image_file_name
        shutil.move(tmp_input_path, tmp_output_path)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] マーキング画像ファイルのコピーに失敗しました。', app_id, app_name)
            return result

        # NG画像ファイルを、未検知画像格納フォルダにコピーする。
        tmp_result, error = file_util.copy_file(ng_image_file_path, output_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] NG画像ファイルのコピーに失敗しました。', app_id, app_name)
            return result


        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル更新
#
# 処理概要           ：1.NG判定でエラーになった場合、RAPID解析情報テーブルのステータス
#                        （RAPID結果、端判定結果、マスキング判定結果）を更新する。
#
# 引数               ：コネクションオブジェクト
#                     カーソルオブジェクト
#                     反番
#                     検査番号
#                     検査日付
#                     号機情報
#                     判定結果ステータス(none)
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    検査情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_rapid_analysis(conn, cur, fabric_name, inspection_num, inspection_date,
                          ng_image_file_name, unit_num):

    # DB登録値
    status = common_inifile.get('ANALYSIS_STATUS', 'none')

    inspection_num = str(int(inspection_num))

    ### クエリを作成する
    sql = 'update "rapid_%s_%s_%s" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where ng_image = \'%s\' and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, status, status, status, ng_image_file_name, unit_num)

    logger.debug('[%s:%s] RAPID解析情報テーブル更新SQL=[%s]' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.未検知画像登録を行う。
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main():
    # 変数定義
    error_file_name = None
    conn = None
    cur = None
    target_ng_image_files = None
    try:

        ### 設定ファイルからの値取得
        # 共通設定：各種通知ファイルが格納されるルートパス
        input_root_path = inifile.get('PATH', 'input_path')
        # 共通設定：各種通知ファイルを退避させるルートパス
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # RAPIDサーバホスト名
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        # 連携基盤のルートディレクトリ
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # 未検知画像格納ファイルパス
        undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # 完了通知が格納されるフォルダパス
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path
        # 未検知画像通知ファイル名パターン
        file_name_pattern = inifile.get('FILE', 'file_name_pattern')
        # 未検知画像通知ファイル：退避ディレクトリパス
        backup_path = inifile.get('PATH', 'backup_path')
        backup_path = backup_root_path + '\\' + backup_path
        # 撮像画像を格納するパス
        image_path = inifile.get('PATH', 'image_dir')
        # 検査対象ライン番号
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        # プログラム判定値：未検知画像
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))
        # マーキングNG画像名の接頭辞
        marking_ng_image_file_name_prefix = inifile.get('FILE', 'marking_ng_image_file_name_prefix')
        # NG画像サイズ
        resize_width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        resize_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### 未検知画像通知監視
        while True:
            # フォルダ内に未検知画像通知ファイルが存在するか確認する
            logger.debug('[%s:%s] 未検知画像通知ファイルの確認を開始します。', app_id, app_name)
            result, sorted_files = get_file(file_path, file_name_pattern)

            if result:
                pass
            else:
                logger.error('[%s:%s] 未検知画像通知ファイルの確認に失敗しました。', app_id, app_name)
                sys.exit()

            # 未検知画像通知ファイルがない場合は一定期間sleepして再取得
            if len(sorted_files) == 0:
                logger.info('[%s:%s] 未検知画像通知ファイルが存在しません。', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            logger.debug('[%s:%s] 未検知画像通知ファイルを発見しました。:未検知画像通知ファイル名[%s]',
                         app_id, app_name, sorted_files)

            # DB共通処理を呼び出して、DBに接続する。
            logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
                sys.exit()

            for i in range(len(sorted_files)):

                basename = os.path.basename(sorted_files[i])
                file_name = re.split('[_.]', basename)
                fabric_name = file_name[1]
                inspection_num = file_name[3]
                logger.info('[%s:%s] %s処理を開始します。 [反番,検査番号]=[%s, %s]', app_id, app_name, app_name, fabric_name,
                            inspection_num)
                ### ファイル読込
                logger.debug('[%s:%s] 未検知画像通知ファイルの読込を開始します。', app_id, app_name)

                result, product_name, fabric_name, inspection_num, \
                face_num, camera_num_1, camera_num_2, ng_image_file_name, file_datetime = \
                    read_file(sorted_files[i])

                if result:
                    logger.debug('[%s:%s] 未検知画像通知ファイルの読込が終了しました。未検知画像通知ファイル名=[%s]',
                                 app_id, app_name, sorted_files[i])
                else:
                    logger.error('[%s:%s] 未検知画像通知ファイルの読込に失敗しました。未検知画像通知ファイル名=[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # 未検知画像通知ファイルを退避するフォルダを作成する。
                logger.debug('[%s:%s] 未検知画像通知ファイルを退避するフォルダ作成を開始します。', app_id, app_name)
                result = exists_dir(backup_path)

                if result:
                    logger.debug('[%s:%s] 未検知画像通知ファイルを退避するフォルダ作成が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 未検知画像通知ファイルを退避するフォルダ作成に失敗しました。退避先フォルダ名=[%s]',
                                 app_id, app_name, backup_path)
                    sys.exit()

                # 退避する未検知画像通知ファイルと同名のファイルが存在するか確認する。
                logger.debug('[%s:%s] 撮像完了通知ファイル移動を開始します。撮像完了通知ファイル名=[%s]',
                             app_id, app_name, sorted_files[i])
                result = move_file(sorted_files[i], backup_path)

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイル移動が終了しました。退避先フォルダ=[%s], 撮像完了通知ファイル名=[%s]',
                                 app_id, app_name, backup_path, sorted_files[i])
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルの退避に失敗しました。撮像完了通知ファイル名=[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                ### 対象画像の格納されているフォルダを全RAPIDサーバーを参照して特定する。
                logger.debug('[%s:%s] 対象画像の特定を開始します。', app_id, app_name)

                for rapid_hostname in rapid_hostname_list:
                    target_path = '\\\\' + rapid_hostname + '\\' + image_path
                    logger.debug('[%s:%s] 対象画像の特定を開始します。[%s]', app_id, app_name, target_path)
                    result, target_ng_image_files = \
                        get_ng_image_file(file_datetime, product_name, fabric_name, inspection_num,
                                          target_path, ng_image_file_name)
                    if result:
                        logger.debug('[%s:%s] 対象画像の特定が終了しました。[%s]', app_id, app_name, target_path)
                    else:
                        logger.error('[%s:%s] 対象画像の特定に失敗しました。[%s]', app_id, app_name, target_path)
                        sys.exit()
                    # RAPIDサーバ上でファイルを発見した場合は、その時点で特定を終了
                    if target_ng_image_files is not None:
                        break

                if target_ng_image_files is not None:
                    logger.debug('[%s:%s] 対象画像の特定が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 対象画像の特定に失敗しました。', app_id, app_name)
                    sys.exit()

                # 未検知画像圧縮フォルダを作成し、作成したフォルダに特定した画像をコピーする。
                target_ng_image_path = rk_root_path + '\\' + undetected_image_file_path + '\\' + os.path.splitext(os.path.basename(sorted_files[i]))[0]
                logger.debug('[%s:%s] 対象画像のコピーを開始します。', app_id, app_name)
                result = copy_ng_image_file(target_ng_image_files, target_ng_image_path,
                                             marking_ng_image_file_name_prefix + ng_image_file_name)

                if result:
                    logger.debug('[%s:%s] 対象画像のコピーが終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 対象画像のコピーに失敗しました。', app_id, app_name)
                    sys.exit()

                ### RAPID解析情報登録
                # NG座標の中心座標を取得
                logger.debug('[%s:%s] NG座標の中心座標取得を開始します。', app_id, app_name)
                result, center_ng_point = get_center_ng_point(resize_width, resize_height)

                if result:
                    logger.debug('[%s:%s] NG座標の中心座標取得が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG座標の中心座標取得に失敗しました。', app_id, app_name)
                    sys.exit()

                # DB共通処理を呼び出して、RAPID解析情報テーブルに以下の項目を登録する。
                logger.debug('[%s:%s] RAPID解析情報テーブルの登録を開始します。', app_id, app_name)

                file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(target_ng_image_files)).strftime("%Y-%m-%d %H:%M:%S")
                tmp_result, records, conn, cur = select_inspection_info(conn, cur, fabric_name, inspection_num, file_timestamp, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] 作業者情報取得が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 作業者情報取得に失敗しました。', app_id, app_name)
                    return tmp_result, conn, cur

                worker_1 = records[0]
                worker_2 = records[1]
                imaging_starttime = records[2]
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                result, error, conn, cur = \
                    insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2, inspection_num,
                                               '#' + str(face_num), ng_image_file_name, center_ng_point, worker_1,
                                               worker_2, cur, conn, inspection_date, unit_num)

                if result:
                    logger.debug('[%s:%s] RAPID解析情報テーブルの登録が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] RAPID解析情報テーブルの登録に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # コミットする
                conn.commit()

                ### NG行・列判定機能呼出
                logger.debug('[%s:%s] NG行・列判定機能呼出を開始します。', app_id, app_name)

                result = register_ng_info_undetect.main(
                    product_name, fabric_name, inspection_num, 0, ng_image_file_name, center_ng_point,
                    undetected_image_flag_is_undetected, imaging_starttime, unit_num)

                if result:
                    logger.debug('[%s:%s] NG行・列判定機能呼出が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG行・列判定機能呼出に失敗しました。', app_id, app_name)
                    logger.info('[%s:%s] NG判定エラーのため、RAPID解析情報テーブルを更新します。 [反番,検査番号]=[%s, %s]', app_id, app_name, fabric_name, inspection_num)

                    result, conn, cur = update_rapid_analysis(conn, cur, fabric_name, inspection_num, inspection_date, ng_image_file_name, unit_num)

                    if result:
                        logger.info('[%s:%s] RAPID解析情報テーブル更新が完了しました。', app_id, app_name)
                        # コミットする
                        conn.commit()
                    else:
                        logger.error('[%s:%s] RAPID解析情報テーブル更新に失敗しました。', app_id, app_name)
                        sys.exit()


                ### NG画像圧縮・転送機能呼出
                logger.debug('[%s:%s] NG画像圧縮・転送機能呼出を開始します。', app_id, app_name)

                result = compress_image_undetect.main(
                    product_name, fabric_name, inspection_num, file_datetime,
                    rk_root_path + '\\' + undetected_image_file_path, target_ng_image_path)

                if result:
                    logger.debug('[%s:%s] NG画像圧縮・転送機能呼出が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG画像圧縮・転送機能呼出に失敗しました。', app_id, app_name)
                    sys.exit()

                logger.info("[%s:%s] %s処理は正常に終了しました。[反番,検査番号]=[%s, %s]", app_id, app_name, app_name, fabric_name, inspection_num)

            # DB接続を切断
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)

            # 次の監視間隔までスリープ
            time.sleep(sleep_time)

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
    main()
