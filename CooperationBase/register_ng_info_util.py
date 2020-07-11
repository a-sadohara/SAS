# -*- coding: SJIS -*-
# NG�s�񔻒�@�\
#
import configparser
import logging.config
import re
import sys

import cv2
import numpy as np

import db_util
import error_detail

# ���O�ݒ�擾
logging.config.fileConfig("D:/CI/programs/config/logging_register_ng_info.conf")
logger = logging.getLogger("register_ng_info")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_ng_info_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

#

master_data_dict = {'file_num': 0, 'register_flg': 1, 'selection_flg': 2, 'product_name': 3,
                    'inspection_param_num': 4, 'airbag_imagepath': 5, 'length': 6, 'width': 7,
                    'marker_color_flat': 8, 'marker_color_back': 9, 'auto_print': 10, 'auto_inspection_stop': 11,
                    'regimark_1_imagepath': 12, 'regimark_1_point_x': 13, 'regimark_1_point_y': 14,
                    'regimark_1_size_w': 15, 'regimark_1_size_h': 16, 'regimark_2_imagepath': 17,
                    'regimark_2_point_x': 18, 'regimark_2_point_y': 19, 'regimark_2_size_w': 20,
                    'regimark_2_size_h': 21, 'base_point_1_x': 22, 'base_point_1_y': 23, 'base_point_2_x': 24,
                    'base_point_2_y': 25, 'base_point_3_x': 26, 'base_point_3_y': 27, 'base_point_4_x': 28,
                    'base_point_4_y': 29, 'base_point_5_x': 30, 'base_point_5_y': 31,
                    'point_1_plus_direction_x': 32, 'point_1_plus_direction_y': 33, 'point_2_plus_direction_x': 34,
                    'point_2_plus_direction_y': 35, 'point_3_plus_direction_x': 36, 'point_3_plus_direction_y': 37,
                    'point_4_plus_direction_x': 38, 'point_4_plus_direction_y': 39, 'point_5_plus_direction_x': 40,
                    'point_5_plus_direction_y': 41, 'stretch_rate_x': 42, 'stretch_rate_y': 43,
                    'stretch_rate_x_upd': 44, 'stretch_rate_y_upd': 45, 'regimark_3_imagepath': 46,
                    'regimark_4_imagepath': 47, 'stretch_rate_auto_calc_flg': 48, 'width_coefficient': 49,
                    'correct_value': 50, 'black_thread_cnt_per_line': 51, 'measuring_black_thread_num': 52,
                    'camera_num': 53, 'column_cnt': 54, 'illumination_information': 55,
                    'start_regimark_camera_num': 56, 'end_regimark_camera_num': 57, 'line_length': 58,
                    'regimark_between_length': 59, 'taking_camera_cnt': 60, 'column_threshold_01': 61,
                    'column_threshold_02': 62, 'column_threshold_03': 63, 'column_threshold_04': 64,
                    'line_threshold_a1': 65, 'line_threshold_a2': 66, 'line_threshold_b1': 67,
                    'line_threshold_b2': 68, 'line_threshold_c1': 69, 'line_threshold_c2': 70,
                    'line_threshold_d1': 71, 'line_threshold_d2': 72, 'line_threshold_e1': 73,
                    'line_threshold_e2': 74, 'top_point_a': 75, 'top_point_b': 76, 'top_point_c': 77,
                    'top_point_d': 78, 'top_point_e': 79, 'ai_model_non_inspection_flg': 80, 'ai_model_name': 81}

