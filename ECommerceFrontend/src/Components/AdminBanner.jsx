import { useEffect, useState } from "react";

import {
    getBanners,
    addBanner,
    updateBanner,
    deleteBanner
} from "../Services/BannerService.jsx";

import "./AdminBanner.css";

function AdminBanner() {

    const [banners, setBanners] = useState([]);

    const [showBannerForm, setShowBannerForm] = useState(false);

    const [editingBannerId, setEditingBannerId] = useState(null);

    const [bannerTitle, setBannerTitle] = useState("");
    const [bannerDescription, setBannerDescription] = useState("");
    const [bannerButtonText, setBannerButtonText] = useState("");
    const [bannerImageUrl, setBannerImageUrl] = useState("");
    const [bannerLink, setBannerLink] = useState("");

    const [bannerError, setBannerError] = useState("");
    const [bannerSuccess, setBannerSuccess] = useState("");

    const loadBanners = async () => {
        try {
            const data = await getBanners();
            setBanners(data);
        } catch (error) {
            console.error("Error loading banners:", error);
        }
    };

    useEffect(() => {
        loadBanners();
    }, []);

    const resetBannerForm = () => {
        setEditingBannerId(null);
        setBannerTitle("");
        setBannerDescription("");
        setBannerButtonText("");
        setBannerImageUrl("");
        setBannerLink("");
        setBannerError("");
        setBannerSuccess("");
    };

    const handleSubmitBanner = async (e) => {
        e.preventDefault();

        setBannerError("");
        setBannerSuccess("");

        if (!bannerTitle || !bannerDescription || !bannerButtonText || !bannerImageUrl || !bannerLink) 
            {
                setBannerError("All banner fields are required.");
                return;
            }

        const bannerData = {
            title: bannerTitle,
            description: bannerDescription,
            buttonText: bannerButtonText,
            imageUrl: bannerImageUrl,
            link: bannerLink
        };

        try {
            if (editingBannerId) {
                await updateBanner(editingBannerId, bannerData);
                setBannerSuccess("Banner updated successfully.");
            } else {
                await addBanner(bannerData);
                setBannerSuccess("Banner added successfully.");
            }

            await loadBanners();

            resetBannerForm();
            setShowBannerForm(false);

        } catch (error) {
            console.error("Save banner error:", error);

            setBannerError(error.response?.data?.message || "Failed to save banner.");
        }
    };

    const handleDeleteBanner = async (id) => {
        const confirmDelete = window.confirm("Are you sure you want to delete this banner?");

        if (!confirmDelete) {
            return;
        }

        try {
            await deleteBanner(id);
            setBannerSuccess("Banner deleted successfully.");
            await loadBanners();
        } catch (error) {
            console.error("Delete banner error:", error);
            setBannerError(error.response?.data?.message || "Failed to delete banner.");
        }
    };

    const handleEditBanner = (banner) => {
        setEditingBannerId(banner.id);
        setBannerTitle(banner.title);
        setBannerDescription(banner.description);
        setBannerButtonText(banner.buttonText);
        setBannerImageUrl(banner.imageUrl);
        setBannerLink(banner.link);

        setBannerError("");
        setBannerSuccess("");
        setShowBannerForm(true);
    };

    return (
        <div className="banner-management-section">

            <div className="section-heading">
                <h2>Manage Banners</h2>

                {!showBannerForm && (// showbannerform is false now, it makes it true and displays add banner button.
                    <button className="add-product-btn" onClick={() => {resetBannerForm(); setShowBannerForm(true);}}>+ Add Banner</button>
                )}
            </div>

            {showBannerForm && (
                <div className="modal-overlay">
                    <form className="product-form" onSubmit={handleSubmitBanner}>
                        <h3>{editingBannerId ? "Edit Banner": "New Banner"}</h3>

                        <div className="form-item">
                            <label>Title</label>
                            <input type="text" value={bannerTitle} onChange={(e) => setBannerTitle(e.target.value)}placeholder="Banner title"/>
                        </div>

                        <div className="form-item">
                            <label>Description</label>
                            <input type="text" value={bannerDescription} onChange={(e) => setBannerDescription(e.target.value)} placeholder="Banner description"/>
                        </div>

                        <div className="form-item">
                            <label>Button Text</label>
                            <input type="text" value={bannerButtonText} onChange={(e) => setBannerButtonText(e.target.value)}placeholder="Shop Now"/>
                        </div>

                        <div className="form-item">
                            <label>Image URL</label>
                            <input type="text" value={bannerImageUrl} onChange={(e) =>setBannerImageUrl(e.target.value)} placeholder="https://..."/>
                        </div>

                        <div className="form-item">
                            <label>Link</label>
                            <input type="text" value={bannerLink} onChange={(e) => setBannerLink(e.target.value)} placeholder="/products?category=Furniture"/>
                        </div>

                        {bannerError && (
                            <p className="field-error">{bannerError}</p>
                        )}

                        <div className="form-actions">
                            <button type="submit">{editingBannerId ? "Save Changes": "Add Banner"}</button>

                            <button type="button" className="cancel-btn" onClick={() => {resetBannerForm(); setShowBannerForm(false);}}>Cancel</button>
                        </div>
                    </form>
                </div>
            )}

            {bannerSuccess && (
                <p className="success-message">{bannerSuccess}</p>
            )}

            <div className="banner-list">{banners.length === 0 ? (<p>No banners available.</p>
            ) : (
                banners.map((banner) => (
                <div key={banner.id} className="admin-banner-card">

                <div
                    className="admin-banner-preview"
                    style={{ backgroundImage: `url(${banner.imageUrl})` }}
                >
                    <div className="banner-actions">
                        <button onClick={() => handleEditBanner(banner)}>Edit</button>
                        <button onClick={() => handleDeleteBanner(banner.id)}>Delete</button>
                    </div>

                    <div className="banner-overlay">
                        <h2>{banner.title}</h2>
                        <p>{banner.description}</p>
                        <button className="banner-cta-btn">{banner.buttonText}</button>
                    </div>
                </div>

            </div>
        ))
    )}
</div>

        </div>
    );
}

export default AdminBanner;