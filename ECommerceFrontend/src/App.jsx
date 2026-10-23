import { useEffect, useState } from "react";
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
import AdminNotifications from "./Pages/AdminNotifications.jsx";
import Community from "./Pages/Community.jsx";
import UserOrders from "./Pages/UserOrders.jsx";
import UserOrderDetails from "./Pages/UserOrderDetails.jsx";
import Wishlist from "./Pages/Wishlist.jsx";

import { NotificationProvider } from "./Context/NotificationContext.jsx";
import "./style.css";

function App() {
  
  const [loginPopup, setLoginPopup] = useState(false);
  const [pendingAction, setPendingAction] = useState(null);
  const [registerPopup, setRegisterPopup] = useState(false);
  

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

  return (
    <NotificationProvider >
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<UserHome openLoginPopup={openLoginPopup} />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/home" element={ <ProtectedRoute allowedRole="User"> <UserHome openLoginPopup={openLoginPopup} /> </ProtectedRoute> } />
        <Route path="/admin" element={<ProtectedRoute allowedRole="Admin"> <AdminHome /> </ProtectedRoute>} />
        <Route path="/products" element={<Products openLoginPopup={openLoginPopup} />} />
        <Route path="/product/:id" element={<ProductDetails openLoginPopup={openLoginPopup} />} />
        <Route path="/cart" element={<ProtectedRoute allowedRole="User"><Cart /></ProtectedRoute>} />
        <Route path="/checkout" element={<ProtectedRoute allowedRole="User"><Checkout /></ProtectedRoute>}/>
        <Route path="/orders" element={<ProtectedRoute allowedRole="User"><UserOrders /></ProtectedRoute>} />
        <Route path="/orders/:id" element={<ProtectedRoute allowedRole="User"><UserOrderDetails /></ProtectedRoute>} />
        <Route path="/admin/orders" element={<ProtectedRoute allowedRole="Admin"><AdminOrders /></ProtectedRoute>}/>
        <Route path="/admin/orders/:id"element={<ProtectedRoute allowedRole="Admin"><AdminOrderDetails /></ProtectedRoute>}/>
        <Route path="/admin/products" element={<ProtectedRoute allowedRole="Admin"><AdminManageProducts /></ProtectedRoute>}/>
        <Route path="/admin/banners" element={<ProtectedRoute allowedRole="Admin"><AdminBanner /></ProtectedRoute>}/>
        <Route path="/admin/reviews" element={<ProtectedRoute allowedRole="Admin"><AdminReviews /></ProtectedRoute>}/>
        <Route path="/admin/notifications" element={<ProtectedRoute allowedRole="Admin"><AdminNotifications /></ProtectedRoute>} />
        <Route path="/community" element={<ProtectedRoute allowedRole="User"><Community /></ProtectedRoute>} />
        <Route path="/wishlist" element={<ProtectedRoute allowedRole="User"><Wishlist /></ProtectedRoute>} />
      </Routes>

      {loginPopup && (
        <Login isPopup={true} onClose={closeLoginPopup} onLoginSuccess={handleLoginSuccess} onRegister={openRegisterPopup}/>
        )}

      {registerPopup && (<Register isPopup={true} onClose={closeRegisterPopup}
              onLogin={() => {
                  setRegisterPopup(false);
                  setLoginPopup(true);
              }}
          />
      )}

    </BrowserRouter>
    </NotificationProvider>
  );
}

export default App;