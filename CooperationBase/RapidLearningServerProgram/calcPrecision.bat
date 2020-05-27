@echo off 
setlocal enabledelayedexpansion 

set python_path=D:\CI\python\python.exe
set program_path=D:\CI\programs\
set program_name=calcPrecision.pyc

:: Does powershell.exe exist within %PATH%? 
set label_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='���x���t�@�C���I��';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
set result_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='RAPID��͌��ʃt�@�C���I��';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 

:: capture choice to a variable 
echo MsgBox "���x���t�@�C���I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "���x���t�@�C����I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choselabelmsgbox.vbs & %TEMP%\choselabelmsgbox.vbs
for /f "delims=" %%L in ('%label_chooser%') do set "label_filename=%%L" 
if not %label_filename% == "" (
  echo You chose %label_filename% 
) else (
  exit /B false
)

echo MsgBox "RAPID��͌��ʃt�@�C���I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "RAPID��͌��ʃt�@�C����I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseresultmsgbox.vbs & %TEMP%\choseresultmsgbox.vbs
for /f "delims=" %%R in ('%result_chooser%') do set "result_filename=%%R" 
if not %result_filename% == "" (
  echo You chose %result_filename% 
) else (
  exit /B false
)

echo MsgBox "���x�]�����ʃt�@�C���o�͐�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "���x�]�����ʃt�@�C���o�͐��I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'���x�]�����ʃt�@�C���o�͐��I�����Ă��������B',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder=%%I"
if not "%folder%" == "" (
  echo You chose !folder!
) else (
  exit /B false
)

%python_path% %program_path%%program_name% -o "%folder%" -labelfile %label_filename%  -resultfile %result_filename%  2>NUL 
if %ERRORLEVEL% == 0 (
  echo MsgBox "���x�]�����������܂����B", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
  echo MsgBox "���x�]�����s���ɃG���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B" ^& vbCr ^& "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)

:: Clean up the mess 
del "%temp%\chooser.exe" 2>NUL 
goto :EOF 