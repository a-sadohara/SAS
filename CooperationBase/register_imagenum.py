# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能302  撮像枚数登録
# ----------------------------------------

import configparser
from datetime import datetime
import logging.config
import os
import re
import sys
import time
import traceback

import error_detail
import error_util
import db_util
import file_util
import register_ng_info
import register_regimark_info
import check_image_num

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_imagenum.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_imagenum")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_imagenum_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

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
def get_file(file_path, file_name, network_path_error):
    result = False
    sorted_files = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        logger.debug('[%s:%s] 撮像完了通知ファイル格納フォルダパス=[%s]', app_id, app_name, file_path)
        # 共通関数で撮像完了通知格納フォルダ情報を取得する

        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result == True:
            # 成功時
            pass
        elif tmp_result == network_path_error:
            # 失敗時
            logger.debug("[%s:%s] 撮像完了通知格納フォルダにアクセス出来ません。", app_id, app_name)
            return tmp_result, sorted_files, error, func_name
        else:
            # 失敗時
            logger.error("[%s:%s] 撮像完了通知格納フォルダにアクセス出来ません。", app_id, app_name)
            return result, sorted_files, error, func_name

        # 取得したファイルパスをファイル更新日時でソートする（古い順に処理するため）
        file_names = []
        for files in file_list:
            file_names.append((os.path.getmtime(files), files))

        sorted_files = []
        for mtime, path in sorted(file_names):
            sorted_files.append(path)

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
# 引数               ：撮像完了通知ファイルのファイルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      反番
#                      検査番号
#                      撮像枚数
#                      撮像完了時刻
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    fabric_name = None
    inspection_num = None
    image_num = None
    imaging_endtime = None
    inspection_line = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        logger.debug('[%s:%s] 撮像完了通知ファイル=%s', app_id, app_name, file)
        # 撮像完了通知ファイルパスからファイル名を取得し、版番、検査番号を取得する
        # なお、ファイル名は「IC_品名_反番_検査番号_日付.CSV」を想定している
        basename = os.path.basename(file)
        file_name = re.split('[_.]', basename)

        # 撮像完了通知ファイルから、最終更新日付を取得する
        endtime = datetime.fromtimestamp(os.path.getctime(file))

        # 撮像完了通知ファイルから、項目を取得する
        with open(file) as f:
            notification = [s.split(',') for s in f.readlines()]
            fabric_name, inspection_num, imaging_endtime, image_num, inspection_line = \
                file_name[2], file_name[3], endtime, notification[1][2], notification[1][3].rstrip('\n')

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, fabric_name, inspection_num, image_num, imaging_endtime, inspection_line, error, func_name


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
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 関数名             ：撮像開始時刻取得
#
# 処理概要           ：1.反物情報テーブルの撮像開始日時を取得する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      号機
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, fabric_name, inspection_num, unit_num, inspection_date):
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    # sql = 'select imaging_starttime from fabric_info where fabric_name = \'%s\' and inspection_num = %s ' \
    #       'and unit_num = \'%s\' and imaging_endtime IS NULL and CAST(imaging_starttime AS DATE) = \'%s\' order by imaging_starttime asc' \
    #       % (fabric_name, inspection_num, unit_num, inspection_date)
    sql = 'select fi.imaging_starttime, fi.imaging_endtime, ii.inspection_direction, ii.inspection_start_line from fabric_info as fi, '\
          'inspection_info_header as ii where fi.fabric_name = \'%s\' and fi.inspection_num = %s ' \
          'and fi.unit_num = \'%s\' and CAST(fi.imaging_starttime AS DATE) = \'%s\' '\
          'and fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num '\
          'and fi.imaging_starttime = ii.start_datetime order by imaging_starttime asc' \
          % (fabric_name, inspection_num, unit_num, inspection_date)

    logger.debug('[%s:%s] 撮像開始時刻取得SQL [%s]', app_id, app_name, sql)
    ### 反物情報テーブルを更新
    result, imaging_starttime, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, imaging_starttime, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：検査情報登録
