# -*- coding: SJIS -*-
# ----------------------------------------
# ■機能310  NG画像圧縮・転送機能共通機能
# ----------------------------------------
import logging.config
import os
import shutil
import re
import sys
from pathlib import Path

import error_detail
import configparser
import file_util
# ADD 20200701 KQRM 下吉 START
import math
# ADD 20200701 KQRM 下吉 END

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_compress_image.conf", disable_existing_loggers=False)
logger = logging.getLogger("compress_image")

# 設定ファイル読み込み（共通設定）
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/compress_image_config.ini', 'SJIS')

# ログに使用する機能ID,機能名を取得する
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：フォルダ存在チェック
#
# 処理概要           ：1.フォルダが存在するかチェックする。
#                    2.フォルダが存在しない場合は作成する。
#
# 引数               ：フォルダパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exists_dir(target_path, logger):
    func_name = sys._getframe().f_code.co_name
    logger.debug('[%s:%s] フォルダを作成します。フォルダ名：[%s]',
                 app_id, app_name, target_path, logger)
    result, error = file_util.make_directory(target_path, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG画像圧縮・転送機能実行
#
# 処理概要           ：1.NG画像圧縮・転送を行う。
#
# 引数               ： 品名
#                      反番
#                      検査番号
#                      撮像開始時刻
#                      NG画像ルートパス
#                      圧縮対象NG画像パス（未検知画像フラグが"未検知画像"の場合のみ指定。それ以外はNoneを指定する）
#                      未検知画像フラグ（1:未検知画像である、それ以外:未検知画像でない）
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def exec_compress_and_transfer(
        product_name, fabric_name, inspection_num, imaging_starttime,
        image_root_path,
        archive_ng_image_file_path,
        undetected_image_flag, logger):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        ### 設定ファイルからの値取得
        # 連携基盤のルートディレクトリ
        rk_root_path = common_inifile.get('FILE_PATH', 'rk_path')

        # 圧縮NG画像送信先ホスト名
        send_hostname = common_inifile.get('RAPID_SERVER', 'host_name')
        send_hostname = re.split(',', send_hostname)[0]
        # 圧縮NG画像送信先パス
        send_ng_image_file_path = inifile.get('PATH', 'send_ng_image_file_path')
        send_ng_image_file_path = '\\\\' + send_hostname + '\\' + send_ng_image_file_path
        # 検査完了通知送信先パス
        send_inspection_file_path = inifile.get('PATH', 'send_inspection_file_path')
        send_inspection_file_path = '\\\\' + send_hostname + '\\' + send_inspection_file_path + '\\'
        # プログラム判定値：未検知画像
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))


        # NG画像圧縮・転送機能以外から実行された際は、開始ログメッセージを表示する
        if undetected_image_flag == undetected_image_flag_is_undetected:
            logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]',
                        app_id, app_name, app_name, fabric_name, inspection_num, imaging_starttime)
        else:
            pass

        ### NG画像圧縮
        logger.debug('[%s:%s] NG画像圧縮を開始します。', app_id, app_name)
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # 未検知画像の場合
            # 合否確認・判定登録部へのファイル送信パス（検査完了通知）
            undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
            undetected_image_file_path = '\\\\' + send_hostname + '\\' + undetected_image_file_path
            tmp_result, ng_image_zip_file_path, error, func_name = ng_image_compress_undetected_image(
                archive_ng_image_file_path, image_root_path, logger)

        else:

            tmp_result, ng_image_zip_file_path, error, func_name = ng_image_compress(image_root_path,
                                                                   imaging_starttime, product_name,
                                                                   fabric_name, inspection_num, logger)
        if tmp_result:
            if ng_image_zip_file_path:
                logger.debug('[%s:%s] NG画像圧縮が終了しました。', app_id, app_name)
            else:
                logger.debug('[%s:%s] NG画像が存在しませんでした。', app_id, app_name)
                result = True
                return result, error, func_name
        else:
            logger.error('[%s:%s] NG画像圧縮に失敗しました。', app_id, app_name)
            return result, error, func_name

        ### 検査完了通知作成
        logger.debug('[%s:%s] 検査完了通知作成を開始します。', app_id, app_name)
        tmp_result, inspection_file_path, undetected_image_file_path, error, func_name = \
            make_inspection_completion_notification(
                undetected_image_flag, product_name, fabric_name, inspection_num, imaging_starttime, rk_root_path,
                ng_image_zip_file_path, logger)

        if tmp_result:
            logger.debug('[%s:%s] 検査完了通知作成が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] 検査完了通知作成に失敗しました。', app_id, app_name)
            return result, error, func_name

        ### ファイル転送
        logger.debug('[%s:%s] ファイル転送を開始します。', app_id, app_name)
        send_undetected_image_file_path = send_inspection_file_path
        tmp_result, error, func_name = send_file(undetected_image_flag, ng_image_zip_file_path, inspection_file_path,
                               undetected_image_file_path, send_ng_image_file_path, send_inspection_file_path,
                               send_undetected_image_file_path, logger)

        if tmp_result:
            logger.debug('[%s:%s] ファイル転送が終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] ファイル転送に失敗しました。', app_id, app_name)
            return result, error, func_name

        # NG画像圧縮・転送機能以外から実行された際は、終了ログメッセージを表示する
        if undetected_image_flag == undetected_image_flag_is_undetected:
            logger.info('[%s:%s] %s処理は正常に終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]',
                        app_id, app_name, app_name, fabric_name, inspection_num, imaging_starttime)
        else:
            pass

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：NG画像圧縮
#
# 処理概要           ：1.NG画像圧縮を行う。
#
# 引数               ： NG画像ルートパス
#                      撮像開始時刻
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    NG画像圧縮ファイルパス
#
# ------------------------------------------------------------------------------------
def ng_image_compress(image_root_path, imaging_starttime, product_name, fabric_name, inspection_num, logger):
    result = False
    ng_image_zip_file_path = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        ### 設定ファイルからの値取得
        # ADD 20200701 KQRM 下吉 START
        # 画像分割枚数
        image_division_number =inifile.get('VALUE', 'max_file')
        # ADD 20200701 KQRM 下吉 END
        # マーキングNG画像名の接頭辞
        marking_ng_image_file_name_prefix = inifile.get('FILE', 'marking_ng_image_file_name_prefix')
        # 画像ファイルの拡張子
        extension = common_inifile.get('FILE_PATTERN', 'image_file')

        # NG画像ファイル名パターン
        ng_image_file_name_pattern = marking_ng_image_file_name_prefix + extension

        ### NG画像圧縮
        # 参照先パス構成は以下を想定する。
        # 「(撮像開始時刻:YYYYMMDD形式)_(品名)_(反番)_(検査番号)」
        path_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num)
        ng_image_path = image_root_path + '\\' \
                        + path_name + '\\'

        tmp_file_list, error, func_name = exists_dir(ng_image_path, logger)
        if tmp_file_list:
            logger.debug('[%s:%s] NG画像フォルダ存在チェックが終了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] NG画像フォルダ存在チェックに失敗しました。', app_id, app_name)

        logger.debug('[%s:%s] NG画像ファイルの確認を開始します。', app_id, app_name)
        tmp_result, ng_image_files, error, func_name = get_file(ng_image_path, ng_image_file_name_pattern, logger)

        if tmp_result:
            logger.debug('[%s:%s] NG画像ファイルの確認が完了しました。', app_id, app_name)
        else:
            logger.error('[%s:%s] NG画像ファイルの確認に失敗しました。', app_id, app_name)
            return result, ng_image_zip_file_path, error, func_name

        logger.debug('[%s:%s] NG画像ファイル:[%s]', app_id, app_name, ng_image_files)

        # NG画像が存在する場合には、NG画像フォルダを圧縮する。
        if ng_image_files:
            zip_path = image_root_path
            # ファイル名は、(撮像開始時刻:YYYYMMDD形式)_反番_検査番号.zip
            zip_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(inspection_num)
            ng_image_zip_file_path = zip_path + '\\' + zip_file_name
            # DEL 20200701 KQRM 下吉 START
            # shutil.make_archive(ng_image_zip_file_path, 'zip', root_dir=image_root_path, base_dir=path_name)
            # # 戻り値（NG画像圧縮ファイルパス）に、拡張子を付与する。
            # ng_image_zip_file_path = ng_image_zip_file_path + '.zip'
            # DEL 20200701 KQRM 下吉 END

            # ADD 20200701 KQRM 下吉 START
            if image_division_number == 0:
                logger.debug('[%s:%s] NG画像ファイルの圧縮を開始します。', app_id, app_name)
                ng_image_zip_file_path = compress_process(ng_image_zip_file_path, image_root_path, path_name, logger)
                logger.debug('[%s:%s] NG画像ファイルの圧縮が完了しました。', app_id, app_name)
            else:
                index = 1
                file_list = []
                count = math.ceil(len(ng_image_files) / int(image_division_number))
                default_path = ng_image_zip_file_path
                work_path = ng_image_zip_file_path + '_work'

                logger.debug('[%s:%s] NG画像ファイルの分割圧縮を開始します。分割数=[%s]', app_id, app_name, count)

                # 分割作業のため、フォルダ名を変更し、ファイル情報を再取得する
                os.rename(default_path, work_path)
                tmp_result, ng_image_files, error, func_name = get_file(work_path + '\\', ng_image_file_name_pattern, logger)

                # 画像分割枚数毎に区切ったファイル情報の配列を生成する
                divided_array = [ng_image_files[i:i + int(image_division_number)] for i in range(0, len(ng_image_files), int(image_division_number))]

                for array in divided_array:
                    # ディレクトリを再生成する
                    tmp_directory = exists_dir(default_path, logger)

                    if not tmp_directory:
                        logger.error(
                            '[%s:%s] 分割圧縮用のフォルダ作成が失敗しました。フォルダパス=[%s]',
                            app_id,
                            app_name,
                            default_path)
                        return result, default_path, error, func_name

                    for image_path in array:
                        # 画像をフォルダにコピーする
                        tmp_file = file_util.copy_file(image_path, default_path, logger, app_id, app_name)

                        if not tmp_file:
                            logger.error(
                                '[%s:%s] NG画像のファイルコピーに失敗しました。ファイルパス=[%s]',
                                app_id,
                                app_name,
                                image_path)
                            return result, default_path, error, func_name

                    # zipファイル名に連番を付与する
                    target_path = default_path + '_' + str(index).zfill(2)

                    # 圧縮する
                    file_list.append(
                        compress_process(
                            target_path,
                            image_root_path,
                            path_name,
                            logger))

                    # 作業フォルダを削除する
                    shutil.rmtree(default_path)
                    index += 1

                ng_image_zip_file_path = file_list
                # 変更したフォルダ名を元に戻す
                os.rename(work_path, default_path)
                logger.debug('[%s:%s] NG画像ファイルの分割圧縮が完了しました。', app_id, app_name)
            # ADD 20200701 KQRM 下吉 END
        else:
            zip_path = image_root_path
            # ファイル名は、(撮像開始時刻:YYYYMMDD形式)_反番_検査番号.zip
            zip_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(
                inspection_num) + '.zip'
            Path(zip_path + '\\' + zip_file_name).touch()
            ng_image_zip_file_path = zip_path + '\\' + zip_file_name

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, ng_image_zip_file_path, error, func_name

