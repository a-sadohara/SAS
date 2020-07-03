# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\312  �s�ԎB�������`�F�b�N
# ----------------------------------------
import codecs
import csv
import re
import sys
from concurrent.futures.process import ProcessPoolExecutor
import configparser
import logging.config
import time
import traceback
import os
import logging.handlers

import error_detail
import file_util
import db_util
import error_util
import custom_handler

logging.handlers.CustomTimedRotatingFileHandler = custom_handler.ParallelTimedRotatingFileHandler

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_check_image_num.conf", disable_existing_loggers=False)
logger = logging.getLogger("check_image_num")

# �@�\304�ݒ�Ǎ�
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_image_num_config.ini', 'SJIS')
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
    sql = 'select fi.product_name, fi.fabric_name, fi.inspection_num, fi.imaging_starttime, ii.inspection_direction from '\
          'fabric_info as fi, inspection_info_header as ii ' \
          'where fi.unit_num = \'%s\' and fi.imaging_endtime IS NULL and fi.status != 0 and fi.product_name = ii.product_name and '\
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and '\
          'fi.imaging_starttime = ii.start_datetime order by imaging_starttime asc' \
          % (unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����猟�������擾����B
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, fabric_info, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# ������             �F�����s���擾
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
def select_inspection_line(conn, cur, product_name, fabric_name, inspection_num, inspection_date, unit_num):
    func_name = sys._getframe().f_code.co_name
    #  �N�G�����쐬����
    sql = 'select inspection_start_line from inspection_info_header ' \
          'where unit_num = \'%s\' and product_name = \'%s\' and fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and cast(start_datetime as date) = \'%s\' and branch_num = 1' % (unit_num, product_name, fabric_name, inspection_num, inspection_date)

    logger.debug('[%s:%s] �����s���擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����猟�������擾����B
    result, inspection_start_line, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, inspection_start_line, error, conn, cur, func_name


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
    sql = 'select imaging_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, fabric_info, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    if fabric_info is not None:
        # �擾���ʂ���B�����������𒊏o����B
        imaging_endtime = fabric_info[0]
    else:
        imaging_endtime = None

    return result, imaging_endtime, error, conn, cur, func_name

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
#                      �f�B���N�g����
#                      �B������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����ϖ���
#                      ����ID
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def exec_check_image_num_multi_thread(product_name, fabric_name, inspection_num, image_dir,
                                            move_image_dir, rapid_host_name, inspection_date,
                                            after_tmp_file_info, line_info_index, last_flag):
    result = False
    check_image_list = None
    error = None
    func_name = sys._getframe().f_code.co_name
    ### ���O�ݒ�
    logger_name = "check_image_num_" + str(rapid_host_name)
    logger_subprocess = logging.getLogger(logger_name)
    try:

        logger_subprocess.debug('[%s:%s] %s�}���`�v���Z�X�������J�n���܂��B �z�X�g��=[%s]' %
                                (app_id, app_name, app_name, rapid_host_name))

        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        camera_key = 'cam_' + str(rapid_host_name[-4:]).lower()
        camera_num = common_inifile.get('CAMERA', camera_key)

        # ���O�ݒ�Ǎ�
        logger_subprocess.info(
            '[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' % (
                app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
        logger_subprocess.debug('[%s:%s] �B���摜���X�g�̎擾���J�n���܂��B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))

        # �擾����摜�����`����B
        image_file_name = "\\*" + fabric_name + "_" + inspection_date + "_" + str(inspection_num).zfill((2)) + image_file_pattern
        # �B���摜�i�[�t�H���_���ɂ���B���摜�̃��X�g���擾����
        tmp_result, file_list, error, func_name = monitor_image_file(image_dir, image_file_name, logger_subprocess)

        if tmp_result:
            logger_subprocess.debug('[%s:%s] �B���摜���X�g�̎擾���I�����܂����B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))
            logger_subprocess.debug('[%s:%s] �B���摜���X�g �B���摜���X�g=[%s] �z�X�g��=[%s]' % (app_id, app_name, file_list, rapid_host_name))
            pass
        else:
            logger_subprocess.error('[%s:%s] �B���摜���X�g�̎擾�����s���܂����B �z�X�g��=[%s]' % (app_id, app_name, rapid_host_name))
            return result, rapid_host_name, file_list, error, func_name

        if last_flag == 0:
            if len(after_tmp_file_info) == 1 or line_info_index == 0:
                check_target_front_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index-1][0])[6])
                check_target_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[6])
                face = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[4])
                check_image_list = [x for x in file_list if int(re.split('[._]', x.split('\\')[-1])[6]) < check_target_line_image_num and int(re.split('[._]', x.split('\\')[-1])[4]) == face]
            else:
                check_target_front_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index-1][0])[6])
                check_target_line_image_num = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[6])
                face = int(re.split('[._]', after_tmp_file_info[line_info_index][0])[4])
                check_image_list = [x for x in file_list if check_target_front_line_image_num <= int(re.split('[._]', x.split('\\')[-1])[6]) and
                                int(re.split('[._]', x.split('\\')[-1])[6]) < check_target_line_image_num and int(re.split('[._]', x.split('\\')[-1])[4]) == face]

            logger_subprocess.debug('[%s:%s] �s�ԓ��摜���X�g�擾�B �摜���X�g=[%s], �z�X�g��=[%s]' % (app_id, app_name, check_image_list, rapid_host_name))

            if len(check_image_list) < int(after_tmp_file_info[line_info_index][5]) * int(camera_num):
                result = 'image_shortage'
            else:
                result = True

            if len(check_image_list) == 0:
                result = True
                return result, rapid_host_name, check_image_list, error, func_name
            else:
                pass

            tmp_result, error, func_name = move_file(check_image_list, move_image_dir)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] �B���摜�̈ړ����I�����܂����B �z�X�g��=[%s]'
                                        % (app_id, app_name, rapid_host_name))
            else:
                logger_subprocess.error('[%s:%s] �B���摜�̈ړ������s���܂����B '
                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                raise Exception
        else:
            check_image_list = file_list
            if len(check_image_list) == 0:
                result = True
                return result, rapid_host_name, check_image_list, error, func_name
            else:
                pass

            tmp_result, error, func_name = move_file(file_list, move_image_dir)
            if tmp_result:
                logger_subprocess.debug('[%s:%s] �B���摜�̈ړ����I�����܂����B �z�X�g��=[%s]'
                                        % (app_id, app_name, rapid_host_name))
                result = True

            else:
                logger_subprocess.error('[%s:%s] �B���摜�̈ړ������s���܂����B '
                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))


        return result, rapid_host_name, check_image_list, error, func_name

    except Exception as error:
        logger_subprocess.error('[%s:%s] �\�����Ȃ��G���[���������܂��� '
                                '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                % (app_id, app_name, fabric_name, inspection_num, inspection_date))
        logger_subprocess.error(traceback.format_exc())
        result = False
        return result, rapid_host_name, check_image_list, error, func_name

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
# �֐���             �F�t�@�C���擾
#
# �����T�v           �F1.�B�������ʒm�t�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F�B�������ʒm�t�@�C���i�[�t�H���_�p�X
#                      �B�������ʒm�t�@�C����
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B�������ʒm�t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name, network_path_error, flag):
    result = False
    sorted_files = None
    error = None
    func_name = sys._getframe().f_code.co_name
    if flag == 'tmp':
        message = 'tmp�t�@�C��'
    elif flag == 'scan':
        message = '�B�������ʒm�t�@�C��'
    else:
        message = '���W�}�[�N�ǎ挋�ʃt�@�C��'

    try:
        logger.debug('[%s:%s] %s�i�[�t�H���_�p�X=[%s]', app_id, app_name, message, file_path)
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result == True:
            # ������
            pass
        elif tmp_result == network_path_error:
            # ���s��
            logger.debug("[%s:%s] %s�i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name, message)
            return tmp_result, sorted_files, error, func_name
        else:
            # ���s��
            logger.error("[%s:%s] %s�i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name, message)
            return result, sorted_files, error, func_name

        # �擾�����t�@�C���p�X���t�@�C�����Ń\�[�g����i�Â����ɏ������邽�߁j
        sorted_files = sorted(file_list)

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, sorted_files, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �F�t�@�C���Ǎ�
#
# �����T�v           �F1.�B�������ʒm�t�@�C����Ǎ���
#
# ����               �F�B�������ʒm�t�@�C���̃t�@�C���t���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���t�@�C����
#                      ���
#                      ���W��
#                      ���W��
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    regimark_info = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # ���W�}�[�N�ǎ挋�ʃt�@�C���p�X����t�@�C�������擾���A���ԁA�����ԍ����擾����
        # �Ȃ��A�t�@�C�����́uRM_�i��_����_�����ԍ�_���t_������_���W�}�[�N���.CSV�v��z�肵�Ă���

        # ���W�}�[�N�ǎ挋�ʃt�@�C������A���ڂ��擾����
        with codecs.open(file, "r", "SJIS") as f:
            notification = [re.sub('\r', '', s[:-1]).split(',') for s in f.readlines()][1:]

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, notification, error, func_name

# ------------------------------------------------------------------------------------
# ������             �F�B���摜�ړ�
#
# �����T�v           �F1.�B���摜���@�\304�ŊĎ����Ă���t�H���_�Ɉړ�����B
#
# ����               �F�B���摜
#                      �ړ���t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    func_name = sys._getframe().f_code.co_name
    result = None
    error = None

    for file in target_file:
        # �t�@�C���ړ�
        result, error = file_util.move_file(file, move_dir, logger, app_id, app_name)

    return result, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �Ftmp�t�@�C���o��
#
# �����T�v           �F1.tmp�t�@�C���ɏo�͂���B
#
# ����               �F�o�͐�p�X
#                      �i��
#                      ����
#                      �����ԍ�
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def write_checked_linenum_file(output_file_path, base_filename, inspection_direction, regimark_info, line_imagenum, tmp_files):
    result = False
    file_name = base_filename.replace('RM', 'LINECHECK_' + inspection_direction)
    tmp_file = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        if len(tmp_files) != 0:
            tmp_file = output_file_path + "\\" + file_name
            with codecs.open(tmp_file, "a", "SJIS") as f:
                for i in range(len(regimark_info)):
                    write_data = regimark_info[i][0] + ',' + regimark_info[i][1] + ',' + regimark_info[i][2] + ',' \
                                 + regimark_info[i][3] + ',' + regimark_info[i][4] + ',' + str(line_imagenum[i]) + ','
                    f.write(write_data)
                    f.write("\r\n")
        else:
            tmp_file = output_file_path + "\\" + file_name
            with codecs.open(tmp_file, "w", "SJIS") as f:
                f.write("�B���t�@�C����,���,���W��,���W��,�p���X,�s�Ԗ���,�`�F�b�N�t���O")
                f.write("\r\n")
                for i in range(len(regimark_info)):
                    write_data = regimark_info[i][0] + ',' + regimark_info[i][1] + ',' + regimark_info[i][2] + ',' \
                                 + regimark_info[i][3] + ',' + regimark_info[i][4] + ',' + str(line_imagenum[i]) + ','
                    f.write(write_data)
                    f.write("\r\n")

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, tmp_file, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �Ftmp�t�@�C���ҏW
#
# �����T�v           �F1.tmp�t�@�C���ɏo�͂���B
#
# ����               �F�o�͐�p�X
#                      �i��
#                      ����
#                      �����ԍ�
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def edit_checked_linenum_file(file_name, line_info_index, after_line_info):
    result = False
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        print(after_line_info)
        for i in range(len(after_line_info)):
            if i == line_info_index:
                after_line_info[i][-1] = '1'
            else:
                pass

        with codecs.open(file_name, "w", "SJIS") as f:
            f.write("�B���t�@�C����,���,���W��,���W��,�p���X,�s�Ԗ���,�`�F�b�N�t���O")
            f.write("\r\n")
            writer = csv.writer(f)
            writer.writerows(after_line_info)


        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, file_name, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �F�s�ԎB�������`�F�b�N
#
# �����T�v           �F1.���W�}�[�N�t�@�C����ǂݍ���
#                     2.N�s��N-1�s��
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def confirm_line_between_imagenum(regimark_info, tmp_file, read_tmp_file):
    result = False
    line_imagenum = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        if len(regimark_info) == 1 and (len(tmp_file) == 0):
            image_num = int(re.split('[._]', regimark_info[0][0])[6])
            line_imagenum.append(image_num - 1)
            regimark_info = regimark_info
            result = True
        elif len(regimark_info) == len(read_tmp_file):
            line_imagenum = []
            regimark_info = []
        else:
            print("sss")
            checked_line = len(read_tmp_file)
            print(checked_line)
            for i in range(checked_line, len(regimark_info)):
                front_line = int(re.split('[._]',  regimark_info[i-1][0])[6])
                line = int(re.split('[._]',  regimark_info[i][0])[6])
                line_imagenum.append(line - front_line)

            regimark_info = regimark_info[checked_line:]
            result = True

        return result, regimark_info, line_imagenum, error, func_name
    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)
        return result, regimark_info, line_imagenum, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�o�͐�t�H���_���݃`�F�b�N
#
# �����T�v           �F1.���W�}�[�N�ԋ���CSV���o�͂���t�H���_�����݂��邩�`�F�b�N����B
#                      2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�o�͐�t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �F�s�ԎB�������`�F�b�N
#
# �����T�v           �F1.���W�}�[�N�t�@�C����ǂݍ���
#                     2.N�s��N-1�s��
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    scaninfo_file = None
    error = None
    func_name = sys._getframe().f_code.co_name
    error_list = None
    check_result = []
    try:

        # �ݒ�t�@�C������l���擾����B
        error_file_name = inifile.get('ERROR_FILE', 'file')
        image_dir = inifile.get('FILE_PATH', 'file_path')
        move_image_dir = inifile.get('FILE_PATH', 'move_dir')
        tmp_file_dir = inifile.get('FILE_PATH', 'tmp_file')
        input_path = common_inifile.get('FILE_PATH', 'input_path')
        input_dir_name = inifile.get('FILE_PATH', 'input_file_path')
        file_name_pattern = inifile.get('FILE_PATTREN', 'file_name_pattern')
        scan_input_dir_name = inifile.get('FILE_PATH', 'input_scan_file_path')
        scan_file_name_pattern = inifile.get('FILE_PATTREN', 'scan_file_name_pattern')
        file_extension_pattern = inifile.get('FILE_PATTREN', 'file_extension_pattern')
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        ip_address = common_inifile.get('RAPID_SERVER', 'ip_address')
        ip_address_list = ip_address.split(',')
        # �l�b�g���[�N�p�X�G���[���b�Z�[�W
        network_path_error = inifile.get('VALUE', 'networkpath_error')
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s�@�\���N�����܂����B' % (app_id, app_name, app_name))
        # DB�֐ڑ�����B
        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
        tmp_result, error, conn, cur, func_name = create_connection()
        # True�̏ꍇ�ADB�ڑ��I��
        if tmp_result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂��B' % (app_id, app_name))
            pass
        # False�̏ꍇ�ADB�ڑ����s
        else:
            logger.debug('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
            raise Exception

        # tmp�t�@�C�����o�͂���t�H���_�̑��݂��m�F����
        logger.debug('[%s:%s] tmp�t�@�C���i�[�t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
        result, error, func_name = exists_dir(tmp_file_dir)

        if result:
            logger.debug('[%s:%s] tmp�t�@�C���i�[�t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
            pass
        else:
            logger.error('[%s:%s] tmp�t�@�C���i�[�t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�i�[��t�H���_��[%s]',
                         app_id, app_name, tmp_file_dir)
            raise Exception

        # ������(�`�F�b�N��)�̌��������m�F����B
        # �t�H���_����tmp�t�@�C�������݂��邩�m�F����B
        tmp_file = None
        logger.debug('[%s:%s] tmp�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        for i in range(error_continue_num):
            result, tmp_file, error, func_name = get_file(tmp_file_dir, '\\*.CSV', network_path_error, 'tmp')
            if result == True:
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] tmp�t�@�C���ɃA�N�Z�X�ł��܂���B', app_id, app_name)
                message = 'tmp�t�@�C���ɃA�N�Z�X�ł��܂���B'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] tmp�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                raise Exception

        if len(tmp_file) == 0:
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
                raise Exception

            if fabric_info is not None:
                # �擾�����f�[�^��ϐ��ɑ������
                print(fabric_info)
                product_name = fabric_info[0]
                fabric_name = fabric_info[1]
                inspection_num = str(fabric_info[2])
                inspection_date = str(fabric_info[3].strftime('%Y%m%d'))
                inspection_direction = fabric_info[4]
                logger.debug('[%s:%s] ������񂪑��݂��܂��B [�i��=%s] [����=%s] [�����ԍ�=%s]' %
                             (app_id, app_name, product_name, fabric_name, inspection_num))
            else:
                logger.info('[%s:%s] �`�F�b�N�Ώۂ����݂��܂���B' % (app_id, app_name))
                return True, scaninfo_file, 'continue', func_name

        else:
            basename = os.path.basename(tmp_file[0])
            file_name = re.split('[_.]', basename)
            product_name = file_name[2]
            fabric_name = file_name[3]
            inspection_num = file_name[4]
            inspection_date = file_name[5]
            inspection_direction = file_name[1]
            # �擾�����f�[�^��ϐ��ɑ������
            logger.debug('[%s:%s] �s�Ԗ����`�F�b�N���̌�����񂪑��݂��܂��B [�i��=%s] [����=%s] [�����ԍ�=%s]' %
                         (app_id, app_name, product_name, fabric_name, inspection_num))

        logger.debug('[%s:%s] �����s���擾���J�n���܂��B' % (app_id, app_name))
        # �������w�b�_�[�e�[�u�����猟���J�n�s�����擾����
        result, inspection_start_line, error, conn, cur, func_name = select_inspection_line(conn, cur, product_name,
                                                                                            fabric_name, inspection_num,
                                                                                            inspection_date, unit_num)
        # �������ʊm�F�BTrue�̏ꍇ�A�������擾����
        if result:
            logger.debug('[%s:%s] �����s���擾���I�����܂����B' % (app_id, app_name))
            conn.commit()
        # False�̏ꍇ�A�������擾���s
        else:
            logger.debug('[%s:%s] �����s���擾�����s���܂����B' % (app_id, app_name))
            conn.rollback()
            raise Exception

        join_file_name = [product_name, fabric_name, inspection_num, inspection_date]
        # ���W�}�[�N�t�@�C����ǂݍ���
        input_file_path = input_path + '\\' + input_dir_name

        scan_input_file_path = input_path + '\\' + scan_input_dir_name
        scan_file_pattern = '\\' + scan_file_name_pattern + '_' + '_'.join(join_file_name) + '*' + file_extension_pattern

        # �B�������ʒm�t�@�C���m�F�B
        logger.debug('[%s:%s] �B�������ʒm�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        for i in range(error_continue_num):
            result, scan_info, error, func_name = get_file(scan_input_file_path, scan_file_pattern,
                                                                network_path_error, 'scan')
            if result == True:
                logger.error('[%s:%s] �B�������ʒm�t�@�C�� %s', app_id, app_name, scan_info)
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] �B�������ʒm�t�@�C���ɃA�N�Z�X�ł��܂���B', app_id, app_name)
                message = '�B�������ʒm�t�@�C���ɃA�N�Z�X�ł��܂���B'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] �B�������ʒm�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                raise Exception

        # �������� S, X �̏ꍇ�A�I�����W�}�[�N�͓Ǎ��s�v
        if inspection_direction == 'S' or inspection_direction == 'X':
            file_pattern = '\\' + file_name_pattern + '_'  + '_'.join(join_file_name) + '_*1' + file_extension_pattern
        # �������� Y, R �̏ꍇ�A�I�����W�}�[�N�͓Ǎ��s�v
        else:
            file_pattern = '\\' + file_name_pattern + '_' + '_'.join(join_file_name) + '_*2' + file_extension_pattern

        scaninfo_file = scan_info
        print(file_pattern)
        print('�����ʒm�t�@�C����', len(scan_info))
        logger.debug('[%s:%s] ���W�}�[�N�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        for i in range(error_continue_num):
            result, regimark_files, error, func_name = get_file(input_file_path, file_pattern, network_path_error, 'regi')
            if result == True and len(regimark_files) == 0:
                logger.info('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�������݂��܂���B', app_id, app_name)
                if len(scaninfo_file) != 0:
                    return True, scaninfo_file, 'end', func_name
                else:
                    return True, scaninfo_file, 'continue', func_name

            elif result == True and len(regimark_files) != 0:
                break
            elif result == network_path_error:
                logger.warning('[%s:%s] ���W�}�[�N�t�@�C���ɃA�N�Z�X�ł��܂���B', app_id, app_name)
                message = '���W�}�[�N�t�@�C���ɃA�N�Z�X�ł��܂���B'
                error_util.write_eventlog_warning(app_name, message)
                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] ���W�}�[�N�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                raise Exception

        # ���W�}�[�N�ǎ挋�ʃt�@�C����Ǎ���
        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����J�n���܂��B', app_id, app_name)

        for file in sorted(regimark_files):
            base_filename = os.path.basename(file)
            regimark_face = re.split('_', (file.split('\\'))[-1])[5]
            result, regimark_info, error, func_name = read_file(file)
            if result:
                logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����I�����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                             app_id, app_name, file)
                logger.debug('[%s:%s] ���W�}�[�N��� : %s', app_id, app_name, regimark_info)
            else:
                logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                             app_id, app_name, file)

            if len(tmp_file) == 0:
                tmp_file_info = []
                pass
            else:
                read_tmp_file = [x for x in tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
                result, tmp_file_info, error, func_name = read_file(read_tmp_file[0])
                if result:
                    logger.debug('[%s:%s] tmp�t�@�C���̓Ǎ����I�����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, file)
                    logger.debug('[%s:%s] tmp�t�@�C������� : %s', app_id, app_name, tmp_file_info)
                else:
                    logger.error('[%s:%s] tmp�t�@�C���t�@�C���̓Ǎ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, file)

            result, regimark_info, line_imagenum ,error, func_name = confirm_line_between_imagenum(regimark_info, tmp_file, tmp_file_info)
            logger.debug('[%s:%s] �s�Ԗ����`�F�b�N���� %s %s', app_id, app_name, regimark_info, line_imagenum)

            # if len(regimark_info) == 0:
            #     pass
            # else:
            logger.debug('[%s:%s] tmp�t�@�C���̏o�͂��J�n���܂��B', app_id, app_name)
            result, output_tmp_file, error, func_name = write_checked_linenum_file(tmp_file_dir, base_filename,
                                                                            inspection_direction, regimark_info, line_imagenum, tmp_file)
            if result:
                logger.debug('[%s:%s] tmp�t�@�C���̏o�͂��I�����܂����B' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] tmp�t�@�C���̏o�͂Ɏ��s���܂����B' % (app_id, app_name))
                raise Exception

            logger.debug('[%s:%s] tmp�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
            for i in range(error_continue_num):
                result, after_tmp_file, error, func_name = get_file(tmp_file_dir, '\\*.CSV', network_path_error, 'tmp')
                if result == True:
                   break
                elif result == network_path_error:
                    logger.warning('[%s:%s] tmp�t�@�C���ɃA�N�Z�X�ł��܂���B', app_id, app_name)
                    message = 'tmp�t�@�C���ɃA�N�Z�X�ł��܂���B'
                    error_util.write_eventlog_warning(app_name, message)
                    time.sleep(sleep_time)
                    continue
                else:
                    logger.error('[%s:%s] tmp�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                    raise Exception

            read_tmp_file = [x for x in after_tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
            result, after_tmp_file_info, error, func_name = read_file(read_tmp_file[0])
            if result:
                logger.debug('[%s:%s] tmp�t�@�C���̓Ǎ����I�����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                             app_id, app_name, read_tmp_file[0])
                logger.debug('[%s:%s] tmp�t�@�C������� : %s', app_id, app_name, after_tmp_file_info)
            else:
                logger.error('[%s:%s] tmp�t�@�C���̓Ǎ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                             app_id, app_name, read_tmp_file[0])

            for i in range(len(after_tmp_file_info)):
                if after_tmp_file_info[i][6] == '1':
                    continue
                else:
                    result_list = []
                    error_list = []
                    move_image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                                move_image_dir for i in range(thread_num)]
                    image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                      image_dir for i in range(thread_num)]

                    line_info_index = i
                    func_list = []

                    with ProcessPoolExecutor() as executor:
                        for j in range(thread_num):
                            # �X���b�h���s
                            func_list.append(
                                executor.submit(
                                    exec_check_image_num_multi_thread,
                                    product_name, fabric_name, inspection_num, image_dir_list[j],
                                    move_image_dir_list[j], rapid_hostname_list[j], inspection_date,
                                    after_tmp_file_info, line_info_index, 0))

                        for k in range(thread_num):
                            # �X���b�h�߂�l���擾
                            result_list.append(func_list[k].result())
                            # �}���`�X���b�h���s���ʂ��m�F����B

                        for l, multi_result in enumerate(result_list):
                            # ��������=True�̏ꍇ
                            if multi_result[0] is True:
                                host_name = multi_result[1]
                                file_list = multi_result[2]
                                logger.debug('[%s:%s] �}���`�X���b�h�ł̍s�Ԗ����`�F�b�N���I�����܂����B �z�X�g��=[%s] �t�@�C����=[%s]' %
                                             (app_id, app_name, host_name, len(file_list)))
                            # ��������=image_shortage�̏ꍇ
                            elif multi_result[0] == 'image_shortage':
                                host_name = multi_result[1]
                                file_list = multi_result[2]
                                logger.error('[%s:%s] �}���`�X���b�h�ł̍s�Ԗ����̌������������Ă��܂��B �z�X�g��=[%s], �t�@�C�����X�g=[%s], �s�ԍ�=[%s]' %
                                             (app_id, app_name, host_name, file_list, int(line_info_index) -1 + int(inspection_start_line[0])))
                                logger.debug('[%s:%s] �G���[�t�@�C���o�͂��J�n���܂��B', app_id, app_name)
                                error_file_name = 'Image_on_line_' + \
                                                  str(int(line_info_index) -1 + int(inspection_start_line[0])) + '_is_lost.txt'
                                result = error_util.common_execute(error_file_name, logger, app_id, app_name)
                                if result:
                                    logger.debug('[%s:%s] �G���[�t�@�C���o�͂��I�����܂����B' % (app_id, app_name))
                                else:
                                    logger.error('[%s:%s] �G���[�t�@�C���o�͂����s���܂����B' % (app_id, app_name))
                                    logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))
                            # ��������=False�̏ꍇ
                            else:
                                host_name = multi_result[1]
                                error = multi_result[3]
                                func_name = multi_result[4]
                                logger.error('[%s:%s] �}���`�X���b�h�ł̍s�Ԗ����`�F�b�N�����s���܂����B '
                                             '[����, �����ԍ�, �������t]=[%s, %s, %s] �z�X�g��=[%s]'
                                             % (app_id, app_name, fabric_name, inspection_num, inspection_date, host_name))
                                error_list.extend([host_name, error, func_name])
                                raise Exception

                result, file_name, error, func_name = edit_checked_linenum_file(read_tmp_file[0], line_info_index, after_tmp_file_info)
                if result:
                    logger.debug('[%s:%s] tmp�t�@�C���̕ҏW���I�����܂����B:tmp�t�@�C����[%s]',
                                 app_id, app_name, file_name)
                else:
                    logger.error('[%s:%s] tmp�t�@�C���t�@�C���̕ҏW�Ɏ��s���܂����B:tmp�t�@�C����[%s]',
                                 app_id, app_name, file_name)

        if len(scan_info) == 0:
            return True, scaninfo_file, 'continue', func_name
        else:
            scaninfo_file = scan_info
            for file in regimark_files:
                base_filename = os.path.basename(file)
                regimark_face = re.split('_', (file.split('\\'))[-1])[5]
                result, regimark_info, error, func_name = read_file(file)
                if result:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����I�����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, regimark_files)
                    logger.debug('[%s:%s] ���W�}�[�N��� : %s', app_id, app_name, regimark_info)
                else:
                    logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, regimark_files)
                    raise Exception

                read_tmp_file = [x for x in after_tmp_file if regimark_face in re.split('_', x.split('\\')[-1])[6]]
                result, after_tmp_file_info, error, func_name = read_file(read_tmp_file[0])

                if len(regimark_info) == len(after_tmp_file_info):
                    check_result.append('OK')

                    tmp_file_move_dir = tmp_file_dir + '\\CHECKED'
                    # tmp�t�@�C�����o�͂���t�H���_�̑��݂��m�F����
                    logger.debug('[%s:%s] tmp�t�@�C���ړ���t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                    result, error, func_name = exists_dir(tmp_file_move_dir)

                    if result:
                        logger.debug('[%s:%s] tmp�t�@�C���ړ���t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                        pass
                    else:
                        logger.error('[%s:%s] tmp�t�@�C���ړ���t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�i�[��t�H���_��[%s]',
                                     app_id, app_name, tmp_file_move_dir)
                        raise Exception

                    result, error, func_name = move_file(read_tmp_file, tmp_file_move_dir)
                    if result:
                        logger.debug('[%s:%s] tmp�t�@�C���̈ړ����I�����܂����B' % (app_id, app_name))
                    else:
                        logger.error('[%s:%s] tmp�t�@�C���̈ړ������s���܂����B '
                                     '[����, �����ԍ�, �������t, ������]=[%s, %s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date, regimark_face))
                        raise Exception
                else:
                    pass

        if check_result[0] == 'OK' and check_result[1] == 'OK':
            result_list = []
            error_list = []
            move_image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                                   move_image_dir for i in range(thread_num)]
            image_dir_list = ["\\\\" + ip_address_list[i] + "\\" +
                              image_dir for i in range(thread_num)]
            after_tmp_file_info = []
            line_info_index = len(after_tmp_file_info)
            func_list = []

            with ProcessPoolExecutor() as executor:
                for j in range(thread_num):
                    # �X���b�h���s
                    func_list.append(
                        executor.submit(
                            exec_check_image_num_multi_thread,
                            product_name, fabric_name, inspection_num, image_dir_list[j],
                            move_image_dir_list[j], rapid_hostname_list[j], inspection_date,
                            after_tmp_file_info, line_info_index, 1))

                for k in range(thread_num):
                    # �X���b�h�߂�l���擾
                    result_list.append(func_list[k].result())
                    # �}���`�X���b�h���s���ʂ��m�F����B

                print(result_list)
                for l, multi_result in enumerate(result_list):
                    # ��������=True�̏ꍇ
                    if multi_result[0] is True:
                        host_name = multi_result[1]
                        file_list = multi_result[2]
                        logger.debug('[%s:%s] �}���`�X���b�h�ł̎B���摜�ړ����I�����܂����B �z�X�g��=[%s] �t�@�C����=[%s]' %
                                     (app_id, app_name, host_name, len(file_list)))
                    # ��������=image_shortage�̏ꍇ
                    elif multi_result[0] == 'image_shortage':
                        host_name = multi_result[1]
                        file_list = multi_result[2]
                        logger.error('[%s:%s] �}���`�X���b�h�ł̍s�Ԗ����̌������������Ă��܂��B �z�X�g��=[%s], �t�@�C�����X�g=[%s], �s�ԍ�=[%s]' %
                                     (app_id, app_name, host_name, file_list,
                                      int(line_info_index) + int(inspection_start_line[0])))
                    # ��������=False�̏ꍇ
                    else:
                        host_name = multi_result[1]
                        error = multi_result[3]
                        func_name = multi_result[4]
                        logger.error('[%s:%s] �}���`�X���b�h�ł̎B���摜�ړ������s���܂����B '
                                     '[����, �����ԍ�, �������t]=[%s, %s, %s] �z�X�g��=[%s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date, host_name))
                        error_list.extend([host_name, error, func_name])
                        raise Exception

            #
            return True, scaninfo_file, 'end', func_name

    except Exception as error:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)
        logger.error('[%s:%s] %s [�G���[�R�[�h:%s]' % (app_id, app_name, error_message, error_id))

        event_log_message = '[�@�\��, �G���[�R�[�h]=[%s, %s] %s' % (app_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)

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
