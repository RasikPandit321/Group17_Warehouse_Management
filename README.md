Smart Warehouse Management System 

A fully integrated simulation of a smart warehouse conveyor system, featuring a C# Control Backend, a Node.js Real-time Bridge, and a React/TypeScript HMI Dashboard.

This system simulates package sorting, environmental control, energy monitoring, and emergency handling protocols.


Features

1. 1. HMI Dashboard (Frontend)
Live Controls: Start/Stop the conveyor and adjust motor speed (RPM) via a slider.

Real-time Visualization:

Speed & Temperature Charts: Live trend analysis using recharts.

Live Lane Tracking: Visual representation of packages being sorted into 3 colored lanes (Express, Standard, Heavy).

Environment Monitoring: Displays temperature, fan status (ON/OFF), and a real-time Energy Efficiency Score.

Alarm System: Active alarm list with severity levels (High/Medium/Low) and a modal popup to acknowledge and clear faults.

Simulation Controls: Buttons to trigger "Jam", "E-Stop", and "Generate Report" directly from the UI.

2. Control System (C# Backend)
Hardware Simulation: Simulates motor states, temperature sensors (sine wave pattern), and barcode scanners.

Routing Logic: Automatically sorts packages based on weight:

🟢 Lane 1 (Express): < 5kg

🔵 Lane 2 (Standard): 5kg – 20kg

🟠 Lane 3 (Heavy): > 20kg

Safety Protocols: Automatic motor shutdown on "Jam" or "E-Stop" triggers. Auto-restart sequence upon clearance.

Energy Reporting: Calculates an energy score based on fan usage vs. temperature and exports data to CSV.

3. Communication Bridge (Node.js)
Uses Socket.IO to act as a bidirectional relay between the C# backend and the React frontend.

Handles event broadcasting for seamless state synchronization.


Technology Stack

Frontend: React, TypeScript, Vite, CSS (Responsive Grid), Recharts.

Backend: .NET 8 (C# Console Application).

Middleware: Node.js, Express, Socket.IO.

Testing: MSTest (Unit & Integration Tests).


Installation & Setup
You will need 3 separate terminals to run the full system.


Prerequisites

Node.js (v16+)
.NET SDK (v8.0+)

Step 1: Start the Socket Bridge (Terminal 1)
This allows the C# app and Website to talk to each other.

cd hmi-dashboard
node socket-server.cjs
Expected Output: 🚀 Socket.IO Bridge running on http://localhost:3001

Step 2: Start the HMI Dashboard (Terminal 2)
This launches the web interface.

cd hmi-dashboard
npm install  # (Only needed the first time)
npm run dev
Open the link provided (usually http://localhost:5173) in your browser.

Step 3: Start the Control System (Terminal 3)
This runs the simulation logic.

cd WareHouse_Management
dotnet run
Expected Output: ✅ Connected to HMI Dashboard!


How to Use
Dashboard Controls
Start/Stop: Click the button to toggle the motor.

Speed Slider: Drag to change conveyor RPM (updates live on the chart).

Simulate Jam/E-Stop: Click these buttons to trigger faults. The lanes will flash red ("BLOCKED"), and an alarm will appear.

Acknowledge Alarm: Click on a red/orange alarm in the list to open details, then click "Acknowledge" to clear it.

Generate Report: Click to save the current energy data to a CSV file in the backend folder.


Keyboard Shortcuts (C# Console)
You can also control the system directly from the C# terminal window:

S - Start Conveyor

X - Stop Conveyor

J - Toggle Jam Simulation

E - Toggle E-Stop Simulation

Q - Quit Application


Testing
The project includes a robust test suite covering Unit and Integration scenarios.

To run the tests:

cd Warehouse_Management_Test
dotnet test


Test Modules:

AlarmAndEStopUnitTests.cs: Verifies logging, file creation, and alarm triggering.

ConveyorAndMotorUnitTests.cs: Tests speed limits, start/stop logic, and safety locks.

EnvironmentUnitTests.cs: Validates fan hysteresis, sensor ranges, and energy score math.

RoutingAndSortingUnitTests.cs: Checks weight thresholds and lane assignments.

IntegrationTests.cs: End-to-end tests (e.g., "Does E-Stop actually stop the motor?").


Project Structure

├── hmi-dashboard/           # Frontend (React + Vite)
│   ├── src/
│   │   ├── App.tsx          # Main Dashboard UI Logic
│   │   ├── App.css          # Styling & Animations
│   │   └── socket.ts        # Socket Client Config
│   └── socket-server.cjs    # Middleware Bridge
│
├── WareHouse_Management/    # Backend (C#)
│   ├── Program.cs           # Main Simulation Loop
│   ├── Conveyor and Motor/  # Hardware Logic
│   ├── Environment/         # Sensors & Energy Reporting
│   ├── Routing and Sorting/ # Scanner & Lane Logic
│   └── Alarm and Estop/     # Safety & Logging
│
└── Warehouse_Management_Test/ # MSTest Project


Troubleshooting

Status says "Disconnected": Make sure node socket-server.cjs is running.

Lanes are empty: Ensure the Conveyor is Running. The scanner halts when the motor stops.

Visual Jitter: The CSS includes tabular-nums and fixed widths to prevent text resizing updates from shifting the layout.