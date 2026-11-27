$(function () {

    $(document).on("click", ".btn-plus", function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        updateQty($(this).data("id"), 1);
    });

    $(document).on("click", ".btn-minus", function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();
        updateQty($(this).data("id"), -1);
    });

});

function updateQty(id, quantity) {

    $.ajax({
        url: '/Cart/PlusMinus',
        type: 'POST',
        data: {
            id: id,
            quantity: quantity
        },

        success: function (res) {

            if (!res.success) {
                alert(res.message);
                return;
            }

            // update badge trực tiếp
            $(".cart-count").text(res.cartCount);

            if (!res.newQuantity) {
                $("#row-" + id).remove();
                recalcCart();
                return;
            }

            $("#qty-" + id).val(res.newQuantity);

            let price = parseFloat($("#price-" + id).data("price"));
            let total = price * res.newQuantity;

            $("#total-" + id).text(
                total.toLocaleString("vi-VN")
            );

            recalcCart();
        }
    });
}

function recalcCart() {

    let subtotal = 0;

    $("[id^='total-']").each(function () {

        let value = $(this).text()
            .replace(/\./g, '');

        subtotal += parseFloat(value) || 0;
    });

    $("#cart-subtotal").text(
        subtotal.toLocaleString("vi-VN")
    );

    $("#cart-total").text(
        subtotal.toLocaleString("vi-VN")
    );
}