import { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";

import ProtectedRoute from "./Components/ProtectedRoutes.jsx";

import Login from "./Pages/Login.jsx";
import Register from "./Pages/Register.jsx";
import UserHome from "./Pages/UserHome.jsx";
import AdminHome from "./Pages/AdminHome.jsx";
import Products from "./Pages/Products.jsx";
import ProductDetails from "./Pages/ProductDetails.jsx";
import Cart from "./Pages/Cart.jsx";
import Checkout from "./Pages/Checkout.jsx";
import AdminOrders from "./Pages/AdminOrders.jsx";
import AdminOrderDetails from "./Pages/AdminOrderDetails.jsx";
import AdminManageProducts from "./Pages/AdminManageProducts.jsx";
import AdminBanner from "./Pages/AdminBanner.jsx";
import AdminReviews from "./Pages/AdminReviews";
import Community from "./Pages/Community.jsx";
import UserOrders from "./Pages/UserOrders.jsx";
import UserOrderDetails from "./Pages/UserOrderDetails.jsx";
import Toast from "./Components/Toast.jsx";

import "./style.css";

function App() {
  
  const [darkMode, setDarkMode] = useState(false);
  const [toast, setToast] = useState(null);
  const [loginPopup, setLoginPopup] = useState(false);
  const [pendingAction, setPendingAction] = useState(null);
  const [registerPopup, setRegisterPopup] = useState(false);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
  }, [darkMode]);

const showToast = (message, type = "info") => {
    setToast({ message, type });

    setTimeout(() => {
        setToast(null);
    }, 3000);
};

const openLoginPopup = (action = null) => {
    setPendingAction(() => action);
    setLoginPopup(true);
};

const closeLoginPopup = () => {
    setLoginPopup(false);
};

const handleLoginSuccess = () => {
    if (pendingAction) {
        pendingAction();
        setPendingAction(null);
    }
};

const openRegisterPopup = () => {
    setLoginPopup(false);
    setRegisterPopup(true);
};

const closeRegisterPopup = () => {
    setRegisterPopup(false);
};

// Gives the showToast function from App to Login file.
  return (
    <BrowserRouter>
    <Toast message={toast?.message} type={toast?.type} />
      <Routes>
        <Route path="/" element={<UserHome showToast={showToast} />} />
        <Route path="/login" element={<Login showToast={showToast} />} />
        <Route path="/register" element={<Register showToast={showToast} />} />
        <Route path="/home" element={ <ProtectedRoute allowedRole="User"> <UserHome showToast={showToast}/> </ProtectedRoute> } />
        <Route path="/admin" element={<ProtectedRoute allowedRole="Admin"> <AdminHome showToast={showToast} /> </ProtectedRoute>} />
        <Route path="/products" element={<Products showToast={showToast} openLoginPopup={openLoginPopup} />} />
        <Route path="/product/:id" element={<ProductDetails showToast={showToast} openLoginPopup={openLoginPopup} />} />
        <Route path="/cart" element={<ProtectedRoute allowedRole="User"><Cart showToast={showToast} /></ProtectedRoute>} />
        <Route path="/checkout" element={<ProtectedRoute allowedRole="User"><Checkout showToast={showToast} /></ProtectedRoute>}/>
        <Route path="/orders" element={<ProtectedRoute allowedRole="User"><UserOrders /></ProtectedRoute>} />
        <Route path="/orders/:id" element={<ProtectedRoute allowedRole="User"><UserOrderDetails /></ProtectedRoute>} />
        <Route path="/admin/orders" element={<ProtectedRoute allowedRole="Admin"><AdminOrders /></ProtectedRoute>}/>
        <Route path="/admin/orders/:id"element={<ProtectedRoute allowedRole="Admin"><AdminOrderDetails /></ProtectedRoute>}/>
        <Route path="/admin/products" element={<ProtectedRoute allowedRole="Admin"><AdminManageProducts showToast={showToast} /></ProtectedRoute>}/>
        <Route path="/admin/banners" element={<ProtectedRoute allowedRole="Admin"><AdminBanner /></ProtectedRoute>}/>
        <Route path="/admin/reviews" element={<ProtectedRoute allowedRole="Admin"><AdminReviews /></ProtectedRoute>}/>
        <Route path="/community" element={<ProtectedRoute allowedRole="User"><Community /></ProtectedRoute>} />
        
      </Routes>

      {loginPopup && (
        <Login showToast={showToast} isPopup={true} onClose={closeLoginPopup} onLoginSuccess={handleLoginSuccess} onRegister={openRegisterPopup}/>
        )}

      {registerPopup && (<Register showToast={showToast} isPopup={true} onClose={closeRegisterPopup}
              onLogin={() => {
                  setRegisterPopup(false);
                  setLoginPopup(true);
              }}
          />
      )}

    </BrowserRouter>
  );
}

export default App;