import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import axios from "axios";

function ProtectedRoute({ children, allowedRole }) {
    const [ok, setOk] = useState(null); // null = still checking, true means allowed, false means not allowed

    useEffect(() => {
        const token = localStorage.getItem("token");

        if (!token) {
            setOk(false);
            return;
        }

        axios
            .get(`${import.meta.env.VITE_API_URL}/Auth/verify`, {
                headers: { Authorization: `Bearer ${token}` }// axios.get gives a result
            })// .then runs only when the server answered with a success status (200)
            //.then(...) needs a function to run when the server's answer arrives.
            .then((res) => setOk(!allowedRole || res.data.role === allowedRole))
            // !allowedrole means if there is no role, no role is required
            // !allowedRole is required when we have a route like <ProtectedRoute><Profile /></ProtectedRoute>,here  !allowedRole || res.data.role === allowedRole
            // It is useful when we have a page that both Admin and User should access means when we have no allowedRole
            .catch((error) => {
                if (error.response?.status === 401)
                    localStorage.removeItem("token");
                setOk(false);
            });
    }, [allowedRole]);

    if (ok === null) return <div>Checking authentication...</div>;

    return ok ? children : <Navigate to="/login" replace />;
}

export default ProtectedRoute;