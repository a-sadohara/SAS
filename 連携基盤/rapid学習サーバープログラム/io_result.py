# -*- coding: utf-8 -*-
#----------------------------------------
# ■ resultファイルの入出力処理
#----------------------------------------

import codecs

# 定数
LABEL_NAME = ["NG", "_others"]

# 改行付きのファイル出力
def writeLine(fp, string):
    fp.write(string + "\n")

# RAPIDの結果ファイルを読み込む
def readResultFile(result_file):
    result_data = {}
    fp = codecs.open(result_file, "r", "utf-8")
    line = fp.readline()
    if line.find("[DataPath]") != -1:
        itemList = line[:-1].split("[DataPath]")[1]
        datapath = itemList.split("\"")[1]
        result_data["datapath"] = datapath
        line = fp.readline()
    else:
        result_data["datapath"] = ""
    datas = []
    while line:
        # 結果ファイルのフォーマット
        # 3,"<ファイル名>",<X座標>,<Y座標>,<幅>,<高さ>,<ラベル>,<確信度>・・・
        #もしくは
        # "<ファイル名>",<X座標>,<Y座標>,<幅>,<高さ>,<ラベル>,<確信度>・・・
        data = {}
        if line.find("[") != -1:
            line = fp.readline()
            continue
        itemList = line.strip().split(",")
        idx = 0
        if itemList[0] == "3":
            idx = 1
        data["filename"] = itemList[idx].split("\"")[1]
        point = {}
        point["x"] = int(itemList[idx + 1])
        point["y"] = int(itemList[idx + 2])
        data["point"] = point
        data["width"] = int(itemList[idx + 3])
        data["height"] = int(itemList[idx + 4])
        data["label"] = itemList[idx + 5]
        rate = {}
        for cnt in range(0, len(LABEL_NAME)):
            rate[itemList[idx + 5 + (2 * cnt)]] = float(itemList[idx + 6 + (2 * cnt)])
        data["rate"] = rate
        data["duplication"] = 1
        # 過検知フラグ
        if data["label"] == "_others":
            data["missdetect"] = False
        else:
            data["missdetect"] = True
        datas.append(data)
        line = fp.readline()
    result_data["data"] = datas
    fp.close()
    
    return result_data

# RAPIDの結果ファイルのフォーマットで出力
def writeResultFile(output_file, result_data, datapath = ""):
    fp = codecs.open(output_file, "w", "utf-8")
    if result_data["datapath"] == "":
        writeLine(fp, "[DataPath]\"" + result_data["datapath"] + "\"")
    else:
        writeLine(fp, "[DataPath]\"" + datapath + "\"")
    for result in result_data["data"]:
        buf = "3"
        buf = buf + "," + "\"" + result["filename"] + "\""
        buf = buf + "," + str(result["point"]["x"])
        buf = buf + "," + str(result["point"]["y"])
        buf = buf + "," + str(result["width"])
        buf = buf + "," + str(result["height"])
        if result["label"] != "NG":
            buf = buf + "," + "_others"
            buf = buf + "," + str(result["rate"]["_others"])
            buf = buf + "," + "NG"
            buf = buf + "," + "0"
        else:
            buf = buf + "," + "NG"
            buf = buf + "," + str(result["rate"]["NG"])
            buf = buf + "," + "_others"
            buf = buf + "," + "0"
        writeLine(fp, buf)
    fp.close()

