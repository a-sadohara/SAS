@echo off

:: �ݒ�t�@�C���p�X
set conf_file=D:\CI\programs\config\rapid_model_config.ini
set rapid_model_path=D$\AI\

:: �ݒ�t�@�C���Ǎ�
for /F "usebackq eol=# tokens=1,2 delims==" %%a in ("%conf_file%") do (
    set %%a=%%b
)

:: ���f���t�@�C���t�H���_�I��
set model_file_path=%1
REM echo MsgBox "���f���t�@�C���t�H���_�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "���f���t�@�C���t�H���_���i�[����Ă���t�H���_��I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
REM set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'���f���t�@�C�����i�[����Ă���t�H���_��I�����Ă��������B',0,0).self.path""

REM for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "model_file_path=%%I"
REM if not "%model_file_path%" == "" (
REM  echo You chose "%model_file_path%"
REM ) else (
REM   exit /B false
REM )
ECHO ���f���t�@�C���t�H���_:%model_file_path%
set model_file="%model_file_path%"

set model_file=%model_file:"=%
set model_file=%model_file:/=\%
if "%model_file:~-1%"=="\" (set model_file=%model_file:~0,-1%)

:loop_model_file
set model_file=%model_file:*\=%
if not "%model_file:*\=%"=="%model_file%" (goto loop_model_file)
echo %model_file%


:: ���f��������
set model_name=%2
ECHO ���f���t�@�C���t�H���_:%model_name%
REM echo Input = InputBox("�C���|�[�g���郂�f��������͂��Ă��������B") > %TEMP%\inputmodel.vbs
REM echo Wscript.Echo(Input) >> %TEMP%\inputmodel.vbs
REM for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\inputmodel.vbs`) do set "model_name=%%f"
REM if "%model_name%" == "" (
REM    :: ���͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
REM    exit /b
REM )
ECHO �����f���t�@�C����z�z���܂� ----------------------��
xcopy "%model_file_path%" \\%HOST_1%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_2%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_3%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_4%\%rapid_model_path% /s/e/i
xcopy "%model_file_path%" \\%HOST_5%\%rapid_model_path% /s/e/i

ECHO ��AI���f����o�^���܂�       ----------------------��
echo WMIC /NODE:192.168.164.128 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_1%" -m {%model_name%,\\%HOST_1%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.129 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_2%" -m {%model_name%,\\%HOST_2%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.130 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_3%" -m {%model_name%,\\%HOST_3%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.131 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_4%" -m {%model_name%,\\%HOST_4%\%rapid_model_path%\%model_file%}"
echo WMIC /NODE:192.168.164.132 /user:sas_admin /password:p@ssw0rdnec12345 process call create "rapid /S IMA/manage_data.rpd model-import -g "GUI" -path "%DB_5%" -m {%model_name%,\\%HOST_5%\%rapid_model_path%\%model_file%}"

REM echo Input = MsgBox("RAPID���f���C���|�[�g���������܂����B" , vbOK + vbInformation, "���") > %TEMP%\msgbox.vbs
REM echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
REM for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
REM if not "!confirm!" == "1" (
REM    :: OK���I������Ȃ��������͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
REM    exit /b
REM )