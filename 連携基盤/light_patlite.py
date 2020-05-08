# -*- coding: SJIS -*-
# ----------------------------------------
# ■ 共通機能 パトライト鳴動
# ----------------------------------------

import urllib.request
import configparser

# 設定ファイル読み込み
inifile = configparser.ConfigParser()
inifile.read('D:/CI/programs/config/light_patlite_config.ini', 'SJIS')

# ------------------------------------------------------------------------------------
# 関数名             ：パトライト鳴動
#
# 処理概要           ：1.パトライトを点灯する
#
# 引数               ：点灯パターン
#                      ロガー
#                      機能ID
#                      機能名
#
# 戻り値             ：処理結果（True:成功、False:失敗）
# ------------------------------------------------------------------------------------
def light_patlite(light_pattern, logger, app_id, app_name):
    ip_address = inifile.get('PATLITE_INFO', 'ip_address')
    url = 'http://' + ip_address + '/api/control?alert=' + light_pattern
    res = urllib.request.urlopen(url)
    body = str(res.read())
    if body == 'b\'Success.\'':
        result = True
    else:
        result = False

    return result
