// socket-server.js
const { Server } = require("socket.io");
const http = require("http");

const server = http.createServer();
const io = new Server(server, {
    cors: { origin: "*" }
});

io.on("connection", (socket) => {
    console.log("🔌 Client connected");

    // Simulate backend events every few seconds
    setInterval(() => {
        socket.emit("alarm:new", { message: "🔥 Overheat detected" });
        socket.emit("conveyor:start");
        socket.emit("env:temperature", { value: (20 + Math.random() * 10).toFixed(1) });
        socket.emit("barcode:scanned", { code: "PKG123A" });
        socket.emit("diverter:activated", { zone: "Zone A" });
    }, 5000);
});

server.listen(3001, () => {
    console.log("🚀 Socket server running on port 3001");
});