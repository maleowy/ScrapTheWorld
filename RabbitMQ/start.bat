@echo off
set "current=%cd%"

if exist "%current%\rabbitmq_server-3.6.6\" goto Run

:Extract

Powershell.exe -executionpolicy remotesigned -File extract.ps1

:Run

call "%current%\environment.bat"

TITLE RabbitMQ - http://127.0.0.1:15672/
call "%current%\rabbitmq_server-3.6.6\sbin\rabbitmq-server.bat"

:Exit