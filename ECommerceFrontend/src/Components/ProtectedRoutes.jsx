import { Navigate } from "react-router-dom";
import { useAuth } from "../Context/AuthContext.jsx";

function ProtectedRoute({ allowedRole, children }) {
    const { user, isLoggedIn } = useAuth();// when protectedroute asks the value of user and isLoggedIn, AuthProvider already has the value 

    if (!isLoggedIn) return <Navigate to="/login" replace />;

    if (allowedRole && user.role !== allowedRole) return <Navigate to="/" replace />;

    return children;
}

export default ProtectedRoute;