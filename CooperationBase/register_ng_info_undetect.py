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
logging.config.fileConfig("D:/CI/programs/config/logging_register_undetectedimage.conf", disable_existing_loggers=False)
logger = logging.getLogger("register_ng_info_undetect")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/register_ng_info_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �F�������w�b�_�擾
#
# �����T�v           �F1.�������w�b�_�e�[�u�����猟�������ݒ���擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �����ԍ�
#                      �i��
#                      ����
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �����X�e�[�^�X���
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, inspection_num, product_name, fabric_name, imaging_starttime, unit_num):
    ### �N�G�����쐬����
    sql = 'select inspection_direction from ' \
          'inspection_info_header ' \
          'where inspection_num = %s and product_name = \'%s\' and fabric_name = \'%s\' ' \
          'and start_datetime = \'%s\' and unit_num = \'%s\'' \
          % (inspection_num, product_name, fabric_name, imaging_starttime, unit_num)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, select_result, error, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.NG�s�E�񔻒���s���B
#
# ����               �F�i��
#                    ����
#                    �����ԍ�
#                    �A��
#                    NG�摜��
#                    NG���W
#                    �����m�摜�t���O
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def main(product_name, fabric_name, inspection_num, num, ng_image_file_name, ng_point,
         undetected_image_flag_is_undetected, imaging_starttime, unit_num):
    # �ϐ���`
    # ��������
    result = False
    # �R�l�N�V�����I�u�W�F�N�g, �J�[�\���I�u�W�F�N�g
    conn = None
    cur = None
    # �G���[�t�@�C����
    error_file_name = None

    try:
        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
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

        logger.info('[%s:%s] %s�@�\���N�����܂�' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B' % (app_id, app_name))
        # DB�ɐڑ�����
        result, error, conn, cur, func_name = register_ng_info_util.create_connection(logger)

        if result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B' % (app_id, app_name))
            pass
        else:
            logger.error('[%s:%s] DB�ڑ������s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] ���W�}�[�N���擾���J�n���܂��B' % (app_id, app_name))
        result, regimark_info, error, conn, cur, func_name = register_ng_info_util.select_regimark_info(conn, cur, fabric_name,
                                                                                      inspection_num, imaging_starttime,
                                                                                      unit_num, logger)
        if result:
            logger.debug('[%s:%s] ���W�}�[�N���擾���I�����܂����B' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] ���W�}�[�N���擾�����s���܂����B' % (app_id, app_name))
            conn.rollback()
            sys.exit()

        logger.debug('[%s:%s] �����Ώۉ摜��� %s' % (app_id, app_name, ng_image_file_name))
        logger.debug('[%s:%s] �}�X�^���擾���J�n���܂��B' % (app_id, app_name))
        result, mst_data, error, conn, cur, func_name = register_ng_info_util.select_product_master_info(conn, cur, product_name, logger)
        if result:
            logger.debug('[%s:%s] �}�X�^���擾���I�����܂����B' % (app_id, app_name))
            logger.debug('[%s:%s] �}�X�^����� %s' % (app_id, app_name, mst_data))
        else:
            logger.debug('[%s:%s] �}�X�^���擾�����s���܂����B' % (app_id, app_name))
            conn.rollback()
            sys.exit()

        logger.debug('[%s:%s] �������擾���J�n���܂��B' % (app_id, app_name))
        result, inspection_direction, conn, cur = select_inspection_info(conn, cur, inspection_num, product_name,
                                                                         fabric_name, imaging_starttime, unit_num)
        if result:
            logger.debug('[%s:%s] �������擾���I�����܂����B' % (app_id, app_name))
            inspection_direction = inspection_direction[0]
        else:
            logger.debug('[%s:%s] �������擾�����s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] �s�ԍ�������J�n���܂��B' % (app_id, app_name))

        ng_image_info = []
        ng_image_info.append(str(num))
        ng_image_info.append(ng_image_file_name)
        ng_image_info.append(ng_point)
        result, line_info, last_flag, error, func_name = register_ng_info_util.specific_line_num(regimark_info, ng_image_info, inspection_direction, logger)
        if result:
            logger.debug('[%s:%s] �s�ԍ����肪�I�����܂����B�s��� [%s]' % (app_id, app_name, line_info))
        else:
            logger.debug('[%s:%s] �s�ԍ����肪���s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦�Z�o���J�n���܂��B' % (app_id, app_name))
        result, regimark_length_ratio, conf_regimark_between_length_pix, error, func_name = \
            register_ng_info_util.calc_length_ratio(regimark_info, line_info, nonoverlap_image_height_pix,
                                                    overlap_height_pix, resize_image_height, 
                                                    mst_data, master_image_width, 
                                                    actual_image_height, inspection_direction, logger)
        if result:
            logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦�Z�o���I�����܂����B' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] ���W�}�[�N�Ԓ���/���䗦�Z�o�����s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] NG�ʒu������J�n���܂��B' % (app_id, app_name))
        result, length_on_master, width_on_master, ng_face, error, func_name = register_ng_info_util.specific_ng_point(line_info, ng_image_info, nonoverlap_image_width_pix,
            nonoverlap_image_height_pix, overlap_width_pix, overlap_height_pix,
            resize_image_height, resize_image_width, regimark_length_ratio,
            mst_data, inspection_direction, master_image_width, master_image_height, actual_image_width, actual_image_overlap, logger)
        if result:
            logger.debug('[%s:%s] NG�ʒu���肪�I�����܂����B' % (app_id, app_name))
        else:
            logger.debug('[%s:%s] NG�ʒu���肪���s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] NG�s�E�������J�n���܂��B' % (app_id, app_name))
        result, judge_result, length_on_master, width_on_master, error, func_name = register_ng_info_util.specific_ng_line_colum(
            line_info, length_on_master, width_on_master, mst_data,
            conf_regimark_between_length_pix, inspection_direction, last_flag, logger)
        if result == True and judge_result == None:
            logger.debug('[%s:%s] NG�s�E����肪�I�����܂����B' % (app_id, app_name))
            logger.debug('[%s:%s] NG�s�E�񋫊E�l������J�n���܂��B' % (app_id, app_name))
            result, judge_result, length_on_master, width_on_master, error, func_name = register_ng_info_util.specific_ng_line_colum_border(
                regimark_info, length_on_master, width_on_master, mst_data, conf_regimark_between_length_pix,
                inspection_direction, last_flag, logger)
            if result:
                logger.debug('[%s:%s] NG�s�E�񋫊E�l���肪�I�����܂����B[�s,��] = %s' % (app_id, app_name, judge_result))
            else:
                logger.debug('[%s:%s] NG�s�E�񋫊E�l���肪���s���܂����B' % (app_id, app_name))
                sys.exit()

        elif result == True and judge_result != None:
            logger.debug('[%s:%s] NG�s�E����肪�I�����܂����B[�s,��] = %s' % (app_id, app_name, judge_result))
        else:
            logger.debug('[%s:%s] NG�s�E����肪���s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.debug('[%s:%s] ��_�����NG�����Z�o���J�n���܂��B' % (app_id, app_name))
        result, ng_dist, error, func_name = register_ng_info_util.calc_distance_from_basepoint(
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
        num = ng_image_info[0]
        ng_file = ng_image_info[1]
        inspection_date = str(imaging_starttime.strftime('%Y%m%d'))

        result, error, conn, cur, func_name = register_ng_info_util.update_ng_info(
            conn, cur, fabric_name, inspection_num, ng_line, ng_colum, master_point,
            ng_distance_x, ng_distance_y, num, ng_file, undetected_image_flag_is_undetected, inspection_date, unit_num, logger)
        if result:
            logger.debug('[%s:%s] NG���o�^���I�����܂����B' % (app_id, app_name))
            conn.commit()
        else:
            logger.debug('[%s:%s] NG���o�^�����s���܂����B' % (app_id, app_name))
            sys.exit()

        # �����Ώ۔�����񂪑��݂��Ȃ����߁ADB�ڑ���؂�A��莞�ԃX���[�v���Ă���A�Ď擾���s���B
        logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
        result, error, func_name = register_ng_info_util.close_connection(conn, cur, logger)

        if result:
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���������܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] DB�ڑ��̐ؒf�Ɏ��s���܂����B' % (app_id, app_name))
            sys.exit()

    except SystemExit:
        result = False
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        #error_util.common_execute(error_file_name, logger, app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B', app_id, app_name)

    except:
        result = False
        logger.error('[%s:%s] %s�@�\�ŗ\�����Ȃ��G���[���������܂����B[%s]', app_id, app_name, app_name, traceback.format_exc())
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        #error_util.common_execute(error_file_name, logger, app_id, app_name)
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


if __name__ == "__main__":
    main()
