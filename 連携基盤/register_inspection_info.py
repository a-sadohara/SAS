# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\301  �������o�^�@�\
# ----------------------------------------
import configparser
import logging.config
import sys
import time
import traceback

import db_util
import error_util

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
          'inspection_info_header.unit_num = \'%s\' and fabric_info.fabric_name is null ' \
          'order by start_datetime asc' % unit_num

    logger.debug('[%s:%s] �������擾SQL %s' % (app_id, app_name, sql))

    ### �������e�[�u������f�[�^�擾
    result, select_result, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)
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
                        unit_no = inspection_info[i][4]

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
