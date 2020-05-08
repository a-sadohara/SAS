# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 運用機能  ファイル運用機能
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

# ログ設定
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
                logger.info('[%s:%s] ファイル確認処理を開始します。 [ファイル名=%s]' % (app_id, app_name, file_name))
                if not ext.endswith('.zip'):
                    if flag == 2:
                        if update_time <= limit_month:
                            logger.info('[%s:%s] 保存期限を過ぎています。対象ファイルを削除します。 [ファイル名=%s]' % (app_id, app_name, file_name))
                            os.remove(file_list[i])
                        else:
                            logger.info('[%s:%s] 対象ではありません。 [ファイル名=%s]' % (app_id, app_name, file_name))
                    elif flag != 2 and update_time <= limit_day:

                        if flag == 0:
                            logger.info(
                                '[%s:%s] ファイル保存期限を過ぎています。対象ファイルをZIPし、ファイル削除を開始します。 [ファイル名=%s]' % (
                                    app_id, app_name, file_name))
                            with zipfile.ZipFile(file_dir + "\\" + base_name + "_" +
                                                 datetime.date.strftime(now_date, '%Y%m%d') + ".zip",
                                                 'w', compression=zipfile.ZIP_DEFLATED) as zip_log:
                                zip_log.write(file_list[i], arcname=file_name)
                            os.remove(file_list[i])

                        else:
                            logger.info(
                                '[%s:%s] ファイル保存期限を過ぎています。対象ファイルをZIPし、ファイル削除を開始します。 [ファイル名=%s]' % (
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
                        logger.info('[%s:%s] 対象ではありません。 [ファイル名=%s]' % (app_id, app_name, file_name))

                elif update_time <= limit_month and ext.endswith('.zip'):
                    logger.info('[%s:%s] ZIP保存期限を過ぎています。ZIPファイル削除を開始します。 [ファイル名=%s]' % (app_id, app_name, file_name))
                    os.remove(file_list[i])
                else:
                    logger.info('[%s:%s] 対象ではありません。 [ファイル名=%s]' % (app_id, app_name, file_name))
                    result = True

                logger.info('[%s:%s] ファイル確認処理が終了しました。 [ファイル名=%s]' % (app_id, app_name, file_name))
                result = True
            else:

                base, ext = os.path.splitext(file_list[i])
                name = file_list[i].split('\\')
                update_time = datetime.date.fromtimestamp(int(os.path.getctime(file_list[i])))
                logger.info('[%s:%s] フォルダ確認処理を開始します。 [フォルダ名=%s]' % (app_id, app_name, base))
                if flag == 1:

                    if update_time <= limit_day:
                        logger.info('[%s:%s] 保存期限を過ぎています。対象ファイルをZIPします。 [フォルダ名=%s]' % (app_id, app_name, base))
                        shutil.make_archive(file_list[i], format='zip', root_dir=file_list[i] + "\\")
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] 対象ではありません。 [フォルダ名=%s]' % (app_id, app_name, base))
                        pass

                elif flag == 2 and server == 'rapid':
                    if update_time <= limit_month:
                        logger.info('[%s:%s] 保存期限を過ぎています。対象フォルダを削除します。 [フォルダ名=%s]' % (app_id, app_name, base))
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] 対象ではありません。 [フォルダ名=%s]' % (app_id, app_name, base))
                        pass
                elif flag == 2 and server == 'rk':
                    if update_time <= limit_day:
                        logger.info('[%s:%s] 保存期限を過ぎています。対象フォルダを削除します。 [フォルダ名=%s]' % (app_id, app_name, base))
                        shutil.rmtree(file_list[i] + "\\")
                    else:
                        logger.info('[%s:%s] 対象ではありません。 [フォルダ名=%s]' % (app_id, app_name, base))
                        pass
                else:
                    logger.info('[%s:%s] 対象ではありません。 [フォルダ名=%s]' % (app_id, app_name, base))
                    pass
                logger.info('[%s:%s] フォルダ確認処理が終了しました。 [フォルダ名=%s]' % (app_id, app_name, base))
                result = True

    except Exception as e:

        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result


