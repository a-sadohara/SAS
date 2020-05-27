@echo off 
setlocal enabledelayedexpansion 

set python_path=D:\CI\python\python.exe
set program_path=D:\CI\programs\
set program_name=calcPrecision.pyc

:: Does powershell.exe exist within %PATH%? 
set label_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='ラベルファイル選択';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
set result_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='RAPID解析結果ファイル選択';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 

:: capture choice to a variable 
echo MsgBox "ラベルファイル選択画面に遷移します。" ^& vbCr ^& "ラベルファイルを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choselabelmsgbox.vbs & %TEMP%\choselabelmsgbox.vbs
for /f "delims=" %%L in ('%label_chooser%') do set "label_filename=%%L" 
if not %label_filename% == "" (
  echo You chose %label_filename% 
) else (
  exit /B false
)

echo MsgBox "RAPID解析結果ファイル選択画面に遷移します。" ^& vbCr ^& "RAPID解析結果ファイルを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseresultmsgbox.vbs & %TEMP%\choseresultmsgbox.vbs
for /f "delims=" %%R in ('%result_chooser%') do set "result_filename=%%R" 
if not %result_filename% == "" (
  echo You chose %result_filename% 
) else (
  exit /B false
)

echo MsgBox "精度評価結果ファイル出力先選択画面に遷移します。" ^& vbCr ^& "精度評価結果ファイル出力先を選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'精度評価結果ファイル出力先を選択してください。',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder=%%I"
if not "%folder%" == "" (
  echo You chose !folder!
) else (
  exit /B false
)

%python_path% %program_path%%program_name% -o "%folder%" -labelfile %label_filename%  -resultfile %result_filename%  2>NUL 
if %ERRORLEVEL% == 0 (
  echo MsgBox "精度評価が完了しました。", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
  echo MsgBox "精度評価実行時にエラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。" ^& vbCr ^& "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)

:: Clean up the mess 
del "%temp%\chooser.exe" 2>NUL 
goto :EOF 