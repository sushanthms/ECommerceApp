import { useState } from "react";
import { useNavigate } from "react-router-dom";

import Header from "../Header.jsx";
import Sidebar from "../Sidebar.jsx";

import "./Community.css";

function Community() {

    const [menuOpen, setMenuOpen] = useState(true);
    const [darkMode, setDarkMode] = useState(localStorage.getItem("theme") === "dark");

    const navigate = useNavigate();

    return (
        <>
            <Header menuOpen={menuOpen} setMenuOpen={setMenuOpen} darkMode={darkMode} setDarkMode={setDarkMode} role="User" onCartClick={() => navigate("/cart")} />

            <div className="page-layout">

                <Sidebar menuOpen={menuOpen} setMenuOpen={setMenuOpen} onCartClick={() => navigate("/cart")} role="User" />

                <main className="community-page">

                    <section className="community-hero">
                        <h1>Community</h1>

                        <p>
                            Share ideas, discover inspiration, and connect with
                            other homecraft lovers.
                        </p>

                        <button>Share Your Creation</button>
                    </section>


                    <section className="community-section">

                        <div className="community-heading">
                            <h2>Ideas & Inspiration</h2>

                            <p>
                                Discover creative ideas from our community.
                            </p>
                        </div>


                        <div className="community-grid">

                            <article className="community-card">

                                <div className="community-image">
                                    🏠
                                </div>

                                <div className="community-content">

                                    <span>Home Decor</span>

                                    <h3>
                                        Make Your Living Room Beautiful
                                    </h3>

                                    <p>
                                        Simple homecraft ideas that can give
                                        your living room a fresh and beautiful
                                        look.
                                    </p>

                                    <div className="community-footer">
                                        <span>❤️ 24 Likes</span>
                                        <span>💬 8 Comments</span>
                                    </div>

                                </div>

                            </article>


                            <article className="community-card">

                                <div className="community-image">
                                    🎨
                                </div>

                                <div className="community-content">

                                    <span>DIY Ideas</span>

                                    <h3>
                                        Creative DIY Decoration Ideas
                                    </h3>

                                    <p>
                                        Try these simple handmade decoration
                                        ideas for your home.
                                    </p>

                                    <div className="community-footer">
                                        <span>❤️ 38 Likes</span>
                                        <span>💬 12 Comments</span>
                                    </div>

                                </div>

                            </article>


                            <article className="community-card">

                                <div className="community-image">
                                    🪴
                                </div>

                                <div className="community-content">

                                    <span>Inspiration</span>

                                    <h3>
                                        Bring Nature Into Your Home
                                    </h3>

                                    <p>
                                        Explore creative ways to use plants
                                        and handmade products together.
                                    </p>

                                    <div className="community-footer">
                                        <span>❤️ 31 Likes</span>
                                        <span>💬 10 Comments</span>
                                    </div>

                                </div>

                            </article>

                        </div>

                    </section>


                    <section className="share-section">

                        <div>
                            <h2>Have an idea to share?</h2>

                            <p>
                                Share your homecraft ideas and inspire other
                                members of the community.
                            </p>
                        </div>

                        <button>
                            Share Your Creation
                        </button>

                    </section>

                </main>

            </div>
        </>
    );
}

export default Community;