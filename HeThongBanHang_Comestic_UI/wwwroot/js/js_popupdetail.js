//$(document).ready(function () {

//    let selectedOptions = {};
//    let selectedVariant = null;

//    // default variant
//    selectedVariant =
//        window.productVariants.find(v => v.isDefault)
//        || window.productVariants[0];

//    if (selectedVariant) {
//        updateUI(selectedVariant);

//        Object.keys(selectedVariant.options).forEach(group => {
//            selectedOptions[group] = selectedVariant.options[group];

//            $(`.option-btn[data-group="${group}"][data-option="${selectedVariant.options[group]}"]`)
//                .addClass("active");
//        });
//    }

//    // click chọn option
//    $(".option-btn").click(function () {

//        const group = $(this).data("group");
//        const option = $(this).data("option");

//        selectedOptions[group] = option;

//        $(`.option-btn[data-group="${group}"]`)
//            .removeClass("active");

//        $(this)
//            .addClass("active");

//        if (Object.keys(selectedOptions).length === window.totalGroups) {

//            const matched = window.productVariants.find(v => {

//                return Object.keys(v.options).every(g =>
//                    v.options[g] === selectedOptions[g]
//                );

//            });

//            if (matched) {
//                selectedVariant = matched;
//                updateUI(matched);
//            }
//        }
//    });

//    // add cart
//    $("#addToCartBtn").click(function () {

//        if (!selectedVariant) {
//            alert("Vui lòng chọn biến thể");
//            return;
//        }

//        window.location.href =
//            `/Cart/AddToCart?variantUid=${selectedVariant.uid}&quantity=1`;
//    });

//    function updateUI(variant) {

//        $("#priceDisplay").text(
//            Number(variant.price).toLocaleString("vi-VN")
//        );

//        $("#productImage").attr("src", variant.image);
//    }

//});