import api from './api';

const youtubeService = {
  submitVideo: async (url) => {
    const response = await api.post('/youtube/process', { url });
    return response.data;
  },

  getResult: async (id) => {
    const response = await api.get(`/youtube/result/${id}`);
    return response.data;
  },

  getHistory: async () => {
    const response = await api.get('/youtube/history');
    return response.data;
  },
};

export default youtubeService; 