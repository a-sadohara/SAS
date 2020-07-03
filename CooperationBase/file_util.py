# -*- coding: SJIS -*-
# ----------------------------------------
# ��   �t�@�C�����실�ʋ@�\
# ----------------------------------------
import datetime
import configparser
import glob
import shutil
import codecs
import os
import cv2
import numpy as np
from pathlib import Path

import error_detail

#  ���ʐݒ�t�@�C���Ǎ���
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')
#  ���ʐݒ�t�@�C���Ǎ���
file_inifile = configparser.ConfigParser()
file_inifile.read('D:/CI/programs/config/file_util.ini', 'SJIS')

setting_loop_num = int(common_inifile.get('ERROR_RETRY', 'util_num'))
flag = file_inifile.get('CSV', 'flag')
label_ng = file_inifile.get('CSV', 'label_ng')
label_others = file_inifile.get('CSV', 'label_others')
networkpath_error = file_inifile.get('ERROR_INFO', 'networkpath_error')


# ------------------------------------------------------------------------------------
# ������             �F�t�@�C�����X�g�擾
#
# �����T�v           �F1.�t�H���_�Ɋi�[����Ă���t�@�C���̃��X�g���擾����B
#                      2.�l�b�g���[�N�p�X�G���[�̏ꍇ�̓t�H���_�ւ̍Đڑ��𕡐���s���B
#
# ����               �F�i�[�t�H���_�p�X
#                      �t�@�C�����X�g
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �t�@�C�����X�g
# ------------------------------------------------------------------------------------
def get_file_list(file_path, file_patern, logger, app_id, app_name):
    result = False
    file_list = None
    e = None
    for x in range(setting_loop_num):
        try:
            if os.path.exists(file_path):
                file_list = glob.glob(file_path + file_patern)
                # ��������=True
                result = True
                return result, file_list, e
            else:
                os.listdir(file_path)
                continue
        except Exception as e:
            # �G���[�ڍה�����s��

            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, file_list, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, file_list, e


# ------------------------------------------------------------------------------------
# ������             �F�t�@�C���ړ�
#
# �����T�v           �F1.�t�@�C���̈ړ����s���B
#                      2.�l�b�g���[�N�p�X�G���[�̏ꍇ�̓t�H���_�ւ̍Đڑ��𕡐���s���B
#
# ����               �F�ړ����t�@�C���p�X
#                      �ړ���t�@�C���p�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#                      �p�[�~�b�V�����G���[���b�Z�[�W
# ------------------------------------------------------------------------------------
def move_file(file_path, output_path, logger, app_id, app_name):
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            file_name = os.path.basename(file_path)
            base_dir = os.path.dirname(file_path)
            if os.path.exists(file_path):
                if os.path.exists(output_path + '\\' + file_name):
                    # ���݂���ꍇ�A�ޔ��t�@�C���ɕʖ��i���j������B
                    # �i���j�ޔ��t�@�C���̍ŏI�X�V�����iYYYYMMDD_HHMMSS�j���A�t�@�C�����̖����ɕt�^����B
                    # xxxx.csv �� xxxx.csv.20200101_235959
                    timestamp = datetime.datetime.now()
                    file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
                    target_file_name = file_name + '.' + file_timestamp
                    # �t�@�C�����ړ�����
                    shutil.move(file_path, output_path + "\\" + target_file_name)
                else:
                    # ���݂��Ȃ��ꍇ�A�t�@�C�����͂��̂܂܃t�@�C�����ړ�����
                    shutil.move(file_path, output_path + "\\" + file_name)
                # ��������=True
                result = True
                return result, e
            else:
                os.listdir(base_dir)
                continue
        except Exception as e:
            # �G���[�ڍה�����s��
            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# ������             �F�t�@�C���R�s�[
