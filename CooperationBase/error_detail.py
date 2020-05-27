# -*- coding: SJIS -*-
# ----------------------------------------
# ■ エラー共通処理
# ----------------------------------------


import psycopg2
import configparser
import traceback

import db_util

# エラー設定ファイル読込
error_inifile = configparser.ConfigParser()
error_inifile.read('D:/CI/programs/config/error_config.ini', 'SJIS')
#  共通設定ファイル読込み
# 共通設定読込
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')


# ------------------------------------------------------------------------------------
# 処理名             ：DB例外
#
# 処理概要           ：1.DBに関する例外処理を行う。
#                       2.エラーや処理に応じた戻り値を返却する。
#
# 引数               ：エラー情報
#                       DB再接続回数
#                       ロガー
#                       機能ID
#                       機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def db_exception(error_type, retry_num, logger, app_id, app_name):

    try:
        if 'OperationalError' in error_type or 'InterfaceError' in error_type:
            for i in range(retry_num):
                try:
                    logger.warning('[%s:%s] 例外詳細 : DB接続エラーが発生しました。' % (app_id, app_name)),
                    logger.warning('[%s:%s] [DB接続再試行%s回目] DBとの接続を開始します。' % (app_id, app_name, i+1)),
                    conn, cur = db_util.base_create_connection()
                    logger.warning('[%s:%s] [DB接続再試行%s回目] DBとの接続が完了しました。' % (app_id, app_name, i+1))
                except psycopg2.Error:
                    logger.warning('[%s:%s] 再接続失敗。再接続を行います' % (app_id, app_name))
                else:
                    logger.warning('[%s:%s] 再接続成功' % (app_id, app_name))
                    return True, conn, cur
            else:
                logger.error('[%s:%s] 再接続失敗。接続設定を見直してください' % (app_id, app_name))
                logger.error(str(traceback.format_exc()))
                return False
        else:
            logger.error('[%s:%s] 例外詳細 : DBエラーが発生しました。' % (app_id, app_name))
            logger.error(str(traceback.format_exc()))
            return False
    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result


# ------------------------------------------------------------------------------------
# 処理名             ：システム例外
#
# 処理概要           ：1.システムに関する例外処理を行う。
#                       2.エラーや処理に応じた戻り値を返却する。
#
# 引数               ：エラー情報
#                       ネットワークパスエラー文
#                       パーミッションエラー文
#                       ロガー
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      パーミッションエラー文
# ------------------------------------------------------------------------------------
def system_exception(e, network_error_str, permission_error_str, logger, app_id, app_name):
    try:
        error_type = str(type(e))
        if network_error_str in str(e.args):
            logger.warning('[%s:%s] 例外詳細 : ネットワークパスが見つかりません。' % (app_id, app_name))
            logger.warning(str(traceback.format_exc()))
            return network_error_str

        elif permission_error_str in error_type:
            logger.warning('[%s:%s] 例外詳細 : プロセスはファイルにアクセスできません。別のプロセスが使用中です' % (app_id, app_name))
            logger.warning(str(traceback.format_exc()))
            return permission_error_str
        else:
            logger.error('[%s:%s] 例外詳細 : エラーが発生しました。' % (app_id, app_name))
            logger.error(str(traceback.format_exc()))
            return False
    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result


# ------------------------------------------------------------------------------------
# 処理名             ：例外判定
#
# 処理概要           ：1.各機能から連携されたエラー情報の詳細判定を行う。
#
# 引数               ：エラー情報
#                       ロガー
#                       機能ID
#                       機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def exception(e, logger, app_id, app_name):
    try:
        # 設定定義
        db_error = error_inifile.get('ERROR_INFO', 'db_error')
        network_error_str = error_inifile.get('ERROR_INFO', 'networkpath_error')
        permission_error_str = error_inifile.get('ERROR_INFO', 'permission_error')
        retry_num = int(common_inifile.get('ERROR_RETRY', 'connect_num'))
        error_type = str(type(e))

        if db_error in error_type:
            logger.error('[%s:%s] DB関連エラーが発生しました。' % (app_id, app_name))
            tmp_result = db_exception(error_type, retry_num, logger, app_id, app_name)
            if type(tmp_result) == tuple:
                result = tmp_result[0]
                conn = tmp_result[1]
                cur = tmp_result[2]
                return result, conn, cur
            else:
                result = False
                return result
        else:
            logger.error('[%s:%s] システム関連エラーが発生しました。' % (app_id, app_name))
            result = system_exception(e, network_error_str, permission_error_str, logger, app_id, app_name)
            return result
    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result
