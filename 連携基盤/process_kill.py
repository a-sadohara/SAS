# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能 強制停止機能
# ----------------------------------------

import configparser
import logging.config
import os
import signal
import subprocess
import re
import sys
import traceback

import error_detail
import error_util

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_process_kill.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/process_kill_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：プロセスID取得
#
# 処理概要           ：1.OSの起動プロセス状態から、引数のプログラム名に一致するプロセスIDを取得する。
#
# 引数               ：プログラム名リスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    プロセスIDリスト
# ------------------------------------------------------------------------------------
def get_process_id(program_name):
    result = False
    processid_list = []

    try:
        # プロセス一覧取得コマンドの実行結果取得時の文字コード
        encoding = inifile.get('SUB_PROCESS', 'encoding')

        # プロセス一覧をwmic経由で取得する
        # pythonをexe化した際、「tasklisk」コマンドでは判別不可（python内部で引数でpyファイルを指定しており、tasklistでは判断できない）
        # のため利用している。
        command_str = ' '.join(
            ['wmic', 'process', 'get', '/FORMAT:LIST'])
        result = subprocess.run(command_str, shell=True, stdout=subprocess.PIPE)
        if not result.stdout:
            # コマンド出力結果が存在しない
            result, processid_list

        name = ''
        # プロセス取得コマンドの標準出力結果からプロセスIDを取得する
        # wmic経由で取得すると、「項目名=項目値」のフォーマットで情報が取得できるので、
        # 「ProcessId」の項目名を探し、その値を取得する。
        # なお、pythonをexe化したものを実行すると、プロセスが2つ（親子（親:execのrapper,子:実行プログラム））
        # 起動する事を確認しているので、該当プロセスは一通り取得する
        for line in result.stdout.decode(encoding).split('\r\r\n'):
            # プロセスの区切りチェック
            if line == '':
                # プロセスの区切りの場合
                if name:
                    if name in program_name:
                        # 対象プロセスIDの場合
                        processid_list.append(processid)

                    name = ''
                    processid = ''

                continue

            else:
                pass

            key = line.split('=')[0]
            value = line.split('=')[1]

            # プログラム名取得
            if key == 'Name':
                name = value
            # プロセスID取得
            if key == 'ProcessId':
                processid = value

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result, processid_list


# ------------------------------------------------------------------------------------
# 処理名             ：プロセス停止実行
#
# 処理概要           ：1.引数のプロセスIDを停止する。
#
# 引数               ：プロセスIDリスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def exec_process_kill(process_id_list):
    result = False

    try:
        for process_id in process_id_list:
            os.kill(int(process_id), signal.SIGINT)

        result = True

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.強制停止機能を行う。
#                      引数（プログラム名）を指定した際、引数のプログラム名のプロセスを停止する。
#                      引数が指定されなかった際、定義ファイル内の全プログラム名のプロセスを停止する。
#
# 引数               ：プログラム名
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def main(param_program_name=None):
    # 変数定義
    error_file_name = None
    result = False
    try:
        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # 全機能停止する際の対象プログラム名
        all_target_process_name = inifile.get('NAME', 'all_target_process_name')
        all_target_process_name = re.split(',', all_target_process_name)

        logger.info('[%s:%s] %s機能を起動します。', app_id, app_name, app_name)

        ### 対象プログラムのプロセスID取得
        # 引数が指定されている場合は、引数のプログラム名、未指定の場合は全機能（設定ファイル値）を停止する
        target_program_name = []
        if param_program_name:
            target_program_name.append(param_program_name)
        else:
            target_program_name = all_target_process_name

        logger.debug('[%s:%s] 対象プログラムのプロセスID取得を開始します。[対象プログラム名]=[%s]', app_id, app_name, target_program_name)

        tmp_result, process_id_list = get_process_id(target_program_name)

        if tmp_result:
            logger.debug('[%s:%s] 対象プログラムのプロセスID取得が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 対象プログラムのプロセスID取得に失敗しました。', app_id, app_name)
            sys.exit()

        if len(process_id_list) == 0:
            # 該当プロセスIDが存在しない（既にプログラム停止している場合）
            logger.info('[%s:%s] 対象プログラムのプロセスIDは存在しません。', app_id, app_name)
        else:
            pass

        logger.info('[%s:%s] %s処理を開始します。', app_id, app_name, app_name)

        logger.debug('[%s:%s] 対象プログラムのプロセス停止実行を開始します。[対象プロセスID]=[%s]', app_id, app_name, process_id_list)
        tmp_result = exec_process_kill(process_id_list)

        if tmp_result:
            logger.debug('[%s:%s] 対象プログラムのプロセス停止実行が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 対象プログラムのプロセス停止実行に失敗しました。', app_id, app_name)
            sys.exit()

        logger.debug('[%s:%s] プロセス停止確認を開始します。[対象プログラム名]=[%s]', app_id, app_name, target_program_name)

        tmp_result, process_id_list = get_process_id(target_program_name)

        if tmp_result:
            logger.debug('[%s:%s] プロセス停止確認が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] プロセス停止確認が失敗しました。', app_id, app_name)
            sys.exit()

        if len(process_id_list) == 0:
            # 該当プロセスIDが存在しない（既にプログラム停止している場合）
            logger.debug('[%s:%s] 対象プログラムのプロセスIDは存在しません。', app_id, app_name)
        else:
            logger.error('[%s:%s] 対象プログラムのプロセス停止実行に失敗しました。', app_id, app_name)
            sys.exit()

        logger.info("[%s:%s] %s処理は正常に終了しました。", app_id, app_name, app_name)
        result = True

    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))

    finally:
        pass

    # 戻り値設定
    # 呼出側（バッチプログラム側）で戻り値判定（ERRORLEVEL）する際の戻り値を設定する。
    if result:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    param_program_name = None
    args = sys.argv
    if len(args) > 1:
        param_program_name = args[1]
    main(param_program_name)
