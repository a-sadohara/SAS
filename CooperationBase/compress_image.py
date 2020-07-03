# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\310  NG�摜���k�E�]��
# ----------------------------------------

import configparser
import logging.config
import os
import re
import sys
import time
import traceback
import datetime
from concurrent.futures.thread import ThreadPoolExecutor

import win32_setctime

import error_detail
import error_util
import db_util
import file_util
import light_patlite

import compress_image_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_compress_image.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


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
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


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
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F�������擾�iDB�|�[�����O�j
#
# �����T�v           �F1.�������e�[�u�����甽�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
#                    �����X�e�[�^�X�e�[�u���̃X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �����X�e�[�^�X���
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fabric_info_db_polling(conn, cur, fabric_info_status_ng_ziptrans_start, unit_num):
    ### �N�G�����쐬����
    sql = 'select product_name, fabric_name, inspection_num, imaging_starttime, processed_num from fabric_info ' \
          'where status = %s and unit_num = \'%s\' order by ng_endtime asc, imaging_starttime asc' \
          % (fabric_info_status_ng_ziptrans_start, unit_num)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, records, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�t�@�C���擾
#
# �����T�v           �F1.�B���摜�t�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F�B���摜�t�@�C���i�[�t�H���_�p�X
#                      �B���摜�t�@�C��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B�������ʒm�t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    result = False
    file_list = None

    try:
        logger.debug('[%s:%s] �B���摜�t�@�C���i�[�t�H���_�p�X=[%s]', app_id, app_name, file_path)

        # ���ʊ֐��ŎB���摜�t�@�C���i�[�t�H���_�����擾����
        file_list = None
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result:
            # ������
            pass
        else:
            # ���s��
            logger.error("[%s:%s] �B���摜�t�@�C���i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
            return result, file_list

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, file_list


# ------------------------------------------------------------------------------------
# ������             �F�t�H���_���݃`�F�b�N
#
# �����T�v           �F1.�t�H���_�����݂��邩�`�F�b�N����B
#                    2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�t�H���_�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path):
    logger.debug('[%s:%s] �t�H���_���쐬���܂��B�t�H���_���F[%s]',
                 app_id, app_name, target_path)
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F�B���摜�t�@�C���ړ�
#
# �����T�v           �F1.�B���摜�t�@�C�����A�t�H���_�Ɉړ�������B
#
# ����               �F�B���摜�t�@�C��
#                      �ړ���t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    # �t�@�C���ړ�
    ctime = datetime.datetime.fromtimestamp(os.path.getctime(target_file)).timestamp()
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)
    win32_setctime.setctime(move_dir + '\\' + os.path.basename(target_file), ctime)

    return result


# ------------------------------------------------------------------------------------
# ������             �F��t�H���_�폜
#
# �����T�v           �F1.�w�肳�ꂽ�t�H���_��艺�̋�t�H���_���폜����B
#
# ����               �F�폜�t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def delete_empty_dir(del_path):
    result = False
    file_list = []
    try:
        # �t�@�C�������擾
        tmp_result, file_list, error = file_util.get_file_list(del_path, '*', logger, app_id, app_name)
        if tmp_result:
            pass
        else:
            logger.error("[%s:%s] �B���摜�t�@�C���i�[�t�H���_�ɃA�N�Z�X�o���܂���B:�t�H���_��[%s]", app_id, app_name, del_path)
            return result

        for file in file_list:
            if os.path.isdir(file):
                # �t�H���_�̏ꍇ�́A��ł��邩�ǂ������ׂ�
                tmp_result, tmp_file_list, error = file_util.get_file_list(file, '\\*', logger, app_id, app_name)

                if tmp_result:
                    pass
                else:
                    logger.error("[%s:%s] �B���摜�t�@�C���i�[�t�H���_�ɃA�N�Z�X�o���܂���B:�t�H���_��[%s]", app_id, app_name, file)
                    return result
                if tmp_file_list:
                    # ��t�H���_�łȂ��ꍇ�A�������Ȃ�
                    pass
                else:
                    # ��t�H���_�̏ꍇ�A�폜����
                    tmp_result, error = file_util.delete_dir(file, logger, app_id, app_name)

                    if tmp_result:
                        pass
                    else:
                        logger.error("[%s:%s] �B���摜�t�@�C���i�[�t�H���_�̍폜�Ɏ��s���܂����B:�t�H���_��[%s]", app_id, app_name, file)
                        return result

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F�B���摜����
#
# �����T�v           �F1.�B���摜�𐮗�����B
#
# ����               �F�B���摜���[�g�t�H���_
#                     �B���J�n����
#                     �i��
#                     ����
#                     �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def image_organize(image_root_path, date, product_name, fabric_name, inspection_num):
    result = False
    try:
        ### �ݒ�t�@�C������̒l�擾
        # �摜�t�@�C���̊g���q
        extension = common_inifile.get('FILE_PATTERN', 'image_file')
        # �B���摜���J�����ԍ��P�ʂŐ�������B

        # �Q�Ɛ�p�X�\���͈ȉ���z�肷��B
        # �u(�i��)_(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)�v
        #                 �b-(����ID�P)
        #                 �b-(����ID�Q)
        #                 �b-(����ID�R)
        image_path = image_root_path + product_name + '_' + fabric_name + '_' + date + '_' + \
                     str(inspection_num).zfill(2) + '\\'

        # �t�@�C�����p�^�[���͈ȉ���z�肷��B
        # �u\\*\\*.jpg�v
        image_file_name_pattern = '*\\' + extension

        logger.debug('[%s:%s] �B���摜�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        tmp_file_list, image_files = get_file(image_path, image_file_name_pattern)

        if tmp_file_list:
            logger.debug('[%s:%s] �B���摜�t�@�C���̊m�F���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �B���摜�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
            return result

        logger.debug('[%s:%s] �B���摜�t�@�C��:[%s]', app_id, app_name, image_files)

        # �摜������J�����ԍ�����肵�A���݂��鏈��ID�P�ʂ̃t�H���_���J�����ԍ��P�ʂ̃t�H���_�Ƃ��ĉ摜���ړ�������B
        # �Ȃ��A�t�@�C�����́u[�i��]_[����]_[���t]_[�����ԍ�]_ [������No]_[�J�����ԍ�]_[�A��].jpg�v��z�肵�Ă���
        #
        # �u(�i��)_(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)�v
        #                 �b-(����ID)
        # ��
        # �u(�i��)_(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)�v
        #                 �b-[������No]_[�J�����ԍ�]
        for image_file in image_files:
            # �t�@�C�������猟����No�ƃJ�����ԍ����擾
            image_basename = os.path.basename(image_file)
            file_name = re.split('[_.]', image_basename)
            face_no = file_name[4]
            camera_num = file_name[5]

            move_path = image_root_path + product_name + '_' + fabric_name + '_' + date + '_' + \
                        str(inspection_num).zfill(2) + '\\' + str(face_no) + '_' + str(camera_num) + '\\'

            # �t�@�C���i�[�悪���݂��Ȃ���΍쐬
            logger.debug('[%s:%s] �B���摜���ړ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
            tmp_file_list = exists_dir(move_path)

            if tmp_file_list:
                logger.debug('[%s:%s] �B���摜���ړ�����t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] �B���摜���ړ�����t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ړ���t�H���_��[%s]',
                             app_id, app_name, move_path)
                return result

            # �B�������ʒm�t�@�C�����A�ޔ��t�H���_�Ɉړ�������B
            logger.debug('[%s:%s] �B���摜�t�@�C���ړ����J�n���܂��B:�B���摜�t�@�C����[%s]',
                         app_id, app_name, image_files)
            if image_file.find(move_path) == 0:
                # ���Ɉړ��ρi����ID�ƃJ�����ԍ�������ňړ����s�v�j�̏ꍇ
                pass
            else:
                tmp_file_list = move_file(image_file, move_path)
                if tmp_file_list:
                    logger.debug('[%s:%s] �B���摜�t�@�C���ړ����I�����܂����B:�ړ���t�H���_[%s], �B���摜�t�@�C����[%s]',
                                 app_id, app_name, move_path, image_file)
                else:
                    logger.error('[%s:%s] �B���摜�t�@�C���ړ��Ɏ��s���܂����B:�B���摜�t�@�C���@�C����[%s]',
                                 app_id, app_name, image_file)
                    return result

        # �S�Ẳ摜���ړ���������A�c������̃t�H���_���폜����B
        logger.debug('[%s:%s] ��t�H���_�폜���J�n���܂��B', app_id, app_name)

        tmp_file_list = delete_empty_dir(image_path)

        if tmp_file_list:
            logger.debug('[%s:%s] ��t�H���_�폜���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] ��t�H���_�폜�Ɏ��s���܂����B', app_id, app_name)
            return result

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F�������e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.�������e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�X�e�[�^�X
#                      �J������
#                      ���ݓ��t
#                      ����
#                      �����ԍ�
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info_status(status, column, now_datetime, fabric_name, inspection_num, cur, conn, imaging_starttime,
                              unit_num):
    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, now_datetime, fabric_name, inspection_num, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�p�g���C�g�_��
#
# �����T�v           �F1.�p�g���C�g�_�����s���B
#
# ����               �F�o�̓p�X
#                     �o�̓t�@�C����
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def exec_patrite(file_name):
    result, error = file_util.light_patlite(file_name, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.NG�摜���k�E�]�����s���B
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
    try:

        ### �ݒ�t�@�C������̒l�擾
        # �A�g��Ղ̃��[�g�p�X
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # �������e�[�u��.�����X�e�[�^�X�FNG�s�񔻒芮��
        fabric_info_status_ng_end = common_inifile.get('FABRIC_STATUS', 'ng_end')
        # �������e�[�u��.�����X�e�[�^�X�FNG�摜���k�E�]���J�n
        fabric_info_status_zip_start = common_inifile.get('FABRIC_STATUS', 'ng_ziptrans_start')
        # �������e�[�u��.�����X�e�[�^�X�FNG�摜���k�E�]������
        fablic_info_status_zip_end = common_inifile.get('FABRIC_STATUS', 'ng_ziptrans_end')
        # �������e�[�u��.�J�������FNG�摜���k�E�]���J�n����
        fabric_info_column_zip_start = inifile.get('COLUMN', 'ng_ziptrans_start')
        # �������e�[�u��.�J�������FNG�摜���k�E�]����������
        fablic_info_column_zip_end = inifile.get('COLUMN', 'ng_ziptrans_end')
        # �p�g���C�g�_���̃g���K�[�ƂȂ�t�@�C���p�X
        send_patrite_trigger_file_path = inifile.get('PATH', 'send_patrite_trigger_file_path')
        # �p�g���C�g�_���̃g���K�[�ƂȂ�t�@�C����
        send_parrite_file_name = inifile.get('FILE', 'send_parrite_file_name')
        # �B���摜���i�[����p�X
        image_path = inifile.get('PATH', 'image_dir')
        # NG�摜�i�[�p�X
        rapid_ng_image_file_path = inifile.get('PATH', 'rapid_ng_image_file_path')
        # RAPID�T�[�o�z�X�g��
        rapid_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        rapid_hostname_list = rapid_hostname.split(',')
        # �����Ώۃ��C���ԍ�
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### ���k�Ώۓ���iDB�|�[�����O�j
        while True:
            # DB���ʏ������Ăяo���āADB�ڑ����s��
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
                sys.exit()

            # DB���ʏ������Ăяo���āA�������e�[�u������f�[�^���擾����B
            logger.debug('[%s:%s] �������擾���J�n���܂��B', app_id, app_name)
            result, target_records, conn, cur = \
                select_fabric_info_db_polling(conn, cur, fabric_info_status_ng_end, unit_num)

            if result:
                logger.debug('[%s:%s] �������擾���I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] �������擾�Ɏ��s���܂����B', app_id, app_name)
                conn.rollback()
                sys.exit()

            # ������񂪂Ȃ��ꍇ�͈�����sleep���čĎ擾
            if len(target_records) == 0:
                logger.info('[%s:%s] ���k�Ώۂ����݂��܂���B', app_id, app_name)
                time.sleep(sleep_time)
                continue
            else:
                pass

            # DB���ʏ������Ăяo���A�������e�[�u���̈ȉ��̍��ڂ��X�V����B
            for target_record in target_records:

                product_name = target_record[0]
                fabric_name = target_record[1]
                inspection_num = target_record[2]
                imaging_starttime = target_record[3]
                processed_num = int(target_record[4])
                now_datetime = datetime.datetime.now()
                date = imaging_starttime.strftime('%Y%m%d')

                logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]',
                            app_id, app_name, app_name, fabric_name, inspection_num, date)

                logger.debug('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]���J�n�j���J�n���܂��B', app_id, app_name)
                result, conn, cur = \
                    update_fabric_info_status(fabric_info_status_zip_start, fabric_info_column_zip_start, now_datetime,
                                              fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num)

                if result:
                    logger.debug('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]���J�n�j���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]���J�n�j�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # �R�~�b�g����
                conn.commit()
                
                if processed_num == 0:
                    pass
                else:
                    ### �B���摜����
                    logger.debug('[%s:%s] �B���摜�������J�n���܂��B', app_id, app_name)
                    result_list=[]
                    error_list=[]
                    with ThreadPoolExecutor() as executor:
                        func_list = []
                        for rapid_hostname in rapid_hostname_list:
                            target_path = '\\\\' + rapid_hostname + '\\' + image_path + '\\'
                            # �X���b�h���s
                            func_list.append(
                                executor.submit(
                                    image_organize,
                                    target_path, date, product_name, fabric_name, inspection_num))
                        for i in range(len(rapid_hostname_list)):
                            # �X���b�h�߂�l���擾
                            result_list.append(func_list[i].result())

                    for i, multi_result in enumerate(result_list):
                        if multi_result is True:
                            logger.debug('[%s:%s] �B���摜�������������܂����B', app_id, app_name)
                        else:
                            logger.error('[%s:%s] �B���摜�����Ɏ��s���܂����B', app_id, app_name)
                            error_list.append(multi_result)

                    if len(error_list) > 0:
                        sys.exit()
                    else:
                        pass

                ### NG�摜���k�A���������ʒm�쐬�A�t�@�C���]��

                logger.debug('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]�����J�n���܂��B', app_id, app_name)
                tmp_result, error, func_name = compress_image_util.exec_compress_and_transfer(
                    product_name, fabric_name, inspection_num, date,
                    rk_root_path + '\\' + rapid_ng_image_file_path, None, 0, logger)

                if tmp_result:
                    logger.debug('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]�����������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
                    return result

                ### �X�e�[�^�X�X�V
                now_datetime = datetime.datetime.now()
                logger.debug('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]�������j���J�n���܂��B', app_id, app_name)
                result, conn, cur = \
                    update_fabric_info_status(fablic_info_status_zip_end, fablic_info_column_zip_end, now_datetime,
                                              fabric_name, inspection_num, cur, conn, imaging_starttime, unit_num)
                if result:
                    logger.debug('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]�������j���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �������e�[�u���̍X�V�iNG�摜���k�E�]�������j�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # �R�~�b�g����
                conn.commit()

                # �p�g���C�g�_��
                logger.debug('[%s:%s] �p�g���C�g�_���������J�n���܂��B', app_id, app_name)

                light_pattern = '001000'
                result = light_patlite.light_patlite(light_pattern, logger, app_id, app_name)

                if result:
                    logger.debug('[%s:%s] �p�g���C�g�_���������I�����܂����B',
                                 app_id, app_name)
                else:
                    logger.warning('[%s:%s] �p�g���C�g�_�������Ɏ��s���܂����B',
                                   app_id, app_name)

                logger.info('[%s:%s] %s�����͐���ɏI�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]',
                            app_id, app_name, app_name, fabric_name, inspection_num, date)

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
