@echo off

:: 設定ファイルパス
set conf_file=D:\CI\programs\config\rapid_model_import_config.ini
set remort_rapid_model_path=D$\AI\model\
set local_rapid_model_path=D:\AI\model\

:: 設定ファイル読込
for /F "usebackq eol=# tokens=1,2 delims==" %%a in ("%conf_file%") do (
    set %%a=%%b
)

:: モデルファイルフォルダ選択
echo MsgBox "モデルファイルフォルダ選択画面に遷移します。" ^& vbCr ^& "モデルファイルフォルダが格納されているフォルダを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'モデルファイルが格納されているフォルダを選択してください。',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "model_file_path=%%I"
if not "%model_file_path%" == "" (
  echo You chose "%model_file_path%"
) else (
  exit /B false
)

set model_file="%model_file_path%"

set model_file=%model_file:"=%
set model_file=%model_file:/=\%
if "%model_file:~-1%"=="\" (set model_file=%model_file:~0,-1%)

:loop_model_file
set model_file=%model_file:*\=%
if not "%model_file:*\=%"=="%model_file%" (goto loop_model_file)
echo %model_file%


:: モデル名入力
echo Input = InputBox("インポートするモデル名を入力してください。") > %TEMP%\inputmodel.vbs
echo Wscript.Echo(Input) >> %TEMP%\inputmodel.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\inputmodel.vbs`) do set "model_name=%%f"
if "%model_name%" == "" (
    :: 入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
) 

xcopy "%model_file_path%" \\%HOST_1%\%remort_rapid_model_path%\%model_file% /s/e/i
xcopy "%model_file_path%" \\%HOST_2%\%remort_rapid_model_path%\%model_file% /s/e/i
xcopy "%model_file_path%" \\%HOST_3%\%remort_rapid_model_path%\%model_file% /s/e/i
xcopy "%model_file_path%" \\%HOST_4%\%remort_rapid_model_path%\%model_file% /s/e/i
xcopy "%model_file_path%" \\%HOST_5%\%remort_rapid_model_path%\%model_file% /s/e/i

echo rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_1%" -m {%model_name%,%local_rapid_model_path%%model_file%} > \\%HOST_1%\%remort_rapid_model_path%\model_import.bat
echo rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_2%" -m {%model_name%,%local_rapid_model_path%%model_file%} > \\%HOST_2%\%remort_rapid_model_path%\model_import.bat
echo rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_3%" -m {%model_name%,%local_rapid_model_path%%model_file%} > \\%HOST_3%\%remort_rapid_model_path%\model_import.bat
echo rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_4%" -m {%model_name%,%local_rapid_model_path%%model_file%} > \\%HOST_4%\%remort_rapid_model_path%\model_import.bat
echo rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_5%" -m {%model_name%,%local_rapid_model_path%%model_file%} > \\%HOST_5%\%remort_rapid_model_path%\model_import.bat

WMIC /NODE:192.168.164.129 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %local_rapid_model_path%\model_import.bat -verb runas"
WMIC /NODE:192.168.164.130 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %local_rapid_model_path%\model_import.bat -verb runas"
WMIC /NODE:192.168.164.131 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %local_rapid_model_path%\model_import.bat -verb runas"
WMIC /NODE:192.168.164.132 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %local_rapid_model_path%\model_import.bat -verb runas"
WMIC /NODE:192.168.164.133 /user:sas_admin /password:p@ssw0rdnec12345 process call create "powershell start-process %local_rapid_model_path%\model_import.bat -verb runas"

echo Input = MsgBox("RAPIDモデルインポートが完了しました。" , vbOK + vbInformation, "情報") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OKが選択されなかった入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)