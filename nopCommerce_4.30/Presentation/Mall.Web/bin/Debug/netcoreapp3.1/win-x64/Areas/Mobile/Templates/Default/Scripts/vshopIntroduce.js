var vshopId = $("#vshopId").val();
var shopid = $("#shopId").val();

$(function () {
    returnFavoriteHref = "/" + areaName + "/vshop/introduce/" + vshopId;
    returnFavoriteHref = encodeURIComponent(returnFavoriteHref);

    $("#favorite").click(function () {
        checkLogin(returnFavoriteHref, function () {
            addFavorite();
        });
    });
});

function addFavorite() {
    var loading = showLoading();
    var url;
    var value;
    var favoriteEle = $("#favorite");
    if (favoriteEle.hasClass('on')) {
    	url = "../DeleteFavorite";
    	favoriteEle.removeClass('on')
    }
    else {
        url = "../AddFavorite";
        favoriteEle.addClass('on')
    }
    $.post(url, { shopId: shopid }, function (result) {
        loading.close();
        $.dialog.succeedTips(result.msg, null, 0.5);
    });
}