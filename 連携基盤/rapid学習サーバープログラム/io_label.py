# -*- coding: utf-8 -*-
#----------------------------------------
# �� �}�[�L���O���x���t�@�C���̓��o�͏���
#----------------------------------------

import codecs

#==============================================
# RAPID�̃}�[�L���O���x���t�@�C����ǂݍ���
#==============================================
def readLabelFile(label_file):
    label_data = {}
    
    # �t�@�C�����J��
    fp = codecs.open(label_file, "r", "utf-8")
    line = fp.readline()
    
    # �w�b�_������ǂݍ���
    if line.find("[DataPath]") != -1:
        itemList = line[:-1].split("[DataPath]")[1]
        datapath = itemList.split("\"")[1]
        label_data["datapath"] = datapath
        line = fp.readline()
    if line.find("[LabelType]") != -1:
        line = fp.readline()
    
    datas = []
    # 1�s������
    while line:
        # �}�[�L���O���x���t�@�C���̃t�H�[�}�b�g
        # 3,"<�t�@�C����>",<���x��>,<X���W>,<Y���W>�E�E�E
        data = {}
        
        # �ꉞ�A�w�b�_�����łȂ����m�F
        if line.find("[") != -1:
            line = fp.readline()
            continue
        
        # �w,�x�ŋ�؂�
        itemList = line.strip().split(",")
        
        # �ΏۊO�̏ꍇ�͖���
        if itemList[0] == "0":
            line = fp.readline()
            continue
        
        # �t�@�C����
        data["filename"] = itemList[1].split("\"")[1]
        points = []
        # �}�[�L���O����Ă���΁A���x���ƍ��W���擾
        if len(itemList) >= 3:
            # ���x��
            data["label"] = itemList[2]
            # �}�[�L���O���W
            for idx in range(3, len(itemList), 2):
                point = {}
                point["x"] = int(itemList[idx])
                point["y"] = int(itemList[idx + 1])
                # �]���p�ɍ��W�_�Ɍ��m�t���O��p��
                point["detect"] = False
                points.append(point)
        data["points"] = points
        # �]���p�ɖ����m�t���O��p��
        data["undetect"] = True
        datas.append(data)
        line = fp.readline()
    label_data["data"] = datas
    fp.close()
    
    return label_data
