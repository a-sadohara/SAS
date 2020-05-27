# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能310  NG画像圧縮・転送
# ----------------------------------------

import configparser
import logging.config
import sys
import traceback
import error_util
import file_util

import compress_image_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image_undetect")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

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
def exec_patrite(file_name , logger, app_id, app_name):
    result = file_util.light_patlite(file_name, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.NG画像圧縮・転送を行う。
#
# 引数               ：品名
#                    反番
#                    検査番号
#                    撮像開始時刻
#                    NG画像名
#                   圧縮対象NG画像パス（未検知画像フラグが"未検知画像"の場合のみ指定。それ以外はNoneを指定する）
#                   未検知画像フラグ（1:未検知画像である、それ以外:未検知画像でない）
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, imaging_starttime, image_path, target_ng_image_path):
    # 変数定義
    error_file_name = None
    result = False
    try:

        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # パトライト点灯のトリガーとなるファイルパス
        send_patrite_trigger_file_path = common_inifile.get('FILE_PATH', 'patlite_path')
        # パトライト点灯のトリガーとなるファイル名
        send_parrite_file_name = inifile.get('FILE', 'send_parrite_file_name')

        # プログラム判定値：未検知画像
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))


        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### NG画像圧縮、検査完了通知作成、ファイル転送

        logger.debug('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送を開始します。', app_id, app_name)
        tmp_result = compress_image_util.exec_compress_and_transfer(
        product_name, fabric_name, inspection_num, imaging_starttime,
        image_path, target_ng_image_path, undetected_image_flag_is_undetected, logger)

        if tmp_result:
            logger.debug('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] NG画像圧縮、検査完了通知作成、ファイル転送に失敗しました。', app_id, app_name)
            sys.exit()

        logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)
        result = True

    except SystemExit:
        # sys.exit()実行時の例外処理
        result = False
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        temp_result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if temp_result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

    except:
        result = False
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        temp_result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if temp_result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

    return result


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    main()