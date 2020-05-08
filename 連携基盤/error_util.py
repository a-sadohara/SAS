# -*- coding: SJIS -*-
# ----------------------------------------
# �� �G���[�����ʋ@�\
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

#  ���ʐݒ�t�@�C���Ǎ���
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
# �֐���             �F�G���[�����ʏ������s
#
# �����T�v           �F1.�@�\�ňُ�I������ۂɁA���ʂŎ��s���鏈�����s���B
#
# ����               �F�G���[�o�̓t�@�C����
#
# �߂�l             �F�Ȃ�
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
