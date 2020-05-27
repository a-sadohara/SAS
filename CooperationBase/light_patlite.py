# -*- coding: SJIS -*-
# ----------------------------------------
# �� ���ʋ@�\ �p�g���C�g��
# ----------------------------------------

import urllib.request
import configparser

# �ݒ�t�@�C���ǂݍ���
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/light_patlite_config.ini', 'SJIS')

# ------------------------------------------------------------------------------------
# �֐���             �F�p�g���C�g��
#
# �����T�v           �F1.�p�g���C�g��_������
#
# ����               �F�_���p�^�[��
#                      ���K�[
#                      �@�\ID
#                      �@�\��
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def light_patlite(light_pattern, logger, app_id, app_name):
    ip_address = inifile.get('PATLITE_INFO', 'ip_address')
    url = 'http://' + ip_address + '/api/control?alert=' + light_pattern
    res = urllib.request.urlopen(url)
    body = str(res.read())
    if body == 'b\'Success.\'':
        result = True
    else:
        result = False

    return result
