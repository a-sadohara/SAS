# -*- coding: SJIS -*-
# �}�X�^����`�t�@�C���쐬�@�\
#
import configparser
import csv
import glob
import re
import sys
import logging.config
import traceback

import error_detail

# ���O�ݒ�
import error_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_create_master_data.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

# �ݒ�t�@�C���ǂݍ��݁i���ʐݒ�j
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/create_master_data_config.ini', 'SJIS')

# ���O�Ɏg�p����@�\ID,�@�\�����擾����
app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �Fcsv�t�@�C���Ǎ�
#
# �����T�v           �F1.臒l�����쐬����i������SV�t�@�C������Ǎ���
#
# ����               �FCSV�t�@�C���p�X
#
# �߂�l             �F�Ǎ�����
# ------------------------------------------------------------------------------------
def read_csv(csv_file):
    read_csv_line = None
    try:
        with open(csv_file, 'r', encoding='shift_jis') as f:
            reader = csv.reader(f)
            read_csv_line = [row for row in reader]
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, read_csv_line


# ------------------------------------------------------------------------------------
# ������             �Fini�t�@�C���Ǎ�
#
# �����T�v           �F1.�e�i�Ԃ�ini�t�@�C����Ǎ���
#
# ����               �Fini�t�@�C���p�X
#
# �߂�l             �F�Ǎ�����
# ------------------------------------------------------------------------------------
def readfile(filename):
    config_data = None
    try:
        file = open(filename)
        config_data = [line.strip() for line in file.readlines()]
        file.close()
        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, config_data


# ------------------------------------------------------------------------------------
# ������             �F�񐔌v�Z
#
# �����T�v           �F1.ini�t�@�C���̃f�[�^����A�񐔂��v�Z����
#
# ����               �Fini�t�@�C���Ǎ�����
#
# �߂�l             �F�Ǎ�����
# ------------------------------------------------------------------------------------
def column_num_calc(airbag_data):
    column_num = None
    try:
        vertex_num = [num for num in airbag_data if 'Number=' in num]
        # Number=0�̂��̂͑Ώۗ񂪑��݂��Ȃ��i��
        column_num = len([column for column in vertex_num if 'Number=0' not in column])
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False
    return result, column_num


# ------------------------------------------------------------------------------------
# ������             �F���W�擾
#
# �����T�v           �F1.�񐔂�ini�t�@�C������臒l�̍��W���擾����
#
# ����               �F��
#                      ini�t�@�C���p�X
#
# �߂�l             �F���W���X�g
# ------------------------------------------------------------------------------------
def get_coordinate(colum_num, airbag_ini_file):
    coords = None
    try:
        coord_inifile = configparser.ConfigParser()
        coord_inifile.read(airbag_ini_file, 'SJIS')

        coords = []
        for colum in range(0, colum_num):
            one_colum_coords = []
            section = 'AIRBAG00' + str(colum)
            for key in coord_inifile.options(section):
                if ('coord' in key):
                    coord = coord_inifile.get(section, key)
                    one_colum_coords.append("(" + coord.replace(' ', ',') + ")")

            coords.append(one_colum_coords)
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, coords


# ------------------------------------------------------------------------------------
# ������             �F�s臒l�Z�o
#
# �����T�v           �F1.ini�t�@�C������擾����臒l���W����A�s臒l���Z�o����B
#
# ����               �F���W���X�g
#                      ��
#
# �߂�l             �F�s臒l���X�g
# ------------------------------------------------------------------------------------
def get_x_threshold(coord_list, colum_num):
    x_threshold = []
    x_threshold_list = []
    try:

        for i in range(colum_num):
            coords = []
            for j in range(len(coord_list[i])):
                sp_coord_list = (re.sub('[()]', '', coord_list[i][j])).split(',')
                coords.append(sp_coord_list)

            x_coords = []
            for k in range(len(coords)):
                x_coords.append(int(coords[k][0]))

            x_min = min([int(x_min_coords) for x_min_coords in x_coords if 0 < int(x_min_coords)])
            x_max = max(x_coords)
            x = x_min, x_max
            x_threshold.append(x)

        x_list = []
        limit_column = 5
        for l in range(len(x_threshold)):
            str(x_threshold[l]).split(',')
            x_list.append(re.sub('[()| ]', '', str(x_threshold[l])).split(','))

        for n in range(limit_column):

            if n <= (len(x_list) - 1):
                x_threshold_list.append(int(x_list[n][0]))
                x_threshold_list.append(int(x_list[n][1]))
            else:
                no_th = ','
                x_threshold_list.append(no_th)
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, x_threshold_list


