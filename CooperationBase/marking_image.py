# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\308  �摜�}�[�L���O
# ----------------------------------------

import traceback
import configparser
from PIL import Image, ImageDraw, ImageFile
import datetime
import os
import logging.config

import db_util
import error_detail
import file_util

#  ���ʐݒ�t�@�C���Ǎ���
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
#  �摜�}�[�L���O�ݒ�t�@�C���Ǎ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/marking_image_config.ini', 'SJIS')

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_marking_image.conf")
logger = logging.getLogger("marking_image")

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�X�V
#
# �����T�v           �F1.�摜�}�[�L���O�����J�n�E�������ɏ����X�e�[�^�X���X�V����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ����ID
#                      �X�V�J������
#                      �X�V����
#                      RAPID�T�[�o�[�z�X�g��
#                      �X�e�[�^�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_processing_status(conn, cur, fabric_name, inspection_num, processing_id, column_name, time,
                             rapid_host_name, status, imaging_starttime, unit_num):
    # �N�G�����쐬����
    sql = 'UPDATE processing_status SET status=%s, %s =\'%s\' ' \
          'WHERE fabric_name=\'%s\' AND inspection_num = %s AND processing_id = %s AND rapid_host_name = \'%s\' ' \
          'and imaging_starttime = \'%s\' and unit_num = \'%s\'' % \
          (status, column_name, time, fabric_name, inspection_num, processing_id, rapid_host_name, imaging_starttime,
           unit_num)

    logger.debug('[%s:%s] �����X�e�[�^�X�X�VSQL %s' % (app_id, app_name, sql))
    # �����X�e�[�^�X�i���T�C�Y�����j���X�V����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�}�[�L���O����
#
# �����T�v           �F1.NG�摜��NG�ӏ��Ƀ}�[�L���O���s���B
#
# ����               �FNG�E�}�[�L���O�摜�p�X
#                      ���ʃt�@�C���Ǎ��f�[�^
#                      �}�[�L���O�J���[
#                      �}�[�L���O��
#                      �}�[�L���O�摜��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def marking_process(output_path, masking_result, marking_color, line_width):
    result = False
    image = None
    try:

        # �摜���J��
        image = Image.open(output_path)

        # ���`�����`����
        upper_left = {"x": masking_result["point"]["x"], "y": masking_result["point"]["y"]}
        upper_right = {"x": masking_result["point"]["x"] + masking_result["width"], "y": masking_result["point"]["y"]}
        lower_right = {"x": masking_result["point"]["x"] + masking_result["width"], "y": masking_result["point"]["y"]
                                                                                         + masking_result["height"]}
        lower_left = {"x": masking_result["point"]["x"], "y": masking_result["point"]["y"] + masking_result["height"]}

        # ���`����
        draw = ImageDraw.Draw(image)
        draw.line((upper_left["x"], upper_left["y"], upper_right["x"], upper_right["y"]),
                  fill=marking_color, width=line_width)
        draw.line((upper_right["x"], upper_right["y"], lower_right["x"], lower_right["y"]),
                  fill=marking_color, width=line_width)
        draw.line((lower_right["x"], lower_right["y"], lower_left["x"], lower_left["y"]),
                  fill=marking_color, width=line_width)
        draw.line((lower_left["x"], lower_left["y"], upper_left["x"], upper_left["y"]),
                  fill=marking_color, width=line_width)

        # ���`�����摜��ۑ�����
        image.save(output_path, quality=100)

        result = True

    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, image


# ------------------------------------------------------------------------------------
# ������             �FNG���擾
#
# �����T�v           �F1.�}�X�L���O���茋�ʃt�@�C����Ǎ���Ńf�[�^��Ԃ��B
#
# ����               �F�}�X�L���O���茋�ʃt�@�C���p�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �Ǎ��f�[�^
# ------------------------------------------------------------------------------------
def read_result_file(result_file):
    # �}�X�L���O���茋��CSV�t�@�C����Ǎ���
    result, result_data, error = file_util.read_result_file(result_file, logger, app_id, app_name)

    return result, result_data


# ------------------------------------------------------------------------------------
# ������             �FNG�摜�ړ�
#
# �����T�v           �F1.�}�X�L���O���茋��NG�̉摜��ʃt�H���_�Ɉړ�����B
#
# ����               �FNG�摜�p�X
#                      NG�摜���t�@�C���p�X
#                      �}�[�L���O�摜���t�@�C���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def move_image(image_path, output_path, save_path):
    result = False
    file_name = os.path.basename(image_path)
    logger.info('[%s:%s] %s %s' % (app_id, app_name, file_name, output_path))
    # �ړ���Ɋ��Ƀt�@�C�������݂��邩�m�F����B
    if os.path.exists(output_path + "\\" + file_name):
        result = True
        return result

    # �t�@�C�������݂��Ȃ��ꍇ�A�t�@�C�����ړ�������B
    else:
        mk_result, error = file_util.make_directory(output_path, logger, app_id, app_name)
        if mk_result:
            mv_result, error = file_util.move_file(image_path, output_path, logger, app_id, app_name)
            if mv_result:
                # �t�@�C���ړ���ɁA�ʖ��imarking_*.jpg�j�ŃR�s�[����B
                cp_result, error = file_util.copy_file(output_path + "\\" + file_name, save_path, logger, app_id, app_name)
                if cp_result:
                    result = True
                    return result
                else:
                    return result
            else:
                return result
        else:
            return result


