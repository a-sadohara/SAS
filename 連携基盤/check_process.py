# -*- coding: SJIS -*-
# ----------------------------------------
# �� �^�p�@�\ �N���@�\
# ----------------------------------------

# �ݒ�t�@�C���ǂݍ���
import tkinter as tk
import traceback
from tkinter import messagebox
import configparser
import re
import subprocess
import logging.config
import sys

import error_detail

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_check_process.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_process_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')

num = None
result = False
connected_list = []
disconnected_list = []


# ------------------------------------------------------------------------------------
# ������             �F�v���Z�XID�擾
#
# �����T�v           �F1.OS�̋N���v���Z�X��Ԃ���A�����̃v���O�������Ɉ�v����v���Z�XID���擾����B
#
# ����               �F�v���O���������X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �v���Z�XID���X�g
# ------------------------------------------------------------------------------------
def get_process_id(program_name):
    result = False
    processid_list = []

    try:
        # �v���Z�X�ꗗ�擾�R�}���h�̎��s���ʎ擾���̕����R�[�h
        encoding = inifile.get('SUB_PROCESS', 'encoding')

        # �v���Z�X�ꗗ��wmic�o�R�Ŏ擾����
        # python��exe�������ہA�utasklisk�v�R�}���h�ł͔��ʕs�ipython�����ň�����py�t�@�C�����w�肵�Ă���Atasklist�ł͔��f�ł��Ȃ��j
        # �̂��ߗ��p���Ă���B
        command_str = ' '.join(
            ['wmic', 'process', 'get', '/FORMAT:LIST'])
        result = subprocess.run(command_str, shell=True, stdout=subprocess.PIPE)
        if not result.stdout:
            # �R�}���h�o�͌��ʂ����݂��Ȃ�
            result, processid_list

        name = ''
        # �v���Z�X�擾�R�}���h�̕W���o�͌��ʂ���v���Z�XID���擾����
        # wmic�o�R�Ŏ擾����ƁA�u���ږ�=���ڒl�v�̃t�H�[�}�b�g�ŏ�񂪎擾�ł���̂ŁA
        # �uProcessId�v�̍��ږ���T���A���̒l���擾����B
        # �Ȃ��Apython��exe���������̂����s����ƁA�v���Z�X��2�i�e�q�i�e:exec��rapper,�q:���s�v���O�����j�j
        # �N�����鎖���m�F���Ă���̂ŁA�Y���v���Z�X�͈�ʂ�擾����
        for line in result.stdout.decode(encoding).split('\r\r\n'):

            # �v���Z�X�̋�؂�`�F�b�N
            if line == '':
                # �v���Z�X�̋�؂�̏ꍇ
                if name:
                    if name in program_name:
                        # �Ώۃv���Z�XID�̏ꍇ
                        processid_list.append(name)

                    name = ''
                    processid = ''

                continue

            else:
                pass

            key = line.split('=')[0]
            value = line.split('=')[1]

            # �v���O�������擾
            if key == 'Name':
                name = value
            # �v���Z�XID�擾
            if key == 'ProcessId':
                processid = value

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, processid_list


def check_process():
    global result
    global connected_list
    global disconnected_list
    result = False
    try:
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �Ώۋ@�\��
        all_function_id = inifile.get('FUNCTION', 'all_function_id').split(',')
        all_function_name = inifile.get('FUNCTION', 'all_function_name').split(',')

        function_dict = dict(zip(all_function_id, all_function_name))

        connected_list = []
        disconnected_list = []

        # �S�@�\�N������ۂ̑Ώۃv���O������
        all_target_process_name = inifile.get('FUNCTION', 'all_target_process_name')
        all_target_process_name = re.split(',', all_target_process_name)

        logger.info('[%s:%s] %s�������J�n���܂��B' % (app_id, app_name, app_name))

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
            logger.info('[%s:%s] �@�\�͑S�ċN�����Ă��܂��B' % (app_id, app_name))
            result = True

        elif len(processid_list) == 0:
            logger.info('[%s:%s] �@�\�͑S�Ē�~���Ă��܂�' % (app_id, app_name))
            result = True

        else:
            if num == 0:
                logger.warning('[%s:%s] ���L�̋@�\���N�����Ă��܂���B �Y���@�\[%s]' % (app_id, app_name, disconnected_list))
                result = False
            else:
                logger.warning('[%s:%s] ���L�̋@�\����~���Ă��܂���B �Y���@�\[%s]' % (app_id, app_name, connected_list))
                result = False

        logger.info('[%s:%s] %s�����͐���ɏI�����܂����B' % (app_id, app_name, app_name))



    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))

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


### ���C���E�B���h�E ###
root = tk.Tk()
root.title('�N���Ώۋ@�\�I��')
root.update_idletasks()
ww = root.winfo_screenwidth()
lw = root.winfo_width()
wh = root.winfo_screenheight()
lh = root.winfo_height()
root.geometry(str(lw + 50) + "x" + str(lh) + "+" + str(int(ww / 2 - lw / 2)) + "+" + str(int(wh / 2 - lh / 2)))

label1 = tk.Label(text="\n�@�\�̃v���Z�X�m�F���s���܂��B\n���L����Ώۂ̏�����I���\n���s�{�^���������ĉ������B", padx=50)
label1.pack()

# ���W�I�{�^���̃��x�������X�g������
rdo_txt = ['�N���m�F', '��~�m�F']
# ���W�I�{�^���̏��
rdo_var = tk.IntVar()

# ���W�I�{�^���𓮓I�ɍ쐬���Ĕz�u
for i in range(len(rdo_txt)):
    rdo = tk.Radiobutton(root, value=i, variable=rdo_var, text=rdo_txt[i])
    rdo.place(x=80, y=80 + (i * 24))

### Button ###
button_1 = tk.Button(root, text="���s", command=button_selected)
button_2 = tk.Button(root, text="�L�����Z��", command=close_window)
button_1.place(x=70, y=170)
button_2.place(x=120, y=170)

root.mainloop()

root = tk.Tk()
root.withdraw()

if result is True:
    if len(connected_list) != 0:
        messagebox.showinfo('�m�F�I��', '�Ώۋ@�\�͑S�ċN�����Ă��܂��B')
    else:
        messagebox.showinfo('�m�F�I��', '�Ώۋ@�\�͑S�Ē�~���Ă��܂��B')

elif result is False:
    if num == 0:
        disconnected_list_str = re.sub(r'[][\']', '', str(disconnected_list)).replace(',', '\n')
        messagebox.showerror('�m�F�I��', '�ꕔ�@�\���N�����Ă��܂���B\n�y�Y���@�\�z\n%s' % disconnected_list_str)
    else:
        connected_list_str = re.sub(r'[][\']', '', str(connected_list)).replace(',', '\n')
        messagebox.showerror('�m�F�I��', '�ꕔ�@�\����~���Ă��܂���B\n�y�Y���@�\�z\n%s' % connected_list_str)
else:
    pass
