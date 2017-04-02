@echo off
set "current=%cd%"

set "username=user"
set "password=pass"

call "%current%\environment.bat"

call "%current%\rabbitmq_server-3.6.6\sbin\rabbitmqctl.bat" add_user %username% %password%
call "%current%\rabbitmq_server-3.6.6\sbin\rabbitmqctl.bat" set_permissions -p / %username% ".*" ".*" ".*"
call "%current%\rabbitmq_server-3.6.6\sbin\rabbitmqctl.bat" set_user_tags %username% administrator

pause