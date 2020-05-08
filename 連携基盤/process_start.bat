@echo off
setlocal
setlocal enabledelayedexpansion

:: 実行コマンド定義
set program_path=D:\CI\programs
set program_name=process_start.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"



%exec_command%

