# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能306  RAPID解析実行
# ----------------------------------------
import codecs
import os
import re
import socket
import multiprocessing as multi
import configparser
import logging.config
from concurrent.futures.thread import ThreadPoolExecutor
import sys
import json
import traceback
import time
import datetime
import gc

import db_util
import error_util
import file_util
import error_detail

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_rapid_analysis.conf")
logger = logging.getLogger("rapid_analysis")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/rapid_analysis_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：RAPIDサーバ接続情報取得（worker単位）
#
# 処理概要           ：1.RAPIDサーバ接続情報を取得する。
#
# 引数               ：RAPIDサーバホスト名
#                    RAPIDサーバのポート番号
#                    リトライ回数
#
# 戻り値             ：処理成否（True:成功、False:失敗）
#                    socket情報
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info_worker(ip_address, port_numbers, retry_num):
    result = False
    sock = []

    try:
        # IMA 常駐プロセスのipアドレスとport番号を指定
        ip_address = str(ip_address)
        port_number = int(port_numbers)

        # IMA 常駐プロセスに接続
        logger.debug('[%s:%s] IMA 常駐プロセスに接続します。[IPアドレス:%s][ポート番号:%s]',
                     app_id, app_name,
                     ip_address, port_number
                     )
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        err = sock.connect_ex((ip_address, port_number))

        # 結果判定
        retry_count = 0
        while err != 0:
            #  接続失敗
            logger.warning('[%s:%s] %s: [IPアドレス:%s][ポート番号:%s] ソケットの接続に失敗しました。',
                           app_id, app_name,
                           str(err), ip_address, str(port_number))
            # 失敗時には一定回数リトライする。
            if retry_count < retry_num:
                err = sock.connect_ex((ip_address, port_number))
                pass
            else:
                # リトライ回数超過した場合
                break
            # リトライ回数をカウントアップする
            retry_count = retry_count + 1

        else:
            logger.debug('[%s:%s] %s: [IPアドレス:%s][ポート番号:%s] ソケットの接続に成功しました。[リトライ回数:%s]',
                         app_id, app_name,
                         str(err), ip_address, str(port_number), retry_count)

        if err != 0:
            # エラー終了
            logger.error('[%s:%s] RAPIDサーバ接続の接続に失敗しました。', app_id, app_name)
            return result, sock

        else:
            # 正常終了
            result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock


# ------------------------------------------------------------------------------------
# 処理名             ：RAPIDサーバ接続情報取得（ホスト単位）
#
# 処理概要           ：1.RAPIDサーバ接続情報を取得する。
#
# 引数               ：RAPIDサーバホスト名
#                    RAPIDサーバのポート番号
#                    worker数
#                    リトライ回数
#
# 戻り値             ：処理成否（True:成功、False:失敗）
#                    socket情報リスト
#
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info_host(ip_address, port_numbers, worker, retry_num):
    result = False
    sock = []

    try:
        # port番号を分割
        port_numbers = port_numbers.split(',')

        # worker数分、RAPID常駐プロセスに接続
        for i in range(worker):
            # IMA 常駐プロセスのipアドレスとport番号を指定
            ip_address = str(ip_address)
            port_number = int(port_numbers[i])

            tmp_result, sock_work = get_rapid_server_connect_info_worker(ip_address, port_number, retry_num)

            if tmp_result:
                # 成功した場合
                sock.append(sock_work)
            else:
                # 失敗した場合
                return result, sock

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock


# ------------------------------------------------------------------------------------
# 処理名             ：RAPIDサーバ接続情報取得
#
# 処理概要           ：1.RAPIDサーバ接続情報を取得する。
#
# 引数               ：RAPIDサーバホスト名
#                    RAPIDサーバのポート番号
#                    worker数
#                    リトライ回数
#
# 戻り値             ：処理成否（True:成功、False:失敗）
#                    socket情報リスト
#
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info(ip_addresses, port_numbers, worker, retry_num):
    result = False
    sock_all = []

    try:
        # ipアドレス数分、RAPID常駐プロセスに接続
        for i in range(len(ip_addresses)):
            sock_all.append([])
            ip_address = str(ip_addresses[i])

            tmp_result, sock = \
                get_rapid_server_connect_info_host(ip_address, port_numbers, worker, retry_num)

            if tmp_result:
                # 正常終了
                sock_all[i].extend(sock)
            else:
                # 異常終了
                # 内部関数でログ出力しているため、ここではログ出力しない
                return result, sock_all

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock_all


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
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報取得（DBポーリング）
#
# 処理概要           ：1.処理ステータステーブルから反物情報を取得する。
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
def select_fabric_info_db_polling(conn, cur, processing_status_status):
    ### クエリを作成する
    sql = 'select' \
          '  fabric_info.product_name' \
          ' ,fabric_info.fabric_name' \
          ' ,fabric_info.inspection_num' \
          ' ,fabric_info.imaging_starttime' \
          ' ,processing_status.rapid_starttime' \
          ' from fabric_info,processing_status' \
          ' where fabric_info.fabric_name = processing_status.fabric_name' \
          '  and fabric_info.inspection_num = processing_status.inspection_num' \
          '  and processing_status.status = %s' \
          '  order by processing_status.resize_endtime asc' \
          % processing_status_status

    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：品番登録情報取得
#
# 処理概要           ：1.品番登録情報テーブルからAIモデル名を取得する。
#
# 引数               ：コネクションオブジェクト
#                    カーソルオブジェクト
#                    品名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    AIモデル名
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_matser_info(conn, cur, product_name):
    ### クエリを作成する
    sql = 'select ai_model_name from mst_product_info where product_name = \'%s\'' % product_name

    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報テーブルステータス更新
#
# 処理概要           ：1.反物情報テーブルのステータスを更新する。
#
# 引数               ：ステータス
#                     版番
#                     検査番号
#                     カーソルオブジェクト
#                     コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info(column_name, time, status, fabric_name, inspection_num, cur, conn):
    ### クエリを作成する
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s' \
          % (status, column_name, time, fabric_name, inspection_num)

    ### 反物情報テーブルを更新
    logger.debug('[%s:%s] 反番[%s], 検査番号[%s]のレコードを更新しました。ステータス[%s]',
                 app_id, app_name, fabric_name, inspection_num, status)
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス情報取得（RAPID解析対象）
#
# 処理概要           ：1.処理ステータステーブルから情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                     カーソルオブジェクト
#                     版番
#                     検査番号
#                     RAPIDサーバホスト名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    処理ステータス情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_processing_status_rapid_target(conn, cur, fabric_name, inspection_num, rapid_host_name, status_resize_end):
    ### クエリを作成する
    sql = 'select processing_id from processing_status ' \
          'where fabric_name = \'%s\' and inspection_num = %s and rapid_host_name = \'%s\' and status = %s ' \
          % (fabric_name, inspection_num, rapid_host_name, status_resize_end)

    # DB共通処理を呼び出して、処理ステータステーブルからデータを取得する。
    result, records, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータステーブルステータス更新
