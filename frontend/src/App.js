import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

import Navbar from './components/layout/Navbar';
import Footer from './components/layout/Footer';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import LinkSubmit from './components/youtube/LinkSubmit';
import TranscriptionResult from './components/youtube/TranscriptionResult';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/auth/ProtectedRoute';
import History from './components/layout/History';
import UnprotectedRoute from './components/auth/UnprotectedRoute';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <AuthProvider>
      <ThemeProvider theme={theme}>
        <BrowserRouter>
          <div className="App" style={{ position: 'relative' }}>
            <Navbar />
            <main style={{ minHeight: 'calc(100vh - 130px)', padding: '20px' }}>
              <Routes>
                <Route path="/login" element={
                    <UnprotectedRoute>
                        <Login />
                    </UnprotectedRoute>
                } />
                <Route path="/register" element={
                    <UnprotectedRoute>
                        <Register />
                    </UnprotectedRoute>
                } />
                <Route path="/history" element={
                    <ProtectedRoute>
                        <History />
                    </ProtectedRoute>
                } />
                <Route
                  path="/"
                  element={
                    <ProtectedRoute>
                      <LinkSubmit/>
                      <TranscriptionResult />
                    </ProtectedRoute>
                  }
                />
              </Routes>
            </main>
            <Footer />
            <ToastContainer />
          </div>
        </BrowserRouter>
      </ThemeProvider>
    </AuthProvider>
  );
}

export default App;