# ------------------------------------------------------------------------------------
# ������             �F��臒l�Z�o
#
# �����T�v           �F1.ini�t�@�C������擾����臒l���W����A��臒l���Z�o����B
#
# ����               �F���W���X�g
#                      ��
#
# �߂�l             �F��臒l���X�g
# ------------------------------------------------------------------------------------
def get_y_coordinate(coord_list, colum_num):
    y_threshold = []
    y_threshold_list = []
    try:
        for i in range(colum_num):
            coords = []
            for j in range(len(coord_list[i])):
                sp_coord_list = (re.sub('[()]', '', coord_list[i][j])).split(',')
                coords.append(sp_coord_list)

            y_coords = []
            for k in range(len(coords)):
                y_coords.append(int(coords[k][1]))

            y_min = min([y_min_coords for y_min_coords in y_coords if 0 < int(y_min_coords)])
            y_max = max([y_max_coords for y_max_coords in y_coords if 0 < int(y_max_coords)])
            y = y_min, y_max
            y_threshold.append(y)

        y_list = []

        for l in range(0, colum_num):
            str(y_threshold[l]).split(',')
            y_list.append(re.sub('[()| ]', '', str(y_threshold[l])).split(','))

        for n in range(0, colum_num - 1):
            y_th = round(int(y_list[n][1]) + (int(y_list[n + 1][0]) - int(y_list[n][1])) / 2)
            y_threshold_list.append(y_th)

        no_th = ''
        if 5 - int(colum_num) > 0:
            for m in range(5 - int(colum_num)):
                y_threshold_list.append(no_th)
        result = True

    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result, y_threshold_list


