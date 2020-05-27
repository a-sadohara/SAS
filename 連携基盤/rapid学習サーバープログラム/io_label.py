# -*- coding: utf-8 -*-
#----------------------------------------
# ■ マーキングラベルファイルの入出力処理
#----------------------------------------

import codecs

#==============================================
# RAPIDのマーキングラベルファイルを読み込む
#==============================================
def readLabelFile(label_file):
    label_data = {}
    
    # ファイルを開く
    fp = codecs.open(label_file, "r", "utf-8")
    line = fp.readline()
    
    # ヘッダ部分を読み込む
    if line.find("[DataPath]") != -1:
        itemList = line[:-1].split("[DataPath]")[1]
        datapath = itemList.split("\"")[1]
        label_data["datapath"] = datapath
        line = fp.readline()
    if line.find("[LabelType]") != -1:
        line = fp.readline()
    
    datas = []
    # 1行ずつ処理
    while line:
        # マーキングラベルファイルのフォーマット
        # 3,"<ファイル名>",<ラベル>,<X座標>,<Y座標>・・・
        data = {}
        
        # 一応、ヘッダ部分でないか確認
        if line.find("[") != -1:
            line = fp.readline()
            continue
        
        # 『,』で区切る
        itemList = line.strip().split(",")
        
        # 対象外の場合は無視
        if itemList[0] == "0":
            line = fp.readline()
            continue
        
        # ファイル名
        data["filename"] = itemList[1].split("\"")[1]
        points = []
        # マーキングされていれば、ラベルと座標を取得
        if len(itemList) >= 3:
            # ラベル
            data["label"] = itemList[2]
            # マーキング座標
            for idx in range(3, len(itemList), 2):
                point = {}
                point["x"] = int(itemList[idx])
                point["y"] = int(itemList[idx + 1])
                # 評価用に座標点に検知フラグを用意
                point["detect"] = False
                points.append(point)
        data["points"] = points
        # 評価用に未検知フラグを用意
        data["undetect"] = True
        datas.append(data)
        line = fp.readline()
    label_data["data"] = datas
    fp.close()
    
    return label_data
