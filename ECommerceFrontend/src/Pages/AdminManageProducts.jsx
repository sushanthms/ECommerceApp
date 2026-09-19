import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import {uploadProducts, getAdminProducts, addProduct, uploadProductImages, updateProduct, searchAdminProducts, hideProduct, restoreProduct,permanentDeleteProduct} from "../Services/ProductService";

import "./AdminManageProducts.css";

function AdminManageProducts({ showToast }) {

    const navigate = useNavigate();

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [file, setFile] = useState(null);
    const [csvImageFiles, setCsvImageFiles] = useState([]);
    const [csvFolderFiles, setCsvFolderFiles] = useState([]);
    const [products, setProducts] = useState([]);
    const [search, setSearch] = useState("");

    const [page, setPage] = useState(1);
    const [pageSize] = useState(20);
    const [totalPages, setTotalPages] = useState(1);

    const [showForm, setShowForm] = useState(false);

    const [editingId, setEditingId] = useState(null);
    const [sku, setSku] = useState("");
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [price, setPrice] = useState("");
    const [stock, setStock] = useState("");
    const [category, setCategory] = useState("");
    const [imageFiles, setImageFiles] = useState([]);

    const [formError, setFormError] = useState("");
    const [formSuccess, setFormSuccess] = useState("");

    const userData = localStorage.getItem("user");
    const user = userData ? JSON.parse(userData) : null;

    useEffect(() => {
        document.documentElement.setAttribute("data-theme",darkMode ? "dark" : "light");
    }, [darkMode]);

    const handleFileChange = (event) => {
        setFile(event.target.files[0]);
    };

    const handleCsvImageChange = (event) => {
        setCsvImageFiles(Array.from(event.target.files));
    };

    const handleCsvFolderChange = (event) => {
        setCsvFolderFiles(Array.from(event.target.files));
    };

    const handleImageChange = (event) => {// stores image files
        setImageFiles(Array.from(event.target.files));
    };

    const handleUpload = async () => {

        if (!file) {
            showToast("Please select a CSV file.", "warning");
            return;
        }

        try {

            const allCsvImages = [...csvImageFiles, ...csvFolderFiles];
            const response = await uploadProducts(file, allCsvImages);

            showToast(response.message, "success");
            setFile(null);
            setCsvImageFiles([]);
            setCsvFolderFiles([]);

            setPage(1);
            setSearch("");
            setShowAllProducts(false);

            await loadProducts(1, "");

        } catch (error) {

            console.error("Upload error:", error);

            showToast(
                error.response?.data?.message || "Product upload failed.",
                "error"
            );
        }
};
    const loadProducts = async (pageNumber = 1, searchValue = "") => {

        try {

            let data;

            if (searchValue.trim() === "") {
                data = await getAdminProducts(pageNumber, pageSize);
            } else {
                data = await searchAdminProducts(searchValue, pageNumber, pageSize);
            }

            setProducts(data.products);
            setTotalPages(data.totalPages);

        } catch (error) {

            console.error("Error loading products:", error);
        }
    };

    useEffect(() => {

    const timer = setTimeout(async () => {

        const trimmedSearch = search.trim();

        try {

            let data;

            if (trimmedSearch === "") {

                data = await getAdminProducts(page, pageSize);

            } else if (trimmedSearch.length >= 2) {

                data = await searchAdminProducts(trimmedSearch, page,pageSize);

            } else {

                return;
            }

            setProducts(data.products);
            setTotalPages(data.totalPages);

        } catch (error) {

            console.error("Error loading products:", error);
        }

    }, 400);

    return () => {
        clearTimeout(timer);
    };

}, [search, pageSize, page]);

    const handleSearch = (value) => {
        setSearch(value);
    };

    const handlePreviousPage = () => {
    if (page > 1) {
        setPage(page - 1);
    }
    };

    const handleNextPage = () => {
        if (page < totalPages) {
            setPage(page + 1);
        }
    };

    const getPageNumbers = () => {
    const pages = [];

    if (totalPages <= 7) {
        for (let i = 1; i <= totalPages; i++) {
            pages.push(i);
        }

    } else if (page <= 4) {// near the beginning

        pages.push(1, 2, 3, 4, 5, "...", totalPages);

    } else if (page >= totalPages - 3) {// near the end

        pages.push(
            1,
            "...",
            totalPages - 4,
            totalPages - 3,
            totalPages - 2,
            totalPages - 1,
            totalPages
        );

    } else {

        pages.push(
            1,
            "...",
            page - 1,
            page,
            page + 1,
            "...",
            totalPages
        );
    }

    return pages;
};

const handlePageChange = (pageNumber) => {
    if (pageNumber >= 1 && pageNumber <= totalPages) {
        setPage(pageNumber);
    }
};

    const resetForm = () => {
        setEditingId(null);
        setSku("");
        setName("");
        setDescription("");
        setPrice("");
        setStock("");
        setCategory("");
        setImageFiles([]);
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
        setImageFiles([]);
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
            sku,// this got values from set states
            name,
            description,
            price: parseFloat(price),
            stock: parseInt(stock),
            category
        };

        try {

            if (editingId) {

                await updateProduct(editingId, productData, token);

                if (imageFiles.length > 0) {
                await uploadProductImages(editingId, imageFiles, token);
            }
                showToast("Product updated successfully!", "success");

            } else {

                const createdProduct = await addProduct(productData, token);

                if (imageFiles.length > 0) {
                    await uploadProductImages(createdProduct.id, imageFiles, token);
                }

                showToast("Product added successfully!", "success");
            }

            resetForm();
            setShowForm(false);

            setPage(1);
            await loadProducts(1, search);

        } catch (error) {

            console.error("Save product error:", error);
            setFormError(error.response?.data?.message || "Failed to save product.");
        }
    };

    const handleHideProduct = async (id) => {

        try {

            await hideProduct(id);

            showToast("Product hidden successfully.", "success");

            await loadProducts(page, search);

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

            await restoreProduct(id);

            showToast("Product made visible successfully.", "success");

            await loadProducts(page, search);

        } catch (error) {

            console.error("Restore product error:", error);

            showToast(error.response?.data?.message || "Failed to restore product.", "error");
        }
    };

    const handlePermanentDelete = async (id) => {

        const confirmed = window.confirm("Are you sure you want to permanently delete this product? This cannot be undone.");

        if (!confirmed) {
            return;
        }

        try {

            await permanentDeleteProduct(id);
            showToast("Product permanently deleted.", "success");
            await loadProducts(page, search);

        } catch (error) {

            console.error("Permanent delete error:", error);
            showToast(error.response?.data?.message || "Failed to permanently delete product.", "error");
        }
    };

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin"/>

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin"/>

                <main className="main-content">

                    <div className="upload-section">

                        <h2>Upload Products (CSV)</h2>
                        <p>Upload a CSV file containing product details.</p>

                        <div className="upload-controls">
                         <label className="file-upload-btn">Choose CSV File<input type="file" accept=".csv" onChange={handleFileChange}/> </label>
                         <label className="file-upload-btn">Choose Images<input type="file" accept="image/*" multiple onChange={handleCsvImageChange}/></label>
                         <label className="file-upload-btn">Choose Image Folder<input type="file" accept="image/*" multiple webkitdirectory="" onChange={handleCsvFolderChange}/></label>
                         <button onClick={handleUpload}>Upload Products</button>
                        </div>

                        {file && (
                            <p className="selected-file">Selected file: {file.name}</p>
                        )}

                        {csvImageFiles.length > 0 && (
                            <p className="selected-file">{csvImageFiles.length} product image(s) selected</p>
                        )}

                        {csvFolderFiles.length > 0 && (
                            <p className="selected-file">{csvFolderFiles.length} image(s) selected from folder</p>
                        )}

                    </div>

                    <div className="product-form-section">

                        <div className="section-heading">

                            <h2>Manage Products</h2>

                            {!showForm && (
                                <button className="add-product-btn" onClick={handleAddClick}>+ Add Product</button>
                            )}

                        </div>

                        {showForm && (

                            <div className="modal-overlay">

                                <form onSubmit={handleSubmitProduct} className="product-form">

                                    <h3>{editingId ? "Edit Product" : "New Product"}</h3>   

                                    <div className="form-row">

                                        <div className="form-item">
                                            <label>Name</label>
                                            <input type="text" value={name} onChange={(e) => setName(e.target.value)} placeholder="Product name"/>
                                        </div>

                                        <div className="form-item">
                                            <label>Category</label>
                                            <input type="text" value={category} onChange={(e) => setCategory(e.target.value)}placeholder="Category"/>
                                        </div>

                                    </div>

                                    <div className="form-item">
                                        <label>Description</label>
                                        <input type="text" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Description"/>

                                    </div>

                                    <div className="form-row">

                                        <div className="form-item">
                                            <label>Price</label>
                                            <input type="number" step="0.01" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="0.00"/>
                                        </div>

                                        <div className="form-item">
                                            <label>Stock</label>
                                            <input type="number" value={stock} onChange={(e) => setStock(e.target.value)} placeholder="0"/>
                                        </div>

                                    </div>

                                    <div className="form-item">
                                        <label>Product Images</label>
                                        <input type="file" accept="image/*" multiple onChange={handleImageChange}/>
                                            {imageFiles.length > 0 && (
                                                <p>{imageFiles.length} image(s) selected</p>
                                            )}
                                        </div>

                                    {formError && (
                                        <p className="field-error">{formError}</p>
                                    )}

                                    <div className="form-actions">
                                        <button type="submit">{editingId ? "Save Changes" : "Add Product"}</button>
                                        <button type="button" className="cancel-btn" onClick={handleCancel}>Cancel</button>
                                    </div>

                                </form>

                            </div>

                        )}

                    </div>

                    <div className="featured-section">

                        <div className="section-heading">
                            <h2>{search.trim() ? "Search Results" : "Products"}</h2>
                        </div>

                        <div className="search-section">
                            <input type="text" value={search} onChange={(e) => handleSearch(e.target.value)} placeholder="Search products, categories or SKU..."/>
                        </div>

                        {products.length === 0 ? (

                            <p>No products found.</p>

                        ) : (

                            <div className="product-grid">

                                {products.map((product) => (
                                    <div key={product.id} className="product-card">

                                        <div className="product-image">

                                            {product.images?.length > 0 ? (

                                                <img
                                                    src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${product.images[0].imageUrl}`}
                                                    alt={product.name}
                                                    onError={(e) => {
                                                        e.target.style.display = "none";
                                                        e.target.nextSibling.style.display = "flex";
                                                    }}
                                                />

                                            ) : null}

                                            <span className="product-fallback"
                                                style={{
                                                    display: product.images?.length > 0
                                                        ? "none"
                                                        : "flex"
                                                }}
                                            >
                                                📦
                                            </span>

                                        </div>

                                        <h3>{product.name}</h3>

                                        <p>
                                            {product.description?.length > 100
                                                ? product.description.substring(0, 100) + "..."
                                                : product.description}
                                        </p>

                                        <strong>₹{product.price}</strong>

                                        <p>Stock: {product.stock}</p>

                                        <p>Category: {product.category}</p>

                                        <p
                                            className={
                                                product.isDeleted
                                                    ? "product-status hidden"
                                                    : "product-status visible"
                                            }
                                        >
                                            Status:{" "}
                                            {product.isDeleted
                                                ? "Hidden"
                                                : "Visible"}
                                        </p>

                                        <button onClick={() => startEdit(product)}>Edit</button>

                                        {product.isDeleted ? (

                                            <button onClick={() =>handleRestoreProduct(product.id)}>Make Visible</button>

                                        ) : (

                                            <button onClick={() =>handleHideProduct(product.id)}>Hide Product</button>

                                        )}

                                        {product.isDeleted && (<button className="delete-product-btn" onClick={() =>handlePermanentDelete(product.id)}>Permanently Delete</button>)}

                                    </div>

                                ))}

                            </div>

                        )}

                        <div className="pagination-controls">

                            <button onClick={handlePreviousPage} disabled={page === 1}>← Previous</button>

                            {getPageNumbers().map((pageNumber, index) =>
                                pageNumber === "..." ? (
                                    <span key={`ellipsis-${index}`}>...</span>
                                ) : (
                                    <button key={pageNumber} className={page === pageNumber ? "active-page" : ""} onClick={() => handlePageChange(pageNumber)}>{pageNumber}</button>
                                )
                            )}

                            <button onClick={handleNextPage} disabled={page === totalPages}>Next →</button>

                        </div>

                    </div>

                </main>

            </div>
        </>
    );
}

export default AdminManageProducts;