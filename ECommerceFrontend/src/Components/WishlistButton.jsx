import { useAuth } from "../Context/AuthContext.jsx";
import { useWishlist } from "../Context/WishlistContext.jsx";

function WishlistButton({ productId, onLoginRequired }) {
    const { isWishlisted, toggleWishlist, isUser } = useWishlist();
    const { isLoggedIn } = useAuth();

    const saved = isWishlisted(productId);

    const handleClick = (e) => {
        e.stopPropagation();// cards are clickable, so it doesnot open the product page

        if (!isLoggedIn) {
            onLoginRequired?.();
            return;
        }
        if (!isUser) return;// admins have no wishlist

        toggleWishlist(productId);
    };

    return (
        <button type="button" className="wishlist-btn" onClick={handleClick} aria-label={saved ? "Remove from wishlist" : "Add to wishlist"}>{saved ? "♥" : "♡"}</button>
    );
}

export default WishlistButton;