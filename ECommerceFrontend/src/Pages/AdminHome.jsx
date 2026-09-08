import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import axios from "axios";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import AdminBanner from "../Components/AdminBanner.jsx";
import { getAdminProducts, addProduct, updateProduct, searchAdminProducts } from "../Services/ProductService.jsx";

import "./AdminHome.css";

const API_URL = `${import.meta.env.VITE_API_URL}/Product`;

function AdminHome({showToast}) {

    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [file, setFile] = useState(null);
    const [products, setProducts] = useState([]);
    const [search, setSearch] = useState("");
    const [showAllProducts, setShowAllProducts] = useState(false);

    const [showForm, setShowForm] = useState(false);

    const [editingId, setEditingId] = useState(null);
    const [sku, setSku] = useState("");
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState("");
    const [stock, setStock] = useState("");
    const [category, setCategory] = useState("");
    const [imageUrl, setImageUrl] = useState("");

    const [formError, setFormError] = useState("");
    const [formSuccess, setFormSuccess] = useState("");

    const userData = localStorage.getItem("user");

    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        document.documentElement.setAttribute(
            "data-theme",
            darkMode ? "dark" : "light"
        );
    }, [darkMode]);

    const handleFileChange = (event) => {
        setFile(event.target.files[0]);
    };

    const handleUpload = async () => {

        if (!file) {
            showToast("Please select a CSV file.", "warning");
            return;
        }

        try {

            const token = localStorage.getItem("token");
            const formData = new FormData();
            formData.append("file", file);

            const response = await axios.post(
                `${API_URL}/upload`,
                formData,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );

            showToast(response.data.message, "success");

            setFile(null);
            loadProducts();

        } catch (error) {

            console.error("Upload error:", error);
            showToast(error.response?.data?.message || "Product upload failed.", "error");
        }
    };

    const loadProducts = async () => {
        try {
            const data = await getAdminProducts();
            setProducts(data);
        } catch (error) {
            console.error("Error loading products:", error);
        }
    };

    useEffect(() => {
        loadProducts();
    }, []);// This runs when AdminHome is first displayed.

    const resetForm = () => {
        setEditingId(null);
        setSku("");
        setName("");
        setDescription("");
        setPrice("");
        setStock("");
        setCategory("");
        setImageUrl("");
        setFormError("");
    };

    const startEdit = (product) => {
        setEditingId(product.id);
        setSku(product.sku);
        setName(product.name);
        setDescription(product.description);
        setPrice(product.price);
        setStock(product.stock);
        setCategory(product.category);
        setImageUrl(product.imageUrl);
        setFormError("");
        setFormSuccess("");
        setShowForm(true);
    };

    const handleAddClick = () => {
        resetForm();
        setFormSuccess("");
        setShowForm(true);
    };

    const handleCancel = () => {
        resetForm();
        setShowForm(false);
    };

    const handleSubmitProduct = async (e) => {

        e.preventDefault();

        setFormError("");
        setFormSuccess("");

        if (!name || !price || !stock) {
            setFormError("Name, Price, and Stock are required.");
            return;
        }

        const token = localStorage.getItem("token");

        const productData = {
            sku,
            name,
            description,
            price: parseFloat(price),
            stock: parseInt(stock),
            category,
            imageUrl
        };

        try {

            if (editingId) {
                await updateProduct(editingId, productData, token);
                setFormSuccess("Product updated successfully.");
                showToast("Product updated successfully!", "success");
            } else {
                await addProduct(productData, token);
                setFormSuccess("Product added successfully.");
                showToast("Product added successfully!", "success");
            }

            resetForm();
            loadProducts();
            setShowForm(false);

        } catch (error) {

            console.error("Save product error:", error);

            setFormError(
                error.response?.data?.message ||
                "Failed to save product."
            );
        }
    };

    const handleSearch = (value) => {
            setSearch(value);
        };
    
    useEffect(() => {
    const timer = setTimeout(async () => {
        try {
            if (search.trim() === "") {
                await loadProducts();
            } else {
                const data = await searchAdminProducts(search);
                setProducts(data);
            }
        } catch (error) {
            console.error("Error searching products:", error);
        }
    }, 300);

    return () => {
        clearTimeout(timer);
    };
}, [search]);

const handleHideProduct = async (id) => {
    try {
        const token = localStorage.getItem("token");

        await axios.put(
            `${API_URL}/${id}/hide`,
            {},
            {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            }
        );

        showToast("Product hidden successfully.", "success");

        loadProducts();
    } catch (error) {
        console.error("Hide product error:", error);

        showToast(
            error.response?.data?.message || "Failed to hide product.",
            "error"
        );
    }
};

const handleRestoreProduct = async (id) => {
    try {
        const token = localStorage.getItem("token");

        await axios.put(
            `${API_URL}/${id}/restore`,
            {},
            {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            }
        );

        showToast("Product made visible successfully.", "success");

        loadProducts();
    } catch (error) {
        console.error("Restore product error:", error);

        showToast(
            error.response?.data?.message || "Failed to restore product.",
            "error"
        );
    }
};

