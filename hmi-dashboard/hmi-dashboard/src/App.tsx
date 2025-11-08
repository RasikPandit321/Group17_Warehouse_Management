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

const socket = io('http://localhost:3001');

function App() {
    const [status, setStatus] = useState<string>('Disconnected');
    const [conveyorRunning, setConveyorRunning] = useState<boolean>(false);
    const [speed, setSpeed] = useState<number>(50);
    const [speedHistory, setSpeedHistory] = useState<{ time: number; speed: number }[]>([]);
    const [temperature, setTemperature] = useState<number>(0);

    useEffect(() => {
        socket.on('connect', () => setStatus('Connected'));
        socket.on('disconnect', () => setStatus('Disconnected'));

        socket.on('conveyor:update', (data: { running: boolean; speed: number }) => {
            setConveyorRunning(data.running);
            setSpeed(data.speed);
            setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: data.speed }]);
        });

        socket.on('env:temperature', ({ value }: { value: string }) => {
            const parsed = parseFloat(value);
            if (!isNaN(parsed)) setTemperature(parsed);
        });

        return () => {
            socket.off('connect');
            socket.off('disconnect');
            socket.off('conveyor:update');
            socket.off('env:temperature');
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
        setSpeedHistory((prev) => [...prev, { time: Date.now(), speed: newSpeed }]);
    };

    const chartData = speedHistory.length > 0
        ? speedHistory
        : [{ time: Date.now(), speed }];

    return (
        <div style={{ padding: '20px', fontFamily: 'Arial' }}>
            <h1>Smart Warehouse Dashboard</h1>
            <p>Connection Status: {typeof status === 'string' ? status : 'Unknown'}</p>
            <p>Conveyor: {conveyorRunning ? 'Running' : 'Stopped'}</p>
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
                <span> {typeof speed === 'number' ? speed : 0}</span>
            </div>

            <div style={{ marginTop: '40px' }}>
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

            <div style={{ marginTop: '40px' }}>
                <h2>Live Data</h2>
                <p>🌡 Temperature: {typeof temperature === 'number' ? temperature : 0}°C</p>
            </div>
        </div>
    );
}

export default App;