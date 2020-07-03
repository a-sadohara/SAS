# -*- coding: SJIS -*-
# ----------------------------------------
# �� �@�\307 �}�X�L���O����
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

# �摜���T�C�Y�ݒ�t�@�C���Ǎ�
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/masking_fabric_config.ini', 'SJIS')
# ���ʐݒ�t�@�C���Ǎ�
common_inifile = configparser.ConfigParser()
common_inifile.read('D:/CI/programs/config/common_config.ini', 'SJIS')

# ���O�ݒ�
logging.config.fileConfig("D:/CI/programs/config/logging_masking_fabric.conf")
logger = logging.getLogger("masking_fabric")

BLACK = int(inifile.get('MASK_VALUE', 'black'))
WHITE = int(inifile.get('MASK_VALUE', 'white'))

# ���O�o�͂Ɏg�p����A�@�\ID�A�@�\��
app_id = int(inifile.get('APP', 'app_id'))
app_name = inifile.get('APP', 'app_name')


# ------------------------------------------------------------------------------------
# ������             �F��c�����}�X�L���O��������
#
# �����T�v           �F1.��c�����̃}�X�L���O�����ֈ�����n���B
#
# ����               �F����
#
# �߂�l             �F�}�X�N�摜�f�[�^
# ------------------------------------------------------------------------------------
def wrapper_filter_non_inflatable_portion(args):
    # ��c�����}�X�L���O�����ֈ�����n��
    result = filter_non_inflatable_portion(*args)
    return result


