# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\303  ���W�}�[�N���o�^
# ----------------------------------------
import codecs
import configparser
import sys
import datetime
import time
import logging.config
import traceback
import os
import re
import datetime

import db_util
import file_util
import error_detail
import error_util
import register_ng_info

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_regimark_info.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_regimark_info")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_regimark_info_config.ini', 'SJIS')

error_inifile = configparser.ConfigParser()
error_inifile.read('D:/CI/programs/config/error_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ڑ�
#
# �����T�v           �F1.DB�Ɛڑ�����
#
# ����               �F�@�\ID

#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_connection():
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F���W�}�[�N�}�X�^�[���擾
#
# �����T�v           �F1.�i��o�^���e�[�u�����烌�W�}�[�N�}�X�^�[�����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ���W�}�[�N�}�X�^���(�ݒ背�W�}�[�N�����A�L�k��X���A�L�k��Y��)
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
#
# ------------------------------------------------------------------------------------
def select_master_data(conn, cur, product_name):
    ### �N�G�����쐬����
    sql = 'select regimark_between_length, stretch_rate_x, stretch_rate_y, ai_model_non_inspection_flg ' \
          'from mst_product_info where product_name = \'%s\'' % product_name

    logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾SQL %s' % (app_id, app_name, sql))
    ### �i��o�^���e�[�u������f�[�^�擾
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F�������擾
#
# �����T�v           �F1.�������w�b�_�[�e�[�u�����猟�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����J�n�s
#                      �ŏI�s��
#                      ��������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, product_name, fabric_name, inspection_num, start_datetime, unit_num):
    ### �N�G�����쐬����
    sql = 'select inspection_start_line, inspection_end_line, inspection_direction ' \
          'from inspection_info_header where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, start_datetime, unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    ### �������w�b�_�[�e�[�u������f�[�^�擾
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F���W�}�[�N���o�^�ς݊m�F
#
# �����T�v           �F1.�J�n/�I�����W�}�[�N�ǎ挋�ʃt�@�C�������ԍ��ŘA�g���ꂽ�ꍇ�A
#                       �ǂ��炩�����łɓo�^�ς݂ł��邩�m�F����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����J�n�s
#                      �ŏI�s��
#                      ��������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_regimark_info(conn, cur, product_name, fabric_name, inspection_num, line_num,
                         regimark_face, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select * from regimark_info where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' ' \
          'and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, line_num, regimark_face, imaging_starttime, unit_num)

    logger.debug('[%s:%s] ���W�}�[�N���o�^�ς݊m�FSQL %s' % (app_id, app_name, sql))
    ### �������w�b�_�[�e�[�u������f�[�^�擾
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F���W�}�[�N���o�^
#
# �����T�v           �F1.���W�}�[�N���e�[�u���փ��W�}�[�N����o�^����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ���W�}�[�N���
#                      �i��
#                      ����
#                      �����ԍ�
#                      �ݒ背�W�}�[�N����
#                      ���摜����(Y��)
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def insert_regimark_info(conn, cur, regimark, regimark_between_length_list, strech_ratio_list,
                         product_name, fabric_name, inspection_num, conf_regimark_len, image_height, image_width,
                         regimark_type, regimark_face, imaging_starttime, unit_num):
    result = False
    sql = None
    try:
        # �B���摜(���T�C�Y�摜)�̕�(pix)
        resize_image_width = float(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        # �B���摜(���T�C�Y�摜)�̍���(pix)
        resize_image_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        for i in range(len(regimark)):

            line_num = regimark[i][0]
            tmp_result, registed_info, conn, cur = select_regimark_info(conn, cur, product_name, fabric_name,
                                                                        inspection_num, line_num, regimark_face,
                                                                        imaging_starttime, unit_num)

            if tmp_result:
                pass
            else:
                # ���s��
                logger.error("[%s:%s] �o�^�ς݃��W�}�[�N���m�F�����s���܂����B", app_id, app_name)
                return result

            # ���ꔽ�ԁA�����ԍ��A�s�ԍ������o�^�̏ꍇ
            if len(registed_info) == 0:
                # �o�^�Ώۂ��J�n���W�}�[�N�̏ꍇ
                if re.split('\.', regimark_type)[0] == '1':
                    # �o�^�ΏۂɎ������W�}�[�N�A����/���L�k�����܂܂��ꍇ
                    # �J�n���W�}�[�N�̏ꍇ�A����������S�AX�̍ۂɎ������W�}�[�N���Z�o���Ă���
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'setting_regimark_length, actual_regimark_length, len_stretchrate, width_stretchrate, ' \
                              'start_regimark_file, start_regimark_point_org, start_regimark_point_resize, face, ' \
                              'imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, %s, %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, conf_regimark_len,
                                 actual_regimark_length,
                                 len_stretchrate, width_stretchrate, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize, face, imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                # �o�^�Ώۂ��I�����W�}�[�N�̏ꍇ
                else:
                    # �o�^�ΏۂɎ������W�}�[�N�A����/���L�k�����܂܂��ꍇ
                    # �I�����W�}�[�N�̏ꍇ�A����������Y�AR�̍ۂɎ������W�}�[�N���Z�o���Ă���
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'setting_regimark_length, actual_regimark_length, len_stretchrate, width_stretchrate, ' \
                              'end_regimark_file, end_regimark_point_org, end_regimark_point_resize, face, ' \
                              'imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, %s, %s, %s, %s, \'%s\', \'%s\', \'%s\',\'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, conf_regimark_len,
                                 actual_regimark_length,
                                 len_stretchrate, width_stretchrate, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'insert into regimark_info (product_name, fabric_name, inspection_num, line_num, ' \
                              'end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize, face, imaging_starttime, unit_num) values ' \
                              '(\'%s\', \'%s\', %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', \'%s\')' \
                              % (product_name, fabric_name, inspection_num, line_num, image_name, regimark_point_org,
                                 regimark_point_resize, regimark_face, imaging_starttime, unit_num)


            # ���ꔽ�ԁA�����ԍ��A�s�ԍ����o�^�ς̏ꍇ
            else:
                # �o�^�Ώۂ��J�n���W�}�[�N�̏ꍇ
                if re.split('\.', regimark_type)[0] == '1':
                    # �o�^�ΏۂɎ������W�}�[�N�A����/���L�k�����܂܂��ꍇ
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'update regimark_info set (setting_regimark_length, actual_regimark_length, ' \
                              'len_stretchrate, width_stretchrate, start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize) = (%s, %s, %s, %s, \'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and inspection_num = %s ' \
                              'and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
                              % (
                                  conf_regimark_len, actual_regimark_length, len_stretchrate, width_stretchrate,
                                  image_name,
                                  regimark_point_org, regimark_point_resize, product_name, fabric_name, inspection_num,
                                  line_num, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'update regimark_info set (start_regimark_file, start_regimark_point_org, ' \
                              'start_regimark_point_resize) = (\'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and inspection_num = %s ' \
                              'and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' and unit_num =\'%s\'' \
                              % (image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                 inspection_num, line_num, regimark_face, imaging_starttime, unit_num)
                # �o�^�Ώۂ��I�����W�}�[�N�̏ꍇ
                else:
                    # �o�^�ΏۂɎ������W�}�[�N�A����/���L�k�����܂܂��ꍇ
                    if regimark_between_length_list != None:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = strech_ratio_list[i][0]
                        width_stretchrate = strech_ratio_list[i][1]
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'update regimark_info set (setting_regimark_length, actual_regimark_length, ' \
                              'len_stretchrate, width_stretchrate, end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize) = (%s, %s, %s, %s, \'%s\', \'%s\', \'%s\') ' \
                              'where product_name = \'%s\'and fabric_name = \'%s\' and ' \
                              'inspection_num = %s and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\'' \
                              'and unit_num = \'%s\'' \
                              % (
                                  conf_regimark_len, actual_regimark_length, len_stretchrate, width_stretchrate,
                                  image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                  inspection_num, line_num, regimark_face, imaging_starttime, unit_num)
                    else:
                        image_name = re.sub('_jpg', '.jpg', '_'.join(regimark[i][1:9]))
                        regimark_point_org = "(" + regimark[i][10] + "," + regimark[i][11] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][10]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][11]) / image_height)) + ")"

                        ### �N�G�����쐬����
                        sql = 'update regimark_info set (end_regimark_file, end_regimark_point_org, ' \
                              'end_regimark_point_resize) = ' \
                              '(\'%s\', \'%s\', \'%s\') where product_name = \'%s\' and fabric_name = \'%s\' and ' \
                              'inspection_num = %s and line_num =%s and face = \'%s\' and imaging_starttime = \'%s\'' \
                              'and unit_num = \'%s\'' \
                              % (image_name, regimark_point_org, regimark_point_resize, product_name, fabric_name,
                                 inspection_num, line_num, regimark_face, imaging_starttime, unit_num)

            logger.debug('[%s:%s] ���W�}�[�N���o�^SQL %s' % (app_id, app_name, sql))
            # ���W�}�[�N����o�^����
            tmp_result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

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
    result = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# �֐���             �F�t�@�C���擾