#
# �����T�v           �F1.�t�@�C���̃R�s�[���s���B
#                      2.�l�b�g���[�N�p�X�G���[�̏ꍇ�̓t�H���_�ւ̍Đڑ��𕡐���s���B
#
# ����               �F�R�s�[���t�@�C���p�X
#                      �R�s�[��t�@�C���p�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def copy_file(file_path, output_path, logger, app_id, app_name):
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            file_name = os.path.basename(file_path)
            base_dir = os.path.dirname(file_path)
            if os.path.exists(file_path):
                if os.path.exists(output_path + '\\' + file_name):
                    # ���݂���ꍇ�A�ޔ��t�@�C���ɕʖ��i���j������B
                    # �i���j�ޔ��t�@�C���̍ŏI�X�V�����iYYYYMMDD_HHMMSS�j���A�t�@�C�����̖����ɕt�^����B
                    # xxxx.csv �� xxxx.csv.20200101_235959
                    timestamp = datetime.datetime.now()
                    file_timestamp = timestamp.strftime("%Y%m%d_%H%M%S")
                    target_file_name = file_name + '.' + file_timestamp
                    # �t�@�C�����ړ�����
                    shutil.copy2(file_path, output_path + "\\" + target_file_name)
                else:
                    # ���݂��Ȃ��ꍇ�A�t�@�C�����͂��̂܂܃t�@�C�����ړ�����
                    shutil.copy2(file_path, output_path)
                # ��������=True
                result = True
                return result, e
            else:
                os.listdir(base_dir)
                continue
        except Exception as e:
            # �G���[�ڍה�����s��
            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, e

# ------------------------------------------------------------------------------------
# ������             �F�f�B���N�g���쐬
#
# �����T�v           �F1.�f�B���N�g���쐬���s���B
#                      2.�l�b�g���[�N�p�X�G���[�̏ꍇ�̓t�H���_�ւ̍Đڑ��𕡐���s���B
#
# ����               �F�f�B���N�g���쐬�Ώۃp�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F �������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def make_directory(file_path, logger, app_id, app_name):
    # �ϐ��ݒ�
    result = False
    e = None
    for x in range(setting_loop_num):
        try:
            # �f�B���N�g���쐬
            os.makedirs(file_path, exist_ok=True)
            # ��������=True
            result = True
            return result, e
        except Exception as e:
            # �G���[�ڍה�����s��
            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# ������             �F�摜�Ǎ�
