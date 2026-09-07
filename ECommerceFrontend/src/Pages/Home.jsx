import { useNavigate } from "react-router-dom";
import "./Home.css";

function Home() {

const navigate = useNavigate();

return (
    <div className="public-home">

        <header className="public-header">

            <h1>Web Page</h1>

            <nav>
                <a href="#">Home</a>
                <a href="/products">Products</a>
                <a href="#">Community</a>
            </nav>

            <button className="public-login-btn" onClick={() => navigate("/login")}>Login</button>

        </header>

        <main className="public-main">

            <section className="hero-section">

                <h1>Welcome to Web Page</h1>
                <p>Explore our products, learn new things,and join our community.</p>
                <button className="public-login-btn" onClick={() => navigate("/login")}>Login to Continue</button>

            </section>

        </main>

    </div>
);

}

export default Home;
