import { useEffect, useState } from 'react';
import io from 'socket.io-client';
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    Tooltip,
    CartesianGrid,
    ResponsiveContainer
} from 'recharts';
import './App.css';
import simulatedData from './alarms.json';

const socket = io('http://localhost:3000');

interface Alarm {
    id: string;
    message: string;
    severity: 'High' | 'Medium' | 'Low';
    timestamp: number;
    description?: string;        // New Field
    recommended_action?: string; // New Field
}

function App() {
    const [status, setStatus] = useState('Disconnected');
    const [conveyorRunning, setConveyorRunning] = useState(false);
    const [speed, setSpeed] = useState(50);
    const [speedHistory, setSpeedHistory] = useState<{ time: number; speed: number }[]>([]);

    // Alarms State
    const [alarms, setAlarms] = useState<Alarm[]>([]);
    // Selected Alarm State (for the popup)
    const [selectedAlarm, setSelectedAlarm] = useState<Alarm | null>(null);

    useEffect(() => {
        socket.on('connect', () => setStatus('Connected'));
        socket.on('disconnect', () => setStatus('Disconnected'));

        socket.on('conveyor:update', (data) => {
            setConveyorRunning(data.running);
            setSpeed(data.speed);
            setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: data.speed }].slice(-20));
        });

        socket.on('alarm:update', (updatedAlarms: Alarm[]) => {
            setAlarms(updatedAlarms);
        });

        return () => {
            socket.off('connect');
            socket.off('disconnect');
            socket.off('conveyor:update');
            socket.off('alarm:update');
        };
    }, []);

    const handleStart = () => {
        socket.emit('conveyor:start');
        setConveyorRunning(true);
    };

    const handleStop = () => {
        socket.emit('conveyor:stop');
        setConveyorRunning(false);
    };

    const handleSpeedChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const newSpeed = Number(e.target.value);
        setSpeed(newSpeed);
        socket.emit('conveyor:speed', newSpeed);
        setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: newSpeed }].slice(-20));
    };

    const handleSimulateAlarms = () => {
        const currentAlarms: Alarm[] = simulatedData.map((alarm, index) => ({
            ...alarm,
            severity: alarm.severity as 'High' | 'Medium' | 'Low',
            timestamp: Date.now() - (index * 2000)
        }));
        setAlarms(currentAlarms);
    };

    const chartData = speedHistory.length > 0 ? speedHistory : [{ time: Date.now(), speed }];

    return (
        <div className="dashboard-container">
            <h1 id="dashboard-title">Smart Warehouse Dashboard</h1>

            <div className="status-bar">
                <span id="connection-status" className={status === 'Connected' ? 'status-ok' : 'status-err'}>
                    Connection: {status}
                </span>
                <span id="conveyor-status" className={conveyorRunning ? 'status-running' : 'status-stopped'}>
                    Conveyor: {conveyorRunning ? 'Running' : 'Stopped'}
                </span>
            </div>

            <div className="controls-section">
                <div className="button-group">
                    <button onClick={handleStart} disabled={conveyorRunning}>Start Conveyor</button>
                    <button onClick={handleStop} disabled={!conveyorRunning} className="stop-btn">Stop Conveyor</button>
                    <button onClick={handleSimulateAlarms} style={{ backgroundColor: '#ff9800', color: '#000' }}>
                        ⚠ Simulate Alarm
                    </button>
                </div>

                <div className="slider-container">
                    <label htmlFor="speed-slider">Speed Control: </label>
                    <input
                        id="speed-slider"
                        type="range"
                        min="0"
                        max="100"
                        value={speed}
                        onChange={handleSpeedChange}
                    />
                    <span className="speed-value">{speed} RPM</span>
                </div>
            </div>

            <div id="chart-container" className="chart-panel">
                <h2>Speed Trend (Real-time)</h2>
                <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={chartData}>
                        <CartesianGrid stroke="#444" strokeDasharray="5 5" />
                        <XAxis
                            dataKey="time"
                            tickFormatter={(t: number) => new Date(t).toLocaleTimeString()}
                            stroke="#ccc"
                        />
                        <YAxis domain={[0, 100]} stroke="#ccc" />
                        <Tooltip contentStyle={{ backgroundColor: '#333', border: 'none' }} labelStyle={{ color: '#ccc' }} />
                        <Line type="monotone" dataKey="speed" stroke="#8884d8" strokeWidth={2} dot={false} />
                    </LineChart>
                </ResponsiveContainer>
            </div>

            <div className="alarm-panel">
                <div className="alarm-header">
                    <h2>System Alarms</h2>
                    <span className="alarm-count">{alarms.length} Active</span>
                </div>

                {alarms.length === 0 ? (
                    <div className="no-alarms">No active alarms. System normal.</div>
                ) : (
                    <ul className="alarm-list">
                        {alarms.map((alarm) => (
                            <li
                                key={alarm.id}
                                className={`alarm-item ${alarm.severity.toLowerCase()} clickable`}
                                onClick={() => setSelectedAlarm(alarm)} // Click handler
                            >
                                <div className="alarm-info">
                                    <span className="alarm-time">{new Date(alarm.timestamp).toLocaleTimeString()}</span>
                                    <span className="alarm-severity">{alarm.severity}</span>
                                </div>
                                <div className="alarm-message">{alarm.message}</div>
                                <div className="click-hint">Click for details</div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            {/* Alarm Details Modal */}
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
                                <h5>Description:</h5>
                                <p>{selectedAlarm.description || "No description available."}</p>
                            </div>

                            <div className="detail-section">
                                <h5>Recommended Action:</h5>
                                <p>{selectedAlarm.recommended_action || "Investigate immediately."}</p>
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