# ------------------------------------------------------------------------------------
# ������             �F臒l���CSV�쐬
#
# �����T�v           �F1.�擾����臒l����CSV�ŏo�͂���B
#
# ����               �F臒l���
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def create_master_data_file(output_file_path, master_data):
    try:
        master_file_path = output_file_path + "\\" + "臒l���.CSV"

        master_file = open(master_file_path, 'a')

        master_file.write(
            '�i��,�B���J������,��臒l01,��臒l02,��臒l03,��臒l04,�s臒lA1,�s臒lA2,�s臒lB1,�s臒lB2,�s臒lC1,�s臒lC2,�s臒lD1,�s臒lD2,�s臒lE1,�s臒lE2,AI���f����')
        master_file.write("\n")

        for i in range(len(master_data)):
            master_file.write(master_data[i][0])
            master_file.write(master_data[i][1])
            master_file.write(master_data[i][2])
            master_file.write(master_data[i][3])
            master_file.write(master_data[i][4])
            master_file.write("\n")
        master_file.close()
        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)
        result = False

    return result


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F臒l�����쐬����
#
# ����               �Fini�t�@�C���i�[�p�X
#                      �C���v�b�gCSV�t�@�C���i�[�p�X
#
# �߂�l             �F�Ȃ�
# ------------------------------------------------------------------------------------
def main(file_path, csv_file, output_file_path):
    error_file_name = None
    result = False

    try:
        ## �ݒ�t�@�C������l���擾
        # �G���[�t�@�C����
        error_file_name = inifile.get('ERROR_FILE', 'error_filename')

        # �ϐ��ݒ�
        file_list = []
        master_data = []
        product_name = []
        camera_num = ',27'

        logger.info('[%s:%s] %s�������N�����܂��B' % (app_id, app_name, app_name))

        logger.debug('[%s:%s] �C���v�b�gCSV�t�@�C���̓Ǎ����J�n���܂��B' % (app_id, app_name))
        tmp_result, read_result_list = read_csv(csv_file)
        if tmp_result:
            logger.debug('[%s:%s] �C���v�b�gCSV�t�@�C���̓Ǎ����I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �C���v�b�gCSV�t�@�C���̓Ǎ������s���܂����B' % (app_id, app_name))
            sys.exit()

        logger.info('[%s:%s] %s�������J�n���܂��B' % (app_id, app_name, app_name))
        for i in range(len(read_result_list)):
            file_pattern = 'AirBagCoord' + str(read_result_list[i][0]) + '.ini'

            file_list.append(glob.glob(file_path + '\\' + file_pattern)[0])
            product_name.append(read_result_list[i][1])

        for i in range(len(file_list)):
            logger.debug('[%s:%s] �Ώۃ}�X�^ [�i��=%s]' % (app_id, app_name, read_result_list[i][1]))

            master_data_list = [product_name[i], camera_num]

            logger.debug('[%s:%s] �}�X�^�t�@�C���Ǎ����J�n���܂��B' % (app_id, app_name))
            tmp_result, airbag_data = readfile(file_list[i])
            if tmp_result:
                logger.debug('[%s:%s] �}�X�^�t�@�C���Ǎ����I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] �}�X�^�t�@�C���Ǎ������s���܂����B' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] �񐔌v�Z���J�n���܂��B' % (app_id, app_name))
            # AirBagCoord.ini����񐔂��v�Z����B
            tmp_result, column_num = column_num_calc(airbag_data)
            if tmp_result:
                logger.debug('[%s:%s] �񐔌v�Z���I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] �񐔌v�Z�����s���܂����B' % (app_id, app_name))
                sys.exit()

            logger.debug('[%s:%s] ���W�擾���J�n���܂��B' % (app_id, app_name))
            # AirBagCoord.ini����e��̍��W�����X�g�Ɋi�[����B
            tmp_result, coord_list = get_coordinate(column_num, file_list[i])
            if tmp_result:
                logger.debug('[%s:%s] ���W�擾���I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] ���W�擾�����s���܂����B' % (app_id, app_name))
                sys.exit()

            # coord_list�ɂ�[,],'���܂܂��̂Œu������B
            re_coord_list = []
            for replace in range(len(coord_list)):
                re_coord_list.append(re.sub('[|]|\'', '', str(coord_list[replace])))

            # �s臒l�Z�o
            logger.debug('[%s:%s] �s臒l�Z�o���J�n���܂��B' % (app_id, app_name))
            tmp_result, x_threshold_list = get_x_threshold(coord_list, column_num)
            if tmp_result:
                logger.debug('[%s:%s] �s臒l�Z�o���I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] �s臒l�Z�o�����s���܂����B' % (app_id, app_name))
                sys.exit()

            # ��臒l�Z�o
            logger.debug('[%s:%s] ��臒l�Z�o���J�n���܂��B' % (app_id, app_name))
            tmp_result, y_threshold_list = get_y_coordinate(coord_list, column_num)
            if tmp_result:
                logger.debug('[%s:%s] ��臒l�Z�o���I�����܂����B' % (app_id, app_name))
            else:
                logger.error('[%s:%s] ��臒l�Z�o�����s���܂����B' % (app_id, app_name))
                sys.exit()

            base_y = ',' + str(y_threshold_list)
            logger.debug('[%s:%s] 臒l���Y���W %s' % (app_id, app_name, base_y))
            y_threshold = re.sub('[][\'| ]', '', base_y)
            logger.debug('[%s:%s] 臒l���Y���W %s' % (app_id, app_name, y_threshold))
            master_data_list.append(y_threshold)

            base_x = ',' + str(x_threshold_list)
            x_threshold = re.sub('[][\'| ]', '', base_x)
            master_data_list.append(x_threshold)

            model_name = ',' + read_result_list[i][2]
            master_data_list.append(model_name)
            master_data.append(master_data_list)

        logger.debug('[%s:%s] 臒l��� %s' % (app_id, app_name, master_data))

        logger.debug('[%s:%s] 臒l���CSV�쐬���J�n���܂��B' % (app_id, app_name))
        tmp_result = create_master_data_file(output_file_path, master_data)
        if tmp_result:
            logger.debug('[%s:%s] 臒l���CSV�쐬���I�����܂����B' % (app_id, app_name))
            logger.info('[%s:%s] %s�����͐���ɏI�����܂����B' % (app_id, app_name, app_name))
        else:
            logger.error('[%s:%s] 臒l���CSV�쐬�����s���܂����B' % (app_id, app_name))
            sys.exit()

        result = True

    except SystemExit:
        # sys.exit()���s���̗�O����
        logger.debug('[%s:%s] sys.exit()�ɂ��v���O�������I�����܂��B' % (app_id, app_name))
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B' % (app_id, app_name))

    except:
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B', app_id, app_name)

    # �߂�l�ݒ�
    # �ďo���i�o�b�`�v���O�������j�Ŗ߂�l����iERRORLEVEL�j����ۂ̖߂�l��ݒ肷��B
    if result:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":
    ini_file_path = None
    csv_file_path = None
    output_file_path = None

    args = sys.argv

    if len(args) > 3:
        ini_file_path = args[1]
        csv_file_path = args[2]
        output_file_path = args[3]

    main(ini_file_path, csv_file_path, output_file_path)