line_name_dict = {1: 'A', 2: 'B', 3: 'C', 4: 'D', 5: 'E'}
line_num_dict = {'A': 1, 'B': 2, 'C': 3, 'D': 4, 'E': 5}


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
def create_connection(logger):
    func_name = sys._getframe().f_code.co_name
    result, error, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F���W�}�[�N���擾
#
# �����T�v           �F1.���W�}�[�N���e�[�u�����珈���Ώ۔��ԁA�����ԍ��̃��W�}�[�N�����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ���W�}�[�N���(�s�ԍ��A�J�n���W�}�[�N�摜���A�J�n���W�}�[�N���W�A�I�����W�}�[�N�摜���A�I�����W�}�[�N���W)
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_regimark_info(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num, logger):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'select line_num, start_regimark_file, start_regimark_point_resize, end_regimark_file, ' \
          'end_regimark_point_resize, len_stretchrate, width_stretchrate, face from regimark_info where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] ���W�}�[�N���擾SQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, select_result, error, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �F�����}�X�^���擾
#
# �����T�v           �F1.�i��o�^���e�[�u������}�X�^�����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �}�X�^���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_product_master_info(conn, cur, product_name, logger):
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    sql = 'select * from mst_product_info where product_name = \'%s\' ' % product_name

    logger.debug('[%s:%s] �����}�X�^���擾SQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�i��o�^���e�[�u������}�X�^�����擾����B
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, error, conn, cur, func_name


# ------------------------------------------------------------------------------------
# ������             �FNG���o�^
#
# �����T�v           �F1.�i��o�^���e�[�u������}�X�^�����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      NG�s
#                      NG��
#                      �}�X�^���W
#                      ��_����̋���(X)
#                      ��_����̋���(Y)
#                      �A��
#                      NG�摜��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_ng_info(conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point, ng_distance_x,
                   ng_distance_y, num, ng_file, undetected_image_flag_is_undetected, inspection_date, unit_num, logger):
    inspection_num = str(int(inspection_num))
    func_name = sys._getframe().f_code.co_name
    ### �N�G�����쐬����
    if undetected_image_flag_is_undetected == 1:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s  where ng_image = \'%s\' and unit_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point,
                 ng_distance_x, ng_distance_y, ng_file, unit_num)

    elif undetected_image_flag_is_undetected == 2:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s where unti_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point,
                 ng_distance_x, ng_distance_y, unit_num)
    else:
        sql = 'update \"rapid_%s_%s_%s\" set ng_line = %s, columns = \'%s\', master_point = \'%s\', ' \
              'ng_distance_x = %s, ng_distance_y = %s  where num = %s and ng_image = \'%s\' and unit_num = \'%s\'' \
              % (fabric_name, inspection_num, inspection_date, ng_line, ng_colum, master_point, ng_distance_x,
                 ng_distance_y, num, ng_file, unit_num)

    logger.debug('[%s:%s] NG���o�^SQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�i��o�^���e�[�u������}�X�^�����擾����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
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
def close_connection(conn, cur, logger):
    func_name = sys._getframe().f_code.co_name
    result, error = db_util.close_connection(conn, cur, logger, app_id, app_name)
    return result, error, func_name


# ------------------------------------------------------------------------------------
# ������             �FNG�摜�Y���s����
#
# �����T�v           �F1.�J�n���W�}�[�N�摜����NG�摜���̎B���ԍ����r���A�Y���s�ԍ�����肷��B
#                      2.�Y���s�ԍ��̃��W�}�[�N���ƊY���s��+1�̍s�ԍ��̃��W�}�[�N�𒊏o����B
#
# ����               �F���W�}�[�N���
#                      NG�摜��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�s���W�}�[�N���
# ------------------------------------------------------------------------------------
def specific_line_num(regimark_info, ng_file, inspection_direction, logger):
    result = False
    line_info = None
    last_flag = 0
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        # �摜���̎B���ԍ�����Y���s�ԍ�����肷�邽�߁A�摜����[._]�ŕ�������B
        sp_ng_file = re.split('[_.]', ng_file[1])
        sp_ng_file.append(ng_file[2])
        ng_face = int(sp_ng_file[4])

        logger.debug('[%s:%s] ���W�}�[�N��� %s' % (app_id, app_name, regimark_info))
        logger.debug('[%s:%s] NG�摜��� %s, �������� %s' % (app_id, app_name, sp_ng_file, inspection_direction))

        if inspection_direction[0] == 'S' or inspection_direction[0] == 'X':
            sp_regimark_file = [[x[:][0]] + re.split('[._]', x[:][1]) for x in regimark_info if
                                (int(sp_ng_file[6]) >= int(re.split('[._]', x[:][1])[6])) and ng_face == int(x[:][7])]
            logger.debug('[%s:%s] S�EX���� �X�v���b�g���W�}�[�N��� %s' % (app_id, app_name, sp_regimark_file))

            if len(sp_regimark_file) == 0:
                result = 'error'
                return result, line_info, last_flag, error, func_name
            else:
                pass

            # NG�摜�̎B���ԍ��ȉ��̍ő�l���Y���s�̊J�n���W�}�[�N�摜�Ƃ��ē��肷��B
            line_num_index = max([int(y[7]) for y in sp_regimark_file])
            line_num = int([z[0] for z in sp_regimark_file if line_num_index == int(z[:][7])][0])
            line_info = sorted([i for i in regimark_info if
                                ((line_num == i[:][0]) or ((line_num + 1) == i[:][0])) and ng_face == int(i[:][7])])
            last_flag = 0

            if len(line_info) == 1:
                line_info = sorted([i for i in regimark_info if
                                    ((line_num == i[:][0]) or ((line_num -1) == i[:][0])) and ng_face == int(i[:][7])])
                last_flag = 1
            else:
                pass

        else:
            sp_regimark_file = [[x[:][0]] + re.split('[._]', x[:][3]) for x in regimark_info if
                                (int(sp_ng_file[6]) >= int(re.split('[._]', x[:][3])[6])) and ng_face == int(x[:][7])]
            logger.debug('[%s:%s] �X�v���b�g���W�}�[�N��� %s' % (app_id, app_name, sp_ng_file))

            if len(sp_regimark_file) == 0:
                result = 'error'
                return result, line_info, last_flag, error, func_name
            else:
                pass

            # NG�摜�̎B���ԍ��ȉ��̍ő�l���Y���s�̊J�n���W�}�[�N�摜�Ƃ��ē��肷��B
            line_num_index = max([int(y[7]) for y in sp_regimark_file])
            line_num = int([z[0] for z in sp_regimark_file if line_num_index == int(z[:][7])][0])
            line_info = sorted([i for i in regimark_info if
                                ((line_num == i[:][0]) or ((line_num - 1) == i[:][0])) and ng_face == int(i[:][7])],
                               reverse=True)
            last_flag = 0

            if len(line_info) == 1:
                line_info = sorted([i for i in regimark_info if
                                    ((line_num == i[:][0]) or ((line_num + 1) == i[:][0])) and ng_face == int(i[:][7])], reverse=True)
                last_flag = 1
            else:
                pass

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, line_info, last_flag, error, func_name


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�ƃ}�X�^�摜�̍s�E��̒����̔䗦�Z�o
#
# �����T�v           �F1.���W�}�[�N�Ԓ���/���W�}�[�N�ԕ��̎B���������Z�o����B
#                      2.�B���摜��̃��W�}�[�N�Ԓ���[pix]�E���W�}�[�N�ԕ�[pix]���Z�o����B
#                      3.�}�X�^�摜��̃��W�}�[�N�Ԓ���[pix]���Z�o����B
#                      4.�B���摜�ƃ}�X�^�摜�̃��W�}�[�N�Ԓ����E���W�}�[�N�ԕ��̔䗦���Z�o����B
#
# ����               �F���W�}�[�N���
#                      �I�[�o�[���b�v���O��1�B���摜�̕�(X��������)
#                      �I�[�o�[���b�v���O��1�B���摜�̍���(Y��������)
#                      �I�[�o�[���b�v���̕�(X��������)
#                      �I�[�o�[���b�v���̍���(Y��������)
#                      ���T�C�Y��摜�̕�(X��������)
#                      ���T�C�Y��摜�̍���(Y��������)
#                      �}�X�^���
#                      �}�X�^�摜�̕�(X��������)
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����䗦
#                      ���䗦
#                      �ݒ背�W�}�[�N�Ԓ���[pix]
#
# ------------------------------------------------------------------------------------
def calc_length_ratio(regimark_info, line_info, nonoverlap_image_height_pix,
                      overlap_height_pix, resize_image_height, mst_data, master_image_width,
                      actual_image_height, inspection_direction, logger):
    result = False
    regimark_length_ratio = None
    regimark_width_ratio = None
    conf_regimark_between_length_pix = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # �}�X�^��񂩂�K�v�ȏ����擾����B���J���������������߃}�X�^��񎫏��𗘗p���ď����擾����B
        conf_regimark_between_length = int(mst_data[master_data_dict['regimark_between_length']])
        length = int(mst_data[master_data_dict['length']])
        regimark_1_point_y = int(mst_data[master_data_dict['regimark_1_point_y']])
        regimark_2_point_y = int(mst_data[master_data_dict['regimark_2_point_y']])

        ng_face = int(line_info[0][5])
        # �}�X�^�摜��[pix]:�}�X�^�摜������[mm]=X:�ݒ背�W�}�[�N�Ԓ���[mm]
        conf_regimark_between_length_pix = master_image_width * conf_regimark_between_length / length

        # NG�摜���܂܂��s���͈ȉ��`���Ŏ擾
        # [(1, 'S354_380613-0AC_20191120_01_1_01_00001.jpg', '(619,646)',
        # 'S354_380613-0AC_20191120_01_1_01_00010.jpg', '(619,646)', '1'),
        # (2, 'S354_380613-0AC_20191120_01_1_01_00011.jpg', '(619,646)',
        # 'S354_380613-0AC_20191120_01_1_01_00020.jpg', '(619,646)', '1')]
        # ���������� S, X�̏ꍇ�A�J�n���W�}�[�N�A���s�J�n���W�}�[�N�̎B���ԍ��A���W�𒊏o����B
        if len(regimark_info) != 1 and len(line_info) == 2:
            if inspection_direction == 'S' or inspection_direction == 'X':
                # �s�ԍ��A�J�n���W�}�[�N�t�@�C�����A���W���K�v�B
                sp_regimark_list = [[x[0]] + re.split('[._]', x[:][1]) + [x[2]] + [x[3]] + [x[4]]
                                    for x in line_info]
                start_image_num = int(sp_regimark_list[0][7])
                start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[1])

                next_start_image_num = int(sp_regimark_list[1][7])
                next_start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[1][9])))[1])

            # ���������� Y, R�̏ꍇ�A�I�����W�}�[�N�A���s�I�����W�}�[�N�̎B���ԍ��A���W�𒊏o����B
            else:
                sp_regimark_list = [[x[0]] + [x[1]] + [x[2]] + re.split('[._]', x[:][3]) + [x[4]]
                                    for x in line_info]
                print(sp_regimark_list)

                start_image_num = int(sp_regimark_list[0][9])
                start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[1])

                next_start_image_num = int(sp_regimark_list[1][9])
                next_start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[1][11])))[1])

            # �J�n-���s�J�n�Ԓ����A�J�n-�I���ԕ��̎B���������Z�o����B
            if start_image_num != next_start_image_num:
                start_between_x_image_count = abs(next_start_image_num - start_image_num) - 1
            else:
                start_between_x_image_count = 0

            # �B�������~1�B���摜�̍���(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜����-[�J�n���W�}�[�Ny���W]-[�I�[�o�[���b�v������])�{[���s�J�n���W�}�[�Ny���W]
            start_regimark_x_pix = start_between_x_image_count * nonoverlap_image_height_pix + (
                    resize_image_height - start_regimark_y - overlap_height_pix) + next_start_regimark_y

             # �䗦�Z�o
            regimark_length_ratio = start_regimark_x_pix / conf_regimark_between_length_pix

        else:
            stretch_rate_x = float(mst_data[master_data_dict['stretch_rate_x']])
            regimark_x_pix = (conf_regimark_between_length * (
                        stretch_rate_x / 100)) * resize_image_height / actual_image_height
            regimark_length_ratio = regimark_x_pix / conf_regimark_between_length_pix

        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, regimark_length_ratio, conf_regimark_between_length_pix, error, func_name


