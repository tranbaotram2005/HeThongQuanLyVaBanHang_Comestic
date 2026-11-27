$(document).ready(function () {

    $("#sendOtpBtn").click(function () {

        var email = $("#Email").val();

        if (!email) {
            alert("Nhập email trước");
            return;
        }

        $.post('/Account/SendOtp', { email: email }, function (res) {
            alert(res.message);
        });

    });

});