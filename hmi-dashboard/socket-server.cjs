const { Server } = require("socket.io");
const http = require("http");

const server = http.createServer();
// Configure Socket.IO with CORS enabled for Vite
const io = new Server(server, {
    cors: {
        origin: "*", // Allow all origins for dev
        methods: ["GET", "POST"]
    }
});

io.on("connection", (socket) => {
    console.log("✅ Client connected:", socket.id);

    // --- C# -> REACT ---
    // Forward data from Backend to Frontend
    socket.on("system:update", (data) => io.emit("system:update", data));
    socket.on("barcode:scanned", (data) => io.emit("barcode:scanned", data));
    socket.on("diverter:activated", (data) => io.emit("diverter:activated", data));
    socket.on("alarm:new", (data) => io.emit("alarm:new", data));
    socket.on("report:generated", (data) => io.emit("report:generated", data));

    // --- REACT -> C# ---
    // Forward commands from Frontend to Backend
    socket.on("conveyor:start", () => io.emit("conveyor:start"));
    socket.on("conveyor:stop", () => io.emit("conveyor:stop"));
    socket.on("conveyor:speed", (val) => io.emit("conveyor:speed", val));

    // Simulation Commands
    socket.on("sim:jam", () => io.emit("sim:jam"));
    socket.on("sim:estop", () => io.emit("sim:estop"));
    socket.on("request:report", () => io.emit("request:report"));
});

server.listen(3001, () => {
    console.log("🚀 Socket.IO Bridge running on http://localhost:3001");
});