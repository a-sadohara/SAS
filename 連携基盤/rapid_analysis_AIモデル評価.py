# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\306  RAPID��͎��s
# ----------------------------------------
import codecs
import os
import re
import socket
import multiprocessing as multi
import configparser
import logging.config
from concurrent.futures.thread import ThreadPoolExecutor
import sys
import json
import traceback
import time
import datetime
import gc

import db_util
import error_util
import file_util
import error_detail

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_rapid_analysis.conf")
logger = logging.getLogger("rapid_analysis")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/rapid_analysis_config.ini', 'SJIS')

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �FRAPID�T�[�o�ڑ����擾�iworker�P�ʁj
#
# �����T�v           �F1.RAPID�T�[�o�ڑ������擾����B
#
# ����               �FRAPID�T�[�o�z�X�g��
#                    RAPID�T�[�o�̃|�[�g�ԍ�
#                    ���g���C��
#
# �߂�l             �F�������ہiTrue:�����AFalse:���s�j
#                    socket���
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info_worker(ip_address, port_numbers, retry_num):
    result = False
    sock = []

    try:
        # IMA �풓�v���Z�X��ip�A�h���X��port�ԍ����w��
        ip_address = str(ip_address)
        port_number = int(port_numbers)

        # IMA �풓�v���Z�X�ɐڑ�
        logger.debug('[%s:%s] IMA �풓�v���Z�X�ɐڑ����܂��B[IP�A�h���X:%s][�|�[�g�ԍ�:%s]',
                     app_id, app_name,
                     ip_address, port_number
                     )
        sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        err = sock.connect_ex((ip_address, port_number))

        # ���ʔ���
        retry_count = 0
        while err != 0:
            #  �ڑ����s
            logger.warning('[%s:%s] %s: [IP�A�h���X:%s][�|�[�g�ԍ�:%s] �\�P�b�g�̐ڑ��Ɏ��s���܂����B',
                           app_id, app_name,
                           str(err), ip_address, str(port_number))
            # ���s���ɂ͈��񐔃��g���C����B
            if retry_count < retry_num:
                err = sock.connect_ex((ip_address, port_number))
                pass
            else:
                # ���g���C�񐔒��߂����ꍇ
                break
            # ���g���C�񐔂��J�E���g�A�b�v����
            retry_count = retry_count + 1

        else:
            logger.debug('[%s:%s] %s: [IP�A�h���X:%s][�|�[�g�ԍ�:%s] �\�P�b�g�̐ڑ��ɐ������܂����B[���g���C��:%s]',
                         app_id, app_name,
                         str(err), ip_address, str(port_number), retry_count)

        if err != 0:
            # �G���[�I��
            logger.error('[%s:%s] RAPID�T�[�o�ڑ��̐ڑ��Ɏ��s���܂����B', app_id, app_name)
            return result, sock

        else:
            # ����I��
            result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock


# ------------------------------------------------------------------------------------
# ������             �FRAPID�T�[�o�ڑ����擾�i�z�X�g�P�ʁj
#
# �����T�v           �F1.RAPID�T�[�o�ڑ������擾����B
#
# ����               �FRAPID�T�[�o�z�X�g��
#                    RAPID�T�[�o�̃|�[�g�ԍ�
#                    worker��
#                    ���g���C��
#
# �߂�l             �F�������ہiTrue:�����AFalse:���s�j
#                    socket��񃊃X�g
#
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info_host(ip_address, port_numbers, worker, retry_num):
    result = False
    sock = []

    try:
        # port�ԍ��𕪊�
        port_numbers = port_numbers.split(',')

        # worker�����ARAPID�풓�v���Z�X�ɐڑ�
        for i in range(worker):
            # IMA �풓�v���Z�X��ip�A�h���X��port�ԍ����w��
            ip_address = str(ip_address)
            port_number = int(port_numbers[i])

            tmp_result, sock_work = get_rapid_server_connect_info_worker(ip_address, port_number, retry_num)

            if tmp_result:
                # ���������ꍇ
                sock.append(sock_work)
            else:
                # ���s�����ꍇ
                return result, sock

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock


# ------------------------------------------------------------------------------------
# ������             �FRAPID�T�[�o�ڑ����擾
#
# �����T�v           �F1.RAPID�T�[�o�ڑ������擾����B
#
# ����               �FRAPID�T�[�o�z�X�g��
#                    RAPID�T�[�o�̃|�[�g�ԍ�
#                    worker��
#                    ���g���C��
#
# �߂�l             �F�������ہiTrue:�����AFalse:���s�j
#                    socket��񃊃X�g
#
# ------------------------------------------------------------------------------------
def get_rapid_server_connect_info(ip_addresses, port_numbers, worker, retry_num):
    result = False
    sock_all = []

    try:
        # ip�A�h���X�����ARAPID�풓�v���Z�X�ɐڑ�
        for i in range(len(ip_addresses)):
            sock_all.append([])
            ip_address = str(ip_addresses[i])

            tmp_result, sock = \
                get_rapid_server_connect_info_host(ip_address, port_numbers, worker, retry_num)

            if tmp_result:
                # ����I��
                sock_all[i].extend(sock)
            else:
                # �ُ�I��
                # �����֐��Ń��O�o�͂��Ă��邽�߁A�����ł̓��O�o�͂��Ȃ�
                return result, sock_all

        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, sock_all


# ------------------------------------------------------------------------------------
# �֐���             �FDB�ڑ�
#
# �����T�v           �F1.DB�Ɛڑ�����
#
# ����               �F�Ȃ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def create_connection():
    result, conn, cur = db_util.create_connection(logger, app_id, app_name)
    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�������擾�iDB�|�[�����O�j
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u�����甽�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
#                    �����X�e�[�^�X�e�[�u���̃X�e�[�^�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �����X�e�[�^�X���
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fabric_info_db_polling(conn, cur, processing_status_status):
    ### �N�G�����쐬����
    sql = 'select' \
          '  fabric_info.product_name' \
          ' ,fabric_info.fabric_name' \
          ' ,fabric_info.inspection_num' \
          ' ,fabric_info.imaging_starttime' \
          ' ,processing_status.rapid_starttime' \
          ' from fabric_info,processing_status' \
          ' where fabric_info.fabric_name = processing_status.fabric_name' \
          '  and fabric_info.inspection_num = processing_status.inspection_num' \
          '  and processing_status.status = %s' \
          '  order by processing_status.resize_endtime asc' \
          % processing_status_status

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�i�ԓo�^���擾
#
# �����T�v           �F1.�i�ԓo�^���e�[�u������AI���f�������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
#                    �i��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    AI���f����
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_matser_info(conn, cur, product_name):
    ### �N�G�����쐬����
    sql = 'select ai_model_name from mst_product_info where product_name = \'%s\'' % product_name

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)
    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�������e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.�������e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�X�e�[�^�X
#                     �Ŕ�
#                     �����ԍ�
#                     �J�[�\���I�u�W�F�N�g
#                     �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_fabric_info(column_name, time, status, fabric_name, inspection_num, cur, conn):
    ### �N�G�����쐬����
    sql = 'update fabric_info set status = %s, %s = \'%s\' ' \
          'where fabric_name = \'%s\' and inspection_num = %s' \
          % (status, column_name, time, fabric_name, inspection_num)

    ### �������e�[�u�����X�V
    logger.debug('[%s:%s] ����[%s], �����ԍ�[%s]�̃��R�[�h���X�V���܂����B�X�e�[�^�X[%s]',
                 app_id, app_name, fabric_name, inspection_num, status)
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X���擾�iRAPID��͑Ώہj
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     �Ŕ�
#                     �����ԍ�
#                     RAPID�T�[�o�z�X�g��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �����X�e�[�^�X���
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_processing_status_rapid_target(conn, cur, fabric_name, inspection_num, rapid_host_name, status_resize_end):
    ### �N�G�����쐬����
    sql = 'select processing_id from processing_status ' \
          'where fabric_name = \'%s\' and inspection_num = %s and rapid_host_name = \'%s\' and status = %s ' \
          % (fabric_name, inspection_num, rapid_host_name, status_resize_end)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u������f�[�^���擾����B
    result, records, conn, cur = db_util.select_fetchall(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�e�[�u���X�e�[�^�X�X�V
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �F�X�e�[�^�X
#                     �Ŕ�
#                     �����ԍ�
#                     ����ID
#                     RAPID�T�[�o�z�X�g��
#                     �J�[�\���I�u�W�F�N�g
#                     �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_processing_status(time, status, fabric_name, inspection_num, processing_id, rapid_server_host_name,
                             cur, conn):
    ### �N�G�����쐬����
    sql = 'update processing_status set status = %s, rapid_starttime = \'%s\'' \
          '  where fabric_name = \'%s\' and inspection_num = %s and processing_id = %s and rapid_host_name = \'%s\' ' \
          % (status, time, fabric_name, inspection_num, processing_id, rapid_server_host_name)

    ### �����X�e�[�^�X�e�[�u�����X�V
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�t�@�C���擾
#
# �����T�v           �F1.�B���摜�t�@�C���̃t�@�C�����X�g���擾����B
#
# ����               �F�B���摜�t�@�C���i�[�t�H���_�p�X
#                      �B���摜�t�@�C��
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �B�������ʒm�t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name):
    logger.debug('[%s:%s] �B���摜�t�@�C���i�[�t�H���_�p�X=[%s]', app_id, app_name, file_path)
    # ���ʊ֐��ŎB���摜�t�@�C���i�[�t�H���_�����擾����

    result, file_list = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

    return result, file_list


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
def exists_dir(target_path):
    result = file_util.make_directory(target_path, logger, app_id, app_name)

    return result


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͎��s�i���b�p�[�֐��j
#
# �����T�v           �F1.RAPID��͎��s�������Ăяo���B
#
# ����               �F�p�����[�^���
#
# �߂�l             �FRAPID��͎��s�̖߂�l�i�����j���A�P���X�g�`���ŕԋp����B
#                     ��F
#                      RAPID��͎��s�̖߂�l��3�uTrue, 'xxx', sock�I�u�W�F�N�g�v�̏ꍇ�A
#                      �{�֐��ł́A�ureturn[0]='True', return[1]='xxx', return[2]=sock�I�u�W�F�N�g�v��ԋp����B
# ------------------------------------------------------------------------------------
def wrapper_process(args):
    return process(*args)


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͎��s
#
# �����T�v           �F1.RAPID�T�[�o�Ƀ��N�G�X�g���M���A���ʂ��擾����B
#
#
# ����               �F�\�P�b�g���
#                    ���N�G�X�gJSON
#                    RAPID�T�[�o�z�X�g��
#                    RAPID�T�[�o�̃|�[�g�ԍ�
#                    ���g���C��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    ���X�|���X����
#                    �\�P�b�g���
# ------------------------------------------------------------------------------------
def process(sock, request_json, ip_address, port_num, retry_num):

    result = False
    received_str = None

    try:
        ### �ݒ�t�@�C������̒l�擾
        retry_target_err_no = inifile.get('VALUE', 'rapid_server_retry_error_code')
        retry_target_err_no = retry_target_err_no.split(',')

        for x in range(retry_num):
            # �J�n�����擾
            start = time.perf_counter()
            # IMA �풓�v���Z�X�Ƀ��N�G�X�g�𑗐M
            sock.send(request_json.encode('utf-8'))

            # IMA �풓�v���Z�X���烌�X�|���X����M
            # ���X�|���X�̍ŏ��� 9byte �̓��X�|���X�̒����ibyte�j������
            response_size = int(sock.recv(9))

            # IMA �풓�v���Z�X����̃��X�|���X�����X�|���X�̒�������M
            received_byte = 0
            received_str = ''
            while received_byte < response_size:
                response = sock.recv(min(4096, response_size - received_byte))
                received_byte = received_byte + len(response)
                received_str = received_str + response.decode('utf-8', 'ignore')

            # �I�������擾
            end = time.perf_counter()
            logger.debug('[%s:%s] process: exec time:[%s]', app_id, app_name, end - start)

            # �G���[�R�[�h����
            result_data = json.loads(received_str)
            result_data_len = len(result_data['result_list'])

            # RAPID��͌��ʂ���A�������ʁiIMA_status�j���m�F����B
            is_retry = False
            if result_data_len > 0:
                for j in range(result_data_len):
                    result_list = result_data['result_list'][j]
                    ima_status = str(result_list['IMA_status'])
                    logger.debug('[%s:%s] �������ʁiIMA_status�j:[%s]', app_id, app_name, ima_status)

                    if ima_status == '0':
                        # ����I���̏ꍇ
                        result = True
                        return result, received_str, sock
                    else:
                        # �ُ�I���̏ꍇ
                        if x < (retry_num - 1):
                            # ���g���C�񐔓��̏ꍇ
                            # ���g���C�Ώۂł��邩���ׂ�
                            retry_target_err_no = retry_target_err_no
                            if ima_status in retry_target_err_no:
                                # 102�FDEAD_PROCESS or 303�FNOREV_FOUND or 591�̏ꍇ
                                logger.warning('[%s:%s] RAPID��͏��������g���C���܂��B:[%s]', app_id, app_name, ima_status)
                                # socket�Đڑ�
                                worker_result, sock = \
                                    get_rapid_server_connect_info_worker(ip_address, port_num, retry_num)
                                break
                            else:
                                # ��L�ȊO�̃G���[
                                logger.error('[%s:%s] RAPID��͏������ُ�I�����܂����B:[%s]', app_id, app_name, ima_status)
                                return result, received_str, sock
                        else:
                            # ���g���C�񐔒��߂̏ꍇ
                            logger.error('[%s:%s] RAPID��͏������ُ�I�����܂����B�i���g���C�񐔒��߁j:[%s]', app_id, app_name, ima_status)
                            return result, received_str, sock

            else:
                # ���X�|���X���ʂ������ꍇ�̓G���[
                return result, received_str, sock

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result, received_str, sock


# ------------------------------------------------------------------------------------
# ������             �FNG���ʓo�^
#
# �����T�v           �F1.RAPID��͌��ʂ���͂��ANG���ʂ�RAPID��͏��e�[�u���ɓo�^����B
#
# ����               �FJSON���X�|���X������
#                     �i��
#                     �Ŕ�
#                     �����ԍ�
#                     �v���Z�XID
#                     �R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    NG����
#                    NG�摜�̃t�@�C���p�X�i���΁j
#
# ------------------------------------------------------------------------------------
def ng_result_register(received_str, product_name, fabric_name, inspection_num, processing_id, start_time,
                       end_time, rapid_server_host_name, image_dir, conn, cur):
    result = False
    rapid_processed_num = 0
    rapid_ng_num = 0
    ng_file_relative_path_list = []

    ### �ݒ�t�@�C������̒l�擾
    # RAPID��͌���:�m�M�x
    analysis_ng_Threshold = float(inifile.get('VALUE', 'analysis_ng_Threshold'))

    # RAPID��͌��ʂ�dict�`���ɕϊ�
    result_data = json.loads(received_str)
    result_data_len = len(result_data['result_list'])

    # RAPID��͌��ʂ���A�e�摜�̉�͌��ʂ��m�F����B
    if result_data_len > 0:
        for i in range(result_data_len):
            image_ok_flag = True

            result_list = result_data['result_list'][i]
            image_file = result_list['image']
            results = result_list["result"]
            logger.debug('[%s:%s] [%s]:[result_list=[%s]]', app_id, app_name, i, result_list)
            logger.debug('[%s:%s] [%s]:[image_file=[%s],[results=[%s]]', app_id, app_name,
                         i, result_list, image_file)

            if len(results) > 0:
                for j in range(len(results)):
                    rapid_result = results[j]
                    logger.debug('[%s:%s] [%s]:[rapid_result=[%s]', app_id, app_name, j, rapid_result)
                    # ��͌���
                    result_status = rapid_result.get('category')
                    # �m�M�x
                    confidence = float(rapid_result.get('confidence'))

                    logger.debug('[%s:%s] [%s]:[result_status(category)=[%s],[confidence=[%s]',
                                 app_id, app_name, j,
                                 result_status, confidence)

                    # ���ʔ���
                    if result_status == 'NG' and confidence >= analysis_ng_Threshold:
                        # ��͌���=NG ���� �m�M�x��0.5�̏ꍇ
                        image_ok_flag = False

                        # NG���W
                        result_position_list = rapid_result.get('position')
                        result_position = str(result_position_list[0]) + ',' + str(result_position_list[1])
                        # �t�@�C��������A�u�J�����ԍ�_������No1�v�u�J�����ԍ�_������No2�v���擾����B
                        # �Ȃ��A�t�@�C�����́u[�i��]_[����]_[���t]_[�����ԍ�]_ [������No]_[�J�����ԍ�]_[�A��].jpg�v��z�肵�Ă���
                        image_basename = os.path.basename(image_file)
                        file_name = re.split('[_.]', image_basename)
                        # FACE��񂩂猟�ŕ��𔻒f����B
                        #face_no = file_name[4]
                        face_no = 1
                        if face_no == '1':
                            # ���ŕ�No1�̏ꍇ
                            camera_no_1 = 1
                            camera_no_2 = None
                        else:
                            # ���ŕ�No2�̏ꍇ
                            camera_no_1 = None
                            camera_no_2 = 2

                        logger.debug('[%s:%s] [%s]:[result_position=[%s],[camera_num_1=[%s],[camera_num_2=[%s]]',
                                     app_id, app_name, j,
                                     result_position, camera_no_1, camera_no_2)

                        # DB���ʏ������Ăяo���āARAPID��͏��e�[�u���Ɉȉ��̍��ڂ�o�^����B

                        tmp_result, conn, cur = \
                            insert_rapid_analysis_info(
                                product_name,
                                fabric_name,
                                camera_no_1,
                                camera_no_2,
                                inspection_num,
                                processing_id,
                                image_basename,
                                start_time,
                                end_time,
                                int(common_inifile.get('ANALYSIS_STATUS', 'ng')),
                                result_position,
                                confidence,
                                rapid_server_host_name,
                                image_dir + image_file,
                                cur, conn)

                        if tmp_result:
                            logger.debug('[%s:%s] NG���ʓo�^���������܂����B', app_id, app_name)
                        else:
                            logger.error('[%s:%s] NG���ʓo�^�Ɏ��s���܂����B', app_id, app_name)
                            return result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list

                    else:
                        # ��͌���=OK ���邢�� �m�M�x<0.5�̏ꍇ
                        pass

            else:
                # result�������i����I�������摜�̏ꍇ�Aresult���͊܂܂�Ȃ��j
                logger.debug('[%s:%s] result��񂪂���܂���B[�X�e�[�^�X=OK]', app_id, app_name)

            # �����ϖ����ANG�����̃J�E���g�A�b�v
            if image_ok_flag:
                # �摜��NG�����������ꍇ
                rapid_processed_num = rapid_processed_num + 1
            else:
                # �摜��NG���L�����ꍇ
                rapid_ng_num = rapid_ng_num + 1
                rapid_processed_num = rapid_processed_num + 1
                ng_file_relative_path_list.append(image_file)

        # �����܂œ��B����ꍇ�͐���I��
        result = True
    else:
        # �擾���ʂ������i�G���[�j
        logger.error('[%s:%s] RAPID��͌��ʂ̗v�f�Ɂuresult_list�v������܂���B', app_id, app_name)

    return result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list

# ------------------------------------------------------------------------------------
# ������             �F�������擾
#
# �����T�v           �F1.�������w�b�_�e�[�u����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     ����
#                     �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �������
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_inspection_info(conn, cur, fabric_name, inspection_num, timestamp):
    ### �N�G�����쐬����
    sql = 'select worker_1, worker_2 from inspection_info_header ' \
          'where fabric_name = \'%s\' and inspection_num = %s and ' \
          'insert_datetime <= \'%s\' order by insert_datetime desc'\
          % (fabric_name, inspection_num, timestamp)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u������f�[�^���擾����B
    result, records, conn, cur = db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)

    return result, records, conn, cur

# ------------------------------------------------------------------------------------
# ������             �FRAPID��͏��e�[�u���o�^
#
# �����T�v           �F1.RAPID��͏��e�[�u����NG���ʃ��R�[�h��o�^����B
#
# ����               �F�i��
#                     �Ŕ�
#                     �J�����ԍ� ������No.1
#                     �J�����ԍ� ������No.2
#                     �����ԍ�
#                     �v���Z�X�ԍ�
#                      NG�摜��
#                      RAPID��͊J�n����
#                      �X�e�[�^�X
#                      NG���W
#                      �m�M�x
#                      �J�[�\���I�u�W�F�N�g
#                      �R�l�N�V�����I�u�W�F�N�g
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def insert_rapid_analysis_info(product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id,
                               image_file, imaging_starttime, end_time, result_status, result_position, confidence,
                               rapid_server_host_name, image_name, cur, conn):
    # �J�����ԍ� �������ɂ��āA�l�������ꍇ�ɂ�null�����ɒu��������
    if camera_num_1 is None:
        camera_num_1 = 'null'
    if camera_num_2 is None:
        camera_num_2 = 'null'

    file_timestamp = datetime.datetime.fromtimestamp(os.path.getctime(image_name)).strftime("%Y-%m-%d %H:%M:%S")
    tmp_result, records, conn, cur = select_inspection_info(conn, cur, fabric_name, inspection_num, file_timestamp)
    if tmp_result:
        logger.debug('[%s:%s] ��Ǝҏ��擾���������܂����B', app_id, app_name)
    else:
        logger.error('[%s:%s] ��Ǝҏ��擾�Ɏ��s���܂����B', app_id, app_name)
        return tmp_result, conn, cur

    worker_1 = records[0]
    worker_2 = records[1]

    ### �N�G�����쐬����
    sql = 'insert into "rapid_%s_%s" (' \
          'product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id, ng_image, ' \
          'rapid_starttime, rapid_endtime, rapid_result, ng_point, confidence, rapid_host_name, worker_1, worker_2 ' \
          ') values (' \
          '\'%s\',\'%s\', %s, %s, %s, %s, \'%s\', \'%s\', \'%s\', \'%s\', \'%s\', %s, \'%s\', \'%s\', \'%s\')' \
          % (fabric_name, inspection_num, product_name, fabric_name, camera_num_1, camera_num_2, inspection_num, processing_id,
             image_file, imaging_starttime, end_time, result_status, result_position, confidence,
             rapid_server_host_name, worker_1, worker_2)

    ### rapid��͏��e�[�u���Ƀ��R�[�h�ǉ�
    return db_util.operate_data(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# ������             �F�t�@�C���R�s�[
#
# �����T�v           �F1.�w��t�@�C���̃R�s�[���s���B
#
# ����               �F�R�s�[���t�@�C���p�X
#                     �R�s�[��t�@�C���p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def copy_file(target_file_path, copy_path):
    # �t�@�C���R�s�[
    result = file_util.copy_file(target_file_path, copy_path, logger, app_id, app_name)
    return result


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X�e�[�u���X�e�[�^�X�X�V�i��͊����j
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u���̃X�e�[�^�X���X�V����B
#
# ����               �FNG����
#                     �����ϖ���
#                     �Ŕ�
#                     �����ԍ�
#                     �v���Z�XID
#                     RAPID�T�[�o�z�X�g��
#                     �J�[�\���I�u�W�F�N�g
#                     �R�l�N�V�����I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def update_processing_status_rapid_end(rapid_ng_num, rapid_processed_num,
                                       fabric_name, inspection_num, processing_id, rapid_host_name, end_time,
                                       cur, conn):
    # �����X�e�[�^�X�e�[�u��.�����X�e�[�^�X�FRAPID��͊���
    status = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
    ### �N�G�����쐬����
    sql = 'update processing_status set status = %s, rapid_ng_num = %s, rapid_processed_num = %s, ' \
          'rapid_endtime = \'%s\'' \
          '  where fabric_name = \'%s\' and inspection_num = %s and processing_id = %s and rapid_host_name = \'%s\' ' \
          % (status, rapid_ng_num, rapid_processed_num, end_time,
             fabric_name, inspection_num, processing_id, rapid_host_name)

    ### �����X�e�[�^�X�e�[�u����ǉ�
    result, conn, cur = db_util.operate_data(conn, cur, sql, logger, app_id, app_name)

    return result, conn, cur


# ------------------------------------------------------------------------------------
# ������             �FRAPID��͕�����s�i�}���`�X���b�h�j
#
# �����T�v           �F1.RAPID��͏������A����i�q�X���b�h���Ɓj�Ɏ��s����B
#
# ����               �F�v���Z�XID
#                     RAPID�T�[�o�z�X�g��
#                     worker��
#                     RAPID�T�[�o�̃|�[�g�ԍ�
#                     �i��
#                     �Ŕ�
#                     �����ԍ�
#                     �B���J�n����
#                     �\�P�b�g���
#                     �R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def exec_rapid_analysis_multi_thread(rapid_server_host_name, worker, port_num,
                                     product_name, fabric_name, inspection_num,
                                     imaging_starttime,
                                     sock, ai_model_name,
                                     all_processed_num, all_ng_num):
    logger.debug('[%s:%s] exec_rapid_analysis_multi_thread start'
                 '[rapid_server_host_name:%s][worker:%s][port_num:%s]'
                 '[product_name:%s][fabric_name:%s][inspection_num:%s][imaging_starttime:%s]',
                 app_id, app_name,
                 rapid_server_host_name, worker, port_num,
                 product_name, fabric_name, inspection_num, imaging_starttime)

    result = False

    try:
        # �����X�e�[�^�X�e�[�u��.�����X�e�[�^�X�FRAPID��͊J�n
        processing_status_rapid_start = common_inifile.get('PROCESSING_STATUS', 'rapid_start')
        # �����X�e�[�^�X�e�[�u��.�J�������FRAPID��͊J�n����
        status_resize_end = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        # �����X�e�[�^�X�e�[�u��.�J�������FRAPID��͊�������
        processing_column_rapid_end = inifile.get('COLUMN', 'processing_rapid_end')
        # �摜����臒l
        image_num_threshold = int(inifile.get('VALUE', 'image_num_threshold'))
        # NG��臒l
        ng_threshold = float(inifile.get('VALUE', 'ng_threshold'))
        # RAPID�摜�i�[���[�g�p�X�i���΁j
        image_dir = inifile.get('FILE_PATH', 'image_dir')
        image_dir = '\\\\' + rapid_server_host_name + '\\' + image_dir
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')
        mode = inifile.get('REQUEST_PARAMS', 'mode')
        processor = inifile.get('REQUEST_PARAMS', 'processor')
        preprocess = inifile.get('REQUEST_PARAMS', 'preprocess')
        summary = inifile.get('REQUEST_PARAMS', 'summary')
        # rapid���g���C��
        rapid_connect_num = inifile.get('ERROR_RETRY', 'rapid_connect_num')

        image_file_pattern = common_inifile.get('FILE_PATTERN', 'image_file')
        ng_dir = inifile.get('FILE_PATH', 'ng_path')

        today_date = str(imaging_starttime.strftime('%Y%m%d'))



        # DB���ʋ@�\���Ăяo���āADB�ɐڑ�����B
        logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
        tmp_result, conn, cur = create_connection()

        if tmp_result:
            logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
            return result, conn, cur, all_processed_num, all_ng_num

        ### ����ID�擾
        # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u�����猟�������擾����B
        logger.debug('[%s:%s] �����X�e�[�^�X�f�[�^�擾�iRAPID��͑Ώہj���J�n���܂��B', app_id, app_name)
        tmp_result, records, conn, cur = \
            select_processing_status_rapid_target(conn, cur, fabric_name, inspection_num,
                                                  rapid_server_host_name, status_resize_end)

        if tmp_result:
            logger.debug('[%s:%s] �����X�e�[�^�X�f�[�^�擾�iRAPID��͑Ώہj���I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] �����X�e�[�^�X�f�[�^�擾�iRAPID��͑Ώہj�Ɏ��s���܂����B', app_id, app_name)
            return result, conn, cur, all_processed_num, all_ng_num

        if len(records) > 0:
            logger.debug('[%s:%s] RAPID��͑Ώۂ����݂��܂��B', app_id, app_name)
        else:
            logger.debug('[%s:%s] RAPID��͑Ώۂ����݂��܂���B', app_id, app_name)
            result = True
            return result, conn, cur, all_processed_num, all_ng_num

        for i in range(len(records)):
            start_time = datetime.datetime.now()
            processing_id = records[i][0]
            ### �����X�e�[�^�X�X�V
            logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���X�V���J�n���܂��B', app_id, app_name)
            tmp_result, conn, cur = update_processing_status(start_time, processing_status_rapid_start, fabric_name,
                                                             inspection_num, processing_id, rapid_server_host_name, cur, conn)

            if tmp_result:
                logger.debug('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V���I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] �����X�e�[�^�X�e�[�u���̍X�V�Ɏ��s���܂����B', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            ### ���m�Ώۉ摜����
            ## ���m�Ώۉ摜����肷��B
            # �t�@�C�����p�^�[���͈ȉ���z�肷��B
            # �u*(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)*.jpg�v
            #image_file_name_pattern = '\\*' + fabric_name + '_' + today_date + '_' + str(inspection_num).zfill((2)) + image_file_pattern
            image_file_name_pattern = '\\*'

            # �Q�Ɛ�p�X�\���͈ȉ���z�肷��B
            # �u(�i��)_(����)_(�B���J�n����:YYYYMMDD�`��)_(�����ԍ�)�v
            #                 �b-(����ID�P)
            #                 �b-(����ID�Q)
            #                 �b-(����ID�R)
            image_path = image_dir + '\\' + product_name + '_' + fabric_name + '_' + today_date + '_' + \
                         str(inspection_num).zfill((2)) + '\\' + str(processing_id)

            logger.debug('[%s:%s] ���m�Ώۉ摜�t�@�C���̊m�F���J�n���܂��B', app_id, app_name)
            tmp_result, image_files = get_file(image_path, image_file_name_pattern)

            if tmp_result:
                logger.debug('[%s:%s] ���m�Ώۉ摜�t�@�C���̊m�F���������܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] ���m�Ώۉ摜�t�@�C���̊m�F�Ɏ��s���܂����B', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            logger.debug('[%s:%s] ���m�Ώۉ摜�t�@�C��:[%s]', app_id, app_name, image_files)

            # RAPID��͑Ώۂ̎B���摜�����i��͎��s�p�����[�^�����j����B
            rapid_images = []
            # ���m�Ώۉ摜�̃t�@�C���p�X�i�t���p�X�j����A���΃p�X�`�������B
            for path in image_files:
                rapid_images.append(path.replace(image_dir, '\\'))

            # �摜�����[�J�[���œ�������
            rapid_image = []
            for i in range(0, worker):
                rapid_image.append([])
            for num in range(0, len(rapid_images)):
                rapid_image[int(num / (len(rapid_images) / worker))].append(rapid_images[num])

            # IMA �풓�v���Z�X�ɑ΂��郊�N�G�X�g�iJSON�t�H�[�}�b�g�j���쐬
            ### ���N�G�X�gJSON�쐬
            request_jsons = []

            for i in range(0, worker):
                request_json = json.dumps(
                    {"mode": mode, "image": rapid_image[i], "processor": processor, "model": ai_model_name,
                     "preprocess": preprocess, "summary": summary}, ensure_ascii=False)
                # IMA �풓�v���Z�X�ɑ΂��郊�N�G�X�g�̖����ɕK�����s�L��"\n"������
                request_json = request_json + "\n"
                request_jsons.append(request_json)

                ## RAPID��͂Ƀ��N�G�X�g�Ƃ��đ���JSON���쐬����B
                # �t�@�C�����͈ȉ���z�肷��B
                # �u(�B���J�n����:YYYYMMDD�`��)_(�i��)_(����)_(�����ԍ�)_(����ID)_(worker�ԍ��i1����̘A�ԁj)_request.txt�v
                request_json_file_name = today_date + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num).zfill((2)) + '_' + str(processing_id) + '_' + str(i+1) + '_request.txt'
                logger.debug('[%s:%s] request_json_file_name:[%s]', app_id, app_name, request_json_file_name)

                # �Q�Ɛ�p�X�\���͈ȉ���z�肷��B
                # �u(�B���J�n����:YYYYMMDD�`��)_(�i��)_(����)_(�����ԍ�)_JSON�v
                request_json_file_path = rk_root_path + '\\JSON\\' + today_date + '_' + product_name + '_' + \
                                         fabric_name + '_' + str(inspection_num).zfill((2)) + "\\" + rapid_server_host_name \
                                         + "\\" + str(processing_id)
                logger.debug('[%s:%s] request_json_file_path:[%s]', app_id, app_name, request_json_file_path)

                # �i�[��t�H���_���쐬����
                logger.debug('[%s:%s] JSON�t�@�C���i�[�t�H���_�쐬���J�n���܂��B', app_id, app_name)
                tmp_result = exists_dir(request_json_file_path)

                if tmp_result:
                    logger.debug('[%s:%s] JSON�t�@�C���i�[�t�H���_�쐬���������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] JSON�t�@�C���i�[�t�H���_�쐬�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

                # JSON�t�@�C���쐬
                logger.debug('[%s:%s] ���N�G�X�gJSON�t�@�C���쐬���J�n���܂��B:[%s]', app_id, app_name,
                             request_json_file_path + '\\' + request_json_file_name)
                ofp = codecs.open(request_json_file_path + '\\' + request_json_file_name, 'w', 'utf-8')
                ofp.write(request_json)
                ofp.close()
                logger.debug('[%s:%s] ���N�G�X�gJSON�t�@�C���쐬���������܂����B:[%s]', app_id, app_name,
                             request_json_file_path + '\\' + request_json_file_name)

            # ����
            # ���[�J�[�����̃v���Z�X��p��
            pool = multi.Pool(worker)

            # �����̏���
            process_args = [
                [sock[i], request_jsons[i], rapid_server_host_name, port_num[i], int(rapid_connect_num)]
                for i in
                range(0, worker)]

            ### RAPID��͎��s
            logger.info('[%s:%s] RAPID��͎��s���J�n���܂��B�z�X�g��=[%s]', app_id, app_name, rapid_server_host_name)
            received_data = pool.map(wrapper_process, process_args)
            pool.close()

            # �������ʊm�F
            received_str = []
            for i in range(0, worker):
                tmp_result = received_data[i][0]
                logger.info('[%s:%s] received_data:[%s]', app_id, app_name, tmp_result)
                if tmp_result:
                    # ����I����
                    if received_data[i][1] is None or received_data[i][1] == '':
                        logger.error('[%s:%s] RAPID��͌��ʂ̎擾�Ɏ��s���܂����B', app_id, app_name)
                        conn.rollback()
                        return result, conn, cur, all_processed_num, all_ng_num
                    else:
                        # �q�X���b�h����̌��ʂ�ϐ��ɐݒ�
                        received_str.append(received_data[i][1])
                        sock[i] = received_data[i][2]

                else:
                    # �ُ�I����
                    logger.error('[%s:%s] RAPID��͎��s�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

            end_time = datetime.datetime.now()

            # �󂯎������͌��ʂ��e�L�X�g�t�@�C���Ƃ��ĕۑ�����B
            for i in range(0, worker):
                # �t�@�C�����͈ȉ���z�肷��B
                # �u(�B���J�n����:YYYYMMDD�`��)_(�i��)_(����)_(�����ԍ�)_(����ID)_(worker�ԍ��i1����̘A�ԁj)_response.txt�v
                response_json_file_name = today_date + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num).zfill((2)) + '_' + \
                                          str(processing_id) + '_' + str(i+1) + '_response.txt'
                logger.debug('[%s:%s] response_json_file_name:[%s]', app_id, app_name, response_json_file_name)

                # �Q�Ɛ�p�X�\���͈ȉ���z�肷��B
                # ���N�G�X�gJSON�t�@�C���i�[�Ɠ����p�X
                request_json_file_path = rk_root_path + '\\JSON\\' + today_date + '_' + product_name + '_' + \
                                         fabric_name + '_' + str(inspection_num).zfill((2)) + "\\" + rapid_server_host_name \
                                         + "\\" + str(processing_id)

                response_json_file_path = request_json_file_path
                logger.debug('[%s:%s] response_json_file_path:[%s]', app_id, app_name, response_json_file_path)

                # JSON�t�@�C���쐬
                logger.debug('[%s:%s] ���X�|���XJSON�t�@�C���̍쐬���J�n���܂��B:[%s]', app_id, app_name,
                             response_json_file_path + '\\' + response_json_file_name)

                ofp = codecs.open(response_json_file_path + '\\' + response_json_file_name, 'w', 'utf-8')
                ofp.write(received_str[i])
                ofp.close()
                logger.debug('[%s:%s] ���X�|���XJSON�t�@�C���̍쐬���������܂����B:[%s]', app_id, app_name,
                             response_json_file_path + '\\' + response_json_file_name)

#############################
#                continue
#############################

                ### NG���ʓo�^
                # RAPID��͂̌��ʂ��m�F����臒l�𒴂��Ă���ꍇ�ARAPID��͏��e�[�u���ɓo�^����B
                tmp_result, conn, cur, rapid_processed_num, rapid_ng_num, ng_file_relative_path_list \
                    = ng_result_register(received_str[i], product_name, fabric_name, inspection_num, processing_id,
                                         start_time, end_time, rapid_server_host_name, image_dir, conn, cur)
                all_ng_num += rapid_ng_num
                all_processed_num += rapid_processed_num

                if tmp_result:
                    logger.debug('[%s:%s] NG���ʓo�^���������܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG���ʓo�^�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    return result, conn, cur, all_processed_num, all_ng_num

                ### NG�摜�R�s�[
                # RAPID��͌��ʂ�NG�摜�����݂��邩
                if len(ng_file_relative_path_list) > 0:
                    # NG�摜�����݂���ꍇ
                    # �R�s�[��p�X�\���͈ȉ���z�肷��B
                    # �u(�B���J�n����:YYYYMMDD�`��)_(�i��)_(����)_(NG)�v
                    #                 �b-(����ID�P)
                    #                 �b-(����ID�Q)
                    #                 �b-(����ID�R)
                    copy_path = rk_root_path + '\\' + ng_dir + "\\" + today_date + '_' + product_name + '_' + \
                                fabric_name + '_' + str(inspection_num).zfill((2)) + "\\"

                    # �R�s�[��t�H���_���쐬����
                    logger.debug('[%s:%s] NG�摜�t�@�C���̃R�s�[��t�H���_�쐬���J�n���܂��B:�R�s�[��t�H���_[%s]', app_id, app_name, copy_path)
                    tmp_result = exists_dir(copy_path)

                    if tmp_result:
                        logger.debug('[%s:%s] NG�摜�t�@�C���̃R�s�[��t�H���_�쐬���������܂����B:�R�s�[��t�H���_[%s]', app_id, app_name, copy_path)
                    else:
                        logger.error('[%s:%s] NG�摜�t�@�C���̃R�s�[��t�H���_�쐬�Ɏ��s���܂����B:�R�s�[��t�H���_[%s]', app_id, app_name, copy_path)
                        conn.rollback()
                        return result, conn, cur, all_processed_num, all_ng_num

                    for i in range(len(ng_file_relative_path_list)):
                        logger.debug('[%s:%s] NG�摜�R�s�[���J�n���܂��B ', app_id, app_name)
                        # NG�摜�t�@�C���̃p�X�͑��΂Ȃ̂ŁA��΃p�X�ɒu������B
                        ng_file_relative_path = ng_file_relative_path_list[i]
                        ng_file_basename = os.path.basename(ng_file_relative_path)
                        ng_file_path = image_path + '\\' + ng_file_basename

                        tmp_result = copy_file(ng_file_path, copy_path)

                        if tmp_result:
                            logger.debug('[%s:%s] NG�摜�R�s�[���������܂����B �R�s�[��t�H���_[%s], NG�摜�t�@�C����[%s]',
                                         app_id, app_name, copy_path, ng_file_path)
                        else:
                            logger.error('[%s:%s] NG�摜�R�s�[�Ɏ��s���܂����B �R�s�[��t�H���_[%s], NG�摜�t�@�C����[%s]',
                                         app_id, app_name, copy_path, ng_file_path)
                            conn.rollback()
                            return result, conn, cur, all_processed_num, all_ng_num

            ### �����X�e�[�^�X�X�V
            logger.debug('[%s:%s] �����X�e�[�^�X�X�V���J�n���܂��B', app_id, app_name)
            end_time = datetime.datetime.now()
            tmp_result, conn, cur = \
                update_processing_status_rapid_end(all_ng_num,all_processed_num,fabric_name, inspection_num,
                                                   processing_id,rapid_server_host_name, end_time,cur, conn)

            if tmp_result:
                logger.debug('[%s:%s] �����X�e�[�^�X�X�V���I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] �����X�e�[�^�X�X�V�Ɏ��s���܂����B', app_id, app_name)
                conn.rollback()
                return result, conn, cur, all_processed_num, all_ng_num

            conn.commit()

            logger.debug('[%s:%s] �����ϖ����m�F���J�n���܂��B', app_id, app_name)
            if all_processed_num >= image_num_threshold:
                logger.debug('[%s:%s] �����ϖ����m�F���I�����܂����B�摜����臒l�𒴂��Ă��܂��B', app_id, app_name)
                logger.debug('[%s:%s] ��ʌ��m������J�n���܂��B', app_id, app_name)
                # �������ʂ��m�F����
                ng_rate = all_ng_num / all_processed_num
                if all_processed_num >= image_num_threshold and ng_rate >= ng_threshold:
                    # �����ϖ���=[�摜����臒l] ���� NG�����������ϖ�����[NG��臒l]�ȏ�̏ꍇ
                    # ��ʌ��m�Ƃ��Ĉُ�I��
                    logger.warning('[%s:%s] ��ʌ��m�ł��B:�����ϖ���[%s] NG����[%s] NG��[%s] ', app_id, app_name,
                                   all_processed_num, all_ng_num, ng_rate)
                    result = True
                    return result, conn, cur, all_processed_num, all_ng_num

                else:
                    logger.debug('[%s:%s] ��ʌ��m�ł͂���܂���B', app_id, app_name)
                    logger.info('[%s:%s] RAPID��͂̃}���`�X���b�h���s���I�����܂��B[%s]', app_id, app_name, rapid_server_host_name)
                    result = True
                    #return result, conn, cur, all_processed_num, all_ng_num
            else:

                logger.info('[%s:%s] RAPID��͂̃}���`�X���b�h���s���I�����܂��B[%s]', app_id, app_name, rapid_server_host_name)
                result = True
                #return result, conn, cur, all_processed_num, all_ng_num

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    logger.debug('[%s:%s] exec_rapid_analysis_multi_thread end'
                 '[rapid_server_host_name:%s][worker:%s][port_num:%s]'
                 '[product_name:%s][fabric_name:%s][inspection_num:%s][imaging_starttime:%s]',
                 app_id, app_name,
                 rapid_server_host_name, worker, port_num,
                 product_name, fabric_name, inspection_num, imaging_starttime)

    return result, conn, cur, all_processed_num, all_ng_num


# ------------------------------------------------------------------------------------
# ������             �F�������擾�i����/���T�C�Y���������j
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u�����番��/���T�C�Y�����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     �Ŕ�
#                     �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �����X�e�[�^�X���
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_fabric_info_separateresize_endtime(conn, cur, fabric_name, inspection_num):
    ### �N�G�����쐬����
    sql = 'select separateresize_endtime from fabric_info' \
          ' where fabric_name = \'%s\' and inspection_num = %s' \
          % (fabric_name, inspection_num)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u���Ɣ������e�[�u������f�[�^���擾����B
    return db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)


# ------------------------------------------------------------------------------------
# ������             �F�����X�e�[�^�X���擾�iRAPID��͏��������j
#
# �����T�v           �F1.�����X�e�[�^�X�e�[�u����������擾����B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                     �J�[�\���I�u�W�F�N�g
#                     �X�e�[�^�X
#                     �Ŕ�
#                     �����ԍ�
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                    �����X�e�[�^�X���
#                    �R�l�N�V�����I�u�W�F�N�g
#                    �J�[�\���I�u�W�F�N�g
# ------------------------------------------------------------------------------------
def select_processing_status_rapid_end(conn, cur, status, fabric_name, inspection_num):
    ### �N�G�����쐬����
    sql = 'select ' \
          'count(*) as record_count ,' \
          'count(case when rapid_endtime is not null then 1 else null end) as rapid_end_count' \
          ' from processing_status' \
          ' where fabric_name = \'%s\' and inspection_num = \'%s\' group by fabric_name, inspection_num' \
          % (fabric_name, inspection_num)

    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u������f�[�^���擾����B

    return db_util.select_fetchone(conn, cur, sql, logger, app_id, app_name)


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
    # DB�ڑ���ؒf����B
    res = db_util.close_connection(conn, cur, logger, app_id, app_name)

    return res


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�B�������o�^���s���B
#
# ����               �F�Ȃ�
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def main():
    error_file_name = None
    conn = None
    cur = None
    try:
        ### �ϐ���`
        # �R�l�N�V�����I�u�W�F�N�g, �J�[�\���I�u�W�F�N�g

        ### �ݒ�t�@�C������̒l�擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')
        # �X���b�h��
        thread_num = int(common_inifile.get('VALUE', 'thread_num'))
        ### RAPID�T�[�o�ڑ����
        # ip�A�h���X
        ip_addresses = common_inifile.get('RAPID_SERVER', 'ip_address')
        # port�ԍ�
        port_numbers = common_inifile.get('RAPID_SERVER', 'port_number')
        # worker��
        worker = int(common_inifile.get('RAPID_SERVER', 'worker'))

        # NG��臒l
        ng_threshold = float(inifile.get('VALUE', 'ng_threshold'))
        # �X���[�v����
        sleep_time = int(inifile.get('VALUE', 'sleep_time'))
        # �����X�e�[�^�X�e�[�u��.�����X�e�[�^�X�F���T�C�Y����
        processing_status_resize_end = common_inifile.get('PROCESSING_STATUS', 'resize_end')
        # �������e�[�u��.�����X�e�[�^�X�FRAPID��͊J�n
        fabric_info_status_rapid_start = common_inifile.get('FABRIC_STATUS', 'rapid_start')
        # �������e�[�u��.�����X�e�[�^�X�FRAPID��͊���
        fabric_info_status_rapid_end = common_inifile.get('FABRIC_STATUS', 'rapid_end')
        # �����X�e�[�^�X�e�[�u��.�����X�e�[�^�X�FRAPID��͊���
        processing_status_rapid_end = common_inifile.get('PROCESSING_STATUS', 'rapid_end')
        # �������e�[�u��.�����X�e�[�^�X�FRAPID��͊J�n����
        fabric_info_column_rapid_start = inifile.get('COLUMN', 'fabric_rapid_start')
        # �������e�[�u��.�����X�e�[�^�X�FRAPID��͊�������
        fabric_info_column_rapid_end = inifile.get('COLUMN', 'fabric_rapid_end')

        # RAPID�T�[�o�ڑ����g���C��
        rapid_connect_retry_num = int(inifile.get('ERROR_RETRY', 'rapid_connect_num'))
        # ������������G���[���g���C��
        error_continue_num = int(inifile.get('VALUE', 'error_continue_num'))

        # NG���������l
        all_ng_num = 0
        # �����ϖ��������l
        all_processed_num = 0

        # �����ϖ������}���`�X���b�h�p�ɔz��
        multi_processed_num = [all_processed_num for i in range(thread_num)]
        # NG�������}���`�X���b�h�p�ɔz��
        multi_ng_num = [all_ng_num for i in range(thread_num)]

        # �G���[���p���ϐ�
        confirm_error_flag = 0

        logger.info('[%s:%s] %s�@�\���N�����܂��B', app_id, app_name, app_name)

        ### �풓�v���Z�X�ڑ�
        ip_address = ip_addresses.split(',')
        result, sock = get_rapid_server_connect_info(ip_address, port_numbers, worker, rapid_connect_retry_num)
        if result:
            logger.debug('[%s:%s] RAPID�풓�v���Z�X�ڑ����I�����܂����B', app_id, app_name)
        else:
            logger.error('[%s:%s] RAPID�풓�v���Z�X�ڑ��ŃG���[���������܂����B', app_id, app_name)
            sys.exit()
        while True:
            # DB���ʋ@�\���Ăяo���āADB�ɐڑ�����B
            logger.debug('[%s:%s] DB�ڑ����J�n���܂��B', app_id, app_name)
            result, conn, cur = create_connection()

            if result:
                logger.debug('[%s:%s] DB�ڑ����I�����܂����B', app_id, app_name)
            else:
                logger.error('[%s:%s] DB�ڑ����ɃG���[���������܂����B', app_id, app_name)
                sys.exit()

            ### �������擾�iDB�|�[�����O�j
            while True:
                # �����X�e�[�^�X�e�[�u�����甽�������擾����B
                logger.debug('[%s:%s] �������擾�iDB�|�[�����O�j���J�n���܂��B', app_id, app_name)
                result, target_record, conn, cur = \
                    select_fabric_info_db_polling(conn, cur,
                                                  processing_status_resize_end)

                if result:
                    logger.debug('[%s:%s] �������擾�iDB�|�[�����O�j���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �������擾�iDB�|�[�����O�j�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                # �擾���ʂ���A�Y�����R�[�h�����݂��邩�m�F����B
                if target_record is not None and len(target_record) > 0:
                    # �Y�����R�[�h�����݂���ꍇ
                    # �ȍ~�̏����𑱂���B
                    pass
                else:
                    # �Y�����R�[�h�������ꍇ
                    # ��莞�ԃX���[�v������A�������擾�����s����B
                    logger.info('[%s:%s] �����Ώۂ̔�����񂪑��݂��܂���B', app_id, app_name)
                    time.sleep(sleep_time)
                    continue

                product_name = target_record[0]
                fabric_name = target_record[1]
                inspection_num = target_record[2]
                start_time = datetime.datetime.now()

                logger.info('[%s:%s] %s�������J�n���܂��B:[����,�����ԍ�]=[%s,%s]',
                            app_id, app_name, app_name, fabric_name, inspection_num)

                logger.debug('[%s:%s] �i�ԓo�^���e�[�u������AI���f�����̎擾���J�n���܂��B', app_id, app_name)
                result, ai_model_name, conn, cur = select_matser_info(conn, cur, product_name)

                if result:
                    logger.debug('[%s:%s] �i�ԓo�^���e�[�u������AI���f�����̎擾���I�����܂����B', app_id, app_name)
                else:
                    logger.debug('[%s:%s] �i�ԓo�^���e�[�u������AI���f�����̎擾���I�����܂����B', app_id, app_name)
                    sys.exit()

                logger.debug('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊J�n�j���J�n���܂��B', app_id, app_name)
                result, conn, cur = update_fabric_info(fabric_info_column_rapid_start, start_time,
                                                       fabric_info_status_rapid_start, fabric_name, inspection_num,
                                                       cur, conn)
                if result:
                    logger.debug('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊J�n�j���I�����܂����B', app_id, app_name)
                    conn.commit()
                else:
                    logger.error('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊J�n�j�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                ### RAPID��͕�����s
                # �������擾�Ŏ擾����������ɁARAPID��͂������s������B
                # ������s�̂��߂̈������쐬����B
                tmp_ip_address = ip_address
                tmp_port_numbers = port_numbers.split(',')

                multi_param_rapid_server_host_name = []
                multi_param_worker = worker
                multi_param_port_num = []
                multi_param_product_name = []
                multi_param_fabric_name = []
                multi_param_inspection_num = []
                multi_param_rapid_starttime = []

                for i in range(len(tmp_ip_address)):
                    # �}���`�X���b�h�p�̈�����ݒ�
                    multi_param_rapid_server_host_name.append(tmp_ip_address[i])
                    multi_param_port_num.append(tmp_port_numbers)
                    multi_param_product_name.append(target_record[0])
                    multi_param_fabric_name.append(target_record[1])
                    multi_param_inspection_num.append(target_record[2])
                    multi_param_rapid_starttime.append(target_record[3])

                # �������ʃ��X�g
                result_list = []
                conn_status = str(conn)

                # ��������ɁA�q�X���b�h���쐬���āA[����ID�擾]����[NG�摜�R�s�[]���A����ID�P�ʂŕ�����s����B
                logger.debug('[%s:%s] �}���`�X���b�h���J�n���܂��B', app_id, app_name)

                # �}���`�X���b�h���s
                with ThreadPoolExecutor() as executor:
                    func_list = []
                    for i in range(thread_num):
                        # �X���b�h���s
                        func_list.append(
                            executor.submit(
                                exec_rapid_analysis_multi_thread,
                                multi_param_rapid_server_host_name[i], multi_param_worker,
                                multi_param_port_num[i],
                                multi_param_product_name[i], multi_param_fabric_name[i], multi_param_inspection_num[i],
                                multi_param_rapid_starttime[i],
                                sock[i], ai_model_name[0],
                                multi_processed_num[i], multi_ng_num[i]))
                        time.sleep(1)
                    for i in range(thread_num):
                        # �X���b�h�߂�l���擾
                        result_list.append(func_list[i].result())

                logger.info('[%s:%s] �}���`�X���b�h���I�����܂����B', app_id, app_name)

                ### �q�X���b�h�̏������ʔ���
                # �e�X���b�h�̌��ʂ��擾
                logger.debug('[%s:%s] result_list: %s', app_id, app_name, result_list)
                for i, multi_result in enumerate(result_list):
                    if multi_result[0] is True:
                        logger.debug('[%s:%s] multi_result�y�}���`�X���b�h[%d]�z:����I�����܂����B[%s]', app_id, app_name, i, str(multi_result))
                        multi_processed_num[i] = multi_result[3]
                        multi_ng_num[i] = multi_result[4]
                        close_connection(multi_result[1], multi_result[2])

                    else:
                        # �ُ�I�����Ă�����̂����݂���ꍇ
                        logger.error('[%s:%s] multi_result�y�}���`�X���b�h[%d]�z:�ُ�I�����܂����B[%s]', app_id, app_name, i, str(multi_result))
                        conn.rollback()
                        sys.exit()

                conn_str = str(conn)
                confirm_close = conn_str.split(',')[1]
                

                logger.debug('[%s:%s] ��������������J�n���܂��B', app_id, app_name)
                # DB���ʏ������Ăяo���āA���ԏ��e�[�u�����猟�������Ńf�[�^���擾����B
                logger.debug('[%s:%s] �������擾�i����/���T�C�Y���������j���J�n���܂��B', app_id, app_name)
                result, records_fabric, conn, cur = \
                    select_fabric_info_separateresize_endtime(conn, cur, fabric_name, inspection_num)

                if result:
                    logger.debug('[%s:%s] �������擾�i����/���T�C�Y���������j���I�����܂����B', app_id, app_name)
                else:
                    logger.error('[%s:%s] �������擾�i����/���T�C�Y���������j�Ɏ��s���܂����B', app_id, app_name)
                    conn.rollback()
                    sys.exit()

                if records_fabric[0] is None:
                    logger.debug('[%s:%s] �����P�ʕ����A�摜���T�C�Y���I�����Ă��܂���B', app_id, app_name)
                    logger.debug('[%s:%s] ��������������I�����܂����B', app_id, app_name)
                    if str(0) in confirm_close:
                        logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B' % (app_id, app_name))
                        close_connection(conn, cur)
                        logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B' % (app_id, app_name))
                    else:
                        pass
                    time.sleep(sleep_time)
                    break

                else:
                    # DB���ʏ������Ăяo���āA�����X�e�[�^�X�e�[�u�����猟�������Ńf�[�^���擾����B
                    logger.debug('[%s:%s] �����X�e�[�^�X���擾�iRAPID��͏��������j���J�n���܂��B', app_id, app_name)
                    result, record_processing, conn, cur = \
                        select_processing_status_rapid_end(
                            conn, cur, processing_status_rapid_end, fabric_name, inspection_num)

                    if result:
                        logger.debug('[%s:%s] �����X�e�[�^�X���擾�iRAPID��͏��������j���I�����܂����B', app_id, app_name)
                    else:
                        logger.error('[%s:%s] �����X�e�[�^�X���擾�iRAPID��͏��������j�Ɏ��s���܂����B', app_id, app_name)
                        conn.rollback()
                        sys.exit()

                    # �擾���ʔ���
                    if record_processing[0] == record_processing[1]:
                        # �����X�e�[�^�X����=�Y�������ԍ��̃��R�[�h�� ����  ����/���T�C�Y���������������Ă���ꍇ
                        # DB���ʏ������Ăяo���āA���ԏ��e�[�u�����X�V����B
                        logger.debug('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊����j���J�n���܂��B', app_id, app_name)
                        result, conn, cur = update_fabric_info(fabric_info_column_rapid_end, start_time,
                                                               fabric_info_status_rapid_end, fabric_name,
                                                               inspection_num,
                                                               cur, conn)

                        if result:
                            logger.debug('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊����j���I�����܂����B', app_id, app_name)
                        else:
                            logger.error('[%s:%s] �������e�[�u���̍X�V�iRAPID��͊����j�Ɏ��s���܂����B', app_id, app_name)
                            conn.rollback()
                            sys.exit()

                        confirm_error_flag = 0

                    else:
                        if confirm_error_flag == error_continue_num:
                            logger.error('[%s:%s] ������������G���[臒l�𒴉߂��܂����B', app_id, app_name)
                            sys.exit()
                        else:
                            confirm_error_flag += 1

                    logger.info('[%s:%s] %s����������ɏI�����܂����B:[����,�����ԍ�]=[%s,%s]', app_id, app_name, app_name, fabric_name, inspection_num)
                    # �R�~�b�g����
                    conn.commit()
                
                break


    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B', app_id, app_name)
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))

    except:

        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)
        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))
    finally:
        if conn is not None:
            # DB�ڑ��ς̍ۂ̓N���[�Y����
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���J�n���܂��B', app_id, app_name)
            close_connection(conn, cur)
            logger.debug('[%s:%s] DB�ڑ��̐ؒf���I�����܂����B', app_id, app_name)
        else:
            # DB���ڑ��̍ۂ͉������Ȃ�
            pass


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()
