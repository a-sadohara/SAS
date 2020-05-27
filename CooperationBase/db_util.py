# -*- coding: SJIS -*-
# ----------------------------------------
# ��   DB���ʋ@�\
# ----------------------------------------


import configparser
import psycopg2

import error_detail

#  ���ʐݒ�t�@�C���Ǎ���
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))


# ------------------------------------------------------------------------------------
# ������             �FDB�ڑ���ꏈ��
#
# �����T�v           �F1.DB�ڑ��������s���B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def base_create_connection():

    inifile = configparser.ConfigParser()
    inifile.read('D:/CI/programs/config/db_config.ini', 'SJIS')

    dbname = inifile.get('DB_INFO', 'dbname')
    user = inifile.get('DB_INFO', 'username')
    password = inifile.get('DB_INFO', 'password')
    port = inifile.get('DB_INFO', 'portnumber')
    host = inifile.get('DB_INFO', 'hostname')
    timeout = inifile.get('DB_INFO', 'timeout')
    url = "dbname=" + dbname + " host=" + host + " port=" + port + " user=" + user + " password=" + password + \
          " connect_timeout=" + timeout
    conn = psycopg2.connect(url)
    cur = conn.cursor()
    return conn, cur


# ------------------------------------------------------------------------------------
# ������             �FDB�ڑ�����
#
# �����T�v           �F1.DB�ڑ��������s���B
#                      2.DB�ڑ��G���[�̏ꍇ�͍Đڑ��𕡐���s���B
#
# ����               �F���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def create_connection(logger, app_id, app_name):
    # �ϐ��ݒ�
    result = False
    x = 0
    conn = None
    cur = None
    try:
        conn, cur = base_create_connection()
        # �ڑ������̂��ߏ�������=True
        result = True
        # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A��������=True��ԋp����B
        return result, conn, cur
    except Exception as e:
        # �G���[�ڍה�����s��
        tmp_result = error_detail.exception(e, logger, app_id, app_name)
        # ���茋�ʂ���������ꍇ�ADB�Đڑ�����
        if type(tmp_result) == tuple:
            # �R�l�N�V�����I�u�W�F�N�g�ƁA�J�[�\���I�u�W�F�N�g����
            result = tmp_result[0]
            conn = tmp_result[1]
            cur = tmp_result[2]
            # �ڑ������̂��ߏ�������=True
            # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A��������=True��ԋp����B
            return result, conn, cur
        # ���茋��=False�̏ꍇ�A�G���[
        else:
            # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�A��������=False��ԋp����B
            return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FDB�Q�Ə����i1�s�擾�j
#
# �����T�v           �F1.DB�Q�Ƃ��s���B
#                      2.DB�ڑ��G���[�̏ꍇ�͍Đڑ����A�ĂюQ�Ə����𕡐���s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �N�G����
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �擾����
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fetchone(conn, cur, sql, logger, app_id, app_name):
    # �ϐ��ݒ�
    result = False
    select_result = None
    for x in range(setting_loop_num):
        try:
            # DB���ʏ������Ăяo����DB���Q�Ƃ���
            cur.execute(sql)
            select_result = cur.fetchone()
            # �Q�Ɗ����̂��ߏ�������=Ture
            result = True
            # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�������ʁA�Q�ƌ��ʂ�ԋp����B
            return result, select_result, conn, cur
        except Exception as e:
            # �G���[�ڍה�����s��
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            # ���[�v�񐔊m�F�A�ݒ胋�[�v�������̏ꍇ�A�ȉ��̏������s��
            if x < (setting_loop_num - 1):
                # ���茋��=True�̏ꍇ�ADB�Đڑ�����
                if type(tmp_result) == tuple:
                    # �R�l�N�V�����I�u�W�F�N�g�ƁA�J�[�\���I�u�W�F�N�g����
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # �Ď��{
                    continue
                # ���茋��=False�̏ꍇ�A�G���[
                else:
                    # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�A��������=False��ԋp����B
                    return result, select_result, conn, cur
            # ���[�v�񐔏������߂̂��߁A�G���[
            else:
                # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�A��������=False��ԋp����B
                return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FDB�Q�Ə����i�S�s�擾�j
