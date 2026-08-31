import "./Sidebar.css"

function Sidebar({ menuOpen, setMenuOpen }) {
    return (
        <aside className={menuOpen ? "sidebar open" : "sidebar close"}>
            <div className="sidebar-header">
                <button className="sidebar-toggle" onClick={() => setMenuOpen(prev => !prev)}>☰</button>
                <h2 className="sidebar-title">Dashboard</h2>
            </div>

            <ul className="sidebar-list">
                <li><a href="/Home" className="active">Overview</a></li>
                <li><a href="#">My Orders</a></li>
                <li><a href="#">Cart</a></li>
                <li><a href="#">Offers</a></li>
                <li><a href="#">My Profile</a></li>
                <li><a href="#">Settings</a></li>
            </ul>
        </aside>
    )
}

export default Sidebar;