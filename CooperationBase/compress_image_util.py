# -*- coding: SJIS -*-
# ----------------------------------------
# ���@�\310  NG�摜���k�E�]���@�\���ʋ@�\
# ----------------------------------------
import logging.config
import os
import shutil
import re
from pathlib import Path

import error_detail
import configparser
import file_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_compress_image.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �F�t�H���_���݃`�F�b�N
#
# �����T�v           �F1.�t�H���_�����݂��邩�`�F�b�N����B
#                    2.�t�H���_�����݂��Ȃ��ꍇ�͍쐬����B
#
# ����               �F�t�H���_�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path, logger):
    logger.debug('[%s:%s] �t�H���_���쐬���܂��B�t�H���_���F[%s]',
                 app_id, app_name, target_path)
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �FNG�摜���k�E�]���@�\���s
#
# �����T�v           �F1.NG�摜���k�E�]�����s���B
#
# ����               �F �i��
#                      ����
#                      �����ԍ�
#                      �B���J�n����
#                      NG�摜���[�g�p�X
#                      ���k�Ώ�NG�摜�p�X�i�����m�摜�t���O��"�����m�摜"�̏ꍇ�̂ݎw��B����ȊO��None���w�肷��j
#                      �����m�摜�t���O�i1:�����m�摜�ł���A����ȊO:�����m�摜�łȂ��j
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def exec_compress_and_transfer(
        product_name, fabric_name, inspection_num, imaging_starttime,
        image_root_path,
        archive_ng_image_file_path,
        undetected_image_flag, logger):
    result = False
    try:
        ### �ݒ�t�@�C������̒l�擾
        # �A�g��Ղ̃��[�g�f�B���N�g��
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # ���kNG�摜���M��z�X�g��
        send_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        send_hostname = re.split(',', send_hostname)[0]
        # ���kNG�摜���M��p�X
        send_ng_image_file_path = inifile.get('PATH', 'send_ng_image_file_path')
        send_ng_image_file_path = '\\\\' + send_hostname + '\\' + send_ng_image_file_path
        # ���������ʒm���M��p�X
        send_inspection_file_path = inifile.get('PATH', 'send_inspection_file_path')
        send_inspection_file_path = '\\\\' + send_hostname + '\\' + send_inspection_file_path + '\\'
        # �v���O��������l�F�����m�摜
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))


        # NG�摜���k�E�]���@�\�ȊO������s���ꂽ�ۂ́A�J�n���O���b�Z�[�W��\������
        if undetected_image_flag == undetected_image_flag_is_undetected:
            logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]',
                        app_id, app_name, app_name, fabric_name, inspection_num, imaging_starttime)
        else:
            pass

        ### NG�摜���k
        logger.debug('[%s:%s] NG�摜���k���J�n���܂��B', app_id, app_name)
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # �����m�摜�̏ꍇ
            # ���ۊm�F�E����o�^���ւ̃t�@�C�����M�p�X�i���������ʒm�j
            undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
            undetected_image_file_path = '\\\\' + send_hostname + '\\' + undetected_image_file_path
            tmp_result, ng_image_zip_file_path = ng_image_compress_undetected_image(
                archive_ng_image_file_path, image_root_path, logger)

        else:

            tmp_result, ng_image_zip_file_path = ng_image_compress(image_root_path,
                                                                   imaging_starttime, product_name,
                                                                   fabric_name, inspection_num, logger)
        if tmp_result:
            if ng_image_zip_file_path:
                logger.debug('[%s:%s] NG�摜���k���I�����܂����B', app_id, app_name)
            else:
                logger.debug('[%s:%s] NG�摜�����݂��܂���ł����B', app_id, app_name)
                result = True
                return result
        else:
            logger.error('[%s:%s] NG�摜���k�Ɏ��s���܂����B', app_id, app_name)
            return result

        ### ���������ʒm�쐬
        logger.debug('[%s:%s] ���������ʒm�쐬���J�n���܂��B', app_id, app_name)
        tmp_result, inspection_file_path, undetected_image_file_path = \
            make_inspection_completion_notification(
                undetected_image_flag, product_name, fabric_name, inspection_num, imaging_starttime, rk_root_path,
                ng_image_zip_file_path, logger)

        if tmp_result:
            logger.debug('[%s:%s] ���������ʒm�쐬���I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] ���������ʒm�쐬�Ɏ��s���܂����B', app_id, app_name)
            return result

        ### �t�@�C���]��
        logger.debug('[%s:%s] �t�@�C���]�����J�n���܂��B', app_id, app_name)
        send_undetected_image_file_path = send_inspection_file_path
        tmp_result = send_file(undetected_image_flag, ng_image_zip_file_path, inspection_file_path,
                               undetected_image_file_path, send_ng_image_file_path, send_inspection_file_path,
                               send_undetected_image_file_path, logger)

        if tmp_result:
            logger.debug('[%s:%s] �t�@�C���]�����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
            return result

        # NG�摜���k�E�]���@�\�ȊO������s���ꂽ�ۂ́A�I�����O���b�Z�[�W��\������
        if undetected_image_flag == undetected_image_flag_is_undetected:
            logger.info('[%s:%s] %s�����͐���ɏI�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]',
                        app_id, app_name, app_name, fabric_name, inspection_num, imaging_starttime)
        else:
            pass

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �FNG�摜���k
#
# �����T�v           �F1.NG�摜���k���s���B
#
# ����               �F NG�摜���[�g�p�X
#                      �B���J�n����
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    NG�摜���k�t�@�C���p�X
#
# ------------------------------------------------------------------------------------
def ng_image_compress(image_root_path, imaging_starttime, product_name, fabric_name, inspection_num, logger):
    result = False
    ng_image_zip_file_path = None
    try:
        ### �ݒ�t�@�C������̒l�擾
        # �}�[�L���ONG�摜���̐ړ���
        marking_ng_image_file_name_prefix = inifile.get('FILE', 'marking_ng_image_file_name_prefix')
        # �摜�t�@�C���̊g���q
        extension = common_inifile.get('FILE_PATTERN', 'image_file')

        # NG�摜�t�@�C�����p�^�[��
        ng_image_file_name_pattern = marking_ng_image_file_name_prefix + extension

        ### NG�摜���k
        # �Q�Ɛ�p�X�\���͈ȉ���z�肷��B
        # �u(�B���J�n����:YYYYMMDD�`��)_(�i��)_(����)_(�����ԍ�)�v
        path_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num)
        ng_image_path = image_root_path + '\\' \
                        + path_name + '\\'

        tmp_file_list = exists_dir(ng_image_path, logger)
        if tmp_file_list:
            logger.debug('[%s:%s] NG�摜�t�H���_���݃`�F�b�N���I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] NG�摜�t�H���_���݃`�F�b�N�Ɏ��s���܂����B', app_id, app_name)

        logger.debug('[%s:%s] NG�摜�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
        tmp_result, ng_image_files = get_file(ng_image_path, ng_image_file_name_pattern, logger)

        if tmp_result:
            logger.debug('[%s:%s] NG�摜�t�@�C���̊m�F���������܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] NG�摜�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
            return result, ng_image_zip_file_path

        logger.debug('[%s:%s] NG�摜�t�@�C��:[%s]', app_id, app_name, ng_image_files)

        # NG�摜�����݂���ꍇ�ɂ́ANG�摜�t�H���_�����k����B
        if ng_image_files:
            zip_path = image_root_path
            # �t�@�C�����́A(�B���J�n����:YYYYMMDD�`��)_����_�����ԍ�.zip
            zip_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num)
            ng_image_zip_file_path = zip_path + '\\' + zip_file_name
            shutil.make_archive(ng_image_zip_file_path, 'zip', root_dir=image_root_path, base_dir=path_name)
            # �߂�l�iNG�摜���k�t�@�C���p�X�j�ɁA�g���q��t�^����B
            ng_image_zip_file_path = ng_image_zip_file_path + '.zip'
        else:
            zip_path = image_root_path
            # �t�@�C�����́A(�B���J�n����:YYYYMMDD�`��)_����_�����ԍ�.zip
            zip_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(
                inspection_num) + '.zip'
            Path(zip_path + '\\' + zip_file_name).touch()
            ng_image_zip_file_path = zip_path + '\\' + zip_file_name

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, ng_image_zip_file_path


# ------------------------------------------------------------------------------------
# ������             �FNG�摜���k�i�����m�摜�o�^�p�j
#
# �����T�v           �F1.NG�摜���k���s���B
#
# ����               �F ���k�Ώ�NG�摜�p�X
#                      �o�̓p�X
#                      ����
#                      �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    NG�摜���k�t�@�C���p�X
#
# ------------------------------------------------------------------------------------
def ng_image_compress_undetected_image(archive_ng_image_file_path, output_path, logger):
    result = False
    ng_image_zip_file_path = None
    try:
        root_dir = os.path.dirname(archive_ng_image_file_path)
        base_dir = os.path.basename(archive_ng_image_file_path)

        # NG�摜�t�H���_�����k����B
        zip_path = output_path
        # �t�@�C�����́A�����m�摜�t�@�C�����i���t�H���_���Ɠ��ꖼ�j.zip
        zip_file_name = base_dir
        ng_image_zip_file_path = output_path + '\\' + zip_file_name
        shutil.make_archive(ng_image_zip_file_path, 'zip', root_dir=root_dir, base_dir=base_dir)
        # �߂�l�iNG�摜���k�t�@�C���p�X�j�ɁA�g���q��t�^����B
        ng_image_zip_file_path = ng_image_zip_file_path + '.zip'

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, ng_image_zip_file_path


# ------------------------------------------------------------------------------------
# ������             �F���������ʒm�쐬
#
# �����T�v           �F1.���������ʒm�쐬���s���B
#
# ����               �F �����m�摜�t���O�i1:�����m�摜�ł���A����ȊO:�����m�摜�łȂ��j
#                      �i��
#                      ����
#                      �����ԍ�
#                      �A�g��Ղ̃��[�g�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    ���������ʒm�t�@�C���p�X
#                    �����m�摜�o�^�����t�@�C���p�X�i�����m�摜�t���O���u1:�v�̏ꍇ�̂ݕԂ��A����ȊO��None��Ԃ��j
#
# ------------------------------------------------------------------------------------
def make_inspection_completion_notification(
        undetected_image_flag, product_name, fabric_name, inspection_num, imaging_starttime, rk_root_path,
        archive_ng_image_file_path, logger):
    result = False
    result_inspection_file_path = None
    result_undetected_image_file_path = None

    try:
        ### �ݒ�t�@�C������̒l�擾
        # �����m�摜�t���O:�����m�摜�ł���
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))
        # ���������ʒm�t�@�C���p�X
        undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
        # �����m�摜�o�^�����t�@�C����
        undetected_image_name_suffix = inifile.get('FILE', 'undetected_image_name_suffix')
        base_dir = os.path.basename(archive_ng_image_file_path)

        # �����m�摜������s���B
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # �����m�摜�o�^�����t�@�C�����쐬����B
            # �t�@�C�����́A�����m�摜�t�@�C����.txt
            base_dir = re.split('\.', base_dir)[0]
            tmp_file_name = base_dir + '.txt'
            tmp_path_path = rk_root_path + '\\' + tmp_file_name
            Path(tmp_path_path).touch()
            result_undetected_image_file_path = tmp_path_path
            result_inspection_file_path = None

        else:
            # ���������ʒm���쐬����B
            tmp_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(
                inspection_num) + '.txt'
            tmp_path_path = rk_root_path + '\\' + tmp_file_name
            Path(tmp_path_path).touch()
            result_inspection_file_path = tmp_path_path
            result_undetected_image_file_path = None

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, result_inspection_file_path, result_undetected_image_file_path


