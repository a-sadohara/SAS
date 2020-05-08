# -*- coding: SJIS -*-
# ----------------------------------------
# ■ エラー時共通機能
# ----------------------------------------
from pathlib import Path
import configparser
import traceback
from sys import argv
import win32api
import win32con
import win32evtlog
import win32security
import win32evtlogutil

import error_detail

#  共通設定ファイル読込み
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

error_file_path = common_inifile.get('ERROR_FILE', 'path')


def write_eventlog_error(appName, message, logger, app_id, app_name):
    ph = win32api.GetCurrentProcess()
    th = win32security.OpenProcessToken(ph, win32con.TOKEN_READ)
    sid = win32security.GetTokenInformation(th, win32security.TokenUser)[0]
    data = "Application\0Data".encode("ascii")

    eventID = 65502
    type = win32evtlog.EVENTLOG_ERROR_TYPE
    desc = [message]

    win32evtlogutil.ReportEvent(
        appName,
        eventID,
        eventType=type,
        strings=desc,
        data=data,
        sid=sid
    )

def write_eventlog_warning(appName, message):
    ph = win32api.GetCurrentProcess()
    th = win32security.OpenProcessToken(ph, win32con.TOKEN_READ)
    sid = win32security.GetTokenInformation(th, win32security.TokenUser)[0]
    data = "Application\0Data".encode("ascii")

    eventID = 65503
    type = win32evtlog.EVENTLOG_WARNING_TYPE
    desc = [message]

    win32evtlogutil.ReportEvent(
        appName,
        eventID,
        eventType=type,
        strings=desc,
        data=data,
        sid=sid
    )


# ------------------------------------------------------------------------------------
# 関数名             ：エラー時共通処理実行
#
# 処理概要           ：1.機能で異常終了する際に、共通で実行する処理を行う。
#
# 引数               ：エラー出力ファイル名
#
# 戻り値             ：なし
# ------------------------------------------------------------------------------------
def common_execute(error_file, logger, app_id, app_name):
    result = False
    try:
        Path(error_file_path + '\\' +  error_file).touch()
        result = True
        return result
    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        write_eventlog_error(app_name, str(traceback.format_exc()), logger, app_id, app_name)
        return result
