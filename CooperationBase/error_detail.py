# -*- coding: SJIS -*-
# ----------------------------------------
# �� �G���[���ʏ���
# ----------------------------------------


import psycopg2
import configparser
import traceback

import db_util

# �G���[�ݒ�t�@�C���Ǎ�
error_inifile = configparser.ConfigParser()
error_inifile.read('D:/CI/programs/config/error_config.ini', 'SJIS')
#  ���ʐݒ�t�@�C���Ǎ���
# ���ʐݒ�Ǎ�
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')


# ------------------------------------------------------------------------------------
# ������             �FDB��O
#
# �����T�v           �F1.DB�Ɋւ����O�������s���B
#                       2.�G���[�⏈���ɉ������߂�l��ԋp����B
#
# ����               �F�G���[���
#                       DB�Đڑ���
#                       ���K�[
#                       �@�\ID
#                       �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def db_exception(error_type, retry_num, logger, app_id, app_name):

    try:
        if 'OperationalError' in error_type or 'InterfaceError' in error_type:
            for i in range(retry_num):
                try:
                    logger.warning('[%s:%s] ��O�ڍ� : DB�ڑ��G���[���������܂����B' % (app_id, app_name)),
                    logger.warning('[%s:%s] [DB�ڑ��Ď��s%s���] DB�Ƃ̐ڑ����J�n���܂��B' % (app_id, app_name, i+1)),
                    conn, cur = db_util.base_create_connection()
                    logger.warning('[%s:%s] [DB�ڑ��Ď��s%s���] DB�Ƃ̐ڑ����������܂����B' % (app_id, app_name, i+1))
                except psycopg2.Error:
                    logger.warning('[%s:%s] �Đڑ����s�B�Đڑ����s���܂�' % (app_id, app_name))
                else:
                    logger.warning('[%s:%s] �Đڑ�����' % (app_id, app_name))
                    return True, conn, cur
            else:
                logger.error('[%s:%s] �Đڑ����s�B�ڑ��ݒ���������Ă�������' % (app_id, app_name))
                logger.error(str(traceback.format_exc()))
                return False
        else:
            logger.error('[%s:%s] ��O�ڍ� : DB�G���[���������܂����B' % (app_id, app_name))
            logger.error(str(traceback.format_exc()))
            return False
    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂���' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result


# ------------------------------------------------------------------------------------
# ������             �F�V�X�e����O
#
# �����T�v           �F1.�V�X�e���Ɋւ����O�������s���B
#                       2.�G���[�⏈���ɉ������߂�l��ԋp����B
#
# ����               �F�G���[���
#                       �l�b�g���[�N�p�X�G���[��
#                       �p�[�~�b�V�����G���[��
#                       ���K�[
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �p�[�~�b�V�����G���[��
# ------------------------------------------------------------------------------------
def system_exception(e, network_error_str, permission_error_str, logger, app_id, app_name):
    try:
        error_type = str(type(e))
        if network_error_str in str(e.args):
            logger.warning('[%s:%s] ��O�ڍ� : �l�b�g���[�N�p�X��������܂���B' % (app_id, app_name))
            logger.warning(str(traceback.format_exc()))
            return network_error_str

        elif permission_error_str in error_type:
            logger.warning('[%s:%s] ��O�ڍ� : �v���Z�X�̓t�@�C���ɃA�N�Z�X�ł��܂���B�ʂ̃v���Z�X���g�p���ł�' % (app_id, app_name))
            logger.warning(str(traceback.format_exc()))
            return permission_error_str
        else:
            logger.error('[%s:%s] ��O�ڍ� : �G���[���������܂����B' % (app_id, app_name))
            logger.error(str(traceback.format_exc()))
            return False
    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂���' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result


# ------------------------------------------------------------------------------------
# ������             �F��O����
#
# �����T�v           �F1.�e�@�\����A�g���ꂽ�G���[���̏ڍה�����s���B
#
# ����               �F�G���[���
#                       ���K�[
#                       �@�\ID
#                       �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def exception(e, logger, app_id, app_name):
    try:
        # �ݒ��`
        db_error = error_inifile.get('ERROR_INFO', 'db_error')
        network_error_str = error_inifile.get('ERROR_INFO', 'networkpath_error')
        permission_error_str = error_inifile.get('ERROR_INFO', 'permission_error')
        retry_num = int(common_inifile.get('ERROR_RETRY', 'connect_num'))
        error_type = str(type(e))

        if db_error in error_type:
            logger.error('[%s:%s] DB�֘A�G���[���������܂����B' % (app_id, app_name))
            tmp_result = db_exception(error_type, retry_num, logger, app_id, app_name)
            if type(tmp_result) == tuple:
                result = tmp_result[0]
                conn = tmp_result[1]
                cur = tmp_result[2]
                return result, conn, cur
            else:
                result = False
                return result
        else:
            logger.error('[%s:%s] �V�X�e���֘A�G���[���������܂����B' % (app_id, app_name))
            result = system_exception(e, network_error_str, permission_error_str, logger, app_id, app_name)
            return result
    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂���' % (app_id, app_name))
        logger.error(traceback.format_exc())
        result = False
        return result