# ------------------------------------------------------------------------------------
# ������             �F�}�[�L���O�摜���X�V
#
# �����T�v           �F1.RAPID��͏��e�[�u���Ƀ}�[�L���O�摜����o�^����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ����ID
#                      �}�[�L���O�摜��
#                      RAPID�T�[�o�[�z�X�g��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_marking_image_name(conn, cur, fabric_name, inspection_num, ng_image, marking_image,
                              judge_ng, inspection_date, unit_num):
    # �N�G�����쐬����
    sql = 'UPDATE "rapid_%s_%s_%s" SET marking_image = \'%s\' ' \
          'WHERE ng_image = \'%s\' and masking_result = %s and unit_num = \'%s\'' % \
          (fabric_name, inspection_num, inspection_date, marking_image, ng_image, judge_ng, unit_num)

    logger.debug('[%s:%s] �}�[�L���O�摜���X�VSQL %s' % (app_id, app_name, sql))
    # �����X�e�[�^�X�i���T�C�Y�����j���X�V����B
    result, error, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�}�X�L���O���茋�ʃt�@�C����Ǎ��݁ANG�̉摜�ɑ΂��ă}�[�L���O���s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �i��
#                      ����
#                      �������
#                      �������i����ID�ARAPID�T�[�o�[�z�X�g���j
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def main(conn, cur, product_name, fabric_name, inspection_num, result_file, process_id_list,
         imaging_starttime, inspection_date, unit_num):
    result = False
    try:

        # �ݒ�t�@�C������ݒ�l���擾����B
        root_path = common_inifile.get('FILE_PATH', 'rk_path')
        ng_path = inifile.get('MARKING_INFO', 'ng_path')
        label_others = inifile.get('CSV_INFO', 'label_others')
        marking_colors = inifile.get('MARKING_INFO', 'color').split(',')
        marking_color = int(marking_colors[0]), int(marking_colors[1]), int(marking_colors[2])
        line_width = int(inifile.get('MARKING_INFO', 'line_width'))
        marking_name = inifile.get('MARKING_INFO', 'marking_name')
        marking_start_column = inifile.get('COLUMN', 'marking_start')
        marking_end_column = inifile.get('COLUMN', 'marking_end')
        marking_start = common_inifile.get('PROCESSING_STATUS', 'marking_start')
        marking_end = common_inifile.get('PROCESSING_STATUS', 'marking_end')
        judge_ng = int(common_inifile.get('ANALYSIS_STATUS', 'ng'))

        # �����J�n�������擾����B
        start_time = datetime.datetime.now()

        # �T�C�Y�̑傫�ȉ摜���X�L�b�v���Ȃ�
        ImageFile.LOAD_TRUNCATED_IMAGES = True

        logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' % 
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        # �������i����ID�ARAPID�T�[�o�[�z�X�g���j�̐������A�����X�e�[�^�X���X�V����B
        processing_id = 0
        for i in range(len(process_id_list)):
            if len(process_id_list) == 1 or process_id_list[i] != process_id_list[i - 1]:
                processing_id = process_id_list[i][0]
                rapid_host_name = process_id_list[i][1]

                logger.debug('[%s:%s] �����X�e�[�^�X�̍X�V���J�n���܂��B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s]'
                             % (app_id, app_name, processing_id, rapid_host_name))

                # �����X�e�[�^�X�i�}�[�L���O�J�n�j���X�V����B
                tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                                 marking_start_column, start_time, rapid_host_name,
                                                                 marking_start, imaging_starttime, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] �����X�e�[�^�X�̍X�V���I�����܂����B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s] '
                                 % (app_id, app_name, processing_id, rapid_host_name,))
                else:
                    logger.error('[%s:%s] �����X�e�[�^�X�̍X�V�����s���܂����B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s] '
                                 % (app_id, app_name, processing_id, rapid_host_name))
                    conn.rollback()
                    return result, conn, cur
            else:
                pass

        conn.commit()

        logger.debug('[%s:%s] �}�X�L���O���茋��CSV�̓Ǎ����J�n���܂��B' % (app_id, app_name))
        # ���ʃt�@�C���ǂݍ���
        tmp_result, result_data = read_result_file(result_file)

        if tmp_result:
            logger.debug('[%s:%s] �}�X�L���O���茋��CSV�̓Ǎ����I�����܂����B' % (app_id, app_name))

        else:
            logger.error('[%s:%s] �}�X�L���O���茋��CSV�̓Ǎ������s���܂����B' % (app_id, app_name))
            return result, conn, cur

        # �ϐ���`
        image_dir = result_data["datapath"]
        date = str(imaging_starttime.strftime('%Y%m%d'))

        # �ǎ挋�ʂ��Ƃɏ������s���B
        for masking_result in result_data["data"]:

            ng_image = masking_result["filename"]
            marking_image = marking_name + masking_result["filename"]

            # �ϐ���`
            image_path = image_dir + ng_image
            output_path = root_path + '\\' + ng_path + "\\" + date + "_" + product_name + "_" + fabric_name + "_" + \
                          str(inspection_num) + "\\"
            move_path = output_path + ng_image
            save_path = output_path + marking_image

            # �J�e�S�����u_others�v�̏ꍇ�A�}�[�L���O�s�v�̂��ߎ��̓ǎ挋�ʂ���������B
            if masking_result["label"] == label_others:
                logger.debug('[%s:%s] NG�摜�ł͂Ȃ����߁A �����͂���܂���B �t�@�C����=[%s]' % (app_id, app_name,
                                                                           masking_result["filename"]))
                continue

            # �J�e�S�����uNG�v�̏ꍇ�A�}�[�L���O�������s���B
            else:
                logger.debug('[%s:%s] NG�摜�ړ����J�n���܂��B �ړ���=[%s] �ړ���=[%s]' % (app_id, app_name, image_path, move_path))

                # NG�摜��NG�E�}�[�L���O�p�t�H���_�Ɉړ�����B
                tmp_result = move_image(image_path, output_path, save_path)

                if tmp_result:
                    logger.debug('[%s:%s] NG�摜�ړ����I�����܂����B ' % (app_id, app_name))
                else:
                    logger.error('[%s:%s] NG�摜�ړ������s���܂����B ' % (app_id, app_name))
                    return result, conn, cur

                logger.debug('[%s:%s] �}�[�L���O�������J�n���܂��B �t�@�C����=[%s]' % (app_id, app_name,
                                                                    masking_result["filename"]))

                # �摜��NG�ӏ��Ƀ}�[�L���O���ĕۑ�����B
                tmp_result, image = marking_process(save_path, masking_result, marking_color, line_width)

                if tmp_result:
                    logger.debug('[%s:%s] �}�[�L���O�������I�����܂����B �t�@�C����=[%s]' % (app_id, app_name,
                                                                         masking_result["filename"]))

                else:
                    logger.error('[%s:%s] �}�[�L���O���������s���܂����B �t�@�C����=[%s]' % (app_id, app_name,
                                                                         masking_result["filename"]))
                    return result, conn, cur

                # �}�[�L���O�摜����o�^����B
                logger.debug('[%s:%s] DB�ւ̃}�[�L���O�摜���o�^���J�n���܂��B �t�@�C����=[%s]' % (app_id, app_name,
                                                                           masking_result["filename"]))
                tmp_result, conn, cur = update_marking_image_name(conn, cur, fabric_name, inspection_num, ng_image,
                                                                  marking_image, judge_ng, inspection_date, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] DB�ւ̃}�[�L���O�摜���o�^���I�����܂����B �t�@�C����=[%s]' % (app_id, app_name,
                                                                                masking_result["filename"]))
                    conn.commit()

                else:
                    logger.error('[%s:%s] DB�ւ̃}�[�L���O�摜���o�^�����s���܂����B �t�@�C����=[%s]' % (app_id, app_name,
                                                                                masking_result["filename"]))
                    return result, conn, cur

        # �����I���������擾����B
        end_time = datetime.datetime.now()

        # �������i����ID�ARAPID�T�[�o�[�z�X�g���j�̐����A�����X�e�[�^�X���X�V����B
        for i in range(len(process_id_list)):
            if len(process_id_list) == 1 or process_id_list[i] != process_id_list[i - 1]:
                processing_id = process_id_list[i][0]
                rapid_host_name = process_id_list[i][1]

                logger.debug('[%s:%s] �����X�e�[�^�X�̍X�V���J�n���܂��B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s]'
                             % (app_id, app_name, processing_id, rapid_host_name))

                # �����X�e�[�^�X�i�}�[�L���O�����j���X�V����B
                tmp_result, conn, cur = update_processing_status(conn, cur, fabric_name, inspection_num, processing_id,
                                                                 marking_end_column, end_time, rapid_host_name,
                                                                 marking_end, imaging_starttime, unit_num)
                if tmp_result:
                    logger.debug('[%s:%s] �����X�e�[�^�X�̍X�V���I�����܂����B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s]'
                                 % (app_id, app_name, processing_id, rapid_host_name))
                else:
                    logger.error('[%s:%s] �����X�e�[�^�X�̍X�V�����s���܂����B ����ID=[%s] RAPID�T�[�o�[�z�X�g��=[%s]'
                                 % (app_id, app_name, processing_id, rapid_host_name))
                    conn.rollback()
                    return result, conn, cur

            else:
                pass

        conn.commit()

        logger.info('[%s:%s] %s�����͐���ɏI�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]' % 
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        result = True


    except Exception as e:
        # �z��O�G���[����
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, conn, cur
