@echo off
setlocal
setlocal enabledelayedexpansion

:: 実行コマンド定義
set program_path=D:\CI\programs
set program_name=process_kill.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"

echo Input = MsgBox("全機能停止を開始します。"^& vbCr ^& "中止する場合はキャンセルを押して下さい。 ", vbOKCancel + vbInformation, "情報") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OKが選択されなかった入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)

%exec_command%

if !ERRORLEVEL! == 0 (
    echo MsgBox "全機能停止が完了しました。", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "エラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。"^& vbCr ^& "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)