Get-ChildItem .\ -include bin,obj -Recurse | Where {$_.FullName -notlike "*RabbitMQ*" -and $_.FullName -notlike "*GoogleChromePortable*"} | foreach ($_) { remove-item $_.fullname -Force -Recurse }