#
# 処理概要           ：1.処理ステータステーブルのステータスを更新する。
#
# 引数               ：ステータス
#                     版番
#                     検査番号
#                     処理ID
#                     RAPIDサーバホスト名
#                     カーソルオブジェクト
#                     コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_processing_status(time, status, fabric_name, inspection_num, processing_id, rapid_server_host_name,
                             cur, conn):
    ### クエリを作成する
    sql = 'update processing_status set status = %s, rapid_starttime = \'%s\'' \
          '  where fabric_name = \'%s\' and inspection_num = %s and processing_id = %s and rapid_host_name = \'%s\' ' \
          % (status, time, fabric_name, inspection_num, processing_id, rapid_server_host_name)

    ### 処理ステータステーブルを更新
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像ファイル取得
#
# 処理概要           ：1.撮像画像ファイルのファイルリストを取得する。
#
# 引数               ：撮像画像ファイル格納フォルダパス
#                      撮像画像ファイル
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像完了通知ファイルリスト
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    logger.debug('[%s:%s] 撮像画像ファイル格納フォルダパス=[%s]', app_id, app_name, file_path)
    # 共通関数で撮像画像ファイル格納フォルダ情報を取得する

    result, file_list = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

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
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析実行（ラッパー関数）
#
# 処理概要           ：1.RAPID解析実行処理を呼び出す。
#
# 引数               ：パラメータ情報
#
# 戻り値             ：RAPID解析実行の戻り値（複数個）を、１リスト形式で返却する。
#                     例：
#                      RAPID解析実行の戻り値が3つ「True, 'xxx', sockオブジェクト」の場合、
#                      本関数では、「return[0]='True', return[1]='xxx', return[2]=sockオブジェクト」を返却する。
# ------------------------------------------------------------------------------------
def wrapper_process(args):
    return process(*args)


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析実行
#
# 処理概要           ：1.RAPIDサーバにリクエスト送信し、結果を取得する。
#
#
# 引数               ：ソケット情報
#                    リクエストJSON
#                    RAPIDサーバホスト名
#                    RAPIDサーバのポート番号
#                    リトライ回数
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    レスポンス結果
#                    ソケット情報
# ------------------------------------------------------------------------------------
def process(sock, request_json, ip_address, port_num, retry_num):

    result = False
    received_str = None

    try:
        ### 設定ファイルからの値取得
        retry_target_err_no = inifile.get('VALUE', 'rapid_server_retry_error_code')
        retry_target_err_no = retry_target_err_no.split(',')

        for x in range(retry_num):
            # 開始時刻取得
            start = time.perf_counter()
            # IMA 常駐プロセスにリクエストを送信
            sock.send(request_json.encode('utf-8'))

            # IMA 常駐プロセスからレスポンスを受信
            # レスポンスの最初の 9byte はレスポンスの長さ（byte）を示す
            response_size = int(sock.recv(9))

            # IMA 常駐プロセスからのレスポンスをレスポンスの長さ分受信
            received_byte = 0
            received_str = ''
            while received_byte < response_size:
                response = sock.recv(min(4096, response_size - received_byte))
                received_byte = received_byte + len(response)
                received_str = received_str + response.decode('utf-8', 'ignore')

            # 終了時刻取得
            end = time.perf_counter()
            logger.debug('[%s:%s] process: exec time:[%s]', app_id, app_name, end - start)

            # エラーコード判定
            result_data = json.loads(received_str)
            result_data_len = len(result_data['result_list'])

            # RAPID解析結果から、処理結果（IMA_status）を確認する。
            is_retry = False
            if result_data_len > 0:
                for j in range(result_data_len):
                    result_list = result_data['result_list'][j]
                    ima_status = str(result_list['IMA_status'])
                    logger.debug('[%s:%s] 処理結果（IMA_status）:[%s]', app_id, app_name, ima_status)

                    if ima_status == '0':
                        # 正常終了の場合
                        result = True
                        return result, received_str, sock
                    else:
                        # 異常終了の場合
                        if x < (retry_num - 1):
                            # リトライ回数内の場合
                            # リトライ対象であるか調べる
                            retry_target_err_no = retry_target_err_no
                            if ima_status in retry_target_err_no:
                                # 102：DEAD_PROCESS or 303：NOREV_FOUND or 591の場合
                                logger.warning('[%s:%s] RAPID解析処理をリトライします。:[%s]', app_id, app_name, ima_status)
                                # socket再接続
                                worker_result, sock = \
                                    get_rapid_server_connect_info_worker(ip_address, port_num, retry_num)
                                break
                            else:
                                # 上記以外のエラー
                                logger.error('[%s:%s] RAPID解析処理が異常終了しました。:[%s]', app_id, app_name, ima_status)
                                return result, received_str, sock
                        else:
                            # リトライ回数超過の場合
                            logger.error('[%s:%s] RAPID解析処理が異常終了しました。（リトライ回数超過）:[%s]', app_id, app_name, ima_status)
                            return result, received_str, sock

            else:
                # レスポンス結果が無い場合はエラー
                return result, received_str, sock

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, received_str, sock


