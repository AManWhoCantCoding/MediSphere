(function () {
    // Thêm CSS bắt buộc để hiển thị tab ngày/giờ dựa trên class .in của thư viện cũ
    var style = document.createElement('style');
    style.innerHTML = 
        '.bootstrap-datetimepicker-widget .collapse.in { display: block !important; height: auto !important; } ' +
        '.bootstrap-datetimepicker-widget .collapse:not(.in) { display: none !important; }';
    document.head.appendChild(style);

    // Sử dụng kỹ thuật Capture Phase của DOM để can thiệp TRƯỚC KHI sự kiện click lọt vào thư viện Datetimepicker
    document.addEventListener('click', function (e) {
        // Kiểm tra xem người dùng có đang click vào nút chuyển đổi Ngày/Giờ hay không
        if (e.target.closest('.bootstrap-datetimepicker-widget [data-action="togglePicker"]')) {
            
            // Tạm thời "vô hiệu hóa" Bootstrap 5 Collapse bằng cách xóa hàm $.fn.collapse
            // Việc này sẽ ép thư viện Eonasdan Datetimepicker sử dụng cơ chế dự phòng (fallback) nội bộ của nó
            // (tức là chỉ dùng jQuery addClass/removeClass('in') thay vì gọi animation của Bootstrap)
            if (window.jQuery && window.jQuery.fn && window.jQuery.fn.collapse) {
                var tempCollapse = window.jQuery.fn.collapse;
                window.jQuery.fn.collapse = undefined;
                
                // Phục hồi lại Bootstrap 5 Collapse ngay sau khi logic của thư viện xử lý xong
                setTimeout(function () {
                    window.jQuery.fn.collapse = tempCollapse;
                }, 0);
            }
        }
    }, true); // true = Capture phase
})();
