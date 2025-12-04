import { useEffect, useState } from 'react';
import {
    LineChart, Line, XAxis, YAxis, Tooltip, CartesianGrid, ResponsiveContainer, ReferenceLine
} from 'recharts';
import { socket } from './socket';
import './App.css';

// --- Types ---
interface Alarm {
    id: string;
    message: string;
    severity: 'High' | 'Medium' | 'Low';
    timestamp: number;
    description?: string;
    recommended_action?: string;
}

function App() {
    // --- State: Connection & Controls ---
    const [status, setStatus] = useState('Disconnected');
    const [conveyorRunning, setConveyorRunning] = useState(false);
    const [speed, setSpeed] = useState(50);

    // --- State: Sprint 3 Environment ---
    const [temperature, setTemperature] = useState<number>(20);
    const [fanRunning, setFanRunning] = useState<boolean>(false);
    const [energyScore, setEnergyScore] = useState<number>(100);

    // --- State: Routing & History ---
    const [barcode, setBarcode] = useState<string>('--');
    const [zone, setZone] = useState<string>('--');
    const [history, setHistory] = useState<{ time: string; speed: number; temp: number }[]>([]);

    // --- State: Sprint 2 Alarms (Restored) ---
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

    // --- Helpers ---
    const determineSeverity = (msg: string): 'High' | 'Medium' | 'Low' => {
        if (msg.toLowerCase().includes('stop') || msg.toLowerCase().includes('jam')) return 'High';
        if (msg.toLowerCase().includes('overweight')) return 'Medium';
        return 'Low';
    };

    const getAction = (msg: string) => {
        if (msg.toLowerCase().includes('jam')) return 'Clear physical obstruction and restart system.';
        if (msg.toLowerCase().includes('stop')) return 'Release E-Stop button and verify safety.';
        if (msg.toLowerCase().includes('overweight')) return 'Remove heavy package from rejection lane.';
        return 'Monitor system closely.';
    };

    // --- Socket Listeners ---
    useEffect(() => {
        socket.on('connect', () => setStatus('Connected'));
        socket.on('disconnect', () => setStatus('Disconnected'));

        // Sprint 3: Unified Update
        socket.on('system:update', (data) => {
            setConveyorRunning(data.conveyor.running);
            setSpeed(data.conveyor.speed);
            setTemperature(parseFloat(data.env.temp));
            setFanRunning(data.env.fanOn);
            setEnergyScore(data.env.energyScore);

            setHistory((prev) => [...prev, {
                time: new Date().toLocaleTimeString(),
                speed: data.conveyor.speed,
                temp: parseFloat(data.env.temp)
            }].slice(-20));
        });

        socket.on('barcode:scanned', ({ code }) => setBarcode(code));
        socket.on('diverter:activated', ({ zone }) => setZone(zone));

        // Sprint 2 Logic Adapted for Sprint 3 Stream
        socket.on('alarm:new', ({ message }) => {
            const newAlarm: Alarm = {
                id: Date.now().toString(),
                message: message,
                severity: determineSeverity(message),
                timestamp: Date.now(),
                description: `System detected event: ${message}`,
                recommended_action: getAction(message)
            };
            setAlarms((prev) => [newAlarm, ...prev]);
        });

        return () => {
            socket.off('connect');
            socket.off('disconnect');
            socket.off('system:update');
            socket.off('barcode:scanned');
            socket.off('diverter:activated');
            socket.off('alarm:new');
        };
    }, []);

    // --- Handlers ---
    const toggleConveyor = () => {
        const action = conveyorRunning ? 'conveyor:stop' : 'conveyor:start';
        socket.emit(action);
    };

    const handleSpeed = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = Number(e.target.value);
        setSpeed(val);
        socket.emit('conveyor:speed', val);
    };

    return (
        <div className="dashboard-container">
            <h1 id="dashboard-title">Smart Warehouse Dashboard</h1>

            {/* Header Status Bar */}
            <div className="status-bar">
                <span className={status === 'Connected' ? 'status-ok' : 'status-err'}>
                    Connection: {status}
                </span>
                <span className={conveyorRunning ? 'status-running' : 'status-stopped'}>
                    Conveyor: {conveyorRunning ? 'RUNNING' : 'STOPPED'}
                </span>
            </div>

            <div className="grid-layout">
                {/* 1. Controls */}
                <div className="controls-section">
                    <h2>⚙️ Controls</h2>
                    <div className="button-group">
                        <button
                            className={conveyorRunning ? 'stop-btn' : 'start-btn'}
                            onClick={toggleConveyor}
                        >
                            {conveyorRunning ? 'STOP' : 'START'}
                        </button>
                    </div>
                    <div className="slider-container">
                        <label>Speed: {speed} RPM</label>
                        <input type="range" min="0" max="100" value={speed} onChange={handleSpeed} disabled={!conveyorRunning} />
                    </div>
                </div>

                {/* 2. Sprint 3: Environment Panel */}
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
                            <span className="label">Energy Score</span>
                            <div className="progress-bar">
                                <div className="fill" style={{ width: `${energyScore}%`, backgroundColor: energyScore > 80 ? '#4caf50' : '#f44336' }}></div>
                            </div>
                            <span className="score-text">{energyScore.toFixed(0)}/100</span>
                        </div>
                    </div>
                </div>

                {/* 3. Routing Panel */}
                <div className="routing-panel">
                    <h2>📦 Live Routing</h2>
                    <div className="routing-display">
                        <span className="barcode">{barcode}</span>
                        <span className="arrow">➜</span>
                        <span className={`zone-badge ${zone === 'BLOCKED' ? 'blocked' : ''}`}>{zone}</span>
                    </div>
                </div>

                {/* 4. Charts */}
                <div className="chart-panel">
                    <h2>Trend Analysis</h2>
                    <ResponsiveContainer width="100%" height={200}>
                        <LineChart data={history}>
                            <CartesianGrid stroke="#444" strokeDasharray="3 3" />
                            <XAxis dataKey="time" stroke="#888" fontSize={10} />
                            <YAxis stroke="#888" fontSize={10} />
                            <Tooltip contentStyle={{ backgroundColor: '#333' }} />
                            <ReferenceLine y={30} stroke="red" strokeDasharray="3 3" />
                            <Line type="monotone" dataKey="speed" stroke="#8884d8" dot={false} strokeWidth={2} />
                            <Line type="monotone" dataKey="temp" stroke="#82ca9d" dot={false} strokeWidth={2} />
                        </LineChart>
                    </ResponsiveContainer>
                </div>

                {/* 5. Sprint 2: Interactive Alarm Panel */}
                <div className="alarm-panel full-width">
                    <div className="alarm-header">
                        <h2>⚠️ Active Alarms</h2>
                        <span className="alarm-count">{alarms.length}</span>
                    </div>
                    {alarms.length === 0 ? (
                        <div className="no-alarms">No active alarms. System normal.</div>
                    ) : (
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
                    )}
                </div>
            </div>

            {/* Sprint 2: Modal */}
            {selectedAlarm && (
                <div className="modal-backdrop" onClick={() => setSelectedAlarm(null)}>
                    <div className="modal-content" onClick={(e) => e.stopPropagation()}>
                        <div className={`modal-header ${selectedAlarm.severity.toLowerCase()}`}>
                            <h3>{selectedAlarm.severity} Alert</h3>
                            <button className="close-btn" onClick={() => setSelectedAlarm(null)}>×</button>
                        </div>
                        <div className="modal-body">
                            <h4>{selectedAlarm.message}</h4>
                            <p><strong>Time:</strong> {new Date(selectedAlarm.timestamp).toLocaleString()}</p>
                            <div className="detail-section">
                                <h5>Recommended Action:</h5>
                                <p>{selectedAlarm.recommended_action}</p>
                            </div>
                        </div>
                        <div className="modal-footer">
                            <button onClick={() => setSelectedAlarm(null)}>Acknowledge</button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}

export default App;