@echo off
setlocal
setlocal enabledelayedexpansion

SCHTASKS /RUN /TN "function301 startup"
SCHTASKS /RUN /TN "function302 startup"
SCHTASKS /RUN /TN "function304 startup"
SCHTASKS /RUN /TN "function306 startup"
SCHTASKS /RUN /TN "function307 startup"
SCHTASKS /RUN /TN "function310 startup"
SCHTASKS /RUN /TN "function311 startup"

set input_path=D:\CI\temp\if_files\CI
call del /Q %input_path%\Alram\*