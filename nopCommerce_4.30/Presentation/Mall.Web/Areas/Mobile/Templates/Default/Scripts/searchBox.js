$('#searchBtn').click(function (e) {
    var keywords = $('#searchBox').val();
    var vshopid = QueryString("vshopid");
    if ($.trim(keywords) == "")
        $.dialog.tips("请输入搜索关键字");
    var jumpurl = '/' + areaName + '/search?keywords=' + encodeURIComponent(keywords) + '&returnUrl=' + encodeURIComponent(location.href);
    if (vshopid) {
        jumpurl += "&vshopid=" + vshopid;
    }
    location.href = jumpurl;
    e.preventDefault();
});

$("#searchBox").keypress(function (e) {
    if (e.keyCode == 13) {
        $('#searchBtn').trigger("click");
        e.preventDefault();
    }
});