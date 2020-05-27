# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能 起動機能
# ----------------------------------------

# 設定ファイル読み込み
import tkinter as tk
import traceback
from tkinter import messagebox
import configparser
import re
import subprocess
import logging.config
import sys

import error_detail

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_check_process.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_process_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')

num = None
result = False
connected_list = []
disconnected_list = []


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
                        processid_list.append(name)

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


def check_process():
    global result
    global connected_list
    global disconnected_list
    result = False
    try:
        # エラーファイル名
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # 対象機能名
        all_function_id = inifile.get('FUNCTION', 'all_function_id').split(',')
        all_function_name = inifile.get('FUNCTION', 'all_function_name').split(',')

        function_dict = dict(zip(all_function_id, all_function_name))

        connected_list = []
        disconnected_list = []

        # 全機能起動する際の対象プログラム名
        all_target_process_name = inifile.get('FUNCTION', 'all_target_process_name')
        all_target_process_name = re.split(',', all_target_process_name)

        logger.info('[%s:%s] %s処理を開始します。' % (app_id, app_name, app_name))

        tmp_result, processid_list = get_process_id(all_target_process_name)
        if tmp_result:
            pass
        else:
            sys.exit()

        processid_list = sorted(set(processid_list), key=processid_list.index)


        for target_name in all_target_process_name:
            tmp_conect_list = []
            logger.info('target_name : %s' % (target_name))
            for i in range(len(processid_list)):
                logger.info('%s : %s' % (i, processid_list))
                logger.info('connected_list : %s' % connected_list)
                logger.info('tmp_connect_list : %s' % tmp_conect_list)
                logger.info('disconnected_list : %s' % disconnected_list)
                if target_name == processid_list[i]:
                    process_name = target_name[9:-4]
                    process_name = function_dict.get(process_name)
                    connected_list.append(process_name)
                    tmp_conect_list.append(process_name)
                elif i == processid_list.index(processid_list[-1]) and len(tmp_conect_list) == 0:
                    process_name = target_name[9:-4]
                    process_name = function_dict.get(process_name)
                    disconnected_list.append(process_name)
                    break
                else:
                    continue

        if len(connected_list) == len(all_target_process_name):
            logger.info('[%s:%s] 機能は全て起動しています。' % (app_id, app_name))
            result = True

        elif len(processid_list) == 0:
            logger.info('[%s:%s] 機能は全て停止しています' % (app_id, app_name))
            result = True

        else:
            if num == 0:
                logger.warning('[%s:%s] 下記の機能が起動していません。 該当機能[%s]' % (app_id, app_name, disconnected_list))
                result = False
            else:
                logger.warning('[%s:%s] 下記の機能が停止していません。 該当機能[%s]' % (app_id, app_name, connected_list))
                result = False

        logger.info('[%s:%s] %s処理は正常に終了しました。' % (app_id, app_name, app_name))



    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))

    finally:
        root.destroy()


def button_selected():
    global num
    num = rdo_var.get()
    check_process()


def close_window():
    global result
    result = 'destory'
    root.destroy()


### メインウィンドウ ###
root = tk.Tk()
root.title('起動対象機能選択')
root.update_idletasks()
ww = root.winfo_screenwidth()
lw = root.winfo_width()
wh = root.winfo_screenheight()
lh = root.winfo_height()
root.geometry(str(lw + 50) + "x" + str(lh) + "+" + str(int(ww / 2 - lw / 2)) + "+" + str(int(wh / 2 - lh / 2)))

label1 = tk.Label(text="\n機能のプロセス確認を行います。\n下記から対象の処理を選んで\n実行ボタンを押して下さい。", padx=50)
label1.pack()

# ラジオボタンのラベルをリスト化する
rdo_txt = ['起動確認', '停止確認']
# ラジオボタンの状態
rdo_var = tk.IntVar()

# ラジオボタンを動的に作成して配置
for i in range(len(rdo_txt)):
    rdo = tk.Radiobutton(root, value=i, variable=rdo_var, text=rdo_txt[i])
    rdo.place(x=80, y=80 + (i * 24))

### Button ###
button_1 = tk.Button(root, text="実行", command=button_selected)
button_2 = tk.Button(root, text="キャンセル", command=close_window)
button_1.place(x=70, y=170)
button_2.place(x=120, y=170)

root.mainloop()

root = tk.Tk()
root.withdraw()

if result is True:
    if len(connected_list) != 0:
        messagebox.showinfo('確認終了', '対象機能は全て起動しています。')
    else:
        messagebox.showinfo('確認終了', '対象機能は全て停止しています。')

elif result is False:
    if num == 0:
        disconnected_list_str = re.sub(r'[][\']', '', str(disconnected_list)).replace(',', '\n')
        messagebox.showerror('確認終了', '一部機能が起動していません。\n【該当機能】\n%s' % disconnected_list_str)
    else:
        connected_list_str = re.sub(r'[][\']', '', str(connected_list)).replace(',', '\n')
        messagebox.showerror('確認終了', '一部機能が停止していません。\n【該当機能】\n%s' % connected_list_str)
else:
    pass
