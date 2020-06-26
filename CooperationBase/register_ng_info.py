# -*- coding: SJIS -*-
# NG行列判定機能
#
import configparser
import sys
import datetime
import time
import logging.config
import traceback

import db_util
import error_detail
import error_util
import register_ng_info_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_ng_info.conf", disable_existing_loggers=False)
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


# ------------------------------------------------------------------------------------
# 処理名             ：処理ステータス情報取得（DBポーリング）
#
# 処理概要           ：1.処理ステータステーブルから反物情報を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理ステータステーブルのステータス
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      処理ステータス情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_processing_target(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'select ii.inspection_direction ' \
          'FROM fabric_info as fi inner join inspection_info_header as ii on ' \
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and fi.imaging_starttime = ' \
          'ii.start_datetime and ii.unit_num = \'%s\' ' \
          'where imageprocessing_starttime IS NOT NULL and ng_endtime IS NULL and status <> 0 and fi.fabric_name = \'%s\' and fi.inspection_num = %s and fi.imaging_starttime = \'%s\' ' \
          'order by fi.imaging_starttime asc, imageprocessing_starttime asc ' % (unit_num, fabric_name, inspection_num, imaging_starttime)

    logger.debug('[%s:%s] 処理ステータス情報取得SQL [%s]' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：端判定、マスキング判定、マーキング完了時刻取得
#
# 処理概要           ：1.反物情報テーブルから端判定、マスキング判定、マーキング完了時刻を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      端判定、マスキング判定、マーキング完了時刻
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_imageprocess_time(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'select imageprocessing_endtime FROM fabric_info ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 端判定、マスキング判定、マーキング完了時刻取得SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報ステータス更新
#
# 処理概要           ：1.反物情報テーブルのステータスを更新する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      反物情報テーブルのステータス
#                      更新カラム名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column_name, imaging_starttime, unit_num):
    update_time = datetime.datetime.now()

    ### クエリを作成する
    sql = 'update fabric_info set status = %s, %s = \'%s\' where fabric_name = \'%s\' and inspection_num  = %s and ' \
          'imaging_starttime = \'%s\' and unit_num = \'%s\' and %s IS NULL' \
          % (status, column_name, update_time, fabric_name, inspection_num, imaging_starttime, unit_num, column_name)

    logger.debug('[%s:%s] 反物情報ステータス更新SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、反物情報テーブルを更新する。
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：処理対象画像特定
#
# 処理概要           ：1.RAPID解析情報テーブルから処理対象画像を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      マスキング結果ステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG画像情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_processing_target_image(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### クエリを作成する
    sql = 'select num, ng_image, ng_point from \"rapid_%s_%s_%s\" where fabric_name = \'%s\' and inspection_num = %s ' \
          'and masking_result = %s and unit_num = \'%s\' and master_point IS NULL' \
          % (fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, status, unit_num)

    logger.debug('[%s:%s] 処理対象画像特定SQL [%s]' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、RAPID解析情報テーブルから処理対象画像情報を取得する。
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：反物情報マーキング完了時刻取得
#
# 処理概要           ：1.反物情報テーブルからマーキング完了時刻を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      マーキング完了時刻
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fabric_marking_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'select imageprocessing_endtime from fabric_info ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] 反物情報マーキング完了時刻取得SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、品種登録情報テーブルからマスタ情報を取得する。
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル更新
#
# 処理概要           ：1.RAPID解析情報テーブルから処理対象画像を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      NG座標
#                      ステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG画像情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_rapid_analysis(conn, cur, fabric_name, inspection_num, ng_file, status, inspection_date, unit_num):
    ng_image = ng_file[1]
    ng_point = ng_file[2]

    ### クエリを作成する
    sql = 'update \"rapid_%s_%s_%s\" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s and ng_image = \'%s\' and ng_point = \'%s\' ' \
          'and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, inspection_date, status, status, status, fabric_name, inspection_num,
             ng_image, ng_point, unit_num)

    logger.debug('[%s:%s] RAPID解析情報テーブル更新SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、RAPID解析情報テーブルを更新する。
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：NG行・列未登録NG画像数確認
#
# 処理概要           ：1.RAPID解析情報テーブルからNG行・列未登録NG画像数を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      解析結果ステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG行・列未登録NG画像数
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_judged_ng_info_count(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### クエリを作成する
    sql = 'select count(*) from \"rapid_%s_%s_%s\" where fabric_name = \'%s\' and inspection_num = %s ' \
          'and rapid_result = %s and edge_result = %s and masking_result = %s and unit_num = \'%s\' ' \
          'and master_point IS NULL ' \
          % (
              fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, status, status, status,
              unit_num)

    logger.debug('[%s:%s] NG行・列未登録NG画像数確認SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、RAPID解析情報テーブルから処理対象画像情報を取得する。
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：RAPID解析情報テーブル更新
#
# 処理概要           ：1.RAPID解析情報テーブルから処理対象画像を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      NG座標
#                      ステータス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      NG画像情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def update_rapid_analysis_all(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### クエリを作成する
    sql = 'update \"rapid_%s_%s_%s\" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\'' \
          % (
              fabric_name, inspection_num, inspection_date, status, status, status, fabric_name, inspection_num,
              unit_num)

    logger.debug('[%s:%s] RAPID解析情報テーブル更新SQL %s' % (app_id, app_name, sql))
    # DB共通処理を呼び出して、RAPID解析情報テーブルを更新する。
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


def main(product_name, fabric_name, inspection_num, imaging_starttime):
    # 変数定義
    # コネクションオブジェクト, カーソルオブジェクト
    conn = None
    cur = None
    # エラーファイル名
    error_file_name = None

    try:
        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # スリープ時間
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # ステータス(反物情報テーブル)　「8：NG行・列登録開始」
        fabric_ng_start_status = int(common_inifile.get('FABRIC_STATUS', 'ng_start'))
        # ステータス(反物情報テーブル)　「9：NG行・列登録完了」
        fabric_ng_end_status = int(common_inifile.get('FABRIC_STATUS', 'ng_end'))
        # 更新カラム名(反物情報テーブル)　「ng_starttime」
        fabric_ng_start_column = inifile.get('COLUMN', 'processing_ng_start_column')
        # 更新カラム名(反物情報テーブル)　「ng_endtime」
        fabric_ng_end_column = inifile.get('COLUMN', 'processing_ng_end_column')
        # ステータス(RAPID解析情報テーブル)　「2：NG」
        anarysis_ng_status = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))
        # ステータス(RAPID解析情報テーブル)　「4：対象行なし」
        anarysis_none_status = int(common_inifile.get('ANALYSIS_STATUS', 'none'))
        # 撮像画像の幅(mm)
        actual_image_width = int(common_inifile.get('IMAGE_SIZE', 'actual_image_width'))
        # 撮像画像の高さ(mm)
        actual_image_height = float(common_inifile.get('IMAGE_SIZE', 'actual_image_height'))
        # 撮像画像のオーバーラップ(mm)
        actual_image_overlap = int(common_inifile.get('IMAGE_SIZE', 'actual_image_overlap'))
        # 撮像画像(リサイズ画像)の幅(pix)
        resize_image_width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        # 撮像画像(リサイズ画像)の高さ(pix)
        resize_image_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))
        # 撮像画像(リサイズ画像)の幅(pix)
        master_image_width = int(common_inifile.get('IMAGE_SIZE', 'master_image_width'))
        # 撮像画像(リサイズ画像)の高さ(pix)
        master_image_height = int(common_inifile.get('IMAGE_SIZE', 'master_image_height'))
        # オーバーラップ分を除いた1撮像画像の幅(X軸)の長さ[pix]
        nonoverlap_image_width_pix = resize_image_width * (
                actual_image_width - actual_image_overlap) / actual_image_width
        # オーバーラップ分を除いた1撮像画像の高さ(Y軸)の長さ[pix]
        nonoverlap_image_height_pix = resize_image_height * (
                actual_image_height - actual_image_overlap) / actual_image_height
        # オーバーラップ分の幅(X軸)の長さ[pix]
        overlap_width_pix = resize_image_width * actual_image_overlap / actual_image_width
        # オーバーラップ分の高さ(Y軸)の長さ[pix]
        overlap_height_pix = resize_image_height * actual_image_overlap / actual_image_height
        # 検査対象ライン番号
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s機能を起動します' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
        # DBに接続する
        result, conn, cur = register_ng_info_util.create_connection(logger)

        if result:
            logger.debug('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
            pass
        else:
            logger.error('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
            sys.exit()
        
        count = 0

        while True:
            logger.debug('[%s:%s] 処理対象反物情報取得を開始します。' % (app_id, app_name))
            # 検査情報テーブルから検査情報を取得する
            result, fabric_info, conn, cur = select_processing_target(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num)

            if result:
                logger.debug('[%s:%s] 処理対象反物情報取得が終了しました。' % (app_id, app_name))
                logger.debug('[%s:%s] 処理対象反物情報 [%s]' % (app_id, app_name, fabric_info))
                pass
            else:
                logger.error('[%s:%s] 処理対象反物情報取得が失敗しました。' % (app_id, app_name))
                conn.rollback()
                sys.exit()

            if len(fabric_info) != 0:
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                            (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                inspection_direction = fabric_info[0][0]

                logger.debug('[%s:%s] レジマーク情報取得を開始します。' % (app_id, app_name))
                result, regimark_info, conn, cur = \
                    register_ng_info_util.select_regimark_info(conn, cur, fabric_name,
                                                               inspection_num, imaging_starttime, unit_num, logger)
                if result:
                    logger.debug('[%s:%s] レジマーク情報取得が終了しました。' % (app_id, app_name))
                    logger.debug('[%s:%s] レジマーク情報 [ %s ]' % (app_id, app_name, regimark_info))
                else:
                    logger.debug('[%s:%s] レジマーク情報取得が失敗しました。' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                while True:
                    if len(regimark_info) == 0:
                        logger.info('[%s:%s] レジマーク情報が存在しません。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                    (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        logger.debug('[%s:%s] 処理対象画像情報取得を開始します。' % (app_id, app_name))
                        result, ng_image_info, conn, cur = \
                            select_processing_target_image(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                           inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] 処理対象画像情報取得が終了しました。' % (app_id, app_name))
                        else:
                            logger.debug('[%s:%s] 処理対象画像情報取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] 後処理終了時刻取得を開始します。' % (app_id, app_name))
                        result, imageprocess_endtime, conn, cur = select_imageprocess_time(conn, cur, fabric_name,
                                                                                           inspection_num,
                                                                                           imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] 後処理終了時刻取得が終了しました。' % (app_id, app_name))
                            imageprocess_endtime = imageprocess_endtime[0]
                        else:
                            logger.debug('[%s:%s] 後処理終了時刻取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_image_info) != 0 and imageprocess_endtime is not None:
                            logger.info('[%s:%s] NG対象が存在します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                            if result:
                                logger.debug('[%s:%s] 反物情報ステータス更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] 反物情報ステータス更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            logger.debug('[%s:%s] RAPID解析情報テーブルのNG情報登録を開始します。' % (app_id, app_name))
                            # TODO 0行で登録する（rapid解析情報テーブル）（ほかの値も同じく）
                            # TODO 項目が同じで、画像は入れてあげる？
                            result, conn, cur = update_rapid_analysis_all(conn, cur, fabric_name, inspection_num,
                                                                          anarysis_none_status, inspection_date,
                                                                          unit_num)
                            if result:
                                logger.debug('[%s:%s] RAPID解析情報テーブルのNG情報登録が終了しました。' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] RAPID解析情報テーブルのNG情報登録が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                            logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] 反物情報ステータスの更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] 反物情報ステータスの更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                        elif len(ng_image_info) == 0 and imageprocess_endtime is not None:
                            logger.info('[%s:%s] NG対象とレジマーク情報が存在しません。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] 反物情報ステータス更新を開始します。' % (app_id, app_name))
                            # 検査情報テーブルから検査情報を取得する
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                            if result:
                                logger.debug('[%s:%s] 反物情報ステータス更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                                pass
                            else:
                                logger.error('[%s:%s] 反物情報ステータス更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                            logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] 反物情報ステータスの更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] 反物情報ステータスの更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                        else:
                            logger.debug('[%s:%s] 後処理終了時刻取得が存在しません。' % (app_id, app_name))
                            time.sleep(sleep_time)
                            continue

                    else:
                        logger.debug('[%s:%s] 反物情報ステータス更新を開始します。' % (app_id, app_name))
                        # 検査情報テーブルから検査情報を取得する
                        result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                               fabric_ng_start_status, fabric_ng_start_column,
                                                               imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] 反物情報ステータス更新が終了しました。' % (app_id, app_name))
                            conn.commit()
                            pass
                        else:
                            logger.error('[%s:%s] 反物情報ステータス更新が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] 処理対象画像情報取得を開始します。' % (app_id, app_name))
                        result, ng_image_info, conn, cur = \
                            select_processing_target_image(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                           inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] 処理対象画像情報取得が終了しました。' % (app_id, app_name))
                        else:
                            logger.debug('[%s:%s] 処理対象画像情報取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] 後処理終了時刻取得を開始します。' % (app_id, app_name))
                        result, imageprocess_endtime, conn, cur = select_imageprocess_time(conn, cur, fabric_name,
                                                                                           inspection_num,
                                                                                           imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] 後処理終了時刻取得が終了しました。' % (app_id, app_name))
                            imageprocess_endtime = imageprocess_endtime[0]
                        else:
                            logger.debug('[%s:%s] 後処理終了時刻取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_image_info) != 0:
                            logger.debug('[%s:%s] 処理対象画像情報 %s' % (app_id, app_name, ng_image_info))
                            logger.debug('[%s:%s] マスタ情報取得を開始します。' % (app_id, app_name))
                            result, mst_data, conn, cur = \
                                register_ng_info_util.select_product_master_info(conn, cur, product_name, logger)
                            if result:
                                logger.debug('[%s:%s] マスタ情報取得が終了しました。' % (app_id, app_name))
                                logger.debug('[%s:%s] マスタ情報情報 %s' % (app_id, app_name, mst_data))
                            else:
                                logger.debug('[%s:%s] マスタ情報取得が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            for j in range(len(ng_image_info)):
                                logger.debug('[%s:%s] 行番号特定を開始します。NG情報 %s' % (app_id, app_name, ng_image_info[j]))
                                result, line_info = \
                                    register_ng_info_util.specific_line_num(regimark_info, ng_image_info[j],
                                                                            inspection_direction, logger)

                                if result == 'error':
                                    logger.debug('[%s:%s] 該当行が存在しません。' % (app_id, app_name))
                                    result, conn, cur = \
                                        update_rapid_analysis(conn, cur, fabric_name, inspection_num, ng_image_info[j],
                                                              anarysis_none_status, inspection_date, unit_num)
                                    if result:
                                        logger.debug('[%s:%s] RAPID解析情報ステータス(none)を更新しました。' % (app_id, app_name))
                                        conn.commit()
                                        continue
                                    else:
                                        logger.debug('[%s:%s] RAPID解析情報ステータス(none)の更新が失敗しました。' % (app_id, app_name))
                                        conn.rollback()
                                        sys.exit()
                                elif result:
                                    logger.debug('[%s:%s] 行番号特定が終了しました。' % (app_id, app_name))
                                else:
                                    logger.debug('[%s:%s] 行番号特定が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] %s行目のレジマーク間長さ/幅比率算出を開始します。レジマーク情報 [%s]' % (
                                    app_id, app_name, line_info[0][0], line_info))
                                result, regimark_length_ratio, regimark_width_ratio, conf_regimark_between_length_pix = \
                                    register_ng_info_util.calc_length_ratio(regimark_info, line_info,
                                                                            nonoverlap_image_width_pix,
                                                                            nonoverlap_image_height_pix,
                                                                            overlap_width_pix, overlap_height_pix,
                                                                            resize_image_height, resize_image_width,
                                                                            mst_data, master_image_width,
                                                                            master_image_height,
                                                                            actual_image_width, actual_image_height,
                                                                            inspection_direction, logger)
                                if result:
                                    logger.debug('[%s:%s] レジマーク間長さ/幅比率算出が終了しました。' % (app_id, app_name))
                                    logger.debug('[%s:%s] レジマーク間長さ/幅比率 [長さ:%s 幅:%s]' % (
                                        app_id, app_name, regimark_length_ratio, regimark_width_ratio))
                                else:
                                    logger.debug('[%s:%s] レジマーク間長さ/幅比率算出が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG位置特定を開始します。' % (app_id, app_name))
                                result, length_on_master, width_on_master, ng_face = register_ng_info_util.specific_ng_point(
                                    regimark_info, line_info, ng_image_info[j], nonoverlap_image_width_pix,
                                    nonoverlap_image_height_pix, overlap_width_pix, overlap_height_pix,
                                    resize_image_height, resize_image_width, regimark_length_ratio,
                                    regimark_width_ratio,
                                    mst_data, inspection_direction, master_image_width, master_image_height, logger)
                                if result:
                                    logger.debug('[%s:%s] NG位置特定が終了しました。' % (app_id, app_name))
                                    logger.debug('[%s:%s] NG位置特定 X座標 = %s, Y座標 = %s' % (
                                        app_id, app_name, length_on_master, width_on_master))
                                else:
                                    logger.debug('[%s:%s] NG位置特定が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG行・列特定を開始します。' % (app_id, app_name))
                                result, judge_result, length_on_master, width_on_master = register_ng_info_util.specific_ng_line_colum(
                                    line_info, length_on_master, width_on_master, mst_data,
                                    conf_regimark_between_length_pix, inspection_direction, logger)

                                if result == True and judge_result == None:
                                    logger.debug('[%s:%s] NG行・列特定が終了しました。' % (app_id, app_name))
                                    logger.debug('[%s:%s] NG行・列境界値判定を開始します。' % (app_id, app_name))
                                    result, judge_result, length_on_master, width_on_master = register_ng_info_util.specific_ng_line_colum_border(
                                        line_info, length_on_master, width_on_master, mst_data,
                                        conf_regimark_between_length_pix, inspection_direction, logger)
                                    if result:
                                        logger.debug(
                                            '[%s:%s] NG行・列境界値判定が終了しました。[行,列] = %s' % (app_id, app_name, judge_result))
                                    else:
                                        logger.debug('[%s:%s] NG行・列境界値判定が失敗しました。' % (app_id, app_name))
                                        sys.exit()

                                elif result == True and judge_result != None:
                                    logger.debug('[%s:%s] NG行・列特定が終了しました。[行,列] = %s' % (app_id, app_name, judge_result))

                                else:
                                    logger.debug('[%s:%s] NG行・列特定が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] 基準点からのNG距離算出を開始します。' % (app_id, app_name))
                                result, ng_dist = register_ng_info_util.calc_distance_from_basepoint(
                                    length_on_master, width_on_master, judge_result, mst_data, master_image_width,
                                    master_image_height, logger)
                                if result:
                                    logger.debug('[%s:%s] 基準点からのNG距離算出が終了しました。' % (app_id, app_name))
                                    logger.debug('[%s:%s] 基準点からのNG距離 X方向:%s, Y方向:%s' % (
                                        app_id, app_name, ng_dist[0], ng_dist[1]))
                                else:
                                    logger.debug('[%s:%s] 基準点からのNG距離算出が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG情報登録を開始します。' % (app_id, app_name))
                                ng_line = judge_result[0]
                                ng_colum = judge_result[1]
                                master_point = str(round(length_on_master)) + ',' + str(round(width_on_master))
                                ng_distance_x = ng_dist[0]
                                ng_distance_y = ng_dist[1]
                                num = ng_image_info[j][0]
                                ng_file = ng_image_info[j][1]

                                result, conn, cur = register_ng_info_util.update_ng_info(
                                    conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point,
                                    ng_distance_x, ng_distance_y, num, ng_file, None, inspection_date, unit_num, logger)
                                if result:
                                    logger.debug('[%s:%s] NG情報登録が終了しました。' % (app_id, app_name))
                                    conn.commit()
                                else:
                                    logger.debug('[%s:%s] NG情報登録が失敗しました。' % (app_id, app_name))
                                    sys.exit()

                            del ng_image_info
                        elif len(ng_image_info) != 0 and imageprocess_endtime != None:
                            logger.info('[%s:%s] 処理対象画像が0件の検査です。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.info('[%s:%s] 全てNG画像のNG行・列登録が終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] 反物情報ステータスの更新を開始します。' % (app_id, app_name))
                            # 反物情報ステータスを取得する。
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] 反物情報ステータスの更新が終了しました。' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] 反物情報ステータスの更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                        else:
                            logger.info('[%s:%s] 処理対象画像がありません。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            time.sleep(10)

                        logger.debug('[%s:%s] 処理完了判定を開始します。' % (app_id, app_name))
                        logger.debug('[%s:%s] 反物情報マーキング完了時刻の取得を開始します。' % (app_id, app_name))
                        # 反物情報ステータスを取得する。
                        result, fabric_marikingendtime, conn, cur = \
                            select_fabric_marking_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime,
                                                          unit_num)
                        if result:
                            logger.debug('[%s:%s] 反物情報マーキング完了時刻の取得が終了しました。' % (app_id, app_name))
                        else:
                            logger.error('[%s:%s] 反物情報マーキング完了時刻の取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] NG行・列情報未登録件数の取得を開始します。' % (app_id, app_name))
                        # 端判定・マスキング判定結果を取得する
                        result, processed_count, conn, cur = \
                            select_judged_ng_info_count(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                        inspection_date, unit_num)
                        if result:
                            logger.debug('[%s:%s] NG行・列情報未登録件数の取得が終了しました。' % (app_id, app_name))
                        else:
                            logger.error('[%s:%s] NG行・列情報未登録件数の取得が失敗しました。' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if (fabric_marikingendtime[0] == None) or (
                                (fabric_marikingendtime[0] != None) and (processed_count[0] != 0)):
                            logger.info('[%s:%s] NG行・列未登録のNG画像が存在します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        else:
                            logger.info('[%s:%s] 全てNG画像のNG行・列登録が終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] 反物情報ステータスの更新を開始します。' % (app_id, app_name))
                            # 反物情報ステータスを取得する。
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] 反物情報ステータスの更新が終了しました。' % (app_id, app_name))
                                conn.commit()

                            else:
                                logger.error('[%s:%s] 反物情報ステータスの更新が失敗しました。' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            # 処理対象反物情報が存在しないため、DB接続を切り、一定時間スリープしてから、再取得を行う。
                            logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
                            result = register_ng_info_util.close_connection(conn, cur, logger)

                            if result:
                                logger.debug('[%s:%s] DB接続の切断が完了しました。' % (app_id, app_name))
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] DB接続の切断に失敗しました。' % (app_id, app_name))
                                sys.exit()

            else:
                logger.info('[%s:%s] 処理対象反物情報がありません。' % (app_id, app_name))
                logger.debug('[%s:%s] %s秒スリープします' % (app_id, app_name, sleep_time))
                time.sleep(sleep_time)
                if count == 5:
                    logger.debug('[%s:%s] 反物情報ステータス更新を開始します。' % (app_id, app_name))
                    # 検査情報テーブルから検査情報を取得する
                    result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                    if result:
                        logger.debug('[%s:%s] 反物情報ステータス更新が終了しました。' % (app_id, app_name))
                        conn.commit()
                        pass
                    else:
                        logger.error('[%s:%s] 反物情報ステータス更新が失敗しました。' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()
                    logger.debug('[%s:%s] 反物情報テーブルの更新を開始します。' % (app_id, app_name))
                    result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                    if result:
                        logger.debug('[%s:%s] 反物情報ステータスの更新が終了しました。' % (app_id, app_name))
                        conn.commit()
                        result = True
                        return result
                    else:
                        logger.error('[%s:%s] 反物情報ステータスの更新が失敗しました。' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()
                else:
                    count += 1
                    continue


    except SystemExit:
        # sys.exit()実行時の例外処理
        result = False
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行が終了しました。', app_id, app_name)

    except:
        result = False
        logger.error('[%s:%s] %s機能で予期しないエラーが発生しました。[%s]', app_id, app_name, app_name, traceback.format_exc())
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行が終了しました。', app_id, app_name)

    finally:
        if conn is not None:
            # DB接続済の際はクローズする
            logger.debug('[%s:%s] DB接続の切断を開始します。', app_id, app_name)
            register_ng_info_util.close_connection(conn, cur, logger)
            logger.debug('[%s:%s] DB接続の切断が終了しました。', app_id, app_name)
        else:
            # DB未接続の際は何もしない
            pass

    return result


if __name__ == "__main__":  # このパイソンファイル名で実行した場合

    main()
