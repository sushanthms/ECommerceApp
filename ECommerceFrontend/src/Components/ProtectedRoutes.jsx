import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import axios from "axios";

function ProtectedRoute({ children, allowedRole }) {
    const [checking, setChecking] = useState(true);
    const [authorized, setAuthorized] = useState(false);

    const token = localStorage.getItem("token");

    useEffect(() => {
        const verifyToken = async () => {
            if (!token) {
                setChecking(false);
                return;
            }

            try {
                const response = await axios.get(
                    `${import.meta.env.VITE_API_URL}/Auth/verify`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );

                const actualRole = response.data.role;

                if (allowedRole && actualRole !== allowedRole) {
                    setAuthorized(false);
                    setChecking(false);
                    return;
                }

                setAuthorized(true);
                setChecking(false);
            } 
            catch (error) {
                console.log("VERIFY ERROR:", error);
                console.log("STATUS:", error.response?.status);
                console.log("DATA:", error.response?.data);

                setAuthorized(false);
                setChecking(false);
            }
        };

        verifyToken();
    }, [token, allowedRole]);

    if (checking) {
        return <div>Checking authentication...</div>;
    }

    if (!authorized) {
        return <Navigate to="/login" replace />;
    }

    return children;
}

export default ProtectedRoute;
