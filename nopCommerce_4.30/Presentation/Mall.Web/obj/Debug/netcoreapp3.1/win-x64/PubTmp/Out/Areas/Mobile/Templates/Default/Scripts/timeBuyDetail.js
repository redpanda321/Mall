$(function() {
    initCountButton();
    if ($(".btn-goshop_in").hasClass("disabled")) {
        $(".btn-goshop_in").attr('disabled', "true");
    }
    
    $('#choose').MallSku({
    	data : {id : $('#mainId').val()},
		productId : $('#gid').val(),
		resultClass:{
			price:'#salePrice',
			stock: '#stockNum',
			minmath: '#minMath'
		},
		ajaxType : 'POST',
		ajaxUrl:'../GetSkus',
		skuPosition: 'Details',
		callBack: function (select) {
		    $('#stockNum').html(select.TotalCount);

		}
    });
    $(".cover").on('click', function () {
        $(".cover").hide();
    });
    $(".goods-info").on('click', ".icon-share", function () {
        $(".cover").show();
    });
});
function initCountButton() {
    $("#buy-num").blur(function() {
        var max = parseInt($("#maxSaleCount").val());
        if (parseInt($('#buy-num').val()) < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
        }
        if (parseInt($('#buy-num').val()) > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            $('#buy-num').val(max);
        }
    });
    $('#btn-add').click(function() {
        var max = parseInt($("#maxSaleCount").val());
        if (max < parseInt($('#buy-num').val())+1) {            
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            $('#buy-num').val(max);
        }
    });
}

function checkFirstSKUWhenHas() {
    if ($(".spec").length == 0)
        return;
    $(".spec").each(function () {
        $(this).children("div:first").not(".disabled").find("span:first").trigger("click");
    });
}

var skuId = new Array(3);
// chooseResult();
function chooseResult() {
    // 已选择显示
    var len = $('#choose .spec .selected').length;
    for (var i = 0; i < len; i++) {
        var index = parseInt($('#choose .spec .selected').eq(i).attr('st'));
        skuId[index] = $('#choose .spec .selected').eq(i).attr('cid');
    }
    // 请求Ajax获取价格
    if (len === $(".spec").length) {
        var gid = $("#gid").val();
        var sku = '';
        for (var i = 0; i < skuId.length; i++) {
            sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
        }
        if (sku.length === 0) { sku = "0_0_0_"; }
    }
}

$("#justBuy").click(function() {
    checkIsLogin(function(func) {
        //justBuy(func);
        //检测是否开启了手机号强制绑定
        $.post("/" + areaName + "/Member/IsConBindSms", null, function (result) {
            if (result.success) {
                justBuy(func);
            } else {//说明开启了强制绑定手机号，而用户没有绑定，则需要先去绑定
                var url = location.href;
                location.href = "/" + areaName + "/Member/BindPhone?returnUrl=" + encodeURIComponent(url) + "&type=1";
            }
        });
    });
});

function justBuy(callBack) {
    chooseResult();
    var has = $("#has").val();
    var dis = $("#justBuy").hasClass('disabled');
    if (has != 1 || dis) return;
    if (dis) return false;
    var len = $('#choose .spec .selected').length;
    if (len === $(".spec").length) {
        var sku = getskuid();
        var num = $("#buy-num").val();
        var stock = $("#stockNum").html();
        if (parseInt(num) > parseInt(stock)) {
            $.dialog.errorTips("库存不足");
            return;
        }
        var activeid = $('#mainId').val();
        var returnUrl = "/" + areaName + "/limittimebuy/detail/" + activeid;
        returnUrl = encodeURIComponent(returnUrl);
        $.post('../CheckLimitTimeBuy', { skuIds: sku, counts: num }, function(result) {
            if (result.success) {
                var url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=submit&skuIds={0}&counts={1}&isLimit=1&returnUrl={2}'.format(sku, num, returnUrl);
                location.href = url;
                //location.href = "../../Order/Submit?skuIds=" + sku + "&counts=" + num+"&isLimit=1";
            } else if (result.data.remain <= 0) {
                $.dialog.errorTips("亲，限购" + result.data.maxSaleCount + "件，不能再买了哦");
            } else {
                $.dialog.errorTips("亲，限购" + result.data.maxSaleCount + "件，您最多还能买" + result.data.remain + "件");
            }
        });
    } else {
        $.dialog.errorTips("请选择商品规格！");
    }
}

function checkIsLogin(callBack) {
    var memberId = $("#logined").val();
    if (memberId == "1") {
        callBack();
    }
    else {
        location.href = "/" + areaName + "/Redirect?redirectUrl=" + location.href;
        // $.fn.login( {}, function () {
        //     callBack( function () { location.reload(); } );
        // }, '', '', '/Login/Login' );
    }
}

function getskuid() {
    var gid = $("#gid").val();
    var sku = '';
    for (var i = 0; i < skuId.length; i++) {
        sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
    }
    if (sku.length === 0) { sku = "0_0_0_"; }
    sku = gid + '_' + sku.substring(0, sku.length - 1);
    return sku;
}