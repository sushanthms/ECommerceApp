import { useEffect, useState } from "react";
import { getAllReviews, deleteReview } from "../Services/ReviewService.jsx";
import Header from "../Components/Header.jsx";
import Sidebar from "../Components/Sidebar.jsx";
import "./AdminReviews.css";

const AdminReviews = () => {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const [reviews, setReviews] = useState([]);
    const [productFilter, setProductFilter] = useState("all");
    const [userFilter, setUserFilter] = useState("all");
    const [ratingFilter, setRatingFilter] = useState("all");

    useEffect(() => {
        document.documentElement.setAttribute("data-theme", darkMode ? "dark" : "light");
    }, [darkMode]);

    useEffect(() => {
        const loadReviews = async () => {
            try {
                const data = await getAllReviews();
                setReviews(data);
            } catch (error) {
                console.error("Error loading reviews:", error);
            }
        };

        loadReviews();
    }, []);

    const handleDeleteReview = async (id) => {
    const confirmDelete = window.confirm("Are you sure you want to delete this review?");

    if (!confirmDelete)
        return;

    try {
        await deleteReview(id);
        setReviews(reviews.filter(review => review.id !== id));
    } catch (error) {
        console.error("Error deleting review:", error);
    }
};

const productOptions = [...new Set(reviews.map(review => review.productName))];

const userOptions = reviews.filter(
    (review, index, self) =>
        index === self.findIndex(r => r.userId === review.userId)
);

const filteredReviews = reviews.filter((review) => {

    const productMatches = productFilter === "all" || review.productName === productFilter;

    const userMatches = userFilter === "all" || review.userId === Number(userFilter);

    const ratingMatches = ratingFilter === "all" || review.rating === Number(ratingFilter);

    return productMatches && userMatches && ratingMatches;
});

    return (
    <>
        <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="Admin" />

        <div className="page-layout">

            <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} role="Admin" />

            <main className="main-content">

                <div className="admin-reviews">
                    <h1>Manage Reviews</h1>
                    <p>Total Reviews: {reviews.length}</p>

                    <div className="review-filters">
                        <select value={productFilter} onChange={(e) => setProductFilter(e.target.value)}>
                            <option value="all">All Products</option>
                            {productOptions.map((product) => (
                                <option key={product} value={product}>{product}</option>))}
                        </select>
                        
                        <select value={userFilter} onChange={(e) => setUserFilter(e.target.value)}>
                            <option value="all">All Users</option>
                            {userOptions.map((user) => (// userFilter has inside value userId option value={user.userId}
                                <option key={user.userId} value={user.userId}>{user.userName} (User Id {user.userId})</option>
                                ))}
                        </select>
                        
                        <select value={ratingFilter} onChange={(e) => setRatingFilter(e.target.value)}>
                            <option value="all">All Ratings</option>
                            <option value="1">★ 1 Star</option>
                            <option value="2">★ 2 Stars</option>
                            <option value="3">★ 3 Stars</option>
                            <option value="4">★ 4 Stars</option>
                            <option value="5">★ 5 Stars</option>
                        </select>
                    </div>

                    {filteredReviews.length === 0 ? (
                        <p className="no-reviews">{reviews.length === 0 ? "No reviews found." : "No matching reviews found."}</p>
                    ) : (
                        filteredReviews.map((review) => (
                            <div className="review" key={review.id}>
                                <h3>{review.productName}</h3>
                                <div>{"★".repeat(review.rating)}{"☆".repeat(5 - review.rating)}</div>
                                <h3>{review.userName} - User Id {review.userId}</h3>
                                <p>{review.comment}</p>
                                <small>{new Date(review.createdAt).toLocaleDateString()}</small>
                                <button onClick={() => handleDeleteReview(review.id)}>Delete</button>
                            </div>
                        ))
                    )}
                </div>

            </main>

        </div>
    </>
);
};

export default AdminReviews;