// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minification.

// Enhance navigation with active state
document.addEventListener('DOMContentLoaded', function() {
    // Set active nav link based on current page
    const currentLocation = window.location.pathname;
    const navLinks = document.querySelectorAll('.navbar-nav a.nav-link');
    
    navLinks.forEach(link => {
        link.classList.remove('active');
        const href = link.getAttribute('href');
        if (href === currentLocation || (href === '/' && currentLocation === '/')) {
            link.classList.add('active');
        }
    });

    // Add smooth scroll behavior
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            const href = this.getAttribute('href');
            if (href !== '#') {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });

    // Format currency on product prices
    formatCurrency();
});

function formatCurrency() {
    const prices = document.querySelectorAll('[data-price]');
    prices.forEach(element => {
        const value = parseInt(element.getAttribute('data-price'));
        if (value) {
            element.textContent = value.toLocaleString('vi-VN') + ' đ';
        }
    });
}

// Add to cart functionality
function addToCart(productId) {
    if (typeof isAuthenticated !== 'undefined' && !isAuthenticated) {
        // User is not authenticated, show login prompt
        const login = confirm('Bạn cần đăng nhập để thêm sản phẩm vào giỏ hàng. Bạn có muốn đăng nhập ngay bây giờ?');
        if (login) {
            window.location.href = '/Account/Login';
        }
        return;
    }
    
    console.log('Product added to cart: ' + productId);
    alert('Sản phẩm đã được thêm vào giỏ hàng!');
}

// Wishlist functionality (placeholder)
function addToWishlist(productId) {
    console.log('Product added to wishlist: ' + productId);
    alert('Sản phẩm đã được thêm vào danh sách yêu thích!');
}