#
# 処理概要           ：1.反物情報テーブルの検査情報を更新する。
#
# 引数               ：撮像枚数
#                      撮像完了時刻
#                      反番
#                      検査番号
#                      コネクションオブジェクト
#                      カーソルオブジェクトステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info(update_db_status, image_num, imaging_endtime, fabric_name, inspection_num,
                       cur, conn, imaging_startime, unit_num):
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'update fabric_info set status = %s, image_num = %s, imaging_endtime = \'%s\'' \
          'where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\' ' \
          % (update_db_status, image_num, imaging_endtime, fabric_name, inspection_num, imaging_startime, unit_num)

    logger.debug('[%s:%s] 検査情報登録SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    logger.debug('[%s:%s] 反番[%s], 検査番号[%s]のレコードを更新しました。', app_id, app_name, fabric_name, inspection_num)
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# 処理名             ：検査情報登録
#
# 処理概要           ：1.反物情報テーブルの検査情報を更新する。
#
# 引数               ：撮像枚数
#                      撮像完了時刻
#                      反番
#                      検査番号
#                      コネクションオブジェクト
#                      カーソルオブジェクトステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_inspection_info_header(fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num, end_line, inspection_target_line):
    func_name = sys._getframe().f_code.co_name
    ### クエリを作成する
    sql = 'update inspection_info_header set inspection_end_line = %s, inspection_target_line = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s ' \
          'and start_datetime = \'%s\' and unit_num = \'%s\' ' \
          % (end_line, inspection_target_line, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 検査情報(最終行数)更新SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    logger.debug('[%s:%s] 反番[%s], 検査番号[%s], 行数[%s] を更新しました。', app_id, app_name, fabric_name, inspection_num, end_line)
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# 処理名             ：退避フォルダ存在チェック
#
# 処理概要           ：1.撮像完了通知ファイルを退避するフォルダが存在するかチェックする。
#                    2.フォルダが存在しない場合は作成する。
#
# 引数               ：退避フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：撮像完了通知ファイル退避
#
# 処理概要           ：1.撮像完了通知ファイルを、退避フォルダに移動させる。
#
# 引数               ：撮像完了通知ファイル
#                      退避フォルダ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    func_name = sys._getframe().f_code.co_name
    # ファイル移動
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

    return result, error, func_name


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
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result, func_name

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
def select_before_inspection_data(conn, cur, inspection_num, inspection_date, unit_num):
    inspection_date = inspection_date[0:4] + '/' + inspection_date[4:6] + '/' + inspection_date[6:8]
    # 検査番号が1の場合、現在日の検査番号-1の情報
    # 検査番号の場合、現在日-1の最新検査の情報
    ### クエリを作成する
    if inspection_num == '1':
        sql = 'select product_name, fabric_name, inspection_num, imaging_endtime, imaging_starttime from fabric_info where unit_num = \'%s\' ' \
              'and cast(imaging_starttime as date) < \'%s\' order by imaging_starttime desc ' % (unit_num, inspection_date)
    else:
        sql = 'select product_name, fabric_name, inspection_num, imaging_endtime, imaging_starttime from fabric_info where unit_num = \'%s\' ' \
              'and cast(imaging_starttime as date)  = \'%s\' and inspection_num = %s ' % (unit_num, inspection_date, int(inspection_num) - 1)

    logger.debug('[%s:%s] 前検査情報取得SQL %s' % (app_id, app_name, sql))

    ### 検査情報テーブルからデータ取得
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur

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
    # 変数定義
    error_file_name = None
    conn = None
    cur = None
    error = None
    func_name = None
    try:

        ### 設定ファイルからの値取得
        # 共通設定：各種通知ファイルが格納されるルートパス
        input_root_path = common_inifile.get('FILE_PATH', 'input_path')
        # 共通設定：各種通知ファイルを退避させるルートパス
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # 完了通知が格納されるフォルダパス
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path + '\\'
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # DB更新値：反物情報テーブル:ステータス：（更新完了）
        update_db_status = common_inifile.get('FABRIC_STATUS', 'imaging_end')
        # 撮像完了通知ファイル：退避ディレクトリパス
        backup_path = inifile.get('PATH', 'backup_path')
        backup_path = backup_root_path + '\\' + backup_path + '\\'
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # 撮影完了通知ファイル名パターン
        file_name_pattern = inifile.get('PATH', 'file_name_pattern')
        # 検査対象ライン番号
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')
        # ネットワークパスエラーメッセージ
        network_path_error = inifile.get('ERROR_INFO', 'networkpath_error')

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### 撮像完了通知フォルダを監視する
        while True:

            while True:
                logger.info('[%s:%s] 行間撮像枚数チェック機能を呼び出します。', app_id, app_name)
                result, scaninfo_file, flag, func_name = check_image_num.main()
                if result:
                    if flag == 'continue':
                        time.sleep(sleep_time)
                        continue
                    else:
                        file_name_pattern = scaninfo_file[0].split('\\')[-1]
                        break

            # フォルダ内に撮像完了通知ファイルが存在するか確認する
            logger.debug('[%s:%s] 撮像完了通知ファイルの確認を開始します。', app_id, app_name)
            result, sorted_files, error, func_name = get_file(file_path, file_name_pattern, network_path_error)

            if result == True:
                pass
            elif result == network_path_error:
                logger.warning('[%s:%s] 撮像完了通知ファイルにアクセスできません。', app_id, app_name)

                message = '撮像完了通知ファイルにアクセスできません。'
                error_util.write_eventlog_warning(app_name, message)

                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] 撮像完了通知ファイルの確認に失敗しました。', app_id, app_name)
                sys.exit()

            # 撮像完了通知ファイルがない場合は一定期間sleepして再取得
            if len(sorted_files) == 0:
                logger.info('[%s:%s] 撮像完了通知ファイルが存在しません。', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            logger.debug('[%s:%s] 撮像完了通知ファイルを発見しました。:撮像完了通知ファイル名[%s]',
                         app_id, app_name, sorted_files)

            sp_sorted_files = []
            for sp_file in sorted_files:
                print(sp_file)
                sp_sorted_files.append(re.split('[_.]', sp_file.split('\\')[-1]))

            print(sp_sorted_files)
            min_date = min([int(x[:][4]) for x in sp_sorted_files])
            print(min_date)
            min_inspection_num = min([int(x[:][3]) for x in sp_sorted_files])
            print(min_inspection_num)

            print(sorted_files)
            sorted_files = [x for x in sorted_files if '_' + str(min_inspection_num) + '_' in x and '_' + str(min_date) + '.CSV']
            print(sorted_files)
            # DB共通処理を呼び出して、DB接続を行う
            logger.debug('[%s:%s] DB接続を開始します。', app_id, app_name)
            result, error, conn, cur, func_name = create_connection()

            if result:
                logger.debug('[%s:%s] DB接続が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] DB接続時にエラーが発生しました。', app_id, app_name)
                sys.exit()

            for i in range(len(sorted_files)):
                basename = os.path.basename(sorted_files[i])
                file_name = re.split('[_.]', basename)
                product_name = file_name[1]
                fabric_name = file_name[2]
                inspection_num = file_name[3]
                inspection_date = file_name[4]
                logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号]=[%s, %s]', app_id, app_name, app_name, fabric_name,
                            inspection_num)

                logger.debug('[%s:%s] 前検査情報取得を開始します。' % (app_id, app_name))
                # 検査情報テーブルから前検査情報を取得する
                result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur, inspection_num,
                                                                                          inspection_date, unit_num)
                if result:
                    logger.debug('[%s:%s] 前検査情報取得が終了しました。' % (app_id, app_name))
                    if before_inspection_info is None:
                        pass
                    elif before_inspection_info[3] != None:
                        before_fabric_name = before_inspection_info[1]
                        before_inspection_num = before_inspection_info[2]
                        before_starttime = before_inspection_info[4]
                        before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                        logger.debug('[%s:%s] 前検査情報 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % (
                            app_id, app_name, before_fabric_name, before_inspection_num, before_inspection_date))
                    else:
                        before_fabric_name = before_inspection_info[1]
                        before_inspection_num = before_inspection_info[2]
                        before_starttime = before_inspection_info[4]
                        before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                        logger.info('[%s:%s] 前検査の検査完了時刻が存在しません。撮像完了通知取り込みを中止します。' % (app_id, app_name))
                        logger.info('[%s:%s] 前検査情報 [反番, 検査番号, 検査日付]=[%s, %s, %s]' % (
                        app_id, app_name, before_fabric_name, before_inspection_num, before_inspection_date))
                        time.sleep(sleep_time)
                        break

                # 撮像完了通知ファイルを読込む
                logger.debug('[%s:%s] 撮像完了通知ファイルの読込を開始します。', app_id, app_name)
                result, fabric_name, inspection_num, \
                image_num, imaging_endtime, inspection_line, error, func_name = read_file(sorted_files[i])

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイルの読込が終了しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルの読込に失敗しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # 撮像開始日時を取得する
                org_inspection_date = file_name[4]
                inspection_date = str(
                    org_inspection_date[:4] + '/' + org_inspection_date[4:6] + '/' + org_inspection_date[6:])

                logger.debug('[%s:%s] 撮像開始日時の取得を開始します。',
                             app_id, app_name)
                result, fabric_info, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, unit_num, inspection_date)

                if result:
                    logger.debug('[%s:%s] 撮像開始日時の取得が終了しました。', app_id, app_name)
                    logger.debug('[%s:%s] 検査対象 [%s]', app_id, app_name, fabric_info)
                    if fabric_info[1] is not None:
                        logger.info('[%s:%s] 既に撮像完了時刻を登録しています。', app_id, app_name)
                        ### 撮像完了通知ファイルを、別フォルダへ退避する。
                        # 撮像完了通知ファイルを退避するフォルダの存在を確認する
                        logger.debug('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックを開始します。', app_id, app_name)
                        result, error, func_name = exists_dir(backup_path)

                        if result:
                            logger.debug('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックが終了しました。', app_id, app_name)
                        else:
                            logger.error('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                         app_id, app_name, backup_path)
                            sys.exit()

                        # 撮像完了通知ファイルを、退避フォルダに移動させる。
                        logger.debug('[%s:%s] 撮像完了通知ファイル移動を開始します。:撮像完了通知ファイル名[%s]',
                                     app_id, app_name, sorted_files[i])
                        result, error, func_name = move_file(sorted_files[i], backup_path)

                        if result:
                            logger.debug('[%s:%s] 撮像完了通知ファイル移動が終了しました。:退避先フォルダ[%s], 撮像完了通知ファイル名[%s]',
                                         app_id, app_name, backup_path, sorted_files[i])
                            break
                        else:
                            logger.error('[%s:%s] 撮像完了通知ファイルの退避に失敗しました。:撮像完了通知ファイル名[%s]',
                                         app_id, app_name, sorted_files[i])
                            sys.exit()
                    else:
                        pass

                else:
                    logger.error('[%s:%s] 撮像開始日時の取得が失敗しました。',
                                 app_id, app_name)
                    sys.exit()

                if not fabric_info:
                    logger.error('[%s:%s] 対象のデータが登録されていません。 [反番, 検査番号]=[%s, %s]',
                                 app_id, app_name, fabric_name, inspection_num)
                    error = TypeError
                    continue
                else:
                    pass

                imaging_starttime = fabric_info[0]
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                inspection_direction = fabric_info[2]
                inspection_start_line = fabric_info[3]
                end_line = inspection_line

                if inspection_direction == 'S' or 'X':
                    inspection_target_line = int(end_line) - int(inspection_start_line) + 1
                else:
                    inspection_target_line = int(inspection_start_line) - int(end_line) + 1

                # 検査情報ヘッダーテーブルの最終行番を更新する。
                logger.debug('[%s:%s] 検査情報ヘッダーテーブルの最終行番の更新を開始します。', app_id, app_name)
                result, error, conn, cur, func_name = \
                    update_inspection_info_header(fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num,
                                                  end_line, inspection_target_line)
                if result:
                    logger.debug('[%s:%s] 検査情報ヘッダーテーブルの最終行番の更新が終了しました。',
                                 app_id, app_name)
                    logger.info('[%s:%s] 検査情報ヘッダーテーブルの最終行番の更新が終了しました。 [反番, 検査番号, 検査日付, 最終行番]=[%s, %s, %s, %s]',
                                app_id, app_name, fabric_name, inspection_num, inspection_date, end_line)
                else:
                    logger.error('[%s:%s] 検査情報ヘッダーテーブルの最終行番の更新に失敗しました。',
                                 app_id, app_name)
                    sys.exit()

                # 撮像完了通知ファイルの情報を、反物情報テーブルに登録する
                logger.debug('[%s:%s] 反物情報テーブル更新を開始します。', app_id, app_name)
                result, error, conn, cur, func_name = \
                    update_fabric_info(update_db_status, image_num, imaging_endtime, fabric_name, inspection_num,
                                       cur, conn, imaging_starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] 反物情報テーブルの更新が終了しました。 撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    logger.info('[%s:%s] 反物情報テーブルの更新が終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]',
                                app_id, app_name, fabric_name, inspection_num, inspection_date)
                else:
                    logger.error('[%s:%s] 反物情報テーブルの更新に失敗しました。 撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # コミットする
                conn.commit()

                ### 撮像完了通知ファイルを、別フォルダへ退避する。
                # 撮像完了通知ファイルを退避するフォルダの存在を確認する
                logger.debug('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックを開始します。', app_id, app_name)
                result, error, func_name = exists_dir(backup_path)

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックが終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                 app_id, app_name, backup_path)
                    sys.exit()

                # 撮像完了通知ファイルを、退避フォルダに移動させる。
                logger.debug('[%s:%s] 撮像完了通知ファイル移動を開始します。:撮像完了通知ファイル名[%s]',
                             app_id, app_name, sorted_files[i])
                result, error, func_name = move_file(sorted_files[i], backup_path)

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイル移動が終了しました。:退避先フォルダ[%s], 撮像完了通知ファイル名[%s]',
                                 app_id, app_name, backup_path, sorted_files[i])
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルの退避に失敗しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)

                logger.debug("[%s:%s] レジマーク情報登録機能呼出を開始します。", app_id, app_name)
                result, ai_model_flag, error, func_name = \
                    register_regimark_info.main(product_name, fabric_name, inspection_num, imaging_starttime)
                if result:
                    logger.debug("[%s:%s] レジマーク情報登録機能が終了しました。", app_id, app_name)
                else:
                    logger.error("[%s:%s] レジマーク情報登録機能が失敗しました。", app_id, app_name)
                    func_name = '303' + sys._getframe().f_code.co_name
                    sys.exit()

                # AIモデル未検査フラグを確認
                # 0の場合はNG行・列判定登録機能を呼び出す
                if ai_model_flag == 0 or ai_model_flag == None:

                    logger.debug('[%s:%s] NG行・列判定登録機能呼出を開始します。', app_id, app_name)
                    result, error, func_name = register_ng_info.main(product_name, fabric_name, inspection_num,
                                                                     imaging_starttime)
                    if result:
                        logger.debug('[%s:%s] NG行・列判定登録機能が終了しました。', app_id, app_name)

                    else:
                        logger.debug('[%s:%s] NG行・列判定登録機能が失敗しました。', app_id, app_name)
                        func_name = '309' + sys._getframe().f_code.co_name
                        sys.exit()
                else:
                    pass

            # DB接続を切断
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)

            # 次の監視間隔までスリープ
            time.sleep(sleep_time)

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

        logger.debug('[%s:%s] エラー詳細を取得します。' % (app_id, app_name))
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

    except:
        logger.error('[%s:%s] %s機能で予期しないエラーが発生しました。[%s]', app_id, app_name, app_name, traceback.format_exc())

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
