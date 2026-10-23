import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import { useToast } from "../Context/ToastContext.jsx";
import { useWishlist } from "../Context/WishlistContext.jsx";
import { getWishlist, removeFromWishlist } from "../Services/WishlistService";
import RecommendedProducts from "../Components/RecommendedProducts.jsx";
import "./Wishlist.css";

function Wishlist() {
    const navigate = useNavigate();
    const showToast = useToast();
    const { forgetProduct } = useWishlist();

    const [items, setItems] = useState([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(false);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);

    const loadItems = async (pageNumber) => {
        try {
            const data = await getWishlist(pageNumber, 10);
            setItems((prev) => pageNumber === 1 ? data.items : [...prev, ...data.items]);
            setHasMore(data.hasMore);
            setTotal(data.totalItems);
            setPage(pageNumber);
        } catch (error) {
            console.error("Error loading wishlist:", error);
        }
    };

    useEffect(() => {
        loadItems(1).finally(() => setLoading(false));
    }, []);

    const handleLoadMore = async () => {
        setLoadingMore(true);
        await loadItems(page + 1);
        setLoadingMore(false);
    };

    const handleRemove = async (productId) => {
        try {
            await removeFromWishlist(productId);
            setItems((prev) => prev.filter((item) => item.productId !== productId));
            setTotal((prev) => prev - 1);
            forgetProduct(productId);
        } catch (error) {
            showToast(error.response?.data?.message || "Could not remove item.", "error");
        }
    };

    return (
        <>
            <Header />
            <div className="page-layout">
                <Sidebar />
                <main className="main-content">
                    <div className="wishlist-page-header">
                        <h2>My Wishlist</h2>
                        {total > 0 && <span className="wishlist-count">{total} item{total !== 1 ? "s" : ""}</span>}
                    </div>

                    {loading ? (
                        <p>Loading...</p>
                    ) : items.length === 0 ? (
                        <div className="wishlist-empty">
                            <div className="wishlist-empty-icon">♡</div>
                            <p>Your wishlist is empty.</p>
                            <button className="load-more-button" onClick={() => navigate("/products")}>Browse Products</button>
                        </div>
                    ) : (
                        <>
                            <div className="search-results-grid">
                                {items.map((item) => (
                                    <div key={item.productId} className={`search-result-card ${item.stock > 0 ? "" : "is-sold-out"}`} onClick={() => navigate(`/product/${item.productId}`)}>
                                        <div className="search-result-image">
                                            {item.images?.length > 0 ? (
                                                <img
                                                    src={`${import.meta.env.VITE_API_URL.replace("/api", "")}${item.images[0].imageUrl}`}
                                                    alt={item.name}
                                                    loading="lazy"
                                                />
                                            ) : (
                                                <span className="product-fallback">📦</span>
                                            )}
                                        </div>
                                        <h3>{item.name}</h3>
                                        <strong>₹{item.price}</strong>
                                        <p className={`wishlist-stock ${item.stock > 0 ? "in-stock" : "out-of-stock"}`}>{item.stock > 0 ? "In stock" : "Out of stock"}</p>
                                        <button className="wishlist-remove-btn" onClick={(e) => { e.stopPropagation(); handleRemove(item.productId); }}>Remove</button>
                                    </div>
                                ))}
                            </div>

                            {hasMore && (
                                <div className="load-more-container">
                                    <button className="load-more-button" onClick={handleLoadMore} disabled={loadingMore}>
                                        {loadingMore ? "Loading..." : "Load More"}
                                    </button>
                                </div>
                            )}

                            {items.length > 0 && (<RecommendedProducts productId={items[0].productId} title="Because you saved items" />)}
                        </>
                    )}
                </main>
            </div>
        </>
    );
}

export default Wishlist;