#
# �����T�v           �F1.�摜��Ǎ���ŁA�Ǎ��񂾃f�[�^��ԋp����B
#
#
# ����               �F�摜�p�X
#                      �Ǎ��t���O
#                      �f�[�^�^
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�摜�f�[�^
#                      �������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def read_image(filename, logger, app_id, app_name, flags=cv2.IMREAD_COLOR, dtype=np.uint8):
    result = False
    x = 0
    e = None
    try:
        n = np.fromfile(filename, dtype)
        img = cv2.imdecode(n, flags)
        result = True
        return result, img, e

    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        img = None
        return result, img, e


# ------------------------------------------------------------------------------------
# ������             �F���ʃt�@�C���Ǎ�
#
# �����T�v           �F1.�A�g���ꂽ�t�@�C����Ǎ���ŁA�f�[�^��ԋp����B
#
#
# ����               �F�Ǎ��Ώۃt�@�C��
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�Ǎ�����
#                      �������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
# RAPID�̌��ʃt�@�C����ǂݍ���
def read_result_file(result_file, logger, app_id, app_name):
    x = 0
    result_data = {}
    e = None
    try:
        LABEL_NAME = [label_ng, label_others]
        fp = codecs.open(result_file, "r", "SJIS")
        line = fp.readline()
        if line.find("[DataPath]") != -1:
            itemList = line[:-1].split("[DataPath]")[1]
            datapath = itemList.split("\"")[1]
            result_data["datapath"] = datapath
            line = fp.readline()
        else:
            result_data["datapath"] = ""
        datas = []
        while line:
            # ���ʃt�@�C���̃t�H�[�}�b�g
            # 3,"<�t�@�C����>",<X���W>,<Y���W>,<��>,<����>,<���x��>,<�m�M�x>�E�E�E
            # ��������
            # "<�t�@�C����>",<X���W>,<Y���W>,<��>,<����>,<���x��>,<�m�M�x>�E�E�E
            data = {}
            if line.find("[") != -1:
                line = fp.readline()
                continue
            itemList = line.strip().split(",")
            idx = 0
            if itemList[0] == flag:
                idx = 1
            data["filename"] = itemList[idx].split("\"")[1]
            point = {}
            point["x"] = int(itemList[idx + 1])
            point["y"] = int(itemList[idx + 2])
            data["point"] = point
            data["width"] = int(itemList[idx + 3])
            data["height"] = int(itemList[idx + 4])
            data["label"] = itemList[idx + 5]
            rate = {}
            for cnt in range(0, len(LABEL_NAME)):
                rate[itemList[idx + 5 + (2 * cnt)]] = float(itemList[idx + 6 + (2 * cnt)])
            data["rate"] = rate
            data["duplication"] = 1
            datas.append(data)
            line = fp.readline()
        result_data["data"] = datas
        fp.close()
        result = True
        return result, result_data, e

    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result, result_data, e


# ------------------------------------------------------------------------------------
# ������             �F���ʃt�@�C���o��
#
# �����T�v           �F1.�A�g���ꂽ�f�[�^�𐮌`����CSV�Ƃ��ďo�͂���B
#
# ����               �F�f�B���N�g���쐬�Ώۃp�X
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F �������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
# RAPID�̌��ʃt�@�C���̃t�H�[�}�b�g�ŏo��
def write_result_file(output_file, result_data, datapath, logger, app_id, app_name):
    result = False
    x = 0
    e = None
    try:
        fp = codecs.open(output_file, "w", "SJIS")
        if result_data["datapath"] == "":
            fp.write("[DataPath]\"" + result_data["datapath"] + "\"" + "\n")
        else:
            fp.write("[DataPath]\"" + datapath + "\"" + "\n")
        for result in result_data["data"]:
            buf = "3"
            buf = buf + "," + "\"" + result["filename"] + "\""
            buf = buf + "," + str(result["point"]["x"])
            buf = buf + "," + str(result["point"]["y"])
            buf = buf + "," + str(result["width"])
            buf = buf + "," + str(result["height"])
            if result["label"] != label_ng:
                buf = buf + "," + label_others
                buf = buf + "," + str(result["rate"]["_others"])
                buf = buf + "," + label_ng
                buf = buf + "," + "0"
            else:
                buf = buf + "," + label_ng
                buf = buf + "," + str(result["rate"]["NG"])
                buf = buf + "," + label_others
                buf = buf + "," + "0"
            fp.write(buf + "\n")
        fp.close()
        result = True
        return result, e
    except Exception as e:
        result = error_detail.exception(e, logger, app_id, app_name)
        return result, e


# ------------------------------------------------------------------------------------
# ������             �F�p�g���C�g�_��
#
# �����T�v           �F1.�p�g���C�g�_���̍ۂɎw��̃t�H���_�ɒʒm���o�͂���B
#
# ����               �F�f�B���N�g���p�X
#                      �t�@�C����
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F �������ʁiTrue:�����AFalse:���s�j
#
# ------------------------------------------------------------------------------------
def light_patlite(name, logger, app_id, app_name):
    patlite_path = common_inifile.get('FILE_PATH', 'patlite_path')
    e = None
    for x in range(setting_loop_num):
        try:
            if os.path.exists(patlite_path):
                Path(patlite_path + '\\' +  name).touch()
                # ��������=True
                result = True
                return result, e
            else:
                os.listdir(patlite_path)
                continue
        except Exception as e:
            # �G���[�ڍה�����s��
            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, e


# ------------------------------------------------------------------------------------
# �֐���             �F�B���摜�t�H���_�폜
#
# �����T�v           �F1.��̃t�H���_���폜����B
#
# ����               �F�폜�p�X
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def delete_dir(path, logger, app_id, app_name):
    e = None
    for x in range(setting_loop_num):
        try:
            shutil.rmtree(path)
            result = True
            return result, e
        except Exception as e:
            # �G���[�ڍה�����s��
            result = error_detail.exception(e, logger, app_id, app_name)
            # �Ď��s�񐔖����̏ꍇ
            if x < (setting_loop_num - 1):
                # �G���[���茋��=True�̏ꍇ�A �l�b�g���[�N�p�X�G���[�̂��ߍĎ��s
                if result == networkpath_error:
                    continue
                # �G���[���茋��=False�̏ꍇ�A��������=False
                else:
                    return result, e
            # �Ď��s�񐔈ȏ�̏ꍇ�A�G���[����B��������=False
            else:
                return result, e
