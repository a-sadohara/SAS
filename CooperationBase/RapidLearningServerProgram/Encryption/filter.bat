@echo off 
setlocal enabledelayedexpansion 

:: Does powershell.exe exist within %PATH%? 
set result_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='RAPID��͌��ʃt�@�C���I��';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
set result_output=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.SaveFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='�[���茋�ʃt�@�C���ۑ�';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 


:: capture choice to a variable 
echo MsgBox "RAPID��͌��ʃt�@�C���I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "RAPID��͌��ʃt�@�C����I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseresultmsgbox.vbs & %TEMP%\choseresultmsgbox.vbs
for /f "delims=" %%R in ('%result_chooser%') do set "result_filename=%%R" 
if not %result_filename% == "" (
  echo You chose %result_filename% 
) else (
  exit /B false
)

echo MsgBox "��c�����}�X�N���ʃt�@�C���o�͐�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "��c�����}�X�N���ʃt�@�C���o�͐�E�t�@�C�������w�肵�Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\chosefoldermsgbox.vbs & %TEMP%\chosefoldermsgbox.vbs
for /f "delims=" %%R in ('%result_output%') do set "result_output=%%R" 
if not %result_output% == "" (
  echo You chose %result_output% 
) else (
  exit /B false
)

D:\CI\python\python.exe D:\CI\programs\checkFabric.pyc -resultfile %result_filename% -o "%result_output%"  2>NUL 
if %ERRORLEVEL% == 0 (
  echo MsgBox "��c�����}�X�N���肪�������܂����B", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
  echo MsgBox "��c�����}�X�N������s���ɃG���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B" ^& vbCr ^& "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)

:: Clean up the mess 
del "%temp%\chooser.exe" 2>NUL 
goto :EOF 