#
# �����T�v           �F1.DB�Q�Ƃ��s���B
#                      2.DB�ڑ��G���[�̏ꍇ�͍Đڑ����A�ĂюQ�Ə����𕡐���s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �N�G����
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �擾����
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fetchall(conn, cur, sql, logger, app_id, app_name):
    result = False
    select_result = None
    for x in range(setting_loop_num):
        try:
            cur.execute(sql)
            select_result = cur.fetchall()
            result = True
            return result, select_result, conn, cur
        except Exception as e:
            # �G���[�ڍה�����s��
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            # ���[�v�񐔊m�F�A�ݒ胋�[�v�������̏ꍇ�A�ȉ��̏������s��
            if x < (setting_loop_num - 1):
                # ���茋��=True�̏ꍇ�ADB�Đڑ�����
                if type(tmp_result) == tuple:
                    # �R�l�N�V�����I�u�W�F�N�g�ƁA�J�[�\���I�u�W�F�N�g����
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # �Ď��{
                    continue
                # ���茋��=False�̏ꍇ�A�G���[
                else:
                    # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�A��������=False��ԋp����B
                    return result, select_result, conn, cur
            # ���[�v�񐔏������߂̂��߁A�G���[
            else:
                # �R�l�N�V�����I�u�W�F�N�g�A�J�[�\���I�u�W�F�N�g�A�A��������=False��ԋp����B
                return result, select_result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FDB�f�[�^����
#
# �����T�v           �F1.DB�X�V��o�^�A�폜���s���B
#                      2.DB�ڑ��G���[�̏ꍇ�͍Đڑ����A�Ă�DB���쏈���𕡐���s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �N�G����
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def operate_data(conn, cur, sql, logger, app_id, app_name):
    result = False
    for x in range(setting_loop_num):
        try:
            cur.execute(sql)
            result = True
            return result, conn, cur
        except Exception as e:
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            if x < (setting_loop_num - 1):
                # ���茋��=True�̏ꍇ�ADB�Đڑ�����
                if type(tmp_result) == tuple:
                    # �R�l�N�V�����I�u�W�F�N�g�ƁA�J�[�\���I�u�W�F�N�g����
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # �Ď��{
                    continue
                # ���茋��=False�̏ꍇ�A�G���[
                else:
                    return result, conn, cur
            else:
                return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�e�[�u���쐬
#
# �����T�v           �F1.DB�X�V��o�^���s���B
#                      2.DB�ڑ��G���[�̏ꍇ�͍Đڑ����A�Ăэ쐬�����𕡐���s���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ����
#                      �����ԍ�
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def create_table(conn, cur, fabric_name, inspection_num, inspection_date, logger, app_id, app_name):
    result = False
    for x in range(setting_loop_num):
        try:
            cur.callproc('table_create', [fabric_name, inspection_num, inspection_date])
            result = True
            return result, conn, cur
        except Exception as e:
            tmp_result = error_detail.exception(e, logger, app_id, app_name)
            if x < (setting_loop_num - 1):
                # ���茋��=True�̏ꍇ�ADB�Đڑ�����
                if type(tmp_result) == tuple:
                    # �R�l�N�V�����I�u�W�F�N�g�ƁA�J�[�\���I�u�W�F�N�g����
                    conn = tmp_result[1]
                    cur = tmp_result[2]
                    # �Ď��{
                    continue
                # ���茋��=False�̏ꍇ�A�G���[
                else:
                    return result, conn, cur
            else:
                return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FDB�ؒf
#
# �����T�v           �F1.DB�Ƃ̃R�l�N�V������ؒf����
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def close_connection(conn, cur, logger, app_id, app_name):
    result = False
    x = setting_loop_num
    try:
        cur.close()
        conn.close()
        result = True
        return result
    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result