# ------------------------------------------------------------------------------------
# ������             �F��c�����}�X�L���O����
#
# �����T�v           �F1.�}�X�L���O�������s���āA��c�����̏ڍׂȔ�����s���B
#
# ����               �F�}�X�N�Ώۉ摜
#
# �߂�l             �F�}�X�N�摜�f�[�^
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

        # �摜�ǂݍ���
        result, img_org = file_util.read_image(file, logger, app_id, app_name)

        # �O���[�X�P�[����
        img_gray = cv2.cvtColor(img_org, cv2.COLOR_BGR2GRAY)

        # �O���[�X�P�[���������摜��height��weight���擾
        h, w = img_gray.shape

        # ================================================================================================================
        # --�����΍�--
        # �������̕����͓�l�������ۂɉe�����邽��
        black_yarn = img_gray.copy()

        # �m�C�Y�ጸ�̂��߂ڂ����A臒l120��2�l��
        ret, black_yarn_bin = cv2.threshold(cv2.blur(img_gray, (5, 5)), 120, WHITE, cv2.THRESH_BINARY_INV)

        # �������̈�̓m�C�Y�̂��ߏ��O
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=black_yarn_bin, connectivity=8)

        for label in range(1, labelnum):
            # ���labelcontours������ϐ��ɂ����
            x, y, ww, hh, size = labelcontours[label]
            if size < black_yarn_threshold_max:
                black_yarn_bin[labelimg == label] = BLACK

        bool_points = np.logical_and(black_yarn < 120, black_yarn_bin == WHITE)

        # ��f�̕��ςŖ��߂�
        # ���邳�̃��������邽��4�����ŏ���
        for i in range(0, 2):
            for j in range(0, 2):
                # �����Ώۗ̈��؂�o��
                work_img = black_yarn[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                           0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2]

                hh, ww = work_img.shape
                average = np.sum(work_img) / (hh * ww)
                work_bool_points = bool_points[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                                   0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2]
                work_img[work_bool_points] = average

                # �����Ώۗ̈��߂�
                black_yarn[0 + i * int(h / 2):int(h / 2) - 1 + i * int(h / 2) + h % 2,
                0 + j * int(w / 2):int(w / 2) - 1 + j * int(w / 2) + w % 2] = work_img

        # �m�C�Y�ጸ�̂��߂ڂ���(���̖Ԗڂ��Ԃ�)
        black_yarn = cv2.blur(black_yarn, (5, 5))
        # ================================================================================================================

        # ================================================================================================================
        # --�V�[���̈撊�o--
        # �K���I2�l��
        img_seam_bin = cv2.adaptiveThreshold(black_yarn, WHITE, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY,
                                             block_size, c)

        seam_area = img_seam_bin.copy()
        # ���̖��邳�ȉ��̕����͏��O
        # ���I�Ɍ���i����-10�j
        # ���邳�̃��������邽��25�����ŏ���
        for i in range(0, 5):
            for j in range(0, 5):
                # �����Ώۗ̈��؂�o��
                work_img = black_yarn[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]

                hh, ww = work_img.shape
                average = np.sum(work_img) / (hh * ww) - 10
                work_img = seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                img_gray_tmp = img_gray[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                               0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                work_img[img_gray_tmp < average] = BLACK

                # �����Ώۗ̈��߂�
                seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2] = work_img

        # -�m�C�Y�̏��O-
        # ���k����
        kernel = np.array([[0, 1, 0], [0, 1, 0], [0, 0, 0]], np.uint8)
        seam_area = cv2.erode(seam_area, kernel, iterations=1)
        kernel = np.array([[0, 0, 0], [1, 1, 0], [0, 0, 0]], np.uint8)
        seam_area = cv2.erode(seam_area, kernel, iterations=1)

        # �֊s�����o�Ō�����
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # �����ϊ�
        distimg = cv2.distanceTransform(seam_area, cv2.DIST_L1, 3)
        seam_area[distimg < 2] = BLACK

        # -���̈�~��-
        # ���̖��邳�ȏ�̕������~��
        # ���I�Ɍ���i����+20 �������� 210�j
        # ���邳�̃��������邽��25�����ŏ���
        for i in range(0, 5):
            for j in range(0, 5):
                # �����Ώۗ̈��؂�o��
                img_gray_tmp = img_gray[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                               0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]

                hh, ww = img_gray_tmp.shape
                img_gray_tmp = cv2.blur(img_gray_tmp, (5, 5))
                average = max(np.sum(img_gray_tmp) / (hh * ww) + 20, 210)
                work_img = seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                           0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2]
                work_img[img_gray_tmp >= average] = WHITE

                # �����Ώۗ̈��߂�
                seam_area[0 + i * int(h / 5):int(h / 5) - 1 + i * int(h / 5) + h % 2,
                0 + j * int(w / 5):int(w / 5) - 1 + j * int(w / 5) + w % 2] = work_img

        # -�m�C�Y����-
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            # �ʐς��������̈�̓m�C�Yor���_�̉\����
            if size <= remove_noise_threshold_max:
                seam_area[labelimg == label] = BLACK

        # �֊s�����o
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

        # ���ꂽ�̈���Ȃ���
        # �ɑ�����������(�኱�������)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, 30 + 20)

        # �ɑ��������k�����ōׂ߂�
        seam_area = cv2.erode(seam_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=15 + 10)

        # �֊s�����o�Ō�����
        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # -�m�C�Y����-
        labelnum, labelimg, contours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = contours[label]
            local_seam_area = np.zeros(img_org.shape[:2], np.uint8)
            local_seam_area[labelimg == label] = WHITE
            # ���ꏬ��(�摜�̒[�ɐڂ��Ă��Ȃ��̈�)�̓V�[���ł͂Ȃ��\��������
            local_contours, hierarchy = cv2.findContours(local_seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
            noize_flag = True
            for cnt in local_contours[0]:
                cnt = cnt[0]
                local_x = cnt[0]
                local_y = cnt[1]
                if local_x == 0 or local_y == 0 or local_x == w - 1 or local_y == h - 1:
                    noize_flag = False
                    break
            # �������̈�̓m�C�Y�i���_�j�̉\�����傫��
            if size <= remove_noise_size_max or noize_flag:
                seam_area[labelimg == label] = BLACK
        # ================================================================================================================

        # ================================================================================================================
        # --���ڑΉ�--
        # ��c�����p��2�l��
        img_border_bin = cv2.adaptiveThreshold(black_yarn, WHITE, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY,
                                               border_block_size, border_c)

        # -�m�C�Y����-
        # �c�����k
        img_border_bin = cv2.dilate(img_border_bin, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=1)
        img_border_bin = cv2.erode(img_border_bin, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=1)

        # �V�[���̈�����O
        # ���ڗp2�l���ł̓V�[�����{���{���ɂȂ�₷���A��������X�̏����Ō��_�ƌ������₷���Ȃ�
        work_img = seam_area.copy()
        work_img = cv2.dilate(work_img, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=5)
        img_border_bin[work_img == WHITE] = BLACK

        # -�V�[���p2�l���Ŕ�c�����̓_��傫������-
        # �����΍�
        img_seam_bin[black_yarn_bin == WHITE] = BLACK
        # �m�C�Y����
        # �����ϊ�
        distimg = cv2.distanceTransform(img_seam_bin, cv2.DIST_L1, 3)
        img_seam_bin[distimg < 2] = 0
        # ���k����
        kernel = np.array([[0, 1, 0], [0, 1, 0], [0, 0, 0]], np.uint8)
        img_seam_bin = cv2.erode(img_seam_bin, kernel, iterations=1)
        kernel = np.array([[0, 0, 0], [1, 1, 0], [0, 0, 0]], np.uint8)
        img_seam_bin = cv2.erode(img_seam_bin, kernel, iterations=1)

        # -��c�����̓_�p2�l���Ɣ�r-
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=img_border_bin, connectivity=8)
        seam_labelnum, seam_labelimg, seam_labelcontours, seam_GoCs = cv2.connectedComponentsWithStats(
            image=img_seam_bin,
            connectivity=8)
        img_border_bin = np.zeros(img_border_bin.shape[:2], np.uint8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            if img_seam_bin[y + int(hh / 2), x + int(ww / 2)] == WHITE:
                img_border_bin[seam_labelimg == seam_labelimg[y + int(hh / 2), x + int(ww / 2)]] = WHITE

        # ���̖ʐϓ��̗̈���c��
        # �傫������̈���m�C�Y�̉\������
        labelnum, labelimg, labelcontours, GoCs = cv2.connectedComponentsWithStats(image=img_border_bin, connectivity=8)
        non_expansion_area = np.zeros(img_border_bin.shape[:2], np.uint8)
        for label in range(1, labelnum):
            x, y, ww, hh, size = labelcontours[label]
            if size < border_non_expansion_area_max and size > border_non_expansion_area_min:
                non_expansion_area[y + int(hh / 2), x + int(ww / 2)] = WHITE

        # ���̋������ɂ���_������
        non_expansion_area_labelnum, non_expansion_area_labelimg, non_expansion_area_contours, non_expansion_area_GoCs = \
            cv2.connectedComponentsWithStats(image=non_expansion_area, connectivity=8)
        for label1 in range(1, non_expansion_area_labelnum):
            x1, y1, w1, h1, size1 = non_expansion_area_contours[label1]
            for label2 in range(label1 + 1, non_expansion_area_labelnum):
                x2, y2, w2, h2, size2 = non_expansion_area_contours[label2]
                # �Ƃ肠�������[�N���b�h����
                dist = np.sqrt((x2 - x1) ** 2 + (y2 - y1) ** 2)
                # ���܉��m�C�Y�΍�ŋ߂�������̂����O
                if border_dist_min < dist and dist < border_dist_max:
                    cv2.line(non_expansion_area, (x1, y1), (x2, y2), WHITE)

        # -�m�C�Y���O-

        contours, hierarchy = cv2.findContours(non_expansion_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        non_expansion_area = cv2.drawContours(np.zeros(non_expansion_area.shape[:2], np.uint8), contours, -1, WHITE, -1)

        # ����̂���, �ג������̂ŒZ�ӂ��Z�����̂͏��O
        non_expansion_area_labelnum, non_expansion_area_labelimg, non_expansion_area_contours, non_expansion_area_GoCs = cv2.connectedComponentsWithStats(
            image=non_expansion_area, connectivity=8)
        for label in range(1, non_expansion_area_labelnum):
            x, y, ww, hh, size = non_expansion_area_contours[label]
            work_img = np.zeros(img_org.shape[:2], np.uint8)
            work_img[non_expansion_area_labelimg == label] = WHITE
            distimg = cv2.distanceTransform(work_img, cv2.DIST_L1, 3)
            max_dist = np.max(distimg)
            # �ג������́��c���䂪����
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

        # �̈���������Ȃ���A������Ɩc��܂�
        # �̈�Ɍ����J���̂��ł��邾���h��
        non_expansion_area = cv2.dilate(non_expansion_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
                                        iterations=50)
        non_expansion_area = cv2.erode(non_expansion_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)),
                                       iterations=35)

        # ��c�����̗֊s�����o

        contours, hierarchy = cv2.findContours(non_expansion_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        non_expansion_area = cv2.drawContours(np.zeros(non_expansion_area.shape[:2], np.uint8), contours, -1, WHITE, 10)
        # ================================================================================================================

        # ================================================================================================================
        # --��c�����̈�ƃV�[���̈���d�˂�--
        seam_area[non_expansion_area == WHITE] = WHITE

        # �֊s�����o

        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, 30)
        seam_area = cv2.erode(seam_area, cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (3, 3)), iterations=15)

        contours, hierarchy = cv2.findContours(seam_area, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
        seam_area = cv2.drawContours(np.zeros(seam_area.shape[:2], np.uint8), contours, -1, WHITE, -1)
        # ================================================================================================================

        # ================================================================================================================
        # --�V�[���ŕ������ꂽ�̈�𒲂ׂ�--
        # ���̈�����x�����O����̂ŁA�������]
        seam_area = cv2.bitwise_not(seam_area)

        # -�}�X�N�摜�쐬-
        img_mask = np.zeros(img_org.shape[:2], np.uint8)
        labelnum, labelimg, contours, GoCs = cv2.connectedComponentsWithStats(image=seam_area, connectivity=8)
        for label in range(1, labelnum):
            img_label = np.zeros(seam_area.shape[:2], np.uint8)
            img_label[labelimg == label] = WHITE
            bool_points = np.logical_and(img_border_bin == WHITE, img_label == WHITE)
            img_ck_area = np.zeros(img_org.shape[:2], np.uint8)
            img_ck_area[bool_points] = WHITE

            # �`�F�b�N�̈���̔���f�̌ł܂�̐��𐔂���
            # �ʐς��ɏ��̗̈�͏��O
            white_area_cnt = 0
            ck_area_labelnum, ck_area_labelimg, ck_area_contours, ck_area_GoCs = cv2.connectedComponentsWithStats(
                image=img_ck_area, connectivity=8)
            for ck_area_label in range(1, ck_area_labelnum):
                x, y, ww, hh, size = ck_area_contours[ck_area_label]
                if size < white_area_size_max and size > white_area_size_min:
                    white_area_cnt = white_area_cnt + 1
            # �`�F�b�N�̈�̖ʐςɑ΂��Ĕ���f�̌ł܂�(��c�����̓_)�̐�����������(����)���邩�Ŕ���
            img_label[img_label == WHITE] = 1
            if white_area_cnt / np.sum(img_label) >= white_area_cnt_sum_min / (100 * 100):
                img_mask[img_label == 1] = WHITE
            # ���̖ʐϔ䗦��10%�ȏ���}�X�N
            work_img = img_ck_area.copy()
            work_img[work_img == WHITE] = 1
            if np.sum(work_img) / np.sum(img_label) > 0.1:
                img_mask[img_label == 1] = WHITE
            # �_�̐��Ɣ��̖ʐς̕�������
            # �ʐϔ䗦��1%�ȏ�A���A�_�̐������a�ł��}�X�N
            if np.sum(work_img) / np.sum(img_label) > work_img_min and \
                    white_area_cnt / np.sum(img_label) >= white_area_cnt_min / (100 * 100):
                img_mask[img_label == 1] = WHITE

        # �V�[���̈挋�ʂ��}�X�N�摜�ɔ��f
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
        # �G���[�ɂȂ����摜�����o�͂���B
        logger.error('[%s:%s] �G���[�Ώۉ摜��=%s ' % (app_id, app_name, fname))
        raise e


# ------------------------------------------------------------------------------------
# ������             �F��c���������������
#
# �����T�v           �F1.��c�����̔��菈���ֈ�����n���B
#
# ����               �F����
#
# �߂�l             �F��c�������菈������
# ------------------------------------------------------------------------------------
def wrapper_check_rate(args):
    # ��c�������菈���ֈ�����n��
    result = check_rate(*args)
    return result


# ------------------------------------------------------------------------------------
# ������             �F��c�������菈��
#
# �����T�v           �F1.�}�X�L���O���s�����摜�̔��菈�����s���B
#
# ����               �F�}�X�L���O���茋��
#                      �}�X�N�摜�f�[�^
#                      �[����臒l
#                      �J�e�S���[��(NG)
#                      �J�e�S���[��(others)
#
# �߂�l             �F��c�������菈������
# ------------------------------------------------------------------------------------
def check_rate(ng_result, mask_images, masking_rate, label_ng, label_others):
    try:
        # �摜�f�[�^���画��l���Z�o����
        img_bin = mask_images[ng_result["filename"]]
        img_mask_crop = img_bin[ng_result["point"]["y"]:ng_result["point"]["y"] + ng_result["height"],
                        ng_result["point"]["x"]:ng_result["point"]["x"] + ng_result["width"]]
        img_mask_crop[img_mask_crop == WHITE] = 1
        rate = np.sum(img_mask_crop) / (100 * 100)

        rate_result = {}
        # ����l
        if rate < masking_rate:
            rate_result[ng_result["filename"] + "," + str(ng_result["point"]["x"]) + "," + str(
                ng_result["point"]["y"])] = label_ng
        else:
            rate_result[ng_result["filename"] + "," + str(ng_result["point"]["x"]) + "," + str(
                ng_result["point"]["y"])] = label_others

        return rate_result
    except Exception as e:
        # �G���[�ɂȂ����摜�����o�͂���B
        raise e


# ------------------------------------------------------------------------------------
# ������             �F�}�X�L���O����
#
# �����T�v           �F1.�摜�ɑ΂��ă}�X�L���O�������s���A���ʂ𔻒肷��B
#
# ����               �F�[���茋��CSV�t�@�C���p�X
#                      �}�X�L���O���茋��CSV�t�@�C���p�X
#                      NG�J�e�S��
#                      OK�J�e�S��
#                      ������񃊃X�g
#                      �[����臒l
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def base_masking_fabric(checkfabric_result_file, masking_result_file, label_ng, label_others, process_list,
                        masking_rate):
    result = False
    try:
        # ���ʃt�@�C���ǂݍ���
        logger.debug('[%s:%s] ���ʃt�@�C���Ǎ����J�n���܂��B' % (app_id, app_name))

        tmp_result, result_data, error = file_util.read_result_file(checkfabric_result_file, logger, app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] ���ʃt�@�C���Ǎ����I�����܂����B%s ' % (app_id, app_name, result_data))
        else:
            logger.error('[%s:%s] ���ʃt�@�C���Ǎ������s���܂����B' % (app_id, app_name))
            return result, process_list

        # �B���摜�f�B���N�g���ϐ���`
        image_dir = result_data["datapath"]

        # ---�}�X�N�摜�쐬����---
        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�����������J�n���܂��B' % (app_id, app_name))

        # ���񏈗��p�Ɉ���������
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

        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�����������I�����܂����B' % (app_id, app_name))

        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�����̕�����s���J�n���܂��B' % (app_id, app_name))
        # ���񏈗����s
        # p = Pool(multi.cpu_count())
        p = Pool(4)
        res = p.map(wrapper_filter_non_inflatable_portion, process_file_args)
        p.close()
        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�����̕�����s���I�����܂����B' % (app_id, app_name))

        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�������ʎ擾���J�n���܂��B' % (app_id, app_name))
        # ���񏈗����ʎ擾
        mask_images = {}
        for data in res:
            if data is None:
                continue
            for key, value in data.items():
                mask_images[key] = value
        logger.debug('[%s:%s] ��c�����}�X�N�摜�쐬�������ʎ擾���I�����܂����B' % (app_id, app_name))

        # ---��c�������菈��---
        logger.debug('[%s:%s] ��c�������菈���������J�n���܂��B' % (app_id, app_name))
        # ���񏈗��p�Ɉ���������
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
        logger.debug('[%s:%s] ��c�������菈���������I�����܂����B' % (app_id, app_name))

        logger.debug('[%s:%s] ��c�������菈���̕�����s���J�n���܂��B' % (app_id, app_name))
        # ���񏈗����s
        # p = Pool(multi.cpu_count())
        p = Pool(4)
        res = p.map(wrapper_check_rate, process_file_args)
        p.close()
        logger.debug('[%s:%s] ��c�������菈���̕�����s���I�����܂����B' % (app_id, app_name))

        logger.debug('[%s:%s] ��c�������菈�����ʂ̎擾���J�n���܂��B' % (app_id, app_name))
        # ���񏈗����ʎ擾
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

        # �������i����ID�A�B���摜�A�A�ԁj�ƃ}�X�L���O���茋�ʂ̕R�Â����s���B
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

        logger.debug('[%s:%s] ��c�������菈�����ʂ̎擾���I�����܂����B' % (app_id, app_name))

        logger.debug('[%s:%s] �}�X�L���O���茋��CSV�t�@�C���̏o�͂��J�n���܂��B' % (app_id, app_name))
        # �}�X�L���O���茋��CSV�t�@�C�����o�͂���
        tmp_result, error = file_util.write_result_file(masking_result_file, result_data, image_dir, logger, app_id, app_name)
        if tmp_result:
            logger.debug('[%s:%s] �}�X�L���O���茋��CSV�t�@�C���̏o�͂��I�����܂����B' % (app_id, app_name))
        else:
            logger.error('[%s:%s] �}�X�L���O���茋��CSV�t�@�C���̏o�͂����s���܂����B' % (app_id, app_name))
            return result, process_list

        result = True


    except Exception as e:
        error_detail.exception(e, logger, app_id, app_name)

    return result, process_list


# ------------------------------------------------------------------------------------
# ������             �F���C������
#
# �����T�v           �F1.�[���茋��CSV��Ǎ���ŁA�摜�ɑ΂��ă}�X�L���O������s���A���ʂ��o�͂���B
#                      2.�摜�}�[�L���O�������Ăяo���B
#
# ����               �F�R�l�N�V�����I�u�W�F�N�g
#                      �J�[�\���I�u�W�F�N�g
#                      �[���茋��CSV�t�@�C���p�X
#                      �}�X�L���O���茋��CSV�t�@�C���p�X
#                      �J�e�S���[��(NG)
#                      �J�e�S���[��(others)
#                      ������񃊃X�g
#
# �߂�l             �F�������ʁiTrue:�����AFalse:���s�j
# ------------------------------------------------------------------------------------
def main(checkfabric_result_file, masking_result_file, label_ng, label_others, process_list, inspection_date):
    result = False
    try:

        # �ݒ�t�@�C������l���擾

        masking_rate = float(inifile.get('VALUE', 'masking_rate'))

        # �ϐ���`
        fabric_info = os.path.basename(masking_result_file).split('_')
        fabric_name = fabric_info[1]
        inspection_num = fabric_info[3]

        logger.info('[%s:%s] %s�������J�n���܂��B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                    (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        # �}�X�L���O�����E������s��
        result, process_list = base_masking_fabric(checkfabric_result_file, masking_result_file, label_ng,
                                                   label_others, process_list, masking_rate)

        if result:
            logger.info('[%s:%s] %s����������ɏI�����܂����B [����, �����ԍ�, �������t]=[%s, %s, %s]' %
                        (app_id, app_name, app_name, fabric_name, inspection_num, inspection_date))

        else:
            logger.error('[%s:%s] %s���������s���܂����B' % (app_id, app_name, app_name))
            return result, process_list

        result = True

    except Exception as e:
        # �z��O�G���[����
        logger.error('[%s:%s] �\�����Ȃ��G���[���������܂����B' % (app_id, app_name))
        logger.error(traceback.format_exc())

    return result, process_list
