@echo off
setlocal
setlocal enabledelayedexpansion

:: ���s�R�}���h��`
set program_path=D:\CI\programs
set program_name=process_kill.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"

echo Input = MsgBox("�S�@�\��~���J�n���܂��B"^& vbCr ^& "���~����ꍇ�̓L�����Z���������ĉ������B ", vbOKCancel + vbInformation, "���") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OK���I������Ȃ��������͂���Ȃ������i�����́A�������́A�L�����Z���j�ꍇ�͏I������
    exit /b
)

%exec_command%

if !ERRORLEVEL! == 0 (
    echo MsgBox "�S�@�\��~���������܂����B", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "�G���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B"^& vbCr ^& "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)