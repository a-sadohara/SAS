# -*- coding: SJIS -*-
# ----------------------------------------
# �� �^�p�@�\  �t�@�C���^�p�@�\
# ----------------------------------------
import configparser
import os
import datetime
import shutil
import sys
import traceback
import zipfile
import logging.config
from dateutil.relativedelta import relativedelta

import error_detail
import error_util
import file_util

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_operate_file.conf", disable_existing_loggers=False)
logger = logging.getLogger("root")

common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

common_ope_inifile = configparser.ConfigParser()
common_ope_inifile.read('D:/CI/programs/config/common_ope_config.ini', 'SJIS')

inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/operate_file_config.ini', 'SJIS')

app_id = inifile.get('APP', 'app_id')
app_name = inifile.get('APP', 'app_name')


def confirm_file(file_list, zip_name, flag, limit_day, limit_month, server):
    result = False
    try:
        flag = int(flag)
        now_date = datetime.date.today()

        for i in range(len(file_list)):

            if os.path.isfile(file_list[i]):
                ext = os.path.splitext(file_list[i])[1]
                file_name = os.path.basename(file_list[i])
                base_name = str(file_name.split('.')[0])
                file_dir = os.path.dirname(file_list[i])
                update_time = datetime.date.fromtimestamp(int(os.path.getctime(file_list[i])))
                logger.debug('file_list=%s, flag=%s, ext=%s', file_list[i], flag, ext)
                logger.info('[%s:%s] �t�@�C���m�F�������J�n���܂��B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                if not ext.endswith('.zip'):
                    if flag == 2:
                        if update_time <= limit_month:
                            logger.info('[%s:%s] �ۑ��������߂��Ă��܂��B�Ώۃt�@�C�����폜���܂��B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                            os.remove(file_list[i])
                        else:
                            logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                    elif flag != 2 and update_time <= limit_day:

                        if flag == 0:
                            logger.info(
                                '[%s:%s] �t�@�C���ۑ��������߂��Ă��܂��B�Ώۃt�@�C����ZIP���A�t�@�C���폜���J�n���܂��B [�t�@�C����=%s]' % (
                                    app_id, app_name, file_name))
                            with zipfile.ZipFile(file_dir + "\\" + base_name + "_" +
                                                 datetime.date.strftime(now_date, '%Y%m%d') + ".zip",
                                                 'w', compression=zipfile.ZIP_DEFLATED) as zip_log:
                                zip_log.write(file_list[i], arcname=file_name)
                            os.remove(file_list[i])

                        else:
                            logger.info(
                                '[%s:%s] �t�@�C���ۑ��������߂��Ă��܂��B�Ώۃt�@�C����ZIP���A�t�@�C���폜���J�n���܂��B [�t�@�C����=%s]' % (
                                    app_id, app_name, file_name))
                            if i == 0:
                                with zipfile.ZipFile(file_dir + "\\" + zip_name + "_" +
                                                     datetime.date.strftime(now_date, '%Y%m%d') + ".zip",
                                                     'w', compression=zipfile.ZIP_DEFLATED) as zip_log:
                                    zip_log.write(file_list[i], arcname=file_name)
                            else:
                                logger.debug('else')
                                with zipfile.ZipFile(file_dir + "\\" + zip_name + "_" +
                                                     datetime.date.strftime(now_date, '%Y%m%d') + ".zip",
                                                     'a', compression=zipfile.ZIP_DEFLATED) as zip_log:
                                    zip_log.write(file_list[i], arcname=file_name)
                            os.remove(file_list[i])
                    else:
                        logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�@�C����=%s]' % (app_id, app_name, file_name))

                elif update_time <= limit_month and ext.endswith('.zip'):
                    logger.info('[%s:%s] ZIP�ۑ��������߂��Ă��܂��BZIP�t�@�C���폜���J�n���܂��B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                    os.remove(file_list[i])
                else:
                    logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                    result = True

                logger.info('[%s:%s] �t�@�C���m�F�������I�����܂����B [�t�@�C����=%s]' % (app_id, app_name, file_name))
                result = True
            else:

                base, ext = os.path.splitext(file_list[i])
                name = file_list[i].split('\\')
                update_time = datetime.date.fromtimestamp(int(os.path.getctime(file_list[i])))
                logger.info('[%s:%s] �t�H���_�m�F�������J�n���܂��B [�t�H���_��=%s]' % (app_id, app_name, base))
                if flag == 1:

                    if update_time <= limit_day:
                        logger.info('[%s:%s] �ۑ��������߂��Ă��܂��B�Ώۃt�@�C����ZIP���܂��B [�t�H���_��=%s]' % (app_id, app_name, base))
                        shutil.make_archive(file_list[i], format='zip', root_dir=file_list[i] + "\\")
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�H���_��=%s]' % (app_id, app_name, base))
                        pass

                elif flag == 2 and server == 'rapid':
                    if update_time <= limit_month:
                        logger.info('[%s:%s] �ۑ��������߂��Ă��܂��B�Ώۃt�H���_���폜���܂��B [�t�H���_��=%s]' % (app_id, app_name, base))
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�H���_��=%s]' % (app_id, app_name, base))
                        pass
                elif flag == 2 and server == 'rk':
                    if update_time <= limit_day:
                        logger.info('[%s:%s] �ۑ��������߂��Ă��܂��B�Ώۃt�H���_���폜���܂��B [�t�H���_��=%s]' % (app_id, app_name, base))
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�H���_��=%s]' % (app_id, app_name, base))
                        pass
                else:
                    logger.info('[%s:%s] �Ώۂł͂���܂���B [�t�H���_��=%s]' % (app_id, app_name, base))
                    pass
                logger.info('[%s:%s] �t�H���_�m�F�������I�����܂����B [�t�H���_��=%s]' % (app_id, app_name, base))
                result = True

    except Exception as e:

        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


