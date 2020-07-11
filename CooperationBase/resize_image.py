# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\305 �摜���T�C�Y
# ----------------------------------------

import configparser
import sys
from multiprocessing import Pool
import multiprocessing as multi
import os
from PIL import Image
from PIL import ImageFile
import traceback
import datetime
import logging.config
import win32_setctime

import db_util
import error_detail
import error_util
import file_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_resize_image.conf",disable_existing_loggers=False)

# �摜���T�C�Y�ݒ�t�@�C���Ǎ�
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/resize_image_config.ini', 'SJIS')
# ���ʐݒ�t�@�C���Ǎ�
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

ImageFile.LOAD_TRUNCATED_IMAGES = True


# ------------------------------------------------------------------------------------
# ������             �F���T�C�Y����
#
# �����T�v           �F1.�Ώۂ̉摜���J���Ďw��T�C�Y�Ƀ��T�C�Y���ĕۑ�����
#
# ����               �F�B���摜�p�X
#                      �B���摜�i�[�t�H���_�p�X
#                      ��
#                      ����
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def resize_image(file, input_dir, width, height):
    try:
        # �B���摜�t�@�C���p�X����A�摜�t�@�C�������擾����B
        file_name = os.path.basename(file)
        ctime = datetime.datetime.fromtimestamp(os.path.getctime(file)).timestamp()


        # �B���摜���J���B
        img = Image.open(file)

        # �B���摜���A��740�A����648�Ƀ��T�C�Y������B
        rimg = img.resize((width, height), Image.BICUBIC)

        # ���T�C�Y�����摜��ۑ�����B
        rimg.save(input_dir + "\\" + file_name)

        win32_setctime.setctime(file, ctime)

        result = True
        return result

    except Exception as e:

        raise e


# ------------------------------------------------------------------------------------
# ������             �F�摜���T�C�Y��������
#
# �����T�v           �F1.�摜���T�C�Y�����ɑ΂��āA������n��
#
# ����               �F�摜���T�C�Y�������X�g
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def wrapper_resize_image(args):
    # �摜���T�C�Y�����ֈ�����n��

    resize_image(*args)


# ------------------------------------------------------------------------------------
# ������             �F�摜�t�H���_����
#
# �����T�v           �F1.���T�C�Y���s���摜����肷��
#
# ����               �F�B���摜�i�[�p�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���摜���X�g
# ------------------------------------------------------------------------------------
def specific_image(image_file_path, image_pattern, logger):
    func_name = sys._getframe().f_code.co_name
    # ���T�C�Y�Ώۉ摜���X�g���擾����B
    result, file_list, error = file_util.get_file_list(image_file_path, image_pattern, logger, app_id, app_name)

    return result, file_list, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F���s���s����
