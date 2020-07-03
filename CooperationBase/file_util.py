# -*- coding: SJIS -*-
# ----------------------------------------
# ■   ファイル操作共通機能
# ----------------------------------------
import datetime
import configparser
import glob
import shutil
import codecs
import os
import cv2
import numpy as np
from pathlib import Path

import error_detail

#  共通設定ファイル読込み
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
#  共通設定ファイル読込み
file_inifile = configparser.ConfigParser()
file_inifile.read('D:/CI/programs/config/file_util.ini', 'SJIS')

setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))
flag = file_inifile.get('CSV', 'flag')
label_ng = file_inifile.get('CSV', 'label_ng')
label_others = file_inifile.get('CSV', 'label_others')
networkpath_error = file_inifile.get('ERROR_INFO', 'networkpath_error')


# ------------------------------------------------------------------------------------
# 処理名             ：ファイルリスト取得
#
# 処理概要           ：1.フォルダに格納されているファイルのリストを取得する。
#                      2.ネットワークパスエラーの場合はフォルダへの再接続を複数回行う。
#
# 引数               ：格納フォルダパス
#                      ファイルリスト
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      ファイルリスト
# ------------------------------------------------------------------------------------
def get_file_list(file_path, file_patern, logger, app_id, app_name):
    result = False
    file_list = None
    e = None
    for x in range(setting_loop_num):
        try:
            if os.path.exists(file_path):
                file_list = glob.glob(file_path + file_patern)
                # 処理結果=True
                result = True
                return result, file_list, e
            else:
                os.listdir(file_path)
                continue
        except Exception as e:
            # エラー詳細判定を行う

            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, file_list, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, file_list, e


# ------------------------------------------------------------------------------------
# 処理名             ：ファイル移動
#
# 処理概要           ：1.ファイルの移動を行う。
#                      2.ネットワークパスエラーの場合はフォルダへの再接続を複数回行う。
#
# 引数               ：移動元ファイルパス
#                      移動先ファイルパス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      パーミッションエラーメッセージ
# ------------------------------------------------------------------------------------
def move_file(file_path, output_path, logger, app_id, app_name):
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            file_name = os.path.basename(file_path)
            base_dir = os.path.dirname(file_path)
            if os.path.exists(file_path):
                if os.path.exists(output_path + '\\' + file_name):
                    # 存在する場合、退避ファイルに別名（※）をつける。
                    # （※）退避ファイルの最終更新日時（YYYYMMDD_HHMMSS）を、ファイル名の末尾に付与する。
                    # xxxx.csv → xxxx.csv.20200101_235959
                    timestamp = datetime.datetime.now()
                    file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
                    target_file_name = file_name + '.' + file_timestamp
                    # ファイルを移動する
                    shutil.move(file_path, output_path + "\\" + target_file_name)
                else:
                    # 存在しない場合、ファイル名はそのままファイルを移動する
                    shutil.move(file_path, output_path + "\\" + file_name)
                # 処理結果=True
                result = True
                return result, e
            else:
                os.listdir(base_dir)
                continue
        except Exception as e:
            # エラー詳細判定を行う
            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# 処理名             ：ファイルコピー
#
# 処理概要           ：1.ファイルのコピーを行う。
#                      2.ネットワークパスエラーの場合はフォルダへの再接続を複数回行う。
#
# 引数               ：コピー元ファイルパス
#                      コピー先ファイルパス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def copy_file(file_path, output_path, logger, app_id, app_name):
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            file_name = os.path.basename(file_path)
            base_dir = os.path.dirname(file_path)
            if os.path.exists(file_path):
                if os.path.exists(output_path + '\\' + file_name):
                    # 存在する場合、退避ファイルに別名（※）をつける。
                    # （※）退避ファイルの最終更新日時（YYYYMMDD_HHMMSS）を、ファイル名の末尾に付与する。
                    # xxxx.csv → xxxx.csv.20200101_235959
                    timestamp = datetime.datetime.now()
                    file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
                    target_file_name = file_name + '.' + file_timestamp
                    # ファイルを移動する
                    shutil.copy2(file_path, output_path + "\\" + target_file_name)
                else:
                    # 存在しない場合、ファイル名はそのままファイルを移動する
                    shutil.copy2(file_path, output_path)
                # 処理結果=True
                result = True
                return result, e
            else:
                os.listdir(base_dir)
                continue
        except Exception as e:
            # エラー詳細判定を行う
            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, e

# ------------------------------------------------------------------------------------
# 処理名             ：ディレクトリ作成
#
# 処理概要           ：1.ディレクトリ作成を行う。
#                      2.ネットワークパスエラーの場合はフォルダへの再接続を複数回行う。
#
# 引数               ：ディレクトリ作成対象パス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ： 処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def make_directory(file_path, logger, app_id, app_name):
    # 変数設定
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            # ディレクトリ作成
            os.makedirs(file_path, exist_ok=True)
            # 処理完了=True
            result = True
            return result, e
        except Exception as e:
            # エラー詳細判定を行う
            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# 処理名             ：画像読込
#
# 処理概要           ：1.画像を読込んで、読込んだデータを返却する。
#
#
# 引数               ：画像パス
#                      読込フラグ
#                      データ型
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：画像データ
#                      処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def read_image(filename, logger, app_id, app_name, flags=cv2.IMREAD_COLOR, dtype=np.uint8):
    result = False
    x = 0
    e = None
    try:
        n = np.fromfile(filename, dtype)
        img = cv2.imdecode(n, flags)
        result = True
        return result, img, e

    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        img = None
        return result, img, e


