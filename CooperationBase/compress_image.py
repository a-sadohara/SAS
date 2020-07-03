# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能310  NG画像圧縮・転送
# ----------------------------------------

import configparser
import logging.config
import os
import re
import sys
import time
import traceback
import datetime
from concurrent.futures.thread import ThreadPoolExecutor

import win32_setctime

import error_detail
import error_util
import db_util
import file_util
import light_patlite

import compress_image_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_compress_image.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 関数名             ：DB接続
#
# 処理概要           ：1.DBと接続する
#
# 引数               ：なし
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
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def close_connection(conn, cur):
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報取得（DBポーリング）
#
# 処理概要           ：1.反物情報テーブルから反物情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                    カーソルオブジェクト
#                    処理ステータステーブルのステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    処理ステータス情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fabric_info_db_polling(conn, cur, fabric_info_status_ng_ziptrans_start, unit_num):
    ### クエリを作成する
    sql = 'select product_name, fabric_name, inspection_num, imaging_starttime, processed_num from fabric_info ' \
          'where status = %s and unit_num = \'%s\' order by ng_endtime asc, imaging_starttime asc' \
          % (fabric_info_status_ng_ziptrans_start, unit_num)

    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, records, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像ファイル取得
#
# 処理概要           ：1.撮像画像ファイルのファイルリストを取得する。
#
# 引数               ：撮像画像ファイル格納フォルダパス
#                      撮像画像ファイル
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像完了通知ファイルリスト
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    result = False
    file_list = None

    try:
        logger.debug('[%s:%s] 撮像画像ファイル格納フォルダパス=[%s]', app_id, app_name, file_path)

        # 共通関数で撮像画像ファイル格納フォルダ情報を取得する
        file_list = None
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result:
            # 成功時
            pass
        else:
            # 失敗時
            logger.error("[%s:%s] 撮像画像ファイル格納フォルダにアクセス出来ません。", app_id, app_name)
            return result, file_list

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, file_list


# ------------------------------------------------------------------------------------
# 処理名             ：フォルダ存在チェック
#
# 処理概要           ：1.フォルダが存在するかチェックする。
#                    2.フォルダが存在しない場合は作成する。
#
# 引数               ：フォルダパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    logger.debug('[%s:%s] フォルダを作成します。フォルダ名：[%s]',
                 app_id, app_name, target_path)
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：撮像画像ファイル移動
#
# 処理概要           ：1.撮像画像ファイルを、フォルダに移動させる。
#
# 引数               ：撮像画像ファイル
#                      移動先フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    # ファイル移動
    ctime = datetime.datetime.fromtimestamp(os.path.getctime(target_file)).timestamp()
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)
    win32_setctime.setctime(move_dir + '\\' + os.path.basename(target_file), ctime)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：空フォルダ削除
