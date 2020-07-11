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
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_ng_info_undetect")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_ng_info_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：検査情報ヘッダ取得
#
# 処理概要           ：1.検査情報ヘッダテーブルから検査方向設定を取得する。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      検査番号
#                      品名
#                      反番
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      処理ステータス情報
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, inspection_num, product_name, fabric_name, imaging_starttime, unit_num):
    ### クエリを作成する
    sql = 'select inspection_direction from ' \
          'inspection_info_header ' \
          'where inspection_num = %s and product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (inspection_num, product_name, fabric_name, imaging_starttime, unit_num)

    # DB共通処理を呼び出して、処理ステータステーブルと反物情報テーブルからデータを取得する。
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.NG行・列判定を行う。
#
# 引数               ：品名
#                    反番
#                    検査番号
#                    連番
#                    NG画像名
#                    NG座標
#                    未検知画像フラグ
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, num, ng_image_file_name, ng_point,
         undetected_image_flag_is_undetected, imaging_starttime, unit_num):
    # 変数定義
    # 処理結果
    result = False
    # コネクションオブジェクト, カーソルオブジェクト
    conn = None
    cur = None
    # エラーファイル名
    error_file_name = None

    try:
        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
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

        logger.info('[%s:%s] %s機能を起動します' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] DB接続を開始します。' % (app_id, app_name))
        # DBに接続する
        result, error, conn, cur, func_name = register_ng_info_util.create_connection(logger)

        if result:
            logger.debug('[%s:%s] DB接続が終了しました。' % (app_id, app_name))
            pass
        else:
            logger.error('[%s:%s] DB接続が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] レジマーク情報取得を開始します。' % (app_id, app_name))
        result, regimark_info, error, conn, cur, func_name = register_ng_info_util.select_regimark_info(conn, cur, fabric_name,
                                                                                      inspection_num, imaging_starttime,
                                                                                      unit_num, logger)
        if result:
            logger.debug('[%s:%s] レジマーク情報取得が終了しました。' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] レジマーク情報取得が失敗しました。' % (app_id, app_name))
            conn.rollback()
            sys.exit()

        logger.debug('[%s:%s] 処理対象画像情報 %s' % (app_id, app_name, ng_image_file_name))
        logger.debug('[%s:%s] マスタ情報取得を開始します。' % (app_id, app_name))
        result, mst_data, error, conn, cur, func_name = register_ng_info_util.select_product_master_info(conn, cur, product_name, logger)
        if result:
            logger.debug('[%s:%s] マスタ情報取得が終了しました。' % (app_id, app_name))
            logger.debug('[%s:%s] マスタ情報情報 %s' % (app_id, app_name, mst_data))
        else:
            logger.debug('[%s:%s] マスタ情報取得が失敗しました。' % (app_id, app_name))
            conn.rollback()
            sys.exit()

        logger.debug('[%s:%s] 検査情報取得を開始します。' % (app_id, app_name))
        result, inspection_direction, conn, cur = select_inspection_info(conn, cur, inspection_num, product_name,
                                                                         fabric_name, imaging_starttime, unit_num)
        if result:
            logger.debug('[%s:%s] 検査情報取得が終了しました。' % (app_id, app_name))
            inspection_direction = inspection_direction[0]
        else:
            logger.debug('[%s:%s] 検査情報取得が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] 行番号特定を開始します。' % (app_id, app_name))

        ng_image_info = []
        ng_image_info.append(str(num))
        ng_image_info.append(ng_image_file_name)
        ng_image_info.append(ng_point)
        result, line_info, last_flag, error, func_name = register_ng_info_util.specific_line_num(regimark_info, ng_image_info, inspection_direction, logger)
        if result:
            logger.debug('[%s:%s] 行番号特定が終了しました。行情報 [%s]' % (app_id, app_name, line_info))
        else:
            logger.debug('[%s:%s] 行番号特定が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] レジマーク間長さ/幅比率算出を開始します。' % (app_id, app_name))
        result, regimark_length_ratio, conf_regimark_between_length_pix, error, func_name = \
            register_ng_info_util.calc_length_ratio(regimark_info, line_info, nonoverlap_image_height_pix,
                                                    overlap_height_pix, resize_image_height, 
                                                    mst_data, master_image_width, 
                                                    actual_image_height, inspection_direction, logger)
        if result:
            logger.debug('[%s:%s] レジマーク間長さ/幅比率算出が終了しました。' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] レジマーク間長さ/幅比率算出が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] NG位置特定を開始します。' % (app_id, app_name))
        result, length_on_master, width_on_master, ng_face, error, func_name = register_ng_info_util.specific_ng_point(line_info, ng_image_info, nonoverlap_image_width_pix,
            nonoverlap_image_height_pix, overlap_width_pix, overlap_height_pix,
            resize_image_height, resize_image_width, regimark_length_ratio,
            mst_data, inspection_direction, master_image_width, master_image_height, actual_image_width, actual_image_overlap, logger)
        if result:
            logger.debug('[%s:%s] NG位置特定が終了しました。' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] NG位置特定が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] NG行・列特定を開始します。' % (app_id, app_name))
        result, judge_result, length_on_master, width_on_master, error, func_name = register_ng_info_util.specific_ng_line_colum(
            line_info, length_on_master, width_on_master, mst_data,
            conf_regimark_between_length_pix, inspection_direction, last_flag, logger)
        if result == True and judge_result == None:
            logger.debug('[%s:%s] NG行・列特定が終了しました。' % (app_id, app_name))
            logger.debug('[%s:%s] NG行・列境界値判定を開始します。' % (app_id, app_name))
            result, judge_result, length_on_master, width_on_master, error, func_name = register_ng_info_util.specific_ng_line_colum_border(
                regimark_info, length_on_master, width_on_master, mst_data, conf_regimark_between_length_pix,
                inspection_direction, last_flag, logger)
            if result:
                logger.debug('[%s:%s] NG行・列境界値判定が終了しました。[行,列] = %s' % (app_id, app_name, judge_result))
            else:
                logger.debug('[%s:%s] NG行・列境界値判定が失敗しました。' % (app_id, app_name))
                sys.exit()

        elif result == True and judge_result != None:
            logger.debug('[%s:%s] NG行・列特定が終了しました。[行,列] = %s' % (app_id, app_name, judge_result))
        else:
            logger.debug('[%s:%s] NG行・列特定が失敗しました。' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] 基準点からのNG距離算出を開始します。' % (app_id, app_name))
        result, ng_dist, error, func_name = register_ng_info_util.calc_distance_from_basepoint(
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
        num = ng_image_info[0]
        ng_file = ng_image_info[1]
        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

        result, error, conn, cur, func_name = register_ng_info_util.update_ng_info(
            conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point,
            ng_distance_x, ng_distance_y, num, ng_file, undetected_image_flag_is_undetected, inspection_date, unit_num, logger)
        if result:
            logger.debug('[%s:%s] NG情報登録が終了しました。' % (app_id, app_name))
            conn.commit()
        else:
            logger.debug('[%s:%s] NG情報登録が失敗しました。' % (app_id, app_name))
            sys.exit()

        # 処理対象反物情報が存在しないため、DB接続を切り、一定時間スリープしてから、再取得を行う。
        logger.debug('[%s:%s] DB接続の切断を開始します。' % (app_id, app_name))
        result, error, func_name = register_ng_info_util.close_connection(conn, cur, logger)

        if result:
            logger.debug('[%s:%s] DB接続の切断が完了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] DB接続の切断に失敗しました。' % (app_id, app_name))
            sys.exit()

    except SystemExit:
        result = False
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        #error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行が終了しました。', app_id, app_name)

    except:
        result = False
        logger.error('[%s:%s] %s機能で予期しないエラーが発生しました。[%s]', app_id, app_name, app_name, traceback.format_exc())
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        #error_util.common_execute(error_file_name, logger, app_id, app_name)
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


if __name__ == "__main__":
    main()