# ------------------------------------------------------------------------------------
# 処理名             ：結果ファイル読込
#
# 処理概要           ：1.連携されたファイルを読込んで、データを返却する。
#
#
# 引数               ：読込対象ファイル
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：読込結果
#                      処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
# RAPIDの結果ファイルを読み込む
def read_result_file(result_file, logger, app_id, app_name):
    x = 0
    result_data = {}
    e = None
    try:
        LABEL_NAME = [label_ng, label_others]
        fp = codecs.open(result_file, "r", "SJIS")
        line = fp.readline()
        if line.find("[DataPath]") != -1:
            itemList = line[:-1].split("[DataPath]")[1]
            datapath = itemList.split("\"")[1]
            result_data["datapath"] = datapath
            line = fp.readline()
        else:
            result_data["datapath"] = ""
        datas = []
        while line:
            # 結果ファイルのフォーマット
            # 3,"<ファイル名>",<X座標>,<Y座標>,<幅>,<高さ>,<ラベル>,<確信度>・・・
            # もしくは
            # "<ファイル名>",<X座標>,<Y座標>,<幅>,<高さ>,<ラベル>,<確信度>・・・
            data = {}
            if line.find("[") != -1:
                line = fp.readline()
                continue
            itemList = line.strip().split(",")
            idx = 0
            if itemList[0] == flag:
                idx = 1
            data["filename"] = itemList[idx].split("\"")[1]
            point = {}
            point["x"] = int(itemList[idx + 1])
            point["y"] = int(itemList[idx + 2])
            data["point"] = point
            data["width"] = int(itemList[idx + 3])
            data["height"] = int(itemList[idx + 4])
            data["label"] = itemList[idx + 5]
            rate = {}
            for cnt in range(0, len(LABEL_NAME)):
                rate[itemList[idx + 5 + (2 * cnt)]] = float(itemList[idx + 6 + (2 * cnt)])
            data["rate"] = rate
            data["duplication"] = 1
            datas.append(data)
            line = fp.readline()
        result_data["data"] = datas
        fp.close()
        result = True
        return result, result_data, e

    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result, result_data, e


# ------------------------------------------------------------------------------------
# 処理名             ：結果ファイル出力
#
# 処理概要           ：1.連携されたデータを整形してCSVとして出力する。
#
# 引数               ：ディレクトリ作成対象パス
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ： 処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
# RAPIDの結果ファイルのフォーマットで出力
def write_result_file(output_file, result_data, datapath, logger, app_id, app_name):
    result = False
    x = 0
    e = None
    try:
        fp = codecs.open(output_file, "w", "SJIS")
        if result_data["datapath"] == "":
            fp.write("[DataPath]\"" + result_data["datapath"] + "\"" + "\n")
        else:
            fp.write("[DataPath]\"" + datapath + "\"" + "\n")
        for result in result_data["data"]:
            buf = "3"
            buf = buf + "," + "\"" + result["filename"] + "\""
            buf = buf + "," + str(result["point"]["x"])
            buf = buf + "," + str(result["point"]["y"])
            buf = buf + "," + str(result["width"])
            buf = buf + "," + str(result["height"])
            if result["label"] != label_ng:
                buf = buf + "," + label_others
                buf = buf + "," + str(result["rate"]["_others"])
                buf = buf + "," + label_ng
                buf = buf + "," + "0"
            else:
                buf = buf + "," + label_ng
                buf = buf + "," + str(result["rate"]["NG"])
                buf = buf + "," + label_others
                buf = buf + "," + "0"
            fp.write(buf + "\n")
        fp.close()
        result = True
        return result, e
    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result, e


# ------------------------------------------------------------------------------------
# 処理名             ：パトライト点灯
#
# 処理概要           ：1.パトライト点灯の際に指定のフォルダに通知を出力する。
#
# 引数               ：ディレクトリパス
#                      ファイル名
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ： 処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def light_patlite(name, logger, app_id, app_name):
    patlite_path = common_inifile.get('FILE_PATH', 'patlite_path')
    e = None
    for x in range(setting_loop_num):
        try:
            if os.path.exists(patlite_path):
                Path(patlite_path + '\\' +  name).touch()
                # 処理結果=True
                result = True
                return result, e
            else:
                os.listdir(patlite_path)
                continue
        except Exception as e:
            # エラー詳細判定を行う
            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像フォルダ削除
#
# 処理概要           ：1.空のフォルダを削除する。
#
# 引数               ：削除パス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def delete_dir(path, logger, app_id, app_name):
    e = None
    for x in range(setting_loop_num):
        try:
            shutil.rmtree(path)
            result = True
            return result, e
        except Exception as e:
            # エラー詳細判定を行う
            result = error_detail.exception(e, logger, app_id, app_name)
            # 再実行回数未満の場合
            if x < (setting_loop_num - 1):
                # エラー判定結果=Trueの場合、 ネットワークパスエラーのため再実行
                if result == networkpath_error:
                    continue
                # エラー判定結果=Falseの場合、処理結果=False
                else:
                    return result, e
            # 再実行回数以上の場合、エラー判定。処理結果=False
            else:
                return result, e
