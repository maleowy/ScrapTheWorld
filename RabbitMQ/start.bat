@echo off
set "current=%cd%"

if exist "%current%\rabbitmq_server-3.6.6\" goto Run

:Extract

echo Extract RabbitMQ.zip file and try again.
pause
goto Exit

:Run

call "%current%\environment.bat"

TITLE RabbitMQ - http://127.0.0.1:15672/
call "%current%\rabbitmq_server-3.6.6\sbin\rabbitmq-server.bat"

:Exit