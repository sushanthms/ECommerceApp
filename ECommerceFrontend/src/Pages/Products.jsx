import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { getProducts, searchProducts, getCategories } from "../Services/ProductService";
import WishlistButton from "../Components/WishListButton.jsx";

import "./Products.css";

function Products() {

    const navigate = useNavigate();

    const [searchParams] = useSearchParams();

    const categoryFromUrl = searchParams.get("category") || "";
    const searchFromUrl = searchParams.get("search") || "";

    const [products, setProducts] = useState([]);
    const [search, setSearch] = useState("");
    const [category, setCategory] = useState("");
    const [categories, setCategories] = useState([]);

    const [page, setPage] = useState(1);// Which page is currently being displayed? when next is clicked setPage(page+1)
    const [pageSize] = useState(20);
    const [totalPages, setTotalPages] = useState(1);

    useEffect(() => {
        setCategory(categoryFromUrl);
        setSearch(searchFromUrl);
        setPage(1);// whenever url changes, updates the category and goes to page 1
    }, [categoryFromUrl, searchFromUrl]);

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
            <Header />
            <div className="page-layout">
                <Sidebar />

                <main className="main-content">

                    <div className="welcome-section">
                        <h2>Products</h2>
                        <p>Browse all available products.</p>
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

                                            {product.images?.length > 0 ? (
                                                <img
                                                    src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${product.images[0].imageUrl}`}
                                                    alt={product.name}
                                                    loading="lazy"
                                                />
                                            ) : (
                                                <span className="product-fallback">📦</span>
                                            )}
                                            <WishlistButton productId={product.id} onLoginRequired={() => openLoginPopup()} />
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