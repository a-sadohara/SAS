@echo off
setlocal
setlocal enabledelayedexpansion

:: ���s�R�}���h��`
set program_path=D:\CI\programs
set program_name=create_master_data.py
set exec_command=d:\CI\python\python.exe "%program_path%\%program_name%"

echo MsgBox "ini�t�@�C���t�H���_�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "ini�t�@�C�����i�[����Ă���t�H���_��I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'ini�t�@�C�����i�[����Ă���t�H���_��I�����Ă��������B',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "ini_file_path=%%I"
if not "%ini_file_path%" == "" (
  echo You chose !ini_file_path!
) else (
  exit /B false
)

:: Does powershell.exe exist within %PATH%? 
set label_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='�C���v�b�gCSV�I��';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
:: capture choice to a variable 
echo MsgBox "�C���v�b�gCSV�t�@�C���I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "�C���v�b�gCSV�t�@�C����I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\choselabelmsgbox.vbs & %TEMP%\choselabelmsgbox.vbs
for /f "delims=" %%L in ('%label_chooser%') do set "csv_file_path=%%L" 
if not %csv_file_path% == "" (
  echo You chose %csv_file_path% 
) else (
  exit /B false
)

echo MsgBox "�t�@�C���o�̓t�H���_�I����ʂɑJ�ڂ��܂��B" ^& vbCr ^& "臒l���t�@�C�����o�͂���t�H���_��I�����Ă��������B", vbOKOnly + vbInformation, "���" > %TEMP%\chosefilefoldermsgbox.vbs & %TEMP%\chosefilefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'臒l���t�@�C�����o�͂���t�H���_��I�����Ă��������B',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "output_file_path=%%I"
if not "%output_file_path%" == "" (
  echo You chose !output_file_path!
) else (
  exit /B false
)

:: �v���O�������s
set current_dir=%~dp0
cd /d %program_path%
%exec_command% !ini_file_path! !csv_file_path! !output_file_path!

if !ERRORLEVEL! == 0 (
    echo MsgBox "臒l���쐬���������܂���", vbOKOnly + vbInformation, "���" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "臒l���쐬���ɃG���[���������܂����B" ^& vbCr ^& "�G���[���O���m�F���Ă��������B"^& vbCr ^& "" , vbOKOnly + vbCritical, "�G���[" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)
cd /d %current_dir%