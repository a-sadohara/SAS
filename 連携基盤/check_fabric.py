# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\307 �[����
# ----------------------------------------

import os
import sys
import traceback
import cv2
import numpy as np
import csv
import logging.config
import codecs
import datetime
import configparser
import time
from multiprocessing import Pool
import multiprocessing as multi
import gc

import db_util
import error_detail
import error_util
import file_util
import masking_fabric
import marking_image

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_check_fabric.conf", disable_existing_loggers=False)
logger = logging.getLogger("check_fabric")

# �摜���T�C�Y�ݒ�t�@�C���Ǎ�
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/check_fabric_config.ini', 'SJIS')
# ���ʐݒ�t�@�C���Ǎ�
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �FDB�ڑ�
#
# �����T�v           �F1.DB�ɐڑ�����B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def create_connection():
    # DB�ɐڑ�����B
    conn, cur, res = db_util.create_connection(logger, app_id, app_name)
    return conn, cur, res


# ------------------------------------------------------------------------------------
# ������             �F�������擾
#
# �����T�v           �F1.�������e�[�u�����猟�������擾����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �X�e�[�^�X
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
#                      �������
# ------------------------------------------------------------------------------------
def select_fabric_info(conn, cur, status, unit_num):
    # �N�G�����쐬����B
    sql = 'select product_name, fabric_name, inspection_num, imaging_starttime from processing_status ' \
          'where unit_num = \'%s\' and rapid_endtime IS NOT NULL and marking_endtime IS NULL ' \
          'and (status = %s or status = %s) order by imaging_starttime asc, rapid_endtime asc ' \
          % (unit_num, status, int(status) + 1)

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FNG���擾
#
# �����T�v           �F1.RAPID��͏��e�[�u������NG�����擾����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      RAPID��͏��e�[�u����
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
#                      NG���
# ------------------------------------------------------------------------------------
def select_ng_info(conn, cur, fabric_info, inspection_num, rapid_ng_status, inspection_date, imaging_starttime,
                   unit_num):
    # �N�G�����쐬����B
    sql = 'select rp.num, rp.processing_id, rp.ng_image, rp.ng_point, rp.confidence, rp.rapid_host_name, ' \
          'fi.rapid_endtime from "rapid_%s_%s_%s" as rp inner join fabric_info as fi on ' \
          'rp.fabric_name = fi.fabric_name and rp.inspection_num = fi.inspection_num ' \
          'and rp.unit_num = fi.unit_num where rp.rapid_result = %s and fi.imaging_starttime = \'%s\' ' \
          'and fi.unit_num = \'%s\' and rp.edge_result is null and rp.masking_result is null ' \
          % (fabric_info, inspection_num, inspection_date, rapid_ng_status, imaging_starttime, unit_num)

    logger.debug('[%s:%s] NG���擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, ng_list, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)

    return result, ng_list, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FNG���CSV�쐬
#
# �����T�v           �F1.RAPID��͏��e�[�u������擾����NG����CSV�t�@�C���ɏo�͂���B
#
# ����               �FNG�摜�i�[�p�X
#                      CSV�o�̓p�X
#                      �i��
#                      ����
#                      �����ԍ�
#                      �擾NG���
#                      �t�@�C���A��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      CSV�t�@�C���p�X
# ------------------------------------------------------------------------------------
def create_csv(image_path, csv_path, product_name, fabric_name, inspection_num, ng_list, file_num, label_ng,
               label_others):
    csv_file_name = None
    result = False
    try:
        # �ϐ���`
        base_image_name = ng_list[0][2]
        date = base_image_name.split('_')[2]
        flag = inifile.get('CSV_INFO', 'flag')
        width = inifile.get('CSV_INFO', 'width')
        height = inifile.get('CSV_INFO', 'height')
        others_confidence = inifile.get('CSV_INFO', 'others_confidence')
        ng_csv_name = inifile.get('CSV_INFO', 'ng_csv_name')

        csv_file_name = csv_path + "\\" + date + "_" + product_name + "_" + fabric_name + "_" + str(inspection_num) \
                        + "_" + str(file_num) + "_" + ng_csv_name

        # CSV�t�@�C���o�͐�̍쐬
        tmp_result = file_util.make_directory(csv_path, logger, app_id, app_name)

        if tmp_result:
            pass
        else:
            return result, csv_file_name

        # �Ώ�CSV�t�@�C�����J��
        with codecs.open(csv_file_name, "w", "SJIS") as ofp:

            # �w�b�_�[��NG�摜�i�[�p�X����������
            header = "[DataPath]\"" + image_path + "\""
            ofp.write(header)
            ofp.write("\r\n")
            writer = csv.writer(ofp, quotechar=None)

            # NG��񌏐����ACSV�̌`�ɐ��`���ď�������
            for i in range(len(ng_list)):
                file_name = '"' + ng_list[i][2] + '"'
                point_xy = ng_list[i][3].split(',')
                point_x = point_xy[0]
                point_y = point_xy[1]
                confidence = ng_list[i][4]

                writer.writerow([flag, file_name, point_x, point_y, width, height, label_ng,
                                 confidence, label_others, others_confidence])

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, csv_file_name


# ------------------------------------------------------------------------------------
# ������             �F�������e�[�u���X�V
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
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column, time, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (status, column, time, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������e�[�u���X�VSQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�X�V
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ����ID
#                      RAPID�T�[�o�[�z�X�g��
#                      �X�e�[�^�X
#                      �X�V�J������
#                      �X�V����
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def update_processing_status(conn, cur, fabric_name, inspection_num, processing_id, rapid_host_name, status,
                             column, time, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'update processing_status set status = %s, %s = \'%s\' where fabric_name = \'%s\' and inspection_num = %s ' \
          'and processing_id = %s and rapid_host_name = \'%s\' and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column, time, fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime,
           unit_num)

    logger.debug('[%s:%s] �����X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�X�V(�����Ώ۔��ԁA�����ԍ��̑S���R�[�h)
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �X�e�[�^�X
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def update_processing_status_all(conn, cur, fabric_name, inspection_num, update_status, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'update processing_status set status = %s where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (update_status, fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �S�����X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�[���菈��
#
# �����T�v           �F1.���H�����摜�f�[�^����A臒l��p���Ĕ����̒[���ǂ����𔻒肷��B
#
# ����               �FNG�摜
#                      NG���
#                      ����f�f�[�^
#
# �߂�l             �F�[���茋��
# ------------------------------------------------------------------------------------
def filter_fabric(file, fabric_result, white):
    fname, fext = os.path.splitext(os.path.basename(file))

    try:
        crop_x = fabric_result["point"]["x"]
        crop_y = fabric_result["point"]["y"]
        window_width = fabric_result["width"]
        window_height = fabric_result["height"]

        # �摜�ǂݍ���
        result, img_org = file_util.read_image(file, logger, app_id, app_name)
        crop_img = img_org[crop_y:crop_y + window_height, crop_x:crop_x + window_width]

        # �O���[�X�P�[����
        # �m�C�Y�ጸ�p�̂ڂ�������
        img_gray = cv2.cvtColor(crop_img, cv2.COLOR_BGR2GRAY)
        img_gray = cv2.blur(img_gray, (5, 5))

        # 臒l254��2�l��
        ret, img_bin = cv2.threshold(img_gray, 254, white, cv2.THRESH_BINARY)

        # ����f�̊����Ŕ����̒[������
        img_bin[img_bin == white] = 1
        rate = np.sum(img_bin) / (100 * 100)
        rate_result = {}
        if rate >= 0.001:  # 0.001������f��100x100�̒��ŁA10px�ȏ�Ȃ甽���̒[
            fabric_result["label"] = "_others"
            rate_result[fname + "," + str(crop_x) + "," + str(crop_y)] = "_others"
        else:
            rate_result[fname + "," + str(crop_x) + "," + str(crop_y)] = "NG"

        return rate_result
    except Exception as e:
        # �G���[�ɂȂ����摜�����o�͂���B

        raise e


# ------------------------------------------------------------------------------------
# ������             �F�[�����������
#
# �����T�v           �F1.�[���菈���ւƈ�����n��
#
# ����               �F����
#
# �߂�l             �F�[���茋��
# ------------------------------------------------------------------------------------
def wrapper_filter_fabric(args):
    result = filter_fabric(*args)
    return result


# ------------------------------------------------------------------------------------
# ������             �F�[����
#
# �����T�v           �F1.���ʃt�@�C����ǂ݂��݁A�[������s���āA���ʂ�CSV�t�@�C���Ƃ��ďo�͂���B
#
# ����               �FNG���CSV�t�@�C��
#                      �[���茋��CSV�t�@�C��
#                      �J�e�S���[��(NG)
#                      �J�e�S���[��(others)
#                      �������
#                      ����f�f�[�^
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �������
# ------------------------------------------------------------------------------------
def check_fabric(ng_result_file, checkfabric_result_file, label_ng, label_others, process_list, white):
    result = False
    try:
        # ���ʃt�@�C���ǂݍ���
        logger.debug('[%s:%s] ���ʃt�@�C���Ǎ����J�n���܂��B ' % (app_id, app_name))
        tmp_result, result_data = file_util.read_result_file(ng_result_file, logger, app_id, app_name)

        if tmp_result:
            logger.debug('[%s:%s] ���ʃt�@�C���Ǎ����I�����܂����B ' % (app_id, app_name))
        else:
            logger.error('[%s:%s] ���ʃt�@�C���Ǎ������s���܂����B ' % (app_id, app_name))
            return result, process_list

        # NG�摜�p�X�̕ϐ���`
        image_dir = result_data["datapath"]

        logger.debug('[%s:%s] �[���菈���������J�n���܂��B ' % (app_id, app_name))
        # ���񏈗��p�Ɉ���������
        process_file_args = []
        for ng_result in result_data["data"]:
            if ng_result["label"] != label_ng:
                continue
            args = []
            args.append(image_dir + ng_result["filename"])
            args.append(ng_result)
            args.append(white)
            process_file_args.append(args)
        logger.debug('[%s:%s] �[���菈���������I�����܂����B ' % (app_id, app_name))

        # ���񏈗����s
        logger.debug('[%s:%s] �[���菈���̕�����s���J�n���܂��B ' % (app_id, app_name))
        # p = Pool(multi.cpu_count())
        p = Pool(3)
        check_result = p.map(wrapper_filter_fabric, process_file_args)
        p.close()
        logger.debug('[%s:%s] �[���菈���̕�����s���I�����܂����B ' % (app_id, app_name))

        # ���񏈗����ʎ擾
        logger.debug('[%s:%s] �[���菈�����ʎ擾���J�n���܂��B ' % (app_id, app_name))
        for data in check_result:
            if data is None:
                continue
            else:
                for key, value in data.items():
                    if value == label_others:
                        fname, x, y = key.split(",")
                        for ng_result in result_data["data"]:
                            if fname + ".jpg" == ng_result["filename"] and x == str(
                                    ng_result["point"]["x"]) and y == str(ng_result["point"]["y"]):
                                ng_result["label"] = label_others
                                break
                            else:
                                pass
                    else:
                        pass

        for ng_result in result_data["data"]:
            for i in range(len(process_list)):
                if ng_result["label"] == label_others and process_list[i][3] == ng_result["filename"] \
                        and str(process_list[i][4]) == str(ng_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(ng_result["point"]["y"]):
                    process_list[i].append(label_others)
                    continue

                elif ng_result["label"] == label_ng and process_list[i][3] == ng_result["filename"] \
                        and str(process_list[i][4]) == str(ng_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(ng_result["point"]["y"]):
                    process_list[i].append(label_ng)
                    continue
                else:
                    pass

        logger.debug('[%s:%s] �[���茋�ʎ擾���I�����܂����B ' % (app_id, app_name))

        # ���ʃt�@�C���̏o��
        logger.debug('[%s:%s] �[���茋��CSV�t�@�C���̏o�͂��J�n���܂��B ' % (app_id, app_name))
        tmp_result = file_util.write_result_file(checkfabric_result_file, result_data, image_dir, logger,
                                                 app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] �[���茋��CSV�t�@�C���̏o�͂��I�����܂����B ' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �[���茋��CSV�t�@�C���̏o�͂����s���܂����B ' % (app_id, app_name))
            return result, process_list

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_list


# ------------------------------------------------------------------------------------
# ������             �FRAPID���������擾
#
# �����T�v           �F1.�������e�[�u������RAPID�����������擾����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
#                      RAPID��������
# ------------------------------------------------------------------------------------
def select_rapid_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    # �N�G�����쐬����B
    sql = 'select rapid_endtime from fabric_info where fabric_name = \'%s\' and inspection_num = %s ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] RAPID���������擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���X�V
#
# �����T�v           �F1.�[����A�}�X�L���O���茋�ʂ�RAPID��͏��e�[�u���ɔ��f����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g�X�e�[�^�X
#                      ����
#                      �����ԍ�
#                      ����ID
#                      �[���茋��
#                      �}�X�L���O���茋��
#                      �A��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def update_rapid_anarysis(conn, cur, fabric_name, inspection_num, processing_id, edge_result, masking_result,
                          num, inspection_date, unit_num):
    ### �N�G�����쐬����
    sql = 'update "rapid_%s_%s_%s" set edge_result = %s, masking_result = %s ' \
          'where num = %s and processing_id = %s and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, edge_result, masking_result, num, processing_id, unit_num)

    logger.debug('[%s:%s] RAPID��͏��e�[�u���X�VSQL %s' % (app_id, app_name, sql))
    ### �������e�[�u�����X�V
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F���茋�ʓo�^
#
# �����T�v           �F1.�[����ƃ}�X�L���O���茋�ʂ��������ƕR�Â��āARAPID��͏��e�[�u�����X�V����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �������
#                      �J�e�S���[�iNG�j
#                      �J�e�S���[�i_others�j
#                      ����NG�i2�j
#                      ����OK�i1�j
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������R�Â�����
# ------------------------------------------------------------------------------------
def update_check_result(conn, cur, fabric_name, inspection_num, process_list, label_ng,
                        judge_ng, judge_ok, inspection_date, unit_num):
    result = False
    process_id_list = []
    try:
        logger.debug('[%s:%s] ���茋�ʓo�^�����Ώۏ�� %s' % (app_id, app_name, process_list))

        for i in range(len(process_list)):

            # �ϐ���`
            num = process_list[i][0]
            processing_id = process_list[i][1]
            rapid_host_name = process_list[i][2]
            file_name = process_list[i][3]

            # �[���茋��=NG�̏ꍇ
            if process_list[i][6] == label_ng:
                logger.debug(
                    '[%s:%s] �[���茋��=NG [�A��=%s] [����ID=%s] [�摜��=%s]' % (app_id, app_name, num, processing_id, file_name))
                edge_result = judge_ng
            # �[���茋��=OK�̏ꍇ
            else:
                logger.debug(
                    '[%s:%s] �[���茋��=OK [�A��=%s] [����ID=%s] [�摜��=%s]' % (app_id, app_name, num, processing_id, file_name))
                edge_result = judge_ok

            # �}�X�L���O���茋��=NG�̏ꍇ
            if process_list[i][7] == label_ng:
                logger.debug('[%s:%s] �}�X�L���O���茋��=NG [�A��=%s] [����ID=%s] [�摜��=%s]' % (
                    app_id, app_name, num, processing_id, file_name))
                masking_result = judge_ng
                process_id_list += [[processing_id, rapid_host_name]]

            # �}�X�L���O���茋��=OK�̏ꍇ
            else:
                logger.debug('[%s:%s] �}�X�L���O���茋��=OK [�A��=%s] [����ID=%s] [�摜��=%s]' % (
                    app_id, app_name, num, processing_id, file_name))
                masking_result = judge_ok

            logger.debug('[%s:%s] RAPID��͏��e�[�u����NG���X�V���J�n���܂��B' % (app_id, app_name))
            tmp_result, conn, cur = update_rapid_anarysis(conn, cur, fabric_name, inspection_num, processing_id,
                                                          edge_result, masking_result, num, inspection_date, unit_num)

            if tmp_result:
                logger.debug('[%s:%s] RAPID��͏��e�[�u����NG���X�V���I�����܂����B' % (app_id, app_name))
                conn.commit()
            else:
                logger.error('[%s:%s] RAPID��͏��e�[�u����NG���X�V�����s���܂����B' % (app_id, app_name))
                conn.rollback()
                return result, process_id_list, conn, cur

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_id_list, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�}�X�L���O���茏���擾
#
# �����T�v           �F1.RAPID��͏��e�[�u������}�X�L���O���茏�����擾����B
#
# ����               �F�J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      RAPID��͏��e�[�u����
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
#                      �X�L���O���茏��
# ------------------------------------------------------------------------------------
def select_masking_result(conn, cur, fabric_name, inspection_num, inspection_date, unit_num):
    # �N�G�����쐬����B
    sql = 'select count(num) from "rapid_%s_%s_%s" ' \
          'where fabric_name = \'%s\'and inspection_num = %s and unit_num = \'%s\' and masking_result is null ' \
          % (fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, unit_num)

    logger.debug('[%s:%s] �}�X�L���O���茏���擾SQL %s' % (app_id, app_name, sql))
    # �������e�[�u�����瑍�B�������A�����ϖ����A�B�������������擾����B
    result, fabric_info, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, fabric_info, conn, cur


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
# �֐���             �F��ꔻ�菈��
#
# �����T�v           �F1.�[����A�}�X�L���O������Ăяo��
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      NG���
#                      �i��
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def base_check_fabric(conn, cur, ng_list, product_name, fabric_name, inspection_num, file_num,
                      imaging_starttime, unit_num):
    result = False
    try:
        ng_csv_name = inifile.get('CSV_INFO', 'ng_csv_name')
        fabric_csv_name = inifile.get('CSV_INFO', 'fabric_csv_name')
        masking_csv_name = inifile.get('CSV_INFO', 'masking_csv_name')
        root_path = common_inifile.get('FILE_PATH', 'rk_path')
        ng_path = inifile.get('FILE_PATH', 'ng_path')

        processing_column_start = inifile.get('COLUMN', 'processing_column_start')
        processing_status_start = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_start')
        fabric_status_start = common_inifile.get('FABRIC_STATUS', 'imageprocessing_start')
        fabric_column_start = inifile.get('COLUMN', 'fabric_column_start')
        processing_column_end = inifile.get('COLUMN', 'processing_column_end')
        processing_status_end = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_end')

        white = int(inifile.get('VALUE', 'white'))
        label_ng = inifile.get('CSV_INFO', 'label_ng')
        label_others = inifile.get('CSV_INFO', 'label_others')
        judge_ok = int(common_inifile.get('ANALYSIS_STATUS', 'ok'))
        judge_ng = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))

        process_list = []
        processing_status_list = []

        logger.debug('[%s:%s] NG���%s���ȏ㑶�݂��܂��B' % (app_id, app_name, len(ng_list)))
        logger.debug('[%s:%s] NG���CSV�t�@�C���쐬���J�n���܂��B' % (app_id, app_name))

        # �ϐ���`

        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))
        csv_path = root_path + "\\CSV\\" + inspection_date + "_" + product_name + "_" + fabric_name + "_" + \
                   str(inspection_num)
        image_path = root_path + "\\" + ng_path + "\\" + inspection_date + "_" + product_name + "_" \
                     + fabric_name + "_" + str(inspection_num) + "\\"

        # �擾����NG��񂩂�CSV�t�@�C�����쐬����
        tmp_result, ng_result_file = create_csv(image_path, csv_path, product_name, fabric_name,
                                                inspection_num, ng_list, file_num, label_ng, label_others)
        if tmp_result:
            logger.debug('[%s:%s] NG���CSV�t�@�C���쐬���I�����܂����B' % (app_id, app_name))

        else:
            logger.error('[%s:%s] NG���CSV�t�@�C���쐬�Ɏ��s���܂����B' % (app_id, app_name))
            return result, conn, cur

        # �擾����NG��񌏐����A�����X�e�[�^�X���X�V����
        for i in range(len(ng_list)):
            num = ng_list[i][0]
            processing_id = ng_list[i][1]
            image_name = ng_list[i][2]
            rapid_host_name = ng_list[i][5]
            point_xy = ng_list[i][3].split(',')
            point_x = int(point_xy[0])
            point_y = int(point_xy[1])
            process_list += [[num, processing_id, rapid_host_name, image_name, point_x, point_y]]
            check_list = [x for x in processing_status_list if processing_id == x[:][0] and rapid_host_name in x[:][1]]
            if i == 1:
                processing_status_list += [[processing_id, rapid_host_name]]
            elif len(check_list) == 0:
                processing_status_list += [[processing_id, rapid_host_name]]
            else:
                pass

        for i in range(len(processing_status_list)):
            start_time = datetime.datetime.now()
            processing_id = processing_status_list[i][0]
            rapid_host_name = processing_status_list[i][1]
            logger.debug('[%s:%s] �����X�e�[�^�X�X�V���J�n���܂��B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                         (app_id, app_name, processing_id, rapid_host_name))

            tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num,
                                                             processing_id, rapid_host_name, processing_status_start,
                                                             processing_column_start, start_time, imaging_starttime,
                                                             unit_num)
            if tmp_result:
                logger.debug('[%s:%s] �����X�e�[�^�X�X�V���I�����܂����B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
            else:
                logger.error('[%s:%s] �����X�e�[�^�X�X�V�����s���܂����B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
                conn.rollback()
                return result, conn, cur

        conn.commit()

        checkfabric_result_file = csv_path + "\\" + inspection_date + "_" + product_name + "_" + fabric_name + \
                                  "_" + str(inspection_num) + "_" + str(file_num) + "_" + fabric_csv_name
        logger.debug('[%s:%s] �[������J�n���܂��B' % (app_id, app_name))

        # �[���菈�����s��
        tmp_result, process_list = check_fabric(ng_result_file, checkfabric_result_file, label_ng,
                                                label_others, process_list, white)

        if tmp_result:
            logger.debug('[%s:%s] �[���肪�I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �[���肪���s���܂����B' % (app_id, app_name))
            return result, conn, cur

        masking_result_file = csv_path + "\\" + inspection_date + "_" + product_name + "_" + fabric_name + "_" + str(
            inspection_num) + "_" + str(file_num) + "_" + masking_csv_name

        logger.debug('[%s:%s] �}�X�L���O������J�n���܂��B' % (app_id, app_name))

        # �}�X�L���O������Ăяo��
        tmp_result, process_list = masking_fabric.main(checkfabric_result_file, masking_result_file,
                                                       label_ng, label_others, process_list, inspection_date)
        if tmp_result:
            logger.debug('[%s:%s] �}�X�L���O���肪�I�����܂����B %s' % (app_id, app_name, process_list))
        else:
            logger.error('[%s:%s] �}�X�L���O���肪���s���܂����B' % (app_id, app_name))
            return result, conn, cur
        end_time = datetime.datetime.now()

        logger.debug('[%s:%s] �[���茋�ʋy�у}�X�L���O����̌��ʓo�^���J�n���܂��B' % (app_id, app_name))

        tmp_result, process_id_list, conn, cur = update_check_result(conn, cur, fabric_name, inspection_num,
                                                                     process_list, label_ng, judge_ng, judge_ok,
                                                                     inspection_date, unit_num)

        if tmp_result:
            logger.debug('[%s:%s] �[���茋�ʋy�у}�X�L���O����̌��ʓo�^���I�����܂����B' % (app_id, app_name))
            conn.commit()
        else:
            logger.error('[%s:%s] �[���茋�ʋy�у}�X�L���O����̌��ʓo�^�����s���܂����B' % (app_id, app_name))
            return result, conn, cur

        for i in range(len(processing_status_list)):
            processing_id = processing_status_list[i][0]
            rapid_host_name = processing_status_list[i][1]
            logger.debug('[%s:%s] �����X�e�[�^�X�̍X�V���J�n���܂��B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                         (app_id, app_name, processing_id, rapid_host_name))
            tmp_result = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                  rapid_host_name, processing_status_end, processing_column_end,
                                                  end_time, imaging_starttime, unit_num)
            if tmp_result:
                logger.debug('[%s:%s]  �����X�e�[�^�X�̍X�V���I�����܂����B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
            else:
                logger.error('[%s:%s]  �����X�e�[�^�X�̍X�V�����s���܂����B [����ID=%s] [RAPID�T�[�o�[�z�X�g��=%s]' %
                             (app_id, app_name, processing_id, rapid_host_name))
                return conn, cur, result, process_list

        conn.commit()

        process_id_list.sort(key=lambda x: x[0])
        logger.debug('[%s:%s]  �摜�}�[�L���O�������J�n���܂��B' % (app_id, app_name))
        tmp_result, conn, cur = marking_image.main(conn, cur, product_name, fabric_name, inspection_num,
                                                   masking_result_file, process_id_list, imaging_starttime,
                                                   inspection_date, unit_num)

        if tmp_result:
            logger.debug('[%s:%s]  �摜�}�[�L���O�������I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s]  �摜�}�[�L���O���������s���܂����B' % (app_id, app_name))
            result = False
            return result, conn, cur

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F���C������
#
# �����T�v           �F1.�������e�[�u�����猟�������擾���ANG����臒l�ɒB�����ꍇ
#                       NG��񂩂�CSV�t�@�C�����쐬���A�[������s���B
#                      2.�}�X�L���O������Ăяo���B
#                      3.�ꌟ���ԍ��̏����������������ǂ����̔�����s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def main():
    conn = None
    cur = None
    error_file_name = None
    result = False
    try:

        error_file_name = inifile.get('ERROR_FILE', 'file')
        select_status = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
        fabric_column_end = inifile.get('COLUMN', 'fabric_column_end')
        fabric_status_end = common_inifile.get('FABRIC_STATUS', 'imageprocessing_end')
        ng_num = int(inifile.get('VALUE', 'ng_num'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        rapid_ng_status = common_inifile.get('ANALYSIS_STATUS', 'ng')
        processing_status_end = common_inifile.get('PROCESSING_STATUS', 'checkfabric_filter_end')
        file_num = 1
        fabric_status_start = common_inifile.get('FABRIC_STATUS', 'imageprocessing_start')
        fabric_column_start = inifile.get('COLUMN', 'fabric_column_start')

        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s�@�\���N�����܂����B' % (app_id, app_name, app_name))

        while True:
            # DB�ڑ����s��
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
            result, conn, cur = create_connection()
            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
                sys.exit()

            while True:
                # ���������擾����
                logger.debug('[%s:%s] �������擾���J�n���܂��B' % (app_id, app_name))
                result, fabric_info, conn, cur = select_fabric_info(conn, cur, select_status, unit_num)
                if result:
                    logger.debug('[%s:%s] �������擾���I�����܂����B������� %s ' % (app_id, app_name, fabric_info))
                    conn.commit()
                else:
                    logger.debug('[%s:%s] �������擾�����s���܂����B' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                if fabric_info is not None:

                    logger.debug('[%s:%s] ������񂪑��݂��܂��B' % (app_id, app_name))

                    # �ϐ���`
                    product_name, fabric_name, inspection_num, imaging_starttime = \
                        fabric_info[0], fabric_info[1], str(fabric_info[2]), fabric_info[3]
                    logger.debug('[%s:%s] �������e�[�u���X�V���J�n���܂��B' % (app_id, app_name))

                    # �J�n�������擾����
                    start_time = datetime.datetime.now()

                    # ��������
                    inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                    # �������e�[�u�����X�V����
                    tmp_result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                               fabric_status_start, fabric_column_start, start_time,
                                                               imaging_starttime, unit_num)

                    if tmp_result:
                        logger.debug('[%s:%s] �������e�[�u���X�V���I�����܂����B' % (app_id, app_name))
                        conn.commit()
                    else:
                        logger.error('[%s:%s]  �������e�[�u���X�V�Ɏ��s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        return result, conn, cur

                    while True:
                        logger.debug('[%s:%s] NG���擾���J�n���܂��B' % (app_id, app_name))
                        # NG�����擾����
                        result, ng_list, conn, cur = select_ng_info(conn, cur, fabric_name, inspection_num,
                                                                    rapid_ng_status, inspection_date, imaging_starttime,
                                                                    unit_num)

                        if result:
                            logger.debug('[%s:%s] NG���擾���I�����܂����B' % (app_id, app_name))

                        else:
                            logger.error('[%s:%s] NG���擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_list) == 0:
                            logger.debug('[%s:%s] NG��񂪑��݂��܂���B' % (app_id, app_name))

                        else:
                            rapid_endtime = ng_list[0][6]

                            if len(ng_list) >= ng_num:
                                logger.info('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O�������J�n���܂��B '
                                            '[����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                            (app_id, app_name, fabric_name, inspection_num, inspection_date))
                                result, conn, cur = base_check_fabric(conn, cur, ng_list, product_name, fabric_name,
                                                                      inspection_num, file_num, imaging_starttime,
                                                                      unit_num)
                                if result:
                                    logger.debug('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O�������I�����܂����B' % (app_id, app_name))
                                else:
                                    logger.error('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O���������s���܂����B' % (app_id, app_name))
                                    sys.exit()

                            else:
                                if rapid_endtime is not None:
                                    logger.debug('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O�������J�n���܂��B' % (app_id, app_name))
                                    result, conn, cur = base_check_fabric(conn, cur, ng_list, product_name, fabric_name,
                                                                          inspection_num, file_num, imaging_starttime,
                                                                          unit_num)
                                    if result:
                                        logger.debug('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O�������I�����܂����B' % (app_id, app_name))
                                    else:
                                        logger.error('[%s:%s] �[����E�}�X�L���O����E�}�[�L���O���������s���܂����B' % (app_id, app_name))
                                        sys.exit()
                                else:
                                    logger.debug('[%s:%s] NG���%s���ɒB���܂���B' % (app_id, app_name, ng_num))
                                    time.sleep(sleep_time)
                                    continue

                        logger.debug('[%s:%s] ��������������J�n���܂��B' % (app_id, app_name))
                        logger.debug('[%s:%s] RAPID��͊��������̎擾���J�n���܂��B' % (app_id, app_name))
                        # RAPID��͊����������擾����
                        result, fabric_info, conn, cur = select_rapid_endtime(conn, cur, fabric_name, inspection_num,
                                                                              imaging_starttime, unit_num)
                        if result:
                            logger.debug('[%s:%s] RAPID��͊��������̎擾���I�����܂����B' % (app_id, app_name))
                            logger.debug('[%s:%s] RAPID��͊�������: %s' % (app_id, app_name, fabric_info))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] RAPID��͊��������̎擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] �[����E�}�X�L���O���茋�ʂ̎擾���J�n���܂��B' % (app_id, app_name))
                        # �[����E�}�X�L���O���茋�ʂ��擾����
                        result, rapid_info, conn, cur = select_masking_result(conn, cur, fabric_name, inspection_num,
                                                                              inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] �[����E�}�X�L���O���茋�ʂ̎擾���I�����܂����B' % (app_id, app_name))
                            logger.debug('[%s:%s] �[����E�}�X�L���O���茋�ʌ���: %s' % (app_id, app_name, rapid_info))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] �[����E�}�X�L���O���茋�ʂ̎擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        # �ϐ���`
                        rapid_endtime = fabric_info[0]
                        count_masking_result = rapid_info[0]

                        if rapid_endtime is not None and count_masking_result == 0:
                            logger.info('[%s:%s] �����ԍ��̉摜�S�Ă̔��肪�I�����܂����B '
                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            # �����X�e�[�^�X�e�[�u�����X�V����
                            logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                            result, conn, cur = update_processing_status_all(conn, cur, fabric_name, inspection_num,
                                                                             processing_status_end, imaging_starttime,
                                                                             unit_num)

                            if result:
                                logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V�����s���܂����B' % (app_id, app_name))
                                sys.exit()

                            # �I���������擾����
                            end_time = datetime.datetime.now()

                            # �������e�[�u�����X�V����
                            logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_status_end, fabric_column_end, end_time,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] �������e�[�u���̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] �������e�[�u���̍X�V�����s���܂����B' % (app_id, app_name))
                                sys.exit()

                            logger.info('[%s:%s] �[����A�}�X�L���O����A�}�[�L���O�����͐���ɏI�����܂����B '
                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            file_num = 1
                            break

                        else:
                            logger.info('[%s:%s] ����Ώۂ̉摜���c���Ă��܂��B '
                                        '[����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            file_num += 1
                            time.sleep(sleep_time)
                            break

                else:
                    logger.info('[%s:%s] ������񂪑��݂��܂���B' % (app_id, app_name))
                    logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                    result = close_connection(conn, cur)

                    if result:
                        logger.debug('[%s:%s] DB�ڑ��̐ؒf���������܂����B' % (app_id, app_name))
                    else:
                        logger.error('[%s:%s] DB�ڑ��̐ؒf�Ɏ��s���܂����B' % (app_id, app_name))
                        sys.exit()

                    logger.debug('[%s:%s] %s�b�X���[�v���܂�' % (app_id, app_name, sleep_time))
                    time.sleep(sleep_time)
                    break


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
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B' % (app_id, app_name))
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


if __name__ == "__main__":
    import multiprocessing

    multiprocessing.freeze_support()
    main()