const handlePermanentDelete = async (id) => {
    const confirmed = window.confirm(
        "Are you sure you want to permanently delete this product? This cannot be undone."
    );

    if (!confirmed) {
        return;
    }

    try {
        const token = localStorage.getItem("token");

        await axios.delete(
            `${API_URL}/${id}/permanent`,
            {
                headers: {
                    Authorization: `Bearer ${token}`
                }
            }
        );

        showToast("Product permanently deleted.", "success");

        loadProducts();
    } catch (error) {
        console.error("Permanent delete error:", error);

        showToast(
            error.response?.data?.message || "Failed to permanently delete product.",
            "error"
        );
    }
};


    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin" />

            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin" />

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Welcome, {user?.name}!</h2>
                    </div>

                    <div className="upload-section">

                        <h2>Upload Products (CSV)</h2>
                        <p>Upload a CSV file containing product details.</p>

                        <div className="upload-controls">
                            <input type="file" accept=".csv" onChange={handleFileChange} />
                            <button onClick={handleUpload}>Upload Products</button>
                        </div>

                        {file && (<p className="selected-file">Selected file: {file.name}</p>)}
                    </div>

                    <div className="product-form-section">

                        <div className="section-heading">
                            <h2>Manage Products</h2>

                            {!showForm && (<button className="add-product-btn" onClick={handleAddClick}>+ Add Product</button>)}
                        </div>

                        {showForm && (

                             <div className="modal-overlay">

                            <form onSubmit={handleSubmitProduct} className="product-form">

                                <h3>{editingId ? "Edit Product" : "New Product"}</h3>

                                <div className="form-item">
                                    <label>SKU</label>
                                    <input type="text" value={sku} onChange={(e) => setSku(e.target.value)} placeholder="Product SKU"/>
                                </div>

                                <div className="form-row">
                                    <div className="form-item">
                                        <label>Name</label>
                                        <input type="text" value={name} onChange={(e) => setName(e.target.value)} placeholder="Product name" />
                                    </div>

                                    <div className="form-item">
                                        <label>Category</label>
                                        <input type="text" value={category} onChange={(e) => setCategory(e.target.value)} placeholder="Category" />
                                    </div>
                                </div>

                                <div className="form-item">
                                    <label>Description</label>
                                    <input type="text" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description" />
                                </div>

                                <div className="form-row">
                                    <div className="form-item">
                                        <label>Price</label>
                                        <input type="number" step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="0.00" />
                                    </div>

                                    <div className="form-item">
                                        <label>Stock</label>
                                        <input type="number" value={stock} onChange={(e) => setStock(e.target.value)} placeholder="0" />
                                    </div>
                                </div>

                                <div className="form-item">
                                    <label>Image URL</label>
                                    <input type="text" value={imageUrl} onChange={(e) => setImageUrl(e.target.value)} placeholder="https://..." />
                                </div>

                                {formError && <p className="field-error">{formError}</p>}

                                <div className="form-actions">
                                    <button type="submit">{editingId ? "Save Changes" : "Add Product"}</button>
                                    <button type="button" className="cancel-btn" onClick={handleCancel}>Cancel</button>
                                </div>

                            </form>
                            </div>
                        )}

                        {formSuccess && <p className="success-message">{formSuccess}</p>}
                    </div>

                    <div className="orders-section">
                        <div className="section-heading">
                            <h2>Manage Orders</h2>
                            <button className="view-orders-btn" onClick={() => navigate("/admin/orders")}>View Orders</button>
                        </div>
                    </div>

                    <AdminBanner />
                    
                    <div className="featured-section">

                    <div className="section-heading">
                        <h2>{showAllProducts ? "All Products" : "Products"}</h2>

                        <button className="view-products-btn" onClick={() => setShowAllProducts(!showAllProducts)}>{showAllProducts ? "Show Less" : "View All Products"}</button>
                    </div>

                    <div className="search-section">
                        <input type="text" value={search} onChange={(e) => handleSearch(e.target.value)} placeholder="Search products or categories..."/>
                    </div>

                        {products.length === 0 ? (
                            <p>No products found.</p>
                        ) : (
                            <div className="product-grid">
                                {(showAllProducts ? products : products.slice(0, 6)).map((product) => (// when showallproducts is true this runs products.map, when showallproducts is false this runs products.slice
                                    <div key={product.id} className="product-card">

                                        <div className="product-image">
                                            {product.imageUrl ? (
                                                <img src={product.imageUrl} alt={product.name} onError={(e) => {
                                                        e.target.style.display = "none";
                                                        e.target.nextSibling.style.display = "flex";
                                                    }}
                                                />
                                            ) : null}

                                            <span className="product-fallback" style={{display: product.imageUrl ? "none" : "flex"}}>📦</span>
                                        </div>

                                        <h3>{product.name}</h3>
                                        <p>{product.description.length > 100 ? product.description.substring(0, 100) + "...": product.description}</p>
                                        <strong>₹{product.price}</strong>
                                        <p>Stock: {product.stock}</p>
                                        <p>Category: {product.category}</p>

                                        <p className={product.isDeleted ? "product-status hidden" : "product-status visible"}>
    Status: {product.isDeleted ? "Hidden" : "Visible"}
</p>


                                        <button onClick={() => startEdit(product)}>Edit</button>

                                        {product.isDeleted ? (
    <button onClick={() => handleRestoreProduct(product.id)}>
        Make Visible
    </button>
) : (
    <button onClick={() => handleHideProduct(product.id)}>
        Hide Product
    </button>
)}

{product.isDeleted && (
    <button
        className="delete-product-btn"
        onClick={() => handlePermanentDelete(product.id)}
    >
        Permanently Delete
    </button>
)}

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