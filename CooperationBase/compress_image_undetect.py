# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\310  NG�摜���k�E�]��
# ----------------------------------------

import configparser
import logging.config
import sys
import traceback
import error_util
import file_util

import compress_image_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image_undetect")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')

# ------------------------------------------------------------------------------------
# ������             �F�p�g���C�g�_��
#
# �����T�v           �F1.�p�g���C�g�_�����s���B
#
# ����               �F�o�̓p�X
#                     �o�̓t�@�C����
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def exec_patrite(file_name , logger, app_id, app_name):
    result = file_util.light_patlite(file_name, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.NG�摜���k�E�]�����s���B
#
# ����               �F�i��
#                    ����
#                    �����ԍ�
#                    �B���J�n����
#                    NG�摜��
#                   ���k�Ώ�NG�摜�p�X�i�����m�摜�t���O��"�����m�摜"�̏ꍇ�̂ݎw��B����ȊO��None���w�肷��j
#                   �����m�摜�t���O�i1:�����m�摜�ł���A����ȊO:�����m�摜�łȂ��j
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, imaging_starttime, image_path, target_ng_image_path):
    # �ϐ���`
    error_file_name = None
    result = False
    try:

        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �p�g���C�g�_���̃g���K�[�ƂȂ�t�@�C���p�X
        send_patrite_trigger_file_path = common_inifile.get('FILE_PATH', 'patlite_path')
        # �p�g���C�g�_���̃g���K�[�ƂȂ�t�@�C����
        send_parrite_file_name = inifile.get('FILE', 'send_parrite_file_name')

        # �v���O��������l�F�����m�摜
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))


        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### NG�摜���k�A���������ʒm�쐬�A�t�@�C���]��

        logger.debug('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]�����J�n���܂��B', app_id, app_name)
        tmp_result = compress_image_util.exec_compress_and_transfer(
        product_name, fabric_name, inspection_num, imaging_starttime,
        image_path, target_ng_image_path, undetected_image_flag_is_undetected, logger)

        if tmp_result:
            logger.debug('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]�����������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] NG�摜���k�A���������ʒm�쐬�A�t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
            sys.exit()

        logger.info("[%s:%s] %s�����͐���ɏI�����܂����B", app_id, app_name, app_name)
        result = True

    except SystemExit:
        # sys.exit()���s���̗�O����
        result = False
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        temp_result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if temp_result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))

    except:
        result = False
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        temp_result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if temp_result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))

    return result


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()