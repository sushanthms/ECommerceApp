import { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import ProtectedRoute from "./Components/ProtectedRoutes.jsx";

import Home from "./Pages/Home.jsx";
import Login from "./Pages/Login.jsx";
import Register from "./Pages/Register.jsx";
import UserHome from "./Pages/UserHome.jsx";
import AdminHome from "./Pages/AdminHome.jsx";
import ProductDetails from "./Components/ProductDetails.jsx";
import Cart from "./Components/Cart.jsx";
import Checkout from "./Components/Checkout.jsx";
import AdminOrders from "./Pages/AdminOrders.jsx";
import AdminOrderDetails from "./Pages/AdminOrderDetails.jsx";
import Toast from "./Components/Toast.jsx";

import "./style.css";

function App() {
  
  const [darkMode, setDarkMode] = useState(false);
  const [toast, setToast] = useState("");

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
  }, [darkMode]);

const showToast = (message) => {
    setToast(message);

    setTimeout(() => {
        setToast("");
    }, 3000);
};
// Gives the showToast function from App to Login file.
  return (
    <BrowserRouter>
    <Toast message={toast} />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login showToast={showToast} />} />
        <Route path="/register" element={<Register showToast={showToast} />} />
        <Route path="/home" element={ <ProtectedRoute allowedRole="User"> <UserHome showToast={showToast}/> </ProtectedRoute> } />
        <Route path="/admin" element={<ProtectedRoute allowedRole="Admin"> <AdminHome showToast={showToast} /> </ProtectedRoute>} />
        <Route path="/product/:id" element={ <ProtectedRoute allowedRole="User"><ProductDetails showToast={showToast}/></ProtectedRoute>} />
        <Route path="/cart" element={<ProtectedRoute allowedRole="User"><Cart showToast={showToast} /></ProtectedRoute>} />
        <Route path="/checkout" element={<ProtectedRoute allowedRole="User"><Checkout showToast={showToast} /></ProtectedRoute>}/>
        <Route path="/admin/orders" element={<ProtectedRoute allowedRole="Admin"><AdminOrders /></ProtectedRoute>}/>
        <Route path="/admin/orders/:id"element={<ProtectedRoute allowedRole="Admin"><AdminOrderDetails /></ProtectedRoute>}/>
        
      </Routes>

      
    </BrowserRouter>
  );
}

export default App;