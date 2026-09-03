import { useState } from "react";
import { loginUser } from "../Services/authService";
import { useNavigate } from "react-router-dom";

import "./Login.css";

function Login({showToast}) {

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const navigate = useNavigate();

    const handleLogin = async (e) => {

        e.preventDefault();

        setError("");

        if (!email || !password) {
            setError("Please enter email and password.");
            return;
        }

        try {

            const data = await loginUser({
                email,
                password
            });

            console.log("Login response:", data);

            localStorage.setItem("token", data.token);

            localStorage.setItem(
                "user",// key
                JSON.stringify({// value
                    userId: data.userId,
                    name: data.name,
                    email: data.email,
                    role: data.role
                })
            );
// login success for admin, admin gets navigated to /admin which is adminhome. moves to app.jsx, app.jsx has ProtectedRoute allowedRole="User">
// so protectedroute file gets allowedroute parameter from app.jsx.
            if (data.role === "Admin") {
                navigate("/admin");
            }
            else {
                navigate("/home");
            }
            showToast("Login successful!");
        } catch (error) {

            if (error.response) {

                setError(
                    error.response.data.message ||
                    "Invalid email or password."
                );

            } else {

                setError("Unable to connect to the server.");

            }
        }
    };

    return (
        <div className="auth-container">

            <div className="auth-card">

                <h1>Login</h1>

                <form onSubmit={handleLogin}>

                    <div className="form-group">

                        <label>Email</label>
                        <input type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Enter your email" />
                    </div>

                    <div className="form-group">
                        <label>Password</label>
                        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Enter your password" />

                    </div>

                    {error && (
                        <p className="error-message">
                            {error}
                        </p>
                    )}

                    <button type="submit">
                        Login
                    </button>

                </form>

                <p> New user?{" "}
                    <button type="button" onClick={() => navigate("/register")}>Register</button>
                </p>

            </div>

        </div>
    );
}

export default Login;