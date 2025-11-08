import { useEffect, useState } from 'react';
import io from 'socket.io-client';
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    Tooltip,
    CartesianGrid,
} from 'recharts';

const socket = io('http://localhost:3000');

function App() {
    const [status, setStatus] = useState('Disconnected');
    const [conveyorRunning, setConveyorRunning] = useState(false);
    const [speed, setSpeed] = useState(50);
    const [speedHistory, setSpeedHistory] = useState<{ time: number; speed: number }[]>([]);

    // Connection status
    useEffect(() => {
        socket.on('connect', () => setStatus('Connected'));
        socket.on('disconnect', () => setStatus('Disconnected'));
    }, []);

    // Prepare for backend data stream
    useEffect(() => {
        socket.on('conveyor:update', (data) => {
            setConveyorRunning(data.running);
            setSpeed(data.speed);
            setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: data.speed }]);
        });

        return () => {
            socket.off('conveyor:update');
        };
    }, []);

    // Manual controls
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
        setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: newSpeed }]);
    };

    // Fallback chart data
    const chartData = speedHistory.length > 0
        ? speedHistory
        : [{ time: Date.now(), speed }];

    return (
        <div style={{ padding: '20px', fontFamily: 'Arial' }}>
            <h1 id="dashboard-title">Smart Warehouse Dashboard</h1>
            <p id="connection-status">Connection Status: {status}</p>
            <p id="conveyor-status">Conveyor: {conveyorRunning ? 'Running' : 'Stopped'}</p>
            <button onClick={handleStart}>Start Conveyor</button>
            <button onClick={handleStop}>Stop Conveyor</button>

            <div style={{ marginTop: '20px' }}>
                <label htmlFor="speed-slider">Speed: </label>
                <input
                    id="speed-slider"
                    type="range"
                    min="0"
                    max="100"
                    value={speed}
                    onChange={handleSpeedChange}
                />
                <span> {speed}</span>
            </div>

            <div id="chart-container" style={{ marginTop: '40px' }}>
                <h2>Speed Trend</h2>
                <LineChart width={500} height={250} data={chartData}>
                    <CartesianGrid stroke="#ccc" />
                    <XAxis
                        dataKey="time"
                        tickFormatter={(t: number) => new Date(t).toLocaleTimeString()}
                    />
                    <YAxis domain={[0, 100]} />
                    <Tooltip />
                    <Line type="monotone" dataKey="speed" stroke="#8884d8" />
                </LineChart>
            </div>
        </div>
    );
}

export default App;