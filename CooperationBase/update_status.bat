@echo off
setlocal
setlocal enabledelayedexpansion

:: ���s�R�}���h��`
set program_path=D:\CI\programs
set program_name=update_status.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"
:: �������i�f�t�H���g�l�j
set default_inspection_date=%date%
echo %exec_command%

:: ���ԓ���
echo Input = InputBox("���Ԃ���͂��ĉ������B") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "fabric_name=%%f"
if "!fabric_name!" == "" (
    :: ���͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)

:: �����ԍ�����
echo Input = InputBox("�����ԍ�����͂��ĉ������B") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "inspection_num=%%f"
if "!inspection_num!" == "" (
    :: ���͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)

:: ����������
echo Input = InputBox("�������i�N�����܂Łj����͂��ĉ������B"^& vbCr ^& "�i[/][-]��؂�A0���ߗL���A�ǂ���ł��j"^& vbCr ^& vbCr ^& "���͗�:[2020/03/01], [2020-3-1], [2020/3/1]","","%default_inspection_date%") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "inspection_date=%%f"
if "!inspection_date!" == "" (
    :: �L�����Z���̏ꍇ�͏I������
    exit /b
)

:: ���@����
echo Input = InputBox("���@�i�����j����͂��ĉ������B"^& vbCr ^& "���͗�:[1]�i1���@�̏ꍇ�j") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "unit_num=%%f"
if "!unit_num!" == "" (
    :: ���͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)

:: �m�F���
echo Input = MsgBox("�ȉ��̏����ōX�V���܂��B"^& vbCr ^& "�I������ۂ̓L�����Z���{�^���������Ă��������B "^& vbCr ^& vbCr ^& "����:[!fabric_name!], �����ԍ�:[!inspection_num!], ������:[!inspection_date!], ���@:[!unit_num!]" , vbOKCancel + vbInformation, "���") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OK���I������Ȃ��������͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)

:: �X�e�[�^�X�X�V�v���O�������s
set current_dir=%~dp0
cd /d %program_path%
%exec_command% "!fabric_name!" "!inspection_num!" "!inspection_date!" "!unit_num!"

if !ERRORLEVEL! == 0 (
    echo MsgBox "�X�e�[�^�X�X�V���������܂����B", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "�X�e�[�^�X�X�V���s���ɃG���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B"^& vbCr ^& "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)
cd /d %current_dir%

