@echo off
setlocal
setlocal enabledelayedexpansion

set python_path=D:\CI\python\python.exe
set program_path=D:\CI\programs\
set program_name=adjustBrightness.pyc


echo MsgBox "�w�K�p�摜�i�[�t�H���_�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "�w�K�p�摜�i�[�t�H���_��I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'�w�K�p�摜�i�[�t�H���_��I�����Ă��������B',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder=%%I"
if not "%folder%" == "" (
  echo You chose !folder!
) else (
  exit /B false
)

echo Input = InputBox("�␳�l����͂��Ă��������B") > %TEMP%\getparameter.vbs
echo Wscript.Echo(Input) >> %TEMP%\getparameter.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\getparameter.vbs`) do set "brightness=%%f"
if not %brightness% == "" (
    echo You chose !brightness!
    %python_path% %program_path%%program_name% -i !folder! -o !folder!\���x�␳�ς� -v !brightness! 2>NUL 
    if %ERRORLEVEL% == 0 (
        echo MsgBox "���x�␳���������܂����B", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
    ) else (
        echo MsgBox "���x�␳���s���ɃG���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B" & vbCr & "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
    )
) else (
    exit /B false
)