#
# �����T�v           �F1.���W�}�[�N�ǎ挋�ʃt�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F���W�}�[�N�ǎ挋�ʃt�@�C���i�[�t�H���_�p�X
#                      ���W�}�[�N�ǎ挋�ʃt�@�C����
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ���W�}�[�N�ǎ挋�ʃt�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_regimark_file(file_path, file_name_pattern):
    result = False
    sorted_files = None

    try:

        # ���ʊ֐��Ń��W�}�[�N�ǎ挋�ʊi�[�t�H���_�����擾����
        tmp_result, file_list = file_util.get_file_list(file_path, file_name_pattern, logger, app_id, app_name)

        if tmp_result:
            # ������
            pass
        else:
            # ���s��
            logger.error("[%s:%s] ���W�}�[�N�ǎ挋�ʊi�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
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
def read_regimark_file(file):
    result = False
    regimark_info = []

    try:
        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C��=%s', app_id, app_name, file)
        # ���W�}�[�N�ǎ挋�ʃt�@�C���p�X����t�@�C�������擾���A���ԁA�����ԍ����擾����
        # �Ȃ��A�t�@�C�����́uRM_�i��_����_�����ԍ�_���t_������_���W�}�[�N���.CSV�v��z�肵�Ă���

        # ���W�}�[�N�ǎ挋�ʃt�@�C������A���ڂ��擾����
        with codecs.open(file, "r", "SJIS") as f:
            notification = [re.sub('\r', '', s[:-1]).split(',') for s in f.readlines()][1:]

        regimark_info = notification
        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, regimark_info


# ------------------------------------------------------------------------------------
# �֐���             �F���W�}�[�N�ǎ�d���m�F
#
# �����T�v           �F1.���X�g��������񂩂�A���W�}�[�N���m�摜�̃J�����ԍ��A�B���ԍ���
#                      �A�ԂƂȂ��Ă�����̂��m�F����B
#                      2.�A�ԂƂȂ��Ă�����̂����݂���ꍇ�A�̗p���W�}�[�N������Ăяo���A
#                      ���W�}�[�N�Ƃ��č̗p����摜�̓�����s���B
#
# ����               �F�J�n���W�}�[�N���X�g/�I�����W�}�[�N���X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �J�n���W�}�[�N���X�g/�I�����W�}�[�N���X�g
#
# ------------------------------------------------------------------------------------
def confirm_duplicate_registremark(regimark_list, fabric_name, inspection_num, inspection_info, image_height,
                                   image_width):
    result = False
    confirmed_regimark_list = []
    line_num = None

    try:
        inspection_start_line = inspection_info[0]
        inspection_end_line = inspection_info[1]
        inspection_direction = inspection_info[2]

        # ���W�}�[�N��񃊃X�g�����������s���B(�J�n���W�}�[�N���X�g�A�I�����W�}�[�N���X�g��2��)
        for i in range(len(regimark_list)):
            # ���������� S or X�̏ꍇ�A�J�n�s������B������邽�ߍs�ԍ��̏����l�������J�n�s���Ƃ���B
            if inspection_direction == 'S' or inspection_direction == 'X':
                line_num = inspection_start_line
            # ���������� Y or R�̏ꍇ�A�ŏI�s������B������邽�ߍs�ԍ��̏����l���ŏI�s�ԂƂ���B
            elif inspection_direction == 'Y' or inspection_direction == 'R':
                line_num = inspection_start_line

            logger.debug('[%s:%s] ���W�}�[�N���X�g %s', app_id, app_name, regimark_list[i])
            # �摜�t�@�C������[._]�ŕ���
            sp_list = [re.split('[_.]', x[:][0]) + x[1:] for x in regimark_list[i]]
            logger.debug('[%s:%s] �X�v���b�g���W�}�[�N���X�g %s', app_id, app_name, sp_list)

            # ���W�}�[�N�ǎ挋�ʓ��̃��R�[�h�����������s���B
            while len(sp_list) > 0:
                # �d���m�F�p�L�[�𒊏o(�B���ԍ��A�J�����ԍ�)
                key_image_num = int(sp_list[0][6])
                key_cam_num = int(sp_list[0][5])

                # �ȉ������ɓ��Ă͂܂郌�W�}�[�N�ǎ挋�ʂ𒊏o����
                #     �J�����ԍ��{�P ���� �B���ԍ������摜������
                #     �J�����ԍ����� ���� �B���ԍ��{�P�̉摜������
                #     �J�����ԍ��{�P ���� �B���ԍ��{�P�̉摜������
                duplicate_registremarks = [s for s in sp_list if
                                           (key_image_num == int(s[:][6]) and key_cam_num == int(s[:][5])) or \
                                           ((key_image_num + 1) == int(s[:][6]) and key_cam_num == int(s[:][5])) or \
                                           (key_image_num == int(s[:][6]) and (key_cam_num + 1) == int(s[:][5])) or \
                                           ((key_image_num + 1) == int(s[:][6]) and (key_cam_num + 1) == int(s[:][5]))]

                # �����ɓ��Ă͂܂郌�W�}�[�N�ǎ挋�ʂ�1���̏ꍇ�́A���W�}�[�N���Ƃ��ė��p����B
                if len(duplicate_registremarks) == 1:
                    add_line_num = duplicate_registremarks
                    add_line_num[0].insert(0, line_num)
                    confirmed_regimark_list += add_line_num

                # �����ɓ��Ă͂܂郌�W�}�[�N�ǎ挋�ʂ�2���ȏ㑶�݂���ꍇ�́A�̗p���W�}�[�N���菈�������s����B
                else:
                    tmp_result, specificed_regimark_list = specific_regimark(duplicate_registremarks, image_height,
                                                                             image_width)

                    if tmp_result:
                        logger.debug('[%s:%s] �̗p���W�}�[�N���肪�I�����܂����B:[����,�����ԍ�]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        logger.debug('[%s:%s] �̗p���W�}�[�N :%s',
                                     app_id, app_name, specificed_regimark_list)
                    else:
                        logger.error('[%s:%s] �̗p���W�}�[�N����Ɏ��s���܂����B:[����,�����ԍ�]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        return result, confirmed_regimark_list

                    add_line_num = specificed_regimark_list
                    add_line_num.insert(0, line_num)
                    confirmed_regimark_list += [add_line_num]

                # �����σ��W�}�[�N�������X�g����폜����B
                for image in duplicate_registremarks:
                    sp_list.remove(image)

                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_num += 1
                elif inspection_direction == 'Y' or inspection_direction == 'R':
                    line_num -= 1

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, confirmed_regimark_list


# ------------------------------------------------------------------------------------
# �֐���             �F�̗p���W�}�[�N����
#
# �����T�v           �F1.���W�}�[�N�ǎ�摜���d�������ꍇ�A���W�}�[�N�Ƃ��ė��p����摜����肷��B
#                      2.�J�����ԍ��A�B���ԍ����m�F����B
#                      3.�J�����ԍ����A�Ԃ̏ꍇ�́A�B���摜�̒[����̕�(X��)����̗p���郌�W�}�[�N����肷��B
#                      4.�B���ԍ����A�Ԃ̏ꍇ�́A�B���摜�̒[����̍���(Y��)����̗p���郌�W�}�[�N����肷��
#                      5.�J�����ԍ����A�ԁA�B���ԍ����A�Ԃ̏ꍇ�A�B���摜�̒[����̕�(X��)�A����(Y��)���ꂼ�ꂩ��̗p���郌�W�}�[�N����肷��
#
# ����               �F�d�������݂��郌�W�}�[�N���
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �̗p���W�}�[�N���
#
# ------------------------------------------------------------------------------------
def specific_regimark(duplicate_registremarks, image_height, image_width):
    result = False
    pickup_regimark = []

    try:
        # �L�[�ƂȂ�J�����ԍ��𒊏o
        min_camera_num = min([int(mi_cam_num[5]) for mi_cam_num in duplicate_registremarks])
        min_image_num = min([int(mi_img_num[6]) for mi_img_num in duplicate_registremarks])

        ng_face = duplicate_registremarks[0][4]

        regimark_points = []
        x_regimark_points = []
        y_regimark_points = []

        # ���W�}�[�N�ǎ�摜�d����2���̏ꍇ
        if len(duplicate_registremarks) == 2:
            # �B���ԍ����������m�F���A�������ꍇ�́AX�������̒����ō̗p���W�}�[�N����肷��B
            if int(duplicate_registremarks[0][6]) == int(duplicate_registremarks[1][6]):
                if ng_face == '1':
                    for k in range(len(duplicate_registremarks)):
                        if int(duplicate_registremarks[k][5]) == min_camera_num:
                            x_point = int(duplicate_registremarks[k][9])
                            regimark_points.append(x_point)
                        elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                            x_point = (image_width - int(duplicate_registremarks[k][9]))
                            regimark_points.append(x_point)
                else:
                    for k in range(len(duplicate_registremarks)):
                        if int(duplicate_registremarks[k][5]) == min_camera_num:
                            x_point = image_width - int(duplicate_registremarks[k][9])
                            regimark_points.append(x_point)
                        elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                            x_point = (int(duplicate_registremarks[k][9]))
                            regimark_points.append(x_point)
            # �J�����ԍ������������m�F���A�������ꍇ�́AY�������̒����ō̗p���W�}�[�N����肷��B
            elif int(duplicate_registremarks[0][5]) == int(duplicate_registremarks[1][5]):
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][6]) == min_image_num:
                        y_point = (image_height - int(duplicate_registremarks[k][10]))
                        regimark_points.append(y_point)
                    elif int(duplicate_registremarks[k][6]) == (min_image_num + 1):
                        y_point = int(duplicate_registremarks[k][10])
                        regimark_points.append(y_point)

            # �̗p�������W�}�[�N��񂪃��X�g���̂ǂ̈ʒu�Ɋi�[����Ă��邩�m�F���A���W�}�[�N���𒊏o����B
            index = regimark_points.index(max(regimark_points))
            pickup_regimark = duplicate_registremarks[index]

            result = True

        # ���W�}�[�N�ǎ�摜�d����3���ȏ�̏ꍇ
        elif len(duplicate_registremarks) >= 3:
            # X�������̒������m�F���A�ł��傫���摜�̃J�����ԍ����̗p����B
            if ng_face == '1':
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][5]) == min_camera_num:
                        x_point = (int(duplicate_registremarks[k][9]))
                        x_regimark_points.append(x_point)
                    elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                        x_point = image_width - int(duplicate_registremarks[k][9])
                        x_regimark_points.append(x_point)
            else:
                for k in range(len(duplicate_registremarks)):
                    if int(duplicate_registremarks[k][5]) == min_camera_num:
                        x_point = (image_width - int(duplicate_registremarks[k][9]))
                        x_regimark_points.append(x_point)
                    elif int(duplicate_registremarks[k][5]) == (min_camera_num + 1):
                        x_point = int(duplicate_registremarks[k][9])
                        x_regimark_points.append(x_point)

            # X�������̒����������Ƒ傫�����W�}�[�N���̈ʒu����肵�A�Y���J�����ԍ��Ɠ������摜�𒊏o����B
            x_index = x_regimark_points.index(max(x_regimark_points))
            new_duplicate_registremarks = [y for y in duplicate_registremarks if
                                           duplicate_registremarks[x_index][5] in y[5]]

            # �Y���J�����ԍ��Ɠ������摜�̎B���ԍ��Ɠ������摜��Y�������̒������m�F���A�ł��傫���摜���m�F����B
            for m in range(len(new_duplicate_registremarks)):
                if int(new_duplicate_registremarks[m][6]) == min_image_num:
                    y_point = (image_height - int(new_duplicate_registremarks[m][10]))
                    y_regimark_points.append(y_point)
                elif int(new_duplicate_registremarks[m][6]) == (min_image_num + 1):
                    y_point = int(new_duplicate_registremarks[m][10])
                    y_regimark_points.append(y_point)

            # Y�������̒������ł��傫�����W�}�[�N���̈ʒu����肵�A�̗p���W�}�[�N����肷��B
            y_index = y_regimark_points.index(max(y_regimark_points))
            pickup_regimark = new_duplicate_registremarks[y_index]

            result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, pickup_regimark


