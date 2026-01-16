import { useState } from 'react';
import { Container, TextField, Button, Paper, Typography, Box, Chip, Stack } from '@mui/material';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';

// --- CONFIGURATION ---
// In a real app, this comes from import.meta.env.VITE_API_URL
const API_URL = 'https://localhost:7443/api/intake';

// --- THEME DEFINITION ---
const darkTheme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#90caf9' }, // "Blue 200"
        background: { default: '#0d1117', paper: '#161b22' }, // GitHub Dark Dimmed
    },
    typography: {
        fontFamily: '"Roboto Mono", monospace',
    },
});

// --- SUB-COMPONENTS ---
// Breaking this out makes the main App logic cleaner
const ScanResult = ({ scan }) => (
    <Paper sx={{
        p: 2,
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        borderLeft: scan.status === 'FLAGGED' ? '4px solid #f44336' : '4px solid #4caf50',
        animation: 'fadeIn 0.3s ease-in'
    }}>
        <Box>
            <Typography variant="h6" sx={{ fontWeight: 'bold' }}>{scan.vin}</Typography>
            <Typography variant="caption" color="text.secondary">
                {scan.time} • {scan.latency}ms • {scan.details}
            </Typography>
        </Box>
        <Chip
            label={scan.status}
            color={scan.status === 'FLAGGED' ? 'error' : 'success'}
            size="small"
            sx={{ fontWeight: 'bold' }}
        />
    </Paper>
);

// --- MAIN APPLICATION ---
function App() {
    const [vin, setVin] = useState('');
    const [history, setHistory] = useState([]);
    const [loading, setLoading] = useState(false);

    const handleScan = async (e) => {
        e.preventDefault();
        if (!vin) return;

        setLoading(true);
        const startTime = performance.now();

        try {
            const response = await fetch(API_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(vin)
            });

            if (!response.ok) throw new Error(`API Error: ${response.statusText}`);

            const data = await response.json();

            const duration = Math.round(performance.now() - startTime);

            // Create the history record
            const newScan = {
                vin: vin,
                time: new Date().toLocaleTimeString(),
                latency: duration,
                status: parseFloat(data.grade) > 3.0 ? 'CLEARED' : 'FLAGGED',
                details: data.notes ? data.notes.join(', ') : 'No notes'
            };

            setHistory(prev => [newScan, ...prev]); // "Senior" state update pattern
            setVin('');

        } catch (error) {
            console.error("Intake Failed:", error);
            alert(`Connection Failed. Ensure API is running at ${API_URL}`);
        } finally {
            setLoading(false);
        }
    };

    return (
        <ThemeProvider theme={darkTheme}>
            <CssBaseline />
            <Container maxWidth="sm" sx={{ mt: 8 }}>

                {/* HEADER */}
                <Box sx={{ mb: 6, textAlign: 'center' }}>
                    <Typography variant="h4" sx={{ fontWeight: 'bold', letterSpacing: '-1px' }}>
                        AUTO<span style={{ color: '#90caf9' }}>MARKET</span>
                    </Typography>
                    <Typography variant="overline" sx={{ color: '#6e7681', letterSpacing: '2px' }}>
                        Wholesale Intake V2
                    </Typography>
                </Box>

                {/* INPUT FORM */}
                <Paper elevation={0} sx={{ p: 0, mb: 4, background: 'transparent' }}>
                    <form onSubmit={handleScan}>
                        <Stack spacing={2}>
                            <TextField
                                autoFocus
                                fullWidth
                                value={vin}
                                onChange={(e) => setVin(e.target.value.toUpperCase())}
                                placeholder="SCAN VIN..."
                                disabled={loading}
                                variant="outlined"
                                // This styling makes the input look massive and touch-friendly
                                InputProps={{
                                    sx: { fontSize: '1.5rem', fontFamily: 'monospace', bgcolor: '#161b22' }
                                }}
                            />
                            <Button
                                type="submit"
                                variant="contained"
                                size="large"
                                disabled={loading}
                                sx={{ height: 56, fontWeight: 'bold', fontSize: '1.1rem' }}
                            >
                                {loading ? 'PROCESSING...' : 'SUBMIT'}
                            </Button>
                        </Stack>
                    </form>
                </Paper>

                {/* RECENT SCANS */}
                <Stack spacing={2}>
                    {history.map((scan, index) => (
                        <ScanResult key={index} scan={scan} />
                    ))}
                </Stack>

            </Container>
        </ThemeProvider>
    );
}

export default App;