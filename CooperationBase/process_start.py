# -*- coding: SJIS -*-
# ----------------------------------------
# �� �^�p�@�\ �N���@�\
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

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_process_start.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/process_start_config.ini', 'SJIS')
all_function_id = inifile.get('NAME', 'all_function_id').split(',')
all_function_name = inifile.get('NAME', 'all_function_name').split(',')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')

result_flg = False


# ------------------------------------------------------------------------------------
# ������             �F�v���O�����N�����s
#
# �����T�v           �F1.�����̃v���O�������N������B
#
# ����               �F�v���O���������X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
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
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
    logger.debug(result)
    return result


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.������~�@�\���s���B
#                      �����i�v���O�������j���w�肵���ہA�����̃v���O�������̃v���Z�X���~����B
#                      �������w�肳��Ȃ������ہA��`�t�@�C�����̑S�v���O�������̃v���Z�X���~����B
#
# ����               �F�v���O������
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def start_up_func(param_program_name=None, flag=0):
    # �ϐ���`
    error_file_name = None
    result = False

    try:
        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �v���O����(exe)�i�[�p�X
        programs_path = inifile.get('PATH', 'programs_path')

        # �S�@�\�N������ۂ̑Ώۃv���O������
        all_target_process_name = inifile.get('NAME', 'all_target_process_name')
        all_target_process_name = re.split(',', all_target_process_name)

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### �Ώۃv���O�����̃v���Z�XID�擾
        # �������w�肳��Ă���ꍇ�́A�����̃v���O�������A���w��̏ꍇ�͑S�@�\�i�ݒ�t�@�C���l�j���~����
        target_program_name = []
        if param_program_name:
            target_program_name.append(param_program_name)
        else:
            target_program_name = all_target_process_name

        logger.info('[%s:%s] %s�������J�n���܂��B', app_id, app_name, app_name)

        logger.debug('[%s:%s] �Ώۃv���O�����̋N�����s���J�n���܂��B[�v���O������]=[%s]', app_id, app_name, target_program_name)
        tmp_result = exec_program_start(target_program_name, programs_path)

        if tmp_result:
            logger.debug('[%s:%s] �Ώۃv���O�����̋N�����s���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �Ώۃv���O�����̋N�����s�Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        logger.info("[%s:%s] %s�����͐���ɏI�����܂����B", app_id, app_name, app_name)

        result = True

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))

    finally:
        pass

    # �߂�l�ݒ�
    # �ďo���i�o�b�`�v���O�������j�Ŗ߂�l����iERRORLEVEL�j����ۂ̖߂�l��ݒ肷��B
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
        if listbox.get(index) == "�S�@�\�N��":
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
# ������             �F�E�B���h�E����
#
# �����T�v           �F1.�N���I���E�B���h�E�Ɋւ��鏈��
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
args = sys.argv

if len(args) > 1:
    print("Recovery")
    button_selected(1)

else:
    ### ���C���E�B���h�E ###
    root = tk.Tk()
    root.title('�N���Ώۋ@�\�I��')
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
    button_1 = tk.Button(root, text="���s", command=lambda : button_selected(0))
    button_2 = tk.Button(root, text="�L�����Z��", command=close_window)
    button_1.place(x=100, y=200)
    button_2.place(x=150, y=200)

    root.mainloop()

    root = tk.Tk()
    root.withdraw()

    if result_flg is True:
        messagebox.showinfo('�N���I��', '�Ώۋ@�\�N���������I�����܂����B')
    elif result_flg is False:
        messagebox.showerror('�N���G���[', '�Ώۋ@�\�N�����������s���܂����B')
    else:
        pass
