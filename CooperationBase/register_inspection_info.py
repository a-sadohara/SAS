# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\301  �������o�^�@�\
# ----------------------------------------
import configparser
import logging.config
import sys
import time
import traceback
from pathlib import Path
import codecs

import db_util
import error_detail
import error_util
import file_util
# ADD 20200714 KQRM ���g START
import os
import csv
# ADD 20200714 KQRM ���g END

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_inspection_info.conf")
logger = logging.getLogger("register_inspection_info")
# ���ʐݒ�t�@�C��
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
# �������o�^�ݒ�t�@�C��
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_inspection_info_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ڑ�
#
# �����T�v           �F1.DB�Ɛڑ�����
#
# ����               �F�@�\ID
#                     �@�\��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def create_connection():
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name

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
    # RAPID��͏��e�[�u�����쐬����B
    result, error, conn, cur = db_util.create_table(conn, cur, fabric_name, inspection_num, inspection_date,
                                             logger, app_id, app_name)
    return result, error, conn, cur

# ------------------------------------------------------------------------------------
# ������             �F�������擾
#
# �����T�v           �F1.�������e�[�u�����猟�������擾����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def select_inspection_data(conn, cur, unit_num):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'select inspection_info_header.product_name, inspection_info_header.fabric_name ,' \
          'inspection_info_header.inspection_num, ' \
          'inspection_info_header.start_datetime, inspection_info_header.unit_num FROM inspection_info_header left ' \
          'outer join fabric_info on ' \
          '(inspection_info_header.fabric_name = fabric_info.fabric_name and ' \
          'inspection_info_header.inspection_num = fabric_info.inspection_num and ' \
          'inspection_info_header.start_datetime = fabric_info.imaging_starttime) where ' \
          'inspection_info_header.unit_num = \'%s\' and fabric_info.fabric_name is null and inspection_info_header.branch_num = 1 ' \
          'order by start_datetime asc' % unit_num

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))

    ### �������e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# ������             �F�O�������擾
#
# �����T�v           �F1.�������e�[�u������O���������擾����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def select_before_inspection_data(conn, cur, starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select fi.product_name, fi.fabric_name, fi.inspection_num, fi.imaging_endtime, fi.imaging_starttime, '\
          'ii.inspection_direction from fabric_info as fi, inspection_info_header as ii '\
          'where fi.unit_num = \'%s\' and fi.imaging_starttime < \'%s\' and '\
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and '\
          'fi.imaging_starttime = ii.start_datetime order by fi.imaging_starttime desc' % (unit_num, starttime)

    logger.debug('[%s:%s] �O�������擾SQL %s' % (app_id, app_name, sql))

    ### �������e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur

