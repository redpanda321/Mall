

var page = 1;
var curentId = 0;
$(window).scroll(function () {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(document).height();
    var windowHeight = $(this).height();

    if (scrollTop + windowHeight >= scrollHeight - 30) {
        loadProducts(++page);
    }
});
$(function () {
    loadProducts(page);
    $('#btnOK').click(backSubmit);
})

function loadProducts(page) {
    var areaname = areaName;
    var url = 'List';
	if(page==1)
		var loading = showLoading();
    $.post(url, { pageNo: page, pageSize: 10 }, function (result) {
        var html = '';
        if (result.success) {
            var orderid = '';
            var htmlTitle = '', htmlDetail = '', htmlFooter = '';
            $.each(result.data, function (i, record) {
                html += '<li>';
                if (record.ShopBranchId) {
                    html += '<a href="/' + areaName + '/shopbranch/Index/' + record.ShopBranchId + '">';
                    html += '<h6> ' + record.ShopBranchName;
                } else {
                    if (record.Vshopid != null && record.Vshopid != 0) {
                        html += '<a href="../Vshop/Detail/' + record.Vshopid + '">';
                    } else {
                        html += '<a href="../Vshop">';
                    }
                    html += '<h6> ' + record.ShopName;
                }
                html += '<span class="pull-right">' + record.RefundStatus + '</span>';
                html += '</h6></a>';
                if (record.RefundMode == '1') {//整笔订单退
                    $.each(record.OrderItems, function (idx, detail) {
                        html += '<div class="order-goods clearfix">';
                        html += '<a href="RefundDetail?id=' + record.Id + '">';
                        html += '<div class="top">';
                        html += '<img src="' + detail.ThumbnailsUrl + '" />'; //图片后台处理
                        html += '<p>' + detail.ProductName + '</p>';
                        html += '<p class="p-r"><span class="gray">' + (detail.SkuText?detail.SkuText:'')+ '</span></p>';
                        html += '</div></a></div>';
                    });
                }
                else {
                    html += '<div class="order-goods clearfix">';
                    html += '<a href="RefundDetail?id=' + record.Id + '">';
                    html += '<div class="top">';
                    html += '<img src="' + record.Img + '" />';
                    html += '<p>' + record.ProductName + '</p>';
                    html += '<p class="p-r"><span class="gray">' + (record.OrderItems ? record.OrderItems[0].SkuText ? record.OrderItems[0].SkuText : '' : '') + '</span></p>';
                    html += '</div></a></div>';
                }
                html += '<p class="order-text clearfix"><a > <span>交易金额<i>¥ ' + record.OrderTotal + '</i></span><span class="light">退款金额<i>¥ ' + record.Amount + '</i></span></a></p>';
				if (record.SellerAuditStatus == 2 && record.RefundMode != '1') {
				    html += '<p class="refund-btn"><a class="post-back" dataShop="' + record.ShopId + '" datasbid="' + record.ShopBranchId + '" dataid="' + record.Id + '">快递寄回</a></p>';
				}
				
                html += '</li>';
            });

            $('#productList').append(html);
			
            $(".post-back").click(function () {
                var shopId = $(this).attr("dataShop");
                sbid = $(this).attr("datasbid");
                curentId = $(this).attr("dataId");
                $.ajax({
                    type: 'get',
                    async: false,
                    url: "/OrderRefund/GetShopGetAddress?shopId=" + shopId + "&shopBranchId=" + sbid,
                    dataType: 'json',
                    data: {},
                    success: function (result) {
                        var d = result.data;
                        if (d && d.Phone.length > 0) {
                            var str = "" + d.ShipperName + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + d.Phone + "";
                            str += "<br>" + d.Region + "  " + d.Address + "";
                            $("#shopsendaddress").html(str);
                        } else {
                            $("#shopsendaddress").text("商家未设置收货地址，请与商家主动联系！");
                        }
                    }
                });
                $(".reback-dialog").css('visibility', 'visibile').show();
                $("#expressCompanyName").text("");
                $("#shipOrderNumber").text("");
                $(".coverage").addClass("cover");
            });
        }
        else {
        }
		if(page==1)
			loading.close();
    });
};
$(document).ready(function () {

    $(".reback-dialog .glyphicon-remove").click(function () {
        $(".reback-dialog").hide();
        $(".coverage").removeClass("cover");
        $(".reback-dialog").css("top","27%");
    });
    $(".reback-dialog .reback-submit").click(function () {
        $(".reback-dialog").hide();
        $(".coverage").removeClass("cover");
        $(".reback-dialog").css("top","27%");
    });
})

function backSubmit() {
    var expressCompanyName = $("#expressCompanyName").val();
    var shipOrderNumber = $("#shipOrderNumber").val();
    if (expressCompanyName == "" || shipOrderNumber == "") {
        $.dialog.errorTips("请输入快递公司和快递单号！", '', 1);
        return;
    }
    var loading = showLoading();
    $.post('UpdateRefund', { id: curentId, expressCompanyName: expressCompanyName, shipOrderNumber: shipOrderNumber }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.succeedTips("提交成功！", function () {
                Refresh();
            }, 1);
        }
        else {
            $.dialog.errorTips("提交失败," + result.msg, '', 2);
        }
    });

}
