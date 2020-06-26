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
import error_util
import file_util
import error_detail

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
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur

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
    result, conn, cur = db_util.create_table(conn, cur, fabric_name, inspection_num, inspection_date,
                                             logger, app_id, app_name)
    return result, conn, cur

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
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur

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
    sql = 'select product_name, fabric_name, inspection_num, imaging_endtime,imaging_starttime from fabric_info where unit_num = \'%s\' ' \
          'and imaging_starttime < \'%s\' order by imaging_starttime desc' % (unit_num, starttime)

    logger.debug('[%s:%s] �O�������擾SQL %s' % (app_id, app_name, sql))

    ### �������e�[�u������f�[�^�擾
    result, select_result, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


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
    ### �N�G�����쐬����
    sql = 'insert into fabric_info (product_name, fabric_name, inspection_num, imaging_starttime, status, unit_num) ' \
          'values (\'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' % (product_name, fabric_name, inspection_num,
                                                                   starttime, status, unit_no)

    logger.debug('[%s:%s] �������o�^SQL %s' % (app_id, app_name, sql))
    ### �������e�[�u���փf�[�^�o�^
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur

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
def output_dummy_file(product_name, fabric_name, inspection_num, inspection_date):
    result = False
    output_file_path = common_inifile.get('FILE_PATH', 'input_path')
    file_extension = inifile.get('PATH', 'file_extension')

    try:
        scaninfo_path = inifile.get('PATH', 'scaninfo_path')
        scaninfo_prefix = inifile.get('PATH', 'scaninfo_prefix')
        regimark_path = inifile.get('PATH', 'regimark_path')
        regimark_prefix = inifile.get('PATH', 'regimark_prefix')
    
        scaninfo_file_name = scaninfo_prefix +  "_" + product_name + "_" + fabric_name + "_" + str(inspection_num) + "_" + inspection_date + file_extension
        regimark_file_name = regimark_prefix +  "_" + product_name + "_" + fabric_name + "_" + str(inspection_num) + "_" + inspection_date + "_"

        for i in range(0,2):
            for j in range(0,2):
                csv_file = output_file_path + "\\" + regimark_path + "\\" + regimark_file_name + str(i + 1) + "_" + str(j + 1) + file_extension
                with codecs.open(csv_file, "w", "SJIS") as f:
                    f.write("�B���t�@�C����,���,���W��,���W��")
                    f.write("\r\n")
    
        csv_file = output_file_path + "\\" + scaninfo_path + "\\" + scaninfo_file_name
        with codecs.open(csv_file, "w", "SJIS") as f:
            f.write("�J�����䐔,�J����1��̎B������,���B������")
            f.write("\r\n")
            f.write("54,0,0")
            f.write("\r\n")

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result



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
    try:

        # �ݒ�t�@�C������擾
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        imaging_status = int(common_inifile.get('FABRIC_STATUS', 'imaging'))
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        unit_num = common_inifile.get('UNIT_INFO', 'unit_num')

        logger.info('[%s:%s] %s�@�\���N�����܂�' % (app_id, app_name, app_name))
        ###DB�ڑ����s��
        # �����ڑ����s�����ꍇ�́A�ēx�ڑ��������B
        while True:

            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
            # DB�ɐڑ�����
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B' % (app_id, app_name))
                pass
            else:
                logger.error('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
                sys.exit()

            while True:

                logger.debug('[%s:%s] �������擾���J�n���܂��B' % (app_id, app_name))
                # �������e�[�u�����猟�������擾����
                result, inspection_info, conn, cur = select_inspection_data(conn, cur, unit_num)

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

                        for i in range(5):
                            logger.debug('[%s:%s] �O�������擾���J�n���܂��B' % (app_id, app_name))
                            # �������e�[�u������O���������擾����
                            result, before_inspection_info, conn, cur = select_before_inspection_data(conn, cur, starttime, unit_num)

                            if result:
                                logger.debug('[%s:%s] �O�������擾���I�����܂����B' % (app_id, app_name))
                                before_product_name = before_inspection_info[0]
                                before_fabric_name = before_inspection_info[1]
                                before_inspection_num = before_inspection_info[2]
                                before_starttime = before_inspection_info[4]
                                before_inspection_date = str(before_starttime.strftime('%Y%m%d'))
                                                                
                                logger.debug('[%s:%s] �O������� [�i��=%s] [����=%s] [�����ԍ�=%s]' % (app_id, app_name, before_product_name, before_fabric_name, before_inspection_num))
                                if before_inspection_info[3] != None:
                                    break
                                else:
                                    logger.info('[%s:%s] �O�����̌����������������݂��܂���B�Ċm�F���܂��B' % (app_id, app_name))
                                    time.sleep(sleep_time)
                                    continue
                            else:
                                logger.error('[%s:%s] �O�������擾�����s���܂����B' % (app_id, app_name))
                                conn.rollback()
                                sys.exit()
                            
                        if before_inspection_info[3] == None:
                            logger.error('[%s:%s] �O�������������Ă��܂���B�O������� [�i��=%s] [����=%s] [�����ԍ�=%s]' % (app_id, app_name, before_product_name, before_fabric_name, before_inspection_num))

                            error_file_path = common_inifile.get('ERROR_FILE', 'path')
                            Path(error_file_path + '\\' +  error_file_name).touch()

                            logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���J�n���܂��B [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                                        % (app_id, app_name, before_product_name, before_fabric_name, before_inspection_num, before_inspection_date))
                            tmp_result = output_dummy_file(before_product_name, before_fabric_name, before_inspection_num, before_inspection_date)
                            if tmp_result:
                                logger.info('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j���I�����܂����B [�i��, ����, �����ԍ�, �������t]=[%s, %s, %s, %s]'
                                        % (app_id, app_name, before_product_name, before_fabric_name, before_inspection_num, before_inspection_date))
                                time.sleep(sleep_time * 2)
                                break
                            else:
                                logger.error('[%s:%s] ���J�o���i�_�~�[�t�@�C���o�́j�Ɏ��s���܂����B ' % (app_id, app_name))
                                sys.exit()
                        else:
                            pass

                        inspection_date = str(starttime.strftime('%Y%m%d'))

                        logger.debug('[%s:%s] ������� [�i��=%s] [����=%s] [�����ԍ�=%s]' %
                                     (app_id, app_name, product_name, fabric_name, inspection_num))

                        logger.debug('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�]=[%s, %s]' %
                                     (app_id, app_name, app_name, fabric_name, inspection_num))

                        

                        # �������𔽕����e�[�u���ɓo�^����
                        result, conn, cur = insert_fabric_info(conn, cur, product_name, fabric_name, inspection_num,
                                                               starttime, imaging_status, unit_no)
                        if result:
                            conn.commit()
                            logger.info('[%s:%s] �������o�^���I�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]'
                                        % (app_id, app_name, fabric_name, inspection_num, inspection_date))

                        else:
                            logger.error('[%s:%s] �������o�^�Ɏ��s���܂����B ' % (app_id, app_name))
                            conn.rollback()
                            sys.exit()

                        # RAPID��͏��e�[�u�����쐬����
                        logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���J�n���܂��B' % (app_id, app_name))
                        result, conn, cur = create_rapid_table(conn, cur, fabric_name, inspection_num, inspection_date)
                        if result:
                            logger.debug('[%s:%s] RAPID��͏��e�[�u���쐬���I�����܂����B' % (app_id, app_name))
                            conn.commit()
                        else:
                            logger.error('[%s:%s] RAPID��͏��e�[�u���쐬�����s���܂����B '
                                     '[����, �����ԍ�, �������t]=[%s, %s, %s]'
                                     % (app_id, app_name, fabric_name, inspection_num, inspection_date))
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
