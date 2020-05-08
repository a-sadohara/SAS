@echo off
setlocal
setlocal enabledelayedexpansion

:: 実行コマンド定義
set program_path=D:\CI\programs
set program_name=update_status.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"
:: 検査日（デフォルト値）
set default_inspection_date=%date%
echo %exec_command%

:: 反番入力
echo Input = InputBox("反番を入力して下さい。") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "fabric_name=%%f"
if "!fabric_name!" == "" (
    :: 入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)

:: 検査番号入力
echo Input = InputBox("検査番号を入力して下さい。") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "inspection_num=%%f"
if "!inspection_num!" == "" (
    :: 入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)

:: 検査日入力
echo Input = InputBox("検査日（年月日まで）を入力して下さい。"^& vbCr ^& "（[/][-]区切り、0埋め有無、どちらでも可）"^& vbCr ^& vbCr ^& "入力例:[2020/03/01], [2020-3-1], [2020/3/1]","","%default_inspection_date%") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "inspection_date=%%f"
if "!inspection_date!" == "" (
    :: キャンセルの場合は終了する
    exit /b
)

:: 号機入力
echo Input = InputBox("号機（数字）を入力して下さい。"^& vbCr ^& "入力例:[1]（1号機の場合）") > %TEMP%\update_status.vbs
echo Wscript.Echo(Input) >> %TEMP%\update_status.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\update_status.vbs`) do set "unit_num=%%f"
if "!unit_num!" == "" (
    :: 入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)

:: 確認画面
echo Input = MsgBox("以下の条件で更新します。"^& vbCr ^& "終了する際はキャンセルボタンを押してください。 "^& vbCr ^& vbCr ^& "反番:[!fabric_name!], 検査番号:[!inspection_num!], 検査日:[!inspection_date!], 号機:[!unit_num!]" , vbOKCancel + vbInformation, "情報") > %TEMP%\msgbox.vbs
echo Wscript.Echo(Input) >> %TEMP%\msgbox.vbs
for /f "usebackq delims=" %%f in (`cscript /NOlogo %TEMP%\msgbox.vbs`) do set "confirm=%%f"
if not "!confirm!" == "1" (
    :: OKが選択されなかった入力されなかった（未入力、もしくは、キャンセル）場合は終了する
    exit /b
)

:: ステータス更新プログラム実行
set current_dir=%~dp0
cd /d %program_path%
%exec_command% "!fabric_name!" "!inspection_num!" "!inspection_date!" "!unit_num!"

if !ERRORLEVEL! == 0 (
    echo MsgBox "ステータス更新が完了しました。", vbOKOnly + vbInformation, "情報" > %TEMP%\msgbox.vbs & %TEMP%\msgbox.vbs
) else (
    echo MsgBox "ステータス更新実行時にエラーが発生しました。" ^& vbCr ^& "エラーログを確認してください。"^& vbCr ^& "" , vbOKOnly + vbCritical, "エラー" > %TEMP%\error_msgbox.vbs & %TEMP%\error_msgbox.vbs
)
cd /d %current_dir%

