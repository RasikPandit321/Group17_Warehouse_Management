const { Server } = require("socket.io");
const http = require("http");

const server = http.createServer();
const io = new Server(server, {
    cors: {
        origin: "*", 
        methods: ["GET", "POST"]
    }
});

io.on("connection", (socket) => {
    console.log("✅ Client connected:", socket.id);

    // --- C# -> REACT ---
    socket.on("system:update", (data) => io.emit("system:update", data));
    socket.on("barcode:scanned", (data) => io.emit("barcode:scanned", data));
    socket.on("diverter:activated", (data) => io.emit("diverter:activated", data));
    socket.on("alarm:new", (data) => io.emit("alarm:new", data));
    
    // Relay Report Generated Confirmation
    socket.on("report:generated", (data) => io.emit("report:generated", data));

    // --- REACT -> C# (Controls & Simulation) ---
    socket.on("conveyor:start", () => io.emit("conveyor:start"));
    socket.on("conveyor:stop", () => io.emit("conveyor:stop"));
    socket.on("conveyor:speed", (val) => io.emit("conveyor:speed", val));

    // NEW: Simulation Commands
    socket.on("sim:jam", () => io.emit("sim:jam"));
    socket.on("sim:estop", () => io.emit("sim:estop"));
    socket.on("request:report", () => io.emit("request:report"));
});

server.listen(3001, () => {
    console.log("🚀 Socket.IO Bridge running on http://localhost:3001");
});