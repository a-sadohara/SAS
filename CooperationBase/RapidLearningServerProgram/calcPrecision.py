# -*- coding: utf-8 -*-
#----------------------------------------
# ■ 精度評価
#----------------------------------------

import os
import argparse
import codecs

import io_result
import io_label

# 出力ファイル名
OUT_FILE_NAME = ["accuracy.txt", "un-detection.txt", "mis-detection.txt"]

#==============================================
# 実行時パラメータ設定
#==============================================
def init_args():
    parser = argparse.ArgumentParser()
    
    parser.add_argument("-labelfile",   dest = "label_file",  type = str, help = "label file path",  required = True)
    parser.add_argument("-resultfile",  dest = "result_file", type = str, help = "result file path", required = True)
    parser.add_argument("-o",           dest = "output_dir",  type = str, help = "output directory", required = True)
    
    return parser.parse_args()

#==============================================
# 改行付きファイル出力
#==============================================
def writeLine(fp, string):
    fp.write(string + "\n")

#==============================================
# 検知、未検知、過検知チェック
#==============================================
def check_detect(label_data, result_data):
    for label in label_data["data"]:
        for result in result_data["data"]:
            # 違うファイルの場合は無視
            if result["filename"] != label["filename"]:
                continue
            # 検知結果が『_others』(正常)の場合は無視
            if result["label"] == "_others":
                continue
            # マーキング座標をチェック
            for point in label["points"]:
                # マーキング座標が結果矩形の内側かチェック
                if result["point"]["x"] <= point["x"] <= result["point"]["x"] + result["width"] and result["point"]["y"] <= point["y"] <= result["point"]["y"] + result["height"]:
                    # 結果矩形の内側の場合はその座標の検知フラグを立てる
                    point["detect"] = True
                    # 結果矩形の個所の過検知フラグを下げる(どこかのマーキング座標が内側にある場合は少なくとも過検知ではない)
                    result["missdetect"] = False
        
        # マーキング座標がない場合の対応
        if len(label["points"]) == 0:
            # 結果ファイル内に含まれていれば、未検知フラグを下げる
            label["undetect"] = False
            continue
        
        # マーキング座標の検知数を数える
        detect_point = 0
        for point in label["points"]:
            if point["detect"]:
                detect_point = detect_point + 1
        # どこかの座標が検知されていれば、未検知フラグを下げる
        if detect_point >= 1:
            label["undetect"] = False # 未検知フラグを下げる
    return label_data, result_data

#==============================================
# 検知数、未検知数、過検知数を数える
#==============================================
def calc_precision(label_data, result_data):
    true_detect_num = 0 # 検知成功数
    undetect_num = 0    # 未検知数
    missdetect_num = 0  # 過検知数
    
    image_list = []        # 評価画像を格納
    undetect_images = []   # 未検知画像を格納
    missdetect_images = [] # 過検知画像を格納
    
    # 過検知数を数える
    for label in label_data["data"]:
        # 未検知フラグをチェック
        if label["undetect"]:
            undetect_num = undetect_num + 1
            if label["filename"] not in undetect_images:
                undetect_images.append(label["filename"])
        else:
            true_detect_num = true_detect_num + 1
    # 過検知数を数える
    for result in result_data["data"]:
        # 過検知フラグをチェック
        if result["missdetect"]:
            missdetect_num = missdetect_num + 1
            if result["filename"] not in missdetect_images:
                missdetect_images.append(result["filename"])
        # 評価画像数チェックのため
        if result["filename"] not in image_list:
            image_list.append(result["filename"])
    
    return true_detect_num, undetect_num, missdetect_num, image_list, undetect_images, missdetect_images

#==============================================
# メイン関数
#==============================================
def main():
    parsed = init_args()
    
    # ラベルファイル読み込み
    label_file = parsed.label_file.strip()
    label_data = io_label.readLabelFile(label_file)
    # 結果ファイル読み込み
    result_file = parsed.result_file.strip()
    result_data = io_result.readResultFile(result_file)
    # 出力先フォルダ
    output_dir = parsed.output_dir.strip()
    os.makedirs(output_dir, exist_ok = True)
    
    # ---検知、未検知、過検知チェック---
    label_data, result_data = check_detect(label_data, result_data)
    # ---検知数、未検知数、過検知数を数える---
    true_detect_num, undetect_num, missdetect_num, image_list, undetect_images, missdetect_images = calc_precision(label_data, result_data)
    # マーキング数
    label_num = len(label_data["data"])
    # 画像数を計算
    image_num = len(image_list)
    undetect_image_num = len(undetect_images)
    missdetect_image_num = len(missdetect_images)
    
    # ---ファイル出力---
    fp = codecs.open(output_dir + "\\" + OUT_FILE_NAME[0], "w", "utf-8")
    # 画像単位
    writeLine(fp, "検知成功率(Image)       {:>7.2%} ({}/{})".format(((image_num - undetect_image_num) / image_num), (image_num - undetect_image_num), image_num))
    writeLine(fp, "未検知率(Image)         {:>7.2%} ({}/{})".format((undetect_image_num / image_num), undetect_image_num, image_num))
    writeLine(fp, "過検知率(Image)         {:>7.2%} ({}/{})".format((missdetect_image_num / image_num), missdetect_image_num, image_num))
    # マーキング単位
    if label_num != 0:
        writeLine(fp, "検知成功率(Marking)     {:>7.2%} ({}/{})".format((true_detect_num / label_num), true_detect_num, label_num))
        writeLine(fp, "未検知率(Marking)       {:>7.2%} ({}/{})".format((undetect_num / label_num), undetect_num, label_num))
    fp.close()
    
    # 未検知画像リストの出力
    fp = codecs.open(output_dir + "\\" + OUT_FILE_NAME[1], "w", "utf-8")
    for data in undetect_images:
        writeLine(fp, data)
    fp.close()
    
    # 過検知画像リストの出力
    fp = codecs.open(output_dir + "\\" + OUT_FILE_NAME[2], "w", "utf-8")
    for data in missdetect_images:
        writeLine(fp, data)
    fp.close()

if __name__ == "__main__":
    main()
    