# ------------------------------------------------------------------------------------
# ������             �F�t�@�C���]��
#
# �����T�v           �F1.NG�摜���k�t�@�C���Ə��������ʒm���A���ۊm�F�E����o�^���֓]������B
#
# ����               �F �����m�摜�t���O�i1:�����m�摜�ł���A����ȊO:�����m�摜�łȂ��j
#                      NG�摜�A�[�J�C�u�t�@�C���p�X
#                      ���������ʒm�t�@�C���p�X
#                      �����m�摜�o�^�����t�@�C���p�X
#                      ���M���NG�摜�A�[�J�C�u�t�@�C���p�X
#                      ���M��̌��������ʒm�t�@�C���p�X
#                      ���M��̖����m�摜�o�^�����t�@�C���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def send_file(
        undetected_image_flag,
        ng_image_archive_file_path, inspection_file_path, undetected_image_file_path,
        send_ng_image_file_path, send_inspection_file_path, send_undetected_image_file_path, logger):
    result = False
    try:
        ### �ݒ�t�@�C������̒l�擾
        # �����m�摜�t���O:�����m�摜�ł���
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))

        ### ���������ʒm���A���ۊm�F�E����o�^���֓]������B
        # NG�摜
        logger.debug('[%s:%s] NG�摜�̃t�@�C���]�����J�n���܂��B', app_id, app_name)
        tmp_result = file_util.move_file(ng_image_archive_file_path, send_ng_image_file_path, logger, app_id, app_name)

        if tmp_result:
            logger.debug('[%s:%s] NG�摜�̃t�@�C���]�����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] NG�摜�̃t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
            return result

        # �����m�摜�o�^����
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # �����m�摜�̏ꍇ
            logger.debug('[%s:%s] �����m�摜�o�^�����̃t�@�C���]�����J�n���܂��B', app_id, app_name)
            tmp_result = file_util.move_file(undetected_image_file_path, send_undetected_image_file_path, logger,
                                             app_id,
                                             app_name)

            if tmp_result:
                logger.debug('[%s:%s] �����m�摜�o�^�����̃t�@�C���]�����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] �����m�摜�o�^�����̃t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
                return result

        else:
            # ���������ʒm
            logger.debug('[%s:%s] ���������ʒm�̃t�@�C���]�����J�n���܂��B', app_id, app_name)
            tmp_result = file_util.move_file(inspection_file_path, send_inspection_file_path, logger, app_id, app_name)

            if tmp_result:
                logger.debug('[%s:%s] ���������ʒm�̃t�@�C���]�����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] ���������ʒm�̃t�@�C���]���Ɏ��s���܂����B', app_id, app_name)
                return result

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�t�@�C���擾
#
# �����T�v           �F1.�B���摜�t�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F�B���摜�t�@�C���i�[�t�H���_�p�X
#                      �B���摜�t�@�C��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B���摜�t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name, logger):
    result = False
    file_list = None

    try:
        logger.debug('[%s:%s] �B���摜�t�@�C���i�[�t�H���_�p�X=[%s]', app_id, app_name, file_path)

        # ���ʊ֐��ŎB���摜�t�@�C���i�[�t�H���_�����擾����
        file_list = None
        tmp_result, file_list = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result:
            # ������
            pass
        else:
            # ���s��
            logger.error("[%s:%s] �B���摜�t�@�C���i�[�t�H���_�ɃA�N�Z�X�o���܂���B", app_id, app_name)
            return result, file_list

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, file_list
