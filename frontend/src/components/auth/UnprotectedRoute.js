import { Navigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

const UnprotectedRoute = ({ children }) => {
    const { isAuthenticated } = useAuth();

    if (isAuthenticated) {
        return <Navigate to="/" />;
    }

    return children;
};

export default UnprotectedRoute; 