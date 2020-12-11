$(function () {
    var lourl = location.pathname.toLowerCase();
    var url = (lourl == '/m-weixin' || lourl == '/m-wap') ? ('/' + areaName + '/CustomerServices/PlatCustomerServices') : "";//首页调用平台客服
    if (url == "" && lourl.indexOf("/product/detail") != -1 && $("#shopId").val() != null && $("#shopId").val() != "") {//商品详细页调用商家客服
        url = '/' + areaName + '/CustomerServices/ShopCustomerServices?shopId=' + $("#shopId").val();
    }
    if (url.length > 0) {
		$.ajax({
			url: url,
			async: false,
			success: function (data) {		
				$("#_Aside_CustomServices").html(data);
				
			}
		});
	}
    $("#_Aside_CustomServices span.lb-1 i").after("<em style='color:#eee; display:inline-display'>客服</em>");


    $("#two-service").toggle(function() {
        var qq_len = $(".s-line>a>span").length;
        if (qq_len > 0) {
            $(".s-line").css("visibility", "visible");
            $(".line-btn").css("background-color", "rgba(1,21,25,.5)")
            $(".line-btn em").css("display", "none");
            $(".line-btn i").css("margin-top", "7px")
        }
    }, function() {
        $(".s-line").css("visibility", "hidden");
        $(".line-btn").css("background-color", "rgba(1,21,25,.24)")
        $(".line-btn em").css("display", "block");
        $(".line-btn i").css("margin-top", "0")
    });
});