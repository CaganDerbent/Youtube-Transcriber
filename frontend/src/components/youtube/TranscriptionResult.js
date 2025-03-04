import React from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Button,
  Divider,
} from '@mui/material';
import { toast } from 'react-toastify';
import ContentCopyIcon from '@mui/icons-material/ContentCopy';
import DownloadIcon from '@mui/icons-material/Download';

const TranscriptionResult = ({ transcription, onReset, transcriptionStatus }) => {
  const handleCopyText = () => {
    navigator.clipboard.writeText(transcription)
      .then(() => toast.success('Text copied to clipboard!'))
      .catch(() => toast.error('Failed to copy text'));
  };

  const handleDownload = () => {
    const element = document.createElement('a');
    const file = new Blob([transcription], { type: 'text/plain' });
    element.href = URL.createObjectURL(file);
    element.download = 'transcription.txt';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
  };

  // Return null or loading state if transcriptionStatus is false
  if (!transcriptionStatus) {
    return null;
  }

  return (
    <Container maxWidth="md">
      <Paper elevation={3} sx={{ p: 4, mt: 4 }}>
        <Typography variant="h5" component="h1" gutterBottom>
          Transcription Result
        </Typography>

        <Box sx={{ mb: 2 }}>
          <Button
            variant="contained"
            startIcon={<ContentCopyIcon />}
            onClick={handleCopyText}
            sx={{ mr: 2 }}
          >
            Copy Text
          </Button>
          <Button
            variant="outlined"
            startIcon={<DownloadIcon />}
            onClick={handleDownload}
          >
            Download
          </Button>
        </Box>

        <Divider sx={{ my: 2 }} />

        <Typography variant="h6" gutterBottom>
          Transcription:
        </Typography>
        <Paper
          elevation={1}
          sx={{
            p: 2,
            maxHeight: '400px',
            overflow: 'auto',
            backgroundColor: '#f5f5f5',
          }}
        >
          <Typography variant="body1" component="pre" sx={{ whiteSpace: 'pre-wrap' }}>
            {transcription}
          </Typography>
        </Paper>

        <Box sx={{ mt: 3, textAlign: 'center' }}>
          <Button
            variant="contained"
            color="primary"
            onClick={onReset}
          >
            Convert Another Video
          </Button>
        </Box>
      </Paper>
    </Container>
  );
};

export default TranscriptionResult; 