# ------------------------------------------------------------------------------------
# �֐���             �F�������o�^
#
# �����T�v           �F1.�������e�[�u���Ƀf�[�^��o�^����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �����ԍ�
#                      �J�n����
#                      ���@
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def insert_fabric_info(conn, cur, product_name, fabric_name, inspection_num, starttime, status, unit_no):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'insert into fabric_info (product_name, fabric_name, inspection_num, imaging_starttime, status, unit_num) ' \
          'values (\'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' % (product_name, fabric_name, inspection_num,
                                                                   starttime, status, unit_no)

    logger.debug('[%s:%s] �������o�^SQL %s' % (app_id, app_name, sql))
    ### �������e�[�u���փf�[�^�o�^
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�o�͐�t�H���_���݃`�F�b�N
#
# �����T�v           �F1.�B�������ʒm(�_�~�[)�A���W�}�[�N�ǂݎ�茋��(�_�~�[)���o�͂���t�H���_�����݂��邩�`�F�b�N����B
#                      2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�o�͐�t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# �֐���             �F�C���v�b�g�t�@�C���o��
#
# �����T�v           �F1.�B�������ʒm(�_�~�[)�A���W�}�[�N�ǂݎ�茋��(�_�~�[)���o�͂���B
#
# ����               �F�i��
#                      ����
#                      �����ԍ�
#                      ������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def output_dummy_file(product_name, fabric_name, inspection_num, inspection_date, before_inspection_direction):
    result = False
    output_file_path = common_inifile.get('FILE_PATH', 'input_path')
    file_extension = inifile.get('PATH', 'file_extension')

    try:
        scaninfo_path = inifile.get('PATH', 'scaninfo_path')
        scaninfo_prefix = inifile.get('PATH', 'scaninfo_prefix')
        regimark_path = inifile.get('PATH', 'regimark_path')
        regimark_prefix = inifile.get('PATH', 'regimark_prefix')

        scaninfo_file_name = scaninfo_prefix + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + inspection_date + file_extension
        regimark_file_name = regimark_prefix + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + inspection_date + "_"

        # ADD 20200714 KQRM ���g START
        number_list_face_no_1 = []
        number_list_face_no_2 = []
        image_num = 0
        image_num_total = 0
        completed_lines = 0
        # ADD 20200714 KQRM ���g END

        for i in range(0, 2):
            for j in range(0, 2):
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(j + 1) + "_" + str(
                    i + 1) + file_extension

                # ADD 20200714 KQRM ���g START
                if os.path.exists(csv_file):
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�������݂��܂��B[%s]', app_id, app_name, csv_file)
                    if i == 0 and (before_inspection_direction == 'S' or before_inspection_direction == 'X'):
                        # ��������S, X�̏ꍇ�A�J�n���W�}�[�N��ǂݍ���
                        if j == 0:
                            get_serial_number_list(csv_file, number_list_face_no_1, logger)
                        else:
                            get_serial_number_list(csv_file, number_list_face_no_2, logger)
                    elif i == 1 and (before_inspection_direction == 'R' or before_inspection_direction == 'Y'):
                        # ��������R, Y�̏ꍇ�A�I�����W�}�[�N��ǂݍ���
                        if j == 0:
                            get_serial_number_list(csv_file, number_list_face_no_1, logger)
                        else:
                            get_serial_number_list(csv_file, number_list_face_no_2, logger)

                    continue
                else:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�������݂��܂���B�_�~�[�t�@�C�����o�͂��܂��B[%s]', app_id, app_name, csv_file)
                # ADD 20200714 KQRM ���g END

                with codecs.open(csv_file, "w", "SJIS") as f:
                    f.write("�B���t�@�C����,���,���W��,���W��,�p���X")
                    f.write("\r\n")

        # ADD 20200714 KQRM ���g START
        # �u���Ȃ����̓ǎ�s�� -2�v�̒l��ݒ肷��
        if len(number_list_face_no_1) > len(number_list_face_no_2):
            completed_lines = len(number_list_face_no_2) - 2
        else:
            completed_lines = len(number_list_face_no_1) - 2

        # �}�C�i�X�l�̏ꍇ�A0�ŕ␳����
        if completed_lines < 0:
            completed_lines = 0

        # �B���A�Ԃ̍ő�l��ݒ肷��
        if max(number_list_face_no_1, default=0) > max(number_list_face_no_2, default=0):
            image_num = max(number_list_face_no_1, default=0)
        else:
            image_num = max(number_list_face_no_2, default=0)

        # �J�����䐔���l�����A���B��������ݒ肷��
        image_num_total = image_num * 54
        # ADD 20200714 KQRM ���g END

        csv_file = output_file_path + "\\" + scaninfo_path + "\\" + scaninfo_file_name
        with codecs.open(csv_file, "w", "SJIS") as f:
            f.write("�J�����䐔,�J����1��̎B������,���B������,���������s��")
            f.write("\r\n")
            # UPD 20200714 KQRM ���g START
            # f.write("54,0,0,0")
            f.write("54,{},{},{}".format(image_num, image_num_total, completed_lines))
            # UPD 20200714 KQRM ���g END
            f.write("\r\n")

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ADD 20200714 KQRM ���g START
# ------------------------------------------------------------------------------------
# �֐���             �F�A�Ԏ擾
#
# �����T�v           �F1.���W�}�[�N�ǎ挋�ʃt�@�C���̎B���t�@�C�������A�A�Ԃ𒊏o�����X�g�֒ǉ�����B
#
# ����               �F���W�}�[�N�ǎ挋�ʃt�@�C����
#                      �A�ԃ��X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ���W�}�[�N�ǎ挋�ʃt�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_serial_number_list(file_path, serial_number_list, logger):
    result = False
    file_name = None
    index = None

    try:
        with open(file_path) as f:
            # �w�b�_�s��ǂݔ�΂�
            next(csv.reader(f))

            # �B���t�@�C�������B���A�Ԃ𒊏o���A���l�Ƃ��ă��X�g�ɒǉ�����
            for row in csv.reader(f):
                file_name = Path(row[0]).stem
                index = (file_name.find('_') + 1) * -1
                serial_number_list.append(int(file_name[index:]))

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result
# ADD 20200714 KQRM ���g END

# ------------------------------------------------------------------------------------
# �֐���             �FDB�ؒf
#
# �����T�v           �F1.DB�Ƃ̐ڑ���ؒf����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def close_connection(conn, cur):
    func_name = sys._getframe().f_code.co_name
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)
    return result, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �F���C������
