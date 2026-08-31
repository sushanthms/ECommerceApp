import { useEffect, useState } from "react";
import { Navigate } from "react-router-dom";
import axios from "axios";

const API_URL = `${import.meta.env.VITE_API_URL}/Auth`;

function ProtectedRoute({ children, allowedRole }) {

    const token = localStorage.getItem("token");

    const [authorized, setAuthorized] = useState(false);
    const [checking, setChecking] = useState(true);// checking means Are we still waiting

    if (!token) {
        return <Navigate to="/login" replace />;
    }

    useEffect(() => {

        const checkAuthorization = async () => {

            try {

                let url = "";

                if (allowedRole === "Admin") {
                    url = `${API_URL}/admin-test`;
                }
                else if (allowedRole === "User") {
                    url = `${API_URL}/user-test`;
                }

                await axios.get(url, {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                });

                setAuthorized(true);

            } catch (error) {

                setAuthorized(false);

            } finally {

                setChecking(false);

            }
        };

        checkAuthorization();

    }, [token, allowedRole]);

    if (checking) {
        return <p>Checking authorization...</p>;    }

    if (!authorized) {
        return <Navigate to="/login" replace />;
    }

    return children;
}

export default ProtectedRoute;