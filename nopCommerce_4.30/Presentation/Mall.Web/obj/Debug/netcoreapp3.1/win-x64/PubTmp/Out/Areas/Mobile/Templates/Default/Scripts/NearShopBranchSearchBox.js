var location;
$(function () {
    getKeyWordStorage();
    $('#searchBtn').click(function (e) {
        var keyword = $("#searchInput").val();
        setKeyWordStorage(keyword);
        location = shopBranchIndexUrl + "/SearchList?keywords=" + encodeURIComponent(keyword);
        e.preventDefault();
    });
    $("#searchInput").keypress(function (e) {
        if (e.keyCode == 13) {
            var keyword = $(this).val();
            setKeyWordStorage(keyword);
            location = shopBranchIndexUrl + "/SearchList?keywords=" + encodeURIComponent(keyword);
            e.preventDefault();
        }
    });
    $('#clearKeyWord').click(function() {
        MallStorage.removeItem('wxKeyWordList');
        $('#keyWordList').html('');
        //location = "?keywords=";
    });
});

//本地缓存存储历史搜索关键词
function setKeyWordStorage(keyWord) {
    var arry = new Array();
    var value = MallStorage.getItem('wxKeyWordList');
    if (value) {
        arry = value.split(',');
    }
    if (arry.join(',').indexOf(keyWord) == -1) {
        arry.push(keyWord);
    }
    MallStorage.setItem('wxKeyWordList', arry.join(','));
}
//获取缓存关键词
function getKeyWordStorage() {
    var html = '';
    var KeyWordList = MallStorage.getItem('wxKeyWordList');
    if (KeyWordList) {
        var arrKeyWord = KeyWordList.split(',');
        for (var i = arrKeyWord.length - 1; i >= 0; i--) {
            html += '<li><a href="' + shopBranchIndexUrl + '/SearchList?keywords=' + encodeURIComponent(arrKeyWord[i]) + '">' + arrKeyWord[i] + '</a></li>';
        }
    }
    $('#keyWordList').html(html);
}