// server.js
const WebSocket = require('ws');
const fs        = require('fs');
const path      = require('path');

//
// ─── CONFIG ────────────────────────────────────────────────────────────────────
//
const PORT         = 8080;
const LOG_DIR      = path.resolve(__dirname, 'logs');
const MAX_LOG_SIZE = 5 * 1024 * 1024; // 5 MB per file
//

// Ensure log directory exists
if (!fs.existsSync(LOG_DIR)) {
  fs.mkdirSync(LOG_DIR, { recursive: true });
}

// State for log rotation
let logStream;
let bytesWritten = 0;

// Create a new log file (timestamped) and reset counters
function rotateLogFile() {
  if (logStream) logStream.end();
  const ts       = new Date().toISOString().replace(/[:.]/g, '-');
  const filename = `messages-${ts}.log`;
  const filepath = path.join(LOG_DIR, filename);
  logStream      = fs.createWriteStream(filepath, { flags: 'a' });
  bytesWritten   = 0;
  console.log(`[Log] Writing to ${filepath}`);
}

// Write a single log line, then rotate if needed
function logMessage(rawMsg) {
  const iso     = new Date().toISOString();
  const entry   = `${iso} ${rawMsg}\n`;
  const buf     = Buffer.from(entry, 'utf8');

  logStream.write(buf);
  bytesWritten += buf.length;

  if (bytesWritten >= MAX_LOG_SIZE) {
    rotateLogFile();
  }
}

// Initialize first log file
rotateLogFile();


//
// ─── WEBSOCKET SETUP ───────────────────────────────────────────────────────────
//
const wss = new WebSocket.Server({ port: PORT }, () => {
  console.log(`WebSocket server listening on ws://0.0.0.0:${PORT}`);
});

wss.on('connection', ws => {
  console.log('[WS] Client connected');

  ws.on('message', message => {
    // 1) Log every message
    logMessage(message);

    // 2) Broadcast to all other clients
    wss.clients.forEach(client => {
      if (client !== ws && client.readyState === WebSocket.OPEN) {
        client.send(message);
      }
    });
  });

  ws.on('close', () => {
    console.log('[WS] Client disconnected');
  });
});