def execute_file(file_path, file_pattern, zip_name, flag, limit_day, limit_month, server):
    result = False
    try:
        logger.info('[%s:%s] �t�@�C�����擾���J�n���܂��B [�m�F�t�H���_=%s]'
                    % (app_id, app_name, file_path))
        tmp_result, file_list = file_util.get_file_list(file_path, file_pattern,
                                                        logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] �t�@�C�����擾���I�����܂����B [�m�F�t�H���_=%s]'
                        % (app_id, app_name, file_path))
        else:
            logger.error('[%s:%s] �t�@�C�����擾�����s���܂����B [�m�F�t�H���_=%s]'
                         % (app_id, app_name, file_path))
            return result

        logger.info('[%s:%s] �t�@�C���m�F���J�n���܂��B [�m�F�t�H���_=%s]'
                    % (app_id, app_name, file_path))
        tmp_result = confirm_file(file_list, zip_name, flag, limit_day, limit_month, server)
        if tmp_result:
            logger.info('[%s:%s] �t�@�C���m�F���I�����܂����B [�m�F�t�H���_=%s]'
                        % (app_id, app_name, file_path))
        else:
            logger.info('[%s:%s] �t�@�C���m�F�����s���܂����B [�m�F�t�H���_=%s]'
                        % (app_id, app_name, file_path))
            return result

        result = True
    except Exception as e:
        # ���s���͋��ʗ�O�֐��ŃG���[�ڍׂ����O�o�͂���
        error_detail.exception(e, logger, app_id, app_name)

    return result


def main():
    error_file_name = None

    try:
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        rapid_ip_address = common_inifile.get('RAPID_SERVER', 'ip_address').split(',')
        remove_target = inifile.get('TARGET_VALUE', 'target').split('|')

        logger.info('[%s:%s] %s���N�����܂��B' % (app_id, app_name, app_name))

        # �ʒm�t�@�C���m�F

        for target in remove_target:
            target = target.split(',')
            file_path = target[0]
            file_pattern = target[1]
            zip_name = target[2]
            flag = str(target[3])
            server = target[4]
            now_date = datetime.date.today()
            limit_day = now_date - datetime.timedelta(int(target[5]))
            limit_month = now_date - relativedelta(months=int(target[6]))

            if server == 'rapid':
                if 'NG' in file_path or 'NOTICE' in file_path:
                    rapid_ip_address = [rapid_ip_address[0]]
                else:
                    pass
                for ip_address in rapid_ip_address:
                    path = "\\\\" + ip_address + "\\" + file_path

                    logger.info('[%s:%s] %s�������J�n���܂��B [�m�F�t�H���_=%s]'
                                % (app_id, app_name, app_name, target[0]))
                    result = execute_file(path, file_pattern, zip_name, flag, limit_day,
                                          limit_month, server)
                    if result:
                        logger.info('[%s:%s] %s�������I�����܂����B [�m�F�t�H���_=%s]'
                                    % (app_id, app_name, app_name, path))
                    else:
                        logger.error('[%s:%s] %s���������s���܂����B [�m�F�t�H���_=%s]'
                                     % (app_id, app_name, app_name, path))
                        sys.exit()
            else:
                logger.info('[%s:%s] %s�������J�n���܂��B [�m�F�t�H���_=%s]'
                            % (app_id, app_name, app_name, target[0]))
                result = execute_file(file_path, file_pattern, zip_name, flag, limit_day,
                                      limit_month, server)
                if result:
                    logger.info('[%s:%s] %s�������I�����܂����B [�m�F�t�H���_=%s]'
                                % (app_id, app_name, app_name, target[0]))
                else:
                    logger.error('[%s:%s] %s���������s���܂����B [�m�F�t�H���_=%s]'
                                 % (app_id, app_name, app_name, target[0]))
                    sys.exit()


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
        logger.debug('[%s:%s] �G���[�����ʏ������s���J�n���܂��B' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)

        if result:
            logger.debug('[%s:%s] �G���[�����ʏ������s���I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �G���[�����ʏ������s�����s���܂����B' % (app_id, app_name))
            logger.error('[%s:%s] �C�x���g���O���m�F���Ă��������B' % (app_id, app_name))


if __name__ == "__main__":  # ���̃p�C�\���t�@�C�����Ŏ��s�����ꍇ
    main()
