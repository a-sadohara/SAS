@echo off
setlocal
setlocal enabledelayedexpansion

set python_path=D:\CI\python\python.exe
set program_path=D:\CI\programs\
set program_name=adjustBrightness.pyc


echo MsgBox "学習用画像格納フォルダ選択画面に遷移します。" ^& vbCr ^& "学習用画像格納フォルダを選択してください。", vbOKOnly + vbInformation, "情報" > %TEMP%\choseimagefoldermsgbox.vbs & %TEMP%\choseimagefoldermsgbox.vbs
set "psCommand="(new-object -COM 'Shell.Application').BrowseForFolder(0,'学習用画像格納フォルダを選択してください。',0,0).self.path""

for /f "usebackq delims=" %%I in (`powershell %psCommand%`) do set "folder=%%I"
if not "%folder%" == "" (
  echo You chose !folder!
) else (
  exit /B false
)

echo Input = InputBox("補正値を入力してください。") > %TEMP%\getparameter.vbs
echo Wscript.Echo(Input) >> %TEMP%\getparameter.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\getparameter.vbs`) do set "brightness=%%f"
if not %brightness% == "" (
    echo You chose !brightness!
    %python_path% %program_path%%program_name% -i !folder! -o !folder!\明度補正済み -v !brightness! 2>NUL 
    if %ERRORLEVEL% == 0 (
        echo MsgBox "明度補正が完了しました。", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
    ) else (
        echo MsgBox "明度補正実行時にエラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。" & vbCr & "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
    )
) else (
    exit /B false
)



