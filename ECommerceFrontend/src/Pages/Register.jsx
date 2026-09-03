import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { registerUser } from "../Services/authService.jsx";

import "./Login.css";

function Register() {

    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [confirmPassword, setConfirmPassword] = useState("");

    const [error, setError] = useState("");
    const [success, setSuccess] = useState("");
    const [nameError, setNameError] = useState("");

    const navigate = useNavigate();

    const handleRegister = async (e) => {

        e.preventDefault();

        setError("");
        setSuccess("");

        if (!name || !email || !password || !confirmPassword) {
            setError("Please fill in all fields.");
            return;
        }

        const namePattern = /^[A-Za-z ]+$/;

        if (!namePattern.test(name)) {
            setError("Name should contain only alphabets.");
            return;
        }

        if (!email.includes("@") || !email.includes(".")) {
            setError("Please enter a valid email address.");
            return;
        }

        if (password.length < 6) {
            setError("Password must be at least 6 characters.");
            return;
        }

        if (password !== confirmPassword) {
            setError("Passwords do not match.");
            return;
        }

        try {

            const data = await registerUser({
                name,
                email,
                password
            });

            setSuccess(data.message);

        } catch (error) {

            if (error.response) {
                setError(
                    error.response.data.message ||
                    "Registration failed."
                );
            } else {
                setError("Unable to connect to the server.");
            }
        }
    };

    return (
        <div className="auth-container">

            <div className="auth-card">

                <h1>Create Account</h1>

                <form onSubmit={handleRegister}>

                    <div className="form-group">
                        <label>Name</label>

                        <input type="text" value={name}
                            onChange={(e) => {
                                const value = e.target.value;//When we type each letter, the onChange block runs first. Then, because setName() or setNameError() changes state, React re-renders the component, and during that re-render React evaluates the JSX around the input, including {nameError && (...)}.

                                setName(value);

                                if (!/^[A-Za-z ]*$/.test(value)) {
                                    setNameError(
                                        "Name should contain only alphabets."
                                    );
                                } else {
                                    setNameError("");
                                }
                            }}
                            placeholder="Enter your name"
                        />

                        {nameError && (
                            <p className="error-message">
                                {nameError}
                            </p>
                        )}
                    </div>

                    <div className="form-group">
                        <label>Email</label>
                        <input type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Enter your email"/>
                    </div>

                    <div className="form-group">
                        <label>Password</label>
                        <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Enter password"/>
                    </div>

                    <div className="form-group">
                        <label>Confirm Password</label>
                        <input type="password" value={confirmPassword} onChange={(e) =>setConfirmPassword(e.target.value)}placeholder="Confirm password"/>
                    </div>

                    {error && (
                        <p className="error-message">
                            {error}
                        </p>
                    )}

                    {success && (
                        <p className="success-message">
                            {success}
                        </p>
                    )}

                    <button type="submit">Register</button>

                </form>

                <p>Already have an account?{" "}
                    <button type="button" onClick={() => navigate("/login")}>Login</button>
                </p>

            </div>
        </div>
    );
}

export default Register;
