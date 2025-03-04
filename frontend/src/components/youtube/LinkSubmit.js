import React, { useState } from 'react';
import { 
  Container, 
  Paper, 
  TextField, 
  Button, 
  Typography,
  Box 
} from '@mui/material';
import { toast } from 'react-toastify';
import api from '../../services/api';
import TranscriptionResult from './TranscriptionResult';
import { OrbitProgress } from 'react-loading-indicators';

const LinkSubmit = () => {
  const [link, setLink] = useState('');
  const [loading, setLoading] = useState(false);
  const [transcription, setTranscription] = useState(null);
  const [transcriptionStatus, setTranscriptionStatus] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await api.post('/Speech/totext', { url: link });
      console.log('Response:', response);
      toast.success('Video processed successfully!');
      setTranscription(response.data.transcription);
      setTranscriptionStatus(true)
    } catch (error) {
      console.error('Full error:', error);
      toast.error(error.response?.data?.message || 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleReset = () => {
    setTranscription(null);
    setTranscriptionStatus(false)
    setLink('');
  };

  if (transcription) {
    return <TranscriptionResult transcription={transcription} onReset={handleReset} transcriptionStatus = {transcriptionStatus} />;
  }

  return (
    <Container maxWidth="sm" >
      <Paper elevation={3} sx={{ p: 4, mt: 4 }}>
        <Typography variant="h5" component="h1" gutterBottom>
          YouTube to Text Converter
        </Typography>
        <Box component="form" onSubmit={handleSubmit}>
          <TextField
            fullWidth
            label="YouTube URL"
            value={link}
            onChange={(e) => setLink(e.target.value)}
            margin="normal"
            required
            disabled={loading}
          />
          {loading ? <Box sx={{ display: 'flex', justifyContent: 'center', my: 4 }}>
            <OrbitProgress color="#1976d2" size="medium" text="" textColor="" />
          </Box> : ""}
          
          <Button
            type="submit"
            variant="contained"
            fullWidth
            sx={{ mt: 2 }}
            disabled={loading}
          >
            {loading ? 'Processing...' : 'Convert to Text'}
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default LinkSubmit; 