# ADD 20200701 KQRM 下吉 START
# ------------------------------------------------------------------------------------
# 処理名             ：圧縮プロセス
#
# 処理概要           ：1.NG画像圧縮を行う。
#
# 引数               ： 圧縮対象NG画像パス
#                      NG画像ルートパス
#                      フォルダ名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    NG画像圧縮ファイルパス
#
# ------------------------------------------------------------------------------------
def compress_process(archive_ng_image_file_path, image_root_path, path_name, logger):
    zip_file_path = archive_ng_image_file_path
    try:
        shutil.make_archive(zip_file_path, 'zip', root_dir=image_root_path, base_dir=path_name)

        # 戻り値（NG画像圧縮ファイルパス）に、拡張子を付与する。
        zip_file_path = zip_file_path + '.zip'

    except Exception as e:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(e, logger, app_id, app_name)

    return zip_file_path
# ADD 20200701 KQRM 下吉 END

# ------------------------------------------------------------------------------------
# 処理名             ：NG画像圧縮（未検知画像登録用）
#
# 処理概要           ：1.NG画像圧縮を行う。
#
# 引数               ： 圧縮対象NG画像パス
#                      出力パス
#                      反番
#                      検査番号
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    NG画像圧縮ファイルパス
#
# ------------------------------------------------------------------------------------
def ng_image_compress_undetected_image(archive_ng_image_file_path, output_path, logger):
    result = False
    ng_image_zip_file_path = None
    error = None
    func_name = sys._getframe().f_code.co_name
    try:
        root_dir = os.path.dirname(archive_ng_image_file_path)
        base_dir = os.path.basename(archive_ng_image_file_path)

        # NG画像フォルダを圧縮する。
        zip_path = output_path
        # ファイル名は、未検知画像ファイル名（※フォルダ名と同一名）.zip
        zip_file_name = base_dir
        ng_image_zip_file_path = output_path + '\\' + zip_file_name
        shutil.make_archive(ng_image_zip_file_path, 'zip', root_dir=root_dir, base_dir=base_dir)
        # 戻り値（NG画像圧縮ファイルパス）に、拡張子を付与する。
        ng_image_zip_file_path = ng_image_zip_file_path + '.zip'

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, ng_image_zip_file_path, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：検査完了通知作成
#
# 処理概要           ：1.検査完了通知作成を行う。
#
# 引数               ： 未検知画像フラグ（1:未検知画像である、それ以外:未検知画像でない）
#                      品名
#                      反番
#                      検査番号
#                      連携基盤のルートパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                    処理完了通知ファイルパス
#                    未検知画像登録完了ファイルパス（未検知画像フラグが「1:」の場合のみ返す、それ以外はNoneを返す）
#
# ------------------------------------------------------------------------------------
def make_inspection_completion_notification(
        undetected_image_flag, product_name, fabric_name, inspection_num, imaging_starttime, rk_root_path,
        archive_ng_image_file_path, logger):
    result = False
    result_inspection_file_path = None
    result_undetected_image_file_path = None
    error = None
    func_name = sys._getframe().f_code.co_name

    try:
        ### 設定ファイルからの値取得
        # 未検知画像フラグ:未検知画像である
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))
        # 検査完了通知ファイルパス
        undetected_image_file_path = inifile.get('PATH', 'undetected_image_file_path')
        # 未検知画像登録完了ファイル名
        undetected_image_name_suffix = inifile.get('FILE', 'undetected_image_name_suffix')
        # DEL 20200701 KQRM 下吉 START
        # base_dir = os.path.basename(archive_ng_image_file_path)
        # DEL 20200701 KQRM 下吉 END

        # 未検知画像判定を行う。
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # ADD 20200701 KQRM 下吉 START
            base_dir = os.path.basename(archive_ng_image_file_path)
            # ADD 20200701 KQRM 下吉 END
            # 未検知画像登録完了ファイルを作成する。
            # ファイル名は、未検知画像ファイル名.txt
            base_dir = re.split('\.', base_dir)[0]
            tmp_file_name = base_dir + '.txt'
            tmp_path_path = rk_root_path + '\\' + tmp_file_name
            Path(tmp_path_path).touch()
            result_undetected_image_file_path = tmp_path_path
            result_inspection_file_path = None

        else:
            # 処理完了通知を作成する。
            tmp_file_name = imaging_starttime + '_' + product_name + '_' + fabric_name + '_' + str(
                inspection_num) + '.txt'
            tmp_path_path = rk_root_path + '\\' + tmp_file_name
            Path(tmp_path_path).touch()
            result_inspection_file_path = tmp_path_path
            result_undetected_image_file_path = None

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, result_inspection_file_path, result_undetected_image_file_path, error, func_name