#
# �����T�v           �F1.�}���`�v���Z�X���쐬���A��������s���s������
#
# ����               �F�B���摜�i�[�p�X
#                      �B���摜���X�g
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def parallel_execution_process(image_file_path, image_file_list, logger):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        # �ݒ�t�@�C������A���T�C�Y���镝�A�����̒l���擾����B
        width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        # �}���`�v���Z�X�ɓn����������������B
        resize_image_args = [[file, image_file_path, width, height]
                             for file in image_file_list]
        logger.debug('[%s:%s] �}���`�v���Z�X��������' % (app_id, app_name))

        # ���s����}���`�v���Z�X����CPU�����画�f����B
        #p = Pool(multi.cpu_count())
        p = Pool(2)

        # ���s���s���������s����B
        logger.debug('[%s:%s] ���񏈗����s' % (app_id, app_name))
        p.map(wrapper_resize_image, resize_image_args)

        # �}���`�v���Z�X�����B
        p.close()

        result = True

    except Exception as error:
        # �G���[�ڍה�����s��
        error_detail.exception(error, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�X�V
#
# �����T�v           �F1.���T�C�Y�J�n�E�������ɏ����X�e�[�^�X���X�V����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ����ID
#                      �X�V�J������
#                      �X�V����
#                      RAPID�T�[�o�[�z�X�g��
#                      �X�e�[�^�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_processing_status(conn, cur, fabric_name, inspection_num,processing_id, column_name, now_datetime,
                             rapid_host_name, status, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    # �N�G�����쐬����
    sql = 'UPDATE processing_status SET status=%s, %s =\'%s\' ' \
          'WHERE fabric_name=\'%s\' AND inspection_num = %s AND processing_id=%s AND rapid_host_name=\'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column_name, now_datetime, fabric_name, inspection_num, processing_id,
           rapid_host_name, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �����X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    # �����X�e�[�^�X�i���T�C�Y�����j���X�V����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�Ώۉ摜����肵�A���T�C�Y�������s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ����ID
#                      RAPID�T�[�o�[�z�X�g��
#                      �B���摜�i�[�p�X
#                      AI���f���������t���O
#                      ���K�[
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def main(conn, cur, fabric_name, inspection_num, processed_id, rapid_host_name, separate_image_path, ai_model_flag,
         date, imaging_starttime, unit_num):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    ### ���O�ݒ�
    logger_name = "resize_image_" + str(rapid_host_name)
    logger = logging.getLogger(logger_name)
    try:

        # �ݒ�t�@�C������̒l�擾
        resize_start_column = inifile.get('COLUMN', 'resize_start')
        resize_end_column = inifile.get('COLUMN', 'resize_end')
        resize_start_status = common_inifile.get('PROCESSING_STATUS', 'resize_start')
        resize_end_status = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        rapid_model = common_inifile.get('PROCESSING_STATUS', 'rapid_model')
        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')

        # �ϐ��ݒ�

        logger.info('[%s:%s] %s�@�\���N�����܂��B' % (app_id, app_name, app_name))
        logger.info(
            '[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s] ' 
            % (app_id, app_name, app_name, fabric_name, inspection_num, date))
        logger.debug('[%s:%s] ���T�C�Y�Ώۂ̎B���摜������J�n���܂��B' % (app_id, app_name))

        image_file_name = "\\*" + fabric_name + "_" + date + "_" + str(inspection_num).zfill((2)) + image_file_pattern

        # ���T�C�Y�摜����肷��B
        result, image_file_list, error, func_name = specific_image(separate_image_path, image_file_name, logger)

        if result:
            logger.debug('[%s:%s] ���T�C�Y�Ώۂ̎B���摜���肪�I�����܂����B' % (app_id, app_name))
            logger.debug('[%s:%s] ���T�C�Y�Ώۂ̎B���摜 = [%s]' % (app_id, app_name, image_file_list))

        else:
            logger.debug('[%s:%s] ���T�C�Y�Ώۂ̎B���摜����Ɏ��s���܂����B' % (app_id, app_name))
            sys.exit()


        logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�J�n�j�̍X�V���J�n���܂����B' % (app_id, app_name))

        # �����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����i���T�C�Y�J�n�j
        now_datetime = datetime.datetime.now()
        result, error, conn, cur, func_name = \
            update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_start_column,
                                     now_datetime, rapid_host_name, resize_start_status, imaging_starttime, unit_num,
                                     logger)
        if result:
            logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�J�n�j�̍X�V���I�����܂����B' % (app_id, app_name))
            conn.commit()

        else:
            logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�J�n�j�̍X�V�Ɏ��s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] ���T�C�Y�����̕�����s���J�n���܂��B' % (app_id, app_name))

        # �摜���T�C�Y��������s���s����B
        result, error, func_name = parallel_execution_process(separate_image_path, image_file_list, logger)

        if result:
            logger.debug('[%s:%s] ���T�C�Y�����̕�����s���I�����܂����B' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] ���T�C�Y�����̕�����s�����s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�����j�̍X�V���J�n���܂����B' % (app_id, app_name))

        if ai_model_flag == 0:
            now_datetime = datetime.datetime.now()
            # �����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����i���T�C�Y�����j
            result, error, conn, cur, func_name = \
                update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_end_column,
                                         now_datetime, rapid_host_name, resize_end_status, imaging_starttime,
                                         unit_num, logger)
        else:
            # �����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����i�w�K�p�摜���T�C�Y�����j
            now_datetime = datetime.datetime.now()
            result, error, conn, cur, func_name = \
                update_processing_status(conn, cur, fabric_name, inspection_num, processed_id, resize_end_column,
                                         now_datetime, rapid_host_name, rapid_model, imaging_starttime, unit_num,
                                         logger)

        if result:
            logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�����j�̍X�V���I�����܂����B' % (app_id, app_name))
            conn.commit()

        else:
            logger.debug('[%s:%s] �����X�e�[�^�X�i���T�C�Y�����j�̍X�V�Ɏ��s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] %s�����͐���ɏI�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s] ' 
            % (app_id, app_name, app_name, fabric_name, inspection_num, date))

        result = True
        return result, conn, cur, error, func_name

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

        logger.debug('[%s:%s] �G���[�ڍׂ��擾���܂��B' % (app_id, app_name))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)

        logger.error('[%s:%s] %s [�z�X�g��, �G���[�R�[�h:%s, %s]' % (app_id, app_name, error_message, rapid_host_name, error_id))

        event_log_message = '[�@�\��, �z�X�g��, �G���[�R�[�h]=[%s, %s, %s] %s' % (app_name, rapid_host_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message)

        result = False

    except Exception as error:
        # �z��O�G���[����
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, conn, cur, error, func_name
