# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\304  �����P�ʕ����@�\
# ----------------------------------------

import sys
from concurrent.futures.process import ProcessPoolExecutor
import configparser
import logging.config
import datetime
import time
import traceback
import math
import win32_setctime
import os
import logging.handlers

import error_detail
import file_util
import db_util
import error_util
import resize_image
import custom_handler
import light_patlite

logging.handlers.CustomTimedRotatingFileHandler = custom_handler.ParallelTimedRotatingFileHandler

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_separate_process_unit.conf", disable_existing_loggers=False)
logger = logging.getLogger("separate_process_unit")

# �@�\304�ݒ�Ǎ�
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/separate_process_unit.ini', 'SJIS')
# ���ʐݒ�Ǎ�
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �FDB�ڑ�
#
# �����T�v           �F1.DB�ɐڑ�����B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_connection():
    func_name = sys._getframe().f_code.co_name
    # DB�ɐڑ�����B
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�������擾
#
# �����T�v           �F1.�������e�[�u�����猟�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                       �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, unit_num):
    func_name = sys._getframe().f_code.co_name
    #  �N�G�����쐬����
    sql = 'select product_name, fabric_name, inspection_num, status, imaging_starttime from fabric_info ' \
          'where unit_num = \'%s\' and separateresize_endtime IS NULL and status != 0 order by imaging_starttime asc' \
          % (unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����猟�������擾����B
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, fabric_info, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F����ID�擾�i�ő�l�j
#
# �����T�v           �F1.�������e�[�u�����猟�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                       �J�[�\���I�u�W�F�N�g
#                       ����
#                       �����ԍ�
#                       RAPID�T�[�o�z�X�g��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������
# ------------------------------------------------------------------------------------
def select_processing_status_info(conn, cur, fabric_name, inspection_num, rapid_hostname, imaging_starttime,
                                  unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    #  �N�G�����쐬����
    sql = 'select processing_id from processing_status where fabric_name = \'%s\' and inspection_num = %s and ' \
          'rapid_host_name = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\' order by processing_id desc' \
          % (fabric_name, inspection_num, rapid_hostname, imaging_starttime, unit_num)

    logger.debug('[%s:%s] ����ID�擾�i�ő�l�jSQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����猟�������擾����B
    result, processing_id, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, processing_id, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�������X�e�[�^�X�X�V
#
# �����T�v           �F1.�������e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �X�e�[�^�X
#                      �X�V�J������
#                      �X�V����
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column, time, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # �N�G�����쐬����
    sql = 'update fabric_info set status = %s, %s =\'%s\' where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, time, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    # �X�e�[�^�X���X�V����
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �FAI���f���������t���O�擾
#
# �����T�v           �F1.�i�ԓo�^���e�[�u������AI���f���������t���O���擾����B
#
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      AI���f���������t���O
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
## UPD 20200716 NES ���� START
#def select_aimodel_flag(conn, cur, product_name):
def select_aimodel_flag(conn, cur, product_name, fabric_name, inspection_num, inspection_starttime, unit_num):

    func_name = sys._getframe().f_code.co_name
    # �N�G�����쐬����
    #sql = 'select ai_model_non_inspection_flg from mst_product_info where product_name = \'%s\'' % product_name
    sql = 'select ai_model_non_inspection_flg from inspection_info_header where product_name = \'%s\' and ' \
          'fabric_name = \'%s\' and inspection_num = %s and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, inspection_starttime, unit_num)
## UPD 20200716 NES ���� END
    logger.debug('[%s:%s] AI���f���������t���O�擾SQL %s' % (app_id, app_name, sql))
    # �i�ԓo�^���e�[�u������AI���f���������t���O���擾����B
    result, product_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    if product_info:
        ai_model_flag = product_info[0]
    else:
        ai_model_flag = None
    return result, ai_model_flag, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���쐬
#
# �����T�v           �F1.RAPID��͏��e�[�u�����쐬����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_rapid_table(conn, cur, fabric_name, inspection_num, inspection_date):
    func_name = sys._getframe().f_code.co_name
    # RAPID��͏��e�[�u�����쐬����B
    result, error, conn, cur = db_util.create_table(conn, cur, fabric_name, inspection_num, inspection_date,
                                                    logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             :�摜�t�@�C���Ď�
#
# �����T�v           �F1.�B���摜�t�H���_���ɂ���B���摜���X�g���擾����
#
# ����               �F�B���摜�p�X
#                      �B���摜�t�@�C���p�^�[��
#                      �B���摜���X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���摜���X�g
# ------------------------------------------------------------------------------------
def monitor_image_file(image_path, file_pattern, logger):
    func_name = sys._getframe().f_code.co_name
    # �摜�t�@�C�����X�g���擾����B
    result, file_list, error = file_util.get_file_list(image_path, file_pattern, logger, app_id, app_name)
    return result, file_list, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�摜����
#
# �����T�v           �F1.100���P�ʂŏ���ID���̔Ԃ��A�t�H���_���쐬
#                      2.�쐬�����t�H���_�ɎB���摜���ړ�������
#
# ����               �F�B���摜���X�g
#                      �ړ���t�H���_
#                      �����ςݖ���
#                      RAPID�T�[�o�[�z�X�g��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���摜���X�g
#                      �����ςݖ���
# ------------------------------------------------------------------------------------
def separate_image(file_list, output_path, file_num, rapid_host_name, logger, fabric_name, inspection_num, date, original_image_path):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        image_file = file_list[:file_num]
        i = 0
        # �G���[���ʃ��b�Z�[�W
        networkpath_error = inifile.get('VALUE', 'networkpath_error')
        permission_error = inifile.get('VALUE', 'permission_error')
        max_retry_num = int(inifile.get('VALUE', 'retry_num'))

        # TODO �������[�v�ɂȂ�Ȃ��悤�A���s���X�g��ێ����A����𒴉߂�����G���[�Ƃ���i��������j
        # ���g�́Adict�iimage_file, retry_num�j�ŊǗ�����C���[�W
        ng_image_list = []
        # TODO �������[�v�ɂȂ�Ȃ��悤�A���s���X�g��ێ����A����𒴉߂�����G���[�Ƃ���i�����܂Łj

        logger.info('[%s:%s] �����ΏێB���摜 [�B���摜��] = [%s]' % (app_id, app_name, image_file))

        #  ���摜�ۊǃt�H���_���쐬����B
        tmp_result, error = file_util.make_directory(original_image_path, logger, app_id, app_name)

        # �t�@�C������臒l�̐�����while���[�v���̏��������s��
        while i < file_num:

            #  �����t�H���_���쐬����B
            tmp_result, error = file_util.make_directory(output_path, logger, app_id, app_name)

            if tmp_result:
                # �B���摜�����摜�ۊǃt�H���_�ɃR�s�[����B
                ctime = datetime.datetime.fromtimestamp(os.path.getctime(image_file[i])).timestamp()
                tmp_result, error = file_util.copy_file(image_file[i], original_image_path, logger,
                                                        app_id, app_name)
                win32_setctime.setctime(original_image_path + '\\' + os.path.basename(image_file[i]), ctime)

                # �B���摜�𕪊��t�H���_�Ɉړ�����B
                ctime = datetime.datetime.fromtimestamp(os.path.getctime(image_file[i])).timestamp()
                tmp_result, error = file_util.move_file(image_file[i], output_path, logger,
                                                        app_id, app_name)
                win32_setctime.setctime(output_path + '\\' + os.path.basename(image_file[i]), ctime)

                # �������� = True�̏ꍇ�A���̉摜���ړ�����
                if tmp_result is True:
                    i += 1
                # �������� = False�̏ꍇ�A�������s
                elif tmp_result == networkpath_error:
                    logger.error('[%s:%s] �摜�����Ɏ��s���܂����B '
                                 '[����, �����ԍ�, �������t, �摜��]=[%s, %s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, date, image_file[i]))

                    return result, file_list, file_num
                # ��L�ȊO�̏ꍇ�A�p�[�~�b�V�����G���[�Ƃ��đΏۉ摜�����X�g�̍Ō�ɒǉ��B
                elif tmp_result == permission_error:
                    # TODO ���s���X�g�̒��Ń��g���C�񐔂𒲍����Ă���ۂɂ̓G���[�I������B�i��������j
                    is_empty = True
                    for ng_image in ng_image_list:
                        if ng_image['image_file'] == image_file[i]:
                            # ��v������̂�����
                            is_empty = False
                            # ���g���C�񐔂̒��߃`�F�b�N
                            # MAX_RETRY_NUM�́A�{�ӏ��ł�MAX���g���C�񐔁i�ݒ�t�@�C���ɐ؂�o���������ǂ������j
                            if ng_image['retry_num'] >= max_retry_num:
                                # ���߂��Ă���ꍇ�̓G���[
                                logger.error('[%s:%s] �摜�����Ɏ��s���܂����B '
                                             '[����, �����ԍ�, �������t, �摜��]=[%s, %s, %s, %s]'
                                             % (app_id, app_name, fabric_name, inspection_num, date, image_file[i]))
                                return result, file_list, file_num
                            else:
                                # ���߂��Ă��Ȃ��ꍇ�̓J�E���g�A�b�v
                                cnt = ng_image['retry_num'] + 1
                                ng_image['retry_num'] = cnt
                                break
                    # ���s���X�g�ւ̒ǉ�
                    if is_empty:
                        # ���s���X�g�ɖ����ꍇ�͒ǉ�
                        tmp = {'image_file': image_file[i], 'retry_num': 1}
                        ng_image_list.append(tmp)
                    # TODO ���s���X�g�̒��Ń��g���C�񐔂𒲍����Ă���ۂɂ̓G���[�I������B�i�����܂Łj

                    image_file.append(image_file[i])
                    del image_file[i]
                else:
                    file_num = 0
                    return result, file_list, file_num
            else:
                logger.error('[%s:%s] �����t�H���_�쐬�����s���܂����B '
                             '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                             % (app_id, app_name, fabric_name, inspection_num, date))
                return result, file_list, file_num

        # �t�@�C�����X�g���珈�����s���������폜����
        del file_list[:file_num]

        result = True

    except Exception as error:
        # �G���[�ڍה�����s��
        error_detail.exception(error, logger, app_id, app_name)

    return result, file_list, file_num, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�摜�����m�F
#
# �����T�v           �F1.�B���摜���X�g�����m�F���āA�����𕪊򂳂���
#
# ����               �F�B���摜���X�g
#                      �摜������臒l
#                      �B����������
#                      �摜�o�̓p�X
#                      RAPID�T�[�o�[�z�X�g��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���摜���X�g
#                      �����ςݖ���
# ------------------------------------------------------------------------------------
def confirm_image_num(file_list, max_file, endtime, output_path, rapid_host_name, logger,
                      fabric_name, inspection_num, date, original_image_path):
    func_name = sys._getframe().f_code.co_name
    error = None
    # �B���摜���X�g���摜������臒l�ȏ�̏ꍇ
    if len(file_list) >= max_file:

        # �摜�����������s���B
        separete_image_res = separate_image(file_list, output_path, max_file, rapid_host_name, logger,
                                            fabric_name, inspection_num, date, original_image_path)

    # �B���摜���X�g���摜������臒l�����̏ꍇ�A�[�����𕪊����邩�̔�����s���B
    else:
        # �B�����������Ă��Ȃ��ꍇ�A�B���������Ƃ��ď����͍s��Ȃ��B
        if endtime is None:

            result = True
            return result, file_list, 0, error, func_name

        # �B�����������Ă���ꍇ�A�[��������������B
        else:

            separete_image_res = separate_image(file_list, output_path, len(file_list), rapid_host_name, logger,
                                                fabric_name, inspection_num, date, original_image_path)

    # �߂�l����A������̎B���摜���X�g�ƁA�����ϖ����𒊏o����B
    result = separete_image_res[0]
    after_file_list = separete_image_res[1]
    file_num = separete_image_res[2]
    error = separete_image_res[3]
    func_name = separete_image_res[4]

    return result, after_file_list, file_num, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�o�^
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���ɕi���A���ԁA�����ԍ��A����ID�A�X�e�[�^�X�A
#                         �������������ARAPID�T�[�o�[�z�X�g����o�^����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �����ԍ�
#                      ����ID
#                      RAPID�T�[�o�[�z�X�g��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def insert_processing_status(conn, cur, product_name, fabric_name, inspection_num,
                             processed_id, end_time, rapid_host_name, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    # �����X�e�[�^�X�i�B�������j
    status = common_inifile.get('PROCESSING_STATUS', 'separate_end')
    # �N�G�����쐬����B
    sql = 'insert into processing_status (product_name, fabric_name, inspection_num, ' \
          'processing_id, status, split_endtime, rapid_host_name, imaging_starttime, unit_num) values ' \
          '(\'%s\', \'%s\', %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\')' \
          % (product_name, fabric_name, inspection_num, processed_id, status, end_time, rapid_host_name,
             imaging_starttime, unit_num)

    logger.debug('[%s:%s] �����X�e�[�^�X�o�^SQL %s' % (app_id, app_name, sql))
    # �����X�e�[�^�X�e�[�u���Ƀf�[�^��o�^����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�����P�ʕ����}���`�X���b�h���s
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���ɕi���A���ԁA�����ԍ��A����ID�A�X�e�[�^�X�A
#                         �������������ARAPID�T�[�o�[�z�X�g����o�^����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �����ԍ�
#                      �摜�p�X
#                      �����摜�p�X
#                      RAPID�T�[�o�[�z�X�g��
#                      �����ςݖ���
#                      ����ID
#                      �B����������
#                      AI���f���t���O
#                      �f�B���N�g����
#                      �B������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����ϖ���
#                      ����ID
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def exec_separate_process_unit_multi_thread(product_name, fabric_name, inspection_num, image_dir,
                                            separate_image_dir, rapid_host_name, processed_num, processed_id,
                                            imaging_endtime, ai_model_flag, base_dir_name, date, imaging_starttime,
                                            unit_num):
    result = False
    conn = None
    cur = None
    file_list = 0
    error = None
    func_name = sys._getframe().f_code.co_name
    ### ���O�ݒ�
    logger_name = "separate_process_unit_" + str(rapid_host_name)
    logger_subprocess = logging.getLogger(logger_name)
    try:

        logger_subprocess.debug('[%s:%s] %s�}���`�X���b�h�������J�n���܂��B �z�X�g��=[%s]' %
                                (app_id, app_name, app_name, rapid_host_name))

        max_file = int(inifile.get('VALUE', 'max_file'))
        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        al_model_hostname = inifile.get('RAPID_SERVER', 'al_model_hostname')

        # DB���ʋ@�\���Ăяo���āADB�ɐڑ�����B
        logger_subprocess.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
        tmp_result, error, conn, cur, func_name = create_connection()

        if tmp_result:
            logger_subprocess.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger_subprocess.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

        # ���O�ݒ�Ǎ�
        logger_subprocess.info(
            '[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' % (
                app_id, app_name, app_name, fabric_name, inspection_num, date))
        logger_subprocess.debug('[%s:%s] �B���摜���X�g�̎擾���J�n���܂��B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))

        #  �B���摜�Ď��t�H���_���쐬����B
        tmp_result, error = file_util.make_directory(image_dir, logger, app_id, app_name)
        # �擾����摜�����`����B

        image_file_name = "\\*" + fabric_name + "_" + date + "_" + str(inspection_num).zfill((2)) + image_file_pattern
        # �B���摜�i�[�t�H���_���ɂ���B���摜�̃��X�g���擾����
        tmp_result, file_list, error, func_name = monitor_image_file(image_dir, image_file_name, logger_subprocess)

        if tmp_result:
            logger_subprocess.debug('[%s:%s] �B���摜���X�g�̎擾���I�����܂����B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))
            pass
        else:
            logger_subprocess.error('[%s:%s] �B���摜���X�g�̎擾�����s���܂����B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            # �B���摜���X�g�����݂���ꍇ�A�B���摜�̕����������s���B
        if len(file_list) > 0:
            # ����ID�ő�l���擾����B
            logger_subprocess.debug('[%s:%s] ����ID�ő�l�̎擾���J�n���܂��B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))
            tmp_result, max_processed_id, error, conn, cur, func_name = \
                select_processing_status_info(conn, cur, fabric_name, inspection_num, rapid_host_name,
                                              imaging_starttime, unit_num, logger_subprocess)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
                if max_processed_id is None:
                    pass
                else:
                    processed_id = int(max_processed_id[0]) + 1
                conn.commit()
            else:
                logger_subprocess.error('[%s:%s] ����ID�ő�l�̎擾�Ɏ��s���܂����B', app_id, app_name)
                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            logger_subprocess.debug('[%s:%s] �B���摜���X�g�����݂��܂��B ���X�g��=[%s] �z�X�g��=[%s]'
                                    % (app_id, app_name, len(file_list), rapid_host_name))
            # �B���摜���X�g������������臒l�Ŋ���
            file_length = (math.ceil(len(file_list) / max_file))
            for x in range(file_length):

                # �B���摜���X�g�����m�F���A�����P�ʂŎB���摜�𕪊�����
                logger_subprocess.debug('[%s:%s] �B���摜�̕������J�n���܂��B ����ID=[%s] �z�X�g��=[%s]'
                                        % (app_id, app_name, processed_id, rapid_host_name))

                # ���摜�ۊǃp�X�̍쐬
                original_image_path = image_dir + "\\" + 'ORG' + "\\" + base_dir_name
                # �����t�H���_�p�X�̍쐬
                separate_image_path = separate_image_dir + "\\" + str(processed_id)

                # �B�������m�F���s���A�B���摜�𕪊�����B
                tmp_result, file_list, file_num, error, func_name = \
                    confirm_image_num(file_list, max_file, imaging_endtime, separate_image_path, rapid_host_name,
                                      logger_subprocess, fabric_name, inspection_num, date, original_image_path)

                end_time = datetime.datetime.now()

                # confirm_filenum�i�j�̌���=True�̏ꍇ
                # �B���摜100���ȏ� �܂��� �B���摜100���ȉ����B�������̂��߁A�㑱�������s��
                if tmp_result is True and file_num != 0:
                    logger_subprocess.debug('[%s:%s] �B���摜�̕������I�����܂����B ����ID=[%s] ��������=[%s]  �z�X�g��=[%s]'
                                            % (app_id, app_name, processed_id, file_num, rapid_host_name))

                    logger_subprocess.debug('[%s:%s] �����X�e�[�^�X�̓o�^���J�n���܂��B' % (app_id, app_name))
                    # �����X�e�[�^�X�e�[�u���Ƀf�[�^��o�^����B
                    tmp_result, error, conn, cur, func_name = insert_processing_status(conn, cur, product_name,
                                                                                       fabric_name,
                                                                                       inspection_num, processed_id,
                                                                                       end_time,
                                                                                       rapid_host_name,
                                                                                       imaging_starttime, unit_num,
                                                                                       logger_subprocess)
                    if tmp_result:
                        logger_subprocess.debug('[%s:%s] �����X�e�[�^�X�̓o�^���I�����܂����B �z�X�g��=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                        conn.commit()
                    else:
                        logger_subprocess.error('[%s:%s] �����X�e�[�^�X�̓o�^�����s���܂����B '
                                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                                % (app_id, app_name, fabric_name, inspection_num, date))
                        conn.rollback()
                        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    logger_subprocess.debug('[%s:%s] �摜���T�C�Y�@�\���J�n���܂��B �z�X�g��=[%s]'
                                            % (app_id, app_name, rapid_host_name))
                    # ���T�C�Y�������Ăяo���āA�摜�̃��T�C�Y���s��
                    tmp_result, conn, cur, error, func_name = \
                        resize_image.main(conn, cur, fabric_name, inspection_num, processed_id, rapid_host_name,
                                          separate_image_path, ai_model_flag, date, imaging_starttime, unit_num)
                    if tmp_result:
                        logger_subprocess.debug('[%s:%s] �摜���T�C�Y�@�\���I�����܂����B �z�X�g��=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                    else:
                        logger_subprocess.error('[%s:%s] �摜���T�C�Y�@�\�����s���܂����B '
                                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                                % (app_id, app_name, fabric_name, inspection_num, date))
                        func_name = '305main'
                        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    if ai_model_flag == 1:

                        move_file_pattern = "\\" + image_file_pattern
                        logger_subprocess.debug('[%s:%s] RAPID�w�K�p�摜���X�g�̎擾���J�n���܂��B �z�X�g��=[%s]'
                                                % (app_id, app_name, rapid_host_name))
                        tmp_result, move_file_list, error = \
                            file_util.get_file_list(separate_image_path, move_file_pattern,
                                                    logger_subprocess, app_id, app_name)

                        if tmp_result:
                            logger_subprocess.debug('[%s:%s] RAPID�w�K�p�摜���X�g�̎擾���I�����܂����B �z�X�g��=[%s]'
                                                    % (app_id, app_name, rapid_host_name))
                            result_list = []
                            ai_model_path = "\\\\" + al_model_hostname + "\\" + base_dir_name + "\\"
                            logger_subprocess.debug('[%s:%s] RAPID�w�K�p�摜�̈ړ����J�n���܂��B �z�X�g��=[%s]'
                                                    % (app_id, app_name, rapid_host_name))
                            tmp_result, error = file_util.make_directory(ai_model_path, logger_subprocess, app_id,
                                                                         app_name)
                            if tmp_result:
                                pass
                            else:
                                logger_subprocess.error('[%s:%s] RAPID�w�K�p�摜�̈ړ������s���܂����B '
                                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                                        % (app_id, app_name, fabric_name, inspection_num, date))
                                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                            for i in range(len(move_file_list)):
                                tmp_result, error = file_util.move_file(move_file_list[i], ai_model_path,
                                                                        logger_subprocess, app_id, app_name)
                                result_list.append(tmp_result)

                            if False in result_list:
                                logger_subprocess.error('[%s:%s] RAPID�w�K�p�摜�̈ړ������s���܂����B '
                                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                                        % (app_id, app_name, fabric_name, inspection_num, date))
                                return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                            else:
                                logger_subprocess.debug('[%s:%s] RAPID�w�K�p�摜�̈ړ����I�����܂����B �z�X�g��=[%s]'
                                                        % (app_id, app_name, rapid_host_name))
                        else:
                            logger_subprocess.error('[%s:%s] RAPID�w�K�p�摜���X�g�̎擾�����s���܂����B '
                                                    '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                                    % (app_id, app_name, fabric_name, inspection_num, date))
                            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name
                    else:
                        pass

                    # ����ID���Z�A�������ϖ����ɏ����ϖ��������Z����
                    processed_id += 1
                    processed_num += file_num

                    # �B���������̂��߁A�[�����̏����͍s��Ȃ������ꍇ
                elif tmp_result is True and file_num == 0:
                    logger_subprocess.info('[%s:%s] �B���������̂��߁A�B���摜�̕������I�����܂����B [����ID, ����������]=[%s, %s]'
                                           % (app_id, app_name, (processed_id - 1), len(file_list)))
                    processed_num += file_num
                    result = True
                    return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

                    # ��������=False�̏ꍇ
                else:
                    logger_subprocess.error('[%s:%s] �B���摜�̕��������s���܂����B '
                                            '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                            % (app_id, app_name, fabric_name, inspection_num, date))
                    processed_num += file_num
                    return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

            result = True
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

        # �B���摜���X�g�����݂��Ȃ��ꍇ�A�������I������
        else:
            logger_subprocess.info('[%s:%s] �B���摜�����݂��܂���B [����, �����ԍ�, �������t]=[%s, %s, %s]' % (
                app_id, app_name, fabric_name, inspection_num, date))
            result = True
            return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

    except:
        logger_subprocess.error('[%s:%s] �\�����Ȃ��G���[���������܂��� '
                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                % (app_id, app_name, fabric_name, inspection_num, date))
        logger_subprocess.error(traceback.format_exc())
        return result, processed_num, processed_id, rapid_host_name, file_list, error, func_name

    finally:
        # DB�ڑ��ς̍ۂ̓N���[�Y����
        logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
        close_connection(conn, cur)
        logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)


# ------------------------------------------------------------------------------------
# ������             �F�����ςݖ����X�V
#
# �����T�v           �F1.�������e�[�u���̏����ϖ��������݂̑������ϖ����ōX�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �����ϖ���
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def update_processed_num(conn, cur, fabric_name, inspection_num, processed_num, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # �N�G�����쐬����B
    sql = 'update fabric_info set processed_num = %s where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (processed_num, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �����ςݖ����X�VSQL %s' % (app_id, app_name, sql))
    # �������e�[�u���̏����ςݖ������X�V����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�������擾
#
# �����T�v           �F1.�������e�[�u�����瑍�B�������A�B�������������擾����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ���B������
#                      �B����������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    func_name = sys._getframe().f_code.co_name
    # �N�G�����쐬����B
    sql = 'select image_num, imaging_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    if fabric_info is not None:
        # �擾���ʂ��瑍�B�������ƎB�����������𒊏o����B
        image_num = fabric_info[0]
        imaging_endtime = fabric_info[1]
    else:
        image_num = imaging_endtime = None

    return result, image_num, imaging_endtime, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F���������m�F
#
# �����T�v           �F1.�ꌟ���ԍ��̏�������������s��
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ���B������
#                      �����ςݖ���
#                      �B����������
#                      AI���f���������t���O
#                      ���������m�F�t���O
#
# �߂�l             �F��������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def confirm_processed(conn, cur, fabric_name, inspection_num, image_num, processed_num, imaging_endtime, column, time,
                      ai_flag, confirm_errorflg, status, error_continue_num, imaging_starttime, unit_num,
                      file_list_num):
    func_name = sys._getframe().f_code.co_name
    error = None
    if imaging_endtime is None or file_list_num != 0:
        confirm_errorflg = 0
        result = 'continuance'
        return result, confirm_errorflg, conn, cur, error, func_name

    # �B���摜����=�����ϖ��� ���� �B�������������o�^����Ă���ꍇ�A�ȉ�����ɉ����Ĕ������e�[�u���̍X�V���s���B
    elif image_num == processed_num and imaging_endtime is not None:
        confirm_errorflg = 0

        # AI���f���������t���O����̏ꍇ�A�������e�[�u���̃X�e�[�^�X��{�ԗp�ōX�V����B
        if ai_flag == 0:
            update_status = status[0]

            # �������e�[�u���̃X�e�[�^�X���X�V����B
            result, error, conn, cur, tmp_func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                         update_status, column, time, imaging_starttime,
                                                                         unit_num)

            return result, confirm_errorflg, conn, cur, error, func_name
            # AI���f���������t���O����̏ꍇ�A�������e�[�u���̃X�e�[�^�X�������p�ōX�V����B
        else:
            update_status = status[1]

            # �������e�[�u���̃X�e�[�^�X���X�V����B
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

            return result, confirm_errorflg, conn, cur, error, func_name

    # �B���摜�������ϖ��� ���� �B�������������o�^����Ă���ꍇ�A�C�x���g���O�ƃp�g���C�g��_�ł�����B
    elif image_num != processed_num and imaging_endtime is not None:
        logger.warning('[%s:%s] �B�������Ə����ϖ������s��v�ł��B [�B������, �����ϖ���]=[%s, %s]', app_id, app_name, image_num, processed_num)
        # AI���f���������t���O����̏ꍇ�A�������e�[�u���̃X�e�[�^�X��{�ԗp�ōX�V����B
        if ai_flag == 0:
            update_status = status[0]

            # �������e�[�u���̃X�e�[�^�X���X�V����B
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

            # AI���f���������t���O����̏ꍇ�A�������e�[�u���̃X�e�[�^�X�������p�ōX�V����B
        else:
            update_status = status[1]

            # �������e�[�u���̃X�e�[�^�X���X�V����B
            result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                     update_status, column, time, imaging_starttime,
                                                                     unit_num)

        message = '�B�������Ə����ϖ������s��v�ł��B[�B������, �����ϖ���]=[%s, %s]' % (image_num, processed_num)
        error_util.write_eventlog_warning(app_name, message)

        # # �p�g���C�g�_��
        # logger.debug('[%s:%s] �p�g���C�g�_���������J�n���܂��B', app_id, app_name)

        # light_pattern = '010000'
        # result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)

        # if result:
        #     logger.debug('[%s:%s] �p�g���C�g�_���������I�����܂����B', app_id, app_name)
        # else:
        #     logger.warning('[%s:%s] �p�g���C�g�_�������Ɏ��s���܂����B', app_id, app_name)

        return result, confirm_errorflg, conn, cur, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�t�H���_�폜
#
# �����T�v           �F1.RAPID�w�K�p�摜���ړ������ۂɁA��̃t�H���_���폜����B
#
# ����               �F�폜�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def delete_dir(path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.delete_dir(path, logger, app_id, app_name)
    return result, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ؒf
#
# �����T�v           �F1.DB�Ƃ̐ڑ���ؒf����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def close_connection(conn, cur):
    func_name = sys._getframe().f_code.co_name
    # DB�ڑ���ؒf����B
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �F�����P�ʕ���
#
# �����T�v           �F1.�B���摜�������P�ʂŕ������A���T�C�Y�������s���B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    error = None
    func_name = sys._getframe().f_code.co_name
    error_list = None
    try:

        # �ݒ�t�@�C������l���擾����B
        error_file_name = inifile.get('ERROR_FILE', 'file')
        image_dir = inifile.get('FILE_PATH', 'file_path')
        separate_image_dir = inifile.get('FILE_PATH', 'image_dir')
        patlite_name = inifile.get('PATLITE', 'patlite_name')
        confirm_errorflg = int(inifile.get('VALUE', 'confirm_errorflg'))
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))
        end_status = [common_inifile.get('FABRIC_STATUS', 'separate_resize_end'),
                      common_inifile.get('FABRIC_STATUS', 'rapid_model')]
        separate_resize_start = common_inifile.get('FABRIC_STATUS', 'separate_resize_start')
        start_column = inifile.get('COLUMN', 'start')
        end_column = inifile.get('COLUMN', 'end')
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        ip_address = common_inifile.get('RAPID_SERVER', 'ip_address')
        ip_address_list = ip_address.split(',')

        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        all_processed_num = 0
        processed_id = 1
        processed_id_list = [processed_id for i in range(thread_num)]

        logger.info('[%s:%s] %s�@�\���N�����܂����B' % (app_id, app_name, app_name))
        start_time = datetime.datetime.now()

        while True:

            # DB���猟�������擾����
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
            tmp_result, error, conn, cur, func_name = create_connection()
            # �������ʊm�F�BTrue�̏ꍇ�ADB�ڑ��I��
            if tmp_result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂��B' % (app_id, app_name))
                pass
            # False�̏ꍇ�ADB�ڑ����s
            else:
                logger.debug('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] �������擾���J�n���܂��B' % (app_id, app_name))
            # �������e�[�u�����猟�������擾����
            result, fabric_info, error, conn, cur, func_name = select_inspection_info(conn, cur, unit_num)
            # �������ʊm�F�BTrue�̏ꍇ�A�������擾����
            if result:
                logger.debug('[%s:%s] �������擾���I�����܂����B' % (app_id, app_name))
                conn.commit()
            # False�̏ꍇ�A�������擾���s
            else:
                logger.debug('[%s:%s] �������擾�����s���܂����B' % (app_id, app_name))
                conn.rollback()
                sys.exit()

            if fabric_info is not None:

                # �擾�����f�[�^��ϐ��ɑ������
                product_name, fabric_name, inspection_num, status, starttime = \
                    fabric_info[0], fabric_info[1], str(fabric_info[2]), fabric_info[3], fabric_info[4]
                logger.debug('[%s:%s] ������񂪑��݂��܂��B [�i��=%s] [����=%s] [�����ԍ�=%s]' %
                             (app_id, app_name, product_name, fabric_name, inspection_num))

                # �����J�n�������擾���A�B���摜�p�X���쐬����
                inspection_date = str(starttime.strftime('%Y%m%d'))
                base_dir_name = product_name + "_" + fabric_name + "_" + inspection_date + "_" + str(
                    inspection_num).zfill(
                    (2))
                separate_image_path = separate_image_dir + "\\" + base_dir_name
                separate_image_path_list = ["\\\\" + ip_address_list[i] + "\\" +
                                            separate_image_path for i in range(thread_num)]
                image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                  image_dir for i in range(thread_num)]

                # �������e�[�u���̃X�e�[�^�X���X�V����B
                logger.debug('[%s:%s] �������e�[�u���̃X�e�[�^�X�X�V���J�n���܂��B' % (app_id, app_name))
                result, error, conn, cur, func_name = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                         separate_resize_start, start_column,
                                                                         start_time, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] �������e�[�u���̃X�e�[�^�X�X�V���I�����܂����B' % (app_id, app_name))
                    conn.commit()

                else:
                    logger.error('[%s:%s] �������e�[�u���̃X�e�[�^�X�X�V�����s���܂����B '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # �i�ԏ��o�^�e�[�u������AI���f���������t���O�擾����
                # �i�ԏ��o�^�e�[�u�����ƁAAI���f���������t���O�̃J�������Ɋւ��Ă͌�ōŐV�� TODO
                logger.debug('[%s:%s] AI���f���������t���O�擾���J�n���܂��B' % (app_id, app_name))
                ## UPD 20200716 NES ���� START
                #result, ai_flag, error, conn, cur, func_name = select_aimodel_flag(conn, cur, product_name)
                inspection_starttime = starttime
                result, ai_flag, error, conn, cur, func_name = select_aimodel_flag(conn, cur, product_name, fabric_name,
                                                                                   inspection_num, inspection_starttime,
                                                                                   unit_num)
                ## UPD 20200716 NES ���� START

                if result:
                    logger.debug('[%s:%s] AI���f���������t���O�擾���I�����܂����B AI���f���������t���O=[%s]' % (app_id, app_name, ai_flag))
                    conn.commit()
                else:
                    logger.error('[%s:%s] AI���f���������t���O�擾�����s���܂����B '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                if ai_flag == 0:
                    # RAPID��͏��e�[�u�����쐬����
                    logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���J�n���܂��B' % (app_id, app_name))
                    result, error, conn, cur, func_name = create_rapid_table(conn, cur, fabric_name, inspection_num,
                                                                             inspection_date)
                    if result:
                        logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���I�����܂����B' % (app_id, app_name))
                        conn.commit()
                    else:
                        logger.error('[%s:%s] RAPID��͏��e�[�u���쐬�����s���܂����B '
                                     '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                        conn.rollback()
                        sys.exit()
                else:
                    pass

                logger.debug('[%s:%s] ���B�������A�B�����������̎擾���J�n���܂��B' % (app_id, app_name))
                result, image_num, imaging_endtime, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] ���B�������A�B�����������̎擾���I�����܂����B' % (app_id, app_name))
                    conn.commit()
                else:
                    logger.error('[%s:%s] ���B�������A�B�����������̎擾�����s���܂����B  '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # �}���`�X���b�h�p�̕ϐ���`
                result_list = []
                error_list = []
                conn_status = str(conn)
                processed_num = 0
                file_list_num = 0

                # �}���`�X���b�h���s
                logger.info('[%s:%s] �}���`�v���Z�X�ŏ����P�ʕ������J�n���܂��B [����, �����ԍ�,�@��������]=[%s, %s, %s]' %
                            (app_id, app_name, fabric_name, inspection_num, inspection_date))
                with ProcessPoolExecutor() as executor:
                    func_list = []
                    for i in range(thread_num):
                        # �X���b�h���s
                        func_list.append(
                            executor.submit(
                                exec_separate_process_unit_multi_thread,
                                product_name, fabric_name, inspection_num, image_dir_list[i],
                                separate_image_path_list[i],
                                rapid_hostname_list[i], processed_num, processed_id_list[i], imaging_endtime,
                                ai_flag, base_dir_name, inspection_date, starttime, unit_num))
                    for i in range(thread_num):
                        # �X���b�h�߂�l���擾
                        result_list.append(func_list[i].result())

                # �}���`�X���b�h���s���ʂ��m�F����B
                for i, multi_result in enumerate(result_list):
                    # ��������=True�̏ꍇ
                    if multi_result[0] is True:
                        host_name = multi_result[3]
                        logger.debug('[%s:%s] �}���`�X���b�h�ł̏����P�ʕ������I�����܂����B �z�X�g��=[%s]' %
                                     (app_id, app_name, host_name))
                        all_processed_num += multi_result[1]
                        processed_id_list[i] = multi_result[2]
                        file_list_num += len(multi_result[4])
                        conn_str = str(conn)
                        confirm_close = conn_str.split(',')[1]

                    # ��������=False�̏ꍇ
                    else:
                        host_name = multi_result[3]
                        error = multi_result[5]
                        func_name = multi_result[6]
                        logger.error('[%s:%s] �}���`�X���b�h�ł̏����P�ʕ��������s���܂����B '
                                     '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                        error_list.extend([host_name, error, func_name])
                        sys.exit()

                if len(error_list) > 0:
                    sys.exit()
                else:
                    pass
                logger.info('[%s:%s] �}���`�X���b�h�ł̏����P�ʕ������I�����܂����B '
                            '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                            % (app_id, app_name, fabric_name, inspection_num, inspection_date))

                conn_str = str(conn)
                confirm_close = conn_str.split(',')[1]

                logger.debug('[%s:%s] ���������ςݖ����X�V���J�n���܂��B' % (app_id, app_name))
                result, error, conn, cur, func_name = update_processed_num(conn, cur, fabric_name, inspection_num,
                                                                           all_processed_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] ���������ςݖ����X�V���I�����܂����B', app_id, app_name)
                    conn.commit()
                else:
                    logger.error('[%s:%s] ���������ςݖ����X�V�����s���܂����B '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # ������������
                # �������e�[�u������A���B�������A�����ϖ����A�B�������������擾����
                logger.debug('[%s:%s] ���B�������A�B�����������̎擾���J�n���܂��B' % (app_id, app_name))
                result, image_num, imaging_endtime, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] ���B�������A�B�����������̎擾���I�����܂����B' % (app_id, app_name))
                    conn.commit()
                else:
                    logger.error('[%s:%s] ���B�������A�B�����������̎擾�����s���܂����B '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                end_time = datetime.datetime.now()

                logger.debug('[%s:%s] ��������������J�n���܂��B' % (app_id, app_name))
                # ��������������s���B
                result, confirm_errorflg, conn, cur, error, func_name = \
                    confirm_processed(conn, cur, fabric_name, inspection_num, image_num, all_processed_num,
                                      imaging_endtime, end_column, end_time, ai_flag, confirm_errorflg, end_status,
                                      error_continue_num, starttime, unit_num, file_list_num)

                # �����������茋��=True�̏ꍇ�A1�����ԍ��̏����P�ʕ����͊���
                if result is True:
                    logger.debug('[%s:%s] �����������肪�I�����܂����B' % (app_id, app_name))
                    all_processed_num = 0

                    if ai_flag == 1:
                        # logger.debug('[%s:%s] ��t�H���_���폜���܂��B' % (app_id, app_name))
                        # for separate_path in separate_image_path_list:
                        #     result, error, func_name = delete_dir(separate_path)
                        #     if result:
                        #         logger.debug('[%s:%s] ��t�H���_�̍폜���I�����܂����B [�t�H���_�p�X=%s]' %
                        #                      (app_id, app_name, separate_path))
                        #     else:
                        #         logger.error('[%s:%s] ��t�H���_�̍폜�����s���܂����B  '
                        #                      '[����, �����ԍ�, �������t, �t�H���_�p�X]=[%s, %s, %s, %s]'
                        #                      % (app_id, app_name, fabric_name, inspection_num, inspection_date,
                        #                         separate_path))
                        #         sys.exit()
                        # �p�g���C�g�_��������
                        logger.debug('[%s:%s] �p�g���C�g�_���������J�n���܂��B', app_id, app_name)
                        light_pattern = '001000'
                        result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)
                        if result:
                            logger.debug('[%s:%s] �p�g���C�g�_���������I�����܂����B', app_id, app_name)
                        else:
                            logger.warning('[%s:%s] �p�g���C�g�_�������Ɏ��s���܂����B', app_id, app_name)

                    else:
                        pass

                    logger.info('[%s:%s] %s�����͐���ɏI�����܂����B '
                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                % (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                    conn.commit()
                    processed_id_list = [processed_id for i in range(len(processed_id_list))]
                    confirm_errorflg = 0

                # �����������茋��=�����p���̏ꍇ
                elif result == 'continuance':
                    logger.debug('[%s:%s] �����������肪�I�����܂����B' % (app_id, app_name))
                    logger.info('[%s:%s] �B�����������̂��߁A %s�������I�����܂��B '
                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                % (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                    close_connection(conn, cur)
                    logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B' % (app_id, app_name))

                    time.sleep(sleep_time)
                    continue

                # �����������茋��=False�̏ꍇ�A������������Ŏ��s
                else:
                    logger.error('[%s:%s] �����������肪���s���܂����B '
                                 '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                 % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                    conn.rollback()
                    sys.exit()

                # DB�ڑ���ؒf����
                logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                close_connection(conn, cur)
                logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B' % (app_id, app_name))

                time.sleep(sleep_time)
                continue

            else:
                logger.info('[%s:%s] ������񂪑��݂��܂���B' % (app_id, app_name))
                logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                close_connection(conn, cur)
                logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B' % (app_id, app_name))

                all_processed_num = 0

                time.sleep(sleep_time)
                continue


    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))

        logger.debug('[%s:%s] �G���[�ڍׂ��擾���܂��B' % (app_id, app_name))

        if error_list > 0:
            for list in error_list:
                error_message, error_id = error_detail.get_error_message(list[1], app_id, list[2])
                logger.error('[%s:%s] %s [�z�X�g��, �G���[�R�[�h]=[%s, %s]' % (app_id, app_name, error_message, list[0], error_id))

                event_log_message = '[�@�\��, �z�X�g��, �G���[�R�[�h]=[%s, %s, %s] %s' % (app_name, list[0], error_id, error_message)
                error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)
        else:
            error_message, error_id = error_detail.get_error_message(error, app_id, func_name)
            logger.error('[%s:%s] %s [�G���[�R�[�h:%s]' % (app_id, app_name, error_message, error_id))

            event_log_message = '[�@�\��, �G���[�R�[�h]=[%s, %s] %s' % (app_name, error_id, error_message)
            error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)

    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))

        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))

    finally:
        if conn is not None:
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    import multiprocessing

    multiprocessing.freeze_support()
    main()
