# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\302  �B�������o�^
# ----------------------------------------

import configparser
from datetime import datetime
import logging.config
import os
import re
import sys
import time
import traceback

import error_detail
import error_util
import db_util
import file_util
import register_ng_info
import register_regimark_info
import check_image_num

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_imagenum.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_imagenum")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_imagenum_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

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
def get_file(file_path, file_name, network_path_error):
    result = False
    sorted_files = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        logger.debug('[%s:%s] �B�������ʒm�t�@�C���i�[�t�H���_�p�X=[%s]', app_id, app_name, file_path)
        # ���ʊ֐��ŎB�������ʒm�i�[�t�H���_�����擾����

        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result == True:
            # ������
            pass
        elif tmp_result == network_path_error:
            # ���s��
            logger.debug("[%s:%s] �B�������ʒm�i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
            return tmp_result, sorted_files, error, func_name
        else:
            # ���s��
            logger.error("[%s:%s] �B�������ʒm�i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
            return result, sorted_files, error, func_name

        # �擾�����t�@�C���p�X���t�@�C���X�V�����Ń\�[�g����i�Â����ɏ������邽�߁j
        file_names = []
        for files in file_list:
            file_names.append((os.path.getmtime(files), files))

        sorted_files = []
        for mtime, path in sorted(file_names):
            sorted_files.append(path)

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
# ����               �F�B�������ʒm�t�@�C���̃t�@�C���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ����
#                      �����ԍ�
#                      �B������
#                      �B����������
# ------------------------------------------------------------------------------------
def read_file(file):
    result = False
    fabric_name = None
    inspection_num = None
    image_num = None
    imaging_endtime = None
    inspection_line = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        logger.debug('[%s:%s] �B�������ʒm�t�@�C��=%s', app_id, app_name, file)
        # �B�������ʒm�t�@�C���p�X����t�@�C�������擾���A�ŔԁA�����ԍ����擾����
        # �Ȃ��A�t�@�C�����́uIC_�i��_����_�����ԍ�_���t.CSV�v��z�肵�Ă���
        basename = os.path.basename(file)
        file_name = re.split('[_.]', basename)

        # �B�������ʒm�t�@�C������A�ŏI�X�V���t���擾����
        endtime = datetime.fromtimestamp(os.path.getctime(file))

        # �B�������ʒm�t�@�C������A���ڂ��擾����
        with open(file) as f:
            notification = [s.split(',') for s in f.readlines()]
            fabric_name, inspection_num, imaging_endtime, image_num, inspection_line = \
                file_name[2], file_name[3], endtime, notification[1][2], notification[1][3].rstrip('\n')

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, fabric_name, inspection_num, image_num, imaging_endtime, inspection_line, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ڑ�
#
# �����T�v           �F1.DB�Ɛڑ�����
#
# ����               �F�Ȃ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_connection():
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# �֐���             �F�B���J�n�����擾
#
# �����T�v           �F1.�������e�[�u���̎B���J�n�������擾����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ���@
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, fabric_name, inspection_num, unit_num, inspection_date):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    # sql = 'select imaging_starttime from fabric_info where fabric_name = \'%s\' and inspection_num = %s ' \
    #       'and unit_num = \'%s\' and imaging_endtime IS NULL and CAST(imaging_starttime AS DATE) = \'%s\' order by imaging_starttime asc' \
    #       % (fabric_name, inspection_num, unit_num, inspection_date)
    sql = 'select fi.imaging_starttime, fi.imaging_endtime, ii.inspection_direction, ii.inspection_start_line from fabric_info as fi, '\
          'inspection_info_header as ii where fi.fabric_name = \'%s\' and fi.inspection_num = %s ' \
          'and fi.unit_num = \'%s\' and CAST(fi.imaging_starttime AS DATE) = \'%s\' '\
          'and fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num '\
          'and fi.imaging_starttime = ii.start_datetime order by imaging_starttime asc' \
          % (fabric_name, inspection_num, unit_num, inspection_date)

    logger.debug('[%s:%s] �B���J�n�����擾SQL [%s]', app_id, app_name, sql)
    ### �������e�[�u�����X�V
    result, imaging_starttime, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, imaging_starttime, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�������o�^
#
# �����T�v           �F1.�������e�[�u���̌��������X�V����B
#
# ����               �F�B������
#                      �B����������
#                      ����
#                      �����ԍ�
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g�X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info(update_db_status, image_num, imaging_endtime, fabric_name, inspection_num,
                       cur, conn, imaging_startime, unit_num):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s, image_num = %s, imaging_endtime = \'%s\'' \
          'where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\' ' \
          % (update_db_status, image_num, imaging_endtime, fabric_name, inspection_num, imaging_startime, unit_num)

    logger.debug('[%s:%s] �������o�^SQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    logger.debug('[%s:%s] ����[%s], �����ԍ�[%s]�̃��R�[�h���X�V���܂����B', app_id, app_name, fabric_name, inspection_num)
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# ������             �F�������o�^
#
# �����T�v           �F1.�������e�[�u���̌��������X�V����B
#
# ����               �F�B������
#                      �B����������
#                      ����
#                      �����ԍ�
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g�X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_inspection_info_header(fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num, end_line, inspection_target_line):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'update inspection_info_header set inspection_end_line = %s, inspection_target_line = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s ' \
          'and start_datetime = \'%s\' and unit_num = \'%s\' ' \
          % (end_line, inspection_target_line, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������(�ŏI�s��)�X�VSQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    logger.debug('[%s:%s] ����[%s], �����ԍ�[%s], �s��[%s] ���X�V���܂����B', app_id, app_name, fabric_name, inspection_num, end_line)
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, error, conn, cur, func_name

# ------------------------------------------------------------------------------------
# ������             �F�ޔ��t�H���_���݃`�F�b�N
#
# �����T�v           �F1.�B�������ʒm�t�@�C����ޔ�����t�H���_�����݂��邩�`�F�b�N����B
#                    2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�ޔ��t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    func_name = sys._getframe().f_code.co_name
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�B�������ʒm�t�@�C���ޔ�
#
# �����T�v           �F1.�B�������ʒm�t�@�C�����A�ޔ��t�H���_�Ɉړ�������B
#
# ����               �F�B�������ʒm�t�@�C��
#                      �ޔ��t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    func_name = sys._getframe().f_code.co_name
    # �t�@�C���ړ�
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

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
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result, func_name

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
def select_before_inspection_data(conn, cur, inspection_num, inspection_date, unit_num):
    inspection_date = inspection_date[0:4] + '/' + inspection_date[4:6] + '/' + inspection_date[6:8]
    # �����ԍ���1�̏ꍇ�A���ݓ��̌����ԍ�-1�̏��
    # �����ԍ��̏ꍇ�A���ݓ�-1�̍ŐV�����̏��
    ### �N�G�����쐬����
    if inspection_num == '1':
        sql = 'select product_name, fabric_name, inspection_num, imaging_endtime, imaging_starttime from fabric_info where unit_num = \'%s\' ' \
              'and cast(imaging_starttime as date) < \'%s\' order by imaging_starttime desc ' % (unit_num, inspection_date)
    else:
        sql = 'select product_name, fabric_name, inspection_num, imaging_endtime, imaging_starttime from fabric_info where unit_num = \'%s\' ' \
              'and cast(imaging_starttime as date)  = \'%s\' and inspection_num = %s ' % (unit_num, inspection_date, int(inspection_num) - 1)

    logger.debug('[%s:%s] �O�������擾SQL %s' % (app_id, app_name, sql))

    ### �������e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur

# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�B�������o�^���s���B
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
    error = None
    func_name = None
    try:

        ### �ݒ�t�@�C������̒l�擾
        # ���ʐݒ�F�e��ʒm�t�@�C�����i�[����郋�[�g�p�X
        input_root_path = common_inifile.get('FILE_PATH', 'input_path')
        # ���ʐݒ�F�e��ʒm�t�@�C����ޔ������郋�[�g�p�X
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # �����ʒm���i�[�����t�H���_�p�X
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path + '\\'
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # DB�X�V�l�F�������e�[�u��:�X�e�[�^�X�F�i�X�V�����j
        update_db_status = common_inifile.get('FABRIC_STATUS', 'imaging_end')
        # �B�������ʒm�t�@�C���F�ޔ��f�B���N�g���p�X
        backup_path = inifile.get('PATH', 'backup_path')
        backup_path = backup_root_path + '\\' + backup_path + '\\'
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �B�e�����ʒm�t�@�C�����p�^�[��
        file_name_pattern = inifile.get('PATH', 'file_name_pattern')
        # �����Ώۃ��C���ԍ�
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')
        # �l�b�g���[�N�p�X�G���[���b�Z�[�W
        network_path_error = inifile.get('ERROR_INFO', 'networkpath_error')

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### �B�������ʒm�t�H���_���Ď�����
        while True:

            while True:
                logger.info('[%s:%s] �s�ԎB�������`�F�b�N�@�\���Ăяo���܂��B', app_id, app_name)
                result, scaninfo_file, flag, func_name = check_image_num.main()
                if result:
                    if flag == 'continue':
                        time.sleep(sleep_time)
                        continue
                    else:
                        file_name_pattern = scaninfo_file[0].split('\\')[-1]
                        break

            # �t�H���_���ɎB�������ʒm�t�@�C�������݂��邩�m�F����
            logger.debug('[%s:%s] �B�������ʒm�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
            result, sorted_files, error, func_name = get_file(file_path, file_name_pattern, network_path_error)

            if result == True:
                pass
            elif result == network_path_error:
                logger.warning('[%s:%s] �B�������ʒm�t�@�C���ɃA�N�Z�X�ł��܂���B', app_id, app_name)

                message = '�B�������ʒm�t�@�C���ɃA�N�Z�X�ł��܂���B'
                error_util.write_eventlog_warning(app_name, message)

                time.sleep(sleep_time)
                continue
            else:
                logger.error('[%s:%s] �B�������ʒm�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                sys.exit()

            # �B�������ʒm�t�@�C�����Ȃ��ꍇ�͈�����sleep���čĎ擾
            if len(sorted_files) == 0:
                logger.info('[%s:%s] �B�������ʒm�t�@�C�������݂��܂���B', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            logger.debug('[%s:%s] �B�������ʒm�t�@�C���𔭌����܂����B:�B�������ʒm�t�@�C����[%s]',
                         app_id, app_name, sorted_files)

            sp_sorted_files = []
            for sp_file in sorted_files:
                print(sp_file)
                sp_sorted_files.append(re.split('[_.]', sp_file.split('\\')[-1]))

            print(sp_sorted_files)
            min_date = min([int(x[:][4]) for x in sp_sorted_files])
            print(min_date)
            min_inspection_num = min([int(x[:][3]) for x in sp_sorted_files])
            print(min_inspection_num)

            print(sorted_files)
            sorted_files = [x for x in sorted_files if '_' + str(min_inspection_num) + '_' in x and '_' + str(min_date) + '.CSV']
            print(sorted_files)
            # DB���ʏ������Ăяo���āADB�ڑ����s��
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
            result, error, conn, cur, func_name = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
                sys.exit()

            for i in range(len(sorted_files)):
                basename = os.path.basename(sorted_files[i])
                file_name = re.split('[_.]', basename)
                product_name = file_name[1]
                fabric_name = file_name[2]
                inspection_num = file_name[3]
                inspection_date = file_name[4]
                logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�]=[%s, %s]', app_id, app_name, app_name, fabric_name,
                            inspection_num)

                logger.debug('[%s:%s] �O�������擾���J�n���܂��B' % (app_id, app_name))
                # �������e�[�u������O���������擾����
                result, before_inspection_info, error, conn, cur = select_before_inspection_data(conn, cur, inspection_num,
                                                                                          inspection_date, unit_num)
                if result:
                    logger.debug('[%s:%s] �O�������擾���I�����܂����B' % (app_id, app_name))
                    if before_inspection_info is None:
                        pass
                    elif before_inspection_info[3] != None:
                        before_fabric_name = before_inspection_info[1]
                        before_inspection_num = before_inspection_info[2]
                        before_starttime = before_inspection_info[4]
                        before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                        logger.debug('[%s:%s] �O������� [����, �����ԍ�, �������t]=[%s, %s, %s]' % (
                            app_id, app_name, before_fabric_name, before_inspection_num, before_inspection_date))
                    else:
                        before_fabric_name = before_inspection_info[1]
                        before_inspection_num = before_inspection_info[2]
                        before_starttime = before_inspection_info[4]
                        before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                        logger.info('[%s:%s] �O�����̌����������������݂��܂���B�B�������ʒm��荞�݂𒆎~���܂��B' % (app_id, app_name))
                        logger.info('[%s:%s] �O������� [����, �����ԍ�, �������t]=[%s, %s, %s]' % (
                        app_id, app_name, before_fabric_name, before_inspection_num, before_inspection_date))
                        time.sleep(sleep_time)
                        break

                # �B�������ʒm�t�@�C����Ǎ���
                logger.debug('[%s:%s] �B�������ʒm�t�@�C���̓Ǎ����J�n���܂��B', app_id, app_name)
                result, fabric_name, inspection_num, \
                image_num, imaging_endtime, inspection_line, error, func_name = read_file(sorted_files[i])

                if result:
                    logger.debug('[%s:%s] �B�������ʒm�t�@�C���̓Ǎ����I�����܂����B:�B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, sorted_files[i])
                else:
                    logger.error('[%s:%s] �B�������ʒm�t�@�C���̓Ǎ��Ɏ��s���܂����B:�B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # �B���J�n�������擾����
                org_inspection_date = file_name[4]
                inspection_date = str(
                    org_inspection_date[:4] + '/' + org_inspection_date[4:6] + '/' + org_inspection_date[6:])

                logger.debug('[%s:%s] �B���J�n�����̎擾���J�n���܂��B',
                             app_id, app_name)
                result, fabric_info, error, conn, cur, func_name = \
                    select_fabric_info(conn, cur, fabric_name, inspection_num, unit_num, inspection_date)

                if result:
                    logger.debug('[%s:%s] �B���J�n�����̎擾���I�����܂����B', app_id, app_name)
                    logger.debug('[%s:%s] �����Ώ� [%s]', app_id, app_name, fabric_info)
                    if fabric_info[1] is not None:
                        logger.info('[%s:%s] ���ɎB������������o�^���Ă��܂��B', app_id, app_name)
                        ### �B�������ʒm�t�@�C�����A�ʃt�H���_�֑ޔ�����B
                        # �B�������ʒm�t�@�C����ޔ�����t�H���_�̑��݂��m�F����
                        logger.debug('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                        result, error, func_name = exists_dir(backup_path)

                        if result:
                            logger.debug('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                        else:
                            logger.error('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                         app_id, app_name, backup_path)
                            sys.exit()

                        # �B�������ʒm�t�@�C�����A�ޔ��t�H���_�Ɉړ�������B
                        logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����J�n���܂��B:�B�������ʒm�t�@�C����[%s]',
                                     app_id, app_name, sorted_files[i])
                        result, error, func_name = move_file(sorted_files[i], backup_path)

                        if result:
                            logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����I�����܂����B:�ޔ��t�H���_[%s], �B�������ʒm�t�@�C����[%s]',
                                         app_id, app_name, backup_path, sorted_files[i])
                            break
                        else:
                            logger.error('[%s:%s] �B�������ʒm�t�@�C���̑ޔ��Ɏ��s���܂����B:�B�������ʒm�t�@�C����[%s]',
                                         app_id, app_name, sorted_files[i])
                            sys.exit()
                    else:
                        pass

                else:
                    logger.error('[%s:%s] �B���J�n�����̎擾�����s���܂����B',
                                 app_id, app_name)
                    sys.exit()

                if not fabric_info:
                    logger.error('[%s:%s] �Ώۂ̃f�[�^���o�^����Ă��܂���B [����, �����ԍ�]=[%s, %s]',
                                 app_id, app_name, fabric_name, inspection_num)
                    error = TypeError
                    continue
                else:
                    pass

                imaging_starttime = fabric_info[0]
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                inspection_direction = fabric_info[2]
                inspection_start_line = fabric_info[3]
                end_line = inspection_line

                if inspection_direction == 'S' or 'X':
                    inspection_target_line = int(end_line) - int(inspection_start_line) + 1
                else:
                    inspection_target_line = int(inspection_start_line) - int(end_line) + 1

                # �������w�b�_�[�e�[�u���̍ŏI�s�Ԃ��X�V����B
                logger.debug('[%s:%s] �������w�b�_�[�e�[�u���̍ŏI�s�Ԃ̍X�V���J�n���܂��B', app_id, app_name)
                result, error, conn, cur, func_name = \
                    update_inspection_info_header(fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num,
                                                  end_line, inspection_target_line)
                if result:
                    logger.debug('[%s:%s] �������w�b�_�[�e�[�u���̍ŏI�s�Ԃ̍X�V���I�����܂����B',
                                 app_id, app_name)
                    logger.info('[%s:%s] �������w�b�_�[�e�[�u���̍ŏI�s�Ԃ̍X�V���I�����܂����B [����, �����ԍ�, �������t, �ŏI�s��]=[%s, %s, %s, %s]',
                                app_id, app_name, fabric_name, inspection_num, inspection_date, end_line)
                else:
                    logger.error('[%s:%s] �������w�b�_�[�e�[�u���̍ŏI�s�Ԃ̍X�V�Ɏ��s���܂����B',
                                 app_id, app_name)
                    sys.exit()

                # �B�������ʒm�t�@�C���̏����A�������e�[�u���ɓo�^����
                logger.debug('[%s:%s] �������e�[�u���X�V���J�n���܂��B', app_id, app_name)
                result, error, conn, cur, func_name = \
                    update_fabric_info(update_db_status, image_num, imaging_endtime, fabric_name, inspection_num,
                                       cur, conn, imaging_starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] �������e�[�u���̍X�V���I�����܂����B �B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, sorted_files[i])
                    logger.info('[%s:%s] �������e�[�u���̍X�V���I�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]',
                                app_id, app_name, fabric_name, inspection_num, inspection_date)
                else:
                    logger.error('[%s:%s] �������e�[�u���̍X�V�Ɏ��s���܂����B �B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                # �R�~�b�g����
                conn.commit()

                ### �B�������ʒm�t�@�C�����A�ʃt�H���_�֑ޔ�����B
                # �B�������ʒm�t�@�C����ޔ�����t�H���_�̑��݂��m�F����
                logger.debug('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                result, error, func_name = exists_dir(backup_path)

                if result:
                    logger.debug('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �B�������ʒm�t�@�C����ޔ�����t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                 app_id, app_name, backup_path)
                    sys.exit()

                # �B�������ʒm�t�@�C�����A�ޔ��t�H���_�Ɉړ�������B
                logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����J�n���܂��B:�B�������ʒm�t�@�C����[%s]',
                             app_id, app_name, sorted_files[i])
                result, error, func_name = move_file(sorted_files[i], backup_path)

                if result:
                    logger.debug('[%s:%s] �B�������ʒm�t�@�C���ړ����I�����܂����B:�ޔ��t�H���_[%s], �B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, backup_path, sorted_files[i])
                else:
                    logger.error('[%s:%s] �B�������ʒm�t�@�C���̑ޔ��Ɏ��s���܂����B:�B�������ʒm�t�@�C����[%s]',
                                 app_id, app_name, sorted_files[i])
                    sys.exit()

                logger.info("[%s:%s] %s�����͐���ɏI�����܂����B", app_id, app_name, app_name)

                logger.debug("[%s:%s] ���W�}�[�N���o�^�@�\�ďo���J�n���܂��B", app_id, app_name)
                result, ai_model_flag, error, func_name = \
                    register_regimark_info.main(product_name, fabric_name, inspection_num, imaging_starttime)
                if result:
                    logger.debug("[%s:%s] ���W�}�[�N���o�^�@�\���I�����܂����B", app_id, app_name)
                else:
                    logger.error("[%s:%s] ���W�}�[�N���o�^�@�\�����s���܂����B", app_id, app_name)
                    func_name = '303' + sys._getframe().f_code.co_name
                    sys.exit()

                # AI���f���������t���O���m�F
                # 0�̏ꍇ��NG�s�E�񔻒�o�^�@�\���Ăяo��
                if ai_model_flag == 0 or ai_model_flag == None:

                    logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\�ďo���J�n���܂��B', app_id, app_name)
                    result, error, func_name = register_ng_info.main(product_name, fabric_name, inspection_num,
                                                                     imaging_starttime)
                    if result:
                        logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\���I�����܂����B', app_id, app_name)

                    else:
                        logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\�����s���܂����B', app_id, app_name)
                        func_name = '309' + sys._getframe().f_code.co_name
                        sys.exit()
                else:
                    pass

            # DB�ڑ���ؒf
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)

            # ���̊Ď��Ԋu�܂ŃX���[�v
            time.sleep(sleep_time)

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

        logger.debug('[%s:%s] �G���[�ڍׂ��擾���܂��B' % (app_id, app_name))
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

    except:
        logger.error('[%s:%s] %s�@�\�ŗ\�����Ȃ��G���[���������܂����B[%s]', app_id, app_name, app_name, traceback.format_exc())

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
