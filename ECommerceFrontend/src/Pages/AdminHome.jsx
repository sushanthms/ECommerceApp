import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";

import { getProductStats } from "../Services/ProductService";
import { getOrderStats } from "../Services/OrderService";

import "./AdminHome.css";

function AdminHome() {

    const navigate = useNavigate();

    const [productStats, setProductStats] = useState({
    totalProducts: 0,
    lowStockCount: 0,
    lowStockProducts: []
});

const [orderStats, setOrderStats] = useState({
    totalOrders: 0,
    pendingOrders: 0,
    totalSales: 0,
    recentOrders: []
});

    useEffect(() => {
        const loadDashboardData = async () => {
            try {
                const [prodStats, ordStats] = await Promise.all([
                    getProductStats(),
                    getOrderStats()
                ]);

                setProductStats(prodStats);
                setOrderStats(ordStats);
            } catch (error) {
                console.error("Error loading dashboard data:", error);
            }
        };

        loadDashboardData();
    }, []);

    const totalProducts = productStats.totalProducts;
    const lowStockCount = productStats.lowStockCount;

    const totalOrders = orderStats.totalOrders;
    const pendingOrders = orderStats.pendingOrders;
    const totalSales = orderStats.totalSales;

    return (
        <>
            <Header />

            <div className="page-layout">

                <Sidebar />

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Admin Dashboard</h2>
                        <p>Overview of your e-commerce store.</p>
                    </div>

                    <div className="dashboard-cards">

                        <div className="dashboard-card">
                            <h3>Total Products</h3>
                            <strong>{totalProducts}</strong>
                        </div>

                        <div className="dashboard-card">
                            <h3>Total Orders</h3>
                            <strong>{totalOrders}</strong>
                        </div>

                        <div className="dashboard-card">
                            <h3>Pending Orders</h3>
                            <strong>{pendingOrders}</strong>
                        </div>

                        <div className="dashboard-card">
                            <h3>Total Sales</h3>
                            <strong>₹{totalSales}</strong>
                        </div>

                        <div className="dashboard-card">
                            <h3>Low Stock Products</h3>
                            <strong>{lowStockCount}</strong>
                        </div>

                    </div>

                    <div className="dashboard-section">

                        <div className="section-heading">
                            <h2>Quick Actions</h2>
                        </div>

                        <div className="quick-actions">

                            <button onClick={() => navigate("/admin/products")}>Manage Products</button>
                            <button onClick={() => navigate("/admin/orders")}>Manage Orders</button>
                            <button onClick={() => navigate("/admin/banners")}>Manage Banners</button>
                            <button onClick={() => navigate("/admin/reviews")}>Manage Reviews</button>

                        </div>

                    </div>

                    <div className="dashboard-section">

                        <div className="section-heading">
                            <h2>Recent Orders</h2>
                        </div>

                        {orderStats.recentOrders.length === 0 ? (

                            <p>No orders found.</p>

                        ) : (

                            <div className="dashboard-table">

                                <div className="table-row table-header">
                                    <span>Order ID</span>
                                    <span>Customer</span>
                                    <span>Total</span>
                                    <span>Payment</span>
                                    <span>Status</span>
                                </div>

                                {orderStats.recentOrders.map((order) => (
                                    <div className="table-row" key={order.id}
                                        onClick={() => navigate(`/admin/orders/${order.id}`)}>
                                        <span>#{order.id}</span>
                                        <span>{order.deliveryName}</span>
                                        <span>₹{order.totalAmount}</span>
                                        <span>{order.paymentStatus}</span>
                                        <span>{order.status}</span>
                                    </div>
                                ))}
                            </div>
                        )}

                    </div>

                    <div className="dashboard-section">

                        <div className="section-heading">
                            <h2>Low Stock Products</h2>
                        </div>

                        {lowStockCount === 0 ? (

                            <p>No low-stock products.</p>

                        ) : (

                            <div className="low-stock-list">

                                {productStats.lowStockProducts.map((product) => (
                                    <div className="low-stock-item" key={product.id}>
                                        <span>{product.name}</span>
                                        <strong>{product.stock} left</strong>
                                    </div>
                                ))}

                            </div>

                        )}

                    </div>

                </main>

            </div>
        </>
    );
}

export default AdminHome;