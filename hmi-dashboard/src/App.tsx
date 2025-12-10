import { useEffect, useState } from 'react';
import {
    LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, ResponsiveContainer
} from 'recharts';
import { socket } from './socket';
import './App.css';

interface Alarm {
    id: string;
    message: string;
    severity: 'High' | 'Medium' | 'Low';
    timestamp: number;
    recommended_action?: string;
}

function App() {
    const [status, setStatus] = useState('Disconnected');
    const [conveyorRunning, setConveyorRunning] = useState(false);
    const [speed, setSpeed] = useState(50);
    const [temperature, setTemperature] = useState<number>(20);
    const [fanRunning, setFanRunning] = useState<boolean>(false);
    const [energyScore, setEnergyScore] = useState<number>(100);
    const [history, setHistory] = useState<{ time: string; speed: number; temp: number }[]>([]);
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

    // ... (Keep existing state & helpers: isJam, isEStop, Lanes, severity logic) ...
    const [isJam, setIsJam] = useState(false);
    const [isEStop, setIsEStop] = useState(false);
    const [lanes, setLanes] = useState({ Lane1: null as string | null, Lane2: null as string | null, Lane3: null as string | null });

    const determineSeverity = (msg: string): 'High' | 'Medium' | 'Low' => {
        if (msg.toLowerCase().includes('stop') || msg.toLowerCase().includes('jam')) return 'High';
        if (msg.toLowerCase().includes('heavy')) return 'Medium';
        return 'Low';
    };

    const getAction = (msg: string) => {
        if (msg.toLowerCase().includes('jam')) return 'Clear obstruction & restart.';
        if (msg.toLowerCase().includes('heavy')) return 'Verify Lane 3 Clearance.';
        return 'Monitor system.';
    };

    // --- NEW: Acknowledge Function ---
    const acknowledgeAlarm = () => {
        if (selectedAlarm) {
            // Remove the selected alarm from the list
            setAlarms((prev) => prev.filter(a => a.id !== selectedAlarm.id));
            // Close the modal
            setSelectedAlarm(null);
        }
    };

    useEffect(() => {
        socket.on('connect', () => setStatus('Connected'));
        socket.on('disconnect', () => setStatus('Disconnected'));

        socket.on('system:update', (data) => {
            setConveyorRunning(data.conveyor.running);
            setSpeed(data.conveyor.speed);
            setTemperature(parseFloat(data.env.temp));
            setFanRunning(data.env.fanOn);
            setEnergyScore(data.env.energyScore);
            if (data.flags) {
                setIsJam(data.flags.isJam);
                setIsEStop(data.flags.isEStop);
            }
            setHistory((prev) => [...prev, {
                time: new Date().toLocaleTimeString(),
                speed: data.conveyor.speed,
                temp: parseFloat(data.env.temp)
            }].slice(-20));
        });

        socket.on('diverter:activated', ({ zone, code }) => {
            setLanes(prev => ({ ...prev, [zone]: code }));
            setTimeout(() => {
                setLanes(prev => ({
                    ...prev,
                    [zone]: (prev[zone as keyof typeof prev] === code ? null : prev[zone as keyof typeof prev])
                }));
            }, 3000);
        });

        socket.on('alarm:new', ({ message }) => {
            const newAlarm: Alarm = {
                id: Date.now().toString(),
                message: message,
                severity: determineSeverity(message),
                timestamp: Date.now(),
                recommended_action: getAction(message)
            };
            setAlarms((prev) => [newAlarm, ...prev]);
        });

        socket.on('report:generated', ({ filename }) => {
            alert(`✅ Energy Report Generated Successfully!\nFile saved as: ${filename}`);
        });

        return () => {
            socket.off('connect');
            socket.off('disconnect');
            socket.off('system:update');
            socket.off('diverter:activated');
            socket.off('alarm:new');
            socket.off('report:generated');
        };
    }, []);

    const generateReport = () => socket.emit("request:report");
    const simulateJam = () => socket.emit("sim:jam");
    const simulateEStop = () => socket.emit("sim:estop");

    const toggleConveyor = () => {
        const action = conveyorRunning ? 'conveyor:stop' : 'conveyor:start';
        socket.emit(action);
    };

    const handleSpeed = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = Number(e.target.value);
        setSpeed(val);
        socket.emit('conveyor:speed', val);
    };

    const laneStatus = isEStop ? "⛔ E-STOP" : isJam ? "⚠️ JAMMED" : null;

    return (
        <div className="dashboard-container">
            <h1 id="dashboard-title">Smart Warehouse Dashboard</h1>

            <div className="status-bar">
                <span className={status === 'Connected' ? 'status-ok' : 'status-err'}>
                    Connection: {status}
                </span>
                <span className={conveyorRunning ? 'status-running' : 'status-stopped'}>
                    Conveyor: {conveyorRunning ? 'RUNNING' : 'STOPPED'}
                </span>
            </div>

            <div className="grid-layout">
                {/* Controls & Sim */}
                <div className="controls-section">
                    <h2>⚙️ Controls</h2>
                    <div className="button-group">
                        <button className={conveyorRunning ? 'stop-btn' : 'start-btn'} onClick={toggleConveyor}>
                            {conveyorRunning ? 'STOP' : 'START'}
                        </button>
                    </div>
                    <div className="slider-container">
                        <label>Speed: {speed} RPM</label>
                        <input type="range" min="0" max="100" value={speed} onChange={handleSpeed} disabled={!conveyorRunning} />
                    </div>
                    <div className="sim-controls">
                        <h3>🛠️ Simulation & Reports</h3>
                        <div className="sim-buttons">
                            <button className="sim-btn jam" onClick={simulateJam}>Simulate Jam</button>
                            <button className="sim-btn estop" onClick={simulateEStop}>Simulate E-Stop</button>
                            <button className="sim-btn report" onClick={generateReport}>📄 Generate Report</button>
                        </div>
                    </div>
                </div>

                {/* Env Panel */}
                <div className="env-panel">
                    <h2>🌱 Environment</h2>
                    <div className="metrics-row">
                        <div className={`metric-box ${temperature > 30 ? 'warning' : ''}`}>
                            <span className="label">Temp</span>
                            <span className="value">{temperature.toFixed(1)}°C</span>
                        </div>
                        <div className={`metric-box fan-box ${fanRunning ? 'active' : ''}`}>
                            <span className="label">Fan</span>
                            <span className="value icon">{fanRunning ? '🌀 ON' : '⚫ OFF'}</span>
                        </div>
                        <div className="metric-box">
                            <span className="label">Energy</span>
                            <div className="progress-bar">
                                <div className="fill" style={{ width: `${energyScore}%`, backgroundColor: energyScore > 80 ? '#4caf50' : '#f44336' }}></div>
                            </div>
                            <span className="score-text">{energyScore.toFixed(0)}</span>
                        </div>
                    </div>
                </div>

                {/* Lanes */}
                <div className="routing-panel full-width">
                    <h2>📦 Live Lane Tracking</h2>
                    <div className="lanes-container">

                        {/* Lane 1: Added 'express' class */}
                        <div className="lane-box express">
                            <h3>Lane 1 (Express)</h3>
                            <div className={`package-slot ${laneStatus ? 'blocked-lane' : (lanes.Lane1 ? 'occupied' : '')}`}>
                                {laneStatus || lanes.Lane1 || "Empty"}
                            </div>
                        </div>

                        {/* Lane 2: Added 'standard' class */}
                        <div className="lane-box standard">
                            <h3>Lane 2 (Standard)</h3>
                            <div className={`package-slot ${laneStatus ? 'blocked-lane' : (lanes.Lane2 ? 'occupied' : '')}`}>
                                {laneStatus || lanes.Lane2 || "Empty"}
                            </div>
                        </div>

                        {/* Lane 3: Already has 'heavy' class */}
                        <div className="lane-box heavy">
                            <h3>Lane 3 (Heavy)</h3>
                            <div className={`package-slot ${laneStatus ? 'blocked-lane' : (lanes.Lane3 ? 'occupied' : '')}`}>
                                {laneStatus || lanes.Lane3 || "Empty"}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Chart */}
                <div className="chart-panel">
                    <h2>Trend Analysis</h2>
                    <ResponsiveContainer width="100%" height={200}>
                        <LineChart data={history}>
                            <CartesianGrid stroke="#444" strokeDasharray="3 3" />
                            <XAxis dataKey="time" stroke="#888" fontSize={10} />
                            <YAxis stroke="#888" fontSize={10} />
                            <Tooltip contentStyle={{ backgroundColor: '#333' }} />
                            <Line type="monotone" dataKey="speed" stroke="#8884d8" dot={false} strokeWidth={2} />
                            <Line type="monotone" dataKey="temp" stroke="#82ca9d" dot={false} strokeWidth={2} />
                        </LineChart>
                    </ResponsiveContainer>
                </div>

                {/* Alarm List */}
                <div className="alarm-panel full-width">
                    <div className="alarm-header">
                        <h2>⚠️ Active Alarms</h2>
                        <span className="alarm-count">{alarms.length}</span>
                    </div>
                    <ul className="alarm-list">
                        {alarms.map((alarm) => (
                            <li
                                key={alarm.id}
                                className={`alarm-item ${alarm.severity.toLowerCase()} clickable`}
                                onClick={() => setSelectedAlarm(alarm)}
                            >
                                <div className="alarm-info">
                                    <span className="alarm-time">{new Date(alarm.timestamp).toLocaleTimeString()}</span>
                                    <span className="alarm-severity">{alarm.severity}</span>
                                </div>
                                <div className="alarm-message">{alarm.message}</div>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>

            {/* UPDATE: Modal with Acknowledge Button */}
            {selectedAlarm && (
                <div className="modal-backdrop" onClick={() => setSelectedAlarm(null)}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className={`modal-header ${selectedAlarm.severity.toLowerCase()}`}>
                            <h3>{selectedAlarm.severity} Alert</h3>
                            <button className="close-btn" onClick={() => setSelectedAlarm(null)}>×</button>
                        </div>
                        <div className="modal-body">
                            <h4>{selectedAlarm.message}</h4>
                            <p><strong>Action:</strong> {selectedAlarm.recommended_action}</p>
                        </div>
                        <div className="modal-footer">
                            <button style={{
                                backgroundColor: '#4caf50',
                                color: 'white',
                                border: 'none',
                                padding: '8px 16px',
                                borderRadius: '4px',
                                cursor: 'pointer',
                                fontWeight: 'bold'
                            }} onClick={acknowledgeAlarm}>
                                ✅ Acknowledge
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default App;