#
# �����T�v           �F1.�������e�[�u������擾�����������𔽕����e�[�u���ɓo�^����B
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
    func_name = None
    try:

        # �ݒ�t�@�C������擾
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        imaging_status = int(common_inifile.get('FABRIC_STATUS', 'imaging'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')
        output_file_path = common_inifile.get('FILE_PATH', 'input_path')
        file_extension = inifile.get('PATH', 'file_extension')
        scaninfo_path = inifile.get('PATH', 'scaninfo_path')
        scaninfo_prefix = inifile.get('PATH', 'scaninfo_prefix')

        logger.info('[%s:%s] %s�@�\���N�����܂�' % (app_id, app_name, app_name))
        ###DB�ڑ����s��
        # �����ڑ����s�����ꍇ�́A�ēx�ڑ��������B
        while True:

            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
            # DB�ɐڑ�����
            result, error, conn, cur, func_name = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
                sys.exit()

            while True:

                logger.debug('[%s:%s] �������擾���J�n���܂��B' % (app_id, app_name))
                # �������e�[�u�����猟�������擾����
                result, inspection_info, error, conn, cur, func_name = select_inspection_data(conn, cur, unit_num)

                if result:
                    logger.debug('[%s:%s] �������擾���I�����܂����B' % (app_id, app_name))
                    logger.debug('[%s:%s] �������=%s' % (app_id, app_name, inspection_info))
                    pass
                else:
                    logger.error('[%s:%s] �������擾�����s���܂����B' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                # ������񂪑��݂��邩�m�F����
                # ������񂪑��݂���ꍇ�A�������o�^�������s���B
                if len(inspection_info) > 0:
                    logger.debug('[%s:%s] ������񂪑��݂��܂��B' % (app_id, app_name))
                    logger.info('[%s:%s] %s�������J�n���܂��B' % (app_id, app_name, app_name))
                    # �擾����������񕪁A�������s��

                    for i in range(len(inspection_info)):

                        # �擾����������񂩂�i���A���ԁA�����ԍ��A�J�n�����A���@�𒊏o����B
                        product_name = inspection_info[i][0]
                        fabric_name = inspection_info[i][1]
                        inspection_num = inspection_info[i][2]
                        starttime = inspection_info[i][3]
                        inspection_date = str(starttime.strftime('%Y%m%d'))
                        unit_no = inspection_info[i][4]
                        
                        logger.debug('[%s:%s] �O�������擾���J�n���܂��B' % (app_id, app_name))
                        # �������e�[�u������O���������擾����
                        result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur, starttime, unit_num)
                        logger.debug('[%s:%s] �O�������擾���I�����܂����B%s ' % (app_id, app_name, before_inspection_info))
                        if before_inspection_info is None:
                            pass
                        elif before_inspection_info[3] == None:
                            before_product_name = before_inspection_info[0]
                            before_fabric_name = before_inspection_info[1]
                            before_inspection_num = before_inspection_info[2]
                            before_starttime = before_inspection_info[4]
                            before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                            logger.debug('[%s:%s] �O������� [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]' % (
                                app_id, app_name, before_product_name, before_fabric_name,
                                before_inspection_num, before_inspection_date))

                            before_scan_file_name = scaninfo_prefix + "_" + before_product_name + "_" + before_fabric_name + "_" + str(before_inspection_num) + "_" + before_inspection_date + file_extension
                            if os.path.exists(output_file_path + '\\' + scaninfo_path + '\\' + before_scan_file_name):
                                logger.info('[%s:%s] �O�����̎B�������ʒm�͏o�͍ςł��B [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                                            % (app_id, app_name, before_product_name, before_fabric_name,
                                               before_inspection_num, before_inspection_date))
                                time.sleep(sleep_time * 2)
                                break
                            else:
                                for i in range(5):
                                    logger.debug('[%s:%s] �O�������擾���J�n���܂��B' % (app_id, app_name))
                                    # �������e�[�u������O���������擾����
                                    result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur,
                                                                                                              starttime,
                                                                                                              unit_num)

                                    if result:
                                        logger.debug('[%s:%s] �O�������擾���I�����܂����B' % (app_id, app_name))
                                        if before_inspection_info is None:
                                            pass
                                        elif before_inspection_info[3] != None:
                                            before_product_name = before_inspection_info[0]
                                            before_fabric_name = before_inspection_info[1]
                                            before_inspection_num = before_inspection_info[2]
                                            before_starttime = before_inspection_info[4]
                                            before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                                            logger.debug('[%s:%s] �O������� [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]' % (
                                                app_id, app_name, before_product_name, before_fabric_name,
                                                before_inspection_num, before_inspection_date))
                                            break
                                        else:
                                            logger.info('[%s:%s] �O�����̌����������������݂��܂���B�Ċm�F���܂��B' % (app_id, app_name))
                                            time.sleep(sleep_time)
                                            continue
                                    else:
                                        logger.error('[%s:%s] �O�������擾�����s���܂����B' % (app_id, app_name))
                                        conn.rollback()
                                        sys.exit()

                                if before_inspection_info is None:
                                    pass
                                elif before_inspection_info[3] == None:
                                    before_product_name = before_inspection_info[0]
                                    before_fabric_name = before_inspection_info[1]
                                    before_inspection_num = before_inspection_info[2]
                                    before_starttime = before_inspection_info[4]
                                    before_inspection_direction = before_inspection_info[5]
                                    before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                                    logger.info('[%s:%s] �O�������������Ă��܂���B�O������� [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]' % (
                                    app_id, app_name, before_product_name, before_fabric_name, before_inspection_num, before_inspection_date))


                                    error_file_path = common_inifile.get('ERROR_FILE', 'path')
                                    Path(error_file_path + '\\' + error_file_name).touch()

                                    logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���J�n���܂��B [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                                                % (app_id, app_name, before_product_name, before_fabric_name,
                                                   before_inspection_num, before_inspection_date))
                                    tmp_result = output_dummy_file(before_product_name, before_fabric_name,
                                                               before_inspection_num, before_inspection_date, before_inspection_direction)
                                    if tmp_result:
                                        logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���I�����܂����B [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                                                    % (app_id, app_name, before_product_name, before_fabric_name,
                                                       before_inspection_num, before_inspection_date))
                                        time.sleep(sleep_time * 2)
                                        break
                                    else:
                                        logger.error('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j�Ɏ��s���܂����B ' % (app_id, app_name))
                                        sys.exit()
                                else:
                                    pass               
                        else:
                            pass

                        inspection_date = str(starttime.strftime('%Y%m%d'))

                        logger.debug('[%s:%s] ������� [�i��=%s] [����=%s] [�����ԍ�=%s]' %
                                     (app_id, app_name, product_name, fabric_name, inspection_num))

                        logger.debug('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�]=[%s, %s]' %
                                     (app_id, app_name, app_name, fabric_name, inspection_num))

                        # RAPID��͏��e�[�u�����쐬����
                        logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���J�n���܂��B' % (app_id, app_name))
                        result, error, conn, cur = create_rapid_table(conn, cur, fabric_name, inspection_num, inspection_date)
                        if result:
                            logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���I�����܂����B' % (app_id, app_name))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] RAPID��͏��e�[�u���쐬�����s���܂����B '
                                         '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                         % (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            conn.rollback()
                            sys.exit()

                        # �������𔽕����e�[�u���ɓo�^����
                        result, error, conn, cur, func_name = insert_fabric_info(conn, cur, product_name, fabric_name,
                                                                      inspection_num, starttime, imaging_status, unit_no)
                        if result:
                            conn.commit()
                            logger.info('[%s:%s] �������o�^���I�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        else:
                            logger.error('[%s:%s] �������o�^�Ɏ��s���܂����B ' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                    logger.info('[%s:%s] %s�����͐���ɏI�����܂����B' % (app_id, app_name, app_name))

                # ������񂪑��݂��Ȃ��ꍇ�A���[�v�𔲂���
                else:
                    logger.info('[%s:%s] ������񂪂���܂���B' % (app_id, app_name))
                    break

            # ������񂪑��݂��Ȃ����߁ADB�ڑ���؂�A��莞�ԃX���[�v���Ă���A�Ď擾���s���B
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
            result = close_connection(conn, cur)

            if result:
                logger.debug('[%s:%s] DB�ڑ��̐ؒf���������܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] DB�ڑ��̐ؒf�Ɏ��s���܂����B' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] %s�b�X���[�v���܂�' % (app_id, app_name, sleep_time))
            time.sleep(sleep_time)
            continue

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ���ăv���O�������I�����܂��B' % (app_id, app_name))

        logger.debug('[%s:%s] �G���[�ڍׂ��擾���܂��B' % (app_id, app_name))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)

        logger.error('[%s:%s] %s [�G���[�R�[�h:%s]' % (app_id, app_name, error_message, error_id))

        event_log_message = '[�@�\��, �G���[�R�[�h]=[%s, %s] %s' % (app_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message)

        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))
    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂���' % (app_id, app_name))
        logger.error(traceback.format_exc())

        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))
    finally:
        if conn is not None:
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B' % (app_id, app_name))
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()
