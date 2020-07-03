# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 機能307 マスキング判定
# ----------------------------------------

import os
import configparser
import cv2
import numpy as np
import logging.config
import traceback
from multiprocessing import Pool
import multiprocessing as multi

import error_detail
import file_util

# 画像リサイズ設定ファイル読込
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/masking_fabric_config.ini', 'SJIS')
# 共通設定ファイル読込
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ログ設定
logging.config.fileConfig("D:/CI/programs/config/logging_masking_fabric.conf")
logger = logging.getLogger("masking_fabric")

BLACK = int(inifile.get('MASK_VALUE', 'black'))
WHITE = int(inifile.get('MASK_VALUE', 'white'))

# ログ出力に使用する、機能ID、機能名
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# 処理名             ：非膨張部マスキング引数処理
#
# 処理概要           ：1.非膨張部のマスキング処理へ引数を渡す。
#
# 引数               ：引数
#
# 戻り値             ：マスク画像データ
# ------------------------------------------------------------------------------------
def wrapper_filter_non_inflatable_portion(args):
    # 非膨張部マスキング処理へ引数を渡す
    result = filter_non_inflatable_portion(*args)
    return result


# ------------------------------------------------------------------------------------
# 処理名             ：非膨張部マスキング処理
#
# 処理概要           ：1.マスキング処理を行って、非膨張部の詳細な判定を行う。
#
# 引数               ：マスク対象画像
#
# 戻り値             ：マスク画像データ
# ------------------------------------------------------------------------------------
def filter_non_inflatable_portion(file):
    fname, fext = os.path.splitext(os.path.basename(file))
    try:

        black_yarn_threshold_max = int(inifile.get('MASK_VALUE', 'black_yarn_threshold_max'))
        block_size = int(inifile.get('MASK_VALUE', 'block_size'))
        c = int(inifile.get('MASK_VALUE', 'c'))
        remove_noise_threshold_max = int(inifile.get('MASK_VALUE', 'remove_noise_threshold_max'))
        remove_noise_size_max = int(inifile.get('MASK_VALUE', 'remove_noise_size_max'))
        border_block_size = int(inifile.get('MASK_VALUE', 'border_block_size'))
        border_c = int(inifile.get('MASK_VALUE', 'border_c'))
        border_non_expansion_area_max = int(inifile.get('MASK_VALUE', 'border_non_expansion_area_max'))
        border_non_expansion_area_min = int(inifile.get('MASK_VALUE', 'border_non_expansion_area_min'))
        border_dist_min = int(inifile.get('MASK_VALUE', 'border_dist_min'))
        border_dist_max = int(inifile.get('MASK_VALUE', 'border_dist_max'))
        setting_max_dist = int(inifile.get('MASK_VALUE', 'max_dist'))
        setting_aspect = float(inifile.get('MASK_VALUE', 'aspect'))
        setting_short_side = int(inifile.get('MASK_VALUE', 'short_side'))
        white_area_size_max = int(inifile.get('MASK_VALUE', 'white_area_size_max'))
        white_area_size_min = int(inifile.get('MASK_VALUE', 'white_area_size_min'))
        white_area_cnt_sum_min = int(inifile.get('MASK_VALUE', 'white_area_cnt_sum_min'))
        work_img_min = float(inifile.get('MASK_VALUE', 'work_img_min'))
        white_area_cnt_min = int(inifile.get('MASK_VALUE', 'white_area_cnt_min'))
        seam_size_max = float(inifile.get('MASK_VALUE', 'seam_size_max'))

        # 画像読み込み
        result, img_org = file_util.read_image(file, logger, app_id, app_name)

        # グレースケール化
        img_gray = cv2.cvtColor(img_org, cv2.COLOR_BGR2GRAY)

        # グレースケール化した画像のheightとweightを取得
        h, w = img_gray.shape

        # ================================================================================================================
        # --黒糸対策--
        # 黒い糸の部分は二値化した際に影響するため
        black_yarn = img_gray.copy()

        # ノイズ低減のためぼかし、閾値120で2値化
        ret, black_yarn_bin = cv2.threshold(cv2.blur(img_gray, (5, 5)), 120, WHITE, cv2.THRESH_BINARY_INV)

        # 小さい領域はノイズのため除外
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=black_yarn_bin, connectivity=8)

        for label in range(1, labelnum):
            # 上のlabelcontoursを一個ずつ変数にいれる
            x, y, ww, hh, size = labelcontours[label]
            if size < black_yarn_threshold_max:
                black_yarn_bin[labelimg == label] = BLACK

        bool_points = np.logical_and(black_yarn < 120, black_yarn_bin == WHITE)

        # 画素の平均で埋める
        # 明るさのムラがあるため4分割で処理
        for i in range(0, 2):
            for j in range(0, 2):
                # 処理対象領域を切り出す
                work_img = black_yarn[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                           0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2]

                hh, ww = work_img.shape
                average = np.sum(work_img) / (hh * ww)
                work_bool_points = bool_points[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                                   0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2]
                work_img[work_bool_points] = average

                # 処理対象領域を戻す
                black_yarn[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2] = work_img

        # ノイズ低減のためぼかす(糸の網目をつぶす)
        black_yarn = cv2.blur(black_yarn, (5, 5))
        # ================================================================================================================

        # ================================================================================================================
        # --シーム領域抽出--
        # 適応的2値化
        img_seam_bin = cv2.adaptiveThreshold(black_yarn, WHITE, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY,
                                             block_size, c)

        seam_area = img_seam_bin.copy()
        # 一定の明るさ以下の部分は除外
        # 動的に決定（平均-10）
        # 明るさのムラがあるため25分割で処理
        for i in range(0, 5):
            for j in range(0, 5):
                # 処理対象領域を切り出す
                work_img = black_yarn[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]

                hh, ww = work_img.shape
                average = np.sum(work_img) / (hh * ww) - 10
                work_img = seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                img_gray_tmp = img_gray[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                               0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                work_img[img_gray_tmp < average] = BLACK

                # 処理対象領域を戻す
                seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2] = work_img

        # -ノイズの除外-
        # 収縮処理
        kernel = np.array([[0, 1, 0], [0, 1, 0], [0, 0, 0]], np.uint8)
        seam_area = cv2.erode(seam_area, kernel, iterations=1)
        kernel = np.array([[0, 0, 0], [1, 1, 0], [0, 0, 0]], np.uint8)
        seam_area = cv2.erode(seam_area, kernel, iterations=1)

        # 輪郭線抽出で穴埋め
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # 距離変換
        distimg = cv2.distanceTransform(seam_area, cv2.DIST_L1, 3)
        seam_area[distimg < 2] = BLACK

        # -白領域救済-
        # 一定の明るさ以上の部分を救済
        # 動的に決定（平均+20 もしくは 210）
        # 明るさのムラがあるため25分割で処理
        for i in range(0, 5):
            for j in range(0, 5):
                # 処理対象領域を切り出す
                img_gray_tmp = img_gray[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                               0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]

                hh, ww = img_gray_tmp.shape
                img_gray_tmp = cv2.blur(img_gray_tmp, (5, 5))
                average = max(np.sum(img_gray_tmp) / (hh * ww) + 20, 210)
                work_img = seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                work_img[img_gray_tmp >= average] = WHITE

                # 処理対象領域を戻す
                seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2] = work_img

        # -ノイズ除去-
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            # 面積が小さい領域はノイズor欠点の可能性大
            if size <= remove_noise_threshold_max:
                seam_area[labelimg == label] = BLACK

        # 輪郭線検出
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

        # 離れた領域をつなげる
        # 極太線を引いて(若干無理やり)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, 30 + 20)

        # 極太線を収縮処理で細める
        seam_area = cv2.erode(seam_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=15 + 10)

        # 輪郭線抽出で穴埋め
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # -ノイズ除去-
        labelnum, labelimg, contours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = contours[label]
            local_seam_area = np.zeros(img_org.shape[:2], np.uint8)
            local_seam_area[labelimg == label] = WHITE
            # 離れ小島(画像の端に接していない領域)はシームではない可能性が高い
            local_contours, hierarchy = cv2.findContours(local_seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            noize_flag = True
            for cnt in local_contours[0]:
                cnt = cnt[0]
                local_x = cnt[0]
                local_y = cnt[1]
                if local_x == 0 or local_y == 0 or local_x == w - 1 or local_y == h - 1:
                    noize_flag = False
                    break
            # 小さい領域はノイズ（欠点）の可能性が大きい
            if size <= remove_noise_size_max or noize_flag:
                seam_area[labelimg == label] = BLACK
        # ================================================================================================================

        # ================================================================================================================
        # --境目対応--
        # 非膨張部用の2値化
        img_border_bin = cv2.adaptiveThreshold(black_yarn, WHITE, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY,
                                               border_block_size, border_c)

        # -ノイズ除去-
        # 膨張収縮
        img_border_bin = cv2.dilate(img_border_bin, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=1)
        img_border_bin = cv2.erode(img_border_bin, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=1)

        # シーム領域を除外
        # 境目用2値化ではシームがボロボロになりやすく、そこが後々の処理で欠点と結合しやすくなる
        work_img = seam_area.copy()
        work_img = cv2.dilate(work_img, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=5)
        img_border_bin[work_img == WHITE] = BLACK

        # -シーム用2値化で非膨張部の点を大きくする-
        # 黒糸対策
        img_seam_bin[black_yarn_bin == WHITE] = BLACK
        # ノイズ除去
        # 距離変換
        distimg = cv2.distanceTransform(img_seam_bin, cv2.DIST_L1, 3)
        img_seam_bin[distimg < 2] = 0
        # 収縮処理
        kernel = np.array([[0, 1, 0], [0, 1, 0], [0, 0, 0]], np.uint8)
        img_seam_bin = cv2.erode(img_seam_bin, kernel, iterations=1)
        kernel = np.array([[0, 0, 0], [1, 1, 0], [0, 0, 0]], np.uint8)
        img_seam_bin = cv2.erode(img_seam_bin, kernel, iterations=1)

        # -非膨張部の点用2値化と比較-
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=img_border_bin, connectivity=8)
        seam_labelnum, seam_labelimg, seam_labelcontours, seam_GoCs = cv2.connectedComponentsWithStats(
            image=img_seam_bin,
            connectivity=8)
        img_border_bin = np.zeros(img_border_bin.shape[:2], np.uint8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            if img_seam_bin[y + int(hh / 2), x + int(ww / 2)] == WHITE:
                img_border_bin[seam_labelimg == seam_labelimg[y + int(hh / 2), x + int(ww / 2)]] = WHITE

        # 一定の面積内の領域を残す
        # 大きすぎる領域もノイズの可能性あり
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=img_border_bin, connectivity=8)
        non_expansion_area = np.zeros(img_border_bin.shape[:2], np.uint8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            if size < border_non_expansion_area_max and size > border_non_expansion_area_min:
                non_expansion_area[y + int(hh / 2), x + int(ww / 2)] = WHITE

        # 一定の距離内にある点を結ぶ
        non_expansion_area_labelnum, non_expansion_area_labelimg, non_expansion_area_contours, non_expansion_area_GoCs = \
            cv2.connectedComponentsWithStats(image=non_expansion_area, connectivity=8)
        for label1 in range(1, non_expansion_area_labelnum):
            x1, y1, w1, h1, size1 = non_expansion_area_contours[label1]
            for label2 in range(label1 + 1, non_expansion_area_labelnum):
                x2, y2, w2, h2, size2 = non_expansion_area_contours[label2]
                # とりあえずユークリッド距離
                dist = np.sqrt((x2 - x1) ** 2 + (y2 - y1) ** 2)
                # ごま塩ノイズ対策で近すぎるものも除外
                if border_dist_min < dist and dist < border_dist_max:
                    cv2.line(non_expansion_area, (x1, y1), (x2, y2), WHITE)

        # -ノイズ除外-

        contours, hierarchy = cv2.findContours(non_expansion_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        non_expansion_area = cv2.drawContours(np.zeros(non_expansion_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # 線状のもの, 細長いもので短辺が短いものは除外
        non_expansion_area_labelnum, non_expansion_area_labelimg, non_expansion_area_contours, non_expansion_area_GoCs = cv2.connectedComponentsWithStats(
            image=non_expansion_area, connectivity=8)
        for label in range(1, non_expansion_area_labelnum):
            x, y, ww, hh, size = non_expansion_area_contours[label]
            work_img = np.zeros(img_org.shape[:2], np.uint8)
            work_img[non_expansion_area_labelimg == label] = WHITE
            distimg = cv2.distanceTransform(work_img, cv2.DIST_L1, 3)
            max_dist = np.max(distimg)
            # 細長いもの＝縦横比が悪い
            if ww < hh:
                long_side = hh
                short_side = ww
            else:
                long_side = ww
                short_side = hh
            aspect = short_side / long_side
            if max_dist <= setting_max_dist or (aspect < setting_aspect and short_side < setting_short_side) \
                    or (ww * hh < 100 * 100):
                non_expansion_area[non_expansion_area_labelimg == label] = BLACK

        # 領域を結合しながら、ちょっと膨らます
        # 領域に穴が開くのをできるだけ防ぐ
        non_expansion_area = cv2.dilate(non_expansion_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
                                        iterations=50)
        non_expansion_area = cv2.erode(non_expansion_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
                                       iterations=35)

        # 非膨張部の輪郭線検出

        contours, hierarchy = cv2.findContours(non_expansion_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        non_expansion_area = cv2.drawContours(np.zeros(non_expansion_area.shape[:2], np.uint8), contours, -1, WHITE, 10)
        # ================================================================================================================

        # ================================================================================================================
        # --非膨張部領域とシーム領域を重ねる--
        seam_area[non_expansion_area == WHITE] = WHITE

        # 輪郭線検出

        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, 30)
        seam_area = cv2.erode(seam_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=15)

        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)
        # ================================================================================================================

        # ================================================================================================================
        # --シームで分割された領域を調べる--
        # 白領域をラベリングするので、白黒反転
        seam_area = cv2.bitwise_not(seam_area)

        # -マスク画像作成-
        img_mask = np.zeros(img_org.shape[:2], np.uint8)
        labelnum, labelimg, contours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            img_label = np.zeros(seam_area.shape[:2], np.uint8)
            img_label[labelimg == label] = WHITE
            bool_points = np.logical_and(img_border_bin == WHITE, img_label == WHITE)
            img_ck_area = np.zeros(img_org.shape[:2], np.uint8)
            img_ck_area[bool_points] = WHITE

            # チェック領域内の白画素の固まりの数を数える
            # 面積が極小の領域は除外
            white_area_cnt = 0
            ck_area_labelnum, ck_area_labelimg, ck_area_contours, ck_area_GoCs = cv2.connectedComponentsWithStats(
                image=img_ck_area, connectivity=8)
            for ck_area_label in range(1, ck_area_labelnum):
                x, y, ww, hh, size = ck_area_contours[ck_area_label]
                if size < white_area_size_max and size > white_area_size_min:
                    white_area_cnt = white_area_cnt + 1
            # チェック領域の面積に対して白画素の固まり(非膨張部の点)の数がたくさん(密に)あるかで判定
            img_label[img_label == WHITE] = 1
            if white_area_cnt / np.sum(img_label) >= white_area_cnt_sum_min / (100 * 100):
                img_mask[img_label == 1] = WHITE
            # 白の面積比率が10%以上もマスク
            work_img = img_ck_area.copy()
            work_img[work_img == WHITE] = 1
            if np.sum(work_img) / np.sum(img_label) > 0.1:
                img_mask[img_label == 1] = WHITE
            # 点の数と白の面積の複合条件
            # 面積比率が1%以上、かつ、点の数がやや疎でもマスク
            if np.sum(work_img) / np.sum(img_label) > work_img_min and \
                    white_area_cnt / np.sum(img_label) >= white_area_cnt_min / (100 * 100):
                img_mask[img_label == 1] = WHITE

        # シーム領域結果をマスク画像に反映
        img_mask[seam_area == BLACK] = WHITE
        labelnum, labelimg, contours, GoCs = cv2.connectedComponentsWithStats(image=img_mask, connectivity=8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = contours[label]
            if size < seam_size_max * 100 * 100:
                img_mask[labelimg == label] = BLACK
        # ================================================================================================================

        maskimages = {}
        maskimages[os.path.basename(file)] = img_mask
        return maskimages

    except Exception as e:
        # エラーになった画像名を出力する。
        logger.error('[%s:%s] エラー対象画像名=%s ' % (app_id, app_name, fname))
        raise e


# ------------------------------------------------------------------------------------
# 処理名             ：非膨張部判定引数処理
#
# 処理概要           ：1.非膨張部の判定処理へ引数を渡す。
#
# 引数               ：引数
#
# 戻り値             ：非膨張部判定処理結果
# ------------------------------------------------------------------------------------
def wrapper_check_rate(args):
    # 非膨張部判定処理へ引数を渡す
    result = check_rate(*args)
    return result


# ------------------------------------------------------------------------------------
# 処理名             ：非膨張部判定処理
#
# 処理概要           ：1.マスキングを行った画像の判定処理を行う。
#
# 引数               ：マスキング判定結果
#                      マスク画像データ
#                      端判定閾値
#                      カテゴリー名(NG)
#                      カテゴリー名(others)
#
# 戻り値             ：非膨張部判定処理結果
# ------------------------------------------------------------------------------------
def check_rate(ng_result, mask_images, masking_rate, label_ng, label_others):
    try:
        # 画像データから判定値を算出する
        img_bin = mask_images[ng_result["filename"]]
        img_mask_crop = img_bin[ng_result["point"]["y"]:ng_result["point"]["y"] + ng_result["height"],
                        ng_result["point"]["x"]:ng_result["point"]["x"] + ng_result["width"]]
        img_mask_crop[img_mask_crop == WHITE] = 1
        rate = np.sum(img_mask_crop) / (100 * 100)

        rate_result = {}
        # 判定値
        if rate < masking_rate:
            rate_result[ng_result["filename"] + "," + str(ng_result["point"]["x"]) + "," + str(
                ng_result["point"]["y"])] = label_ng
        else:
            rate_result[ng_result["filename"] + "," + str(ng_result["point"]["x"]) + "," + str(
                ng_result["point"]["y"])] = label_others

        return rate_result
    except Exception as e:
        # エラーになった画像名を出力する。
        raise e


# ------------------------------------------------------------------------------------
# 処理名             ：マスキング判定
#
# 処理概要           ：1.画像に対してマスキング処理を行い、結果を判定する。
#
# 引数               ：端判定結果CSVファイルパス
#                      マスキング判定結果CSVファイルパス
#                      NGカテゴリ
#                      OKカテゴリ
#                      処理情報リスト
#                      端判定閾値
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def base_masking_fabric(checkfabric_result_file, masking_result_file, label_ng, label_others, process_list,
                        masking_rate):
    result = False
    try:
        # 結果ファイル読み込み
        logger.debug('[%s:%s] 結果ファイル読込を開始します。' % (app_id, app_name))

        tmp_result, result_data, error = file_util.read_result_file(checkfabric_result_file, logger, app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] 結果ファイル読込が終了しました。%s ' % (app_id, app_name, result_data))
        else:
            logger.error('[%s:%s] 結果ファイル読込が失敗しました。' % (app_id, app_name))
            return result, process_list

        # 撮像画像ディレクトリ変数定義
        image_dir = result_data["datapath"]

        # ---マスク画像作成処理---
        logger.debug('[%s:%s] 非膨張部マスク画像作成処理準備を開始します。' % (app_id, app_name))

        # 並列処理用に引数を準備
        process_file_args = []
        target_fname = []
        for masking_result in result_data["data"]:
            if masking_result["label"] != "NG":
                continue
            if masking_result["filename"] in target_fname:
                continue
            target_fname.append(masking_result["filename"])
            args = []
            args.append(image_dir + "\\" + masking_result["filename"])
            process_file_args.append(args)

        logger.debug('[%s:%s] 非膨張部マスク画像作成処理準備が終了しました。' % (app_id, app_name))

        logger.debug('[%s:%s] 非膨張部マスク画像作成処理の並列実行を開始します。' % (app_id, app_name))
        # 並列処理実行
        # p = Pool(multi.cpu_count())
        p = Pool(4)
        res = p.map(wrapper_filter_non_inflatable_portion, process_file_args)
        p.close()
        logger.debug('[%s:%s] 非膨張部マスク画像作成処理の並列実行が終了しました。' % (app_id, app_name))

        logger.debug('[%s:%s] 非膨張部マスク画像作成処理結果取得を開始します。' % (app_id, app_name))
        # 並列処理結果取得
        mask_images = {}
        for data in res:
            if data is None:
                continue
            for key, value in data.items():
                mask_images[key] = value
        logger.debug('[%s:%s] 非膨張部マスク画像作成処理結果取得が終了しました。' % (app_id, app_name))

        # ---非膨張部判定処理---
        logger.debug('[%s:%s] 非膨張部判定処理準備を開始します。' % (app_id, app_name))
        # 並列処理用に引数を準備
        process_file_args = []
        for masking_result in result_data["data"]:
            if masking_result["label"] != label_ng:
                continue
            args = []
            args.append(masking_result)
            args.append(mask_images)
            args.append(masking_rate)
            args.append(label_ng)
            args.append(label_others)
            process_file_args.append(args)
        logger.debug('[%s:%s] 非膨張部判定処理準備が終了しました。' % (app_id, app_name))

        logger.debug('[%s:%s] 非膨張部判定処理の並列実行を開始します。' % (app_id, app_name))
        # 並列処理実行
        # p = Pool(multi.cpu_count())
        p = Pool(4)
        res = p.map(wrapper_check_rate, process_file_args)
        p.close()
        logger.debug('[%s:%s] 非膨張部判定処理の並列実行が終了しました。' % (app_id, app_name))

        logger.debug('[%s:%s] 非膨張部判定処理結果の取得を開始します。' % (app_id, app_name))
        # 並列処理結果取得
        for data in res:
            if data is None:
                continue
            else:
                for key, value in data.items():
                    if value == label_others:
                        fname, x, y = key.split(",")
                        for masking_result in result_data["data"]:
                            if fname == masking_result["filename"] and x == str(
                                    masking_result["point"]["x"]) and y == str(masking_result["point"]["y"]):
                                masking_result["label"] = label_others
                                break
                            else:
                                pass
                    else:
                        pass

        # 処理情報（処理ID、撮像画像、連番）とマスキング判定結果の紐づけを行う。
        for masking_result in result_data["data"]:
            for i in range(len(process_list)):
                if masking_result["label"] == label_others and process_list[i][3] == masking_result["filename"] \
                        and str(process_list[i][4]) == str(masking_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(masking_result["point"]["y"]):
                    process_list[i].append(label_others)
                    continue

                elif masking_result["label"] == label_ng and process_list[i][3] == masking_result["filename"] \
                        and str(process_list[i][4]) == str(masking_result["point"]["x"]) \
                        and str(process_list[i][5]) == str(masking_result["point"]["y"]):
                    process_list[i].append(label_ng)
                    continue
                else:
                    pass

        logger.debug('[%s:%s] 非膨張部判定処理結果の取得が終了しました。' % (app_id, app_name))

        logger.debug('[%s:%s] マスキング判定結果CSVファイルの出力を開始します。' % (app_id, app_name))
        # マスキング判定結果CSVファイルを出力する
        tmp_result, error = file_util.write_result_file(masking_result_file, result_data, image_dir, logger, app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] マスキング判定結果CSVファイルの出力が終了しました。' % (app_id, app_name))
        else:
            logger.error('[%s:%s] マスキング判定結果CSVファイルの出力が失敗しました。' % (app_id, app_name))
            return result, process_list

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_list


# ------------------------------------------------------------------------------------
# 処理名             ：メイン処理
#
# 処理概要           ：1.端判定結果CSVを読込んで、画像に対してマスキング判定を行い、結果を出力する。
#                      2.画像マーキング処理を呼び出す。
#
# 引数               ：コネクションオブジェクト
#                      カーソルオブジェクト
#                      端判定結果CSVファイルパス
#                      マスキング判定結果CSVファイルパス
#                      カテゴリー名(NG)
#                      カテゴリー名(others)
#                      処理情報リスト
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def main(checkfabric_result_file, masking_result_file, label_ng, label_others, process_list, inspection_date):
    result = False
    try:

        # 設定ファイルから値を取得

        masking_rate = float(inifile.get('VALUE', 'masking_rate'))

        # 変数定義
        fabric_info = os.path.basename(masking_result_file).split('_')
        fabric_name = fabric_info[1]
        inspection_num = fabric_info[3]

        logger.info('[%s:%s] %s処理を開始します。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        # マスキング処理・判定を行う
        result, process_list = base_masking_fabric(checkfabric_result_file, masking_result_file, label_ng,
                                                   label_others, process_list, masking_rate)

        if result:
            logger.info('[%s:%s] %s処理が正常に終了しました。 [反番, 検査番号, 検査日付]=[%s, %s, %s]' %
                        (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        else:
            logger.error('[%s:%s] %s処理が失敗しました。' % (app_id, app_name, app_name))
            return result, process_list

        result = True

    except Exception as e:
        # 想定外エラー発生
        logger.error('[%s:%s] 予期しないエラーが発生しました。' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, process_list
