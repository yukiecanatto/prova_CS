const WebSocket = require('ws');

const ws = new WebSocket('ws://localhost:5000/');

ws.on('open', function open() {
    console.log('Connected to server');
    ws.send('SUBSCRIBE: CPU');
});

ws.on('message', function incoming(data) {
    console.log(`Received: ${data}`);
});

ws.on('close', function close() {
    console.log('Disconnected from server');
});

process.on('SIGINT', function() {
    console.log('Client shutting down...');
    ws.close();
    process.exit();
});
