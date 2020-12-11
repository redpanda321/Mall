$(function () { 
    $.ajax('/home/coupon', {
        type: 'post',
        dataType: 'json',
        cache: false,
        async: false,
        success: function (data) {
            if (data!=null) {
                $("#memberIntegral").html(data.memberIntegral);
                if (data.baseCoupons != null) {
                    $("#mycouponcount").html(data.baseCoupons.length);
                }
                curshopid = data.shopId;
            }
        }
    });
});