$(function () {
    var pid = getProductId();
    initDistribution();
});

function initDistribution() {
    var url = '/' + areaName + '/product/GetDistributionInfo';
    var pid = getProductId();
    $.post(url, { id: pid }, function (result) {
        if (result && result.data && result.data.IsShowBrokerage) {
            var data = result.data;
            var winxinShareArgs = window.winxinShareArgs;

            if (winxinShareArgs) {
                winxinShareArgs = $.extend({
                    appId: data.WeiXinShareArgs.AppId,
                    timestamp: data.WeiXinShareArgs.Timestamp,
                    noncestr: data.WeiXinShareArgs.NonceStr,
                    signature: data.WeiXinShareArgs.Signature
                }, winxinShareArgs);
                winxinShareArgs.share.link = data.ShareUrl;
                if (winxinShareArgs.share.imgUrl == null || winxinShareArgs.share.imgUrl == '')
                    winxinShareArgs.share.imgUrl = location.origin + '/Areas/Mobile/Templates/Default/Images/default.png';

                //初始化微信分享
                initWinxin(winxinShareArgs);
            }
            $("#saleCountsSpan").remove();
            $("#marketpricebox").remove();
            var saleCountsSpan = $("#saleCountsSpan"); 
            //if (saleCountsSpan.length == 0) {
                $("#salefreight").prepend("<span>累计分销" + data.SaleCount + "件</span>");
            //} else {
            //    saleCountsSpan.html("累计分销" + data.SaleCount + "件");
            //}
            $("#dis-maxbrokerage").html('￥'+data.Brokerage.toFixed(2));
            $("#dis-maxbrokeragebox").removeClass("hide");
            $("#dis-share").removeClass("hide");
            if($(".store-prodetail .goods-info .item h4")){
            	$(".store-prodetail .goods-info .item h4").css("padding-right","45px");
            }
        }
    });
}