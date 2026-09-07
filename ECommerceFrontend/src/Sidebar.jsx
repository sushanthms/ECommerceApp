import "./Sidebar.css";

function Sidebar({ menuOpen, setMenuOpen, onCartClick, role }) {

    return (
        <aside className={menuOpen ? "sidebar open" : "sidebar close"}>

            <div className="sidebar-header">
                <button className="sidebar-toggle" onClick={() => setMenuOpen(prev => !prev)}>☰</button>

                <h2 className="sidebar-title">{role === "Admin" ? "Dashboard" : "Dashboard"}</h2>
            </div>

            <ul className="sidebar-list">

                {role === "Admin" ? (
                    <>
                        <li><a href="/admin" className="active">Home</a></li>
                        <li><a href="/admin/orders">Manage Orders</a></li>
                        <li><a href="/admin/users">Manage Users</a></li>
                        <li><a href="#">Manage Products</a></li>
                        <li><a href="#">Settings</a></li>
                    </>
                ) : (
                    <>
                        <li><a href="/Home" className="active">Overview</a></li>
                        <li><a href="#">My Orders</a></li>
                        <li><a href="#" onClick={onCartClick}>My Cart</a></li>
                        <li><a href="#">Offers</a></li>
                        <li><a href="#">My Profile</a></li>
                        <li><a href="#">Settings</a></li>
                    </>
                )}

            </ul>

        </aside>
    );
}

export default Sidebar;