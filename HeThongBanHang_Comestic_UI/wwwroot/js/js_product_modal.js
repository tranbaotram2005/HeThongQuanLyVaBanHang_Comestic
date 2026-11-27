(function ($) {
    "use strict";

    let selectedVariantId = null;
    let selectedOptions = {};

    // ========================================
    // MỞ MODAL DETAIL
    // ========================================
    $(document).on("click", ".btn-detail", function () {
        var id = $(this).data("id");

        $.get("/Product/DetailPopup/" + id)
            .done(function (data) {
                selectedVariantId = null;
                selectedOptions = {};

                $("#modalContent").html(data);
                $("#productModal").modal("show");
            })
            .fail(function (xhr) {
                alert("Không tải được chi tiết sản phẩm (HTTP " + xhr.status + ").");
            });
    });

    // ========================================
    // CHỌN OPTION
    // ========================================
    $(document).on("click", "#modalContent .option-btn", function () {
        let group = $(this).data("group");
        let option = $(this).data("option");

        $('#modalContent .option-btn[data-group="' + group + '"]').removeClass("active");
        $(this).addClass("active");

        selectedOptions[group] = option;

        let totalGroups = (typeof window.totalGroups !== "undefined") ? window.totalGroups : 1;
        if (Object.keys(selectedOptions).length < totalGroups) {
            return;
        }

        let matchedVariant = (window.productVariants || []).find(function (v) {
            let variantOptions = v.options || {};
            return Object.keys(selectedOptions).every(function (key) {
                return variantOptions[key] === selectedOptions[key];
            });
        });

        if (!matchedVariant) {
            console.log("Không tìm thấy variant phù hợp");
            return;
        }

        selectedVariantId = matchedVariant.id;
        $("#priceDisplay").text(Number(matchedVariant.price).toLocaleString("vi-VN"));
        $("#productImage").attr("src", matchedVariant.image);
        $("#addToCartBtn").attr("data-variant-id", matchedVariant.id);
    });

    // ========================================
    // ADD TO CART
    // ========================================
    $(document).on("click", "#modalContent #addToCartBtn", function () {
        let variantId = $(this).attr("data-variant-id");

        // Chưa đăng nhập -> chuyển thẳng sang màn hình đăng nhập
        if (!window.isAuthenticated) {
            let returnUrl = variantId
                ? "/Cart/AddToCart?variantId=" + variantId + "&quantity=1"
                : (window.location.pathname + window.location.search);

            window.location.href = "/Account/Login?ReturnUrl=" + encodeURIComponent(returnUrl);
            return;
        }

        if (!variantId) {
            alert("Vui lòng chọn phân loại");
            return;
        }

        window.location.href = "/Cart/AddToCart?variantId=" + variantId + "&quantity=1";
    });

    // ========================================
    // RESET MODAL
    // ========================================
    $(document).on('hidden.bs.modal', '#productModal', function () {
        selectedVariantId = null;
        selectedOptions = {};
        $("#modalContent").html("");
    });

    // ========================================
    // AUTO SELECT DEFAULT VARIANT
    // ========================================
    $(document).on('shown.bs.modal', '#productModal', function () {
        if (!window.productVariants) return;

        let defaultVariant = null;
        for (let i = 0; i < window.productVariants.length; i++) {
            if (window.productVariants[i].isDefault === true) {
                defaultVariant = window.productVariants[i];
                break;
            }
        }

        if (!defaultVariant) return;

        for (let group in defaultVariant.options) {
            let value = defaultVariant.options[group];
            let btn = $('#modalContent .option-btn[data-group="' + group + '"][data-option="' + value + '"]');
            if (btn.length > 0) {
                btn.trigger("click");
            }
        }
    });

})(jQuery);