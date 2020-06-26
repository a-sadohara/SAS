@echo off
setlocal
setlocal enabledelayedexpansion

:: 実行コマンド定義
set program_path=D:\CI\programs
set program_name=process_start.py
set exec_command=D:\CI\python\python.exe "%program_path%\%program_name%"



%exec_command%

set input_path=\\192.168.164.10\if_files\CI

del /Q %input_path%\Alram