# ------------------------------------------------------------------------------------
# �֐���             �F�������W�}�[�N�Ԓ����Z�o
#
# �����T�v           �F1.�s�Ԃ̎B���摜�������A�������W�}�[�N�Ԓ������Z�o����B
#
# ����               �F�d���m�F�ς݃��W�}�[�N���
#                      ���摜�̕�(X��)(�I�[�o�[���b�v���O��)
#                      ���摜�̃I�[�o�[���b�v�̕�(X��)
#                      �ݒ背�W�}�[�N�Ԓ���
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �������W�}�[�N�Ԓ���
#
# ------------------------------------------------------------------------------------
def calc_actual_regimark_between_length(confirmed_regimark, nonoverlap_image_length, overlap_length,
                                        conf_regimark_between_length, actual_image_height, one_pix_length):
    result = False
    actual_regimark_between_length_list = []

    try:
        # ���W�}�[�N���X�g��1���̏ꍇ�A���s�J�n�Ƃ̊Ԃ̒������m�F�ł��Ȃ����߁A�ݒ背�W�}�[�N�Ԓ����𗘗p����B
        if len(confirmed_regimark) == 1:
            actual_regimark_between_length_list.append(conf_regimark_between_length)

        else:
            # �J�n���W�}�[�N���Ǝ��s�J�n���W�}�[�N���𗘗p���āA�������W�}�[�N�Ԓ������Z�o����B
            for i in range(len(confirmed_regimark)):
                if i != (len(confirmed_regimark) - 1):

                    start_image_num = int(confirmed_regimark[i][7])
                    start_regmark_y = int(confirmed_regimark[i][11])

                    next_start_image_num = int(confirmed_regimark[i + 1][7])
                    next_start_regmark_y = int(confirmed_regimark[i + 1][11])

                    between_image_count = next_start_image_num - start_image_num - 1
                    # �B������(���W�}�[�N�������O)�~image_length�{(���摜���� - [�J�n���W�}�[�Ny���W * 1pix������̒���]-[overlap_length])�{[���s�J�n���W�}�[�Ny���W * 1pix������̒���]
                    regimark_length_mm = between_image_count * nonoverlap_image_length + (
                            actual_image_height - (int(start_regmark_y) * one_pix_length) - overlap_length) + \
                                         (int(next_start_regmark_y) * one_pix_length)
                    actual_regimark_between_length_list.append(regimark_length_mm)


                # ���W�}�[�N���X�g���Ō��1���̏ꍇ�A���s�J�n�Ƃ̊Ԃ̒������m�F�ł��Ȃ����߁A�O�s�̃��W�}�[�N�Ԓ����𗘗p����B
                else:
                    actual_regimark_between_length_list.append(actual_regimark_between_length_list[-1])

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, actual_regimark_between_length_list


# ------------------------------------------------------------------------------------
# �֐���             �F����/���L�k���Z�o
#
# �����T�v           �F1.�����L�k��/���L�k�����Z�o����
#                         Xa�F�ݒ背�W�}�[�N�Ԓ���
#                         Xb�F�̈�L�ї�X��
#                         Yb�F�̈�L�ї�Y��
#                         Xa*Xb�F�̈�L�ї��̕��ϒl
#                         Xd�F�������W�}�[�N��
#
#                         �����L�k�� �� Xd  / Xa
#                         ���L�k�� = Yb * (Xa*Xb)/Xd
#
# ����               �F�������W�}�[�N�Ԓ���
#                      �ݒ背�W�}�[�N�Ԓ���
#                      �L�k��X��
#                      �L�k��Y��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �L�k��(�����L�k���A���L�k��)
#
# ------------------------------------------------------------------------------------
def calc_stretch_ratio(actual_regimark_between_length_list, conf_regimark_between_length, stretch_rate_x,
                       stretch_rate_y):
    result = False
    strech_ratio_list = []

    try:
        for i in range(len(actual_regimark_between_length_list)):
            length_strech_ratio = (actual_regimark_between_length_list[i] / conf_regimark_between_length) * 100
            width_strech_ratio = (stretch_rate_y / 100 * (conf_regimark_between_length * stretch_rate_x / 100) /
                                  actual_regimark_between_length_list[i]) * 100
            ratio = [length_strech_ratio, width_strech_ratio]
            strech_ratio_list.append(ratio)
        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, strech_ratio_list


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
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F���W�}�[�N�ǎ挋�ʃt�@�C���ޔ�
#
# �����T�v           �F1.���W�}�[�N�ǎ挋�ʃt�@�C�����A�ޔ��t�H���_�Ɉړ�������B
#
# ����               �F���W�}�[�N�ǎ挋��
#                      �ޔ��t�H���_
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def move_file(target_file, move_dir):
    # �t�@�C���ړ�
    result = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# �֐���             �F���W�}�[�N�ԋ���csv�o��
#
# �����T�v           �F1.���W�}�[�N�ԋ����Z�o���ʂ�csv�t�@�C���ɏo�͂���B
#
# ����               �F�����σ��W�}�[�N���
#                      �ݒ背�W�}�[�N�Ԓ���
#                      csv�o�͐�p�X
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def output_regimark_between_length_file(confirmed_regimark, regimark_between_length_list, strech_ratio_list,
                                        conf_regimark_between_length, output_file_path, fabric_name, inspection_num):
    result = False
    date = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    file_name = str(date) + "_" + fabric_name + "_" + inspection_num.zfill(3) + "_N.csv"

    try:
        if os.path.exists(output_file_path + '\\' + file_name):
            # ���݂���ꍇ�A�ޔ��t�@�C���ɕʖ��i���j������B
            # �i���j�ޔ��t�@�C���̍ŏI�X�V�����iYYYYMMDD_HHMMSS�j���A�t�@�C�����̖����ɕt�^����B
            # xxxx.csv  ��  xxxx.csv.20200101_235959
            timestamp = datetime.datetime.now()
            file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
            file_name = file_name + '.' + file_timestamp

        csv_file = output_file_path + "\\" + file_name
        with codecs.open(output_file_path + "\\" + file_name, "w", "SJIS") as f:
            f.write("�s�ԍ�,�ݒ背�W�}�[�N�Ԓ���(mm),�������W�}�[�N�Ԓ���(mm),�����L�k��(),���L�k��()")
            f.write("\r\n")

            for i in range(len(confirmed_regimark)):
                line_num = confirmed_regimark[i][0]
                conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                actual_regimark_len = '{:.6f}'.format(float(regimark_between_length_list[i]))
                length_ratio = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                width_ratio = '{:.6f}'.format(float(strech_ratio_list[i][1]))

                string = str(line_num) + "," + str(conf_regimark_len) + "," + str(actual_regimark_len) + "," + str(
                    length_ratio) + "," + str(width_ratio)

                f.write(string)
                f.write("\r\n")

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, csv_file


# ADD 20200626 KQRM ���g START
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
def output_dummy_file(product_name, fabric_name, inspection_num, inspection_date):
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

        for i in range(0, 2):
            for j in range(0, 2):
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(i + 1) + "_" + str(
                    j + 1) + file_extension
                with codecs.open(csv_file, "w", "SJIS") as f:
                    f.write("�B���t�@�C����,���,���W��,���W��")
                    f.write("\r\n")

        # DEL 20200626 KQRM ���g START
        # csv_file = output_file_path + "\\" + scaninfo_path + "\\" + scaninfo_file_name
        # with codecs.open(csv_file, "w", "SJIS") as f:
        #     f.write("�J�����䐔,�J����1��̎B������,���B������")
        #     f.write("\r\n")
        #     f.write("54,0,0")
        #     f.write("\r\n")
        # DEL 20200626 KQRM ���g END

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result

# ADD 20200626 KQRM ���g END

# ------------------------------------------------------------------------------------
# �֐���             �F���C������
#
# �����T�v           �F1.���W�}�[�N�ǂݎ�茋�ʃt�@�C�����Ď�����
#                      2.�������W�}�[�N���Z�o����
#                      3.�������W�}�[�N�ԋ���CSV���o�͂���
#                      4.���W�}�[�N����DB�ɓo�^����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, imaging_starttime):
    # �R�l�N�V�����I�u�W�F�N�g, �J�[�\���I�u�W�F�N�g
    conn = None
    cur = None
    error_file_name = None
    conf_regimark_len = 'null'
    strech_ratio_list = None
    ai_model_flag = None
    try:
        # �ϐ���`
        ### �ݒ�t�@�C������̒l�擾
        # ���ʐݒ�F�e��ʒm�t�@�C�����i�[����郋�[�g�p�X
        input_root_path = common_inifile.get('FILE_PATH', 'input_path')
        # ���ʐݒ�F�e��ʒm�t�@�C����ޔ������郋�[�g�p�X
        backup_root_path = common_inifile.get('FILE_PATH', 'backup_path')
        # ���W�}�[�N�ǎ挋�ʂ��i�[�����t�H���_�p�X
        file_path = inifile.get('PATH', 'file_path')
        file_path = input_root_path + '\\' + file_path + '\\'
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # ���W�}�[�N�ǎ挋�ʃt�@�C�����p�^�[��
        file_name_pattern = inifile.get('PATH', 'file_name_pattern')
        # ���W�}�[�N�ǎ挋�ʊg���q�p�^�[��
        file_extension_pattern = inifile.get('PATH', 'file_extension_pattern')
        # ���W�}�[�N�ԋ���csv���o�͂���t�H���_�p�X
        output_file_path = inifile.get('PATH', 'output_file_path')
        # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_�p�X
        backup_file_path = inifile.get('PATH', 'backup_file_path')
        backup_file_path = backup_root_path + '\\' + backup_file_path
        # ���W�}�[�N�ԋ���csv���ړ�����t�H���_�p�X
        csv_file_path = inifile.get('PATH', 'csv_file_path')

        # �B���摜�̍���(mm)
        actual_image_height = float(common_inifile.get('IMAGE_SIZE', 'actual_image_height'))
        # �B���摜�̃I�[�o�[���b�v(mm)
        actual_image_overlap = float(common_inifile.get('IMAGE_SIZE', 'actual_image_overlap'))
        # �B���摜(���摜)�̍���(pix)
        image_height = float(common_inifile.get('IMAGE_SIZE', 'image_height'))
        # �B���摜(���摜)�̕�(pix)
        image_width = float(common_inifile.get('IMAGE_SIZE', 'image_width'))
        # �I�[�o�[���b�v���������B���摜1���̒���(mm)
        nonoverlap_image_length = actual_image_height - actual_image_overlap
        # ���摜1pix������̒���(mm)
        one_pix_length = actual_image_height / image_height

        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �p�[�~�b�V�����G���[���b�Z�[�W
        permission_error = error_inifile.get('ERROR_INFO', 'permission_error')
        # �t�@�C���ޔ��Ď��s�Ԋu
        file_bk_sleep_time = int(inifile.get('VALUE', 'file_backup_sleep_time'))
        # �t�@�C���ޔ��Ď��s��
        file_bk_retry_count = int(inifile.get('VALUE', 'file_backup_retry_count'))
        # �����Ώۃ��C���ԍ�
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        empty_file = False

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### ���W�}�[�N�ǎ挋�ʃt�H���_���Ď�����
        # �t�H���_���Ƀ��W�}�[�N�ǎ挋�ʃt�@�C�������݂��邩�m�F����
        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))
        file_pattern = file_name_pattern + "_" + product_name + "_" + fabric_name + "_" + \
                       str(inspection_num) + "_" + inspection_date + file_extension_pattern
        logger.info('%s', file_pattern)
        result, sorted_files = get_regimark_file(file_path, file_pattern)

        if result:
            pass
        else:
            logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        # UPD 20200626 KQRM ���g START
        # # �B�������ʒm�t�@�C�����Ȃ��ꍇ�͈�����sleep���čĎ擾
        # if len(sorted_files) != 4:
        #     logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�����������Ă��܂��B', app_id, app_name)
        #     sys.exit()

        # �e�t�@�C���̍s�����擾����B
        file_lines_list = [0] * 4
        for i in range(len(sorted_files)):
            file_lines_list[i] = sum([1 for _ in open(sorted_files[i])])

        # �t�@�C�����A�s�����`�F�b�N����
        if (len(sorted_files) != 4 or
            not (file_lines_list[0] ==
                 file_lines_list[1] ==
                 file_lines_list[2] ==
                 file_lines_list[3])):
            logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�����������Ă��܂��B', app_id, app_name)
        # UPD 20200626 KQRM ���g END
            # ADD 20200626 KQRM ���g END
            if len(sorted_files) != 0:
                # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_�̑��݂��m�F����
                logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                result = exists_dir(backup_file_path)

                if result:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                 app_id, app_name, output_file_path)
                    sys.exit()

                # �������Ă��郌�W�}�[�N�ǎ挋�ʃt�@�C����ޔ��t�H���_�Ɉړ�������B
                retry_count = 0
                file = sorted_files
                while len(file) > 0:
                    logger.debug('[%s:%s] �s���̂��郌�W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����J�n���܂��B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                 app_id, app_name, file)
                    result = move_file(file[0], backup_file_path)

                    if result == permission_error:
                        if retry_count == file_bk_retry_count:
                            logger.error('[%s:%s] �s���̂��郌�W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                         app_id, app_name, file[0])
                            sys.exit()
                        else:
                            logger.debug('[%s:%s] �s���̂��郌�W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����Ď��s���܂��B', app_id, app_name, app_name)
                            file.append(file[0])
                            del file[0]
                            time.sleep(file_bk_sleep_time)
                            retry_count += 1
                    elif result:
                        logger.debug('[%s:%s] �s���̂��郌�W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����I�����܂����B:�ޔ��t�H���_[%s], ���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                     app_id, app_name, output_file_path, file[0])
                        # sorted_files.remove(file[0])
                        file.remove(file[0])
                    else:
                        logger.error('[%s:%s] �s���̂��郌�W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                     app_id, app_name, file[0])
                        sys.exit()

            # ���W�}�[�N�ǎ挋�ʃt�@�C���̃_�~�[�t�@�C�����쐬����B
            logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���J�n���܂��B[�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                        % (app_id, app_name, product_name, fabric_name, inspection_num,
                           inspection_date))
            result = output_dummy_file(product_name, fabric_name, inspection_num,
                                           inspection_date)
            if result:
                logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���I�����܂����B[�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                            % (app_id, app_name, product_name, fabric_name, inspection_num,
                               inspection_date))

                logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̍Ċm�F���J�n���܂��B', app_id, app_name)
                result, sorted_files = get_regimark_file(file_path, file_pattern)

                if result:
                    pass
                else:
                    logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̍Ċm�F�Ɏ��s���܂����B', app_id, app_name)
                    sys.exit()
            else:
                logger.error('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j�Ɏ��s���܂����B' % (app_id, app_name))
                sys.exit()
            # ADD 20200626 KQRM ���g END

        # DEL 20200626 KQRM ���g START
        # else:
        # DEL 20200626 KQRM ���g END
        logger.info('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���𔭌����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                    app_id, app_name, sorted_files)

        # DB���ʏ������Ăяo���āADB�ڑ����s��
        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
        result, conn, cur = create_connection()

        if result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
            sys.exit()

        while len(sorted_files) > 0:
            file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(sorted_files[0])).strftime(
                "%Y-%m-%d %H:%M:%S")

            logger.info('[%s:%s] %s�������J�n���܂��B�@[����,�@�����ԍ�, �������t]=[%s, %s, %s]', app_id, app_name,
                        app_name, fabric_name, inspection_num, inspection_date)

            files = [x for x in sorted_files if
                     (fabric_name + "_" + inspection_num in x)]

            if len(files) == 0:
                break
            else:
                pass

            # �B�������ʒm�t�@�C����Ǎ���
            logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����J�n���܂��B', app_id, app_name)

            for file in sorted(files):
                regimark_list = []
                regimark_face = re.split('_', (file.split('\\'))[-1])[5]
                regimark_type = re.split('_', (file.split('\\'))[-1])[6]
                result, regimark_info = read_regimark_file(file)

                if result:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����I�����܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, file)
                    logger.debug('[%s:%s] ���W�}�[�N��� : %s', app_id, app_name, regimark_info)
                else:
                    logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                 app_id, app_name, file)
                    sys.exit()

                if len(regimark_info) == 0:
                    logger.info('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C������ł��B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]',
                                app_id, app_name, file)
                    message = '���W�}�[�N�ǎ挋�ʃt�@�C������ł��B:���W�}�[�N�ǎ挋�ʃt�@�C���t�@�C����[%s]' % file
                    empty_file = True
                    error_util.write_eventlog_warning(app_name, message)

                else:
                    regimark_list.append(regimark_info)
                    # �����J�n�s�����擾����B
                    logger.debug('[%s:%s] �������擾���J�n���܂��B', app_id, app_name)
                    result, inspection_info, conn, cur = select_inspection_info(conn, cur, product_name,
                                                                                fabric_name, inspection_num,
                                                                                imaging_starttime, unit_num)

                    if result:
                        logger.debug('[%s:%s] �������擾���I�����܂����B' % (app_id, app_name))
                        logger.debug('[%s:%s] [����, �����ԍ�]=[%s, %s] [�����J�n�s��, �ŏI�s��, ��������] =[%s, %s, %s]�B' % (
                            app_id, app_name, fabric_name, inspection_num, inspection_info[0], inspection_info[1],
                            inspection_info[2]))
                        pass
                    else:
                        logger.error('[%s:%s] �������擾�Ɏ��s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()

                    # ���W�}�[�N���m�摜�̏d���m�F���s���B
                    logger.debug('[%s:%s] ���W�}�[�N���m�摜�̏d���m�F���J�n���܂��B', app_id, app_name)
                    result, confirmed_regimark = confirm_duplicate_registremark(regimark_list, fabric_name,
                                                                                inspection_num, inspection_info,
                                                                                image_height, image_width)
                    if result:
                        logger.debug('[%s:%s] ���W�}�[�N���m�摜�̏d���m�F���I�����܂����B' % (app_id, app_name))
                        logger.debug('[%s:%s] ���W�}�[�N���X�g %s' % (app_id, app_name, confirmed_regimark))
                        pass
                    else:
                        logger.error('[%s:%s] ���W�}�[�N���m�摜�̏d���m�F�Ɏ��s���܂����B' % (app_id, app_name))
                        sys.exit()

                    inspection_direction = inspection_info[2]

                    regimark_between_length_list = None

                    if (inspection_direction == 'S' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'X' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'Y' and re.split('\.', regimark_type)[0] == '2') or \
                            (inspection_direction == 'R' and re.split('\.', regimark_type)[0] == '2'):

                        # �ݒ背�W�}�[�N�Ԓ����A�̈�L�ї�X/Y���擾����B
                        logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���J�n���܂��B', app_id, app_name)
                        result, regimark_master, conn, cur = select_master_data(conn, cur, product_name)

                        if result:
                            logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���I�����܂����B' % (app_id, app_name))
                            logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[��� %s' % (app_id, app_name, regimark_master))
                            pass
                        else:
                            logger.error('[%s:%s] ���W�}�[�N�}�X�^�[���擾�Ɏ��s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        # �����L�k���A���L�k�����Z�o����B
                        conf_regimark_between_length = int(regimark_master[0][0])

                        # AI���f���������t���O
                        ai_model_flag = regimark_master[0][3]

                        ### �������W�}�[�N�Ԓ����̎Z�o���s���B
                        logger.debug('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o���J�n���܂��B', app_id, app_name)
                        result, regimark_between_length_list = \
                            calc_actual_regimark_between_length(confirmed_regimark, nonoverlap_image_length,
                                                                actual_image_overlap, conf_regimark_between_length,
                                                                actual_image_height, one_pix_length)
                        if result:
                            logger.debug('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o���I�����܂����B' % (app_id, app_name))
                            logger.debug(
                                '[%s:%s] �������W�}�[�N�Ԓ������X�g %s' % (app_id, app_name, regimark_between_length_list))
                            pass
                        else:
                            logger.error('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o�Ɏ��s���܂����B' % (app_id, app_name))
                            sys.exit()

                        stretch_rate_x = float(regimark_master[0][1])
                        stretch_rate_y = float(regimark_master[0][2])

                        logger.debug('[%s:%s] �����L�k���A���L�k���̎Z�o���J�n���܂��B', app_id, app_name)
                        result, strech_ratio_list = calc_stretch_ratio(regimark_between_length_list,
                                                                       conf_regimark_between_length,
                                                                       stretch_rate_x, stretch_rate_y)
                        if result:
                            logger.debug('[%s:%s] �����L�k���A���L�k���̎Z�o���I�����܂����B' % (app_id, app_name))
                            logger.debug('[%s:%s] �����L�k���A���L�k�����X�g %s' % (app_id, app_name, strech_ratio_list))
                            pass
                        else:
                            logger.error('[%s:%s] �����L�k���A���L�k���̎Z�o�Ɏ��s���܂����B' % (app_id, app_name))
                            sys.exit()

                        if regimark_face == '2':
                            pass
                        else:
                            # ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_�̑��݂��m�F����
                            logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                            result = exists_dir(output_file_path)

                            if result:
                                logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                            else:
                                logger.error('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                             app_id, app_name, output_file_path)
                                sys.exit()

                            # ���W�}�[�N�ԋ���csv���o�͂���B
                            conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                            logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�̏o�͂��J�n���܂��B', app_id, app_name)

                            result, csv_file = output_regimark_between_length_file(confirmed_regimark,
                                                                                   regimark_between_length_list,
                                                                                   strech_ratio_list,
                                                                                   conf_regimark_len,
                                                                                   output_file_path, fabric_name,
                                                                                   inspection_num)

                            if result:
                                logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�̏o�͂��I�����܂����B' % (app_id, app_name))
                                pass
                            else:
                                logger.error('[%s:%s] ���W�}�[�N�ԋ���csv�̏o�͂Ɏ��s���܂����B' % (app_id, app_name))
                                sys.exit()

                            # ���W�}�[�N�ԋ���CSV���ړ�����
                            logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�̈ړ����J�n���܂��B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                         app_id, app_name, file)
                            result = move_file(csv_file, csv_file_path)

                            if result:
                                logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�̈ړ����I�����܂����B', app_id, app_name)
                            else:
                                logger.warning('[%s:%s] ���W�}�[�N�ԋ���CSV�̈ړ��Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                               app_id, app_name, csv_file_path)

                if empty_file:
                    pass
                else:
                    # ���W�}�[�N�������W�}�[�N���e�[�u���ɓo�^����
                    logger.debug('[%s:%s] ���W�}�[�N���̓o�^���J�n���܂��B', app_id, app_name)
                    result, conn, cur = insert_regimark_info(conn, cur, confirmed_regimark,
                                                             regimark_between_length_list,
                                                             strech_ratio_list, product_name, fabric_name,
                                                             inspection_num,
                                                             conf_regimark_len, image_height, image_width,
                                                             regimark_type, regimark_face, imaging_starttime,
                                                             unit_num)

                    if result:
                        pass
                    else:
                        logger.error('[%s:%s] ���W�}�[�N���̓o�^�Ɏ��s���܂����B:[����,�����ԍ�]=[%s, %s]',
                                     app_id, app_name, fabric_name, inspection_num)
                        sys.exit()

                    logger.info('[%s:%s] ���W�}�[�N���̓o�^���I�����܂����B [����,�@�����ԍ�, �������t]=[%s, %s, %s]',
                                 app_id, app_name, fabric_name, inspection_num, inspection_date)
                    # �R�~�b�g����
                    conn.commit()

                # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_�̑��݂��m�F����
                logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                result = exists_dir(backup_file_path)

                if result:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                 app_id, app_name, output_file_path)
                    sys.exit()

                # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ��t�H���_�Ɉړ�������B

                retry_count = 0
                file = [file]
                while len(file) > 0:
                    logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����J�n���܂��B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                 app_id, app_name, file)
                    result = move_file(file[0], backup_file_path)

                    if result == permission_error:
                        if retry_count == file_bk_retry_count:
                            logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                         app_id, app_name, file[0])
                            sys.exit()
                        else:
                            logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����Ď��s���܂��B', app_id, app_name, app_name)
                            file.append(file[0])
                            del file[0]
                            time.sleep(file_bk_sleep_time)
                            retry_count += 1
                    elif result:
                        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ����I�����܂����B:�ޔ��t�H���_[%s], ���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                     app_id, app_name, output_file_path, file[0])
                        sorted_files.remove(file[0])
                        file.remove(file[0])
                    else:
                        logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̑ޔ��Ɏ��s���܂����B:���W�}�[�N�ǎ挋�ʃt�@�C����[%s]',
                                     app_id, app_name, file[0])
                        sys.exit()

            logger.info("[%s:%s] %s�����͐���ɏI�����܂����B", app_id, app_name, app_name)

        # DB�ڑ���ؒf
        logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
        close_connection(conn, cur)
        logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)

        # AI���f���������t���O���m�F
        # 0�̏ꍇ��NG�s�E�񔻒�o�^�@�\���Ăяo��
        if ai_model_flag == 0 or ai_model_flag == None:

            logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\�ďo���J�n���܂��B', app_id, app_name)
            result = register_ng_info.main(product_name, fabric_name, inspection_num, imaging_starttime)
            if result:
                logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\���I�����܂����B', app_id, app_name)
                result = True
                return result
            else:
                logger.debug('[%s:%s] NG�s�E�񔻒�o�^�@�\�����s���܂����B', app_id, app_name)
                sys.exit()

        # 1�̏ꍇ�͏������I�����āA�B�������o�^�@�\�ɖ߂�l��Ԃ�
        else:

            result = True
            return result

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
        result = False

    except:
        logger.error('[%s:%s] %s�@�\�ŗ\�����Ȃ��G���[���������܂����B[%s]', app_id, app_name, app_name, traceback.format_exc())

        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))
        result = False

    finally:
        if conn is not None:
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass

    return result


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()
