import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";

import { getProducts } from "../Services/ProductService";
import { getAllOrders } from "../Services/OrderService";

import "./AdminHome.css";

function AdminHome() {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [products, setProducts] = useState([]);
    const [orders, setOrders] = useState([]);

    const navigate = useNavigate();

    useEffect(() => {
        document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
    }, [darkMode]);

    useEffect(() => {

        const loadDashboardData = async () => {

            try {

                const productData = await getProducts(1, 100);
                setProducts(productData.products);

                const orderData = await getAllOrders();
                setOrders(orderData);

            } catch (error) {

                console.error("Error loading dashboard data:", error);

            }
        };

        loadDashboardData();

    }, []);

    const totalProducts = products.length;

    const totalOrders = orders.length;

    const pendingOrders = orders.filter(
        order => order.status === "Pending"
    ).length;

    const lowStockProducts = products.filter(
        product => product.stock <= 5
    ).length;

    const totalSales = orders
        .filter(order => order.paymentStatus === "Paid")
        .reduce((total, order) => total + order.totalAmount, 0);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin"/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} role="Admin"/>

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
                            <strong>{lowStockProducts}</strong>
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

                        {orders.length === 0 ? (

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

                                {orders.slice(0, 5).map((order) => (

                                    <div className="table-row" key={order.id}
                                        onClick={() =>
                                            navigate(`/admin/orders/${order.id}`, {
                                                state: { order }
                                            })
                                        }
                                    >
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

                        {lowStockProducts === 0 ? (

                            <p>No low-stock products.</p>

                        ) : (

                            <div className="low-stock-list">

                                {products
                                    .filter(product => product.stock <= 5)
                                    .slice(0, 5)
                                    .map((product) => (

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