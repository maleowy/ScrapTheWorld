@echo off
set "current=%cd%"
set "escaped=%current:\=\\%"

echo [erlang] > %current%\erl8.2\bin\erl.ini
echo Bindir=%escaped%\\erl8.2\\erts-8.2\\bin >> %current%\erl8.2\bin\erl.ini
echo Progname=erl >> %current%\erl8.2\bin\erl.ini
echo Rootdir=%escaped%\\erl8.2 >> %current%\erl8.2\bin\erl.ini

set ERLANG_HOME="%current%\erl8.2\"
set ERL_EPMD_ADDRESS=127.0.0.1
:: %current%\erl8.2\bin\erl -sname foo

set RABBITMQ_BASE="%current%\"
set RABBITMQ_CONFIG_FILE="%current%\rabbitmq.config"
set RABBITMQ_NODENAME=rabbit@localhost
set RABBITMQ_NODE_IP_ADDRESS=127.0.0.1
::set NODE_IP_ADDRESS=127.0.0.1