# ------------------------------------------------------------------------------------
# ������             �FNG�ʒu����
#
# �����T�v           �F1.�J�n���W�}�[�N�摜����NG�摜���̎B���ԍ����r���A�Y���s�ԍ�����肷��B
#
# ����               �F���W�}�[�N���
#                      NG�摜���
#                      �I�[�o�[���b�v���O��1�B���摜�̕�(X��������)
#                      �I�[�o�[���b�v���O��1�B���摜�̍���(Y��������)
#                      �I�[�o�[���b�v���̕�(X��������)
#                      �I�[�o�[���b�v���̍���(Y��������)
#                      ���T�C�Y��摜�̕�(X��������)
#                      ���T�C�Y��摜�̍���(Y��������)
#                      ���W�}�[�N�Ԓ����䗦
#                      ���W�}�[�N�ԕ��䗦
#                      �}�X�^���
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �}�X�^�摜���NG���W(X���W)
#                      �}�X�^�摜���NG���W(Y���W)
#
# ------------------------------------------------------------------------------------
def specific_ng_point(line_info, ng_image_info, nonoverlap_image_width_pix, nonoverlap_image_height_pix,
                      overlap_width_pix, overlap_height_pix, resize_image_height, resize_image_width,
                      regimark_length_ratio, mst_data, inspection_direction, master_image_width,
                      master_image_height, actual_image_width, actual_image_overlap, logger):
    result = False
    length_on_master = None
    width_on_master = None
    plus_direction = None
    ng_face = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        # �}�X�^��񂩂�K�v�ȏ����擾����B���J���������������߃}�X�^��񎫏��𗘗p���ď����擾����B
        regimark_1_point_x = int(mst_data[master_data_dict['regimark_1_point_x']])
        regimark_2_point_x = int(mst_data[master_data_dict['regimark_2_point_x']])
        regimark_1_point_y = int(mst_data[master_data_dict['regimark_1_point_y']])
        regimark_2_point_y = int(mst_data[master_data_dict['regimark_2_point_y']])
        master_width = int(mst_data[master_data_dict['width']])

        stretch_rate_x = float(line_info[0][5])
        stretch_rate_y = float(line_info[0][6])
         # �J�n���W�}�[�N�̎B���ԍ��A���W�𒊏o����B
        sp_ng_file = re.split('[_.]', ng_image_info[1]) + [ng_image_info[2]]
        ng_face = int(sp_ng_file[4])

        if inspection_direction == 'S' or inspection_direction == 'X':
            logger.debug('[%s:%s] ��������S or X' % (app_id, app_name))
            sp_regimark_list = [[x[0]] + re.split('[._]', x[:][1]) + [x[2]] + [x[3]] + [x[4]]
                                for x in line_info]

            start_image_num = int(sp_regimark_list[0][7])
            start_regimark_x = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[0])
            start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][9])))[1])
            start_camera_num = int(sp_regimark_list[0][6])

            regimark_x = regimark_1_point_x

            if inspection_direction == 'S' and ng_face == 2:
                logger.debug('[%s:%s] ��������S ������2' % (app_id, app_name))
                regimark_y = master_image_height - regimark_1_point_y
            elif inspection_direction == 'X' and ng_face == 1:
                logger.debug('[%s:%s] ��������X ������1' % (app_id, app_name))
                regimark_y = master_image_height - regimark_1_point_y
            else:
                regimark_y = regimark_1_point_y

        else:
            logger.debug('[%s:%s] ��������Y or R' % (app_id, app_name))
            sp_regimark_list = [[x[0]] + [x[1]] + [x[2]] + re.split('[._]', x[:][3]) + [x[4]]
                                for x in line_info]

            start_image_num = int(sp_regimark_list[0][9])
            start_regimark_x = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[0])
            start_regimark_y = int(re.split(',', (re.sub('[()]', '', sp_regimark_list[0][11])))[1])
            start_camera_num = int(sp_regimark_list[0][8])

            regimark_x = master_image_width - regimark_2_point_x

            if inspection_direction == 'Y' and ng_face == 2:
                logger.debug('[%s:%s] ��������Y ������2' % (app_id, app_name))
                regimark_y = master_image_height - regimark_2_point_y
            elif inspection_direction == 'R' and ng_face == 1:
                logger.debug('[%s:%s] ��������R ������1' % (app_id, app_name))
                regimark_y = master_image_height - regimark_2_point_y
            else:
                regimark_y = regimark_2_point_y

        ng_image_num = int(sp_ng_file[6])
        ng_x = int(re.split(',', (re.sub('[()]', '', sp_ng_file[8])))[0])
        ng_y = int(re.split(',', (re.sub('[()]', '', sp_ng_file[8])))[1])

        ng_camera_num = int(sp_ng_file[5])

        if ng_image_num == start_image_num:
            logger.debug('[%s:%s] �B���ԍ�������' % (app_id, app_name))
            ng_x_pix = abs(start_regimark_y - ng_y)
            if start_regimark_y < ng_y:
                x_plus_direction = 1
            else:
                x_plus_direction = -1
        else:
            between_x_image_count = ng_image_num - start_image_num - 1
            # �B�������~1�B���摜�̍���(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜����-[�J�n���W�}�[�Ny���W]-[�I�[�o�[���b�v������])�{[NG�摜y���W]
            ng_x_pix = between_x_image_count * nonoverlap_image_height_pix + (
                    resize_image_height - start_regimark_y - overlap_height_pix) + ng_y
            x_plus_direction = 1

        if start_camera_num == ng_camera_num:
            logger.debug('[%s:%s] �J�����ԍ�������' % (app_id, app_name))
            between_y_image_count = 0
        else:
            between_y_image_count = abs(ng_camera_num - start_camera_num) - 1

        if ng_face == 1:
            if start_camera_num == ng_camera_num:
                logger.debug('[%s:%s] ������1 �J�����ԍ�������' % (app_id, app_name))
                ng_y_pix = abs(start_regimark_x - ng_x)
                ng_y_mm = ng_y_pix * actual_image_width / resize_image_width
                plus_direction = 1
            elif start_camera_num > ng_camera_num:
                logger.debug('[%s:%s] ������1 ���W�}�[�N�J�����ԍ���NG�J�����ԍ�' % (app_id, app_name))
                # �B�������~1�B���摜�̕�(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜��-[�J�n���W�}�[�Nx���W]-[�I�[�o�[���b�v����])�{[NG�摜x���W]
                register_image_fraction = (resize_image_width - start_regimark_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (ng_x * actual_image_width / resize_image_width)
                plus_direction = -1
            else:
                logger.debug('[%s:%s] ������1 ���W�}�[�N�J�����ԍ���NG�J�����ԍ�' % (app_id, app_name))
                # �B�������~1�B���摜�̕�(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜��-[NG�摜x���W]-[�I�[�o�[���b�v����])�{[�J�n���W�}�[�N x���W]
                register_image_fraction = (resize_image_width - ng_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (start_regimark_x * actual_image_width / resize_image_width)
                plus_direction = 1

        else:
            if start_camera_num == ng_camera_num:
                logger.debug('[%s:%s] ������2 �J�����ԍ�������' % (app_id, app_name))
                ng_y_pix = abs(start_regimark_x - ng_x)
                ng_y_mm = ng_y_pix * actual_image_width / resize_image_width
                plus_direction = 1
            elif start_camera_num > ng_camera_num:
                logger.debug('[%s:%s] ������2 ���W�}�[�N�J�����ԍ���NG�J�����ԍ�' % (app_id, app_name))
                # �B�������~1�B���摜�̕�(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜��-[�J�n���W�}�[�Nx���W]-[�I�[�o�[���b�v����])�{[NG�摜x���W]
                register_image_fraction = (resize_image_width - ng_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (start_regimark_x * actual_image_width / resize_image_width )
                plus_direction = 1
            else:
                logger.debug('[%s:%s] ������2 ���W�}�[�N�J�����ԍ���NG�J�����ԍ�' % (app_id, app_name))
                # �B�������~1�B���摜�̕�(�I�[�o�[���b�v���O��)�{(���T�C�Y�摜��-[NG�摜x���W]-[�I�[�o�[���b�v����])�{[�J�n���W�}�[�N x���W]
                register_image_fraction = (resize_image_width - start_regimark_x - overlap_width_pix) * \
                                          actual_image_width / resize_image_width
                ng_y_mm = between_y_image_count * (actual_image_width - actual_image_overlap) + \
                          register_image_fraction + (ng_x * actual_image_width / resize_image_width )
                plus_direction = -1

        # �䗦�v�Z
        between_length_on_master = ng_x_pix * (stretch_rate_x / 100 )  / regimark_length_ratio
        between_width_on_master = master_image_height * ng_y_mm / (stretch_rate_y / 100)  / master_width

        # �}�X�^�摜���NG���W����
        length_on_master = regimark_x + (between_length_on_master * x_plus_direction)
        width_on_master = regimark_y + (between_width_on_master * plus_direction)

        if inspection_direction == 'S' and ng_face == 2:
            logger.debug('[%s:%s] ��������S ������2 Y�����]' % (app_id, app_name))
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'X' and ng_face == 1:
            logger.debug('[%s:%s] ��������X ������1 Y�����]' % (app_id, app_name))
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'Y' and ng_face == 1:
            logger.debug('[%s:%s] ��������Y ������1 X�����]' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master

        elif inspection_direction == 'Y' and ng_face == 2:
            logger.debug('[%s:%s] ��������Y ������2 X,Y�����]' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'R' and ng_face == 1:
            logger.debug('[%s:%s] ��������R ������1 X,Y�����]' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master
            width_on_master = master_image_height - width_on_master

        elif inspection_direction == 'R' and ng_face == 2:
            logger.debug('[%s:%s] ��������R ������2 X�����]' % (app_id, app_name))
            length_on_master = master_image_width - length_on_master

        else:
            pass

        result = True
    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)
        return result, length_on_master, width_on_master, ng_face, error, func_name

    return result, length_on_master, width_on_master, ng_face, error, func_name


# ------------------------------------------------------------------------------------
# ������             �FNG�s�����
#
# �����T�v           �F1.���_���W���NG�ӏ��̍s�E�����肷��B
#
# ����               �F���W�}�[�N���
#                      �}�X�^�摜���NG���W(X���W)
#                      �}�X�^�摜���NG���W(Y���W)
#                      �}�X�^���
#                      �ݒ背�W�}�[�N�Ԓ���[pix]
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�摜�s�E����
# ------------------------------------------------------------------------------------
def specific_ng_line_colum(line_info, length_on_master, width_on_master, mst_data, conf_regimark_between_length_pix,
                           inspection_direction, last_flag, logger):
    result = False
    judge_result = None
    top_points = []
    last_top_points = []
    next_top_points = []
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # �}�X�^��񂩂�񐔂��擾����B���J���������������߃}�X�^��񎫏��𗘗p���ď����擾����B
        colum_count = mst_data[master_data_dict['column_cnt']]
        ng_point = (length_on_master, width_on_master)

        # �񐔂ɉ�����N�s�ڂ̒��_���W���o
        for i in range(colum_count):
            top_point = mst_data[master_data_dict['top_point_a'] + i]
            sp_top_points = re.split('\),', (re.sub('\(|\)$| ', '', top_point)))
            top_points.append([[int(re.split(',', x)[0]), int(re.split(',', x)[1])] for x in sp_top_points])
            last_top_points.append(
                [[round(int(re.split(',', x)[0]) - conf_regimark_between_length_pix), int(re.split(',', x)[1])] for x in
                 sp_top_points])
            next_top_points.append(
                [[round(int(re.split(',', x)[0]) + conf_regimark_between_length_pix), int(re.split(',', x)[1])] for x in
                 sp_top_points])

        if last_flag == 0:
            line_num = int(line_info[0][0])
        else:
            if len(line_info) == 1:
                line_num = int(line_info[0][0])
            else:
                line_num = int(line_info[1][0])
        # N�s�ڂ̒��_���W���O����
        # �����Ɣ��肳�ꂽ���_�Ō��ʂ�ԋp����
        for j in range(len(top_points)):
            array_top_points = np.array(top_points[j])
            judge_line = cv2.pointPolygonTest(array_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                judge_result = [line_num, line_name_dict[j + 1]]
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        # N-1�s�ڂ̒��_���W���O����
        # �����Ɣ��肳�ꂽ���_�Ō��ʂ�ԋp����
        for k in range(len(last_top_points)):
            array_last_top_points = np.array(last_top_points[k])
            judge_line = cv2.pointPolygonTest(array_last_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                if inspection_direction == 'S' or inspection_direction == 'X':
                    logger.debug('[%s:%s] ��������S or X' % (app_id, app_name))
                    line = line_num - 1
                else:
                    logger.debug('[%s:%s] ��������Y or R' % (app_id, app_name))
                    line = line_num - 1

                judge_result = [line, line_name_dict[k + 1]]
                length_on_master = length_on_master + conf_regimark_between_length_pix
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        # N+1�s�ڂ̒��_���W���O����
        # �����Ɣ��肳�ꂽ���_�Ō��ʂ�ԋp����
        for l in range(len(next_top_points)):
            array_next_top_points = np.array(next_top_points[l])
            judge_line = cv2.pointPolygonTest(array_next_top_points, ng_point, False)
            if judge_line == 1:
                result = True
                if inspection_direction == 'S' or inspection_direction == 'X':
                    logger.debug('[%s:%s] ��������S or X' % (app_id, app_name))
                    line = line_num + 1
                else:
                    logger.debug('[%s:%s] ��������Y or R' % (app_id, app_name))
                    line = line_num + 1

                judge_result = [line, line_name_dict[l + 1]]
                length_on_master = length_on_master - conf_regimark_between_length_pix
                return result, judge_result, length_on_master, width_on_master, error, func_name
            else:
                pass

        result = True
        logger.debug('[%s:%s] ���_���W�O��NG' % (app_id, app_name))

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, judge_result, length_on_master, width_on_master, error, func_name


# ------------------------------------------------------------------------------------
# ������             �FNG�s�����i���_���W�O�j
#
# �����T�v           �F1.�s臒l�A��臒l�����NG�ӏ��̍s�E�����肷��B
#
# ����               �F���W�}�[�N���
#                      �}�X�^�摜���NG���W(X���W)
#                      �}�X�^�摜���NG���W(Y���W)
#                      �}�X�^���
#                      �ݒ背�W�}�[�N�Ԓ���[pix]
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�摜�s�E����
# ------------------------------------------------------------------------------------
def specific_ng_line_colum_border(line_info, length_on_master, width_on_master, mst_data,
                                  conf_regimark_between_length_pix, inspection_direction, last_flag, logger):
    result = False
    judge_result = None
    colum_result = None
    line_result = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        # �}�X�^��񂩂�񐔂��擾����B���J���������������߃}�X�^��񎫏��𗘗p���ď����擾����B
        colum_count = mst_data[master_data_dict['column_cnt']]

        # �񐔕����Ԕ�����s��
        for i in range(colum_count):
            # �������s��
            # 1��ڂ͗�臒l01�������m�F
            if i == 0:
                colum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + i])
                if width_on_master <= colum_threshold:
                    colum_result = line_name_dict[i + 1]
                    break
                else:
                    pass
            # 2��ڂ���(��-1)��ڂ͗�臒l01�����臒l0(N-2)�̂��ꂼ��̊Ԃ��m�F
            elif i == colum_count - 1:
                colum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + (i - 1)])
                if colum_threshold < width_on_master:
                    colum_result = line_name_dict[i + 1]
                    break
                else:
                    pass
            # ��L�ȊO(�ŏI��)�͗�臒l0(N-1)�ȏ���m�F
            else:
                min_cloum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + (i - 1)])
                max_cloum_threshold = int(mst_data[master_data_dict['column_threshold_01'] + i])
                if min_cloum_threshold < width_on_master <= max_cloum_threshold:
                    colum_result = line_name_dict[i + 1]
                    break

        # �s臒l���擾
        find_str = colum_result.lower()
        find_str_1 = 'line_threshold_' + find_str + '1'
        find_str_2 = 'line_threshold_' + find_str + '2'
        min_line_threshold = int(mst_data[master_data_dict[find_str_1]])
        max_line_threshold = int(mst_data[master_data_dict[find_str_2]])


        if last_flag == 0:
            line_num = int(line_info[0][0])
        else:
            if len(line_info) == 1:
                line_num = int(line_info[0][0])
            else:
                line_num = int(line_info[1][0])
        # ���肵����̍s臒l(�ŏ�)�ȉ��̏ꍇ�AN-1�s��N�s�̍s������s��
        if length_on_master <= min_line_threshold:
            # N-1�s�̍s臒l(�ő�)��N�s�̍s臒l(�ŏ�)�̊Ԃ̏ꍇ�A�e臒l����̐�Βl�ŊY���s����肷��
            if (max_line_threshold - conf_regimark_between_length_pix) < length_on_master <= min_line_threshold:
                abs_line = abs(length_on_master - min_line_threshold)
                abs_last_line = abs(length_on_master - (max_line_threshold - conf_regimark_between_length_pix))
                if abs_line <= abs_last_line:
                    line_result = line_num
                else:
                    if inspection_direction == 'S' or inspection_direction == 'X':
                        line_result = line_num - 1
                    else:
                        line_result = line_num - 1

                    length_on_master = length_on_master + conf_regimark_between_length_pix
            else:
                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_result = line_num - 1
                else:
                    line_result = line_num - 1

                length_on_master = length_on_master + conf_regimark_between_length_pix
        # ���肵����̍s臒l(�ŏ�)�ƍs臒l(�ő�)�̊Ԃ̏ꍇ�AN�s�Ɠ��肷��
        elif min_line_threshold < length_on_master <= max_line_threshold:
            line_result = line_num

        # ���肵����̍s臒l(�ő�)�ȏ�̏ꍇ�AN�s��N+1�s�̍s������s��
        elif max_line_threshold <= length_on_master:
            # N�s�̍s臒l(�ő�)��N+1�s�̍s臒l(�ŏ�)�̊Ԃ̏ꍇ�A�e臒l����̐�Βl�ŊY���s����肷��
            if max_line_threshold < length_on_master <= (min_line_threshold + conf_regimark_between_length_pix):
                abs_line = abs(length_on_master - max_line_threshold)
                abs_next_line = abs(length_on_master - (min_line_threshold + conf_regimark_between_length_pix))
                if abs_next_line <= abs_line:
                    line_result = line_num
                else:
                    if inspection_direction == 'S' or inspection_direction == 'X':
                        line_result = line_num + 1
                    else:
                        line_result = line_num + 1

                    length_on_master = length_on_master - conf_regimark_between_length_pix
            else:
                if inspection_direction == 'S' or inspection_direction == 'X':
                    line_result = line_num + 1
                else:
                    line_result = line_num + 1

                length_on_master = length_on_master - conf_regimark_between_length_pix

        judge_result = [line_result, colum_result]
        result = True

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, judge_result, length_on_master, width_on_master, error, func_name


# ------------------------------------------------------------------------------------
# ������             �F��_�����NG�����Z�o
#
# �����T�v           �F1.��_����NG�ӏ��܂ł̋������Z�o����B
#
# ����               �F���W�}�[�N���
#                      �}�X�^�摜���NG���W(X���W)
#                      �}�X�^�摜���NG���W(Y���W)
#                      NG�摜�s�E����
#                      �}�X�^���
#                      �}�X�^�摜�̕�(X��������)
#                      �}�X�^�摜�̍���(Y��������)
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      ��_�����NG�������
# ------------------------------------------------------------------------------------
def calc_distance_from_basepoint(length_on_master, width_on_master, judge_result, mst_data, master_image_width,
                                 master_image_height, logger):
    result = False
    ng_dist = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        line_name = judge_result[1]
        line_num = str(line_num_dict[line_name])

        # �}�X�^��񂩂�K�v�ȏ����擾����B���J���������������߃}�X�^��񎫏��𗘗p���ď����擾����B
        length = master_data_dict['length']
        width = master_data_dict['width']
        find_str_basepoint_x = 'base_point_' + line_num + '_x'
        find_str_basepoint_y = 'base_point_' + line_num + '_y'
        find_str_plusdirect_x = 'point_' + line_num + '_plus_direction_x'
        find_str_plusdirect_y = 'point_' + line_num + '_plus_direction_y'

        length = int(mst_data[length])
        width = int(mst_data[width])
        base_point_x = int(mst_data[master_data_dict[find_str_basepoint_x]])
        base_point_y = int(mst_data[master_data_dict[find_str_basepoint_y]])
        plus_direction_x = mst_data[master_data_dict[find_str_plusdirect_x]]
        plus_direction_y = mst_data[master_data_dict[find_str_plusdirect_y]]

        logger.debug('[%s:%s] ��_ [ %s, %s ]' % (app_id, app_name, base_point_x, base_point_y))
        logger.debug('[%s:%s] �v���X���� [ %s, %s ]' % (app_id, app_name, plus_direction_x, plus_direction_y))
        # ��_����̋���[pix]���Z�o����
        x_point_from_basepoint = length_on_master - base_point_x
        y_point_from_basepoint = base_point_y - width_on_master
        x_dist_ratio = length / master_image_width
        y_dist_ratio = width / master_image_height

        # �}�X�^���̃v���X������񂩂畄�����]�L�����m�F���A��_����̋���[mm]���Z�o����B
        if x_point_from_basepoint >= 0:
            if plus_direction_x == 0:
                x_dist_mm = (x_point_from_basepoint * -1) * x_dist_ratio
            else:
                x_dist_mm = x_point_from_basepoint * x_dist_ratio
        else:
            if plus_direction_x == 0:
                x_dist_mm = (x_point_from_basepoint * -1) * x_dist_ratio
            else:
                x_dist_mm = x_point_from_basepoint * x_dist_ratio

        if y_point_from_basepoint >= 0:
            if plus_direction_y == 0:
                y_dist_mm = y_point_from_basepoint * y_dist_ratio
            else:
                y_dist_mm = (y_point_from_basepoint * -1) * y_dist_ratio
        else:
            if plus_direction_y == 0:
                y_dist_mm = y_point_from_basepoint * y_dist_ratio
            else:
                y_dist_mm = (y_point_from_basepoint * -1) * y_dist_ratio

        result = True
        ng_dist = [round(x_dist_mm / 10), round(y_dist_mm / 10)]

    except Exception as error:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(error, logger, app_id, app_name)

    return result, ng_dist, error, func_name
