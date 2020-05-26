# -*- coding: utf-8 -*-
#----------------------------------------
# ■ 画像の明るさ補正
#----------------------------------------

import os
import argparse
import glob
import cv2

import io_image

from multiprocessing import Pool
import multiprocessing as multi

#==============================================
# 実行時パラメータ設定
#==============================================
def init_args():
    parser = argparse.ArgumentParser()
    
    parser.add_argument("-i", dest = "input_dir",  type = str,   help = "input image directory",    required = True)
    parser.add_argument("-o", dest = "output_dir", type = str,   help = "output image directory",   required = True)
    parser.add_argument("-v", dest = "value",      type = float, help = "adjust value",             required = True)
    
    return parser.parse_args()

#==============================================
# 明るさ補正処理
#==============================================
def wrapper_process_file(args):
    process_file(*args)
def process_file(file, output_dir, value):
    # ---画像読み込み---
    img = io_image.imread(file)
    # ---画像サイズ取得---
    h, w = img.shape[:2]
    
    # ---明るさ補正---
    # RGB→HSVに変換
    img_hsv = cv2.cvtColor(img, cv2.COLOR_BGR2HSV)
    # HSVのVの値を補正
    for y in range(0, h):
        for x in range(0, w):
            img_hsv[y, x][2] = max(0, min(255, int(((img_hsv[y, x][2] / 255.0) + value) * 255)))
    # HSV→RGBに変換
    img = cv2.cvtColor(img_hsv, cv2.COLOR_HSV2BGR)
    
    # ---画像出力---
    io_image.imwrite(output_dir + "\\" + os.path.basename(file), img)

#==============================================
# メイン関数
#==============================================
def main():
    parsed = init_args()
    
    # ---実行時パラメータ取得---
    # 対象ファイル取得
    files = glob.glob(parsed.input_dir.strip() + "\\" + "*.jpg")
    
    # 出力先フォルダ
    output_dir = parsed.output_dir.strip()
    os.makedirs(output_dir, exist_ok = True)
    
    # 補正値
    value = parsed.value
    
    # ---並行処理---
    # 引数の準備
    process_file_args = [[file, output_dir, value] for file in files]
    # 並行実行
    p = Pool(multi.cpu_count())
    p.map(wrapper_process_file, process_file_args)
    p.close()

if __name__ == "__main__":
    main()
