# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能 起動機能
# ----------------------------------------
import tkinter as tk
from tkinter import StringVar, messagebox

import configparser
import logging.config
import subprocess
import re
import sys
import traceback

import error_detail

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_process_start.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/process_start_config.ini', 'SJIS')
all_function_id = inifile.get('NAME', 'all_function_id').split(',')
all_function_name = inifile.get('NAME', 'all_function_name').split(',')

# ログに使用する機能ID,機能名を取得する
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')

result_flg = False


# ------------------------------------------------------------------------------------
# 処理名             ：プログラム起動実行
#
# 処理概要           ：1.引数のプログラムを起動する。
#
# 引数               ：プログラム名リスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def exec_program_start(program_id_list, programs_path):
    result = False

    try:
        for program_id in program_id_list:
            cmd = programs_path + '/' + program_id
            # subprocess.Popen(cmd)
            logger.debug(cmd)
            tmp_result = subprocess.Popen(['start', cmd], shell=True)
            test = tmp_result.communicate()[0]
            return_code = tmp_result.returncode
            if return_code == 0:
                result = True
            else:
                result = False

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)
    logger.debug(result)
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
def start_up_func(param_program_name=None, flag=0):
    # 変数定義
    error_file_name = None
    result = False

    try:
        ### 設定ファイルからの値取得
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # プログラム(exe)格納パス
        programs_path = inifile.get('PATH', 'programs_path')

        # 全機能起動する際の対象プログラム名
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

        logger.info('[%s:%s] %s処理を開始します。', app_id, app_name, app_name)

        logger.debug('[%s:%s] 対象プログラムの起動実行を開始します。[プログラム名]=[%s]', app_id, app_name, target_program_name)
        tmp_result = exec_program_start(target_program_name, programs_path)

        if tmp_result:
            logger.debug('[%s:%s] 対象プログラムの起動実行が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 対象プログラムの起動実行に失敗しました。', app_id, app_name)
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
        global result_flg
        result_flg = True
    else:
        pass
    if flag == 1:
        pass
    else:
        root.destroy()


def button_selected(flag):
    if flag == 1:
        start_up_func(None, flag)
    else:
        if len(listbox.curselection()) == 0:
            return
        index = listbox.curselection()[0]
        print('test')
        print(listbox.get(index))
        if listbox.get(index) == "全機能起動":
            start_up_func(None, flag)
        else:
            function_dict = dict(zip(all_function_name, all_function_id))
            function_num = function_dict.get(listbox.get(index))
            function_name = 'function_' + function_num + '.exe'
            start_up_func(function_name, flag)

def close_window():
    global result_flg
    result_flg = 'destory'
    root.destroy()


# ------------------------------------------------------------------------------------
# 処理名             ：ウィンドウ処理
#
# 処理概要           ：1.起動選択ウィンドウに関する処理
#
# 引数               ：なし
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
args = sys.argv

if len(args) > 1:
    print("Recovery")
    button_selected(1)

else:
    ### メインウィンドウ ###
    root = tk.Tk()
    root.title('起動対象機能選択')
    root.update_idletasks()
    ww = root.winfo_screenwidth()
    lw = root.winfo_width()
    wh = root.winfo_screenheight()
    lh = root.winfo_height()
    root.geometry(str(lw+100) + "x" + str(lh+50) + "+" + str(int(ww / 2 - lw / 2)) + "+" + str(int(wh / 2 - lh / 2)))
    frame = tk.Frame(root, width=400, height=500, bg="white")
    frame.pack(padx=10, pady=10)



    ### Listbox ###
    var = StringVar(value=all_function_name)
    listbox = tk.Listbox(frame,
                         listvariable=var,
                         height=10,
                         width=50)
    listbox.pack()

    ### Button ###
    button_1 = tk.Button(root, text="実行", command=lambda : button_selected(0))
    button_2 = tk.Button(root, text="キャンセル", command=close_window)
    button_1.place(x=100, y=200)
    button_2.place(x=150, y=200)

    root.mainloop()

    root = tk.Tk()
    root.withdraw()

    if result_flg is True:
        messagebox.showinfo('起動終了', '対象機能起動処理が終了しました。')
    elif result_flg is False:
        messagebox.showerror('起動エラー', '対象機能起動処理が失敗しました。')
    else:
        pass