# ------------------------------------------------------------------------------------
# 処理名             ：NG結果登録
#
# 処理概要           ：1.RAPID解析結果を解析し、NG結果をRAPID解析情報テーブルに登録する。
#
# 引数               ：JSONレスポンス文字列
#                     品名
#                     版番
#                     検査番号
#                     プロセスID
#                     コネクションオブジェクト
#                     カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    NG枚数
#                    NG画像のファイルパス（相対）
#
# ------------------------------------------------------------------------------------
def ng_result_register(received_str, product_name, fabric_name, inspection_num, processing_id, start_time,
                       end_time, rapid_server_host_name, image_dir, conn, cur):
    result = False
    rapid_processed_num = 0
    rapid_ng_num = 0
    ng_file_relative_path_list = []

    ### 設定ファイルからの値取得
    # RAPID解析結果:確信度
    analysis_ng_Threshold = float(inifile.get('VALUE', 'analysis_ng_Threshold'))

    # RAPID解析結果をdict形式に変換
    result_data = json.loads(received_str)
    result_data_len = len(result_data['result_list'])

    # RAPID解析結果から、各画像の解析結果を確認する。
    if result_data_len > 0:
        for i in range(result_data_len):
            image_ok_flag = True

            result_list = result_data['result_list'][i]
            image_file = result_list['image']
            results = result_list["result"]
            logger.debug('[%s:%s] [%s]:[result_list=[%s]]', app_id, app_name, i, result_list)
            logger.debug('[%s:%s] [%s]:[image_file=[%s],[results=[%s]]', app_id, app_name,
                         i, result_list, image_file)

            if len(results) > 0:
                for j in range(len(results)):
                    rapid_result = results[j]
                    logger.debug('[%s:%s] [%s]:[rapid_result=[%s]', app_id, app_name, j, rapid_result)
                    # 解析結果
                    result_status = rapid_result.get('category')
                    # 確信度
                    confidence = float(rapid_result.get('confidence'))

                    logger.debug('[%s:%s] [%s]:[result_status(category)=[%s],[confidence=[%s]',
                                 app_id, app_name, j,
                                 result_status, confidence)

                    # 結果判定
                    if result_status == 'NG' and confidence >= analysis_ng_Threshold:
                        # 解析結果=NG 且つ 確信度≧0.5の場合
                        image_ok_flag = False

                        # NG座標
                        result_position_list = rapid_result.get('position')
                        result_position = str(result_position_list[0]) + ',' + str(result_position_list[1])
                        # ファイル名から、「カメラ番号_検反部No1」「カメラ番号_検反部No2」を取得する。
                        # なお、ファイル名は「[品名]_[反番]_[日付]_[検査番号]_ [検反部No]_[カメラ番号]_[連番].jpg」を想定している
                        image_basename = os.path.basename(image_file)
                        file_name = re.split('[_.]', image_basename)
                        # FACE情報から検版部を判断する。
                        #face_no = file_name[4]
                        face_no = 1
                        if face_no == '1':
                            # 検版部No1の場合
                            camera_no_1 = 1
                            camera_no_2 = None
                        else:
                            # 検版部No2の場合
                            camera_no_1 = None
                            camera_no_2 = 2

                        logger.debug('[%s:%s] [%s]:[result_position=[%s],[camera_num_1=[%s],[camera_num_2=[%s]]',
                                     app_id, app_name, j,
                                     result_position, camera_no_1, camera_no_2)

                        # DB共通処理を呼び出して、RAPID解析情報テーブルに以下の項目を登録する。

                        tmp_result, conn, cur = \
                            insert_rapid_analysis_info(
                                product_name,
                                fabric_name,
                                camera_no_1,
                                camera_no_2,
                                inspection_num,
                                processing_id,
                                image_basename,
                                start_time,
                                end_time,
                                int(common_inifile.get('ANALYSIS_STATUS', 'ng')),
                                result_position,
                                confidence,
                                rapid_server_host_name,
                                image_dir + image_file,
                                cur, conn)

                        if tmp_result:
                            logger.debug('[%s:%s] NG結果登録が完了しました。', app_id, app_name)
                        else:
                            logger.error('[%s:%s] NG結果登録に失敗しました。', app_id, app_name)
                            return result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list

                    else:
                        # 解析結果=OK あるいは 確信度<0.5の場合
                        pass

            else:
                # resultが無い（正常終了した画像の場合、result情報は含まれない）
                logger.debug('[%s:%s] result情報がありません。[ステータス=OK]', app_id, app_name)

            # 処理済枚数、NG枚数のカウントアップ
            if image_ok_flag:
                # 画像にNGが無かった場合
                rapid_processed_num = rapid_processed_num + 1
            else:
                # 画像にNGが有った場合
                rapid_ng_num = rapid_ng_num + 1
                rapid_processed_num = rapid_processed_num + 1
                ng_file_relative_path_list.append(image_file)

        # ここまで到達する場合は正常終了
        result = True
    else:
        # 取得結果が無い（エラー）
        logger.error('[%s:%s] RAPID解析結果の要素に「result_list」がありません。', app_id, app_name)

    return result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list

# ------------------------------------------------------------------------------------
# 処理名             ：検査情報取得
#
# 処理概要           ：1.検査情報ヘッダテーブルから情報を取得する。
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
def select_inspection_info(conn, cur, fabric_name, inspection_num, timestamp):
    ### クエリを作成する
    sql = 'select worker_1, worker_2 from inspection_info_header ' \
          'where fabric_name = \'%s\' and inspection_num = %s and ' \
          'insert_datetime <= \'%s\' order by insert_datetime desc'\
          % (fabric_name, inspection_num, timestamp)

    # DB共通処理を呼び出して、処理ステータステーブルからデータを取得する。
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur

# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル登録
#
# 処理概要           ：1.RAPID解析情報テーブルにNG結果レコードを登録する。
#
# 引数               ：品名
#                     版番
#                     カメラ番号 検反部No.1
#                     カメラ番号 検反部No.2
#                     検査番号
#                     プロセス番号
#                      NG画像名
#                      RAPID解析開始時刻
#                      ステータス
#                      NG座標
#                      確信度
#                      カーソルオブジェクト
#                      コネクションオブジェクト
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id,
                               image_file, imaging_starttime, end_time, result_status, result_position, confidence,
                               rapid_server_host_name, image_name, cur, conn):
    # カメラ番号 検反部について、値が無い場合にはnull文字に置き換える
    if camera_num_1 is None:
        camera_num_1 = 'null'
    if camera_num_2 is None:
        camera_num_2 = 'null'

    file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(image_name)).strftime("%Y-%m-%d %H:%M:%S")
    tmp_result, records, conn, cur = select_inspection_info(conn, cur, fabric_name, inspection_num, file_timestamp)
    if tmp_result:
        logger.debug('[%s:%s] 作業者情報取得が完了しました。', app_id, app_name)
    else:
        logger.error('[%s:%s] 作業者情報取得に失敗しました。', app_id, app_name)
        return tmp_result, conn, cur

    worker_1 = records[0]
    worker_2 = records[1]

    ### クエリを作成する
    sql = 'insert into "rapid_%s_%s" (' \
          'product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id, ng_image, ' \
          'rapid_starttime, rapid_endtime, rapid_result, ng_point, confidence, rapid_host_name, worker_1, worker_2 ' \
          ') values (' \
          '\'%s\',\'%s\', %s, %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' \
          % (fabric_name, inspection_num, product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id,
             image_file, imaging_starttime, end_time, result_status, result_position, confidence,
             rapid_server_host_name, worker_1, worker_2)

    ### rapid解析情報テーブルにレコード追加
    return db_util.operate_data(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# 処理名             ：ファイルコピー
#
# 処理概要           ：1.指定ファイルのコピーを行う。
#
# 引数               ：コピー元ファイルパス
#                     コピー先ファイルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def copy_file(target_file_path, copy_path):
    # ファイルコピー
    result = file_util.copy_file(target_file_path, copy_path, logger, app_id, app_name)
    return result


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータステーブルステータス更新（解析完了）
#
# 処理概要           ：1.処理ステータステーブルのステータスを更新する。
#
# 引数               ：NG枚数
#                     処理済枚数
#                     版番
#                     検査番号
#                     プロセスID
#                     RAPIDサーバホスト名
#                     カーソルオブジェクト
#                     コネクションオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_processing_status_rapid_end(rapid_ng_num, rapid_processed_num,
                                       fabric_name, inspection_num, processing_id, rapid_host_name, end_time,
                                       cur, conn):
    # 処理ステータステーブル.処理ステータス：RAPID解析完了
    status = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
    ### クエリを作成する
    sql = 'update processing_status set status = %s, rapid_ng_num = %s, rapid_processed_num = %s, ' \
          'rapid_endtime = \'%s\'' \
          '  where fabric_name = \'%s\' and inspection_num = %s and processing_id = %s and rapid_host_name = \'%s\' ' \
          % (status, rapid_ng_num, rapid_processed_num, end_time,
             fabric_name, inspection_num, processing_id, rapid_host_name)

    ### 処理ステータステーブルを追加
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析並列実行（マルチスレッド）
#
# 処理概要           ：1.RAPID解析処理を、並列（子スレッドごと）に実行する。
#
# 引数               ：プロセスID
#                     RAPIDサーバホスト名
#                     worker数
#                     RAPIDサーバのポート番号
#                     品名
#                     版番
#                     検査番号
#                     撮像開始時刻
#                     ソケット情報
#                     コネクションオブジェクト
#                     カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def exec_rapid_analysis_multi_thread(rapid_server_host_name, worker, port_num,
                                     product_name, fabric_name, inspection_num,
                                     imaging_starttime,
                                     sock, ai_model_name,
                                     all_processed_num, all_ng_num):
    logger.debug('[%s:%s] exec_rapid_analysis_multi_thread start'
                 '[rapid_server_host_name:%s][worker:%s][port_num:%s]'
                 '[product_name:%s][fabric_name:%s][inspection_num:%s][imaging_starttime:%s]',
                 app_id, app_name,
                 rapid_server_host_name, worker, port_num,
                 product_name, fabric_name, inspection_num, imaging_starttime)

    result = False

    try:
        # 処理ステータステーブル.処理ステータス：RAPID解析開始
        processing_status_rapid_start = common_inifile.get('PROCESSING_STATUS', 'rapid_start')
        # 処理ステータステーブル.カラム名：RAPID解析開始時刻
        status_resize_end = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        # 処理ステータステーブル.カラム名：RAPID解析完了時刻
        processing_column_rapid_end = inifile.get('COLUMN', 'processing_rapid_end')
        # 画像枚数閾値
        image_num_threshold = int(inifile.get('VALUE', 'image_num_threshold'))
        # NG率閾値
        ng_threshold = float(inifile.get('VALUE', 'ng_threshold'))
        # RAPID画像格納ルートパス（相対）
        image_dir = inifile.get('FILE_PATH', 'image_dir')
        image_dir = '\\\\' + rapid_server_host_name + '\\' + image_dir
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')
        mode = inifile.get('REQUEST_PARAMS', 'mode')
        processor = inifile.get('REQUEST_PARAMS', 'processor')
        preprocess = inifile.get('REQUEST_PARAMS', 'preprocess')
        summary = inifile.get('REQUEST_PARAMS', 'summary')
        # rapidリトライ回数
        rapid_connect_num = inifile.get('ERROR_RETRY', 'rapid_connect_num')

        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        ng_dir = inifile.get('FILE_PATH', 'ng_path')

        today_date = str(imaging_starttime.strftime('%Y%m%d'))



        # DB共通機能を呼び出して、DBに接続する。
        logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
        tmp_result, conn, cur = create_connection()

        if tmp_result:
            logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
            return result, conn, cur, all_processed_num, all_ng_num

        ### 処理ID取得
        # DB共通処理を呼び出して、処理ステータステーブルから検査情報を取得する。
        logger.debug('[%s:%s] 処理ステータスデータ取得（RAPID解析対象）を開始します。', app_id, app_name)
        tmp_result, records, conn, cur = \
            select_processing_status_rapid_target(conn, cur, fabric_name, inspection_num,
                                                  rapid_server_host_name, status_resize_end)

        if tmp_result:
            logger.debug('[%s:%s] 処理ステータスデータ取得（RAPID解析対象）が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 処理ステータスデータ取得（RAPID解析対象）に失敗しました。', app_id, app_name)
            return result, conn, cur, all_processed_num, all_ng_num

        if len(records) > 0:
            logger.debug('[%s:%s] RAPID解析対象が存在します。', app_id, app_name)
        else:
            logger.debug('[%s:%s] RAPID解析対象が存在しません。', app_id, app_name)
            result = True
            return result, conn, cur, all_processed_num, all_ng_num

        for i in range(len(records)):
            start_time = datetime.datetime.now()
            processing_id = records[i][0]
            ### 処理ステータス更新
            logger.debug('[%s:%s] 処理ステータステーブル更新を開始します。', app_id, app_name)
            tmp_result, conn, cur = update_processing_status(start_time, processing_status_rapid_start, fabric_name,
                                                             inspection_num, processing_id, rapid_server_host_name, cur, conn)

            if tmp_result:
                logger.debug('[%s:%s] 処理ステータステーブルの更新が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 処理ステータステーブルの更新に失敗しました。', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            ### 検知対象画像特定
            ## 検知対象画像を特定する。
            # ファイル名パターンは以下を想定する。
            # 「*(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)*.jpg」
            #image_file_name_pattern = '\\*' + fabric_name + '_' + today_date + '_' + str(inspection_num).zfill((2)) + image_file_pattern
            image_file_name_pattern = '\\*'

            # 参照先パス構成は以下を想定する。
            # 「(品名)_(反番)_(撮像開始時刻:YYYYMMDD形式)_(検査番号)」
            #                 ｜-(処理ID１)
            #                 ｜-(処理ID２)
            #                 ｜-(処理ID３)
            image_path = image_dir + '\\' + product_name + '_' + fabric_name + '_' + today_date + '_' + \
                         str(inspection_num).zfill((2)) + '\\' + str(processing_id)

            logger.debug('[%s:%s] 検知対象画像ファイルの確認を開始します。', app_id, app_name)
            tmp_result, image_files = get_file(image_path, image_file_name_pattern)

            if tmp_result:
                logger.debug('[%s:%s] 検知対象画像ファイルの確認が完了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 検知対象画像ファイルの確認に失敗しました。', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            logger.debug('[%s:%s] 検知対象画像ファイル:[%s]', app_id, app_name, image_files)

            # RAPID解析対象の撮像画像を特定（解析実行パラメータ生成）する。
            rapid_images = []
            # 検知対象画像のファイルパス（フルパス）から、相対パス形式を作る。
            for path in image_files:
                rapid_images.append(path.replace(image_dir, '\\'))

            # 画像をワーカー数で等分する
            rapid_image = []
            for i in range(0, worker):
                rapid_image.append([])
            for num in range(0, len(rapid_images)):
                rapid_image[int(num / (len(rapid_images) / worker))].append(rapid_images[num])

            # IMA 常駐プロセスに対するリクエスト（JSONフォーマット）を作成
            ### リクエストJSON作成
            request_jsons = []

            for i in range(0, worker):
                request_json = json.dumps(
                    {"mode": mode, "image": rapid_image[i], "processor": processor, "model": ai_model_name,
                     "preprocess": preprocess, "summary": summary}, ensure_ascii=False)
                # IMA 常駐プロセスに対するリクエストの末尾に必ず改行記号"\n"をつける
                request_json = request_json + "\n"
                request_jsons.append(request_json)

                ## RAPID解析にリクエストとして送るJSONを作成する。
                # ファイル名は以下を想定する。
                # 「(撮像開始時刻:YYYYMMDD形式)_(品名)_(反版)_(検査番号)_(処理ID)_(worker番号（1からの連番）)_request.txt」
                request_json_file_name = today_date + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num).zfill((2)) + '_' + str(processing_id) + '_' + str(i+1) + '_request.txt'
                logger.debug('[%s:%s] request_json_file_name:[%s]', app_id, app_name, request_json_file_name)

                # 参照先パス構成は以下を想定する。
                # 「(撮像開始時刻:YYYYMMDD形式)_(品名)_(反番)_(検査番号)_JSON」
                request_json_file_path = rk_root_path + '\\JSON\\' + today_date + '_' + product_name + '_' + \
                                         fabric_name + '_' + str(inspection_num).zfill((2)) + "\\" + rapid_server_host_name \
                                         + "\\" + str(processing_id)
                logger.debug('[%s:%s] request_json_file_path:[%s]', app_id, app_name, request_json_file_path)

                # 格納先フォルダを作成する
                logger.debug('[%s:%s] JSONファイル格納フォルダ作成を開始します。', app_id, app_name)
                tmp_result = exists_dir(request_json_file_path)

                if tmp_result:
                    logger.debug('[%s:%s] JSONファイル格納フォルダ作成が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] JSONファイル格納フォルダ作成に失敗しました。', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

                # JSONファイル作成
                logger.debug('[%s:%s] リクエストJSONファイル作成を開始します。:[%s]', app_id, app_name,
                             request_json_file_path + '\\' + request_json_file_name)
                ofp = codecs.open(request_json_file_path + '\\' + request_json_file_name, 'w', 'utf-8')
                ofp.write(request_json)
                ofp.close()
                logger.debug('[%s:%s] リクエストJSONファイル作成が完了しました。:[%s]', app_id, app_name,
                             request_json_file_path + '\\' + request_json_file_name)

            # 並列化
            # ワーカー数分のプロセスを用意
            pool = multi.Pool(worker)

            # 引数の準備
            process_args = [
                [sock[i], request_jsons[i], rapid_server_host_name, port_num[i], int(rapid_connect_num)]
                for i in
                range(0, worker)]

            ### RAPID解析実行
            logger.info('[%s:%s] RAPID解析実行を開始します。ホスト名=[%s]', app_id, app_name, rapid_server_host_name)
            received_data = pool.map(wrapper_process, process_args)
            pool.close()

            # 処理結果確認
            received_str = []
            for i in range(0, worker):
                tmp_result = received_data[i][0]
                logger.info('[%s:%s] received_data:[%s]', app_id, app_name, tmp_result)
                if tmp_result:
                    # 正常終了時
                    if received_data[i][1] is None or received_data[i][1] == '':
                        logger.error('[%s:%s] RAPID解析結果の取得に失敗しました。', app_id, app_name)
                        conn.rollback()
                        return result, conn, cur, all_processed_num, all_ng_num
                    else:
                        # 子スレッドからの結果を変数に設定
                        received_str.append(received_data[i][1])
                        sock[i] = received_data[i][2]

                else:
                    # 異常終了時
                    logger.error('[%s:%s] RAPID解析実行に失敗しました。', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

            end_time = datetime.datetime.now()

            # 受け取った解析結果をテキストファイルとして保存する。
            for i in range(0, worker):
                # ファイル名は以下を想定する。
                # 「(撮像開始時刻:YYYYMMDD形式)_(品名)_(反版)_(検査番号)_(処理ID)_(worker番号（1からの連番）)_response.txt」
                response_json_file_name = today_date + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num).zfill((2)) + '_' + \
                                          str(processing_id) + '_' + str(i+1) + '_response.txt'
                logger.debug('[%s:%s] response_json_file_name:[%s]', app_id, app_name, response_json_file_name)

                # 参照先パス構成は以下を想定する。
                # リクエストJSONファイル格納と同じパス
                request_json_file_path = rk_root_path + '\\JSON\\' + today_date + '_' + product_name + '_' + \
                                         fabric_name + '_' + str(inspection_num).zfill((2)) + "\\" + rapid_server_host_name \
                                         + "\\" + str(processing_id)

                response_json_file_path = request_json_file_path
                logger.debug('[%s:%s] response_json_file_path:[%s]', app_id, app_name, response_json_file_path)

                # JSONファイル作成
                logger.debug('[%s:%s] レスポンスJSONファイルの作成を開始します。:[%s]', app_id, app_name,
                             response_json_file_path + '\\' + response_json_file_name)

                ofp = codecs.open(response_json_file_path + '\\' + response_json_file_name, 'w', 'utf-8')
                ofp.write(received_str[i])
                ofp.close()
                logger.debug('[%s:%s] レスポンスJSONファイルの作成が完了しました。:[%s]', app_id, app_name,
                             response_json_file_path + '\\' + response_json_file_name)

#############################
#                continue
#############################

                ### NG結果登録
                # RAPID解析の結果を確認して閾値を超えている場合、RAPID解析情報テーブルに登録する。
                tmp_result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list \
                    = ng_result_register(received_str[i], product_name, fabric_name, inspection_num, processing_id,
                                         start_time, end_time, rapid_server_host_name, image_dir, conn, cur)
                all_ng_num += rapid_ng_num
                all_processed_num += rapid_processed_num

                if tmp_result:
                    logger.debug('[%s:%s] NG結果登録が完了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG結果登録に失敗しました。', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

                ### NG画像コピー
                # RAPID解析結果でNG画像が存在するか
                if len(ng_file_relative_path_list) > 0:
                    # NG画像が存在する場合
                    # コピー先パス構成は以下を想定する。
                    # 「(撮像開始時刻:YYYYMMDD形式)_(品名)_(反番)_(NG)」
                    #                 ｜-(処理ID１)
                    #                 ｜-(処理ID２)
                    #                 ｜-(処理ID３)
                    copy_path = rk_root_path + '\\' + ng_dir + "\\" + today_date + '_' + product_name + '_' + \
                                fabric_name + '_' + str(inspection_num).zfill((2)) + "\\"

                    # コピー先フォルダを作成する
                    logger.debug('[%s:%s] NG画像ファイルのコピー先フォルダ作成を開始します。:コピー先フォルダ[%s]', app_id, app_name, copy_path)
                    tmp_result = exists_dir(copy_path)

                    if tmp_result:
                        logger.debug('[%s:%s] NG画像ファイルのコピー先フォルダ作成が完了しました。:コピー先フォルダ[%s]', app_id, app_name, copy_path)
                    else:
                        logger.error('[%s:%s] NG画像ファイルのコピー先フォルダ作成に失敗しました。:コピー先フォルダ[%s]', app_id, app_name, copy_path)
                        conn.rollback()
                        return result, conn, cur, all_processed_num, all_ng_num

                    for i in range(len(ng_file_relative_path_list)):
                        logger.debug('[%s:%s] NG画像コピーを開始します。 ', app_id, app_name)
                        # NG画像ファイルのパスは相対なので、絶対パスに置換する。
                        ng_file_relative_path = ng_file_relative_path_list[i]
                        ng_file_basename = os.path.basename(ng_file_relative_path)
                        ng_file_path = image_path + '\\' + ng_file_basename

                        tmp_result = copy_file(ng_file_path, copy_path)

                        if tmp_result:
                            logger.debug('[%s:%s] NG画像コピーが完了しました。 コピー先フォルダ[%s], NG画像ファイル名[%s]',
                                         app_id, app_name, copy_path, ng_file_path)
                        else:
                            logger.error('[%s:%s] NG画像コピーに失敗しました。 コピー先フォルダ[%s], NG画像ファイル名[%s]',
                                         app_id, app_name, copy_path, ng_file_path)
                            conn.rollback()
                            return result, conn, cur, all_processed_num, all_ng_num

            ### 処理ステータス更新
            logger.debug('[%s:%s] 処理ステータス更新を開始します。', app_id, app_name)
            end_time = datetime.datetime.now()
            tmp_result, conn, cur = \
                update_processing_status_rapid_end(all_ng_num,all_processed_num,fabric_name, inspection_num,
                                                   processing_id,rapid_server_host_name, end_time,cur, conn)

            if tmp_result:
                logger.debug('[%s:%s] 処理ステータス更新が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 処理ステータス更新に失敗しました。', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            conn.commit()

            logger.debug('[%s:%s] 処理済枚数確認を開始します。', app_id, app_name)
            if all_processed_num >= image_num_threshold:
                logger.debug('[%s:%s] 処理済枚数確認が終了しました。画像枚数閾値を超えています。', app_id, app_name)
                logger.debug('[%s:%s] 大量検知判定を開始します。', app_id, app_name)
                # 処理結果を確認する
                ng_rate = all_ng_num / all_processed_num
                if all_processed_num >= image_num_threshold and ng_rate >= ng_threshold:
                    # 処理済枚数=[画像枚数閾値] 且つ NG件数が処理済枚数の[NG率閾値]以上の場合
                    # 大量検知として異常終了
                    logger.warning('[%s:%s] 大量検知です。:処理済枚数[%s] NG件数[%s] NG率[%s] ', app_id, app_name,
                                   all_processed_num, all_ng_num, ng_rate)
                    result = True
                    return result, conn, cur, all_processed_num, all_ng_num

                else:
                    logger.debug('[%s:%s] 大量検知ではありません。', app_id, app_name)
                    logger.info('[%s:%s] RAPID解析のマルチスレッド実行を終了します。[%s]', app_id, app_name, rapid_server_host_name)
                    result = True
                    #return result, conn, cur, all_processed_num, all_ng_num
            else:

                logger.info('[%s:%s] RAPID解析のマルチスレッド実行を終了します。[%s]', app_id, app_name, rapid_server_host_name)
                result = True
                #return result, conn, cur, all_processed_num, all_ng_num

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    logger.debug('[%s:%s] exec_rapid_analysis_multi_thread end'
                 '[rapid_server_host_name:%s][worker:%s][port_num:%s]'
                 '[product_name:%s][fabric_name:%s][inspection_num:%s][imaging_starttime:%s]',
                 app_id, app_name,
                 rapid_server_host_name, worker, port_num,
                 product_name, fabric_name, inspection_num, imaging_starttime)

    return result, conn, cur, all_processed_num, all_ng_num


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報取得（分割/リサイズ完了時刻）
#
# 処理概要           ：1.処理ステータステーブルから分割/リサイズ完了時刻を取得する。
#
# 引数               ：コネクションオブジェクト
#                     カーソルオブジェクト
#                     版番
#                     検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    処理ステータス情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fabric_info_separateresize_endtime(conn, cur, fabric_name, inspection_num):
    ### クエリを作成する
    sql = 'select separateresize_endtime from fabric_info' \
          ' where fabric_name = \'%s\' and inspection_num = %s' \
          % (fabric_name, inspection_num)

    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    return db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス情報取得（RAPID解析処理完了）
#
# 処理概要           ：1.処理ステータステーブルから情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                     カーソルオブジェクト
#                     ステータス
#                     版番
#                     検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    処理ステータス情報
#                    コネクションオブジェクト
#                    カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_processing_status_rapid_end(conn, cur, status, fabric_name, inspection_num):
    ### クエリを作成する
    sql = 'select ' \
          'count(*) as record_count ,' \
          'count(case when rapid_endtime is not null then 1 else null end) as rapid_end_count' \
          ' from processing_status' \
          ' where fabric_name = \'%s\' and inspection_num = \'%s\' group by fabric_name, inspection_num' \
          % (fabric_name, inspection_num)

    # DB共通処理を呼び出して、処理ステータステーブルからデータを取得する。

    return db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)


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
    # DB接続を切断する。
    res = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return res


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.撮像枚数登録を行う。
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main():
    error_file_name = None
    conn = None
    cur = None
    try:
        ### 変数定義
        # コネクションオブジェクト, カーソルオブジェクト

        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # スレッド数
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        ### RAPIDサーバ接続情報
        # ipアドレス
        ip_addresses = common_inifile.get('RAPID_SERVER', 'ip_address')
        # port番号
        port_numbers = common_inifile.get('RAPID_SERVER', 'port_number')
        # worker数
        worker = int(common_inifile.get('RAPID_SERVER', 'worker'))

        # NG率閾値
        ng_threshold = float(inifile.get('VALUE', 'ng_threshold'))
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # 処理ステータステーブル.処理ステータス：リサイズ完了
        processing_status_resize_end = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        # 反物情報テーブル.処理ステータス：RAPID解析開始
        fabric_info_status_rapid_start = common_inifile.get('FABRIC_STATUS', 'rapid_start')
        # 反物情報テーブル.処理ステータス：RAPID解析完了
        fabric_info_status_rapid_end = common_inifile.get('FABRIC_STATUS', 'rapid_end')
        # 処理ステータステーブル.処理ステータス：RAPID解析完了
        processing_status_rapid_end = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
        # 反物情報テーブル.処理ステータス：RAPID解析開始時刻
        fabric_info_column_rapid_start = inifile.get('COLUMN', 'fabric_rapid_start')
        # 反物情報テーブル.処理ステータス：RAPID解析完了時刻
        fabric_info_column_rapid_end = inifile.get('COLUMN', 'fabric_rapid_end')

        # RAPIDサーバ接続リトライ回数
        rapid_connect_retry_num = int(inifile.get('ERROR_RETRY', 'rapid_connect_num'))
        # 処理完了判定エラーリトライ回数
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))

        # NG件数初期値
        all_ng_num = 0
        # 処理済枚数初期値
        all_processed_num = 0

        # 処理済枚数をマルチスレッド用に配列化
        multi_processed_num = [all_processed_num for i in range(thread_num)]
        # NG件数をマルチスレッド用に配列化
        multi_ng_num = [all_ng_num for i in range(thread_num)]

        # エラー時継続変数
        confirm_error_flag = 0

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### 常駐プロセス接続
        ip_address = ip_addresses.split(',')
        result, sock = get_rapid_server_connect_info(ip_address, port_numbers, worker, rapid_connect_retry_num)
        if result:
            logger.debug('[%s:%s] RAPID常駐プロセス接続が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] RAPID常駐プロセス接続でエラーが発生しました。', app_id, app_name)
            sys.exit()
        while True:
            # DB共通機能を呼び出して、DBに接続する。
            logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
                sys.exit()

            ### 反物情報取得（DBポーリング）
            while True:
                # 処理ステータステーブルから反物情報を取得する。
                logger.debug('[%s:%s] 反物情報取得（DBポーリング）を開始します。', app_id, app_name)
                result, target_record, conn, cur = \
                    select_fabric_info_db_polling(conn, cur,
                                                  processing_status_resize_end)

                if result:
                    logger.debug('[%s:%s] 反物情報取得（DBポーリング）が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 反物情報取得（DBポーリング）に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # 取得結果から、該当レコードが存在するか確認する。
                if target_record is not None and len(target_record) > 0:
                    # 該当レコードが存在する場合
                    # 以降の処理を続ける。
                    pass
                else:
                    # 該当レコードが無い場合
                    # 一定時間スリープした後、反物情報取得を実行する。
                    logger.info('[%s:%s] 処理対象の反物情報が存在しません。', app_id, app_name)
                    time.sleep(sleep_time)
                    continue

                product_name = target_record[0]
                fabric_name = target_record[1]
                inspection_num = target_record[2]
                start_time = datetime.datetime.now()

                logger.info('[%s:%s] %s処理を開始します。:[反番,検査番号]=[%s,%s]',
                            app_id, app_name, app_name, fabric_name, inspection_num)

                logger.debug('[%s:%s] 品番登録情報テーブルからAIモデル名の取得を開始します。', app_id, app_name)
                result, ai_model_name, conn, cur = select_matser_info(conn, cur, product_name)

                if result:
                    logger.debug('[%s:%s] 品番登録情報テーブルからAIモデル名の取得が終了しました。', app_id, app_name)
                else:
                    logger.debug('[%s:%s] 品番登録情報テーブルからAIモデル名の取得が終了しました。', app_id, app_name)
                    sys.exit()

                logger.debug('[%s:%s] 反物情報テーブルの更新（RAPID解析開始）を開始します。', app_id, app_name)
                result, conn, cur = update_fabric_info(fabric_info_column_rapid_start, start_time,
                                                       fabric_info_status_rapid_start, fabric_name, inspection_num,
                                                       cur, conn)
                if result:
                    logger.debug('[%s:%s] 反物情報テーブルの更新（RAPID解析開始）が終了しました。', app_id, app_name)
                    conn.commit()
                else:
                    logger.error('[%s:%s] 反物情報テーブルの更新（RAPID解析開始）に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                ### RAPID解析並列実行
                # 反物情報取得で取得した情報を基に、RAPID解析を並列実行させる。
                # 並列実行のための引数を作成する。
                tmp_ip_address = ip_address
                tmp_port_numbers = port_numbers.split(',')

                multi_param_rapid_server_host_name = []
                multi_param_worker = worker
                multi_param_port_num = []
                multi_param_product_name = []
                multi_param_fabric_name = []
                multi_param_inspection_num = []
                multi_param_rapid_starttime = []

                for i in range(len(tmp_ip_address)):
                    # マルチスレッド用の引数を設定
                    multi_param_rapid_server_host_name.append(tmp_ip_address[i])
                    multi_param_port_num.append(tmp_port_numbers)
                    multi_param_product_name.append(target_record[0])
                    multi_param_fabric_name.append(target_record[1])
                    multi_param_inspection_num.append(target_record[2])
                    multi_param_rapid_starttime.append(target_record[3])

                # 処理結果リスト
                result_list = []
                conn_status = str(conn)

                # 引数を基に、子スレッドを作成して、[処理ID取得]から[NG画像コピー]を、処理ID単位で並列実行する。
                logger.debug('[%s:%s] マルチスレッドを開始します。', app_id, app_name)

                # マルチスレッド実行
                with ThreadPoolExecutor() as executor:
                    func_list = []
                    for i in range(thread_num):
                        # スレッド実行
                        func_list.append(
                            executor.submit(
                                exec_rapid_analysis_multi_thread,
                                multi_param_rapid_server_host_name[i], multi_param_worker,
                                multi_param_port_num[i],
                                multi_param_product_name[i], multi_param_fabric_name[i], multi_param_inspection_num[i],
                                multi_param_rapid_starttime[i],
                                sock[i], ai_model_name[0],
                                multi_processed_num[i], multi_ng_num[i]))
                        time.sleep(1)
                    for i in range(thread_num):
                        # スレッド戻り値を取得
                        result_list.append(func_list[i].result())

                logger.info('[%s:%s] マルチスレッドが終了しました。', app_id, app_name)

                ### 子スレッドの処理結果判定
                # 各スレッドの結果を取得
                logger.debug('[%s:%s] result_list: %s', app_id, app_name, result_list)
                for i, multi_result in enumerate(result_list):
                    if multi_result[0] is True:
                        logger.debug('[%s:%s] multi_result【マルチスレッド[%d]】:正常終了しました。[%s]', app_id, app_name, i, str(multi_result))
                        multi_processed_num[i] = multi_result[3]
                        multi_ng_num[i] = multi_result[4]
                        close_connection(multi_result[1], multi_result[2])

                    else:
                        # 異常終了しているものが存在する場合
                        logger.error('[%s:%s] multi_result【マルチスレッド[%d]】:異常終了しました。[%s]', app_id, app_name, i, str(multi_result))
                        conn.rollback()
                        sys.exit()

                conn_str = str(conn)
                confirm_close = conn_str.split(',')[1]
                

                logger.debug('[%s:%s] 処理完了判定を開始します。', app_id, app_name)
                # DB共通処理を呼び出して、反番情報テーブルから検索条件でデータを取得する。
                logger.debug('[%s:%s] 反物情報取得（分割/リサイズ完了時刻）を開始します。', app_id, app_name)
                result, records_fabric, conn, cur = \
                    select_fabric_info_separateresize_endtime(conn, cur, fabric_name, inspection_num)

                if result:
                    logger.debug('[%s:%s] 反物情報取得（分割/リサイズ完了時刻）が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 反物情報取得（分割/リサイズ完了時刻）に失敗しました。', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                if records_fabric[0] is None:
                    logger.debug('[%s:%s] 処理単位分割、画像リサイズが終了していません。', app_id, app_name)
                    logger.debug('[%s:%s] 処理完了判定を終了しました。', app_id, app_name)
                    if str(0) in confirm_close:
                        logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                        close_connection(conn, cur)
                        logger.debug('[%s:%s] DB接続の切断が終了しました。' % (app_id, app_name))
                    else:
                        pass
                    time.sleep(sleep_time)
                    break

                else:
                    # DB共通処理を呼び出して、処理ステータステーブルから検索条件でデータを取得する。
                    logger.debug('[%s:%s] 処理ステータス情報取得（RAPID解析処理完了）を開始します。', app_id, app_name)
                    result, record_processing, conn, cur = \
                        select_processing_status_rapid_end(
                            conn, cur, processing_status_rapid_end, fabric_name, inspection_num)

                    if result:
                        logger.debug('[%s:%s] 処理ステータス情報取得（RAPID解析処理完了）が終了しました。', app_id, app_name)
                    else:
                        logger.error('[%s:%s] 処理ステータス情報取得（RAPID解析処理完了）に失敗しました。', app_id, app_name)
                        conn.rollback()
                        sys.exit()

                    # 取得結果判定
                    if record_processing[0] == record_processing[1]:
                        # 処理ステータス件数=該当検査番号のレコード数 且つ  分割/リサイズ完了時刻が入っている場合
                        # DB共通処理を呼び出して、反番情報テーブルを更新する。
                        logger.debug('[%s:%s] 反物情報テーブルの更新（RAPID解析完了）を開始します。', app_id, app_name)
                        result, conn, cur = update_fabric_info(fabric_info_column_rapid_end, start_time,
                                                               fabric_info_status_rapid_end, fabric_name,
                                                               inspection_num,
                                                               cur, conn)

                        if result:
                            logger.debug('[%s:%s] 反物情報テーブルの更新（RAPID解析完了）が終了しました。', app_id, app_name)
                        else:
                            logger.error('[%s:%s] 反物情報テーブルの更新（RAPID解析完了）に失敗しました。', app_id, app_name)
                            conn.rollback()
                            sys.exit()

                        confirm_error_flag = 0

                    else:
                        if confirm_error_flag == error_continue_num:
                            logger.error('[%s:%s] 処理完了判定エラー閾値を超過しました。', app_id, app_name)
                            sys.exit()
                        else:
                            confirm_error_flag += 1

                    logger.info('[%s:%s] %s処理が正常に終了しました。:[反番,検査番号]=[%s,%s]', app_id, app_name, app_name, fabric_name, inspection_num)
                    # コミットする
                    conn.commit()
                
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
