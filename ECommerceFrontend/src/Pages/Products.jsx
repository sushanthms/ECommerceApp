import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";
import { getProducts, searchProducts, getCategories } from "../Services/ProductService.jsx";

import "./Products.css";

function Products({ showToast }) {

    const [searchParams] = useSearchParams();

    const categoryFromUrl = searchParams.get("category") || "";

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [products, setProducts] = useState([]);
    const [search, setSearch] = useState("");
    const [category, setCategory] = useState("");
    const [categories, setCategories] = useState([]);

    const [page, setPage] = useState(1);// Which page is currently being displayed? when next is clicked setPage(page+1)
    const [pageSize] = useState(20);
    const [totalPages, setTotalPages] = useState(1);

    const navigate = useNavigate();

    useEffect(() => {document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");}, [darkMode]);

    useEffect(() => {
        setCategory(categoryFromUrl);
        setPage(1);// whenever url changes, updates the category and goes to page 1
    }, [categoryFromUrl]);

    const handleSearch = (value) => {
        setSearch(value);
        setPage(1);
    };

    const handleCategoryChange = (value) => {
        setCategory(value);
        setPage(1);
    };

    useEffect(() => {
        const timer = setTimeout(async () => {
            try {
                let data;

                if (search.trim() === "") {
                    data = await getProducts(page, pageSize, category);
                } else {
                    data = await searchProducts(search, page, pageSize, category);
                }

                setProducts(data.products);
                setTotalPages(data.totalPages);
            } catch (error) {
                console.error("Error loading products:", error);
            }
        }, 300);

        return () => {
            clearTimeout(timer);
        };
    }, [search, category, page, pageSize]);

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
    } else if (page <= 4) {
        pages.push(1, 2, 3, 4, 5, "...", totalPages);
    } else if (page >= totalPages - 3) {
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

    useEffect(() => {
    const loadCategories = async () => {
        try {
            const data = await getCategories();
            setCategories(data);
        } catch (error) {
            console.error("Error loading categories:", error);
        }
    };

    loadCategories();
}, []);

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")} />

            <div className="page-layout">
                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role="User" />

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Products</h2>
                        <p>Browse all available products.</p>
                    </div>

                    <div className="search-section">
                        <input type="text" value={search} onChange={(e) => handleSearch(e.target.value)} placeholder="Search products or categories..." />
                    </div>

                    <div className="filter-section">
                        <select value={category} onChange={(e) => handleCategoryChange(e.target.value)}>
                            <option value="">All Categories</option>

                            {categories.map((cat) => (
                                <option key={cat} value={cat}>{cat}</option>
                                ))}
                        </select>
                    </div>

                    <div className="featured-section">

                        <div className="section-heading">
                            <h2>All Products</h2>
                        </div>

                        {products.length === 0 ? (
                            <p>No products found.</p>
                        ) : (
                            <div className="product-grid">

                                {products.map((product) => (

                                    <div key={product.id} className="product-card" onClick={() => navigate(`/product/${product.id}`)}>

                                        <div className="product-image">

                                            {product.imageUrl ? (
                                                <img src={product.imageUrl} alt={product.name} loading="lazy" onError={(e) => {
                                                    e.target.style.display = "none";
                                                    e.target.nextSibling.style.display = "flex";
                                                }} />
                                            ) : null}

                                            <span className="product-fallback" style={{ display: product.imageUrl ? "none" : "flex" }}>
                                                📦
                                            </span>

                                        </div>

                                        <h3>{product.name}</h3>

                                        <p>{product.description.length > 100 ? product.description.substring(0, 100) + "..." : product.description}</p>

                                        <strong>₹{product.price}</strong>

                                        <p>Stock: {product.stock}</p>

                                        <p>Category: {product.category}</p>

                                        <button onClick={(e) => { e.stopPropagation(); navigate(`/product/${product.id}`); }}>
                                            View Product
                                        </button>

                                    </div>

                                ))}

                            </div>
                        )}

                        <div className="pagination">
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

export default Products;