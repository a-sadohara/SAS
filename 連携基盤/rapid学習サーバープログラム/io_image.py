# -*- coding: utf-8 -*-
#----------------------------------------
# ■ 画像ファイルの入出力処理(opencv 日本語対応)
#----------------------------------------

import os;
import cv2;
import numpy as np;

#==============================================
# 日本語対応用 opencvの画像読み込み
#==============================================
def imread(filename, flags = cv2.IMREAD_COLOR, dtype = np.uint8):
    try:
        n = np.fromfile(filename, dtype);
        img = cv2.imdecode(n, flags);
        return img;
    except Exception as e:
        print(e);
        return None;

#==============================================
# 日本語対応用 opencvの画像書き込み
#==============================================
def imwrite(filename, img, params = None):
    try:
        os.mkdir(os.path.dirname(filename));
    except FileExistsError:
        pass;
    try:
        ext = os.path.splitext(filename)[1];
        result, n = cv2.imencode(ext, img, params);
        if result:
            with open(filename, mode = "w+b") as f:
                n.tofile(f);
            return True;
        else:
            return False;
    except Exception as e:
        print(e);
        return False;

