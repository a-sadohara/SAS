@echo off
setlocal
setlocal enabledelayedexpansion

:: 実行コマンド定義
set program_path=D:\CI\programs
set program_name=create_master_data.py
set exec_command=d:\CI\python\python.exe "%program_path%\%program_name%"

echo MsgBox "iniファイルフォルダ選択画面に遷移します。" ^& vbCr ^& "iniファイルが格納されているフォルダを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'iniファイルが格納されているフォルダを選択してください。',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "ini_file_path=%%I"
if not "%ini_file_path%" == "" (
  echo You chose !ini_file_path!
) else (
  exit /B false
)

:: Does powershell.exe exist within %PATH%? 
set label_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='インプットCSV選択';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
:: capture choice to a variable 
echo MsgBox "インプットCSVファイル選択画面に遷移します。" ^& vbCr ^& "インプットCSVファイルを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choselabelmsgbox.vbs & %TEMP%\choselabelmsgbox.vbs
for /f "delims=" %%L in ('%label_chooser%') do set "csv_file_path=%%L" 
if not %csv_file_path% == "" (
  echo You chose %csv_file_path% 
) else (
  exit /B false
)

echo MsgBox "ファイル出力フォルダ選択画面に遷移します。" ^& vbCr ^& "閾値情報ファイルを出力するフォルダを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\chosefilefoldermsgbox.vbs & %TEMP%\chosefilefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'閾値情報ファイルを出力するフォルダを選択してください。',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "output_file_path=%%I"
if not "%output_file_path%" == "" (
  echo You chose !output_file_path!
) else (
  exit /B false
)

:: プログラム実行
set current_dir=%~dp0
cd /d %program_path%
%exec_command% !ini_file_path! !csv_file_path! !output_file_path!

if !ERRORLEVEL! == 0 (
    echo MsgBox "閾値情報作成が完了しました", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "閾値情報作成中にエラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。"^& vbCr ^& "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)
cd /d %current_dir%