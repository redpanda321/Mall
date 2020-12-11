
$(function () {
    checkPayDone();
});

function checkPayDone() {
    var orderIds = QueryString('orderIds');
    var type = QueryString('type') || '';
    if (type == 'charge') {
        $.getJSON('/PayState/CheckCharge', { orderIds: orderIds }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('支付成功!', function () {
                    location.href = "/userCenter?url=/usercapital/&tar=usercapital";
                });
            }
            else {
                setTimeout(checkPayDone, 100);
            }
        });
    } else if (type == 'shop' || type == 'shopcashdeposit') {
        var okurl = "/SellerAdmin/shop/Renew?url=/shop/&tar=shoprenewrecords";//店铺续费
        if (type == 'shopcashdeposit')
            okurl = "/SellerAdmin/CashDeposit/Management?url=/shop/&tar=cashok";//店铺交保证金
        $.getJSON('/PayState/CheckShop', { orderIds: orderIds }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('支付成功!', function () {
                    location.href = okurl;
                });
            }
            else {
                setTimeout(checkPayDone, 100);
            }
        });
    }
    else {
        $.getJSON('/PayState/Check', { orderIds: orderIds }, function (result) {
            if (result.success) {
                $.dialog.succeedTips('支付成功!', function () {
                    location.href = "/userCenter?url=/userorder?orderids="+orderIds+"&tar=userorder";
                });
            }
            else {
                setTimeout(checkPayDone, 100);
            }
        });
    }
}

