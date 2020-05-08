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
import register_regimark_info

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

    try:
        logger.debug('[%s:%s] 撮像完了通知ファイル格納フォルダパス=[%s]', app_id, app_name, file_path)
        # 共通関数で撮像完了通知格納フォルダ情報を取得する

        tmp_result, file_list = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result == True:
            # 成功時
            pass
        elif tmp_result == network_path_error:
            # 失敗時
            logger.debug("[%s:%s] 撮像完了通知格納フォルダにアクセス出来ません。", app_id, app_name)
            return tmp_result, sorted_files
        else:
            # 失敗時
            logger.error("[%s:%s] 撮像完了通知格納フォルダにアクセス出来ません。", app_id, app_name)
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
            fabric_name, inspection_num, imaging_endtime, image_num = \
                file_name[2], file_name[3], endtime, notification[1][2].rstrip('\n')

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, fabric_name, inspection_num, image_num, imaging_endtime


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
    ### クエリを作成する
    sql = 'select imaging_starttime from fabric_info where fabric_name = \'%s\' and inspection_num = %s ' \
          'and unit_num = \'%s\' and imaging_endtime IS NULL and CAST(imaging_starttime AS DATE) = \'%s\' order by imaging_starttime asc' \
          % (fabric_name, inspection_num, unit_num, inspection_date)

    ### 反物情報テーブルを更新
    result, imaging_starttime, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, imaging_starttime, conn, cur


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
    ### クエリを作成する
    sql = 'update fabric_info set status = %s, image_num = %s, imaging_endtime = \'%s\'' \
          'where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\' ' \
          % (update_db_status, image_num, imaging_endtime, fabric_name, inspection_num, imaging_startime, unit_num)

    logger.debug('[%s:%s] 検査情報登録SQL %s' % (app_id, app_name, sql))
    ### 反物情報テーブルを更新
    logger.debug('[%s:%s] 反番[%s], 検査番号[%s]のレコードを更新しました。', app_id, app_name, fabric_name, inspection_num)
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


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
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


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
    # ファイル移動
    result = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

    return result


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
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result


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
            # フォルダ内に撮像完了通知ファイルが存在するか確認する
            logger.debug('[%s:%s] 撮像完了通知ファイルの確認を開始します。', app_id, app_name)
            result, sorted_files = get_file(file_path, file_name_pattern, network_path_error)

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

            # DB共通処理を呼び出して、DB接続を行う
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
                product_name = file_name[1]
                fabric_name = file_name[2]
                inspection_num = file_name[3]
                logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号]=[%s, %s]', app_id, app_name, app_name, fabric_name,
                            inspection_num)

                # 撮像完了通知ファイルを読込む
                logger.debug('[%s:%s] 撮像完了通知ファイルの読込を開始します。', app_id, app_name)
                result, fabric_name, inspection_num, \
                image_num, imaging_endtime = read_file(sorted_files[i])

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイルの読込が終了しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルの読込に失敗しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # 撮像開始日時を取得する
                org_inspection_date = file_name[4]
                inspection_date = str(org_inspection_date[:4] + '/' + org_inspection_date[4:6] + '/' + org_inspection_date[6:])

                logger.debug('[%s:%s] 撮像開始日時の取得を開始します。',
                             app_id, app_name)
                result, fabric_info, conn, cur = select_fabric_info(conn, cur, fabric_name, inspection_num, unit_num, inspection_date)

                if result:
                    logger.debug('[%s:%s] 撮像開始日時の取得が終了しました。',
                                 app_id, app_name)
                else:
                    logger.error('[%s:%s] 撮像開始日時の取得が失敗しました。',
                                 app_id, app_name)
                    sys.exit()

                imaging_starttime = fabric_info[0]
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                # 撮像完了通知ファイルの情報を、反物情報テーブルに登録する
                logger.debug('[%s:%s] 反物情報テーブル更新を開始します。', app_id, app_name)
                result, conn, cur = \
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
                result = exists_dir(backup_path)

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックが終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルを退避するフォルダ存在チェックに失敗しました。:退避先フォルダ名[%s]',
                                 app_id, app_name, backup_path)
                    sys.exit()

                # 撮像完了通知ファイルを、退避フォルダに移動させる。
                logger.debug('[%s:%s] 撮像完了通知ファイル移動を開始します。:撮像完了通知ファイル名[%s]',
                             app_id, app_name, sorted_files[i])
                result = move_file(sorted_files[i], backup_path)

                if result:
                    logger.debug('[%s:%s] 撮像完了通知ファイル移動が終了しました。:退避先フォルダ[%s], 撮像完了通知ファイル名[%s]',
                                 app_id, app_name, backup_path, sorted_files[i])
                else:
                    logger.error('[%s:%s] 撮像完了通知ファイルの退避に失敗しました。:撮像完了通知ファイル名[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)

                logger.debug("[%s:%s] レジマーク情報登録機能呼出を開始します。", app_id, app_name, app_name)
                result = register_regimark_info.main(product_name, fabric_name, inspection_num, imaging_starttime)
                if result:
                    logger.debug("[%s:%s] レジマーク情報登録機能が終了しました。", app_id, app_name, app_name)
                else:
                    logger.error("[%s:%s] レジマーク情報登録機能が失敗しました。", app_id, app_name, app_name)
                    sys.exit()

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
    main()
