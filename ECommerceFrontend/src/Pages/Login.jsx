import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../Context/AuthContext.jsx";
import { useToast } from "../Context/ToastContext.jsx";
import { loginUser } from "../Services/authService";

import "./Login.css";

function Login({isPopup = false, onClose, onLoginSuccess, onRegister }) {

    const showToast = useToast();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");

    const navigate = useNavigate();
    const { login } = useAuth();

    const handleLogin = async (e) => {

        e.preventDefault();

        setError("");

        if (!email || !password) {
            setError("Please enter email and password.");
            return;
        }

        try {

            const data = await loginUser({email, password});

            login(// we call this code after the loginUser function has executed and got response from the backend
            {// here userId, name, email, role, all goes inside userData parameter of the login function in the AuthContext file.
                userId: data.userId,// data.token goes inside the second parameter of the login function in the AuthContext file.
                name: data.name,
                email: data.email,
                role: data.role
            },
            data.token
        );

            showToast("Login successful!");

            if (isPopup) {
                onLoginSuccess(data);
                onClose();
                return;
            }

            if (data.role === "Admin") {
                navigate("/admin");
            }
            else {
                navigate("/home");
            }

            }
            catch (error) {
                console.log("Login failed:", error.response?.status);
                if (error.response?.status === 429)
                    {
                        setError("Too many login attempts. Please wait before trying again.");
                    }
                else if (error.response?.status === 401)
                    {
                        setError("Invalid email or password.");
                    }
                else
                    {
                        setError("Something went wrong. Please try again.");
                    }
            }
    };

    return (
        <div className={isPopup ? "login-overlay" : "auth-container"} onClick={isPopup ? onClose : undefined}>

            <div className={isPopup ? "auth-card login-popup" : "auth-card"} onClick={isPopup ? (e) => e.stopPropagation() : undefined}>

                {isPopup && (<button className="login-close" onClick={onClose}>×</button>)}

                <h1>Login</h1>

                <form onSubmit={handleLogin}>

                    <div className="form-group">
                        <label>Email</label>
                        <input type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Enter your email"/>
                    </div>

                    <div className="form-group">
                        <label>Password</label>
                        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Enter your password"/>
                    </div>

                    {error && (<p className="error-message">{error}</p>)}

                    <button type="submit">Login</button>

                </form>

                <p>
                    New user?{" "}
                    <button type="button" onClick={() => {
                            if (isPopup) {
                                onRegister();
                            } else {
                                navigate("/register");
                            }
                        }}
                    >
                        Register
                    </button>
                </p>

            </div>

        </div>
    );
}

export default Login;