@echo off

:: 設定ファイルパス
set conf_file=D:\CI\programs\config\rapid_model_config.ini
set rapid_model_path=D$\AI\

:: 設定ファイル読込
for /F "usebackq eol=# tokens=1,2 delims==" %%a in ("%conf_file%") do (
    set %%a=%%b
)

:: モデルファイルフォルダ選択
set model_file_path=%1
REM echo MsgBox "モデルファイルフォルダ選択画面に遷移します。" ^& vbCr ^& "モデルファイルフォルダが格納されているフォルダを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
REM set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'モデルファイルが格納されているフォルダを選択してください。',0,0).self.path""

REM for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "model_file_path=%%I"
REM if not "%model_file_path%" == "" (
REM  echo You chose "%model_file_path%"
REM ) else (
REM   exit /B false
REM )
ECHO モデルファイルフォルダ:%model_file_path%
set model_file="%model_file_path%"

set model_file=%model_file:"=%
set model_file=%model_file:/=\%
if "%model_file:~-1%"=="\" (set model_file=%model_file:~0,-1%)

:loop_model_file
set model_file=%model_file:*\=%
if not "%model_file:*\=%"=="%model_file%" (goto loop_model_file)
echo %model_file%


:: モデル名入力
set model_name=%2
ECHO モデルファイルフォルダ:%model_name%
REM echo Input = InputBox("インポートするモデル名を入力してください。") > %TEMP%\inputmodel.vbs
REM echo Wscript.Echo(Input) >> %TEMP%\inputmodel.vbs
REM for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\inputmodel.vbs`) do set "model_name=%%f"
REM if "%model_name%" == "" (
REM    :: 入力されなかった（未入力、もしくは、キャンセル）場合は終了する
REM    exit /b
REM )
ECHO ■モデルファイルを配布します ----------------------＞
xcopy "%model_file_path%" \\%HOST_1%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_2%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_3%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_4%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_5%\%rapid_model_path% /s/e/i

ECHO ■AIモデルを登録します       ----------------------＞
echo WMIC /NODE:192.168.164.128 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_1%" -m {%model_name%,\\%HOST_1%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.129 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_2%" -m {%model_name%,\\%HOST_2%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.130 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_3%" -m {%model_name%,\\%HOST_3%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.131 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_4%" -m {%model_name%,\\%HOST_4%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.132 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_5%" -m {%model_name%,\\%HOST_5%\%rapid_model_path%\%model_file%}"

REM echo Input = MsgBox("RAPIDモデルインポートが完了しました。" , vbOK + vbInformation, "情報") > %TEMP%\msgbox.vbs
REM echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
REM for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
REM if not "!confirm!" == "1" (
REM    :: OKが選択されなかった入力されなかった（未入力、もしくは、キャンセル）場合は終了する
REM    exit /b
REM )