# ------------------------------------------------------------------------------------
# 処理名             ：ファイル転送
#
# 処理概要           ：1.NG画像圧縮ファイルと処理完了通知を、合否確認・判定登録部へ転送する。
#
# 引数               ： 未検知画像フラグ（1:未検知画像である、それ以外:未検知画像でない）
#                      NG画像アーカイブファイルパス
#                      検査完了通知ファイルパス
#                      未検知画像登録完了ファイルパス
#                      送信先のNG画像アーカイブファイルパス
#                      送信先の検査完了通知ファイルパス
#                      送信先の未検知画像登録完了ファイルパス
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#
# ------------------------------------------------------------------------------------
def send_file(
        undetected_image_flag,
        ng_image_archive_file_path, inspection_file_path, undetected_image_file_path,
        send_ng_image_file_path, send_inspection_file_path, send_undetected_image_file_path, logger):
    result = False
    func_name = sys._getframe().f_code.co_name
    error = None
    try:
        ### 設定ファイルからの値取得
        # 未検知画像フラグ:未検知画像である
        undetected_image_flag_is_undetected = int(inifile.get('VALUE', 'undetected_image_flag_is_undetected'))

        ### 処理完了通知を、合否確認・判定登録部へ転送する。
        # NG画像
        logger.debug('[%s:%s] NG画像のファイル転送を開始します。', app_id, app_name)
        # DEL 20200701 KQRM 下吉 START
        # tmp_result = file_util.move_file(ng_image_archive_file_path, send_ng_image_file_path, logger, app_id, app_name)
        #
        # if tmp_result:
        #     logger.debug('[%s:%s] NG画像のファイル転送が終了しました。', app_id, app_name)
        # else:
        #     logger.error('[%s:%s] NG画像のファイル転送に失敗しました。', app_id, app_name)
        #     return result
        # DEL 20200701 KQRM 下吉 END

        # ADD 20200701 KQRM 下吉 START
        if type(ng_image_archive_file_path) is str:
            tmp_result, error = file_util.move_file(ng_image_archive_file_path, send_ng_image_file_path, logger, app_id, app_name)

            if tmp_result:
                logger.debug('[%s:%s] NG画像のファイル転送が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] NG画像のファイル転送に失敗しました。', app_id, app_name)
                return result, error, func_name
        else:
            for zip_path in ng_image_archive_file_path:
                tmp_result, error = file_util.move_file(zip_path, send_ng_image_file_path, logger, app_id, app_name)

                if tmp_result:
                    logger.debug('[%s:%s] NG画像のファイル転送が終了しました。', app_id, app_name)
                else:
                    logger.error('[%s:%s] NG画像のファイル転送に失敗しました。', app_id, app_name)
                    return result, error, func_name
        # ADD 20200701 KQRM 下吉 END

        # 未検知画像登録完了
        if undetected_image_flag == undetected_image_flag_is_undetected:
            # 未検知画像の場合
            logger.debug('[%s:%s] 未検知画像登録完了のファイル転送を開始します。', app_id, app_name)
            tmp_result, error = file_util.move_file(undetected_image_file_path, send_undetected_image_file_path, logger,
                                             app_id,
                                             app_name)

            if tmp_result:
                logger.debug('[%s:%s] 未検知画像登録完了のファイル転送が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 未検知画像登録完了のファイル転送に失敗しました。', app_id, app_name)
                return result, error, func_name

        else:
            # 検査完了通知
            logger.debug('[%s:%s] 検査完了通知のファイル転送を開始します。', app_id, app_name)
            tmp_result, error = file_util.move_file(inspection_file_path, send_inspection_file_path, logger, app_id, app_name)

            if tmp_result:
                logger.debug('[%s:%s] 検査完了通知のファイル転送が終了しました。', app_id, app_name)
            else:
                logger.error('[%s:%s] 検査完了通知のファイル転送に失敗しました。', app_id, app_name)
                return result, error, func_name

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, error, func_name


