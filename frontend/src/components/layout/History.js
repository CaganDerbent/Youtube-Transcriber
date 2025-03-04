import { useEffect, useState } from "react";
import api from "../../services/api";
import { toast } from "react-toastify";

const History = () => {
    const [history, setHistory] = useState([]);

    const getHistory = async () => {
        try {
            console.log('Fetching history...');
            const response = await api.get('/Speech/history');
            console.log('History response:', response.data);
            setHistory(response.data);
        } catch (error) {
            console.error('History fetch error:', error);
            toast.error(error.response?.data?.message || 'An error occurred');
        }
    };

    useEffect(() => {
        getHistory();
    }, []);

    return (
        <div className="container mx-auto p-4">
            <h1 className="text-2xl font-bold mb-4">Transcription History</h1>
            {history.length === 0 ? (
                <p>No transcription history found.</p>
            ) : (
                <div className="space-y-4">
                    {[...history]
                        .sort((a, b) => new Date(b.date) - new Date(a.date))
                        .map((item, index) => (
                        <div key={index} className="border p-4 rounded-lg shadow-lg bg-white" style={{boxShadow: "rgba(0, 0, 0, 0.24) 0px 3px 8px",padding:"4px",marginTop:"8px",borderRadius:"8px"}}>
                            <p className="text-sm text-gray-500 mb-2">
                                <strong>{new Date(item.date).toLocaleString()}</strong>
                            </p>
                            <a href={item.url} style={{color:"#1976d2"}}
                               target="_blank" 
                               rel="noopener noreferrer" 
                               className="text-blue-600 hover:underline block mb-2">
                                {item.url}
                            </a>
                            <p className="text-gray-700">{item.transcription}</p>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}

export default History;