#
# 処理概要           ：1.指定されたフォルダより下の空フォルダを削除する。
#
# 引数               ：削除フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def delete_empty_dir(del_path):
    result = False
    file_list = []
    try:
        # ファイル情報を取得
        tmp_result, file_list, error = file_util.get_file_list(del_path, '*', logger, app_id, app_name)
        if tmp_result:
            pass
        else:
            logger.error("[%s:%s] 撮像画像ファイル格納フォルダにアクセス出来ません。:フォルダ名[%s]", app_id, app_name, del_path)
            return result

        for file in file_list:
            if os.path.isdir(file):
                # フォルダの場合は、空であるかどうか調べる
                tmp_result, tmp_file_list, error = file_util.get_file_list(file, '\\*', logger, app_id, app_name)

                if tmp_result:
                    pass
                else:
                    logger.error("[%s:%s] 撮像画像ファイル格納フォルダにアクセス出来ません。:フォルダ名[%s]", app_id, app_name, file)
                    return result
                if tmp_file_list:
                    # 空フォルダでない場合、何もしない
                    pass
                else:
                    # 空フォルダの場合、削除する
                    tmp_result, error = file_util.delete_dir(file, logger, app_id, app_name)

                    if tmp_result:
                        pass
                    else:
                        logger.error("[%s:%s] 撮像画像ファイル格納フォルダの削除に失敗しました。:フォルダ名[%s]", app_id, app_name, file)
                        return result

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：撮像画像整理
#
# 処理概要           ：1.撮像画像を整理する。
#
# 引数               ：撮像画像ルートフォルダ
#                     撮像開始時刻
#                     品名
#                     反番
#                     検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def image_organize(image_root_path, date, product_name, fabric_name, inspection_num):
    result = False
    try:
        ### 設定ファイルからの値取得
        # 画像ファイルの拡張子
        extension = common_inifile.get('FILE_PATTERN', 'image_file')
        # 撮像画像をカメラ番号単位で整理する。

        # 参照先パス構成は以下を想定する。
        # 「(品名)_(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)」
        #                 ｜-(処理ID１)
        #                 ｜-(処理ID２)
        #                 ｜-(処理ID３)
        image_path = image_root_path + product_name + '_' + fabric_name + '_' + date + '_' + \
                     str(inspection_num).zfill(2) + '\\'

        # ファイル名パターンは以下を想定する。
        # 「\\*\\*.jpg」
        image_file_name_pattern = '*\\' + extension

        logger.debug('[%s:%s] 撮像画像ファイルの確認を開始します。', app_id, app_name)
        tmp_file_list, image_files = get_file(image_path, image_file_name_pattern)

        if tmp_file_list:
            logger.debug('[%s:%s] 撮像画像ファイルの確認が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 撮像画像ファイルの確認に失敗しました。', app_id, app_name)
            return result

        logger.debug('[%s:%s] 撮像画像ファイル:[%s]', app_id, app_name, image_files)

        # 画像名からカメラ番号を特定し、存在する処理ID単位のフォルダをカメラ番号単位のフォルダとして画像を移動させる。
        # なお、ファイル名は「[品名]_[反番]_[日付]_[検査番号]_ [検反部No]_[カメラ番号]_[連番].jpg」を想定している
        #
        # 「(品名)_(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)」
        #                 ｜-(処理ID)
        # ⇒
        # 「(品名)_(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)」
        #                 ｜-[検反部No]_[カメラ番号]
        for image_file in image_files:
            # ファイル名から検反部Noとカメラ番号を取得
            image_basename = os.path.basename(image_file)
            file_name = re.split('[_.]', image_basename)
            face_no = file_name[4]
            camera_num = file_name[5]

            move_path = image_root_path + product_name + '_' + fabric_name + '_' + date + '_' + \
                        str(inspection_num).zfill(2) + '\\' + str(face_no) + '_' + str(camera_num) + '\\'

            # ファイル格納先が存在しなければ作成
            logger.debug('[%s:%s] 撮像画像を移動するフォルダ存在チェックを開始します。', app_id, app_name)
            tmp_file_list = exists_dir(move_path)

            if tmp_file_list:
                logger.debug('[%s:%s] 撮像画像を移動するフォルダ存在チェックが終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 撮像画像を移動するフォルダ存在チェックに失敗しました。:移動先フォルダ名[%s]',
                             app_id, app_name, move_path)
                return result

            # 撮像完了通知ファイルを、退避フォルダに移動させる。
            logger.debug('[%s:%s] 撮像画像ファイル移動を開始します。:撮像画像ファイル名[%s]',
                         app_id, app_name, image_files)
            if image_file.find(move_path) == 0:
                # 既に移動済（処理IDとカメラ番号が同一で移動が不要）の場合
                pass
            else:
                tmp_file_list = move_file(image_file, move_path)
                if tmp_file_list:
                    logger.debug('[%s:%s] 撮像画像ファイル移動が終了しました。:移動先フォルダ[%s], 撮像画像ファイル名[%s]',
                                 app_id, app_name, move_path, image_file)
                else:
                    logger.error('[%s:%s] 撮像画像ファイル移動に失敗しました。:撮像画像ファイルァイル名[%s]',
                                 app_id, app_name, image_file)
                    return result

        # 全ての画像を移動させた後、残った空のフォルダを削除する。
        logger.debug('[%s:%s] 空フォルダ削除を開始します。', app_id, app_name)

        tmp_file_list = delete_empty_dir(image_path)

        if tmp_file_list:
            logger.debug('[%s:%s] 空フォルダ削除が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 空フォルダ削除に失敗しました。', app_id, app_name)
            return result

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報テーブルステータス更新
#
# 処理概要           ：1.反物情報テーブルのステータスを更新する。
#
# 引数               ：ステータス
#                      カラム名
#                      現在日付
#                      反番
#                      検査番号
#                      コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info_status(status, column, now_datetime, fabric_name, inspection_num, cur, conn, imaging_starttime,
                              unit_num):
    ### クエリを作成する
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, now_datetime, fabric_name, inspection_num, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：パトライト点灯
#
# 処理概要           ：1.パトライト点灯を行う。
#
# 引数               ：出力パス
#                     出力ファイル名
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def exec_patrite(file_name):
    result, error = file_util.light_patlite(file_name, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.NG画像圧縮・転送を行う。
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
    try:

        ### 設定ファイルからの値取得
        # 連携基盤のルートパス
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # 反物情報テーブル.処理ステータス：NG行列判定完了
        fabric_info_status_ng_end = common_inifile.get('FABRIC_STATUS', 'ng_end')
        # 反物情報テーブル.処理ステータス：NG画像圧縮・転送開始
        fabric_info_status_zip_start = common_inifile.get('FABRIC_STATUS', 'ng_ziptrans_start')
        # 反物情報テーブル.処理ステータス：NG画像圧縮・転送完了
        fablic_info_status_zip_end = common_inifile.get('FABRIC_STATUS', 'ng_ziptrans_end')
        # 反物情報テーブル.カラム名：NG画像圧縮・転送開始時刻
        fabric_info_column_zip_start = inifile.get('COLUMN', 'ng_ziptrans_start')
        # 反物情報テーブル.カラム名：NG画像圧縮・転送完了時刻
        fablic_info_column_zip_end = inifile.get('COLUMN', 'ng_ziptrans_end')
        # パトライト点灯のトリガーとなるファイルパス
        send_patrite_trigger_file_path = inifile.get('PATH', 'send_patrite_trigger_file_path')
        # パトライト点灯のトリガーとなるファイル名
        send_parrite_file_name = inifile.get('FILE', 'send_parrite_file_name')
        # 撮像画像を格納するパス
        image_path = inifile.get('PATH', 'image_dir')
        # NG画像格納パス
        rapid_ng_image_file_path = inifile.get('PATH', 'rapid_ng_image_file_path')
        # RAPIDサーバホスト名
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        # 検査対象ライン番号
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### 圧縮対象特定（DBポーリング）
        while True:
            # DB共通処理を呼び出して、DB接続を行う
            logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
                sys.exit()

            # DB共通処理を呼び出して、反物情報テーブルからデータを取得する。
            logger.debug('[%s:%s] 反物情報取得を開始します。', app_id, app_name)
            result, target_records, conn, cur = \
                select_fabric_info_db_polling(conn, cur, fabric_info_status_ng_end, unit_num)

            if result:
                logger.debug('[%s:%s] 反物情報取得が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 反物情報取得に失敗しました。', app_id, app_name)
                conn.rollback()
                sys.exit()

            # 反物情報がない場合は一定期間sleepして再取得
            if len(target_records) == 0:
                logger.info('[%s:%s] 圧縮対象が存在しません。', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            # DB共通処理を呼び出し、反物情報テーブルの以下の項目を更新する。
            for target_record in target_records:

                product_name = target_record[0]
                fabric_name = target_record[1]
                inspection_num = target_record[2]
                imaging_starttime = target_record[3]
                processed_num = int(target_record[4])
                now_datetime = datetime.datetime.now()
                date = imaging_starttime.strftime('%Y%m%d')

                logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]',
                            app_id, app_name, app_name, fabric_name, inspection_num, date)

                logger.debug('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送開始）を開始します。', app_id, app_name)
                result, conn, cur = \
                    update_fabric_info_status(fabric_info_status_zip_start, fabric_info_column_zip_start, now_datetime,
                                              fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num)

                if result:
                    logger.debug('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送開始）が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送開始）に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # コミットする
                conn.commit()
                
                if processed_num == 0:
                    pass
                else:
                    ### 撮像画像整理
                    logger.debug('[%s:%s] 撮像画像整理を開始します。', app_id, app_name)
                    result_list=[]
                    error_list=[]
                    with ThreadPoolExecutor() as executor:
                        func_list = []
                        for rapid_hostname in rapid_hostname_list:
                            target_path = '\\\\' + rapid_hostname + '\\' + image_path + '\\'
                            # スレッド実行
                            func_list.append(
                                executor.submit(
                                    image_organize,
                                    target_path, date, product_name, fabric_name, inspection_num))
                        for i in range(len(rapid_hostname_list)):
                            # スレッド戻り値を取得
                            result_list.append(func_list[i].result())

                    for i, multi_result in enumerate(result_list):
                        if multi_result is True:
                            logger.debug('[%s:%s] 撮像画像整理が完了しました。', app_id, app_name)
                        else:
                            logger.error('[%s:%s] 撮像画像整理に失敗しました。', app_id, app_name)
                            error_list.append(multi_result)

                    if len(error_list) > 0:
                        sys.exit()
                    else:
                        pass

                ### NG画像圧縮、検査完了通知作成、ファイル転送

                logger.debug('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送を開始します。', app_id, app_name)
                tmp_result, error, func_name = compress_image_util.exec_compress_and_transfer(
                    product_name, fabric_name, inspection_num, date,
                    rk_root_path + '\\' + rapid_ng_image_file_path, None, 0, logger)

                if tmp_result:
                    logger.debug('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送に失敗しました。', app_id, app_name)
                    return result

                ### ステータス更新
                now_datetime = datetime.datetime.now()
                logger.debug('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送完了）を開始します。', app_id, app_name)
                result, conn, cur = \
                    update_fabric_info_status(fablic_info_status_zip_end, fablic_info_column_zip_end, now_datetime,
                                              fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送完了）が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 反物情報テーブルの更新（NG画像圧縮・転送完了）に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # コミットする
                conn.commit()

                # パトライト点灯
                logger.debug('[%s:%s] パトライト点灯処理を開始します。', app_id, app_name)

                light_pattern = '001000'
                result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)

                if result:
                    logger.debug('[%s:%s] パトライト点灯処理が終了しました。',
                                 app_id, app_name)
                else:
                    logger.warning('[%s:%s] パトライト点灯処理に失敗しました。',
                                   app_id, app_name)

                logger.info('[%s:%s] %s処理は正常に終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]',
                            app_id, app_name, app_name, fabric_name, inspection_num, date)

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
