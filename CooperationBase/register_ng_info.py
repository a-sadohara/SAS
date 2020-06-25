# -*- coding: SJIS -*-
# NG�s�񔻒�@�\
#
import configparser
import sys
import datetime
import time
import logging.config
import traceback

import db_util
import error_detail
import error_util
import register_ng_info_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_ng_info.conf", disable_existing_loggers=False)
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


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X���擾�iDB�|�[�����O�j
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u�����甽�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �����X�e�[�^�X�e�[�u���̃X�e�[�^�X
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����X�e�[�^�X���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_processing_target(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select ii.inspection_direction ' \
          'FROM fabric_info as fi inner join inspection_info_header as ii on ' \
          'fi.fabric_name = ii.fabric_name and fi.inspection_num = ii.inspection_num and fi.imaging_starttime = ' \
          'ii.start_datetime and ii.unit_num = \'%s\' ' \
          'where imageprocessing_starttime IS NOT NULL and ng_endtime IS NULL and status <> 0 and fi.fabric_name = \'%s\' and fi.inspection_num = %s and fi.imaging_starttime = \'%s\' ' \
          'order by fi.imaging_starttime asc, imageprocessing_starttime asc ' % (unit_num, fabric_name, inspection_num, imaging_starttime)

    logger.debug('[%s:%s] �����X�e�[�^�X���擾SQL [%s]' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�[����A�}�X�L���O����A�}�[�L���O���������擾
#
# �����T�v           �F1.�������e�[�u������[����A�}�X�L���O����A�}�[�L���O�����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �[����A�}�X�L���O����A�}�[�L���O��������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_imageprocess_time(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select imageprocessing_endtime FROM fabric_info ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �[����A�}�X�L���O����A�}�[�L���O���������擾SQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�������X�e�[�^�X�X�V
#
# �����T�v           �F1.�������e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �������e�[�u���̃X�e�[�^�X
#                      �X�V�J������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info(conn, cur, fabric_name, inspection_num, status, column_name, imaging_starttime, unit_num):
    update_time = datetime.datetime.now()

    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s, %s = \'%s\' where fabric_name = \'%s\' and inspection_num  = %s and ' \
          'imaging_starttime = \'%s\' and unit_num = \'%s\' and %s IS NULL' \
          % (status, column_name, update_time, fabric_name, inspection_num, imaging_starttime, unit_num, column_name)

    logger.debug('[%s:%s] �������X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�������e�[�u�����X�V����B
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�����Ώۉ摜����
#
# �����T�v           �F1.RAPID��͏��e�[�u�����珈���Ώۉ摜���擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      �}�X�L���O���ʃX�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�摜���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_processing_target_image(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### �N�G�����쐬����
    sql = 'select num, ng_image, ng_point from \"rapid_%s_%s_%s\" where fabric_name = \'%s\' and inspection_num = %s ' \
          'and masking_result = %s and unit_num = \'%s\' and master_point IS NULL' \
          % (fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, status, unit_num)

    logger.debug('[%s:%s] �����Ώۉ摜����SQL [%s]' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āARAPID��͏��e�[�u�����珈���Ώۉ摜�����擾����B
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�������}�[�L���O���������擾
#
# �����T�v           �F1.�������e�[�u������}�[�L���O�����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �}�[�L���O��������
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fabric_marking_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select imageprocessing_endtime from fabric_info ' \
          'where fabric_name = \'%s\' and inspection_num = %s and imaging_starttime = \'%s\' and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, imaging_starttime, unit_num)

    logger.debug('[%s:%s] �������}�[�L���O���������擾SQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āA�i��o�^���e�[�u������}�X�^�����擾����B
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���X�V
#
# �����T�v           �F1.RAPID��͏��e�[�u�����珈���Ώۉ摜���擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      NG���W
#                      �X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�摜���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_rapid_analysis(conn, cur, fabric_name, inspection_num, ng_file, status, inspection_date, unit_num):
    ng_image = ng_file[1]
    ng_point = ng_file[2]

    ### �N�G�����쐬����
    sql = 'update \"rapid_%s_%s_%s\" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s and ng_image = \'%s\' and ng_point = \'%s\' ' \
          'and unit_num = \'%s\'' \
          % (fabric_name, inspection_num, inspection_date, status, status, status, fabric_name, inspection_num,
             ng_image, ng_point, unit_num)

    logger.debug('[%s:%s] RAPID��͏��e�[�u���X�VSQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āARAPID��͏��e�[�u�����X�V����B
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FNG�s�E�񖢓o�^NG�摜���m�F
#
# �����T�v           �F1.RAPID��͏��e�[�u������NG�s�E�񖢓o�^NG�摜�����擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ��͌��ʃX�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�s�E�񖢓o�^NG�摜��
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_judged_ng_info_count(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### �N�G�����쐬����
    sql = 'select count(*) from \"rapid_%s_%s_%s\" where fabric_name = \'%s\' and inspection_num = %s ' \
          'and rapid_result = %s and edge_result = %s and masking_result = %s and unit_num = \'%s\' ' \
          'and master_point IS NULL ' \
          % (
              fabric_name, inspection_num, inspection_date, fabric_name, inspection_num, status, status, status,
              unit_num)

    logger.debug('[%s:%s] NG�s�E�񖢓o�^NG�摜���m�FSQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āARAPID��͏��e�[�u�����珈���Ώۉ摜�����擾����B
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���X�V
#
# �����T�v           �F1.RAPID��͏��e�[�u�����珈���Ώۉ摜���擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      NG���W
#                      �X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      NG�摜���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_rapid_analysis_all(conn, cur, fabric_name, inspection_num, status, inspection_date, unit_num):
    ### �N�G�����쐬����
    sql = 'update \"rapid_%s_%s_%s\" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          'where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\'' \
          % (
              fabric_name, inspection_num, inspection_date, status, status, status, fabric_name, inspection_num,
              unit_num)

    logger.debug('[%s:%s] RAPID��͏��e�[�u���X�VSQL %s' % (app_id, app_name, sql))
    # DB���ʏ������Ăяo���āARAPID��͏��e�[�u�����X�V����B
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


def main(product_name, fabric_name, inspection_num, imaging_starttime):
    # �ϐ���`
    # �R�l�N�V�����I�u�W�F�N�g, �J�[�\���I�u�W�F�N�g
    conn = None
    cur = None
    # �G���[�t�@�C����
    error_file_name = None

    try:
        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # �X�e�[�^�X(�������e�[�u��)�@�u8�FNG�s�E��o�^�J�n�v
        fabric_ng_start_status = int(common_inifile.get('FABRIC_STATUS', 'ng_start'))
        # �X�e�[�^�X(�������e�[�u��)�@�u9�FNG�s�E��o�^�����v
        fabric_ng_end_status = int(common_inifile.get('FABRIC_STATUS', 'ng_end'))
        # �X�V�J������(�������e�[�u��)�@�ung_starttime�v
        fabric_ng_start_column = inifile.get('COLUMN', 'processing_ng_start_column')
        # �X�V�J������(�������e�[�u��)�@�ung_endtime�v
        fabric_ng_end_column = inifile.get('COLUMN', 'processing_ng_end_column')
        # �X�e�[�^�X(RAPID��͏��e�[�u��)�@�u2�FNG�v
        anarysis_ng_status = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))
        # �X�e�[�^�X(RAPID��͏��e�[�u��)�@�u4�F�Ώۍs�Ȃ��v
        anarysis_none_status = int(common_inifile.get('ANALYSIS_STATUS', 'none'))
        # �B���摜�̕�(mm)
        actual_image_width = int(common_inifile.get('IMAGE_SIZE', 'actual_image_width'))
        # �B���摜�̍���(mm)
        actual_image_height = float(common_inifile.get('IMAGE_SIZE', 'actual_image_height'))
        # �B���摜�̃I�[�o�[���b�v(mm)
        actual_image_overlap = int(common_inifile.get('IMAGE_SIZE', 'actual_image_overlap'))
        # �B���摜(���T�C�Y�摜)�̕�(pix)
        resize_image_width = int(common_inifile.get('IMAGE_SIZE', 'resize_image_width'))
        # �B���摜(���T�C�Y�摜)�̍���(pix)
        resize_image_height = int(common_inifile.get('IMAGE_SIZE', 'resize_image_height'))
        # �B���摜(���T�C�Y�摜)�̕�(pix)
        master_image_width = int(common_inifile.get('IMAGE_SIZE', 'master_image_width'))
        # �B���摜(���T�C�Y�摜)�̍���(pix)
        master_image_height = int(common_inifile.get('IMAGE_SIZE', 'master_image_height'))
        # �I�[�o�[���b�v����������1�B���摜�̕�(X��)�̒���[pix]
        nonoverlap_image_width_pix = resize_image_width * (
                actual_image_width - actual_image_overlap) / actual_image_width
        # �I�[�o�[���b�v����������1�B���摜�̍���(Y��)�̒���[pix]
        nonoverlap_image_height_pix = resize_image_height * (
                actual_image_height - actual_image_overlap) / actual_image_height
        # �I�[�o�[���b�v���̕�(X��)�̒���[pix]
        overlap_width_pix = resize_image_width * actual_image_overlap / actual_image_width
        # �I�[�o�[���b�v���̍���(Y��)�̒���[pix]
        overlap_height_pix = resize_image_height * actual_image_overlap / actual_image_height
        # �����Ώۃ��C���ԍ�
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s�@�\���N�����܂�' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
        # DB�ɐڑ�����
        result, conn, cur = register_ng_info_util.create_connection(logger)

        if result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B' % (app_id, app_name))
            pass
        else:
            logger.error('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
            sys.exit()
        
        count = 0

        while True:
            logger.debug('[%s:%s] �����Ώ۔������擾���J�n���܂��B' % (app_id, app_name))
            # �������e�[�u�����猟�������擾����
            result, fabric_info, conn, cur = select_processing_target(conn, cur, fabric_name, inspection_num, imaging_starttime, unit_num)

            if result:
                logger.debug('[%s:%s] �����Ώ۔������擾���I�����܂����B' % (app_id, app_name))
                logger.debug('[%s:%s] �����Ώ۔������ [%s]' % (app_id, app_name, fabric_info))
                pass
            else:
                logger.error('[%s:%s] �����Ώ۔������擾�����s���܂����B' % (app_id, app_name))
                conn.rollback()
                sys.exit()

            if len(fabric_info) != 0:
                inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

                logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                            (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))
                inspection_direction = fabric_info[0][0]

                logger.debug('[%s:%s] ���W�}�[�N���擾���J�n���܂��B' % (app_id, app_name))
                result, regimark_info, conn, cur = \
                    register_ng_info_util.select_regimark_info(conn, cur, fabric_name,
                                                               inspection_num, imaging_starttime, unit_num, logger)
                if result:
                    logger.debug('[%s:%s] ���W�}�[�N���擾���I�����܂����B' % (app_id, app_name))
                    logger.debug('[%s:%s] ���W�}�[�N��� [ %s ]' % (app_id, app_name, regimark_info))
                else:
                    logger.debug('[%s:%s] ���W�}�[�N���擾�����s���܂����B' % (app_id, app_name))
                    conn.rollback()
                    sys.exit()

                while True:
                    if len(regimark_info) == 0:
                        logger.info('[%s:%s] ���W�}�[�N��񂪑��݂��܂���B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                    (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        logger.debug('[%s:%s] �����Ώۉ摜���擾���J�n���܂��B' % (app_id, app_name))
                        result, ng_image_info, conn, cur = \
                            select_processing_target_image(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                           inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] �����Ώۉ摜���擾���I�����܂����B' % (app_id, app_name))
                        else:
                            logger.debug('[%s:%s] �����Ώۉ摜���擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] �㏈���I�������擾���J�n���܂��B' % (app_id, app_name))
                        result, imageprocess_endtime, conn, cur = select_imageprocess_time(conn, cur, fabric_name,
                                                                                           inspection_num,
                                                                                           imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] �㏈���I�������擾���I�����܂����B' % (app_id, app_name))
                            imageprocess_endtime = imageprocess_endtime[0]
                        else:
                            logger.debug('[%s:%s] �㏈���I�������擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_image_info) != 0 and imageprocess_endtime is not None:
                            logger.info('[%s:%s] NG�Ώۂ����݂��܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                            logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            logger.debug('[%s:%s] RAPID��͏��e�[�u����NG���o�^���J�n���܂��B' % (app_id, app_name))
                            # TODO 0�s�œo�^����irapid��͏��e�[�u���j�i�ق��̒l���������j
                            # TODO ���ڂ������ŁA�摜�͓���Ă�����H
                            result, conn, cur = update_rapid_analysis_all(conn, cur, fabric_name, inspection_num,
                                                                          anarysis_none_status, inspection_date,
                                                                          unit_num)
                            if result:
                                logger.debug('[%s:%s] RAPID��͏��e�[�u����NG���o�^���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] RAPID��͏��e�[�u����NG���o�^�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                            logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�̍X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                        elif len(ng_image_info) == 0 and imageprocess_endtime is not None:
                            logger.info('[%s:%s] NG�Ώۂƃ��W�}�[�N��񂪑��݂��܂���B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] �������X�e�[�^�X�X�V���J�n���܂��B' % (app_id, app_name))
                            # �������e�[�u�����猟�������擾����
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                                pass
                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                            logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�̍X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                        else:
                            logger.debug('[%s:%s] �㏈���I�������擾�����݂��܂���B' % (app_id, app_name))
                            time.sleep(sleep_time)
                            continue

                    else:
                        logger.debug('[%s:%s] �������X�e�[�^�X�X�V���J�n���܂��B' % (app_id, app_name))
                        # �������e�[�u�����猟�������擾����
                        result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                               fabric_ng_start_status, fabric_ng_start_column,
                                                               imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] �������X�e�[�^�X�X�V���I�����܂����B' % (app_id, app_name))
                            conn.commit()
                            pass
                        else:
                            logger.error('[%s:%s] �������X�e�[�^�X�X�V�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] �����Ώۉ摜���擾���J�n���܂��B' % (app_id, app_name))
                        result, ng_image_info, conn, cur = \
                            select_processing_target_image(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                           inspection_date, unit_num)

                        if result:
                            logger.debug('[%s:%s] �����Ώۉ摜���擾���I�����܂����B' % (app_id, app_name))
                        else:
                            logger.debug('[%s:%s] �����Ώۉ摜���擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] �㏈���I�������擾���J�n���܂��B' % (app_id, app_name))
                        result, imageprocess_endtime, conn, cur = select_imageprocess_time(conn, cur, fabric_name,
                                                                                           inspection_num,
                                                                                           imaging_starttime, unit_num)

                        if result:
                            logger.debug('[%s:%s] �㏈���I�������擾���I�����܂����B' % (app_id, app_name))
                            imageprocess_endtime = imageprocess_endtime[0]
                        else:
                            logger.debug('[%s:%s] �㏈���I�������擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if len(ng_image_info) != 0:
                            logger.debug('[%s:%s] �����Ώۉ摜��� %s' % (app_id, app_name, ng_image_info))
                            logger.debug('[%s:%s] �}�X�^���擾���J�n���܂��B' % (app_id, app_name))
                            result, mst_data, conn, cur = \
                                register_ng_info_util.select_product_master_info(conn, cur, product_name, logger)
                            if result:
                                logger.debug('[%s:%s] �}�X�^���擾���I�����܂����B' % (app_id, app_name))
                                logger.debug('[%s:%s] �}�X�^����� %s' % (app_id, app_name, mst_data))
                            else:
                                logger.debug('[%s:%s] �}�X�^���擾�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            for j in range(len(ng_image_info)):
                                logger.debug('[%s:%s] �s�ԍ�������J�n���܂��BNG��� %s' % (app_id, app_name, ng_image_info[j]))
                                result, line_info = \
                                    register_ng_info_util.specific_line_num(regimark_info, ng_image_info[j],
                                                                            inspection_direction, logger)

                                if result == 'error':
                                    logger.debug('[%s:%s] �Y���s�����݂��܂���B' % (app_id, app_name))
                                    result, conn, cur = \
                                        update_rapid_analysis(conn, cur, fabric_name, inspection_num, ng_image_info[j],
                                                              anarysis_none_status, inspection_date, unit_num)
                                    if result:
                                        logger.debug('[%s:%s] RAPID��͏��X�e�[�^�X(none)���X�V���܂����B' % (app_id, app_name))
                                        conn.commit()
                                        continue
                                    else:
                                        logger.debug('[%s:%s] RAPID��͏��X�e�[�^�X(none)�̍X�V�����s���܂����B' % (app_id, app_name))
                                        conn.rollback()
                                        sys.exit()
                                elif result:
                                    logger.debug('[%s:%s] �s�ԍ����肪�I�����܂����B' % (app_id, app_name))
                                else:
                                    logger.debug('[%s:%s] �s�ԍ����肪���s���܂����B' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] %s�s�ڂ̃��W�}�[�N�Ԓ���/���䗦�Z�o���J�n���܂��B���W�}�[�N��� [%s]' % (
                                    app_id, app_name, line_info[0][0], line_info))
                                result, regimark_length_ratio, regimark_width_ratio, conf_regimark_between_length_pix = \
                                    register_ng_info_util.calc_length_ratio(regimark_info, line_info,
                                                                            nonoverlap_image_width_pix,
                                                                            nonoverlap_image_height_pix,
                                                                            overlap_width_pix, overlap_height_pix,
                                                                            resize_image_height, resize_image_width,
                                                                            mst_data, master_image_width,
                                                                            master_image_height,
                                                                            actual_image_width, actual_image_height,
                                                                            inspection_direction, logger)
                                if result:
                                    logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦�Z�o���I�����܂����B' % (app_id, app_name))
                                    logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦 [����:%s ��:%s]' % (
                                        app_id, app_name, regimark_length_ratio, regimark_width_ratio))
                                else:
                                    logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦�Z�o�����s���܂����B' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG�ʒu������J�n���܂��B' % (app_id, app_name))
                                result, length_on_master, width_on_master, ng_face = register_ng_info_util.specific_ng_point(
                                    regimark_info, line_info, ng_image_info[j], nonoverlap_image_width_pix,
                                    nonoverlap_image_height_pix, overlap_width_pix, overlap_height_pix,
                                    resize_image_height, resize_image_width, regimark_length_ratio,
                                    regimark_width_ratio,
                                    mst_data, inspection_direction, master_image_width, master_image_height, logger)
                                if result:
                                    logger.debug('[%s:%s] NG�ʒu���肪�I�����܂����B' % (app_id, app_name))
                                    logger.debug('[%s:%s] NG�ʒu���� X���W = %s, Y���W = %s' % (
                                        app_id, app_name, length_on_master, width_on_master))
                                else:
                                    logger.debug('[%s:%s] NG�ʒu���肪���s���܂����B' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG�s�E�������J�n���܂��B' % (app_id, app_name))
                                result, judge_result, length_on_master, width_on_master = register_ng_info_util.specific_ng_line_colum(
                                    line_info, length_on_master, width_on_master, mst_data,
                                    conf_regimark_between_length_pix, inspection_direction, logger)

                                if result == True and judge_result == None:
                                    logger.debug('[%s:%s] NG�s�E����肪�I�����܂����B' % (app_id, app_name))
                                    logger.debug('[%s:%s] NG�s�E�񋫊E�l������J�n���܂��B' % (app_id, app_name))
                                    result, judge_result, length_on_master, width_on_master = register_ng_info_util.specific_ng_line_colum_border(
                                        line_info, length_on_master, width_on_master, mst_data,
                                        conf_regimark_between_length_pix, inspection_direction, logger)
                                    if result:
                                        logger.debug(
                                            '[%s:%s] NG�s�E�񋫊E�l���肪�I�����܂����B[�s,��] = %s' % (app_id, app_name, judge_result))
                                    else:
                                        logger.debug('[%s:%s] NG�s�E�񋫊E�l���肪���s���܂����B' % (app_id, app_name))
                                        sys.exit()

                                elif result == True and judge_result != None:
                                    logger.debug('[%s:%s] NG�s�E����肪�I�����܂����B[�s,��] = %s' % (app_id, app_name, judge_result))

                                else:
                                    logger.debug('[%s:%s] NG�s�E����肪���s���܂����B' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] ��_�����NG�����Z�o���J�n���܂��B' % (app_id, app_name))
                                result, ng_dist = register_ng_info_util.calc_distance_from_basepoint(
                                    length_on_master, width_on_master, judge_result, mst_data, master_image_width,
                                    master_image_height, logger)
                                if result:
                                    logger.debug('[%s:%s] ��_�����NG�����Z�o���I�����܂����B' % (app_id, app_name))
                                    logger.debug('[%s:%s] ��_�����NG���� X����:%s, Y����:%s' % (
                                        app_id, app_name, ng_dist[0], ng_dist[1]))
                                else:
                                    logger.debug('[%s:%s] ��_�����NG�����Z�o�����s���܂����B' % (app_id, app_name))
                                    sys.exit()

                                logger.debug('[%s:%s] NG���o�^���J�n���܂��B' % (app_id, app_name))
                                ng_line = judge_result[0]
                                ng_colum = judge_result[1]
                                master_point = str(round(length_on_master)) + ',' + str(round(width_on_master))
                                ng_distance_x = ng_dist[0]
                                ng_distance_y = ng_dist[1]
                                num = ng_image_info[j][0]
                                ng_file = ng_image_info[j][1]

                                result, conn, cur = register_ng_info_util.update_ng_info(
                                    conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point,
                                    ng_distance_x, ng_distance_y, num, ng_file, None, inspection_date, unit_num, logger)
                                if result:
                                    logger.debug('[%s:%s] NG���o�^���I�����܂����B' % (app_id, app_name))
                                    conn.commit()
                                else:
                                    logger.debug('[%s:%s] NG���o�^�����s���܂����B' % (app_id, app_name))
                                    sys.exit()

                            del ng_image_info
                        elif len(ng_image_info) != 0 and imageprocess_endtime != None:
                            logger.info('[%s:%s] �����Ώۉ摜��0���̌����ł��B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.info('[%s:%s] �S��NG�摜��NG�s�E��o�^���I�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���J�n���܂��B' % (app_id, app_name))
                            # �������X�e�[�^�X���擾����B
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()
                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�̍X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                        else:
                            logger.info('[%s:%s] �����Ώۉ摜������܂���B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            time.sleep(10)

                        logger.debug('[%s:%s] ��������������J�n���܂��B' % (app_id, app_name))
                        logger.debug('[%s:%s] �������}�[�L���O���������̎擾���J�n���܂��B' % (app_id, app_name))
                        # �������X�e�[�^�X���擾����B
                        result, fabric_marikingendtime, conn, cur = \
                            select_fabric_marking_endtime(conn, cur, fabric_name, inspection_num, imaging_starttime,
                                                          unit_num)
                        if result:
                            logger.debug('[%s:%s] �������}�[�L���O���������̎擾���I�����܂����B' % (app_id, app_name))
                        else:
                            logger.error('[%s:%s] �������}�[�L���O���������̎擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        logger.debug('[%s:%s] NG�s�E���񖢓o�^�����̎擾���J�n���܂��B' % (app_id, app_name))
                        # �[����E�}�X�L���O���茋�ʂ��擾����
                        result, processed_count, conn, cur = \
                            select_judged_ng_info_count(conn, cur, fabric_name, inspection_num, anarysis_ng_status,
                                                        inspection_date, unit_num)
                        if result:
                            logger.debug('[%s:%s] NG�s�E���񖢓o�^�����̎擾���I�����܂����B' % (app_id, app_name))
                        else:
                            logger.error('[%s:%s] NG�s�E���񖢓o�^�����̎擾�����s���܂����B' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        if (fabric_marikingendtime[0] == None) or (
                                (fabric_marikingendtime[0] != None) and (processed_count[0] != 0)):
                            logger.info('[%s:%s] NG�s�E�񖢓o�^��NG�摜�����݂��܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        else:
                            logger.info('[%s:%s] �S��NG�摜��NG�s�E��o�^���I�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                                        (app_id, app_name, fabric_name, inspection_num, inspection_date))
                            logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���J�n���܂��B' % (app_id, app_name))
                            # �������X�e�[�^�X���擾����B
                            result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                            if result:
                                logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���I�����܂����B' % (app_id, app_name))
                                conn.commit()

                            else:
                                logger.error('[%s:%s] �������X�e�[�^�X�̍X�V�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()

                            # �����Ώ۔�����񂪑��݂��Ȃ����߁ADB�ڑ���؂�A��莞�ԃX���[�v���Ă���A�Ď擾���s���B
                            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                            result = register_ng_info_util.close_connection(conn, cur, logger)

                            if result:
                                logger.debug('[%s:%s] DB�ڑ��̐ؒf���������܂����B' % (app_id, app_name))
                                result = True
                                return result
                            else:
                                logger.error('[%s:%s] DB�ڑ��̐ؒf�Ɏ��s���܂����B' % (app_id, app_name))
                                sys.exit()

            else:
                logger.info('[%s:%s] �����Ώ۔�����񂪂���܂���B' % (app_id, app_name))
                logger.debug('[%s:%s] %s�b�X���[�v���܂�' % (app_id, app_name, sleep_time))
                time.sleep(sleep_time)
                if count == 5:
                    logger.debug('[%s:%s] �������X�e�[�^�X�X�V���J�n���܂��B' % (app_id, app_name))
                    # �������e�[�u�����猟�������擾����
                    result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_start_status, fabric_ng_start_column,
                                                                   imaging_starttime, unit_num)

                    if result:
                        logger.debug('[%s:%s] �������X�e�[�^�X�X�V���I�����܂����B' % (app_id, app_name))
                        conn.commit()
                        pass
                    else:
                        logger.error('[%s:%s] �������X�e�[�^�X�X�V�����s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()
                    logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B' % (app_id, app_name))
                    result, conn, cur = update_fabric_info(conn, cur, fabric_name, inspection_num,
                                                                   fabric_ng_end_status, fabric_ng_end_column,
                                                                   imaging_starttime, unit_num)
                    if result:
                        logger.debug('[%s:%s] �������X�e�[�^�X�̍X�V���I�����܂����B' % (app_id, app_name))
                        conn.commit()
                        result = True
                        return result
                    else:
                        logger.error('[%s:%s] �������X�e�[�^�X�̍X�V�����s���܂����B' % (app_id, app_name))
                        conn.rollback()
                        sys.exit()
                else:
                    count += 1
                    continue


    except SystemExit:
        # sys.exit()���s���̗�O����
        result = False
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B', app_id, app_name)

    except:
        result = False
        logger.error('[%s:%s] %s�@�\�ŗ\�����Ȃ��G���[���������܂����B[%s]', app_id, app_name, app_name, traceback.format_exc())
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B', app_id, app_name)

    finally:
        if conn is not None:
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            register_ng_info_util.close_connection(conn, cur, logger)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass

    return result


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ

    main()
