if(![System.IO.File]::Exists('rabbitmq_server-3.6.6')){
    Expand-Archive RabbitMQ.zip -DestinationPath .
}