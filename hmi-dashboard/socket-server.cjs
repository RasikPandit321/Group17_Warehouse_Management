const { Server } = require("socket.io");
const http = require("http");

const server = http.createServer();
const io = new Server(server, {
    cors: {
        origin: "*", // Allow connections from React (port 5173)
        methods: ["GET", "POST"]
    }
});

io.on("connection", (socket) => {
    console.log("✅ Client connected:", socket.id);

    // --- SECTION 1: C# -> REACT (Data Flow) ---

    // Relay System Status (Temp, Fan, Energy, Conveyor State)
    socket.on("system:update", (data) => {
        io.emit("system:update", data);
    });

    // Relay Barcode Scans
    socket.on("barcode:scanned", (data) => {
        io.emit("barcode:scanned", data);
    });

    // Relay Diverter/Routing Info
    socket.on("diverter:activated", (data) => {
        io.emit("diverter:activated", data);
    });

    // Relay Alarms
    socket.on("alarm:new", (data) => {
        io.emit("alarm:new", data);
    });

    // --- SECTION 2: REACT -> C# (Control Flow) ---

    // Relay Start Command
    socket.on("conveyor:start", () => {
        console.log("▶️ Conveyor Start Requested");
        io.emit("conveyor:start");
    });

    // Relay Stop Command
    socket.on("conveyor:stop", () => {
        console.log("⏹ Conveyor Stop Requested");
        io.emit("conveyor:stop");
    });

    // Relay Speed Command
    socket.on("conveyor:speed", (val) => {
        console.log("⚙️ Speed set to", val);
        io.emit("conveyor:speed", val);
    });
});

server.listen(3001, () => {
    console.log("🚀 Socket.IO Bridge running on http://localhost:3001");
});