# ------------------------------------------------------------------------------------
# 関数名             ：撮像画像ファイル取得
#
# 処理概要           ：1.撮像画像ファイルのファイルリストを取得する。
#
# 引数               ：撮像画像ファイル格納フォルダパス
#                      撮像画像ファイル
#
# 戻り値             ：処理結果（True:成功、False:失敗）
#                      撮像画像ファイルリスト
# ------------------------------------------------------------------------------------
def get_file(file_path, file_name, logger):
    result = False
    file_list = None
    func_name = sys._getframe().f_code.co_name
    error = None

    try:
        logger.debug('[%s:%s] 撮像画像ファイル格納フォルダパス=[%s]', app_id, app_name, file_path)

        # 共通関数で撮像画像ファイル格納フォルダ情報を取得する
        file_list = None
        tmp_result, file_list, error = file_util.get_file_list(file_path, file_name, logger, app_id, app_name)

        if tmp_result:
            # 成功時
            pass
        else:
            # 失敗時
            logger.error("[%s:%s] 撮像画像ファイル格納フォルダにアクセス出来ません。", app_id, app_name)
            return result, file_list, error, func_name

        result = True

    except Exception as error:
        # 失敗時は共通例外関数でエラー詳細をログ出力する
        error_detail.exception(error, logger, app_id, app_name)

    return result, file_list, error, func_name
