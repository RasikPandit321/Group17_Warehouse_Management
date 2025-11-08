const { Server } = require("socket.io");
const http = require("http");

const server = http.createServer();
const io = new Server(server, {
    cors: {
        origin: "http://localhost:3000", // your React app
        methods: ["GET", "POST"]
    }
});

io.on("connection", (socket) => {
    console.log("✅ Client connected");

    // Emit mock data for testing
    socket.emit("conveyor:update", { running: true, speed: 42 });
    socket.emit("env:temperature", { value: "23.5" });
    socket.emit("barcode:scanned", { code: "ABC123" });
    socket.emit("diverter:activated", { zone: "Zone 5" });
    socket.emit("alarm:new", { message: "Overload detected" });

    socket.on("conveyor:start", () => console.log("▶️ Conveyor started"));
    socket.on("conveyor:stop", () => console.log("⏹ Conveyor stopped"));
    socket.on("conveyor:speed", (val) => console.log("⚙️ Speed set to", val));
});

server.listen(3001, () => {
    console.log("🚀 Socket.IO server running on http://localhost:3001");
});