# -*- coding: SJIS -*-
# ----------------------------------------
# �� �^�p�@�\ �X�e�[�^�X���X�V
# ----------------------------------------

import configparser
import logging.config
import sys
import traceback

import db_util
import error_detail
import error_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_update_status.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/update_status_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


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
# ������             �F�������e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.�������e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�X�e�[�^�X
#                      ����
#                      �����ԍ�
#                      �B���J�n���iYYYY/MM/DD�`���j
#                      ���@
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info_status(status, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):

    fabric_imaging_starttime = inifile.get('COLUMN', 'fabric_imaging_starttime')

    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s ,imaging_endtime = now(), ' \
          'separateresize_starttime = (case when separateresize_starttime is Null then now() else separateresize_starttime END), '\
          'separateresize_endtime = (case when separateresize_endtime is Null then now() else separateresize_endtime END), '\
          'rapid_starttime = (case when rapid_starttime is Null then now() else rapid_starttime END), ' \
          'rapid_endtime = (case when rapid_endtime is Null then now() else rapid_endtime END), '\
          'imageprocessing_starttime = (case when imageprocessing_starttime is Null then now() else imageprocessing_starttime END), '\
          'imageprocessing_endtime = (case when imageprocessing_endtime is Null then now() else imageprocessing_endtime END), '\
          'ng_starttime = (case when ng_starttime is Null then now() else ng_starttime END), '\
          'ng_endtime = (case when ng_endtime is Null then now() else ng_endtime END), '\
          'ng_ziptrans_starttime = (case when ng_ziptrans_starttime is Null then now() else ng_ziptrans_starttime END), '\
          'ng_ziptrans_endtime = (case when ng_ziptrans_endtime is Null then now() else ng_ziptrans_endtime END) '\
          'where fabric_name = \'%s\' and inspection_num = %s and %s::date = \'%s\' and unit_num = \'%s\''  \
          % (status, fabric_name, inspection_num, fabric_imaging_starttime, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur

# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�����X�e�[�^�X
#                     �Ŕ�
#                     �����ԍ�
#                      �B���J�n�����iYYYY/MM/DD�`���j
#                      ���@
#                     �J�[�\���I�u�W�F�N�g
#                     �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_processing_status(status, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):

    processing_imaging_starttime = inifile.get('COLUMN', 'processing_imaging_starttime')
    ### �N�G�����쐬����
    sql = 'update processing_status set status = %s ' \
          '  where fabric_name = \'%s\' and inspection_num = %s and %s::date = \'%s\' and unit_num = \'%s\''  \
          % (status, fabric_name, inspection_num, processing_imaging_starttime, imaging_starttime, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.RAPID��͏��X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �FRAPID��͌���
#                     �[���茋��
#                     �}�X�L���O���茋��
#                     �Ŕ�
#                     �����ԍ�
#                      �B���J�n�����iYYYY/MM/DD�`���j
#                      ���@
#                     �J�[�\���I�u�W�F�N�g
#                     �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_rapid_analysis_info(rapid_result, edge_result, masking_result, fabric_name, inspection_num, imaging_starttime, unit_num, cur, conn):
    target_date = imaging_starttime.replace('/', '')
    ### �N�G�����쐬����
    sql = 'update "rapid_%s_%s_%s" set rapid_result = %s, edge_result = %s, masking_result = %s ' \
          '  where fabric_name = \'%s\' and inspection_num = %s and unit_num = \'%s\''  \
          % (fabric_name, inspection_num, target_date, rapid_result, edge_result, masking_result, fabric_name, inspection_num, unit_num)

    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

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
# �֐���             �F���t������ϊ�
#
# �����T�v           �F1.���͂��ꂽ���t��������AYYYY/MM/DD�`���̕�����ɕϊ�����B
#
# ����               �F���t������
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    ���t������iYYYY/MM/DD�j
# ------------------------------------------------------------------------------------
def conv_date(imaging_starttime):
    result = False
    conv_date = None
    dates = []
    try:
        # YYYY-MM-DD��YYYY/MM/DD�ɕϊ��A���A��������
        dates = imaging_starttime.replace('-', '/').split('/')
        if len(dates) == 3:
            # YYYY��MM��YY�������ł����ꍇ
            yyyy = dates[0]
            mm = dates[1]
            dd = dates[2]
        elif len(dates) == 1 and len(imaging_starttime) == 8:
            # YYYYMMDD�`���̏ꍇ
            yyyy = imaging_starttime[:4]
            mm = imaging_starttime[4:6]
            dd = imaging_starttime[6:]
        # ���t������iYYYY/MM/DD�j�ɕϊ�����B
        conv_date = '%04d/%02d/%02d' % (int(yyyy), int(mm), int(dd))
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
    return result, conv_date

# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�X�e�[�^�X���X�V�@�\���s���B
#
# ����               �F����
#                    �����ԍ�
#                    ������
#                    ���@�i�����̂݁j
#
# �߂�l             �F�������ʁi0:����A1:���s�j
# ------------------------------------------------------------------------------------
def main(fabric_name, inspection_num, inspection_date, unit_num):
    # �ϐ���`
    result = False
    error_file_name = None
    conn = None
    cur = None

    try:
        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �X�V�X�e�[�^�X�F�������e�[�u��.�X�e�[�^�X
        fabric_status = inifile.get('FABRIC_INFO', 'status')
        # �X�V�X�e�[�^�X�F�����X�e�[�^�X�e�[�u��.�X�e�[�^�X
        processing_status = inifile.get('PROCESSING_STATUS', 'status')
        # �X�V�X�e�[�^�X�FRAPID��͏��e�[�u��.RAPID��͌���
        rapid_result = inifile.get('RAPID_ANALYSIS_INFO', 'rapid_result')
        # �X�V�X�e�[�^�X�FRAPID��͏��e�[�u��.�[���茋��
        edge_result = inifile.get('RAPID_ANALYSIS_INFO', 'edge_result')
        # �X�V�X�e�[�^�X�FRAPID��͏��e�[�u��.�}�X�L���O���茋��
        masking_result = inifile.get('RAPID_ANALYSIS_INFO', 'masking_result')
        # ���@�̐ړ���
        unit_prefix = inifile.get('UNIT_INFO', 'unit_prefix')

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        # ���t�`���ϊ�
        logger.debug('[%s:%s] ���t�`���ϊ����J�n���܂��B', app_id, app_name)
        tmp_result, format_inspection_date = conv_date(inspection_date)
        if tmp_result:
            logger.debug('[%s:%s] ���t�`���ϊ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] ���t�`���ϊ��ɃG���[���������܂����B', app_id, app_name)
            sys.exit()

        # ���@
        format_unit_num = unit_prefix + str(unit_num)

        ### �e��X�e�[�^�X�X�V����
        logger.info('[%s:%s] %s�������J�n���܂��B:[����,�����ԍ�,������,���@]=[%s, %s, %s, %s]',
                    app_id, app_name, app_name, fabric_name, inspection_num, format_inspection_date, format_unit_num)


        # DB���ʏ������Ăяo���āADB�ڑ����s��
        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
        tmp_result, conn, cur = create_connection()

        if tmp_result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
            sys.exit()

        # �������e�[�u���̃X�e�[�^�X�X�V
        logger.debug('[%s:%s] �������e�[�u���̍X�V���J�n���܂��B', app_id, app_name)
        tmp_result, conn, cur = update_fabric_info_status(fabric_status, fabric_name, inspection_num,
                                                          format_inspection_date, format_unit_num, cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] �������e�[�u���̍X�V���I�����܂����B', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] �������e�[�u���̍X�V�Ɏ��s���܂����B', app_id, app_name)
            conn.rollback()
            sys.exit()

        # �����X�e�[�^�X�e�[�u���̃X�e�[�^�X�X�V
        logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V���J�n���܂��B', app_id, app_name)
        tmp_result, conn, cur = update_processing_status(processing_status, fabric_name, inspection_num,
                                                         format_inspection_date, format_unit_num, cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V���I�����܂����B', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V�Ɏ��s���܂����B', app_id, app_name)
            conn.rollback()

        # RAPID��͏��e�[�u���̃X�e�[�^�X�X�V
        logger.debug('[%s:%s] RAPID��͏��e�[�u���̍X�V���J�n���܂��B', app_id, app_name)
        tmp_result, conn, cur = update_rapid_analysis_info(rapid_result, edge_result, masking_result, fabric_name,
                                                           inspection_num, format_inspection_date, format_unit_num,
                                                           cur, conn)

        if tmp_result:
            logger.debug('[%s:%s] RAPID��͏��e�[�u���̍X�V���I�����܂����B', app_id, app_name)
            conn.commit()
        else:
            logger.error('[%s:%s] RAPID��͏��e�[�u���̍X�V�Ɏ��s���܂����B', app_id, app_name)
            conn.rollback()

        conn.commit()
        logger.info("[%s:%s] %s�����͐���ɏI�����܂����B", app_id, app_name, app_name)

        result = True

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)

    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))

    finally:
        if conn is not None:
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass

    # �߂�l�ݒ�
    # �ďo���i�o�b�`�v���O�������j�Ŗ߂�l����iERRORLEVEL�j����ۂ̖߂�l��ݒ肷��B
    if result:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    fabric_name = None
    inspection_num = None
    inspection_date = None
    unit_num = None
    args = sys.argv
    if len(args) > 4:
        fabric_name = args[1]
        inspection_num = args[2]
        inspection_date = args[3]
        unit_num = args[4]
    main(fabric_name, inspection_num, inspection_date, unit_num)
