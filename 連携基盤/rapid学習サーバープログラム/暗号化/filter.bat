@echo off 
setlocal enabledelayedexpansion 

:: Does powershell.exe exist within %PATH%? 
set result_chooser=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.OpenFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='RAPID解析結果ファイル選択';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 
set result_output=powershell "Add-Type -AssemblyName System.windows.forms|Out-Null;$f=New-Object System.Windows.Forms.SaveFileDialog;$f.InitialDirectory='%cd%';$f.Filter='All Files (*.*)|*.*';$f.Title='端判定結果ファイル保存';$f.showHelp=$true;$f.ShowDialog()|Out-Null;$f.FileName" 


:: capture choice to a variable 
echo MsgBox "RAPID解析結果ファイル選択画面に遷移します。" ^& vbCr ^& "RAPID解析結果ファイルを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseresultmsgbox.vbs & %TEMP%\choseresultmsgbox.vbs
for /f "delims=" %%R in ('%result_chooser%') do set "result_filename=%%R" 
if not %result_filename% == "" (
  echo You chose %result_filename% 
) else (
  exit /B false
)

echo MsgBox "非膨張部マスク結果ファイル出力先選択画面に遷移します。" ^& vbCr ^& "非膨張部マスク結果ファイル出力先・ファイル名を指定してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\chosefoldermsgbox.vbs & %TEMP%\chosefoldermsgbox.vbs
for /f "delims=" %%R in ('%result_output%') do set "result_output=%%R" 
if not %result_output% == "" (
  echo You chose %result_output% 
) else (
  exit /B false
)

D:\CI\python\python.exe D:\CI\programs\checkFabric.pyc -resultfile %result_filename% -o "%result_output%"  2>NUL 
if %ERRORLEVEL% == 0 (
  echo MsgBox "非膨張部マスク判定が完了しました。", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
  echo MsgBox "非膨張部マスク判定実行時にエラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。" ^& vbCr ^& "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)

:: Clean up the mess 
del "%temp%\chooser.exe" 2>NUL 
goto :EOF 