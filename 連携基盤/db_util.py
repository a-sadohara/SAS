# -*- coding: SJIS -*-
# ----------------------------------------
# ■   DB共通機能
# ----------------------------------------


import configparser
import psycopg2

import error_detail

#  共通設定ファイル読込み
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))


# ------------------------------------------------------------------------------------
# 処理名             ：DB接続基底処理
#
# 処理概要           ：1.DB接続処理を行う。
#
# 引数               ：なし
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def base_create_connection():

    inifile = configparser.ConfigParser()
    inifile.read('D:/CI/programs/config/db_config.ini', 'SJIS')

    dbname = inifile.get('DB_INFO', 'dbname')
    user = inifile.get('DB_INFO', 'username')
    password = inifile.get('DB_INFO', 'password')
    port = inifile.get('DB_INFO', 'portnumber')
    host = inifile.get('DB_INFO', 'hostname')
    timeout = inifile.get('DB_INFO', 'timeout')
    url = "dbname=" + dbname + " host=" + host + " port=" + port + " user=" + user + " password=" + password + \
          " connect_timeout=" + timeout
    conn = psycopg2.connect(url)
    cur = conn.cursor()
    return conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：DB接続処理
#
# 処理概要           ：1.DB接続処理を行う。
#                      2.DB接続エラーの場合は再接続を複数回行う。
#
# 引数               ：ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def create_connection(logger, app_id, app_name):
    # 変数設定
    result = False
    x = 0
    conn = None
    cur = None
    try:
        conn, cur = base_create_connection()
        # 接続完了のため処理結果=True
        result = True
        # コネクションオブジェクト、カーソルオブジェクト、処理結果=Trueを返却する。
        return result, conn, cur
    except Exception as e:
        # エラー詳細判定を行う
        tmp_result = error_detail.exception(e, logger, app_id, app_name)
        # 判定結果が複数ある場合、DB再接続完了
        if type(tmp_result) == tuple:
            # コネクションオブジェクトと、カーソルオブジェクトを代入
            result = tmp_result[0]
            conn = tmp_result[1]
            cur = tmp_result[2]
            # 接続完了のため処理結果=True
            # コネクションオブジェクト、カーソルオブジェクト、処理結果=Trueを返却する。
            return result, conn, cur
        # 判定結果=Falseの場合、エラー
        else:
            # コネクションオブジェクト、カーソルオブジェクト、、処理結果=Falseを返却する。
            return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：DB参照処理（1行取得）
#
# 処理概要           ：1.DB参照を行う。
#                      2.DB接続エラーの場合は再接続し、再び参照処理を複数回行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      クエリ文
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      取得結果
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fetchone(conn, cur, sql, logger, app_id, app_name):
    # 変数設定
    result = False
    select_result = None
    for x in range(setting_loop_num):
        try:
            # DB共通処理を呼び出してDBを参照する
            cur.execute(sql)
            select_result = cur.fetchone()
            # 参照完了のため処理結果=Ture
            result = True
            # コネクションオブジェクト、カーソルオブジェクト、処理結果、参照結果を返却する。
            return result, select_result, conn, cur
        except Exception as e:
            # エラー詳細判定を行う
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            # ループ回数確認、設定ループ数未満の場合、以下の処理を行う
            if x < (setting_loop_num - 1):
                # 判定結果=Trueの場合、DB再接続完了
                if type(tmp_result) == tuple:
                    # コネクションオブジェクトと、カーソルオブジェクトを代入
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # 再実施
                    continue
                # 判定結果=Falseの場合、エラー
                else:
                    # コネクションオブジェクト、カーソルオブジェクト、、処理結果=Falseを返却する。
                    return result, select_result, conn, cur
            # ループ回数条件超過のため、エラー
            else:
                # コネクションオブジェクト、カーソルオブジェクト、、処理結果=Falseを返却する。
                return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：DB参照処理（全行取得）
#
# 処理概要           ：1.DB参照を行う。
#                      2.DB接続エラーの場合は再接続し、再び参照処理を複数回行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      クエリ文
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      取得結果
#                      コネクションオブジェクト
#                      カーソルオブジェクト
# ------------------------------------------------------------------------------------
def select_fetchall(conn, cur, sql, logger, app_id, app_name):
    result = False
    select_result = None
    for x in range(setting_loop_num):
        try:
            cur.execute(sql)
            select_result = cur.fetchall()
            result = True
            return result, select_result, conn, cur
        except Exception as e:
            # エラー詳細判定を行う
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            # ループ回数確認、設定ループ数未満の場合、以下の処理を行う
            if x < (setting_loop_num - 1):
                # 判定結果=Trueの場合、DB再接続完了
                if type(tmp_result) == tuple:
                    # コネクションオブジェクトと、カーソルオブジェクトを代入
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # 再実施
                    continue
                # 判定結果=Falseの場合、エラー
                else:
                    # コネクションオブジェクト、カーソルオブジェクト、、処理結果=Falseを返却する。
                    return result, select_result, conn, cur
            # ループ回数条件超過のため、エラー
            else:
                # コネクションオブジェクト、カーソルオブジェクト、、処理結果=Falseを返却する。
                return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：DBデータ操作
#
# 処理概要           ：1.DB更新や登録、削除を行う。
#                      2.DB接続エラーの場合は再接続し、再びDB操作処理を複数回行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      クエリ文
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def operate_data(conn, cur, sql, logger, app_id, app_name):
    result = False
    for x in range(setting_loop_num):
        try:
            cur.execute(sql)
            result = True
            return result, conn, cur
        except Exception as e:
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            if x < (setting_loop_num - 1):
                # 判定結果=Trueの場合、DB再接続完了
                if type(tmp_result) == tuple:
                    # コネクションオブジェクトと、カーソルオブジェクトを代入
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # 再実施
                    continue
                # 判定結果=Falseの場合、エラー
                else:
                    return result, conn, cur
            else:
                return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：テーブル作成
#
# 処理概要           ：1.DB更新や登録を行う。
#                      2.DB接続エラーの場合は再接続し、再び作成処理を複数回行う。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      反番
#                      検査番号
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def create_table(conn, cur, fabric_name, inspection_num, inspection_date, logger, app_id, app_name):
    result = False
    for x in range(setting_loop_num):
        try:
            cur.callproc('table_create', [fabric_name, inspection_num, inspection_date])
            result = True
            return result, conn, cur
        except Exception as e:
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            if x < (setting_loop_num - 1):
                # 判定結果=Trueの場合、DB再接続完了
                if type(tmp_result) == tuple:
                    # コネクションオブジェクトと、カーソルオブジェクトを代入
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # 再実施
                    continue
                # 判定結果=Falseの場合、エラー
                else:
                    return result, conn, cur
            else:
                return result, conn, cur


# ------------------------------------------------------------------------------------
# 処理名             ：DB切断
#
# 処理概要           ：1.DBとのコネクションを切断する
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def close_connection(conn, cur, logger, app_id, app_name):
    result = False
    x = setting_loop_num
    try:
        cur.close()
        conn.close()
        result = True
        return result
    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result
