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
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


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
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'select regimark_between_length, stretch_rate_x, stretch_rate_y, ai_model_non_inspection_flg ' \
          'from mst_product_info where product_name = \'%s\'' % product_name

    logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾SQL %s' % (app_id, app_name, sql))
    ### �i��o�^���e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


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
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    ## UPD 20200716 NES ���� START
    # sql = 'select inspection_start_line, inspection_end_line, inspection_direction ' \
    #       'from inspection_info_header where product_name = \'%s\' and fabric_name = \'%s\' ' \
    #       'and inspection_num = \'%s\' and start_datetime = \'%s\' and unit_num = \'%s\'' \
    #       % (product_name, fabric_name, inspection_num, start_datetime, unit_num)
    sql = 'select inspection_start_line, inspection_end_line, inspection_direction, ai_model_non_inspection_flg ' \
          'from inspection_info_header where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, start_datetime, unit_num)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    ### �������w�b�_�[�e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


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
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'select * from regimark_info where product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and inspection_num = \'%s\' and line_num = %s and face = \'%s\' and imaging_starttime = \'%s\' ' \
          'and unit_num = \'%s\'' \
          % (product_name, fabric_name, inspection_num, line_num, regimark_face, imaging_starttime, unit_num)

    logger.debug('[%s:%s] ���W�}�[�N���o�^�ς݊m�FSQL %s' % (app_id, app_name, sql))
    ### �������w�b�_�[�e�[�u������f�[�^�擾
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


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
    func_name = sys._getframe().f_code.co_name
    error = None

    try:
        # �B���摜(���T�C�Y�摜)�̕�(pix)
        resize_image_width = float(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        # �B���摜(���T�C�Y�摜)�̍���(pix)
        resize_image_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))

        for i in range(len(regimark)):

            line_num = regimark[i][0]
            tmp_result, registed_info, error, conn, cur, func_name = \
                select_regimark_info(conn, cur, product_name, fabric_name, inspection_num, line_num, regimark_face,
                                     imaging_starttime, unit_num)

            if tmp_result:
                pass
            else:
                # ���s��
                logger.error("[%s:%s] �o�^�ς݃��W�}�[�N���m�F�����s���܂����B", app_id, app_name)
                return result, error, conn, cur, func_name

            # ���ꔽ�ԁA�����ԍ��A�s�ԍ������o�^�̏ꍇ
            if len(registed_info) == 0:
                # �o�^�Ώۂ��J�n���W�}�[�N�̏ꍇ
                if re.split('\.', regimark_type)[0] == '1':
                    # �o�^�ΏۂɎ������W�}�[�N�A����/���L�k�����܂܂��ꍇ
                    # �J�n���W�}�[�N�̏ꍇ�A����������S�AX�̍ۂɎ������W�}�[�N���Z�o���Ă���
                    if regimark_between_length_list != None:
                        image_name = regimark[i][1]
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                        width_stretchrate = '{:.6f}'.format(float(strech_ratio_list[i][1]))
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        actual_regimark_length = '{:.6f}'.format(float(regimark_between_length_list[i]))
                        len_stretchrate = strech_ratio_list[i][0]
                        width_stretchrate = strech_ratio_list[i][1]
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
                        image_name = regimark[i][1]
                        regimark_point_org = "(" + regimark[i][3] + "," + regimark[i][4] + ")"
                        regimark_point_resize = "(" + str(
                            round(resize_image_width * int(regimark[i][3]) / image_width)) + "," \
                                                + str(
                            round(resize_image_height * int(regimark[i][4]) / image_height)) + ")"

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
            tmp_result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, error, conn, cur, func_name


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
    error = None
    func_name = sys._getframe().f_code.co_name
    try:

        # ���ʊ֐��Ń��W�}�[�N�ǎ挋�ʊi�[�t�H���_�����擾����
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name_pattern,
                                                                              logger, app_id, app_name)

        if tmp_result:
            # ������
            pass
        else:
            # ���s��
            logger.error("[%s:%s] ���W�}�[�N�ǎ挋�ʊi�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
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
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C��=%s', app_id, app_name, file)
        # ���W�}�[�N�ǎ挋�ʃt�@�C���p�X����t�@�C�������擾���A���ԁA�����ԍ����擾����
        # �Ȃ��A�t�@�C�����́uRM_�i��_����_�����ԍ�_���t_������_���W�}�[�N���.CSV�v��z�肵�Ă���

        # ���W�}�[�N�ǎ挋�ʃt�@�C������A���ڂ��擾����
        with codecs.open(file, "r", "SJIS") as f:
            notification = [re.sub('\r', '', s[:-1]).split(',') for s in f.readlines()][1:]

        regimark_info = notification
        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, regimark_info, error, func_name

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
def calc_actual_regimark_between_length(regimark_list, actual_image_height, image_height, conf_regimark_between_length, stretch_rate_x):
    result = False
    actual_regimark_between_length_list = []
    error = None
    func_name = sys._getframe().f_code.co_name

    # �p���X�l����mm�ւ̕ϊ��ݒ�l���擾
    pulse_to_mm = inifile.get('VALUE', 'pulse_to_mm')

    try:
        # ���W�}�[�N���X�g��1���̏ꍇ�A���s�J�n�Ƃ̊Ԃ̒������m�F�ł��Ȃ����߁A�󕶎���}������B
        if len(regimark_list[0]) == 1:
            actual_regimark_between_length_list.append(conf_regimark_between_length * (stretch_rate_x / 100))
        else:
            # �J�n���W�}�[�N���Ǝ��s�J�n���W�}�[�N���𗘗p���āA�������W�}�[�N�Ԓ������Z�o����B
            for i in range(len(regimark_list[0])):
                if i != (len(regimark_list[0]) - 1):
                    start_regmark_y = int(regimark_list[0][i][3])
                    start_regmark_pulse = int(regimark_list[0][i][4])

                    next_start_regmark_y = int(regimark_list[0][i + 1][3])
                    next_start_regmark_pulse = int(regimark_list[0][i + 1][4])

                    # �p���X�l�̍������Z�o
                    between_image_pulse = next_start_regmark_pulse - start_regmark_pulse
                    # �p���X�l����mm�ւ̕ϊ�
                    regimark_length_mm = between_image_pulse * float(re.split(',', pulse_to_mm)[1]) / float(re.split(',', pulse_to_mm)[0])

                    if start_regmark_y == image_height / 2:
                        pass
                    else:
                        start_fraction = (image_height / 2) - start_regmark_y
                        start_fraction_mm = actual_image_height * start_fraction / image_height
                        regimark_length_mm += start_fraction_mm

                    if next_start_regmark_y == image_height / 2:
                        pass
                    else:
                        next_start_fraction = next_start_regmark_y - (image_height / 2)
                        next_start_fraction_mm = actual_image_height * next_start_fraction / image_height
                        regimark_length_mm += next_start_fraction_mm

                    actual_regimark_between_length_list.append(regimark_length_mm)

                # ���W�}�[�N���X�g���Ō��1���̏ꍇ�A���s�J�n�Ƃ̊Ԃ̒������m�F�ł��Ȃ����߁A�O�s�̃��W�}�[�N�Ԓ����𗘗p����B
                else:
                    actual_regimark_between_length_list.append(actual_regimark_between_length_list[-1])

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, actual_regimark_between_length_list, error, func_name


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
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        for i in range(len(actual_regimark_between_length_list)):
            length_strech_ratio = (actual_regimark_between_length_list[i] / conf_regimark_between_length) * 100
            width_strech_ratio = (stretch_rate_y / 100 * (conf_regimark_between_length * stretch_rate_x / 100) /
                                  actual_regimark_between_length_list[i]) * 100
            ratio = [length_strech_ratio, width_strech_ratio]
            strech_ratio_list.append(ratio)
        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, strech_ratio_list, error, func_name

# ------------------------------------------------------------------------------------
# �֐���             �F�s�ԍ��̔�
#
# �����T�v           �F1.�s�ԍ����̔Ԃ���
#
# ����               �F���W�}�[�N���X�g
#                      �����J�n�s
#                      ��������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �s�ԍ��̔ԍσ��W�}�[�N���X�g
#
# ------------------------------------------------------------------------------------
def numbering_regimark(regimark_list, inspection_direction, inspection_startline):
    result = False
    error = None
    func_name = sys._getframe().f_code.co_name
    numbering_regimark_list = []
    try:
        for i in range(len(regimark_list[0])):
            numbering_regimark = [inspection_startline] + regimark_list[0][i]
            if inspection_direction == 'S' or inspection_direction == 'X':
                inspection_startline += 1
            else:
                inspection_startline -= 1
            numbering_regimark_list.append(numbering_regimark)
        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, numbering_regimark_list, error, func_name

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
    func_name = sys._getframe().f_code.co_name
    # �t�@�C���ړ�
    result, error = file_util.move_file(target_file, move_dir, logger, app_id, app_name)

    return result, error, func_name


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
def output_regimark_between_length_file(regimark_list, regimark_between_length_list, strech_ratio_list,
                                        conf_regimark_between_length, output_file_path, fabric_name, inspection_num):
    result = False
    csv_file = None
    date = datetime.datetime.now().strftime("%Y%m%d_%H%M%S")
    file_name = str(date) + "_" + fabric_name + "_" + str(inspection_num).zfill(3) + "_N.csv"
    error = None
    func_name = sys._getframe().f_code.co_name

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

            for i in range(len(regimark_list)):
                line_num = regimark_list[i][0]
                conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                actual_regimark_len = '{:.6f}'.format(float(regimark_between_length_list[i]))
                length_ratio = '{:.6f}'.format(float(strech_ratio_list[i][0]))
                width_ratio = '{:.6f}'.format(float(strech_ratio_list[i][1]))

                string = str(line_num) + "," + str(conf_regimark_len) + "," + str(actual_regimark_len) + "," + str(
                    length_ratio) + "," + str(width_ratio)

                f.write(string)
                f.write("\r\n")

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, csv_file, error, func_name

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
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(
                    i + 1) + "_" + str(
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
    regimark_between_length_list = None
    numbering_regimark_list = None
    strech_ratio_list = None
    ai_model_flag = None
    error = None
    func_name = None
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
        result, sorted_files, error, func_name = get_regimark_file(file_path, file_pattern)

        if result:
            pass
        else:
            logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        # DB���ʏ������Ăяo���āADB�ڑ����s��
        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
        result, error, conn, cur, func_name = create_connection()

        if result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
            sys.exit()

        logger.debug('[%s:%s] �������擾���J�n���܂��B', app_id, app_name)
        result, inspection_info, error, conn, cur, func_name = \
            select_inspection_info(conn, cur, product_name, fabric_name, inspection_num,
                                   imaging_starttime, unit_num)

        if result:
            logger.debug('[%s:%s] �������擾���I�����܂����B �������=[%s]' % (app_id, app_name, inspection_info))
            logger.debug('[%s:%s] [����, �����ԍ�]=[%s, %s] [�����J�n�s��, �ŏI�s��, ��������] =[%s, %s, %s]�B' % (
                app_id, app_name, fabric_name, inspection_num, inspection_info[0], inspection_info[1],
                inspection_info[2]))
            pass
        else:
            logger.error('[%s:%s] �������擾�Ɏ��s���܂����B' % (app_id, app_name))
            conn.rollback()
            sys.exit()

        # �B�������ʒm�t�@�C�����Ȃ��ꍇ�͈�����sleep���čĎ擾
        # UPD 20200626 KQRM ���g START

        # if len(sorted_files) != 4:
        #     logger.error('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C�����������Ă��܂��B', app_id, app_name)
        #     sys.exit()
        if inspection_info[2] == 'S' or inspection_info[2] == 'X':
            sorted_files_list = [x for x in sorted_files if '1_1.CSV' in x.split('\\')[-1] or '2_1.CSV' in x.split('\\')[-1]]
        else:
            sorted_files_list = [x for x in sorted_files if
                                 '1_2.CSV' in x.split('\\')[-1] or '2_2.CSV' in x.split('\\')[-1]]

        # �e�t�@�C���̍s�����擾����B
        file_lines_list = [0] * 2
        for i in range(len(sorted_files_list)):
            file_lines_list[i] = sum([1 for _ in open(sorted_files_list[i])])

        # �t�@�C�����A�s�����`�F�b�N����
        if (len(sorted_files_list) != 2 or not (file_lines_list[0] == file_lines_list[1])):
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
                    result, sorted_files, error, func_name = get_regimark_file(file_path, file_pattern)

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



        while len(sorted_files) > 0:
            file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(sorted_files[0])).strftime(
                "%Y-%m-%d %H:%M:%S")

            logger.info('[%s:%s] %s�������J�n���܂��B�@[����,�@�����ԍ�, �������t]=[%s, %s, %s]', app_id, app_name,
                        app_name, fabric_name, inspection_num, inspection_date)

            files = [x for x in sorted_files if (fabric_name + "_" + str(inspection_num) in x)]

            if len(files) == 0:
                break
            else:
                pass

            # �B�������ʒm�t�@�C����Ǎ���
            logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C���̓Ǎ����J�n���܂��B', app_id, app_name)

            for file in sorted(files):
                base_file_name = os.path.basename(file)
                if inspection_info[2] == 'S' or inspection_info[2] == 'X':
                    if '_1_1.CSV' in base_file_name or '_2_1.CSV' in base_file_name:
                        pass
                    else:
                        # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_�̑��݂��m�F����
                        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                        result, error, func_name = exists_dir(backup_file_path)

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
                            result, error, func_name = move_file(file[0], backup_file_path)

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
                        continue
                else:
                    if '_1_2.CSV' in base_file_name or '_2_2.CSV' in base_file_name:
                        pass
                    else:
                        # ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_�̑��݂��m�F����
                        logger.debug('[%s:%s] ���W�}�[�N�ǎ挋�ʃt�@�C����ޔ�����t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                        result, error, func_name = exists_dir(backup_file_path)

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
                            result, error, func_name = move_file(file[0], backup_file_path)

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
                        continue

                empty_file = False
                regimark_list = []
                regimark_face = re.split('_', (file.split('\\'))[-1])[5]
                regimark_type = re.split('_', (file.split('\\'))[-1])[6]
                result, regimark_info, error, func_name = read_regimark_file(file)

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

                    ## DEL 20200716 NES ���� START
                    # # �ݒ背�W�}�[�N�Ԓ����A�̈�L�ї�X/Y���擾����B
                    # logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���J�n���܂��B', app_id, app_name)
                    # result, regimark_master, error, conn, cur, func_name = \
                    #     select_master_data(conn, cur, product_name)
                    #
                    # if result:
                    #     logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���I�����܂����B' % (app_id, app_name))
                    #     logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[��� %s' % (app_id, app_name, regimark_master))
                    #     pass
                    # else:
                    #     logger.error('[%s:%s] ���W�}�[�N�}�X�^�[���擾�Ɏ��s���܂����B' % (app_id, app_name))
                    #     conn.rollback()
                    #     sys.exit()
                    #
                    # # �ݒ背�W�}�[�N�Ԓ������擾����B
                    # conf_regimark_between_length = int(regimark_master[0][0])
                    # # ����/���L�k��(�ʎY�l)���擾����B
                    # stretch_rate_x = float(regimark_master[0][1])
                    # stretch_rate_y = float(regimark_master[0][2])
                    ## DEL 20200716 NES ���� END

                    ## UPD 20200716 NES ���� START
                    # AI���f���������t���O
                    #ai_model_flag = regimark_master[0][3]
                    ai_model_flag = inspection_info[3]
                    ## UPD 20200716 NES ���� END

                else:
                    regimark_list.append(regimark_info)
                    # �����J�n�s�����擾����B
                    logger.debug('[%s:%s] �������擾���J�n���܂��B', app_id, app_name)
                    result, inspection_info, error, conn, cur, func_name = \
                        select_inspection_info(conn, cur, product_name, fabric_name, inspection_num,
                                               imaging_starttime, unit_num)

                    if result:
                        logger.debug('[%s:%s] �������擾���I�����܂����B �������=[%s]' % (app_id, app_name, inspection_info))
                        logger.debug('[%s:%s] [����, �����ԍ�]=[%s, %s] [�����J�n�s��, �ŏI�s��, ��������] =[%s, %s, %s]�B' % (
                            app_id, app_name, fabric_name, inspection_num, inspection_info[0], inspection_info[1],
                            inspection_info[2]))
                        pass
                    else:
                        logger.error('[%s:%s] �������擾�Ɏ��s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()

                    inspection_startline = inspection_info[0]
                    inspection_direction = inspection_info[2]



                    logger.debug('[%s:%s] �s�ԍ��̔Ԃ��J�n���܂��B', app_id, app_name)
                    result, numbering_regimark_list, error,func_name = \
                        numbering_regimark(regimark_list, inspection_direction, inspection_startline)

                    if result:
                        logger.debug('[%s:%s] �s�ԍ��̔Ԃ��I�����܂����B' % (app_id, app_name))
                        logger.debug('[%s:%s] [����, �����ԍ�]=[%s, %s] �s�̔ԍς݃��W�}�[�N =[%s]' % (
                            app_id, app_name, fabric_name, inspection_num, numbering_regimark_list))
                    else:
                        logger.error('[%s:%s] �s�ԍ��̔ԂɎ��s���܂����B' % (app_id, app_name))
                        sys.exit()

                    # �ݒ背�W�}�[�N�Ԓ����A�̈�L�ї�X/Y���擾����B
                    logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���J�n���܂��B', app_id, app_name)
                    result, regimark_master, error, conn, cur, func_name = \
                        select_master_data(conn, cur, product_name)

                    if result:
                        logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[���擾���I�����܂����B' % (app_id, app_name))
                        logger.debug('[%s:%s] ���W�}�[�N�}�X�^�[��� %s' % (app_id, app_name, regimark_master))
                        pass
                    else:
                        logger.error('[%s:%s] ���W�}�[�N�}�X�^�[���擾�Ɏ��s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()

                    # �ݒ背�W�}�[�N�Ԓ������擾����B
                    conf_regimark_between_length = int(regimark_master[0][0])
                    # ����/���L�k��(�ʎY�l)���擾����B
                    stretch_rate_x = float(regimark_master[0][1])
                    stretch_rate_y = float(regimark_master[0][2])
                    # AI���f���������t���O
                    ## UPD 20200716 NES ���� START
                    #ai_model_flag = regimark_master[0][3]
                    ai_model_flag = inspection_info[3]
                    ## UPD 20200716 NES ���� END

                    ### �������W�}�[�N�Ԓ����̎Z�o���s���B
                    logger.debug('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o���J�n���܂��B', app_id, app_name)
                    result, regimark_between_length_list, error, func_name = \
                        calc_actual_regimark_between_length(regimark_list, actual_image_height, image_height, conf_regimark_between_length, stretch_rate_x)
                    if result:
                        logger.debug('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o���I�����܂����B' % (app_id, app_name))
                        logger.debug(
                            '[%s:%s] �������W�}�[�N�Ԓ������X�g %s' % (app_id, app_name, regimark_between_length_list))
                        pass
                    else:
                        logger.error('[%s:%s] �������W�}�[�N�Ԓ����̎Z�o�Ɏ��s���܂����B' % (app_id, app_name))
                        sys.exit()


                    logger.debug('[%s:%s] �����L�k���A���L�k���̎Z�o���J�n���܂��B', app_id, app_name)
                    result, strech_ratio_list, error, func_name = \
                        calc_stretch_ratio(regimark_between_length_list, conf_regimark_between_length,
                                           stretch_rate_x, stretch_rate_y)
                    if result:
                        logger.debug('[%s:%s] �����L�k���A���L�k���̎Z�o���I�����܂����B' % (app_id, app_name))
                        logger.debug('[%s:%s] �����L�k���A���L�k�����X�g %s' % (app_id, app_name, strech_ratio_list))
                        pass
                    else:
                        logger.error('[%s:%s] �����L�k���A���L�k���̎Z�o�Ɏ��s���܂����B' % (app_id, app_name))
                        sys.exit()

                    if (inspection_direction == 'S' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'X' and re.split('\.', regimark_type)[0] == '1') or \
                            (inspection_direction == 'Y' and re.split('\.', regimark_type)[0] == '2') or \
                            (inspection_direction == 'R' and re.split('\.', regimark_type)[0] == '2'):

                        if regimark_face == '2':
                            pass
                        else:
                            # ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_�̑��݂��m�F����
                            logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N���J�n���܂��B', app_id, app_name)
                            result, error, func_name = exists_dir(output_file_path)

                            if result:
                                logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
                                pass
                            else:
                                logger.error('[%s:%s] ���W�}�[�N�ԋ���CSV�t�@�C�����o�͂���t�H���_���݃`�F�b�N�Ɏ��s���܂����B:�ޔ��t�H���_��[%s]',
                                             app_id, app_name, output_file_path)
                                sys.exit()

                            # ���W�}�[�N�ԋ���csv���o�͂���B
                            conf_regimark_len = '{:.6f}'.format(float(conf_regimark_between_length))
                            logger.debug('[%s:%s] ���W�}�[�N�ԋ���CSV�̏o�͂��J�n���܂��B', app_id, app_name)

                            result, csv_file, error, func_name = \
                                output_regimark_between_length_file(numbering_regimark_list,
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
                            result, error, func_name = move_file(csv_file, csv_file_path)

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
                    result, error, conn, cur, func_name = insert_regimark_info(conn, cur, numbering_regimark_list,
                                                                               regimark_between_length_list,
                                                                               strech_ratio_list, product_name,
                                                                               fabric_name,
                                                                               inspection_num,
                                                                               conf_regimark_len, image_height,
                                                                               image_width,
                                                                               regimark_type, regimark_face,
                                                                               imaging_starttime,
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
                result, error, func_name = exists_dir(backup_file_path)

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
                    result, error, func_name = move_file(file[0], backup_file_path)

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


            result = True
            return result, ai_model_flag, error, func_name

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

        logger.debug('[%s:%s] �G���[�ڍׂ��擾���܂��B' % (app_id, app_name))
        error_message, error_id = error_detail.get_error_message(error, app_id, func_name)

        logger.error('[%s:%s] %s [�G���[�R�[�h:%s]' % (app_id, app_name, error_message, error_id))

        event_log_message = '[�@�\��, �G���[�R�[�h]=[%s, %s] %s' % (app_name, error_id, error_message)
        error_util.write_eventlog_error(app_name, event_log_message, logger, app_id, app_name)

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

    return result, ai_model_flag, error, func_name


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()
