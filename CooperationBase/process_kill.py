# -*- coding: SJIS -*-
# ----------------------------------------
# �� �^�p�@�\ ������~�@�\
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

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_process_kill.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/process_kill_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


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
                        processid_list.append(processid)

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


# ------------------------------------------------------------------------------------
# ������             �F�v���Z�X��~���s
#
# �����T�v           �F1.�����̃v���Z�XID���~����B
#
# ����               �F�v���Z�XID���X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def exec_process_kill(process_id_list):
    result = False

    try:
        for process_id in process_id_list:
            os.kill(int(process_id), signal.SIGINT)

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

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
def main(param_program_name=None):
    # �ϐ���`
    error_file_name = None
    result = False
    try:
        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �S�@�\��~����ۂ̑Ώۃv���O������
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

        logger.debug('[%s:%s] �Ώۃv���O�����̃v���Z�XID�擾���J�n���܂��B[�Ώۃv���O������]=[%s]', app_id, app_name, target_program_name)

        tmp_result, process_id_list = get_process_id(target_program_name)

        if tmp_result:
            logger.debug('[%s:%s] �Ώۃv���O�����̃v���Z�XID�擾���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �Ώۃv���O�����̃v���Z�XID�擾�Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        if len(process_id_list) == 0:
            # �Y���v���Z�XID�����݂��Ȃ��i���Ƀv���O������~���Ă���ꍇ�j
            logger.info('[%s:%s] �Ώۃv���O�����̃v���Z�XID�͑��݂��܂���B', app_id, app_name)
        else:
            pass

        logger.info('[%s:%s] %s�������J�n���܂��B', app_id, app_name, app_name)

        logger.debug('[%s:%s] �Ώۃv���O�����̃v���Z�X��~���s���J�n���܂��B[�Ώۃv���Z�XID]=[%s]', app_id, app_name, process_id_list)
        tmp_result = exec_process_kill(process_id_list)

        if tmp_result:
            logger.debug('[%s:%s] �Ώۃv���O�����̃v���Z�X��~���s���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �Ώۃv���O�����̃v���Z�X��~���s�Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        logger.debug('[%s:%s] �v���Z�X��~�m�F���J�n���܂��B[�Ώۃv���O������]=[%s]', app_id, app_name, target_program_name)

        tmp_result, process_id_list = get_process_id(target_program_name)

        if tmp_result:
            logger.debug('[%s:%s] �v���Z�X��~�m�F���I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �v���Z�X��~�m�F�����s���܂����B', app_id, app_name)
            sys.exit()

        if len(process_id_list) == 0:
            # �Y���v���Z�XID�����݂��Ȃ��i���Ƀv���O������~���Ă���ꍇ�j
            logger.debug('[%s:%s] �Ώۃv���O�����̃v���Z�XID�͑��݂��܂���B', app_id, app_name)
        else:
            logger.error('[%s:%s] �Ώۃv���O�����̃v���Z�X��~���s�Ɏ��s���܂����B', app_id, app_name)
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
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    param_program_name = None
    args = sys.argv
    if len(args) > 1:
        param_program_name = args[1]
    main(param_program_name)