def execute_file(file_path, file_pattern, zip_name, flag, limit_day, limit_month, server):
    result = False
    try:
        logger.info('[%s:%s] ファイル情報取得を開始します。 [確認フォルダ=%s]'
                    % (app_id, app_name, file_path))
        tmp_result, file_list = file_util.get_file_list(file_path, file_pattern,
                                                        logger, app_id, app_name)
        if tmp_result:
            logger.info('[%s:%s] ファイル情報取得が終了しました。 [確認フォルダ=%s]'
                        % (app_id, app_name, file_path))
        else:
            logger.error('[%s:%s] ファイル情報取得が失敗しました。 [確認フォルダ=%s]'
                         % (app_id, app_name, file_path))
            return result

        logger.info('[%s:%s] ファイル確認を開始します。 [確認フォルダ=%s]'
                    % (app_id, app_name, file_path))
        tmp_result = confirm_file(file_list, zip_name, flag, limit_day, limit_month, server)
        if tmp_result:
            logger.info('[%s:%s] ファイル確認が終了しました。 [確認フォルダ=%s]'
                        % (app_id, app_name, file_path))
        else:
            logger.info('[%s:%s] ファイル確認が失敗しました。 [確認フォルダ=%s]'
                        % (app_id, app_name, file_path))
            return result

        result = True
    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return result


def main():
    error_file_name = None

    try:
        error_file_name = inifile.get('ERROR_FILE', 'error_file_name')
        rapid_ip_address = common_inifile.get('RAPID_SERVER', 'ip_address').split(',')
        remove_target = inifile.get('TARGET_VALUE', 'target').split('|')

        logger.info('[%s:%s] %sを起動します。' % (app_id, app_name, app_name))

        # 通知ファイル確認

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

                    logger.info('[%s:%s] %s処理を開始します。 [確認フォルダ=%s]'
                                % (app_id, app_name, app_name, target[0]))
                    result = execute_file(path, file_pattern, zip_name, flag, limit_day,
                                          limit_month, server)
                    if result:
                        logger.info('[%s:%s] %s処理が終了しました。 [確認フォルダ=%s]'
                                    % (app_id, app_name, app_name, path))
                    else:
                        logger.error('[%s:%s] %s処理が失敗しました。 [確認フォルダ=%s]'
                                     % (app_id, app_name, app_name, path))
                        sys.exit()
            else:
                logger.info('[%s:%s] %s処理を開始します。 [確認フォルダ=%s]'
                            % (app_id, app_name, app_name, target[0]))
                result = execute_file(file_path, file_pattern, zip_name, flag, limit_day,
                                      limit_month, server)
                if result:
                    logger.info('[%s:%s] %s処理が終了しました。 [確認フォルダ=%s]'
                                % (app_id, app_name, app_name, target[0]))
                else:
                    logger.error('[%s:%s] %s処理が失敗しました。 [確認フォルダ=%s]'
                                 % (app_id, app_name, app_name, target[0]))
                    sys.exit()


    except SystemExit:
        # sys.exit()実行時の例外処理
        logger.debug('[%s:%s] sys.exit()によりプログラムを終了します。', app_id, app_name)
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。', app_id, app_name)
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)

        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))

    except:
        logger.error('[%s:%s] 予期しないエラーが発生しました。[%s]' % (app_id, app_name, traceback.format_exc()))
        logger.debug('[%s:%s] エラー時共通処理実行を開始します。' % (app_id, app_name))
        result = error_util.common_execute(error_file_name, logger, app_id, app_name)

        if result:
            logger.debug('[%s:%s] エラー時共通処理実行を終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] エラー時共通処理実行が失敗しました。' % (app_id, app_name))
            logger.error('[%s:%s] イベントログを確認してください。' % (app_id, app_name))


if __name__ == "__main__":  # このパイソンファイル名で実行した場合
    main()
