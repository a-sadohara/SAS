# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\311  �����m�摜�o�^
# ----------------------------------------

import configparser
from decimal import Decimal, ROUND_HALF_UP
import logging.config
import os
import re
import sys
import time
import traceback
import datetime
import shutil

import error_detail
import error_util
import db_util
import file_util

import register_ng_info_undetect
import compress_image_undetect

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_undetectedimage")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_undetectedimage_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# �֐���             �F�t�@�C���擾
#
# �����T�v           �F1.�����m�摜�ʒm�t�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F�����m�摜�ʒm�t�@�C���i�[�t�H���_�p�X
#                      �����m�摜�ʒm�t�@�C����
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����m�摜�ʒm�t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    result = False
    sorted_files = None

    try:

        # ���ʊ֐��Ŗ����m�摜�ʒm�t�@�C���i�[�t�H���_�����擾����
        file_list = None
        tmp_result, file_list, error = file_util.get_file_list(file_path + '\\', file_name, logger, app_id, app_name)

        if tmp_result:
            # ������
            pass
        else:
            # ���s��
            logger.error("[%s:%s] �����m�摜�ʒm�t�@�C���i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
            return result, sorted_files

        # �擾�����t�@�C���p�X���t�@�C���X�V�����Ń\�[�g����i�Â����ɏ������邽�߁j
        file_names = []
        for files in file_list:
            file_names.append((os.path.getmtime(files), files))

        sorted_files = []
        for mtime, path in sorted(file_names):
            sorted_files.append(path)

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, sorted_files


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ڑ�
#
# �����T�v           �F1.DB�Ɛڑ�����
#
# ����               �F�@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_connection():
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


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
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result
# ------------------------------------------------------------------------------------
# ������             �F��Ǝҏ��擾
#
# �����T�v           �F1.�������w�b�_�e�[�u�������Ǝҏ����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     ����
#                     �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �������
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, fabric_name, inspection_num, timestamp, unit_num):
    ### �N�G�����쐬����
    sql = 'select worker_1, worker_2, start_datetime from inspection_info_header ' \
          'where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\' ' \
          'and insert_datetime <= \'%s\' order by insert_datetime desc'\
          % (fabric_name, inspection_num, unit_num, timestamp)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u������f�[�^���擾����B
    result, records, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F�t�@�C���Ǎ�
#
# �����T�v           �F1.�����m�摜�ʒm�t�@�C����Ǎ���
#
# ����               �F�����m�摜�ʒm�t�@�C���̃t�@�C���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �i��
#                      ����
#                      �����ԍ�
#                      ��
#                      �J�����ԍ�_������No1
#                      �J�����ԍ�_������No2
#                      NG�摜��
#                      �B���J�n����
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    product_name = None
    fabric_name = None
    inspection_num = None
    face_num = None
    camera_num_1 = None
    camera_num_2 = None
    ng_image_file_name = None
    file_datetime = None

    try:
        # �摜�t�@�C���̊g���q
        extension = common_inifile.get('FILE_PATTERN', 'image_file')
        extension = extension.replace('*', '')

        logger.debug('[%s:%s] �B�������ʒm�t�@�C��=%s', app_id, app_name, file)
        # �����m�摜�ʒm��Ǎ��ށB
        # �����m�摜�ʒm�̃t�@�C�������擾����B
        # �t�@�C�����́A�uYYYYMMDD_[�i��]_[����]_[�����ԍ�]_[�J�����ԍ�]_[FACE���]_[�B���ԍ�].txt�v��z�肷��B
        basename = os.path.basename(file)
        file_name = re.split('[_.]', basename)

        product_name = file_name[0]
        fabric_name = file_name[1]
        inspection_num = file_name[3]
        face_num = file_name[4]
        file_datetime = file_name[2]
        camera_num = file_name[5]

        # FACE��񂩂猟�ŕ��𔻒f����B
        if face_num == '1':
            camera_num_1 = camera_num
        else:
            camera_num_2 = camera_num

        # �����m�摜�ʒm�̃t�@�C��������A�����m�摜�t�@�C�������쐬����B
        # �t�@�C�����́A�u[�i��]_[����]_[���t]_[�����ԍ�]_ [������No]_[�J�����ԍ�]_[�A��].jpg�v��z�肷��B
        ng_image_file_name = product_name + '_' + fabric_name + '_' + file_datetime + '_' + inspection_num + '_' + face_num + '_' + camera_num + '_' + file_name[6] + extension

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, product_name, fabric_name, inspection_num, face_num, camera_num_1, camera_num_2, \
           ng_image_file_name, file_datetime


# ------------------------------------------------------------------------------------
# ������             �F�ޔ��t�H���_���݃`�F�b�N
#
# �����T�v           �F1.�����m�摜�ʒm�t�@�C����ޔ�����t�H���_�����݂��邩�`�F�b�N����B
#                    2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�ޔ��t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C����ޔ�����t�H���_���쐬���܂��B�t�H���_���F[%s]',
                 app_id, app_name, target_path)
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F�����m�摜�ʒm�t�@�C���ޔ�
#
# �����T�v           �F1.�����m�摜�ʒm�t�@�C�����A�ޔ��t�H���_�Ɉړ�������B
#
# ����               �F�����m�摜�ʒm�t�@�C��
#                      �ޔ��t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    target_file_name = os.path.basename(target_file)
    # �t�@�C���ړ�
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)
    return result


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���o�^
#
# �����T�v           �F1.RAPID��͏��e�[�u����NG���ʃ��R�[�h��o�^����B
#
# ����               �F�i��
#                     �Ŕ�
#                     �����ԍ�
#                     ��
#                      NG�摜��
#                      NG���W
#                      RAPID��͌���
#                      �[���茋��
#                      �}�X�L���O���茋��
#                      �J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2,
                               inspection_num, ng_face, ng_image, ng_point, worker_1, worker_2, cur, conn,
                               inspection_date, unit_num):
    # DB�o�^�l:�����m�摜
    result_status = common_inifile.get('ANALYSIS_STATUS', 'undetec')

    # �J�����ԍ� �������ɂ��āA�l�������ꍇ�ɂ�null�����ɒu��������
    if camera_num_1 is None:
        camera_num_1 = 'null'
    if camera_num_2 is None:
        camera_num_2 = 'null'

    inspection_num = str(int(inspection_num))

    ### �N�G�����쐬����
    sql = 'insert into "rapid_%s_%s_%s" (' \
          'product_name, fabric_name, camera_num_1, camera_num_2, ' \
          'inspection_num, ng_face, ng_image, marking_image, ng_point, ' \
          'rapid_result, edge_result, masking_result, worker_1, worker_2, unit_num ' \
          ') values (' \
          '\'%s\', \'%s\', %s, %s, ' \
          '%s, \'%s\', \'%s\', \'%s\', \'%s\', ' \
          '\'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
          % (fabric_name, inspection_num, inspection_date, product_name, fabric_name, camera_num_1, camera_num_2,
             inspection_num, ng_face, ng_image, 'marking_' + ng_image, ng_point, result_status, result_status,
             result_status, worker_1, worker_2, unit_num)

    ### rapid��͏��e�[�u���Ƀ��R�[�h�ǉ�
    return db_util.operate_data(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# ������             �FNG���W���S���W�擾
#
# �����T�v           �F1.NG���W�̒��S���W���擾����B
#
# ����               �F���T�C�Y�摜��
#                     ���T�C�Y�摜����
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    NG���W���S���W
#
# ------------------------------------------------------------------------------------
def get_center_ng_point(resize_width, resize_height):
    result = False
    center_ng_point = ''

    try:
        # NG�摜�T�C�Y���璆�S���W���擾����B
        # ������؂�Ȃ��ꍇ�A�ۂ߂�i�l�̌ܓ��j
        center_width = str(Decimal(str(resize_width / 2)).quantize(Decimal('0'), rounding=ROUND_HALF_UP))
        center_height = str(Decimal(str(resize_height / 2)).quantize(Decimal('0'), rounding=ROUND_HALF_UP))

        center_ng_point = center_width + ',' + center_height

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, center_ng_point


# ------------------------------------------------------------------------------------
# ������             �F�Ώۉ摜�t�@�C���擾
#
# �����T�v           �F1.�Ώۉ摜�t�@�C�����擾����B
#                     �Ȃ��A�{�����Ŏ擾����̂́ARAPID�T�[�o��̎B���摜�t�@�C����ΏۂƂ��Ă���B
#                     �i�����m�摜�iNG���肳�ꂽ�摜�j��RAPID�T�[�o��̎B���摜�t�H���_����T�������j
#
# ����               �F�B���J�n����
#                     �i��
#                     ����
#                     �����ԍ�
#                     �摜���[�g�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �Ώۉ摜�t�@�C��
# ------------------------------------------------------------------------------------
def get_ng_image_file(imaging_starttime, product_name, fabric_name, inspection_num, ng_image_path,
                      ng_image_file_name):
    result = False
    ng_image_file = None
    try:
        # �u(�i��)_(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)�v
        path_name = product_name + '_' + fabric_name + '_' + imaging_starttime + '_' + str(inspection_num)
        target_file_path = ng_image_path + '\\' + path_name
        target_file_name = '\\*\\' + ng_image_file_name

        if os.path.isdir(target_file_path):
            pass
        else:
            result = True
            return result, ng_image_file

        tmp_result, ng_image_files = get_file(target_file_path, target_file_name)

        if tmp_result:
            # ����I�������ꍇ�ł��A�t�@�C����������Ȃ��P�[�X�͑��݂���̂ŁA
            # ���݂���ꍇ�̂݁A�Ώۉ摜�t�@�C������ݒ肷��
            if ng_image_files:
                # �������ɕK����ӂɂȂ�̂ŁA�擪���X�g�̂ݕԂ�
                ng_image_file = ng_image_files[0]
            else:
                pass
        else:
            return result, ng_image_file

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
    return result, ng_image_file


# ------------------------------------------------------------------------------------
# ������             �F�Ώۉ摜�t�@�C���R�s�[
#
# �����T�v           �F1.�����m�摜���k�t�H���_���쐬����B
#                    2.�쐬�����t�H���_�ɑΏۉ摜�t�@�C���A�}�[�L���O�摜�t�@�C�����R�s�[����B
#
# ����               �F�摜�t�@�C���p�X�i�摜�t�@�C�����܂ށj
#                     �o�̓p�X
#                     �i�R�s�[��́j�}�[�L���O�摜�t�@�C����
#                     �@�\ID
#                     �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def copy_ng_image_file(ng_image_file_path, output_path,
                       marking_ng_image_file_name):
    result = False

    try:
        # �����m�摜�i�[�t�H���_���쐬����
        logger.debug('[%s:%s] �����m�摜�i�[�t�H���_:[%s]', app_id, app_name, output_path)
        tmp_result, error = file_util.make_directory(output_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] �����m�摜�i�[�t�H���_�̍쐬�Ɏ��s���܂����B:[%s]', app_id, app_name, output_path)
            return result

        # �}�[�L���O�摜�t�@�C�����A�����m�摜�i�[�t�H���_�ɃR�s�[����B
        tmp_result, error = file_util.copy_file(ng_image_file_path, output_path, logger, app_id, app_name)
        tmp_input_path = output_path + '\\' + os.path.basename(ng_image_file_path)
        tmp_output_path = output_path + '\\' + marking_ng_image_file_name
        shutil.move(tmp_input_path, tmp_output_path)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] �}�[�L���O�摜�t�@�C���̃R�s�[�Ɏ��s���܂����B', app_id, app_name)
            return result

        # NG�摜�t�@�C�����A�����m�摜�i�[�t�H���_�ɃR�s�[����B
        tmp_result, error = file_util.copy_file(ng_image_file_path, output_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            logger.error('[%s:%s] NG�摜�t�@�C���̃R�s�[�Ɏ��s���܂����B', app_id, app_name)
            return result


        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���X�V
#
# �����T�v           �F1.NG����ŃG���[�ɂȂ����ꍇ�ARAPID��͏��e�[�u���̃X�e�[�^�X
#                        �iRAPID���ʁA�[���茋�ʁA�}�X�L���O���茋�ʁj���X�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     ����
#                     �����ԍ�
#                     �������t
#                     ���@���
#                     ���茋�ʃX�e�[�^�X(none)
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �������
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_rapid_analysis(conn, cur, fabric_name, inspection_num, inspection_date,
                          ng_image_file_name, unit_num):

    # DB�o�^�l
    status = common_inifile.get('ANALYSIS_STATUS', 'none')

    inspection_num = str(int(inspection_num))

    ### �N�G�����쐬����
    sql = 'update "rapid_%s_%s_%s" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where ng_image = \'%s\' and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, status, status, status, ng_image_file_name, unit_num)

    logger.debug('[%s:%s] RAPID��͏��e�[�u���X�VSQL=[%s]' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�����m�摜�o�^���s���B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def main():
    # �ϐ���`
    error_file_name = None
    conn = None
    cur = None
    target_ng_image_files = None
    try:

        ### �ݒ�t�@�C������̒l�擾
        # ���ʐݒ�F�e��ʒm�t�@�C�����i�[����郋�[�g�p�X
        input_root_path = inifile.get('PATH', 'input_path')
        # ���ʐݒ�F�e��ʒm�t�@�C����ޔ������郋�[�g�p�X
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # RAPID�T�[�o�z�X�g��
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        # �A�g��Ղ̃��[�g�f�B���N�g��
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �����m�摜�i�[�t�@�C���p�X
        undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # �����ʒm���i�[�����t�H���_�p�X
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path
        # �����m�摜�ʒm�t�@�C�����p�^�[��
        file_name_pattern = inifile.get('FILE', 'file_name_pattern')
        # �����m�摜�ʒm�t�@�C���F�ޔ��f�B���N�g���p�X
        backup_path = inifile.get('PATH', 'backup_path')
        backup_path = backup_root_path + '\\' + backup_path
        # �B���摜���i�[����p�X
        image_path = inifile.get('PATH', 'image_dir')
        # �����Ώۃ��C���ԍ�
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        # �v���O��������l�F�����m�摜
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))
        # �}�[�L���ONG�摜���̐ړ���
        marking_ng_image_file_name_prefix = inifile.get('FILE', 'marking_ng_image_file_name_prefix')
        # NG�摜�T�C�Y
        resize_width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        resize_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### �����m�摜�ʒm�Ď�
        while True:
            # �t�H���_���ɖ����m�摜�ʒm�t�@�C�������݂��邩�m�F����
            logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
            result, sorted_files = get_file(file_path, file_name_pattern)

            if result:
                pass
            else:
                logger.error('[%s:%s] �����m�摜�ʒm�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                sys.exit()

            # �����m�摜�ʒm�t�@�C�����Ȃ��ꍇ�͈�����sleep���čĎ擾
            if len(sorted_files) == 0:
                logger.info('[%s:%s] �����m�摜�ʒm�t�@�C�������݂��܂���B', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C���𔭌����܂����B:�����m�摜�ʒm�t�@�C����[%s]',
                         app_id, app_name, sorted_files)

            # DB���ʏ������Ăяo���āADB�ɐڑ�����B
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
                sys.exit()

            for i in range(len(sorted_files)):

                basename = os.path.basename(sorted_files[i])
                file_name = re.split('[_.]', basename)
                fabric_name = file_name[1]
                inspection_num = file_name[3]
                logger.info('[%s:%s] %s�������J�n���܂��B [����,�����ԍ�]=[%s, %s]', app_id, app_name, app_name, fabric_name,
                            inspection_num)
                ### �t�@�C���Ǎ�
                logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C���̓Ǎ����J�n���܂��B', app_id, app_name)

                result, product_name, fabric_name, inspection_num, \
                face_num, camera_num_1, camera_num_2, ng_image_file_name, file_datetime = \
                    read_file(sorted_files[i])

                if result:
                    logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C���̓Ǎ����I�����܂����B�����m�摜�ʒm�t�@�C����=[%s]',
                                 app_id, app_name, sorted_files[i])
                else:
                    logger.error('[%s:%s] �����m�摜�ʒm�t�@�C���̓Ǎ��Ɏ��s���܂����B�����m�摜�ʒm�t�@�C����=[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # �����m�摜�ʒm�t�@�C����ޔ�����t�H���_���쐬����B
                logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C����ޔ�����t�H���_�쐬���J�n���܂��B', app_id, app_name)
                result = exists_dir(backup_path)

                if result:
                    logger.debug('[%s:%s] �����m�摜�ʒm�t�@�C����ޔ�����t�H���_�쐬���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �����m�摜�ʒm�t�@�C����ޔ�����t�H���_�쐬�Ɏ��s���܂����B�ޔ��t�H���_��=[%s]',
                                 app_id, app_name, backup_path)
                    sys.exit()

                # �ޔ����関���m�摜�ʒm�t�@�C���Ɠ����̃t�@�C�������݂��邩�m�F����B
                logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����J�n���܂��B�B�������ʒm�t�@�C����=[%s]',
                             app_id, app_name, sorted_files[i])
                result = move_file(sorted_files[i], backup_path)

                if result:
                    logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����I�����܂����B�ޔ��t�H���_=[%s], �B�������ʒm�t�@�C����=[%s]',
                                 app_id, app_name, backup_path, sorted_files[i])
                else:
                    logger.error('[%s:%s] �B�������ʒm�t�@�C���̑ޔ��Ɏ��s���܂����B�B�������ʒm�t�@�C����=[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                ### �Ώۉ摜�̊i�[����Ă���t�H���_��SRAPID�T�[�o�[���Q�Ƃ��ē��肷��B
                logger.debug('[%s:%s] �Ώۉ摜�̓�����J�n���܂��B', app_id, app_name)

                for rapid_hostname in rapid_hostname_list:
                    target_path = '\\\\' + rapid_hostname + '\\' + image_path
                    logger.debug('[%s:%s] �Ώۉ摜�̓�����J�n���܂��B[%s]', app_id, app_name, target_path)
                    result, target_ng_image_files = \
                        get_ng_image_file(file_datetime, product_name, fabric_name, inspection_num,
                                          target_path, ng_image_file_name)
                    if result:
                        logger.debug('[%s:%s] �Ώۉ摜�̓��肪�I�����܂����B[%s]', app_id, app_name, target_path)
                    else:
                        logger.error('[%s:%s] �Ώۉ摜�̓���Ɏ��s���܂����B[%s]', app_id, app_name, target_path)
                        sys.exit()
                    # RAPID�T�[�o��Ńt�@�C���𔭌������ꍇ�́A���̎��_�œ�����I��
                    if target_ng_image_files is not None:
                        break

                if target_ng_image_files is not None:
                    logger.debug('[%s:%s] �Ώۉ摜�̓��肪�I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �Ώۉ摜�̓���Ɏ��s���܂����B', app_id, app_name)
                    sys.exit()

                # �����m�摜���k�t�H���_���쐬���A�쐬�����t�H���_�ɓ��肵���摜���R�s�[����B
                target_ng_image_path = rk_root_path + '\\' + undetected_image_file_path + '\\' + os.path.splitext(os.path.basename(sorted_files[i]))[0]
                logger.debug('[%s:%s] �Ώۉ摜�̃R�s�[���J�n���܂��B', app_id, app_name)
                result = copy_ng_image_file(target_ng_image_files, target_ng_image_path,
                                             marking_ng_image_file_name_prefix + ng_image_file_name)

                if result:
                    logger.debug('[%s:%s] �Ώۉ摜�̃R�s�[���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �Ώۉ摜�̃R�s�[�Ɏ��s���܂����B', app_id, app_name)
                    sys.exit()

                ### RAPID��͏��o�^
                # NG���W�̒��S���W���擾
                logger.debug('[%s:%s] NG���W�̒��S���W�擾���J�n���܂��B', app_id, app_name)
                result, center_ng_point = get_center_ng_point(resize_width, resize_height)

                if result:
                    logger.debug('[%s:%s] NG���W�̒��S���W�擾���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG���W�̒��S���W�擾�Ɏ��s���܂����B', app_id, app_name)
                    sys.exit()

                # DB���ʏ������Ăяo���āARAPID��͏��e�[�u���Ɉȉ��̍��ڂ�o�^����B
                logger.debug('[%s:%s] RAPID��͏��e�[�u���̓o�^���J�n���܂��B', app_id, app_name)

                file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(target_ng_image_files)).strftime("%Y-%m-%d %H:%M:%S")
                tmp_result, records, conn, cur = select_inspection_info(conn, cur, fabric_name, inspection_num, file_timestamp, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] ��Ǝҏ��擾���������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] ��Ǝҏ��擾�Ɏ��s���܂����B', app_id, app_name)
                    return tmp_result, conn, cur

                worker_1 = records[0]
                worker_2 = records[1]
                imaging_starttime = records[2]
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                result, error, conn, cur = \
                    insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2, inspection_num,
                                               '#' + str(face_num), ng_image_file_name, center_ng_point, worker_1,
                                               worker_2, cur, conn, inspection_date, unit_num)

                if result:
                    logger.debug('[%s:%s] RAPID��͏��e�[�u���̓o�^���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] RAPID��͏��e�[�u���̓o�^�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # �R�~�b�g����
                conn.commit()

                ### NG�s�E�񔻒�@�\�ďo
                logger.debug('[%s:%s] NG�s�E�񔻒�@�\�ďo���J�n���܂��B', app_id, app_name)

                result = register_ng_info_undetect.main(
                    product_name, fabric_name, inspection_num, 0, ng_image_file_name, center_ng_point,
                    undetected_image_flag_is_undetected, imaging_starttime, unit_num)

                if result:
                    logger.debug('[%s:%s] NG�s�E�񔻒�@�\�ďo���������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG�s�E�񔻒�@�\�ďo�Ɏ��s���܂����B', app_id, app_name)
                    logger.info('[%s:%s] NG����G���[�̂��߁ARAPID��͏��e�[�u�����X�V���܂��B [����,�����ԍ�]=[%s, %s]', app_id, app_name, fabric_name, inspection_num)

                    result, conn, cur = update_rapid_analysis(conn, cur, fabric_name, inspection_num, inspection_date, ng_image_file_name, unit_num)

                    if result:
                        logger.info('[%s:%s] RAPID��͏��e�[�u���X�V���������܂����B', app_id, app_name)
                        # �R�~�b�g����
                        conn.commit()
                    else:
                        logger.error('[%s:%s] RAPID��͏��e�[�u���X�V�Ɏ��s���܂����B', app_id, app_name)
                        sys.exit()


                ### NG�摜���k�E�]���@�\�ďo
                logger.debug('[%s:%s] NG�摜���k�E�]���@�\�ďo���J�n���܂��B', app_id, app_name)

                result = compress_image_undetect.main(
                    product_name, fabric_name, inspection_num, file_datetime,
                    rk_root_path + '\\' + undetected_image_file_path, target_ng_image_path)

                if result:
                    logger.debug('[%s:%s] NG�摜���k�E�]���@�\�ďo���������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG�摜���k�E�]���@�\�ďo�Ɏ��s���܂����B', app_id, app_name)
                    sys.exit()

                logger.info("[%s:%s] %s�����͐���ɏI�����܂����B[����,�����ԍ�]=[%s, %s]", app_id, app_name, app_name, fabric_name, inspection_num)

            # DB�ڑ���ؒf
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)

            # ���̊Ď��Ԋu�܂ŃX���[�v
            time.sleep(sleep_time)

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
    main()
