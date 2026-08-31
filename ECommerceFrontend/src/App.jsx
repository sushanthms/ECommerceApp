import { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import ProtectedRoute from "./Components/ProtectedRoutes.jsx";

import Home from "./Pages/Home.jsx";
import Login from "./Pages/Login.jsx";
import Register from "./Pages/Register.jsx";
import UserHome from "./Pages/UserHome.jsx";
import AdminHome from "./Pages/AdminHome.jsx";

import "./style.css";

function App() {
  
  const [darkMode, setDarkMode] = useState(false);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
  }, [darkMode]);

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/home" element={ <ProtectedRoute allowedRole="User"> <UserHome /> </ProtectedRoute> } />
        <Route path="/admin" element={<ProtectedRoute allowedRole="Admin"> <AdminHome /> </ProtectedRoute>} />
        
      </Routes>
    </BrowserRouter>
  );
}

export default App;