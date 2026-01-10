// Favorite (wishlist) feature: toggle product favorite on detail page
(function () {
    if (!window.jQuery) {
        // jQuery is required for this script in current project structure
        return;
    }

    function setState($el, isFav) {
        $el.attr('data-is-favorited', isFav ? 'true' : 'false');
        var $icon = $el.find('i');
        $icon.removeClass('bx-heart bxs-heart').addClass(isFav ? 'bxs-heart' : 'bx-heart');
        $el.toggleClass('is-favorited', !!isFav);
    }

    function normalizeBool(v) {
        return String(v).toLowerCase() === 'true';
    }

    $(function () {
        $('.js-favorite-toggle').each(function () {
            var $el = $(this);
            setState($el, normalizeBool($el.attr('data-is-favorited')));
        });
    });

    $(document).on('click', '.js-favorite-toggle', function (e) {
        e.preventDefault();
        e.stopPropagation();

        var $el = $(this);
        var idProduct = $el.data('product-id');

        // NOTE: This project uses a custom jQuery build without $.ajax.
        // Use fetch() with form-urlencoded body so ASP.NET MVC model binder can read it.
        fetch('/Favorite/Toggle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
            },
            body: new URLSearchParams({ idProduct: idProduct }).toString(),
            credentials: 'same-origin'
        })
            .then(function (r) {
                return r.json().catch(function () {
                    // Not JSON
                    throw new Error('Server response is not JSON');
                });
            })
            .then(function (res) {
                if (res && res.requiresLogin) {
                    alert('Bạn cần đăng nhập để sử dụng chức năng này');
                    window.location.href = '/login';
                    return;
                }

                if (res && res.error) {
                    alert(res.error);
                    return;
                }

                if (res && typeof res.isFavorited !== 'undefined') {
                    setState($el, !!res.isFavorited);
                }
            })
            .catch(function (err) {
                alert((err && err.message) ? err.message : 'Không thể thêm vào yêu thích. Vui lòng thử lại.');
            });
    });
})();


